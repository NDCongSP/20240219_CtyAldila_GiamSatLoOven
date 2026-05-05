using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("RoleToPermissions")]
    public class RoleToPermission
    {
        [Key] public Guid Id { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public Guid PermissionId { get; set; }
        public string? PermisionName { get; set; }
        public string PermisionDescription { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedMachine { get; set; }

        public bool? IsActived { get; set; } = true;
    }
}
