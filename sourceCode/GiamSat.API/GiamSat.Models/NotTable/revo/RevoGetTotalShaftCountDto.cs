using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class RevoGetTotalShaftCountDto
    {
        public int RevoId { get; set; }

        public int TotalShaftCurrentHour { get; set; }
        public int TotalShaftFinishCurrentHour { get; set; }
        public int TotalShaftLastHour { get; set; }
        public int TotalShaftFinshLastHour { get; set; }
    }
}
