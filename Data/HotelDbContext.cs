using Microsoft.EntityFrameworkCore;
using HotelBackend.Models;

namespace HotelBackend.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        // ==========================================
        // TABLES : AUTHENTIFICATION & UTILISATEURS
        // ==========================================
        public DbSet<Role> Roles { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; } 
        public DbSet<SessionUtilisateur> SessionsUtilisateurs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; } 
        public DbSet<JournalActivite> JournauxActivites { get; set; } 
        public DbSet<MotDePasseReset> MotsDePasseResets { get; set; } 

        // ==========================================
        // TABLES : GESTION DES CHAMBRES
        // ==========================================
        public DbSet<Chambre> Chambres { get; set; }
        public DbSet<TypeChambre> TypeChambres { get; set; }
        public DbSet<MaintenanceChambre> Maintenances { get; set; } 
        public DbSet<HistoriqueEtat> HistoriqueEtats { get; set; } 

        // ==========================================
        // CONFIGURATION DES CLÉS & MAPPINGS
        // ==========================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configuration RolePermission
    modelBuilder.Entity<RolePermission>()
        .HasKey(rp => new { rp.RoleId, rp.PermissionId });

    // Mappage des tables
    modelBuilder.Entity<Chambre>().ToTable("Chambres");
    modelBuilder.Entity<TypeChambre>().ToTable("TypeChambres");
    modelBuilder.Entity<MaintenanceChambre>().ToTable("Maintenances");
    modelBuilder.Entity<HistoriqueEtat>().ToTable("HistoriqueEtats");

    // Configuration spécifique de Chambre (Groupée pour plus de clarté)
    modelBuilder.Entity<Chambre>(entity =>
    {
        // Conversion de l'Enum Etat en String pour PostgreSQL
        entity.Property(c => c.Etat)
              .HasConversion<string>();

        // Gestion du NULL pour la colonne Equipements (text[])
        entity.Property(c => c.Equipements)
              .HasDefaultValueSql("'{}'::text[]");
    });
}   }
}