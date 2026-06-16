using System;

namespace HotelBackend.Models
{
    public class JournalActivite
    {
        // 1. LES PROPRIÉTÉS (Données du journal issues du diagramme)
        public Guid Id { get; set; } = Guid.NewGuid(); // Identifiant unique du log
        public Guid UtilisateurId { get; set; } // Qui a fait l'action
        public string Action { get; set; } = string.Empty; // Ex: "CONNEXION", "MODIFICATION_CHAMBRE"
        public string Description { get; set; } = string.Empty; // Ex: "A changé le statut de la chambre 5"
        public string AdresseIP { get; set; } = string.Empty; // L'adresse IP de la machine
        public DateTime DateAction { get; set; } = DateTime.UtcNow; // Date et heure exacte de l'action

        // Propriété de navigation pour Entity Framework
        public Utilisateur? Utilisateur { get; set; }


        // 2. LES MÉTHODES (La logique simple)

        // Enregistre une nouvelle action dans l'historique
        public void EnregistrerAction(Guid idUtilisateur, string typeAction, string descriptionAction, string ip)
        {
            this.UtilisateurId = idUtilisateur;
            this.Action = typeAction;
            this.Description = descriptionAction;
            this.AdresseIP = ip;
            this.DateAction = DateTime.UtcNow; // Enregistre l'heure actuelle
        }

        // Simule l'affichage des détails du log
        public string AfficherHistorique()
        {
            return $"[{DateAction}] User: {UtilisateurId} - Action: {Action} - Desc: {Description}";
        }

        // Simule un filtre (renvoie vrai si le log correspond à l'action recherchée)
        public bool FiltrerHistorique(string actionRecherchee)
        {
            return this.Action.Equals(actionRecherchee, StringComparison.OrdinalIgnoreCase);
        }
    }
}