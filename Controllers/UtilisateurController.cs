using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBackend.Data;
using HotelBackend.Models;
using System;
using System.Threading.Tasks;

namespace HotelBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateurController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public UtilisateurController(HotelDbContext context)
        {
            _context = context;
        }

        // 1. Connexion (Simulation pour le test Swagger)
        [HttpPost("login")]
        public IActionResult SeConnecter([FromBody] LoginRequest request)
        {
            if (request.Email == "admin@hotel.com" && request.MotDePasse == "123456")
            {
                return Ok(new { message = "Connexion réussie ! Bienvenue dans le système de l'hôtel." });
            }
            return BadRequest(new { message = "Identifiants incorrects." });
        }

        // 2. Activer un compte
        [HttpPut("{id}/activer")]
        public async Task<IActionResult> ActiverCompte(Guid id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur introuvable." });

            user.ActiverCompte(); // Appelle la méthode simple qu'on a créée dans le modèle
            await _context.SaveChangesAsync();

            return Ok(new { message = "Le compte a bien été activé.", statut = user.Statut });
        }

        // 3. Suspendre un compte
        [HttpPut("{id}/suspendre")]
        public async Task<IActionResult> SuspendreCompte(Guid id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Utilisateur introuvable." });

            user.SuspendreCompte(); // Appelle la méthode simple du modèle
            await _context.SaveChangesAsync();

            return Ok(new { message = "Le compte a été suspendu.", statut = user.Statut });
        }
    }

    // Petit objet pour recevoir l'e-mail et le mot de passe dans Swagger
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
    }
}