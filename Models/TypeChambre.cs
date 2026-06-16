using System;
using System.Collections.Generic;

namespace HotelBackend.Models
{
    public class TypeChambre
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // -Id: Guid [cite: 70]
        public string Nom { get; set; } = string.Empty; // -Nom: string [cite: 70]
        public string? Description { get; set; } // -Description: string [cite: 71]
        public int CapaciteMax { get; set; } // -Capacite Max: int [cite: 71]

        // Propriété de navigation pour Entity Framework (Relation 1 à *) [cite: 75, 77]
        public virtual ICollection<Chambre> Chambres { get; set; } = new List<Chambre>();
    }
}