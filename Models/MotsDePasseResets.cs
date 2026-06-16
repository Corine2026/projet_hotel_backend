using System;

namespace HotelBackend.Models
{
    public class MotDePasseReset
    {
        // 1. LES PROPRIÉTÉS (Données issues de votre diagramme)
        public Guid Id { get; set; } = Guid.NewGuid(); // Identifiant unique de la demande
        public Guid UtilisateurId { get; set; } // L'utilisateur qui a oublié son mot de passe
        public string Code { get; set; } = string.Empty; // Le code secret envoyé (ex: "654321")
        public DateTime DateExpiration { get; set; } // Moment où le code expire
        public bool Utilise { get; set; } = false; // Passe à 'true' dès que le code est consommé

        // Propriété de navigation pour Entity Framework
        public Utilisateur? Utilisateur { get; set; }


        // 2. LES MÉTHODES (La logique simple)

        // Génère un code de récupération et configure l'expiration
        public void GenererCode(Guid idUtilisateur, string codeSecret, int minutesValidite)
        {
            this.UtilisateurId = idUtilisateur;
            this.Code = codeSecret;
            this.DateExpiration = DateTime.UtcNow.AddMinutes(minutesValidite); // Ex: valide 15 minutes
            this.Utilise = false; // Code tout neuf, pas encore utilisé
        }

        // Vérifie si le code tapé par l'utilisateur est correct et encore valide
        public bool VerifierCode(string codeSaisi)
        {
            // Le code est valide si : il correspond, n'est pas expiré et n'a pas encore servi
            return this.Code == codeSaisi && DateTime.UtcNow <= this.DateExpiration && !this.Utilise;
        }

        // Force l'expiration du code (par sécurité ou après utilisation)
        public void ExpirerCode()
        {
            this.Utilise = true; // On marque comme utilisé pour le griller définitivement
        }
    }
}