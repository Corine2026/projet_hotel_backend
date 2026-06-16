using System;

namespace HotelBackend.Models
{
    public class MaintenanceChambre
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // -Id: Guid [cite: 92]
        
       // Clé étrangère vers Chambre [cite: 93]
        public Guid ChambreId { get; set; } 
        public virtual Chambre? Chambre { get; set; }

        public string Motif { get; set; } = string.Empty; // -Motif: string [cite: 94]
        public decimal Cout { get; set; } // -Cout: decimal [cite: 95]
        public DateTime DateDebut { get; set; } // -DateDebut: DateTime [cite: 96]
        public DateTime? DateFin { get; set; } // -DateFin: DateTime? [cite: 97]
        public string Statut { get; set; } = "En Cours"; // -Statut: string [cite: 98]

        // +Demarrer() [cite: 99]
        public void Demarrer()
        {
            Statut = "En Cours";
            DateDebut = DateTime.UtcNow;
            if (Chambre != null)
            {
                Chambre.MettreMaintenance(); // Met directement la chambre hors-service [cite: 87]
            }
        }

        // +Terminer(decimal coutReel) [cite: 99]
        public void Terminer(decimal coutReel)
        {
            Statut = "Terminee";
            Cout = coutReel;
            DateFin = DateTime.UtcNow;
            if (Chambre != null)
            {
                Chambre.MettreNettoyage(); // Envoie la chambre au nettoyage après travaux [cite: 88]
            }
        }

        // +Annuler() [cite: 100]
        public void Annuler()
        {
            Statut = "Annulee";
            DateFin = DateTime.UtcNow;
            if (Chambre != null)
            {
                Chambre.Etat = EtatChambre.Disponible; // Rend la chambre à son état normal [cite: 83]
            }
        }
    }
}