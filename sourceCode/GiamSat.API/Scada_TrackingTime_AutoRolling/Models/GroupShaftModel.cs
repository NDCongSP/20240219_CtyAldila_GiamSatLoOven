using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada_TrackingTime_AutoRolling
{
    public class GroupShaftModel
    {
        public int CurrentHour { get; set; } = 0;

        public int LastHour { get; set; } = 0;
    }
}
