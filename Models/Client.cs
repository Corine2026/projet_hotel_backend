namespace HotelBackend.Models
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public SexeClient Sexe { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? Ville { get; set; }
        public string Telephone { get; set; } = string.Empty;
        public string NumeroCNI { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public List<HistoriqueClient> Historique { get; set; } = new();

        // ── Méthodes métier (comme dans ton diagramme UML) ──

        public int CalculerAge()
        {
            var today = DateTime.UtcNow;
            var age = today.Year - DateNaissance.Year;
            if (DateNaissance.Date > today.AddYears(-age)) age--;
            return age;
        }

        public bool EstMajeur() => CalculerAge() >= 18;

        public void ModifierInfos(string nom, string prenom, string ville, string telephone)
        {
            Nom = nom;
            Prenom = prenom;
            Ville = ville;
            Telephone = telephone;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}