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
    [Table("FT015")]
    public class FT015_SandingRealtime
    {
        [Key]
        [Browsable(false)]
        public Guid Id { get; set; }


        /// <summary>
        /// RevoRealtimeModel
        /// </summary>
        [Column("C001")]
        public string? C001_Data { get; set; }
    }
}
