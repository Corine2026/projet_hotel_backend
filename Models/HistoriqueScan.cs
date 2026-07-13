namespace HotelBackend.Models
{
    public class HistoriqueScan
    {
        public Guid Id { get; set; }
        public Guid ScanId { get; set; }
        public ScanDocument? Scan { get; set; }
        public DateTime DateOperation { get; set; } = DateTime.UtcNow;
        public string Action { get; set; } = string.Empty;
        public string? Utilisateur { get; set; }
        public string? Observation { get; set; }
    }
}