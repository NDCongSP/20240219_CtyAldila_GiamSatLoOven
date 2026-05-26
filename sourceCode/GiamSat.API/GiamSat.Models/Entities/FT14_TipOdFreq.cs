using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    /// <summary>
    /// Bảng lưu các data master các thông số Freq_Target, ABCD, TipOD
    /// </summary>
    [Table("FT14")]
    public class FT14_TipOdFreq
    {
        [Key]
        [Browsable(false)]
        public Guid Id { get; set; }

        [Browsable(false)]
        public DateTime CreatedAt { get; set; }

        [Browsable(false)]
        public string? CreatedMachine { get; set; }

        [Browsable(false)]
        public DateTime? UpdateddAt { get; set; }

        [Browsable(false)]
        public bool? Actived { get; set; } = true;

        /// <summary>
        /// Chính là thông số itemNumber.
        /// </summary>
        public string? PartName { get; set; }

        public double? Length { get; set; } = 0;

        public double? FreqTarget { get; set; } = 0;

        public double? Set_Freq_Offset_Low { get; set; } = 0;
        public double? Set_Freq_Offset_Hight { get; set; } = 0;
        public double? Formula_F { get; set; } = 0;

        public double? A { get; set; } = 0;
        public double? B { get; set; } = 0;
        public double? C { get; set; } = 0;
        public double? D { get; set; } = 0;

        /// <summary>
        /// quy định điểm đo Tip OD sẽ được lấy ở đâu, ví dụ như lấy ở điểm đo nào đó trên máy, hoặc lấy ở điểm đo nào đó trên sản phẩm, hoặc lấy ở điểm đo nào đó trên cả máy và sản phẩm. Tùy vào từng loại sản phẩm mà sẽ có quy định khác nhau.
        /// đọc về cắt lấy phần số truyền xuống PLC. tag Set_Tip_OD_Length_1.
        /// </summary>
        public string? TipOdLength_1 { get; set; }
        public double? Diam_LL_1 { get; set; } = 0;
        public double? Diam_UL_1 { get; set; } = 0;

        public string? TipOdLength_2 { get; set; }
        public double? Diam_LL_2 { get; set; } = 0;
        public double? Diam_UL_2 { get; set; } = 0;

        public string? TipOdLength_3 { get; set; }
        public double? Diam_LL_3 { get; set; } = 0;
        public double? Diam_UL_3 { get; set; } = 0;
    }
}
