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

        /// <summary>
        /// Định dánh máy theo ID để truy cập và DB lây thông tin cấu hình của máy để hoạt động, lưu trữ data theo thông tin này.
        /// </summary>
        public static int RevoId { get; set; }

        /// <summary>
        /// Thong tin Paat, luu lai part dang chay de hien thi, luu tru data theo part, va truy xuat thong tin cau hinh theo part.
        /// </summary>
        public static string Part { get; set; }

        ////chỉ lấy máy autorolling
        //public static RevoConfigs RevoConfigs { get; set; } = new RevoConfigs();
        //public static List<RevoRealtimeModel> RevoRealtimeModel { get; set; } = new List<RevoRealtimeModel>();

        public static string DecodeGotString(int[] words)
        {
            List<byte> bytes = new List<byte>();

            foreach (int w in words)
            {
                ushort word = (ushort)w;

                byte low = (byte)(word & 0xFF);
                byte high = (byte)(word >> 8);

                if (low != 0) bytes.Add(low);
                if (high != 0) bytes.Add(high);
            }

            return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
