using System;

namespace HotelBackend.Models
{
    public class RolePermission
    {
        // 1. LES PROPRIÉTÉS (Les clés étrangères qui font le pont) 
        public Guid RoleId { get; set; } // Identifiant du rôle 
        public Guid PermissionId { get; set; } // Identifiant de la permission 

        // Propriétés de navigation pour Entity Framework
        public Role? Role { get; set; }
        public Permission? Permission { get; set; }


        // 2. LES MÉTHODES (La logique simple) 

        // Associe une permission à un rôle [cite: 52]
        public void AssignerPermission(Guid idRole, Guid idPermission)
        {
            this.RoleId = idRole;
            this.PermissionId = idPermission;
        }

        // Simule le retrait en réinitialisant les identifiants [cite: 52]
        public void RetirerPermission()
        {
            this.RoleId = Guid.Empty;
            this.PermissionId = Guid.Empty;
        }
    }
}