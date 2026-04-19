using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class Permissions
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Module { get; set; }
        public string? Actions { get; set; }
        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedMachine { get; set; }

        public bool? IsActived { get; set; } = true;
    }
}
