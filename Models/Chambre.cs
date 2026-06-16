using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBackend.Models
{
    public class Chambre
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        public Guid TypeChambreId { get; set; }
        
        [ForeignKey(nameof(TypeChambreId))]
        public virtual TypeChambre? TypeChambre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixNuitee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixSieste { get; set; }

        public int Capacite { get; set; }
        
        // Corrigé : int? permet de gérer les valeurs NULL de la base de données
        public int? Etage { get; set; }

        public string? Description { get; set; }

        // Corrigé : List<string> correspond au type text[] dans PostgreSQL
        [Column(TypeName = "text[]")]
        // Modifiez cette ligne
        public List<string>? Equipements { get; set; } = new List<string>();
        [Required]
        public EtatChambre Etat { get; set; } = EtatChambre.Disponible;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<MaintenanceChambre> Maintenances { get; set; } = new List<MaintenanceChambre>();
        public virtual ICollection<HistoriqueEtat> Historiques { get; set; } = new List<HistoriqueEtat>();

        // ==========================================
        // MÉTHODES MÉTIERS
        // ==========================================
        public void MettreMaintenance()
        {
            Etat = EtatChambre.HorsService;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MettreNettoyage()
        {
            Etat = EtatChambre.EnNettoyage;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MettreAJour(string numero, Guid typeChambreId, int etage, int capacite, decimal prixNuitee, decimal prixSieste, string? description, List<string> equipements, EtatChambre etat)
        {
            Numero = numero;
            TypeChambreId = typeChambreId;
            Etage = etage;
            Capacite = capacite;
            PrixNuitee = prixNuitee;
            PrixSieste = prixSieste;
            Description = description;
            Equipements = equipements;
            Etat = etat;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}