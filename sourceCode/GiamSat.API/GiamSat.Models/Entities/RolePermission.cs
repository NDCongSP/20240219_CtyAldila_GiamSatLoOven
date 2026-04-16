using System;
using Microsoft.AspNetCore.Identity;

namespace GiamSat.Models
{
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public IdentityRole? Role { get; set; }

        public Guid PermissionId { get; set; }
        public SecurityPermission? Permission { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
