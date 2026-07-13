namespace HotelBackend.DTOs
{
    // Ce que le frontend envoie après extraction OCR côté navigateur
    public class ScanResultDto
    {
        // Données extraites par Tesseract.js
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Nationalite { get; set; }
        public string? Numero { get; set; }
        public DateTime? DateNaissance { get; set; }
        public DateTime? DateDelivrance { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string PaysEmission { get; set; } = "Cameroun";
        public string TypeDocument { get; set; } = "CNI";

        // Métriques du scan Tesseract
        public double ScoreConfiance { get; set; }
        public string QualiteImage { get; set; } = "Bonne";
        public string? TexteOCRBrut { get; set; }

        // Image encodée en base64 (optionnel, pour sauvegarde)
        public string? ImageBase64 { get; set; }
    }
}