using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GiamSat.Scada
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //đọc các giá trị khởi tạo ban đầu
            //GlobalVariable.ConnectionString = EncodeMD5.DecryptString(Properties.Settings.Default.ConnectionString, "PTAut0m@t!0n30!)@)20");
            GlobalVariable.ConnectionString = Properties.Settings.Default.ConnectionString;
            //GlobalVariable.LogInterval = Properties.Settings.Default.LogInterval;
            //GlobalVariable.DisplayRealtimeInterval = Properties.Settings.Default.DisplayRealtimeInterval;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}