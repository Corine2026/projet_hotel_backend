namespace HotelBackend.DTOs
{
    public class ChambreDto
    {
        public Guid? Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public Guid TypeChambreId { get; set; }
        public decimal PrixNuitee { get; set; }
        public decimal PrixSieste { get; set; }
        public int Capacite { get; set; }
        public int Etage { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string>? Equipements { get; set; }

        // Changement : on utilise string pour la compatibilité JSON/DB
        public string? Etat { get; set; } 
    }
}