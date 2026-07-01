namespace HotelBackend.DTOs
{
    public class ClientDto
    {
        public Guid? Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        // String pour la même raison que ChambreDto.Etat : compatibilité JSON/Enum.TryParse
        public string Sexe { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public string? Ville { get; set; }
        public string Telephone { get; set; } = string.Empty;
        public string NumeroCNI { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}