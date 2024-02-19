using GiamSat.Models;
using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SCommon
    {
        public ISDisplayRealtime SDisplayRealtime { get; private set; }
        public ISDataLog  SDataLog { get;private set; }

        public ISChuongInfo SChuongInfo { get;private set; }
        public ISRealtimeDisplay SRealtimeDisplay { get; private set; }
        public ISFT100 SFT100 { get; private set; }
        public ISFT101 SFT101 { get; private set; }

        public ISAlarmLog SAlarmLog { get; private set; }
        public ISAlarmSettings SAlarmSettings { get; private set; }

        public SCommon(ISDisplayRealtime sDisplayRealtime, ISDataLog sDataLog, ISChuongInfo sChuongInfo, ISRealtimeDisplay sRealtimeDisplay = null
            , ISFT100 sFT100 = null, ISFT101 sFT101 = null, ISAlarmLog sAlarmLog = null, ISAlarmSettings sAlarmSettings = null)
        {
            SDisplayRealtime = sDisplayRealtime;
            SDataLog = sDataLog;
            SChuongInfo = sChuongInfo;
            SRealtimeDisplay = sRealtimeDisplay;
            SFT100 = sFT100;
            SFT101 = sFT101;
            SAlarmLog = sAlarmLog;
            SAlarmSettings = sAlarmSettings;
        }
    }
}
