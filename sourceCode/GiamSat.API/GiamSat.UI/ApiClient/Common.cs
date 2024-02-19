using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.UI
{
    public class Common
    {
        public ISDataLog DataLogApiClient { get;private set; }
        public ISDisplayRealtime DisplayRealtimeApiClient { get;private set; }

        public Common(ISDataLog dataLogApiClient, ISDisplayRealtime displayRealtimeApiClient)
        {
            DataLogApiClient = dataLogApiClient;
            DisplayRealtimeApiClient = displayRealtimeApiClient;
        }
    }
}
