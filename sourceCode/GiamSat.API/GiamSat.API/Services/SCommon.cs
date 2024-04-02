using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SCommon
    {
        public ISFT01 SFT01 { get; private set; }
        public ISFT02 SFT02 { get; private set; }
        public ISFT03 SFT03 { get; private set; }
        public ISFT04 SFT04 { get; private set; }
        public ISFT05 SFT05 { get; private set; }
        public ISFT06 SFT06 { get; private set; }

        public SCommon(ISFT01 sFT01 = null, ISFT02 sFT02 = null, ISFT03 sFT03 = null, ISFT04 sFT04 = null, ISFT05 sFT05 = null, ISFT06 sFT06 = null)
        {
            SFT01 = sFT01;
            SFT02 = sFT02;
            SFT03 = sFT03;
            SFT04 = sFT04;
            SFT05 = sFT05;
            SFT06 = sFT06;
        }
    }
}
