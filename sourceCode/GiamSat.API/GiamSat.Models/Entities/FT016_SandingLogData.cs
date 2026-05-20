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

        public string? Part { get; set; } = null;

        public string? Work { get; set; } = null;

        public int? Formula { get; set; } = null;

        /// <summary>
        /// 1- all
        /// 2- 5 cây
        /// 3- không lưu
        /// </summary>
        public int? LogType { get; set; } = null;

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
    }
}
