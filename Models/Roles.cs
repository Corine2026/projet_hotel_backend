using System;
using System.Collections.Generic;

namespace HotelBackend.Models
{
    public class Role
    {
        // 1. LES PROPRIÉTÉS (Les données du Rôle)
        public Guid Id { get; set; } = Guid.NewGuid(); // Identifiant unique (Guid)
        public string Nom { get; set; } = string.Empty; // Ex: "Admin", "Receptionniste"
        public string Description { get; set; } = string.Empty; // Ex: "Accès total au système"

        // Une liste temporaire pour stocker les permissions de ce rôle sous forme de texte
        public List<string> Permissions { get; set; } = new List<string>();


        // 2. LES MÉTHODES (La logique simple)

        // Ajoute une permission au rôle
        public void AjouterPermission(string codePermission)
        {
            if (!Permissions.Contains(codePermission))
            {
                Permissions.Add(codePermission);
            }
        }

        // Retire une permission du rôle
        public void RetirerPermission(string codePermission)
        {
            if (Permissions.Contains(codePermission))
            {
                Permissions.Remove(codePermission);
            }
        }

        // Retourne la liste de toutes les permissions de ce rôle
        public List<string> ListerPermission()
        {
            return Permissions;
        }

        // Modifie les informations principales du rôle
        public void ModifierRole(string nouveauNom, string nouvelleDescription)
        {
            this.Nom = nouveauNom;
            this.Description = nouvelleDescription;
        }
    }
}