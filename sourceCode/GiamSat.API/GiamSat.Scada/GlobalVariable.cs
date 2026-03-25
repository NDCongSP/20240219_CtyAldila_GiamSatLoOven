using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GiamSat.Scada
{
    public static class GlobalVariable
    {
        public static string ConnectionString { get; set; }

        public static SqlConnection GetDbConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static ConfigModel ConfigSystem { get; set; } = new ConfigModel();
        //public static int LogInterval { get; set; }//chu kỳ log data. đơn vị giây
        //public static int DisplayRealtimeInterval { get; set; }//chu kỳ update data hiển thị. đơn vị giây

        public static void InvokeIfRequired(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
