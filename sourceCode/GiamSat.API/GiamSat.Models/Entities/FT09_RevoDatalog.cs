using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    [Table("FT09")]
    public class FT09_RevoDatalog
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedMachine { get; set; }

        public int? RevoId { get; set; }

        public string? RevoName { get; set; }

        public string? Work { get; set; }

        public string? Part { get; set; }

        public string? Rev { get; set; }

        public string? ColorCode { get; set; }

        public string? Mandrel { get; set; }

        public string? MandrelStart { get; set; }

        public int? StepId { get; set; }

        public string? StepName { get; set; }

        public DateTime? StartedAt { get; set; } = null;

        public DateTime? EndedAt { get; set; } = null;

        /// <summary>
        /// đánh dấu cây shaft mà dữ liệu này thuộc về.
        /// cá bước trong 1 cây shaft thì có cùng 1 ShaftNum.
        /// </summary>
        [Browsable(false)]
        public Guid? ShaftNum { get; set; } = null;

        /// <summary>
        /// Tổng thời gian chạy của bước = (StartAt - EndAt).totalSecond. 
        /// </summary>
        public double? TotalTime { get; set; } = 0;

        [NotMapped]
        public int ShaftNo { get; set; } = 1;
    }
}
