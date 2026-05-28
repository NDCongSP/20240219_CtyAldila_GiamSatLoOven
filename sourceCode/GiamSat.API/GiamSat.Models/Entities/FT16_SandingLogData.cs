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
    [Table("FT16")]
    public class FT16_SandingLogData
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedMachine { get; set; }

        public string? Part { get; set; } = null;

        public string? Work { get; set; } = null;

        public int? Formula { get; set; } = null;

        /// <summary>
        /// Chế độ log data vào bảng data log FT16.
        /// </summary>
        public EnumSandingLogType? LogType { get; set; } = EnumSandingLogType.Log_All;

        public int? ShaftNum { get; set; } = null;

        public double? MotorSandingSpeed { get; set; } = 0;

        //sanding speed của máy mài, đọc từ tag Motor_Sanding_Speed.
        public double? SpineA { get; set; } = 0;
        public double? SpineB { get; set; } = 0;
        public double? SpineTarget { get; set; } = 0;
        public double? Spine_Low { get; set; } = 0;
        public double? Spine_Hight { get; set; } = 0;
        public int? OK_NG_SpineB { get; set; } = 0;

        //OD
        public string? TipOdLength_1 { get; set; } = null;
        public string? TipOdLength_2 { get; set; } = null;
        public string? TipOdLength_3 { get; set; } = null;
        public double? Diam_Reading_1 { get; set; } = 0;
        public double? Diam_Reading_2 { get; set; } = 0;
        public double? Diam_Reading_3 { get; set; } = 0;
        public int? OK_NG_OD_1 { get; set; } = null;
        public int? OK_NG_OD_2 { get; set; } = null;
        public int? OK_NG_OD_3 { get; set; } = null;
        public int? Diam_LL_1 { get; set; } = null;
        public int? Diam_LL_2 { get; set; } = null;
        public int? Diam_LL_3 { get; set; } = null;
        public int? Diam_UL_1 { get; set; } = null;
        public int? Diam_UL_2 { get; set; } = null;
        public int? Diam_UL_3 { get; set; } = null;

        /// <summary>
        /// Báo cho biết log này được tạo ra trong chế độ nào của máy mài, ví dụ như chế độ sản xuất, chế độ test, chế độ demo, v.v. Tùy vào từng loại máy mài mà sẽ có các chế độ khác nhau, và mỗi chế độ sẽ có cách xử lý dữ liệu log khác nhau. Ví dụ như trong chế độ sản xuất thì sẽ lưu tất cả các log, còn trong chế độ test thì chỉ lưu một số log nhất định, còn trong chế độ demo thì không lưu log nào cả.
        /// Khi tinh ABCD thì đọc với mode Test.
        /// AUTO_MANUAL tag.
        /// 
        /// 
        /// </summary>
        public EnumSandingMode? SandingMode { get; set; } = EnumSandingMode.Production;
    }
}
