using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;
using HotelBackend.Data;
using HotelBackend.DTOs;

namespace HotelBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChambresController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ChambresController(HotelDbContext context) 
        { 
            _context = context; 
        }

        // GET: api/Chambres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetChambres()
        {
            var data = await _context.Chambres.Include(c => c.TypeChambre).ToListAsync();

            return data.Select(c => new {
                c.Id, c.Numero, c.TypeChambreId, c.PrixNuitee, c.PrixSieste,
                c.Capacite, c.Description, c.Equipements, c.Etage,
                Etat = c.Etat.ToString(), 
                c.DateCreation, c.UpdatedAt,
                TypeChambreNom = c.TypeChambre?.Nom ?? "Non défini"
            }).ToList();
        }

        // GET: api/Chambres/{id}/historique
        // AJOUT : Cette route manquante corrige votre erreur 404 dans le front-end
        [HttpGet("{id}/historique")]
        public async Task<ActionResult<IEnumerable<object>>> GetHistoriqueChambre(Guid id)
        {
            var chambreExiste = await _context.Chambres.AnyAsync(c => c.Id == id);
            if (!chambreExiste) return NotFound("Chambre non trouvée.");

            var historique = await _context.HistoriqueEtats
                .Where(h => h.ChambreId == id)
                .OrderByDescending(h => h.DateChangement) 
                .ToListAsync();

            return Ok(historique);
        }

        // POST: api/Chambres
        [HttpPost]
        public async Task<ActionResult<Chambre>> PostChambre([FromBody] ChambreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Chambres.AnyAsync(c => c.Numero == dto.Numero))
                return BadRequest($"La chambre numéro {dto.Numero} existe déjà.");

            Enum.TryParse(dto.Etat, true, out EtatChambre etatEnum);

            var chambre = new Chambre
            {
                Id = Guid.NewGuid(),
                Numero = dto.Numero,
                TypeChambreId = dto.TypeChambreId,
                PrixNuitee = dto.PrixNuitee,
                PrixSieste = dto.PrixSieste,
                Capacite = dto.Capacite,
                Description = dto.Description,
                Equipements = dto.Equipements ?? new List<string>(),
                Etage = dto.Etage,
                Etat = etatEnum, 
                DateCreation = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Chambres.Add(chambre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChambres), new { id = chambre.Id }, chambre);
        }

        // PUT: api/Chambres/{id}
        [HttpPut("{id}")]
public async Task<IActionResult> PutChambre(Guid id, [FromBody] ChambreDto dto)
{
    // On vérifie si le DTO est bien reçu
    if (dto == null) return BadRequest("Le corps de la requête est vide.");

    var c = await _context.Chambres.FindAsync(id);
    if (c == null) return NotFound("Chambre non trouvée.");

    // Conversion plus robuste : on essaie de parser l'état uniquement s'il n'est pas vide
    if (!string.IsNullOrWhiteSpace(dto.Etat))
    {
        if (Enum.TryParse(dto.Etat, true, out EtatChambre etatEnum))
        {
            c.Etat = etatEnum;
        }
    }

    c.Numero = dto.Numero;
    c.TypeChambreId = dto.TypeChambreId;
    c.PrixNuitee = dto.PrixNuitee;
    c.PrixSieste = dto.PrixSieste;
    c.Capacite = dto.Capacite;
    c.Description = dto.Description;
    c.Equipements = dto.Equipements ?? c.Equipements;
    c.Etage = dto.Etage;
    c.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return NoContent();
}
    }
}