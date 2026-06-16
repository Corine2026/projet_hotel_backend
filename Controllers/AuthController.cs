using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HotelBackend.Data;
using HotelBackend.DTOs;
using HotelBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(HotelDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(new { message = "Données de connexion manquantes." });
            }

            // Nettoyage de l'email reçu depuis l'interface React
            var emailReçu = loginDto.Email?.Trim().ToLower() ?? string.Empty;
            var motDePasseReçu = loginDto.MotDePasse ?? string.Empty;

            Console.WriteLine("\n==================================================");
            Console.WriteLine($"[DEBUG] Tentative de connexion reçue pour : '{emailReçu}'");

            // 1. Recherche de l'utilisateur dans PostgreSQL
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailReçu);

            if (utilisateur == null)
            {
                Console.WriteLine($"[DEBUG] ❌ ÉCHEC : Aucun utilisateur trouvé en BDD avec l'email '{emailReçu}'");
                return Unauthorized(new { message = "Identifiant ou mot de passe incorrect." });
            }

            Console.WriteLine($"[DEBUG] 🎉 Utilisateur trouvé : {utilisateur.NomUtilisateur}");
            Console.WriteLine($"[DEBUG] Statut du compte en BDD : {utilisateur.Statut}");

            // 2. Vérification du statut du compte
            if (utilisateur.Statut != "Actif")
            {
                Console.WriteLine($"[DEBUG] ❌ ÉCHEC : Le compte de {utilisateur.NomUtilisateur} est '{utilisateur.Statut}'");
                return BadRequest(new { message = "Ce compte n'est pas actif. Veuillez contacter votre administrateur." });
            }

            // 3. LOGS D'ANALYSE DES CHAÎNES DE CARACTÈRES (Pour déceler les espaces invisibles)
            var mdpBDD = utilisateur.MotDePasseHash ?? string.Empty;
            
            Console.WriteLine($"[DEBUG] React envoie le mot de passe : '{motDePasseReçu}' (Longueur : {motDePasseReçu.Length})");
            Console.WriteLine($"[DEBUG] La BDD contient le mot de passe : '{mdpBDD}' (Longueur en BDD : {mdpBDD.Length})");

            // 4. Vérification stricte avec nettoyage .Trim() contre les caprices de PostgreSQL
            if (mdpBDD.Trim() != motDePasseReçu.Trim())
            {
                Console.WriteLine("[DEBUG] ❌ ÉCHEC : Les mots de passe ne correspondent pas (après application du Trim).");
                return Unauthorized(new { message = "Identifiant ou mot de passe incorrect." });
            }

            Console.WriteLine("[DEBUG] 🔑 SUCCÈS : Mot de passe validé avec succès !");

            // 5. Mise à jour de la date de dernière connexion
            try
            {
                utilisateur.DerniereConnexion = DateTime.UtcNow;
                _context.Entry(utilisateur).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                Console.WriteLine("[DEBUG] Date de dernière connexion mise à jour dans PostgreSQL.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Impossible de mettre à jour la date en BDD : {ex.Message}");
            }

            // 6. Gestion et sécurisation du rôle (Tolérance si la table Roles est vide)
            string roleNom = "Receptionniste"; // Rôle par défaut de secours
            
            if (utilisateur.RoleId != Guid.Empty)
            {
                var role = await _context.Roles.FindAsync(utilisateur.RoleId);
                if (role != null && !string.IsNullOrEmpty(role.Nom))
                {
                    roleNom = role.Nom;
                }
            }
            else
            {
                // Attribution dynamique basée sur l'adresse email pour la démonstration
                if (emailReçu.Contains("admin")) roleNom = "Administrateur";
                else if (emailReçu.Contains("compta")) roleNom = "Comptable";
                else if (emailReçu.Contains("gouvernante")) roleNom = "Gouvernante";
            }

            Console.WriteLine($"[DEBUG] Rôle associé pour le Token : {roleNom}");

            // 7. Génération du Token JWT
            var token = GenererJwtToken(utilisateur, roleNom);
            Console.WriteLine("[DEBUG] 🎫 Token JWT généré avec succès !");
            Console.WriteLine("==================================================\n");

            // 8. Envoi de la réponse structurée à votre PWA React
            return Ok(new
            {
                token = token,
                user = new
                {
                    id = utilisateur.Id,
                    email = utilisateur.Email,
                    nom = utilisateur.NomUtilisateur,
                    role = roleNom,
                    roleId = utilisateur.RoleId,
                    personnelId = utilisateur.PersonnelId
                }
            });
        }

        private string GenererJwtToken(Utilisateur utilisateur, string roleNom)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "UneCleSecreteTresLongueEtSecuriseeDeMinimum32Caracteres";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "HotelMamfouoIssuer";
            
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new Claim(ClaimTypes.Email, utilisateur.Email),
                new Claim(ClaimTypes.Name, utilisateur.NomUtilisateur),
                new Claim(ClaimTypes.Role, roleNom)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // Le token reste valide pendant un shift complet de 8h
                Issuer = jwtIssuer,
                Audience = jwtIssuer,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}