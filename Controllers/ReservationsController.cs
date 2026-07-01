using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;
using HotelBackend.Data;
using HotelBackend.DTOs;

namespace HotelBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ReservationsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetReservations()
        {
            var data = await _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Chambre)
                .OrderByDescending(r => r.DateCreation)
                .ToListAsync();

            return data.Select(r => new {
                r.Id, r.Numero, r.ClientId, r.ChambreId,
                Type = r.Type.ToString(),
                r.DateDebut, r.DateFin, r.Montant,
                Statut = r.Statut.ToString(),
                r.DateCreation, r.UpdatedAt,
                ClientNom = r.Client != null ? $"{r.Client.Prenom} {r.Client.Nom}" : "—",
                ClientTelephone = r.Client?.Telephone ?? "—",
                ChambreNumero = r.Chambre?.Numero ?? "—"
            }).ToList();
        }

        // GET: api/Reservations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetReservation(Guid id)
        {
            var r = await _context.Reservations
                .Include(x => x.Client)
                .Include(x => x.Chambre)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return NotFound("Réservation non trouvée.");

            return new {
                r.Id, r.Numero, r.ClientId, r.ChambreId,
                Type = r.Type.ToString(),
                r.DateDebut, r.DateFin, r.Montant,
                Statut = r.Statut.ToString(),
                r.DateCreation, r.UpdatedAt,
                ClientNom = r.Client != null ? $"{r.Client.Prenom} {r.Client.Nom}" : "—",
                ChambreNumero = r.Chambre?.Numero ?? "—"
            };
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation([FromBody] ReservationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var chambre = await _context.Chambres.FindAsync(dto.ChambreId);
            if (chambre == null) return BadRequest("Chambre introuvable.");

            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (client == null) return BadRequest("Client introuvable.");

            if (!Enum.TryParse<TypeReservation>(dto.Type, true, out var typeEnum))
                return BadRequest($"Type invalide : {dto.Type}. Valeurs attendues : Nuitee, Sieste.");

            // Vérification de chevauchement de dates sur la même chambre
            var dateDebutUtc = DateTime.SpecifyKind(dto.DateDebut, DateTimeKind.Utc);
            var dateFinUtc = DateTime.SpecifyKind(dto.DateFin, DateTimeKind.Utc);

            bool chevauchement = await _context.Reservations.AnyAsync(r =>
                r.ChambreId == dto.ChambreId &&
                r.Statut != StatutReservation.Annulee &&
                dateDebutUtc < r.DateFin && dateFinUtc > r.DateDebut);

            if (chevauchement)
                return BadRequest("Cette chambre est déjà réservée sur cette période.");

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                Numero = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                ClientId = dto.ClientId,
                ChambreId = dto.ChambreId,
                Type = typeEnum,
                DateDebut = dateDebutUtc,
                DateFin = dateFinUtc,
                Statut = StatutReservation.EnAttente,
                DateCreation = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            reservation.Montant = dto.Montant ?? reservation.CalculerMontant(chambre.PrixNuitee, chambre.PrixSieste);

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/Reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(Guid id, [FromBody] ReservationDto dto)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return NotFound("Réservation non trouvée.");

            if (Enum.TryParse<TypeReservation>(dto.Type, true, out var typeEnum))
                r.Type = typeEnum;

            r.ClientId = dto.ClientId;
            r.ChambreId = dto.ChambreId;
            r.DateDebut = DateTime.SpecifyKind(dto.DateDebut, DateTimeKind.Utc);
            r.DateFin = DateTime.SpecifyKind(dto.DateFin, DateTimeKind.Utc);
            if (dto.Montant.HasValue) r.Montant = dto.Montant.Value;

            if (!string.IsNullOrWhiteSpace(dto.Statut) &&
                Enum.TryParse<StatutReservation>(dto.Statut, true, out var statutEnum))
                r.Statut = statutEnum;

            r.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/Reservations/{id}/confirmer
        [HttpPatch("{id}/confirmer")]
        public async Task<IActionResult> Confirmer(Guid id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return NotFound();
            r.Confirmer();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/Reservations/{id}/annuler
        [HttpPatch("{id}/annuler")]
        public async Task<IActionResult> Annuler(Guid id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return NotFound();
            r.Annuler();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/Reservations/{id}/terminer
        [HttpPatch("{id}/terminer")]
        public async Task<IActionResult> Terminer(Guid id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return NotFound();
            r.Terminer();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Reservations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r == null) return NotFound();
            _context.Reservations.Remove(r);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}