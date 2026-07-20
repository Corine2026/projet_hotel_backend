using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;
using HotelBackend.Data;
using HotelBackend.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;

namespace HotelBackend.Controllers
{
    /* ─────────────────────────────────────────────────────────────────────
       DTOs pour l'endpoint OCR (Google Cloud Vision)
    ───────────────────────────────────────────────────────────────────── */
    public class OcrRequestDto
    {
        public string ImageBase64 { get; set; } = "";
    }

    public class OcrResultDto
    {
        public string Text { get; set; } = "";
        public double Confidence { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ScanDocumentsController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ScanDocumentsController(
            HotelDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /* ═══════════════════════════════════════════════════════════════
           POST: api/ScanDocuments/ocr
           Reçoit une image en base64, l'envoie à OCR.space (gratuit,
           sans carte bancaire requise), et retourne le texte brut détecté.
           La clé API reste côté serveur — jamais exposée au frontend.
        ═══════════════════════════════════════════════════════════════ */
        [HttpPost("ocr")]
        public async Task<ActionResult<OcrResultDto>> AnalyserImage([FromBody] OcrRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageBase64))
                return BadRequest("Image manquante.");

            var apiKey = _configuration["OcrSpace:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, "Clé API OCR.space non configurée sur le serveur.");

            var client = _httpClientFactory.CreateClient();

            var formData = new Dictionary<string, string>
            {
                ["apikey"]           = apiKey,
                ["base64Image"]      = $"data:image/jpeg;base64,{dto.ImageBase64}",
                ["language"]         = "fre",
                ["OCREngine"]        = "2",   // moteur 2 : plus précis, meilleur pour documents structurés
                ["scale"]            = "true",
                ["detectOrientation"]= "true",
            };

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(
                    "https://api.ocr.space/parse/image",
                    new FormUrlEncodedContent(formData));
            }
            catch (Exception ex)
            {
                return StatusCode(502, $"Impossible de joindre OCR.space : {ex.Message}");
            }

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, $"Erreur OCR.space : {content}");

            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("IsErroredOnProcessing", out var errored) && errored.GetBoolean())
            {
                string errMsg = "Erreur inconnue";
                if (root.TryGetProperty("ErrorMessage", out var em))
                {
                    errMsg = em.ValueKind == JsonValueKind.Array
                        ? string.Join("; ", em.EnumerateArray().Select(e => e.GetString()))
                        : em.GetString() ?? errMsg;
                }
                return StatusCode(500, $"Erreur OCR.space : {errMsg}");
            }

            string text = "";
            if (root.TryGetProperty("ParsedResults", out var parsedResults) && parsedResults.GetArrayLength() > 0)
            {
                var first = parsedResults[0];
                if (first.TryGetProperty("ParsedText", out var pt))
                    text = pt.GetString() ?? "";
            }

            // OCR.space ne renvoie pas de score de confiance global. On estime
            // une confiance raisonnable selon la longueur du texte détecté —
            // suffisant pour piloter les seuils "success/blurry/error" déjà
            // utilisés côté frontend.
            double confidence = string.IsNullOrWhiteSpace(text) ? 0
                               : text.Length > 40 ? 85
                               : text.Length > 15 ? 65
                               : 45;

