namespace HotelBackend.Models
{
    public class ScanDocument
    {
        public Guid Id { get; set; }
        public Guid? DocumentId { get; set; }
        public DocumentIdentite? Document { get; set; }
        public string? ImageOriginale { get; set; }
        public string? TexteOCR { get; set; }
        public double ScoreConfiance { get; set; }
        public string QualiteImage { get; set; } = "Bonne";
        public DateTime DateScan { get; set; } = DateTime.UtcNow;
        public StatutScan Statut { get; set; } = StatutScan.Reussi;
        public List<HistoriqueScan> Historique { get; set; } = new();
    }
}