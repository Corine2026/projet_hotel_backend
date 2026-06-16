using System;

namespace HotelBackend.Models
{
    public class HistoriqueEtat
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // -Id: Guid [cite: 102]
        
        // Clé étrangère vers Chambre [cite: 103]
        public Guid ChambreId { get; set; }
        public virtual Chambre? Chambre { get; set; }

        public string? AncienEtat { get; set; } // -AncienEtat: string [cite: 104]
        public string NouvelEtat { get; set; } = string.Empty; // -NouvelEtat: string [cite: 104]
        public DateTime DateChangement { get; set; } = DateTime.UtcNow; // -DateChangement [cite: 104]
        
        // Clé étrangère vers l'Utilisateur connecté du Module 1
        public Guid UtilisateurId { get; set; } // -UtilisateurId: Guid [cite: 104]
    }
}