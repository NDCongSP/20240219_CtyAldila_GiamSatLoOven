using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Scada
{
    public class SettingsModel
    {
        public Guid Chuongid { get; set; }
        public List<StepSettingsModel> Steps { get; set; } = new List<StepSettingsModel>();
        public GeneralStepSettingsModel GeneralSettings { get; set; } = new GeneralStepSettingsModel();
    }
}
