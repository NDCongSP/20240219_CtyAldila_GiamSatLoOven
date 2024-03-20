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
    /// <summary>
    /// Data Log.
    /// </summary>
    [Table("FT03")]
    public class FT03
    {
        [Key]
        public Guid Id { get; set; }

        public int OvenId { get; set; }
        public string? OvenName { get; set; }
        public double? Temperature { get; set; } = 0;

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public string? CreatedMachine { get; set; }
    }
}
