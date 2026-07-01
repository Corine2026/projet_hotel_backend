namespace HotelBackend.Models
{
    public class HistoriqueClient
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateAction { get; set; }

        public static HistoriqueClient AjouterAction(Guid clientId, string action, string? description = null)
        {
            return new HistoriqueClient
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Action = action,
                Description = description,
                DateAction = DateTime.UtcNow
            };
        }
    }
}