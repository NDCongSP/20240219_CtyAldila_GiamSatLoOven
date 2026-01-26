using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada_TrackingTIme_Revo
{
    public static class GlobalVariable
    {
        /// <summary>
        /// Định dánh máy theo ID để truy cập và DB lây thông tin cấu hình của máy để hoạt động, lưu trữ data theo thông tin này.
        /// </summary>
        public static int RevoId { get; set; }
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
