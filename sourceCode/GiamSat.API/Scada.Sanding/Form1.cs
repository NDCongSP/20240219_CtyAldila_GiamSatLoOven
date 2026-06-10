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
        private ConnectionStatus _easyStatus = ConnectionStatus.Disconnected;
        private CancellationTokenSource _timerCts;
        private Task _timerTask;
        private System.Windows.Forms.Timer _tagInitTimer;

        private string _lastPartName = string.Empty;
        private string _lastWorkOrder = string.Empty;
        private int _lastAutoManual = -1;

        private readonly string _basePath = "Local Station/Channel1/Device1";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                easyDriverConnector1.ConnectionStatusChaged += EasyDriverConnector_ConnectionStatusChanged;
                easyDriverConnector1.Started += EasyDriverConnector_Started;

                LogEvent("Đang khởi chạy EasyDriverConnector...");
                easyDriverConnector1.Start();

                if (easyDriverConnector1.IsStarted)
                {
                    EasyDriverConnector_Started(null, null);
                }

                _tagInitTimer = new System.Windows.Forms.Timer();
                _tagInitTimer.Interval = 2000;
                _tagInitTimer.Tick += TagInitTimer_Tick;
                _tagInitTimer.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi Form_Load EasyDriver");
                LogEvent("[LỖI] Không thể khởi động EasyDriver: " + ex.Message);
            }
            try
            {
                LogEvent("Đang khởi động ứng dụng...");

                // Đọc chuỗi kết nối từ DBContext
                using (var dbContext = new ApplicationDbContext())
                {
                    var constring = dbContext.Database.Connection.ConnectionString;
                    GlobalVariable.ConnectionString = constring;
                    GlobalVariable.IpDbServer = constring.Split(';')[0].Split('=')[1];

                    var maskedConn = System.Text.RegularExpressions.Regex.Replace(constring, "Password=[^;]+", "Password=******");
                    LogEvent("Chuỗi kết nối DB: " + maskedConn);
                }

                // Kiểm tra kết nối CSDL bất đồng bộ
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var db = new ApplicationDbContext())
                        {
                            var start = DateTime.Now;
                            await db.Database.Connection.OpenAsync();
                            db.Database.Connection.Close();
                            var ms = (DateTime.Now - start).TotalMilliseconds;
                            LogEvent("Kết nối CSDL thành công (" + ms.ToString("F0") + " ms).");
                        }
                    }
                    catch (Exception dbEx)
                    {
                        Log.Error(dbEx, "Lỗi kết nối CSDL khi khởi tạo");
                        LogEvent("[LỖI CSDL] Không thể kết nối tới DB: " + dbEx.Message);
                        if (dbEx.InnerException != null)
                        {
                            LogEvent("[LỖI CSDL chi tiết] " + dbEx.InnerException.Message);
                        }
                    }
                });

                // Cập nhật UI ban đầu
                lblPlcStatusVal.Text = "Disconnected";
                lblPlcStatusVal.ForeColor = Color.Red;

                // Bắt đầu timer lưu realtime 5s
                _timerCts = new CancellationTokenSource();
                _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

                LogEvent("Khởi tạo hệ thống thành công.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi Load Form1.");
                LogEvent("Lỗi khởi động: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _timerCts?.Cancel();
                if (easyDriverConnector1 != null)
                {
                    easyDriverConnector1.ConnectionStatusChaged -= EasyDriverConnector_ConnectionStatusChanged;
                    easyDriverConnector1.Started -= EasyDriverConnector_Started;
                    easyDriverConnector1.Stop();
                }
                LogEvent("Đã dừng kết nối PLC.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi đóng Form1.");
            }
        }

        private bool _tagsRegistered = false;

        private void EasyDriverConnector_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _easyStatus = e.NewStatus;
            bool easyConnected = e.NewStatus == ConnectionStatus.Connected;
            GlobalVariable.SandingRealtime.EasyDriverConnected = easyConnected;
            if (!easyConnected)
            {
                GlobalVariable.SandingRealtime.PlcConnected = false;
                GlobalVariable.InvokeIfRequired(this, () =>
                {
                    lblPlcStatusVal.Text = "Disconnected";
                    lblPlcStatusVal.ForeColor = Color.Red;
                });
            }
            UpdateFooterStatus();
            _ = Task.Run(async () => await LogRealtimeDbAsync());
        }

        private int _timerTicks = 0;
        private void TagInitTimer_Tick(object sender, EventArgs e)
        {
            _timerTicks++;
            if (easyDriverConnector1 != null)
            {
                try
                {
                    var allTags = easyDriverConnector1.GetAllTags();
                    int tagCount = allTags != null ? allTags.Count() : 0;

                    var tag = easyDriverConnector1.GetTag(string.Format("{0}/PartName_1", _basePath));
                    if (tag != null && !_tagsRegistered)
                    {
                        _tagInitTimer.Stop();
                        try
                        {
                            RegisterTagEvents();
                            _tagsRegistered = true;
                            LogEvent("Đăng ký sự kiện Tags thành công.");
                            ProcessPartWorkChange();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Lỗi khi đăng ký Tags.");
                            LogEvent("Lỗi đăng ký Tags: " + ex.Message);
                        }
                    }
                    else if (!_tagsRegistered)
                    {
                        if (tagCount > 0)
                        {
                            var sample = allTags.FirstOrDefault(t => t.Path.Contains("PartName_1")) ?? allTags.FirstOrDefault();
                            LogEvent("Không tìm thấy tag '" + _basePath + "/PartName_1'. Tag gần nhất: " + (sample != null ? sample.Path : "null"));
                            _tagInitTimer.Stop();
                        }
                        else if (_timerTicks >= 10 && _timerTicks % 5 == 0)
                        {
                            LogEvent("Vẫn chưa nhận được danh sách Tags nào từ EasyDriver Server!");
                        }
                    }
                }
                catch (Exception tagEx)
                {
                    Log.Error(tagEx, "Exception khi quét tag");
                }
            }
        }

        private void EasyDriverConnector_Started(object sender, EventArgs e)
        {
            LogEvent("EasyDriver Connector đã Started.");
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
                "Set_Trigger_NewPartInfo", "Log_Style", "Trigger_Log_Sanding", "Trigger_Log_OD",
                "Shaft_Num_Sanding", "Spine_A", "Mortor_Sanding_Speed", "Spine_B", "Spine_Target",
                "Spine_Low", "Spine_Hight", "OK_NG_Sanding", "Shaft_Num_OD",
                "Diam_Reading_1", "Diam_Reading_2", "Diam_Reading_3",
                "OK_NG_OD_1", "OK_NG_OD_2", "OK_NG_OD_3",
                "Set_Shaft_Length", "Set_Freq_Target", "Set_Freq_Target_Low", "Set_Freq_Target_Hight",
                "Set_Formula_F", "Set_A", "Set_B", "Set_C", "Set_D",
                "Set_Diam_LL_1", "Set_Diam_LL_2", "Set_Diam_LL_3",
                "Set_Diam_UL_1", "Set_Diam_UL_2", "Set_Diam_UL_3",
                "Set_Tip_OD_Length_1", "Set_Tip_OD_Length_2", "Set_Tip_OD_Length_3",
                "Auto_Manual", "Set_OD_BOD"
            };

            foreach (var name in tags)
            {
                var tag = easyDriverConnector1.GetTag($"{_basePath}/{name}");
                if (tag != null)
                {
                    tag.ValueChanged += Tag_ValueChanged;
                    tag.QualityChanged += Tag_QualityChanged;

                    // Trigger value change hander first time to sync values
                    Tag_ValueChanged(tag, new TagValueChangedEventArgs(tag, "", tag.Value));
                }
            }

            // Explicitly check and initialize PLC connection status at startup
            var partName1Tag = easyDriverConnector1.GetTag($"{_basePath}/PartName_1");
            if (partName1Tag != null)
            {
                bool plcConnected = partName1Tag.Quality == Quality.Good;
                GlobalVariable.SandingRealtime.PlcConnected = plcConnected;
                GlobalVariable.InvokeIfRequired(this, () =>
                {
                    lblPlcStatusVal.Text = plcConnected ? "Connected" : "Disconnected";
                    lblPlcStatusVal.ForeColor = plcConnected ? Color.ForestGreen : Color.Red;
                });
                UpdateFooterStatus();
            }
        }

        private void Tag_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            if (e.Tag.Name == "PartName_1")
            {
                bool plcConnected = e.NewQuality == Quality.Good;
                GlobalVariable.SandingRealtime.PlcConnected = plcConnected;
                GlobalVariable.SandingRealtime.EasyDriverConnected = (easyDriverConnector1 != null && easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected);

                GlobalVariable.InvokeIfRequired(this, () =>
                {
                    lblPlcStatusVal.Text = plcConnected ? "Connected" : "Disconnected";
                    lblPlcStatusVal.ForeColor = plcConnected ? Color.ForestGreen : Color.Red;
                });
                UpdateFooterStatus();
                _ = Task.Run(async () => await LogRealtimeDbAsync());
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

                if (name.StartsWith("Set_"))
                {
                    return;
                }

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
                case "Set_Trigger_NewPartInfo": model.Set_Trigger_NewPartInfo = ParseInt(val); break;
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
                case "Set_Shaft_Length": model.Set_Shaft_Length = ParseDouble(val); break;
                case "Set_Freq_Target": model.Set_Freq_Target = ParseDouble(val); break;
                case "Set_Freq_Target_Low": model.Set_Freq_Target_Low = ParseDouble(val); break;
                case "Set_Freq_Target_Hight": model.Set_Freq_Target_Hight = ParseDouble(val); break;
                case "Set_Formula_F": model.Set_Formula_F = ParseInt(val); break;
                case "Set_A": model.Set_A = ParseDouble(val); break;
                case "Set_B": model.Set_B = ParseDouble(val); break;
                case "Set_C": model.Set_C = ParseDouble(val); break;
                case "Set_D": model.Set_D = ParseDouble(val); break;
                case "Set_Diam_LL_1": model.Set_Diam_LL_1 = ParseDouble(val); break;
                case "Set_Diam_LL_2": model.Set_Diam_LL_2 = ParseDouble(val); break;
                case "Set_Diam_LL_3": model.Set_Diam_LL_3 = ParseDouble(val); break;
                case "Set_Diam_UL_1": model.Set_Diam_UL_1 = ParseDouble(val); break;
                case "Set_Diam_UL_2": model.Set_Diam_UL_2 = ParseDouble(val); break;
                case "Set_Diam_UL_3": model.Set_Diam_UL_3 = ParseDouble(val); break;
                case "Set_Tip_OD_Length_1": model.Set_Tip_OD_Length_1 = ParseDouble(val); break;
                case "Set_Tip_OD_Length_2": model.Set_Tip_OD_Length_2 = ParseDouble(val); break;
                case "Set_Tip_OD_Length_3": model.Set_Tip_OD_Length_3 = ParseDouble(val); break;
                case "Auto_Manual":
                    model.Auto_Manual = ParseInt(val);
                    model.SandingMode = model.Auto_Manual == 2 ? "Test" : "Production";
                    break;
                case "Set_OD_BOD":
                    model.Set_OD_BOD = ParseDouble(val);
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
                    
                    if (_lastAutoManual != -1 && mode != _lastAutoManual)
                    {
                        LogEvent($"Phát hiện đổi Sanding Mode (Auto_Manual): {_lastAutoManual} -> {mode}. Reset bộ đếm Pilot5.");
                        GlobalVariable.Pilot5SandingCount = 0;
                        GlobalVariable.Pilot5OdCount = 0;
                    }
                    _lastAutoManual = mode;
                    break;
                case "Mortor_Sanding_Speed":
                    lblMotorSpeedVal.Text = val;
                    break;
                case "Spine_A":
                    lblSpineAVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Spine_B":
                    lblSpineBVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Spine_Target":
                    lblSpineTargetVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Spine_Low":
                    lblSpineLowVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Spine_Hight":
                    lblSpineHighVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "OK_NG_Sanding":
                    int okNg = ParseInt(val);
                    lblOkNgSandingVal.Text = okNg == 1 ? "OK" : okNg == 2 ? "NG" : "--";
                    lblOkNgSandingVal.ForeColor = okNg == 1 ? Color.ForestGreen : okNg == 2 ? Color.Red : Color.Black;
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
                case "Shaft_Num_Sanding":
                    lblShaftNumSandingVal.Text = val;
                    break;
                case "Shaft_Num_OD":
                    lblShaftNumOdVal.Text = val;
                    break;
                case "Set_Freq_Target":
                    lblSetFreqTargetVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Freq_Target_Low":
                    lblSetFreqOffsetLowVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Freq_Target_Hight":
                    lblSetFreqOffsetHighVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Formula_F":
                    lblSetFormulaFVal.Text = val;
                    break;
                case "Set_A":
                    lblSetAVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_B":
                    lblSetBVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_C":
                    lblSetCVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_D":
                    lblSetDVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Shaft_Length":
                    lblSetShaftLengthVal.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Tip_OD_Length_1":
                    // Do not update from PLC tag to keep the raw DB string on UI
                    break;
                case "Set_Tip_OD_Length_2":
                    // Do not update from PLC tag to keep the raw DB string on UI
                    break;
                case "Set_Tip_OD_Length_3":
                    // Do not update from PLC tag to keep the raw DB string on UI
                    break;
                case "Set_Diam_LL_1":
                    lblSetDiamLL1Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Diam_LL_2":
                    lblSetDiamLL2Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Diam_LL_3":
                    lblSetDiamLL3Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Diam_UL_1":
                    lblSetDiamUL1Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Diam_UL_2":
                    lblSetDiamUL2Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_Diam_UL_3":
                    lblSetDiamUL3Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "Set_OD_BOD":
                    lblSetOD_BOD_Val.Text = ParseDouble(val).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    break;
            }
        }

        private void UpdateDiamUI(int index)
        {
            var model = GlobalVariable.SandingRealtime;
            double reading = 0;
            int okNg = 0;

            if (index == 1)
            {
                reading = model.Diam_Reading_1;
                okNg = model.OK_NG_OD_1;
                lblDiam1.Text = $"OD 1: {reading} mm";
                lblDiam1.ForeColor = okNg == 1 ? Color.ForestGreen : okNg == 2 ? Color.Red : Color.Black;
                lblOkNgOd1Val.Text = okNg == 1 ? "OK" : okNg == 2 ? "NG" : "--";
                lblOkNgOd1Val.ForeColor = lblDiam1.ForeColor;
            }
            else if (index == 2)
            {
                reading = model.Diam_Reading_2;
                okNg = model.OK_NG_OD_2;
                lblDiam2.Text = $"OD 2: {reading} mm";
                lblDiam2.ForeColor = okNg == 1 ? Color.ForestGreen : okNg == 2 ? Color.Red : Color.Black;
                lblOkNgOd2Val.Text = okNg == 1 ? "OK" : okNg == 2 ? "NG" : "--";
                lblOkNgOd2Val.ForeColor = lblDiam2.ForeColor;
            }
            else if (index == 3)
            {
                reading = model.Diam_Reading_3;
                okNg = model.OK_NG_OD_3;
                lblDiam3.Text = $"OD 3: {reading} mm";
                lblDiam3.ForeColor = okNg == 1 ? Color.ForestGreen : okNg == 2 ? Color.Red : Color.Black;
                lblOkNgOd3Val.Text = okNg == 1 ? "OK" : okNg == 2 ? "NG" : "--";
                lblOkNgOd3Val.ForeColor = lblDiam3.ForeColor;
            }
        }

        private void ProcessPartWorkChange()
        {
            // Gather all registers and decode
            int[] partRegs = new int[10];
            int[] workRegs = new int[10];

            for (int i = 1; i <= 10; i++)
            {
                var tagPart = easyDriverConnector1.GetTag($"{_basePath}/PartName_{i}");
                partRegs[i - 1] = tagPart != null ? ParseInt(tagPart.Value) : 0;

                var tagWork = easyDriverConnector1.GetTag($"{_basePath}/Work_{i}");
                workRegs[i - 1] = tagWork != null ? ParseInt(tagWork.Value) : 0;
            }

            string decodedPartName = DecodeAscii(partRegs);
            string decodedWorkOrder = DecodeAscii(workRegs);

            GlobalVariable.SandingRealtime.PartName = decodedPartName;
            GlobalVariable.SandingRealtime.Work = decodedWorkOrder;

            lblPartNameVal.Text = decodedPartName;
            lblWorkOrderVal.Text = decodedWorkOrder;

            bool changed = false;

            if (decodedPartName != _lastPartName || decodedWorkOrder != _lastWorkOrder)
            {
                if (!string.IsNullOrEmpty(_lastPartName) || !string.IsNullOrEmpty(_lastWorkOrder))
                {
                    GlobalVariable.Pilot5SandingCount = 0;
                    GlobalVariable.Pilot5OdCount = 0;
                    LogEvent($"Đã reset bộ đếm Pilot5 do thay đổi Part/Work.");
                }
            }

            if (decodedPartName != _lastPartName)
            {
                LogEvent($"Phát hiện đổi Part Name: '{_lastPartName}' -> '{decodedPartName}'");
                _lastPartName = decodedPartName;
                changed = true;
            }

            if (decodedWorkOrder != _lastWorkOrder)
            {
                LogEvent($"Phát hiện đổi Work Order: '{_lastWorkOrder}' -> '{decodedWorkOrder}'");
                _lastWorkOrder = decodedWorkOrder;
                changed = true;
            }

            if (changed && !string.IsNullOrEmpty(decodedPartName))
            {
                _ = DownloadConfigFromDbAsync(decodedPartName);
            }
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

                        // Xử lý các lệnh ghi log đang bị kẹt (pending) trong lúc chờ chuỗi Part hoàn chỉnh
                        if (GlobalVariable.SandingRealtime.Trigger_Log_Sanding == 1)
                        {
                            LogEvent($"Phát hiện lệnh ghi log Sanding đang chờ, tiến hành xử lý cho Part hợp lệ...");
                            _ = ProcessSandingLogAsync();
                        }
                        if (GlobalVariable.SandingRealtime.Trigger_Log_OD == 1)
                        {
                            LogEvent($"Phát hiện lệnh ghi log OD đang chờ, tiến hành xử lý cho Part hợp lệ...");
                            _ = ProcessOdLogAsync();
                        }
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
                await WriteTagAsync("Set_Freq_Target_Low", (config.Freq_LL ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Freq_Target_Hight", (config.Freq_UL ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Formula_F", (config.Formula ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_OD_BOD", (config.OD_BOD ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture));

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
                double? len1Opt = ExtractDouble(config.TipOdLength_1 ?? "");
                double? len2Opt = ExtractDouble(config.TipOdLength_2 ?? "");
                double? len3Opt = ExtractDouble(config.TipOdLength_3 ?? "");

                // Display raw strings directly on the UI
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => {
                        lblSetTipOdLength1Val.Text = string.IsNullOrEmpty(config.TipOdLength_1) ? "--" : config.TipOdLength_1;
                        lblSetTipOdLength2Val.Text = string.IsNullOrEmpty(config.TipOdLength_2) ? "--" : config.TipOdLength_2;
                        lblSetTipOdLength3Val.Text = string.IsNullOrEmpty(config.TipOdLength_3) ? "--" : config.TipOdLength_3;
                    }));
                }
                else
                {
                    lblSetTipOdLength1Val.Text = string.IsNullOrEmpty(config.TipOdLength_1) ? "--" : config.TipOdLength_1;
                    lblSetTipOdLength2Val.Text = string.IsNullOrEmpty(config.TipOdLength_2) ? "--" : config.TipOdLength_2;
                    lblSetTipOdLength3Val.Text = string.IsNullOrEmpty(config.TipOdLength_3) ? "--" : config.TipOdLength_3;
                }

                bool hasFormatError = false;
                if (!string.IsNullOrEmpty(config.TipOdLength_1) && len1Opt == null) hasFormatError = true;
                if (!string.IsNullOrEmpty(config.TipOdLength_2) && len2Opt == null) hasFormatError = true;
                if (!string.IsNullOrEmpty(config.TipOdLength_3) && len3Opt == null) hasFormatError = true;

                double len1 = len1Opt ?? 0;
                double len2 = len2Opt ?? 0;
                double len3 = len3Opt ?? 0;

                await WriteTagAsync("Set_Tip_OD_Length_1", len1.ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Tip_OD_Length_2", len2.ToString(System.Globalization.CultureInfo.InvariantCulture));
                await WriteTagAsync("Set_Tip_OD_Length_3", len3.ToString(System.Globalization.CultureInfo.InvariantCulture));

                if (hasFormatError)
                {
                    LogEvent("Phát hiện định dạng Tip OD Length không hợp lệ! Kích hoạt Set_Alarm = 1.");
                    await WriteTagAsync("Set_Alarm", "1");
                    GlobalVariable.SandingRealtime.Set_Alarm = 1;
                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        lblFooterStatus.Text += " | [CẢNH BÁO]: Format Tip OD Length sai!";
                        lblFooterStatus.ForeColor = Color.Red;
                        MessageBox.Show("Cảnh báo: Định dạng Tip OD Length 1, 2, 3 không đúng (không tìm thấy số)!", "Cảnh Báo Định Dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }
                else
                {
                    await WriteTagAsync("Set_Alarm", "0");
                    GlobalVariable.SandingRealtime.Set_Alarm = 0;
                }

                LogEvent("Ghi cấu hình thành công. Đang gửi tín hiệu Set_Trigger_NewPartInfo = 1...");
                await WriteTagAsync("Set_Trigger_NewPartInfo", "1");
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
                        if (GlobalVariable.Pilot5SandingCount >= 5)
                        {
                            LogEvent($"[Pilot 5 Limit] Bỏ qua lưu log cho Shaft {currentShaft} (Đã lưu đủ 5 dòng mài).");
                            await WriteTagAsync("Trigger_Log_Sanding", "0");
                            return;
                        }
                    }

                    var model = GlobalVariable.SandingRealtime;
                    int sandingMode = model.Auto_Manual;
                    if (sandingMode != 1 && sandingMode != 2)
                    {
                        LogEvent($"[Bỏ qua] Không ghi log Sanding vì Sanding Mode (Auto_Manual) = {sandingMode} (yêu cầu 1 hoặc 2).");
                        await WriteTagAsync("Trigger_Log_Sanding", "0");
                        return;
                    }

                    if (string.IsNullOrEmpty(part) || string.IsNullOrEmpty(work) || currentShaft <= 0)
                    {
                        LogEvent($"[Bỏ qua] Thiếu thông tin Part ({part}), Work ({work}) hoặc Shaft ({currentShaft}).");
                        await WriteTagAsync("Trigger_Log_Sanding", "0");
                        return;
                    }

                    bool exists = await db.FT16_SandingLogDatas.AnyAsync(x => x.Part == part && x.Work == work && x.ShaftNum == currentShaft);
                    if (exists)
                    {
                        LogEvent($"[Bỏ qua] Dữ liệu Sanding cho Part: {part}, Work: {work}, Shaft: {currentShaft} đã tồn tại (đảm bảo Unique).");
                        await WriteTagAsync("Trigger_Log_Sanding", "0");
                        return;
                    }

                    // Create log row
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

                    if (style == 2)
                    {
                        GlobalVariable.Pilot5SandingCount++;
                        LogEvent($"[Pilot 5] Đã lưu log mài thứ {GlobalVariable.Pilot5SandingCount}/5 cho Shaft {currentShaft}.");
                    }
                    else
                    {
                        LogEvent($"[Thành công] Đã lưu dữ liệu mài Sanding cho Shaft {currentShaft}.");
                    }
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
                        if (GlobalVariable.Pilot5OdCount >= 5)
                        {
                            LogEvent($"[Pilot 5 Limit] Bỏ qua lưu log OD cho Shaft {currentShaft} (Đã lưu đủ 5 dòng OD).");
                            await WriteTagAsync("Trigger_Log_OD", "0");
                            return;
                        }
                    }

                    var model = GlobalVariable.SandingRealtime;
                    int sandingMode = model.Auto_Manual;
                    if (sandingMode != 1 && sandingMode != 2)
                    {
                        LogEvent($"[Bỏ qua] Không ghi log OD vì Sanding Mode (Auto_Manual) = {sandingMode} (yêu cầu 1 hoặc 2).");
                        await WriteTagAsync("Trigger_Log_OD", "0");
                        return;
                    }

                    if (string.IsNullOrEmpty(part) || string.IsNullOrEmpty(work) || currentShaft <= 0)
                    {
                        LogEvent($"[Bỏ qua] Thiếu thông tin Part ({part}), Work ({work}) hoặc Shaft ({currentShaft}).");
                        await WriteTagAsync("Trigger_Log_OD", "0");
                        return;
                    }

                    // Find existing log row matching Part, Work and ShaftNum that hasn't been updated with OD yet
                    var existingRow = await db.FT16_SandingLogDatas
                        .OrderBy(x => x.CreatedAt)
                        .FirstOrDefaultAsync(x => x.Part == part && x.Work == work && x.ShaftNum == currentShaft && x.OK_NG_OD_1 == null);

                    if (existingRow != null)
                    {
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
                        if (style == 2)
                        {
                            GlobalVariable.Pilot5OdCount++;
                            LogEvent($"[Pilot 5] Đã cập nhật kết quả đo OD thứ {GlobalVariable.Pilot5OdCount}/5 cho Shaft {currentShaft}.");
                        }
                        else
                        {
                            LogEvent($"[Thành công] Đã cập nhật kết quả đo OD cho Shaft {currentShaft}.");
                        }
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
                var model = GlobalVariable.SandingRealtime;
                model.EasyDriverConnected = (easyDriverConnector1 != null && easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected);
                if (!model.EasyDriverConnected)
                {
                    model.PlcConnected = false;
                }
                else
                {
                    var partName1Tag = easyDriverConnector1.GetTag($"{_basePath}/PartName_1");
                    model.PlcConnected = partName1Tag != null && partName1Tag.Quality == Quality.Good;
                }

                UpdateFooterStatus();

                using (var db = new ApplicationDbContext())
                {
                    var dataJson = JsonConvert.SerializeObject(model);

                    // Lấy và xóa tất cả dòng cũ để đảm bảo bảng chỉ có duy nhất 1 item
                    var oldRecords = await db.FT15_SandingRealtimes.ToListAsync();
                    if (oldRecords.Any())
                    {
                        db.FT15_SandingRealtimes.RemoveRange(oldRecords);
                    }

                    // Thêm 1 dòng mới
                    var newRecord = new FT15_SandingRealtime()
                    {
                        Id = Guid.NewGuid(),
                        C001_Data = dataJson
                    };
                    db.FT15_SandingRealtimes.Add(newRecord);

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
            if (easyDriverConnector1 != null && easyDriverConnector1.IsStarted)
            {
                var tag = easyDriverConnector1.GetTag($"{_basePath}/{tagName}");
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
                bool plcConnected = GlobalVariable.SandingRealtime.PlcConnected;
                lblPlcStatusVal.Text = plcConnected ? "Connected" : "Disconnected";
                lblPlcStatusVal.ForeColor = plcConnected ? Color.ForestGreen : Color.Red;
                lblFooterStatus.Text = $"EasyDriver Status: {_easyStatus} | PLC Status: {(plcConnected ? "Connected" : "Disconnected")} | DB Server: {GlobalVariable.IpDbServer}";
            });
        }

        private void LogEvent(string message)
        {
            Log.Information($"[SCADA] {message}");
            System.Diagnostics.Debug.WriteLine($"[SCADA] {message}");
        }

        private int ParseInt(string val)
        {
            if (int.TryParse(val, out int result)) return result;
            return 0;
        }

        private double ParseDouble(string val)
        {
            if (string.IsNullOrEmpty(val)) return 0;
            val = val.Replace(',', '.');
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

                if (lowByte != 0) sb.Append((char)lowByte);
                if (highByte != 0) sb.Append((char)highByte);
            }
            return sb.ToString().Trim();
        }

        private double? ExtractDouble(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            // Find number immediately after @ character
            var matchAt = Regex.Match(input, @"@\s*([0-9]+(\.[0-9]+)?)");
            if (matchAt.Success && matchAt.Groups.Count > 1)
            {
                if (double.TryParse(matchAt.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                {
                    return val;
                }
            }

            // Fallback to finding the first number if @ is not present
            var match = Regex.Match(input, @"[0-9]+(\.[0-9]+)?");
            if (match.Success && double.TryParse(match.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val2))
            {
                return val2;
            }

            return null;
        }

        private void lblSetFreqOffsetLow_Click(object sender, EventArgs e)
        {

        }
    }
}
