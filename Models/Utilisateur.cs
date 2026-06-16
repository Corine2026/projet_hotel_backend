using System;

namespace HotelBackend.Models
{
    public class Utilisateur
    {
        // 1. LES PROPRIÉTÉS (Les données de l'utilisateur)
        public Guid Id { get; set; } = Guid.NewGuid(); // Génère un identifiant unique automatiquement [cite: 3]
        public string NomUtilisateur { get; set; } = string.Empty; // [cite: 4]
        public string Email { get; set; } = string.Empty; // [cite: 5]
        public string Telephone { get; set; } = string.Empty; // [cite: 6]
        public string MotDePasseHash { get; set; } = string.Empty; // [cite: 7]
        public Guid RoleId { get; set; } // [cite: 8]
        public Guid PersonnelId { get; set; } // [cite: 9]
        public string Statut { get; set; } = "Actif"; // [cite: 10]
        public DateTime? DerniereConnexion { get; set; } // [cite: 11]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow; // [cite: 12]

        // 2. LES MÉTHODES (La logique simple)
        
        // Simule la connexion d'un utilisateur [cite: 28]
        public bool SeConnecter(string email, string motDePasse)
        {
            // Code temporaire pour tester : si l'email correspond, on valide [cite: 28]
            if (this.Email == email)
            {
                this.DerniereConnexion = DateTime.UtcNow; // Met à jour la date [cite: 11]
                return true;
            }
            return false;
        }

        // Déconnecte l'utilisateur [cite: 29]
        public void SeDeconnecter()
        {
            // Logique de déconnexion [cite: 29]
        }

        // Change le mot de passe [cite: 30]
        public void ChangerMotDePasse(string nouveauMotDePasse)
        {
            this.MotDePasseHash = nouveauMotDePasse; // Applique le nouveau mot de passe [cite: 7, 30]
        }

        // Vérifie si le mot de passe entré est le bon [cite: 31]
        public bool VerifierMotDePasse(string motDePasse)
        {
            return this.MotDePasseHash == motDePasse; // Vérification simple [cite: 31]
        }

        // Modifie les informations du profil [cite: 32]
        public void MettreAJourProfil(string nouveauNom, string nouveauTel)
        {
            this.NomUtilisateur = nouveauNom; // [cite: 4, 32]
            this.Telephone = nouveauTel; // [cite: 6, 32]
        }

        // Active le compte de l'utilisateur [cite: 34]
        public void ActiverCompte()
        {
            this.Statut = "Actif"; // [cite: 10, 34]
        }

        // Suspend le compte de l'utilisateur [cite: 35]
        public void SuspendreCompte()
        {
            this.Statut = "Suspendu"; // [cite: 10, 35]
        }
    }
}