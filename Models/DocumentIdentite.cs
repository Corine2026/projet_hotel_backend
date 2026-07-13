namespace HotelBackend.Models
{
    public class DocumentIdentite
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public TypeDocument TypeDocument { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime? DateDelivrance { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string PaysEmission { get; set; } = "Cameroun";
        public string? PhotoDocument { get; set; }
        public string? PhotoPortrait { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public List<ScanDocument> Scans { get; set; } = new();

        public bool EstValide() =>
            !string.IsNullOrWhiteSpace(Numero) &&
            (DateExpiration == null || DateExpiration > DateTime.UtcNow);

        public bool EstExpire() =>
            DateExpiration.HasValue && DateExpiration.Value <= DateTime.UtcNow;
    }
}