            return Ok(new OcrResultDto { Text = text, Confidence = confidence });
        }

        // POST: api/ScanDocuments/sauvegarder
        // Reçoit le résultat OCR du frontend, crée le ScanDocument + DocumentIdentite
        // + met à jour ou crée le Client automatiquement
        [HttpPost("sauvegarder")]
        public async Task<ActionResult<object>> SauvegarderScan([FromBody] ScanResultDto dto)
        {
            if (dto == null)
                return BadRequest("Résultat de scan manquant.");

            if (!Enum.TryParse<TypeDocument>(dto.TypeDocument, true, out var typeDoc))
                typeDoc = TypeDocument.CNI;

            var statut = dto.ScoreConfiance >= 70 ? StatutScan.Reussi
                       : dto.ScoreConfiance >= 40 ? StatutScan.DocumentIncomplet
                       : StatutScan.Echec;

            // 1. Créer le ScanDocument
            var scan = new ScanDocument
            {
                Id            = Guid.NewGuid(),
                TexteOCR      = dto.TexteOCRBrut,
                ScoreConfiance= dto.ScoreConfiance,
                QualiteImage  = dto.QualiteImage,
                DateScan      = DateTime.UtcNow,
                Statut        = statut,
                ImageOriginale= dto.ImageBase64
            };
            _context.ScanDocuments.Add(scan);

            // 2. Créer ou retrouver le client par numéro de document
            Client? client = null;
            if (!string.IsNullOrWhiteSpace(dto.Numero))
            {
                client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.NumeroCNI == dto.Numero);
            }

            if (client == null && !string.IsNullOrWhiteSpace(dto.Nom))
            {
                client = new Client
                {
                    Id            = Guid.NewGuid(),
                    Nom           = dto.Nom ?? "",
                    Prenom        = dto.Prenom ?? "",
                    NumeroCNI     = dto.Numero ?? "",
                    Telephone     = "",
                    DateNaissance = dto.DateNaissance.HasValue
                        ? DateTime.SpecifyKind(dto.DateNaissance.Value, DateTimeKind.Utc)
                        : DateTime.SpecifyKind(new DateTime(1990,1,1), DateTimeKind.Utc),
                    DateCreation  = DateTime.UtcNow,
                    UpdatedAt     = DateTime.UtcNow
                };
                _context.Clients.Add(client);

                _context.HistoriqueClients.Add(new HistoriqueClient
                {
                    Id          = Guid.NewGuid(),
                    ClientId    = client.Id,
                    Action      = "Création automatique",
                    Description = $"Client créé via scan {typeDoc}.",
                    DateAction  = DateTime.UtcNow
                });
            }

            // 3. Créer le DocumentIdentite
            if (client != null && !string.IsNullOrWhiteSpace(dto.Numero))
            {
                var doc = new DocumentIdentite
                {
                    Id            = Guid.NewGuid(),
                    ClientId      = client.Id,
                    TypeDocument  = typeDoc,
                    Numero        = dto.Numero,
                    PaysEmission  = dto.PaysEmission,
                    DateCreation  = DateTime.UtcNow,
                    DateDelivrance= dto.DateDelivrance.HasValue
                        ? DateTime.SpecifyKind(dto.DateDelivrance.Value, DateTimeKind.Utc)
                        : null,
                    DateExpiration= dto.DateExpiration.HasValue
                        ? DateTime.SpecifyKind(dto.DateExpiration.Value, DateTimeKind.Utc)
                        : null,
                };
                scan.DocumentId = doc.Id;
                _context.DocumentsIdentite.Add(doc);
            }

            _context.HistoriqueScans.Add(new HistoriqueScan
            {
                Id            = Guid.NewGuid(),
                ScanId        = scan.Id,
                DateOperation = DateTime.UtcNow,
                Action        = "Scan initial",
                Utilisateur   = User.Identity?.Name ?? "System",
                Observation   = $"Score: {dto.ScoreConfiance:F1}% — Statut: {statut}"
            });

            await _context.SaveChangesAsync();

            return Ok(new {
                scanId        = scan.Id,
                statut        = statut.ToString(),
                scoreConfiance= scan.ScoreConfiance,
                clientId      = client?.Id,
                clientNom     = client != null ? $"{client.Prenom} {client.Nom}" : null,
                isNewClient   = client != null,
                donnéesExtraites = new {
                    nom           = dto.Nom,
                    prenom        = dto.Prenom,
                    numero        = dto.Numero,
                    dateNaissance = dto.DateNaissance,
                    nationalite   = dto.Nationalite,
                    paysEmission  = dto.PaysEmission,
                    typeDocument  = typeDoc.ToString()
                }
            });
        }

        // GET: api/ScanDocuments/client/{clientId}
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDocumentsClient(Guid clientId)
        {
            var docs = await _context.DocumentsIdentite
                .Where(d => d.ClientId == clientId)
                .Include(d => d.Scans)
                .OrderByDescending(d => d.DateCreation)
                .Select(d => new {
                    d.Id, d.TypeDocument, d.Numero, d.PaysEmission,
                    d.DateDelivrance, d.DateExpiration, d.DateCreation,
                    EstValide   = d.DateExpiration == null || d.DateExpiration > DateTime.UtcNow,
                    EstExpire   = d.DateExpiration.HasValue && d.DateExpiration.Value <= DateTime.UtcNow,
                    NbScans     = d.Scans.Count
                })
                .ToListAsync();

            return Ok(docs);
        }
    }
}