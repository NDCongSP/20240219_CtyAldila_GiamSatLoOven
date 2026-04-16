using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GiamSat.Models
{
    public class SecurityPermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Module { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
