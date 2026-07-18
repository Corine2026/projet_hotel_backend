using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;
using HotelBackend.Data;
using HotelBackend.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

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
           Reçoit une image en base64, l'envoie à Google Cloud Vision
           (TEXT_DETECTION), et retourne le texte brut détecté.
           La clé API reste côté serveur — jamais exposée au frontend.
        ═══════════════════════════════════════════════════════════════ */
        [HttpPost("ocr")]
        public async Task<ActionResult<OcrResultDto>> AnalyserImage([FromBody] OcrRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageBase64))
                return BadRequest("Image manquante.");

            var apiKey = _configuration["Google:VisionApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, "Clé API Google Vision non configurée sur le serveur.");

            var requestBody = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = dto.ImageBase64 },
                        features = new[] { new { type = "TEXT_DETECTION" } },
                        imageContext = new { languageHints = new[] { "fr", "en" } }
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync(
                    $"https://vision.googleapis.com/v1/images:annotate?key={apiKey}",
                    requestBody);
            }
            catch (Exception ex)
            {
                return StatusCode(502, $"Impossible de joindre Google Vision : {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Erreur Google Vision : {errorContent}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonSerializer.DeserializeAsync<JsonElement>(stream);

            if (!json.TryGetProperty("responses", out var responses) || responses.GetArrayLength() == 0)
                return Ok(new OcrResultDto { Text = "", Confidence = 0 });

            var firstResponse = responses[0];

            if (firstResponse.TryGetProperty("error", out var errorElement))
            {
                var msg = errorElement.TryGetProperty("message", out var m) ? m.GetString() : "Erreur inconnue";
                return StatusCode(500, $"Erreur Google Vision : {msg}");
            }

            string fullText = "";
            if (firstResponse.TryGetProperty("fullTextAnnotation", out var fullTextAnnotation)
                && fullTextAnnotation.TryGetProperty("text", out var textProp))
            {
                fullText = textProp.GetString() ?? "";
            }
            else if (firstResponse.TryGetProperty("textAnnotations", out var textAnnotations)
                     && textAnnotations.GetArrayLength() > 0
                     && textAnnotations[0].TryGetProperty("description", out var descProp))
            {
                fullText = descProp.GetString() ?? "";
            }

            // Google Vision (TEXT_DETECTION) ne renvoie pas de score de confiance
            // global simple. On estime une confiance raisonnable selon la longueur
            // du texte détecté — suffisant pour piloter les seuils "success/blurry/error"
            // déjà utilisés côté frontend.
            double confidence = string.IsNullOrWhiteSpace(fullText) ? 0
                               : fullText.Length > 40 ? 92
                               : fullText.Length > 15 ? 75
                               : 50;

            return Ok(new OcrResultDto { Text = fullText, Confidence = confidence });
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