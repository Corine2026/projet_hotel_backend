using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;
using HotelBackend.Data;
using HotelBackend.DTOs;

namespace HotelBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ClientsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetClients()
        {
            var data = await _context.Clients.ToListAsync();

            return data.Select(c => new {
                c.Id, c.Nom, c.Prenom,
                Sexe = c.Sexe.ToString(),
                c.DateNaissance, c.Ville, c.Telephone, c.NumeroCNI, c.PhotoUrl,
                c.DateCreation, c.UpdatedAt,
                Age = CalculerAgeStatic(c.DateNaissance)
            }).ToList();
        }

        // GET: api/Clients/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetClient(Guid id)
        {
            var c = await _context.Clients.FindAsync(id);
            if (c == null) return NotFound("Client non trouvé.");

            return new {
                c.Id, c.Nom, c.Prenom,
                Sexe = c.Sexe.ToString(),
                c.DateNaissance, c.Ville, c.Telephone, c.NumeroCNI, c.PhotoUrl,
                c.DateCreation, c.UpdatedAt,
                Age = CalculerAgeStatic(c.DateNaissance)
            };
        }

        // GET: api/Clients/{id}/historique
        [HttpGet("{id}/historique")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistoriqueClient(Guid id)
        {
            var clientExiste = await _context.Clients.AnyAsync(c => c.Id == id);
            if (!clientExiste) return NotFound("Client non trouvé.");

            var historique = await _context.HistoriqueClients
                .Where(h => h.ClientId == id)
                .OrderByDescending(h => h.DateAction)
                .ToListAsync();

            return Ok(historique);
        }

        // POST: api/Clients
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient([FromBody] ClientDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Clients.AnyAsync(c => c.NumeroCNI == dto.NumeroCNI))
                return BadRequest($"Un client avec le numéro CNI {dto.NumeroCNI} existe déjà.");

            if (!Enum.TryParse<SexeClient>(dto.Sexe, true, out var sexeEnum))
                return BadRequest($"Sexe invalide : {dto.Sexe}. Valeurs attendues : Homme, Femme.");

            // ⚠️ FIX : PostgreSQL (colonnes "timestamp with time zone") exige que tout
            // DateTime écrit en base ait Kind = Utc. Un DateTime désérialisé depuis le
            // JSON du front (ex: "2003-09-01") arrive avec Kind = Unspecified, ce qui
            // fait planter Npgsql avec :
            // "Cannot write DateTime with Kind=Unspecified to PostgreSQL type
            //  'timestamp with time zone', only UTC is supported."
            // DateTime.SpecifyKind ne convertit pas l'heure, il étiquette juste la
            // valeur existante comme étant déjà en UTC — ce qui est correct ici car
            // une date de naissance n'a pas de notion de fuseau horaire réelle.
            var dateNaissanceUtc = DateTime.SpecifyKind(dto.DateNaissance, DateTimeKind.Utc);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Nom = dto.Nom,
                Prenom = dto.Prenom,
                Sexe = sexeEnum,
                DateNaissance = dateNaissanceUtc,
                Ville = dto.Ville,
                Telephone = dto.Telephone,
                NumeroCNI = dto.NumeroCNI,
                PhotoUrl = dto.PhotoUrl,
                DateCreation = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);

            // Historique automatique de la création
            _context.HistoriqueClients.Add(HistoriqueClient.AjouterAction(
                client.Id, "Création", $"Client {client.Prenom} {client.Nom} enregistré."));

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }

        // PUT: api/Clients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(Guid id, [FromBody] ClientDto dto)
        {
            if (dto == null) return BadRequest("Le corps de la requête est vide.");

            var c = await _context.Clients.FindAsync(id);
            if (c == null) return NotFound("Client non trouvé.");

            if (!string.IsNullOrWhiteSpace(dto.Sexe))
            {
                if (Enum.TryParse<SexeClient>(dto.Sexe, true, out var sexeEnum))
                    c.Sexe = sexeEnum;
            }

            c.Nom = dto.Nom;
            c.Prenom = dto.Prenom;
            // Même fix que dans PostClient — voir commentaire ci-dessus.
            c.DateNaissance = DateTime.SpecifyKind(dto.DateNaissance, DateTimeKind.Utc);
            c.Ville = dto.Ville;
            c.Telephone = dto.Telephone;
            c.NumeroCNI = dto.NumeroCNI;
            c.PhotoUrl = dto.PhotoUrl;
            c.UpdatedAt = DateTime.UtcNow;

            _context.HistoriqueClients.Add(HistoriqueClient.AjouterAction(
                c.Id, "Modification", "Informations mises à jour."));

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Clients/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var c = await _context.Clients.FindAsync(id);
            if (c == null) return NotFound("Client non trouvé.");

            _context.Clients.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static int CalculerAgeStatic(DateTime dateNaissance)
        {
            var today = DateTime.UtcNow;
            var age = today.Year - dateNaissance.Year;
            if (dateNaissance.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}