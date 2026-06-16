using System;

namespace HotelBackend.Models
{
    public class SessionUtilisateur
    {
        // 1. LES PROPRIÉTÉS (Données de la session issues du diagramme)
        public Guid Id { get; set; } = Guid.NewGuid(); // Identifiant unique de la session [cite: 61]
        public Guid UtilisateurId { get; set; } // L'utilisateur lié à cette session [cite: 61]
        public string TokenJWT { get; set; } = string.Empty; // Le jeton de sécurité principal [cite: 62]
        public string RefreshToken { get; set; } = string.Empty; // Le jeton pour renouveler la session [cite: 63]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow; // Heure de connexion [cite: 64]
        public DateTime DateExpiration { get; set; } // Heure de fin de validité [cite: 65]
        public string AdresseIP { get; set; } = string.Empty; // IP de l'appareil (ex: 192.168.1.50) [cite: 65]
        public string Appareil { get; set; } = string.Empty; // Ex: "Dell Latitude PC" ou "Android Mobile" [cite: 65]

        // Propriété de navigation pour Entity Framework
        public Utilisateur? Utilisateur { get; set; }


        // 2. LES MÉTHODES (La logique simple)

        // Crée une nouvelle session de connexion [cite: 66]
        public void CreerSession(Guid idUtilisateur, string token, string refreshToken, int heuresValidite, string ip, string nomAppareil)
        {
            this.UtilisateurId = idUtilisateur; //[cite: 61]
            this.TokenJWT = token;  //[cite: 62]
            this.RefreshToken = refreshToken; //[cite: 63]
            this.AdresseIP = ip; //[cite: 65]
            this.Appareil = nomAppareil; //[cite: 65]
            this.DateCreation = DateTime.UtcNow; //[cite: 64]
            this.DateExpiration = DateTime.UtcNow.AddHours(heuresValidite); // Calcule l'expiration [cite: 65]
        }

        // Ferme la session (Déconnexion forcée) [cite: 66]
        public void FermerSession()
        {
            // On fait expirer la session immédiatement en mettant la date d'expiration dans le passé
            this.DateExpiration = DateTime.UtcNow.AddDays(-1); // [cite: 65]
        }

        // Rafraîchit la session avec un nouveau token sans reconnecter l'utilisateur [cite: 67]
        public void RefraichirSession(string nouveauToken, string nouveauRefreshToken, int heuresValidite)
        {
            this.TokenJWT = nouveauToken; //[cite: 62]
            this.RefreshToken = nouveauRefreshToken; //[cite: 63]
            this.DateExpiration = DateTime.UtcNow.AddHours(heuresValidite); // Repousse l'expiration [cite: 65]
        }

        // Vérifie si la session a expiré [cite: 67]
        public bool VerifierExpiration()
        {
            // Renvoie 'true' si l'heure actuelle a dépassé la date d'expiration
            return DateTime.UtcNow > this.DateExpiration; //[cite: 65]
        }
    }
}