namespace HotelBackend.DTOs
{
    public class ReservationDto
    {
        public Guid? Id { get; set; }
        public string? Numero { get; set; }
        public Guid ClientId { get; set; }
        public Guid ChambreId { get; set; }
        public string Type { get; set; } = "Nuitee";
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public decimal? Montant { get; set; }
        public string? Statut { get; set; }
    }
}