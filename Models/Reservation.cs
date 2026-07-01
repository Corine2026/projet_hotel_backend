namespace HotelBackend.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public Guid ChambreId { get; set; }
        public Chambre? Chambre { get; set; }
        public TypeReservation Type { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public decimal Montant { get; set; }
        public StatutReservation Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ── Méthodes métier (UML) ──

        public decimal CalculerMontant(decimal prixNuitee, decimal prixSieste)
        {
            if (Type == TypeReservation.Sieste)
                return prixSieste;

            var nuits = Math.Max(1, (DateFin.Date - DateDebut.Date).Days);
            return prixNuitee * nuits;
        }

        public void Confirmer()
        {
            Statut = StatutReservation.Confirmee;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Annuler()
        {
            Statut = StatutReservation.Annulee;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Terminer()
        {
            Statut = StatutReservation.Terminee;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}