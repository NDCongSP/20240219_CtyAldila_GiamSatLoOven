using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada.Sanding
{
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector;
        private ConnectionStatus _easyStatus = ConnectionStatus.Disconnected;
        private CancellationTokenSource _timerCts;
        private Task _timerTask;

        private string _lastPartName = string.Empty;
        private string _lastWorkOrder = string.Empty;

        private readonly string _basePath = "Local Station/Channel1/Device1";

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                LogEvent("Đang khởi động ứng dụng...");

                // Đọc chuỗi kết nối từ DBContext
                using (var dbContext = new ApplicationDbContext())
                {
                    var constring = dbContext.Database.Connection.ConnectionString;
                    GlobalVariable.ConnectionString = constring;
                    GlobalVariable.IpDbServer = constring.Split(';')[0].Split('=')[1];
                    LogEvent($"Kết nối CSDL: {GlobalVariable.IpDbServer}");
                }

                // Cập nhật UI ban đầu
                lblPlcStatusVal.Text = "Disconnected";
                lblPlcStatusVal.ForeColor = Color.Red;

                // Khởi tạo EasyDriverConnector
                _easyDriverConnector = new EasyDriverConnector();
                _easyDriverConnector.ConnectionStatusChaged += EasyDriverConnector_ConnectionStatusChanged;
                _easyDriverConnector.Started += EasyDriverConnector_Started;

                _easyDriverConnector.BeginInit();
                _easyDriverConnector.EndInit();

                if (_easyDriverConnector.IsStarted)
                {
                    EasyDriverConnector_Started(null, null);
                }

                // Bắt đầu timer lưu realtime 5s
                _timerCts = new CancellationTokenSource();
                _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

                LogEvent("Khởi tạo hệ thống thành công.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi Load Form1.");
                LogEvent($"Lỗi khởi động: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _timerCts?.Cancel();
                if (_easyDriverConnector != null)
                {
                    _easyDriverConnector.ConnectionStatusChaged -= EasyDriverConnector_ConnectionStatusChanged;
                    _easyDriverConnector.Started -= EasyDriverConnector_Started;
                    _easyDriverConnector.Stop();
                }
                LogEvent("Đã dừng kết nối PLC.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi đóng Form1.");
            }
        }

        private void EasyDriverConnector_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _easyStatus = e.NewStatus;
            UpdateFooterStatus();
        }

        private async void EasyDriverConnector_Started(object sender, EventArgs e)
        {
            await Task.Delay(1000);
            LogEvent("EasyDriver Connector đã Started. Đang kết nối các Tags...");

            try
            {
                // Đăng ký sự kiện ValueChanged cho tất cả các tags
                RegisterTagEvents();
                LogEvent("Đăng ký sự kiện Tags thành công.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi đăng ký Tags ValueChanged.");
                LogEvent($"Lỗi đăng ký Tags: {ex.Message}");
            }
        }

        private void RegisterTagEvents()
        {
            // Subscribe to all 63 tags in Modbus Device
            string[] tags = new string[]
            {
                "PartName_1", "PartName_2", "PartName_3", "PartName_4", "PartName_5",
                "PartName_6", "PartName_7", "PartName_8", "PartName_9", "PartName_10",
                "Work_1", "Work_2", "Work_3", "Work_4", "Work_5",
                "Work_6", "Work_7", "Work_8", "Work_9", "Work_10",
                "Set_Trigger_NewPartInfo1", "Log_Style", "Trigger_Log_Sanding", "Trigger_Log_OD",
                "Shaft_Num_Sanding", "Spine_A", "Mortor_Sanding_Speed", "Spine_B", "Spine_Target",
                "Spine_Low", "Spine_Hight", "OK_NG_Sanding", "Shaft_Num_OD",
                "Diam_Reading_1", "Diam_Reading_2", "Diam_Reading_3",
                "OK_NG_OD_1", "OK_NG_OD_2", "OK_NG_OD_3",
                "Set_Shaft_Length", "Set_Freq_Target", "Set_Freq_Offset_Low", "Set_Freq_Offset_Hight",
                "Set_Formula_F", "Set_A", "Set_B", "Set_C", "Set_D",
                "Set_Diam_LL_1", "Set_Diam_LL_2", "Set_Diam_LL_3",
                "Set_Diam_UL_1", "Set_Diam_UL_2", "Set_Diam_UL_3",
                "Set_Tip_OD_Length_1", "Set_Tip_OD_Length_2", "Set_Tip_OD_Length_3",
                "Auto_Manual"
            };

            foreach (var name in tags)
            {
                var tag = _easyDriverConnector.GetTag($"{_basePath}/{name}");
                if (tag != null)
                {
                    tag.ValueChanged += Tag_ValueChanged;
                    tag.QualityChanged += Tag_QualityChanged;

                    // Trigger value change hander first time to sync values
                    Tag_ValueChanged(tag, new TagValueChangedEventArgs(tag, "", tag.Value));
                }
            }
        }

        private void Tag_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            if (e.Tag.Name == "PartName_1")
            {
                bool plcConnected = e.NewQuality == Quality.Good;
                GlobalVariable.SandingRealtime.PlcConnected = plcConnected;

                GlobalVariable.InvokeIfRequired(this, () =>
                {
                    lblPlcStatusVal.Text = plcConnected ? "Connected" : "Disconnected";
                    lblPlcStatusVal.ForeColor = plcConnected ? Color.Lime : Color.Red;
                });
                UpdateFooterStatus();
            }
        }

        private void Tag_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                string name = e.Tag.Name;
                string val = e.NewValue;

                // Sync to SandingRealtimeModel
                UpdateRealtimeModel(name, val);

                // Update UI based on Tag
                GlobalVariable.InvokeIfRequired(this, () =>
                {
                    UpdateUIFromTag(name, val);
                });

                // Process special triggers
                if (name.StartsWith("PartName_") || name.StartsWith("Work_"))
                {
                    ProcessPartWorkChange();
                }
                else if (name == "Trigger_Log_Sanding" && val == "1")
                {
                    _ = ProcessSandingLogAsync();
                }
                else if (name == "Trigger_Log_OD" && val == "1")
                {
                    _ = ProcessOdLogAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Lỗi xử lý Tag_ValueChanged cho Tag {e.Tag.Name}");
            }
        }

        private void UpdateRealtimeModel(string name, string val)
        {
            var model = GlobalVariable.SandingRealtime;
            model.LastUpdated = DateTime.Now;

            switch (name)
            {
                case "Set_Trigger_NewPartInfo1": model.Set_Trigger_NewPartInfo1 = ParseInt(val); break;
                case "Log_Style": model.LogStyle = ParseInt(val); break;
                case "Trigger_Log_Sanding": model.Trigger_Log_Sanding = ParseInt(val); break;
                case "Trigger_Log_OD": model.Trigger_Log_OD = ParseInt(val); break;
                case "Shaft_Num_Sanding": model.Shaft_Num_Sanding = ParseInt(val); break;
                case "Spine_A": model.Spine_A = ParseDouble(val); break;
                case "Mortor_Sanding_Speed": model.Mortor_Sanding_Speed = ParseDouble(val); break;
                case "Spine_B": model.Spine_B = ParseDouble(val); break;
                case "Spine_Target": model.Spine_Target = ParseDouble(val); break;
                case "Spine_Low": model.Spine_Low = ParseDouble(val); break;
                case "Spine_Hight": model.Spine_Hight = ParseDouble(val); break;
                case "OK_NG_Sanding": model.OK_NG_Sanding = ParseInt(val); break;
                case "Shaft_Num_OD": model.Shaft_Num_OD = ParseInt(val); break;
                case "Diam_Reading_1": model.Diam_Reading_1 = ParseDouble(val); break;
                case "Diam_Reading_2": model.Diam_Reading_2 = ParseDouble(val); break;
                case "Diam_Reading_3": model.Diam_Reading_3 = ParseDouble(val); break;
                case "OK_NG_OD_1": model.OK_NG_OD_1 = ParseInt(val); break;
                case "OK_NG_OD_2": model.OK_NG_OD_2 = ParseInt(val); break;
                case "OK_NG_OD_3": model.OK_NG_OD_3 = ParseInt(val); break;
                case "Set_Shaft_Length": model.Set_Shaft_Length = ParseInt(val); break;
                case "Set_Freq_Target": model.Set_Freq_Target = ParseInt(val); break;
                case "Set_Freq_Offset_Low": model.Set_Freq_Offset_Low = ParseInt(val); break;
                case "Set_Freq_Offset_Hight": model.Set_Freq_Offset_Hight = ParseInt(val); break;
                case "Set_Formula_F": model.Set_Formula_F = ParseInt(val); break;
                case "Set_A": model.Set_A = ParseInt(val); break;
                case "Set_B": model.Set_B = ParseInt(val); break;
                case "Set_C": model.Set_C = ParseInt(val); break;
                case "Set_D": model.Set_D = ParseInt(val); break;
                case "Set_Diam_LL_1": model.Set_Diam_LL_1 = ParseInt(val); break;
                case "Set_Diam_LL_2": model.Set_Diam_LL_2 = ParseInt(val); break;
                case "Set_Diam_LL_3": model.Set_Diam_LL_3 = ParseInt(val); break;
                case "Set_Diam_UL_1": model.Set_Diam_UL_1 = ParseInt(val); break;
                case "Set_Diam_UL_2": model.Set_Diam_UL_2 = ParseInt(val); break;
                case "Set_Diam_UL_3": model.Set_Diam_UL_3 = ParseInt(val); break;
                case "Set_Tip_OD_Length_1": model.Set_Tip_OD_Length_1 = ParseDouble(val); break;
                case "Set_Tip_OD_Length_2": model.Set_Tip_OD_Length_2 = ParseDouble(val); break;
                case "Set_Tip_OD_Length_3": model.Set_Tip_OD_Length_3 = ParseDouble(val); break;
                case "Auto_Manual":
                    model.Auto_Manual = ParseInt(val);
                    model.SandingMode = model.Auto_Manual == 2 ? "Test" : "Production";
                    break;
            }
        }

        private void UpdateUIFromTag(string name, string val)
        {
            switch (name)
            {
                case "Log_Style":
                    int style = ParseInt(val);
                    lblLogStyleVal.Text = style == 1 ? "1 (Log All)" : style == 2 ? "2 (Pilot 5)" : style == 3 ? "3 (No Log)" : val;
                    break;
                case "Auto_Manual":
                    int mode = ParseInt(val);
                    lblSandingModeVal.Text = mode == 2 ? "Test (2)" : "Production (1)";
                    break;
                case "Mortor_Sanding_Speed":
                    lblMotorSpeedVal.Text = val;
                    break;
                case "Spine_A":
                    lblSpineAVal.Text = ParseDouble(val).ToString("F3");
                    break;
                case "Spine_B":
                    lblSpineBVal.Text = ParseDouble(val).ToString("F3");
                    break;
                case "Spine_Target":
                    lblSpineTargetVal.Text = ParseDouble(val).ToString("F3");
                    break;
                case "Spine_Low":
                    lblSpineLowVal.Text = ParseDouble(val).ToString("F3");
                    break;
                case "Spine_Hight":
                    lblSpineHighVal.Text = ParseDouble(val).ToString("F3");
                    break;
                case "OK_NG_Sanding":
                    int okNg = ParseInt(val);
                    lblOkNgSandingVal.Text = okNg == 1 ? "OK" : okNg == 2 ? "NG" : "--";
                    lblOkNgSandingVal.ForeColor = okNg == 1 ? Color.Lime : okNg == 2 ? Color.Red : Color.White;
                    break;
                case "Diam_Reading_1":
                case "OK_NG_OD_1":
                    UpdateDiamUI(1);
                    break;
                case "Diam_Reading_2":
                case "OK_NG_OD_2":
                    UpdateDiamUI(2);
                    break;
                case "Diam_Reading_3":
                case "OK_NG_OD_3":
                    UpdateDiamUI(3);
                    break;
            }
        }

        private void UpdateDiamUI(int index)
        {
            var model = GlobalVariable.SandingRealtime;
            double reading = 0;
            int okNg = 0;
            int ll = 0, ul = 0;

            if (index == 1)
            {
                reading = model.Diam_Reading_1;
                okNg = model.OK_NG_OD_1;
                ll = model.Set_Diam_LL_1;
                ul = model.Set_Diam_UL_1;
                lblDiam1.Text = $"OD 1: {reading:F2} mm [LL: {ll} / UL: {ul}]";
                lblDiam1.ForeColor = okNg == 1 ? Color.Lime : okNg == 2 ? Color.Red : Color.White;
            }
            else if (index == 2)
            {
                reading = model.Diam_Reading_2;
                okNg = model.OK_NG_OD_2;
                ll = model.Set_Diam_LL_2;
                ul = model.Set_Diam_UL_2;
                lblDiam2.Text = $"OD 2: {reading:F2} mm [LL: {ll} / UL: {ul}]";
                lblDiam2.ForeColor = okNg == 1 ? Color.Lime : okNg == 2 ? Color.Red : Color.White;
            }
            else if (index == 3)
            {
                reading = model.Diam_Reading_3;
                okNg = model.OK_NG_OD_3;
                ll = model.Set_Diam_LL_3;
                ul = model.Set_Diam_UL_3;
                lblDiam3.Text = $"OD 3: {reading:F2} mm [LL: {ll} / UL: {ul}]";
                lblDiam3.ForeColor = okNg == 1 ? Color.Lime : okNg == 2 ? Color.Red : Color.White;
            }
        }

        private void ProcessPartWorkChange()
        {
            // Gather all registers and decode
            int[] partRegs = new int[10];
            int[] workRegs = new int[10];

            for (int i = 1; i <= 10; i++)
            {
                var tagPart = _easyDriverConnector.GetTag($"{_basePath}/PartName_{i}");
                partRegs[i - 1] = tagPart != null ? ParseInt(tagPart.Value) : 0;

                var tagWork = _easyDriverConnector.GetTag($"{_basePath}/Work_{i}");
                workRegs[i - 1] = tagWork != null ? ParseInt(tagWork.Value) : 0;
            }

            string decodedPartName = DecodeAscii(partRegs);
            string decodedWorkOrder = DecodeAscii(workRegs);

            GlobalVariable.SandingRealtime.PartName = decodedPartName;
            GlobalVariable.SandingRealtime.Work = decodedWorkOrder;

            lblPartNameVal.Text = decodedPartName;
            lblWorkOrderVal.Text = decodedWorkOrder;

            if (decodedPartName != _lastPartName)
            {
                LogEvent($"Phát hiện đổi Part Name: '{_lastPartName}' -> '{decodedPartName}'");
                _lastPartName = decodedPartName;

                if (!string.IsNullOrEmpty(decodedPartName))
                {
                    _ = DownloadConfigFromDbAsync(decodedPartName);
                }
            }

            _lastWorkOrder = decodedWorkOrder;
        }

        private async Task DownloadConfigFromDbAsync(string partName)
        {
            try
            {
                LogEvent($"Đang truy vấn cấu hình cho Part: {partName}...");
                using (var db = new ApplicationDbContext())
                {
                    var config = await db.FT14_TipOdFreqs
                        .FirstOrDefaultAsync(x => x.PartName == partName && x.Actived == true);

                    if (config != null)
                    {
                        LogEvent($"Tìm thấy cấu hình trong FT14. Đang ghi xuống PLC...");
                        await DownloadConfigToPlcAsync(config);
                    }
                    else
                    {
                        LogEvent($"[Cảnh báo]: Không tìm thấy cấu hình hoạt động cho Part: {partName} trong FT14!");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Lỗi khi tải cấu hình cho Part {partName}");
                LogEvent($"Lỗi tải cấu hình: {ex.Message}");
            }
        }

        private async Task DownloadConfigToPlcAsync(FT14_TipOdFreq config)
        {
            try
            {
                // Write each parameter to Set_ tags
                await WriteTagAsync("Set_Shaft_Length", (config.Length ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Freq_Target", (config.FreqTarget ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Freq_Offset_Low", (config.Set_Freq_Offset_Low ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Freq_Offset_Hight", (config.Set_Freq_Offset_Hight ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Formula_F", (config.Formula_F ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                
                await WriteTagAsync("Set_A", (config.A ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_B", (config.B ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_C", (config.C ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_D", (config.D ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));

                await WriteTagAsync("Set_Diam_LL_1", (config.Diam_LL_1 ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Diam_LL_2", (config.Diam_LL_2 ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Diam_LL_3", (config.Diam_LL_3 ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));

                await WriteTagAsync("Set_Diam_UL_1", (config.Diam_UL_1 ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Diam_UL_2", (config.Diam_UL_2 ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Diam_UL_3", (config.Diam_UL_3 ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));

                // Extract numeric values from TipOdLength_1..3 strings
                double len1 = ExtractDouble(config.TipOdLength_1 ?? "");
                double len2 = ExtractDouble(config.TipOdLength_2 ?? "");
                double len3 = ExtractDouble(config.TipOdLength_3 ?? "");

                await WriteTagAsync("Set_Tip_OD_Length_1", len1.ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Tip_OD_Length_2", len2.ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Tip_OD_Length_3", len3.ToString(System.Globalization.CultureInfo.InvariantCulture));

                LogEvent("Ghi cấu hình thành công. Đang gửi tín hiệu Set_Trigger_NewPartInfo1 = 1...");
                await WriteTagAsync("Set_Trigger_NewPartInfo1", "1");
                LogEvent("Gửi tín hiệu NewPartInfo thành công.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi ghi cấu hình xuống PLC.");
                throw;
            }
        }

        private async Task ProcessSandingLogAsync()
        {
            try
            {
                int currentShaft = GlobalVariable.SandingRealtime.Shaft_Num_Sanding;
                string part = GlobalVariable.SandingRealtime.PartName;
                string work = GlobalVariable.SandingRealtime.Work;
                int style = GlobalVariable.SandingRealtime.LogStyle;

                LogEvent($"[Trigger] Nhận tín hiệu Trigger_Log_Sanding = 1 (Shaft: {currentShaft})");

                if (style == 3) // No Log
                {
                    LogEvent($"[Log Mode] Tránh lưu log (No Log). Reset trigger...");
                    await WriteTagAsync("Trigger_Log_Sanding", "0");
                    return;
                }

                using (var db = new ApplicationDbContext())
                {
                    if (style == 2) // Pilot 5
                    {
                        var loggedShafts = await db.FT16_SandingLogDatas
                            .Where(x => x.Part == part && x.Work == work && x.ShaftNum != null)
                            .Select(x => x.ShaftNum.Value)
                            .Distinct()
                            .ToListAsync();

                        if (loggedShafts.Count >= 5 && !loggedShafts.Contains(currentShaft))
                        {
                            LogEvent($"[Pilot 5 Limit] Bỏ qua lưu log cho Shaft {currentShaft} (Đã lưu đủ 5 cây khác).");
                            await WriteTagAsync("Trigger_Log_Sanding", "0");
                            return;
                        }
                    }

                    // Create log row
                    var model = GlobalVariable.SandingRealtime;
                    var logData = new FT16_SandingLogData()
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        CreatedMachine = Environment.MachineName,
                        Part = part,
                        Work = work,
                        Formula = model.Set_Formula_F,
                        LogType = (EnumSandingLogType)style,
                        ShaftNum = currentShaft,
                        MotorSandingSpeed = model.Mortor_Sanding_Speed,
                        SpineA = model.Spine_A,
                        SpineB = model.Spine_B,
                        SpineTarget = model.Spine_Target,
                        Spine_Low = model.Spine_Low,
                        Spine_Hight = model.Spine_Hight,
                        OK_NG_SpineB = model.OK_NG_Sanding,
                        SandingMode = model.Auto_Manual == 2 ? EnumSandingMode.Test : EnumSandingMode.Production
                    };

                    db.FT16_SandingLogDatas.Add(logData);
                    await db.SaveChangesAsync();

                    LogEvent($"[Thành công] Đã lưu dữ liệu mài Sanding cho Shaft {currentShaft}.");
                }

                // Reset trigger
                await WriteTagAsync("Trigger_Log_Sanding", "0");
                LogEvent("Đã reset Trigger_Log_Sanding về 0.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi xử lý lưu Sanding Log.");
                LogEvent($"[Lỗi Datalog Sanding]: {ex.Message}");
            }
        }

        private async Task ProcessOdLogAsync()
        {
            try
            {
                int currentShaft = GlobalVariable.SandingRealtime.Shaft_Num_OD;
                string part = GlobalVariable.SandingRealtime.PartName;
                string work = GlobalVariable.SandingRealtime.Work;
                int style = GlobalVariable.SandingRealtime.LogStyle;

                LogEvent($"[Trigger] Nhận tín hiệu Trigger_Log_OD = 1 (Shaft: {currentShaft})");

                if (style == 3) // No Log
                {
                    LogEvent($"[Log Mode] Tránh lưu log OD (No Log). Reset trigger...");
                    await WriteTagAsync("Trigger_Log_OD", "0");
                    return;
                }

                using (var db = new ApplicationDbContext())
                {
                    if (style == 2) // Pilot 5
                    {
                        var loggedShafts = await db.FT16_SandingLogDatas
                            .Where(x => x.Part == part && x.Work == work && x.ShaftNum != null)
                            .Select(x => x.ShaftNum.Value)
                            .Distinct()
                            .ToListAsync();

                        if (loggedShafts.Count >= 5 && !loggedShafts.Contains(currentShaft))
                        {
                            LogEvent($"[Pilot 5 Limit] Bỏ qua lưu log OD cho Shaft {currentShaft} (Đã lưu đủ 5 cây khác).");
                            await WriteTagAsync("Trigger_Log_OD", "0");
                            return;
                        }
                    }

                    // Find existing log row matching Part, Work and ShaftNum
                    var existingRow = await db.FT16_SandingLogDatas
                        .FirstOrDefaultAsync(x => x.Part == part && x.Work == work && x.ShaftNum == currentShaft);

                    if (existingRow != null)
                    {
                        var model = GlobalVariable.SandingRealtime;
                        existingRow.Diam_Reading_1 = model.Diam_Reading_1;
                        existingRow.Diam_Reading_2 = model.Diam_Reading_2;
                        existingRow.Diam_Reading_3 = model.Diam_Reading_3;
                        existingRow.OK_NG_OD_1 = model.OK_NG_OD_1;
                        existingRow.OK_NG_OD_2 = model.OK_NG_OD_2;
                        existingRow.OK_NG_OD_3 = model.OK_NG_OD_3;

                        // Query master configuration FT14 to copy limit thresholds
                        var config = await db.FT14_TipOdFreqs
                            .FirstOrDefaultAsync(x => x.PartName == part && x.Actived == true);

                        if (config != null)
                        {
                            existingRow.Diam_LL_1 = (int?)config.Diam_LL_1;
                            existingRow.Diam_LL_2 = (int?)config.Diam_LL_2;
                            existingRow.Diam_LL_3 = (int?)config.Diam_LL_3;
                            existingRow.Diam_UL_1 = (int?)config.Diam_UL_1;
                            existingRow.Diam_UL_2 = (int?)config.Diam_UL_2;
                            existingRow.Diam_UL_3 = (int?)config.Diam_UL_3;
                            existingRow.TipOdLength_1 = config.TipOdLength_1;
                            existingRow.TipOdLength_2 = config.TipOdLength_2;
                            existingRow.TipOdLength_3 = config.TipOdLength_3;
                        }

                        await db.SaveChangesAsync();
                        LogEvent($"[Thành công] Đã cập nhật kết quả đo OD cho Shaft {currentShaft}.");
                    }
                    else
                    {
                        LogEvent($"[Cảnh báo] Không tìm thấy dòng Datalog Sanding khớp để cập nhật OD cho Shaft {currentShaft}!");
                    }
                }

                // Reset trigger
                await WriteTagAsync("Trigger_Log_OD", "0");
                LogEvent("Đã reset Trigger_Log_OD về 0.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi xử lý cập nhật OD Log.");
                LogEvent($"[Lỗi Datalog OD]: {ex.Message}");
            }
        }

        private async Task TaskTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Serialize và lưu realtime json vào FT15
                    await LogRealtimeDbAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Lỗi khi chạy TaskTimer Realtime.");
                }

                try
                {
                    await Task.Delay(5000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task LogRealtimeDbAsync()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var dataJson = JsonConvert.SerializeObject(GlobalVariable.SandingRealtime);

                    var record = await db.FT15_SandingRealtimes.FirstOrDefaultAsync();
                    if (record != null)
                    {
                        record.C001_Data = dataJson;
                    }
                    else
                    {
                        var newRecord = new FT15_SandingRealtime()
                        {
                            Id = Guid.NewGuid(),
                            C001_Data = dataJson
                        };
                        db.FT15_SandingRealtimes.Add(newRecord);
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi ghi nhận trạng thái Realtime vào FT15.");
                GlobalVariable.InvokeIfRequired(this, () =>
                {
                    lblFooterStatus.Text = $"EasyDriver: {_easyStatus} | PLC: {(GlobalVariable.SandingRealtime.PlcConnected ? "Connected" : "Disconnected")} | [Lỗi DB]: {ex.Message}";
                });
            }
        }

        private async Task WriteTagAsync(string tagName, string value)
        {
            if (_easyDriverConnector != null && _easyDriverConnector.IsStarted)
            {
                var tag = _easyDriverConnector.GetTag($"{_basePath}/{tagName}");
                if (tag != null)
                {
                    await tag.WriteAsync(value, WritePiority.High);
                }
                else
                {
                    Log.Warning($"Không tìm thấy tag '{tagName}' để ghi.");
                }
            }
        }

        private void UpdateFooterStatus()
        {
            GlobalVariable.InvokeIfRequired(this, () =>
            {
                lblFooterStatus.Text = $"EasyDriver Status: {_easyStatus} | PLC Status: {(GlobalVariable.SandingRealtime.PlcConnected ? "Connected" : "Disconnected")} | DB Server: {GlobalVariable.IpDbServer}";
            });
        }

        private void LogEvent(string message)
        {
            GlobalVariable.InvokeIfRequired(this, () =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                lstEvents.Items.Insert(0, $"[{timestamp}] {message}");
                if (lstEvents.Items.Count > 100)
                {
                    lstEvents.Items.RemoveAt(100);
                }
            });
        }

        private int ParseInt(string val)
        {
            if (int.TryParse(val, out int result)) return result;
            return 0;
        }

        private double ParseDouble(string val)
        {
            if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result)) return result;
            return 0;
        }

        private string DecodeAscii(int[] values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var val in values)
            {
                byte lowByte = (byte)(val & 0xFF);
                byte highByte = (byte)((val >> 8) & 0xFF);
                if (lowByte != 0 && lowByte != 32) sb.Append((char)lowByte);
                if (highByte != 0 && highByte != 32) sb.Append((char)highByte);
            }
            return sb.ToString().Trim();
        }

        private double ExtractDouble(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            var match = Regex.Match(input, @"[0-9]+(\.[0-9]+)?");
            if (match.Success && double.TryParse(match.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
            {
                return val;
            }
            return 0;
        }
    }
}
