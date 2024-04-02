using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Bảng dùng để điều khiển PLC từ web.
    /// </summary>
    [Table("FT06")]
    public class FT06
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Model chứa các thông tin cần điều khiển.List< ControlPlcModel>.
        /// </summary>
        public string C000 { get; set; }
    }
}
