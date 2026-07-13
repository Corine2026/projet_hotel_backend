using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;

namespace HotelBackend.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        // AUTHENTIFICATION & UTILISATEURS
        public DbSet<Role> Roles { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; } 
        public DbSet<SessionUtilisateur> SessionsUtilisateurs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; } 
        public DbSet<JournalActivite> JournauxActivites { get; set; } 
        public DbSet<MotDePasseReset> MotsDePasseResets { get; set; } 

        // GESTION DES CHAMBRES & CLIENTS
        public DbSet<Chambre> Chambres { get; set; }
        public DbSet<TypeChambre> TypeChambres { get; set; }
        public DbSet<MaintenanceChambre> Maintenances { get; set; } 
        public DbSet<HistoriqueEtat> HistoriqueEtats { get; set; } 
        public DbSet<Client> Clients { get; set; }
        public DbSet<HistoriqueClient> HistoriqueClients { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<DocumentIdentite> DocumentsIdentite { get; set; }
        public DbSet<ScanDocument>     ScanDocuments     { get; set; }
        public DbSet<HistoriqueScan>   HistoriqueScans   { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. CONFIGURATION CLIENTS
            modelBuilder.Entity<Client>(entity => 
            {
                entity.ToTable("Clients");
                entity.HasIndex(c => c.NumeroCNI).IsUnique();
            });

            modelBuilder.Entity<HistoriqueClient>(entity =>
            {
                entity.ToTable("HistoriqueClients");
                entity.HasOne(h => h.Client)
                      .WithMany(c => c.Historique)
                      .HasForeignKey(h => h.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 2. CONFIGURATION CHAMBRES
            modelBuilder.Entity<Chambre>(entity =>
            {
                entity.ToTable("Chambres");
                entity.Property(c => c.Etat).HasConversion<string>();
                
                // Correction pour PostgreSQL : Gestion explicite du type tableau
                entity.Property(c => c.Equipements)
                      .HasColumnType("text[]")
                      .HasDefaultValueSql("'{}'::text[]");
            });

            modelBuilder.Entity<DocumentIdentite>(entity => {
    entity.Property(d => d.TypeDocument).HasConversion<string>();
});

modelBuilder.Entity<ScanDocument>(entity => {
    entity.Property(s => s.Statut).HasConversion<string>();
});

modelBuilder.Entity<HistoriqueScan>()
    .HasOne(h => h.Scan)
    .WithMany(s => s.Historique)
    .HasForeignKey(h => h.ScanId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<DocumentIdentite>()
    .HasOne(d => d.Client)
    .WithMany()
    .HasForeignKey(d => d.ClientId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>(entity =>
{
                entity.Property(r => r.Type).HasConversion<string>();
                entity.Property(r => r.Statut).HasConversion<string>();
                entity.HasOne(r => r.Client).WithMany().HasForeignKey(r => r.ClientId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(r => r.Chambre).WithMany().HasForeignKey(r => r.ChambreId).OnDelete(DeleteBehavior.Restrict);
});

            // 3. CONFIGURATION TYPES & MAINTENANCE
            modelBuilder.Entity<TypeChambre>().ToTable("TypeChambres");
            modelBuilder.Entity<MaintenanceChambre>().ToTable("Maintenances");
            modelBuilder.Entity<HistoriqueEtat>().ToTable("HistoriqueEtats");

            // 4. CONFIGURATION ROLES
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });
        }
    }
}