using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Scada
{
   public  class StepSettingsModel
    {
        public int StepId { get; set; }//giai đoạn
        [Required]
        [Range(0, 100, ErrorMessage = "chỉ được nhập từ 0 đến 100")]
        public int FromDate { get; set; }
        [Required]
        [Range(0, 100, ErrorMessage = "chỉ được nhập từ 0 đến 100")]
        public int ToDate { get; set; }

        [Required]
        [RegularExpression(@"^[0-4]{1}$", ErrorMessage = "Chỉ được nhập số nguyên <= 4")]
        public int StaticFanRun { get; set; }

        [Required]
        public double HightTemperature { get; set; }
        [Required]
        public double Lowtemperature { get; set; }

        [Required]
        [RegularExpression(@"^[0-50]{1}$", ErrorMessage = "Chỉ nhập <= 50 Hz")]
        public double HightFrequency { get; set; }
        public double LowFrequency { get; set; }
        public double TempRunFan1 { get; set; }
        public double TempRunFan2 { get; set; }
        public double TempRunFan3 { get; set; }
        public double TempRunFan4 { get; set; }
        public double TempRunCooler { get; set; }
    }
}
