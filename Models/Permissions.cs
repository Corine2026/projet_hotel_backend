using System;

namespace HotelBackend.Models
{
    public class Permission
    {
        // 1. LES PROPRIÉTÉS (Les données de la Permission)
        public Guid Id { get; set; } = Guid.NewGuid(); // Identifiant unique [cite: 23]
        public string Nom { get; set; } = string.Empty; // Nom lisible (Ex: "Créer une chambre") 
        public string Code { get; set; } = string.Empty; // Code technique unique (Ex: "CHAMBRE_CREER") 
        public string Description { get; set; } = string.Empty; // Explications sur ce que permet ce code [cite: 25]


        // 2. LES MÉTHODES (La logique simple)

        // Simule la création d'une nouvelle permission
        public void CreerPermission(string nom, string code, string description)
        {
            this.Nom = nom;
            this.Code = code;
            this.Description = description;
        }

        // Modifie une permission existante
        public void ModifierPermission(string nouveauNom, string nouvelleDescription)
        {
            this.Nom = nouveauNom;
            this.Description = nouvelleDescription;
        }

        // Simule la suppression (en réinitialisant les champs par exemple)
        public void SupprimerPermission()
        {
            this.Nom = string.Empty;
            this.Code = string.Empty;
            this.Description = string.Empty;
        }
    }
}