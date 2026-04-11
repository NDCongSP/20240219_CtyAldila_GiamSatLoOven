using GiamSat.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Scada.TrackingTime_AutoRolling1
{
    public static class GlobalVariable
    {
        public static string ConnectionString { get; set; }
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

        public static List<RevoConfigModel> RevoConfigs { get; set; } = new List<RevoConfigModel>();
        public static List<RevoRealtimeModel> RevoRealtimeModels { get; set; } = new List<RevoRealtimeModel>();

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

        public static List<RevoStep> BuildSteps(AutoRollingTagChangedModel part)
        {
            var result = new List<RevoStep>();

            for (int i = 1; i < part.StepsIsRun.Length; i++)
            {
                result.Add(new RevoStep
                {
                    StepIndex = i,
                    StepName = $"Step {1}",
                    Enable = part.StepsIsRun[i],
                    TotalRunTime = 0
                });
            }

            return result.OrderBy(x => x.StepIndex).ToList();
        }

    }
}
