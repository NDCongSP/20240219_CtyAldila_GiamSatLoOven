using EasyScada.Winforms.Controls;
using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Thong tin Paat, luu lai part dang chay de hien thi, luu tru data theo part, va truy xuat thong tin cau hinh theo part.
        /// </summary>
        public static string Part { get; set; }

        /// <summary>
        /// Tùy chọn để lưu thời gian của 1 cây shaft, hay là lưu chi tiết từng bước.
        /// False: chỉ lưu thời gian tổng của cây shaft, True: lưu chi tiết thời gian của từng bước (P1, P2, P3,...)
        /// </summary>
        public static bool SaveAll { get; set; } = false;

        public static RevoConfigModel RevoConfig { get; set; } = new RevoConfigModel();
        public static RevoRealtimeModel RevoRealtimeModel { get; set; } = new RevoRealtimeModel();

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

        public static string? IpDbServer { get; set; }

        public static List<T> ToModels<T>(DataTable dt) where T : new()
        {
            var result = new List<T>();
            if (dt == null) return result;

            var props = typeof(T).GetProperties()
                                 .Where(p => p.CanWrite)
                                 .ToDictionary(p => p.Name, p => p,
                                               StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in dt.Rows)
            {
                var obj = new T();

                foreach (DataColumn col in dt.Columns)
                {
                    if (!props.TryGetValue(col.ColumnName, out var prop))
                        continue;

                    if (row.IsNull(col))
                        continue;

                    try
                    {
                        var value = row[col];
                        var targetType =
                            Nullable.GetUnderlyingType(prop.PropertyType)
                            ?? prop.PropertyType;

                        var safeValue = Convert.ChangeType(value, targetType);
                        prop.SetValue(obj, safeValue);
                    }
                    catch
                    {
                        // bỏ qua nếu convert lỗi
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        private static readonly PropertyInfo[] StepProps =
                  typeof(PartModel)
                    .GetProperties()
                    .Where(p => Regex.IsMatch(p.Name, @"^P\d{1,2}$"))
                    .OrderBy(p =>
                    {
                        int.TryParse(p.Name.Substring(1), out int n);
                        return n;
                    })
                    .ToArray();

        public static List<RevoStep> BuildSteps(PartModel part)
        {
            var result = new List<RevoStep>();
            int index = 1;

            foreach (var prop in StepProps)
            {
                var value = prop.GetValue(part)?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var stepSplit = value.Split('|');

                var gocQuay = stepSplit.Length > 1 ? 
                    int.Parse(stepSplit[1]) :
                    0;

                var tocDo = stepSplit.Length > 2 ?
                   int.Parse(stepSplit[2]) :
                   0;

                var soXung = (int)(gocQuay * GlobalVariable.RevoConfig.Pulse_Rev / 360.0);
                var speed_PulsePerSec = (int)(tocDo * (GlobalVariable.RevoConfig.Pulse_Rev+GlobalVariable.RevoConfig.Pulse_rev_Offset) / 360);

                result.Add(new RevoStep
                {
                    StepIndex = int.Parse(prop.Name.Substring(1)),
                    StepName = stepSplit[0],
                    StepConfig = value,
                    Visible = true,
                    Enable = gocQuay==0&& tocDo==0 ? false : true,
                    SoLuongXung = soXung,
                    Speed_Hz = speed_PulsePerSec,
                });
            }

            return result.OrderBy(x=>x.StepIndex).ToList();
        }

    }
}
