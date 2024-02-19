using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Scada
{
    public static class GlobalVariable
    {
        public static string ConnectionString { get; set; }

        public static SqlConnection GetDbConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static int LogInterval { get; set; }//chu kỳ log data. đơn vị giây
        public static int DisplayRealtimeInterval { get; set; }//chu kỳ update data hiển thị. đơn vị giây

    }
}
