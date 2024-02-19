using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Bảng lưu lại lịch sử các lần thay đổi thông tin của mẻ nuôi.
    /// </summary>
    public class FT101
    {
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// Chu Ky Nuoi.
        /// </summary>
        public Guid FT100Id { get; set; }
        /// <summary>
        /// Luu lại những lần thay đổi thông số, SettingModel, Quan tâm đến properties NangSuatModel
        /// </summary>
        public string? Details { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;        
        public int? IsActived { get; set; } = 1;
        public string? CreatedMachine { get; set; }
    }
}
