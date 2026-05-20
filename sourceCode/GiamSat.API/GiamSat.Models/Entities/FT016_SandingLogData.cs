using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("FT016")]
    public class FT016_SandingLogData
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedMachine { get; set; }

        public string? Part { get; set; }

        public string? Work { get; set; }
    }
}
