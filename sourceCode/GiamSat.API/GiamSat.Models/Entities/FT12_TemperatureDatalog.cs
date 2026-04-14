using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiamSat.Models
{
    /// <summary>
    /// bảng data log của hệ thống giám sát nhiệt độ nhà máy.
    /// </summary>
    [Table("FT12")]
    public class FT12_TemperatureDatalog
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedMachine { get; set; }

        public DateTime? UpdateddAt { get; set; }

        public int? LocationId { get; set; }

        public string? LocationName { get; set; }

        public double? PV { get; set; } = 0;
    }
}
