using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Bảng chứa giá trị hiển thị web realtime Display.
    /// </summary>
    [Table("FT02")]
    public class FT02

    {
        [Key]
        public Guid Id { get; set; }
        public string? C000 { get; set; }
        public bool? Actived { get; set; } = true;
        public DateTime? CreatedDate { get; set; }=DateTime.Now;
        public string? CreatedMachine { get; set; }
    }
}
