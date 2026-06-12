using GiamSat.Models;
using McProtocolClientLib.Core;
using McProtocolClientLib.Subscription;
using McProtocolClientLib.Tags;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada.TrackingTime_AutoRolling1
{
    /// <summary>
    /// Form sản xuất Auto Rolling — kết nối PLC Mitsubishi qua MC Protocol (McProtocolClientLib).
    /// </summary>
    /// <remarks>
    /// File:     Scada.TrackingTime_AutoRolling1/frmProduction.cs
    /// Author:   Cong.Nguyen
    /// Created:  2024-02-19
    /// Modified: 2026-06-11 — Thay EasyDriverConnector bằng PlcManager + PlcSubscriptionManager (MC Protocol)
    /// </remarks>
    public partial class frmProduction : Form
    {
        // ─── MC Protocol ─────────────────────────────────────────────────────────
        private readonly PlcManager _plcManager = new PlcManager();

        /// <summary>plcName → (runtime, subscription)</summary>
        private readonly Dictionary<string, (PlcRuntime Runtime, PlcSubscriptionManager Sub)> _sessions
            = new Dictionary<string, (PlcRuntime, PlcSubscriptionManager)>();

        // tags.json Name = "Auto_Rolling_1/2/3" được chuẩn hóa khớp với RevoConfig.Path
        // bằng số máy (1/2/3) ở bước 3b trong frmProduction_LoadAsync.

        // ─── Timers / Tasks ───────────────────────────────────────────────────────
        private CancellationTokenSource _timerCts;
        private Task _timerTask;

        private CancellationTokenSource _checkingTimeStepCts;
        private readonly AsyncAutoResetEvent _triggerCheckTimeStep = new AsyncAutoResetEvent();

        private CancellationTokenSource _resetShaftCts;
        private readonly AsyncAutoResetEvent _triggerResetShaft = new AsyncAutoResetEvent();

        private CancellationTokenSource _resetPartCts;
        private readonly AsyncAutoResetEvent _triggerResetPart = new AsyncAutoResetEvent();

        private int _totalCurrentHour = 0, _totalLastHour = 0;

        private List<AutoRollingTagChangedModel> _tagsValueRealtime = new List<AutoRollingTagChangedModel>();

        /// <summary>
        /// Lý do kích hoạt TaskResetShaftAsync.
        /// - PartChanged: chỉ log (KHÔNG đổi ShaftNum, KHÔNG reset TotalRunTime).
        /// - StepRunZero: reset cây shaft mới (đổi ShaftNum + reset TotalRunTime) rồi log.
        /// </summary>
        private enum ShaftActionReason
        {
            PartChanged,
            StepRunZero
        }

        // Dùng ConcurrentDictionary thay vì Queue để mỗi Path chỉ có 1 entry duy nhất.
        private readonly ConcurrentDictionary<string, ShaftActionReason> _resetShaftQueue
            = new ConcurrentDictionary<string, ShaftActionReason>();
        private readonly ConcurrentQueue<string> _resetPartQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Helper: enqueue Path vào queue reset shaft, đảm bảo unique.
        /// </summary>
        private void EnqueueResetShaft(string path, ShaftActionReason reason)
        {
            _resetShaftQueue[path] = reason;
            _triggerResetShaft.Set();
        }

        /// <summary>
        /// Lưu thời điểm reset shaft gần nhất theo Path để debounce cross-batch.
        /// </summary>
        private readonly Dictionary<string, DateTime> _lastShaftResetAt = new Dictionary<string, DateTime>();
        private static readonly TimeSpan SHAFT_RESET_DEBOUNCE = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Lock để serialize UpdateShaftTimesAsync — tránh gọi đồng thời từ timer.
        /// </summary>
        private readonly SemaphoreSlim _logStepRunLock = new SemaphoreSlim(1, 1);

        private bool _loaded = false;

        // Import để cho phép kéo form
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private Panel titleBar;
        private Button btnClose;
        private Button btnMaximize;
        private Button btnMinimize;
        private Button btnMaintenance;
        private Label titleText;

        public frmProduction()
        {
            InitializeComponent();

            #region add header
            this.Text = "Custom Title Bar";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1350, 514);

            titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 40;
            titleBar.BackColor = Color.Black;
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            // Nút Maximize
            btnMaximize = new Button();
            btnMaximize.Text = "";
            btnMaximize.ForeColor = Color.White;
            btnMaximize.BackColor = Color.Black;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.Size = new Size(40, 40);
            btnMaximize.Location = new Point(this.Width - 40, 0);
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.Image = Properties.Resources.maximize_30_white;
            btnMaximize.ImageAlign = ContentAlignment.MiddleCenter;
            btnMaximize.Padding = new Padding(0);
            btnMaximize.TextImageRelation = TextImageRelation.Overlay;
            btnMaximize.Click += BtnMaximize_Click;
            titleBar.Controls.Add(btnMaximize);

            // Nút Minimize
            btnMinimize = new Button();
            btnMinimize.Text = "";
            btnMinimize.ForeColor = Color.White;
            btnMinimize.BackColor = Color.Black;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Size = new Size(40, 40);
            btnMinimize.Location = new Point(this.Width - 80, 0);
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.Image = Properties.Resources.minimize_30_White;
            btnMinimize.ImageAlign = ContentAlignment.MiddleCenter;
            btnMinimize.Padding = new Padding(0);
            btnMinimize.TextImageRelation = TextImageRelation.Overlay;
            btnMinimize.Click += BtnMinimize_Click;
            titleBar.Controls.Add(btnMinimize);

            // Nút Maintenance
            btnMaintenance = new Button();
            btnMaintenance.Text = "";
            btnMaintenance.ForeColor = Color.White;
            btnMaintenance.BackColor = Color.Black;
            btnMaintenance.FlatStyle = FlatStyle.Flat;
            btnMaintenance.FlatAppearance.BorderSize = 0;
            btnMaintenance.Size = new Size(40, 40);
            btnMaintenance.Location = new Point(this.Width - 120, 0);
            btnMaintenance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaintenance.Cursor = Cursors.Hand;
            btnMaintenance.Image = Properties.Resources.maintenance_30_white;
            btnMaintenance.ImageAlign = ContentAlignment.MiddleCenter;
            btnMaintenance.Padding = new Padding(0);
            btnMaintenance.TextImageRelation = TextImageRelation.Overlay;

            var tip = new ToolTip();
            tip.AutoPopDelay = 5000;
            tip.InitialDelay = 300;
            tip.ReshowDelay = 100;
            tip.ShowAlways = true;
            tip.SetToolTip(btnMaintenance, "MAINTENANCE");

            btnMaintenance.MouseEnter += (s, e) => btnMaintenance.BackColor = Color.FromArgb(30, 30, 30);
            btnMaintenance.MouseLeave += (s, e) => btnMaintenance.BackColor = Color.Black;
            btnMaintenance.Click += btnMaitenance_Click;
            titleBar.Controls.Add(btnMaintenance);

            btnMaximize.Size = btnMinimize.Size = btnMaintenance.Size = new Size(30, 30);
            btnMaximize.Anchor = btnMinimize.Anchor = btnMaintenance.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Logo
            PictureBox logo = new PictureBox();
            logo.Image = Properties.Resources.logoAldila;
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.Size = new Size(100, 30);
            logo.Location = new Point(0, 5);
            titleBar.Controls.Add(logo);

            // Title text
            titleText = new Label();
            titleText.Text = $"AUTO ROLLING PRODUCTION";
            titleText.ForeColor = Color.White;
            titleText.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleText.AutoSize = true;
            titleText.Location = new Point(100, 10);
            titleBar.Controls.Add(titleText);
            #endregion

            _labStatus.Dock = DockStyle.Bottom;
            _labStatus.Height = 30;
            _labStatus.BringToFront();

            Load += frmProduction_Load;
        }

        private void frmProduction_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                // Dừng subscription và ngắt kết nối tất cả PLC
                // NOTE: foreach (var (_, session) in _sessions) không compile trên .NET 4.8
                //       vì KeyValuePair không có Deconstruct method — dùng kv.Value thay thế.
                foreach (var kv in _sessions)
                {
                    try
                    {
                        kv.Value.Sub.Stop();
                        kv.Value.Runtime.Client.Dispose();
                    }
                    catch { /* ignore on close */ }
                }

                _timerCts?.Cancel();
                _timerTask?.Wait(1000);

                _checkingTimeStepCts?.Cancel();
                _resetShaftCts?.Cancel();
                _resetPartCts?.Cancel();
            }
            catch { }
            finally
            {
                _timerCts?.Dispose();
                _timerTask = null;

                _checkingTimeStepCts?.Dispose();
                _resetShaftCts?.Dispose();
                _resetPartCts?.Dispose();
            }
        }

        private async void frmProduction_Load(object? sender, EventArgs e)
        {
            try
            {
                await frmProduction_LoadAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "frmProduction_Load unhandled");
                MessageBox.Show($"Lỗi khởi động chương trình:\n{ex.Message}",
                    "LỖI KHỞI ĐỘNG", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task frmProduction_LoadAsync()
        {
            // 1. Đọc config từ DB
            using (var dbContext = new ApplicationDbContext())
            {
                var constring = dbContext.Database.Connection.ConnectionString;
                GlobalVariable.IpDbServer = constring.Split(';')[0].Split('=')[1];

                var ft07 = await dbContext.FT07_RevoConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft07 != null)
                {
                    GlobalVariable.RevoConfigs = JsonConvert
                        .DeserializeObject<List<RevoConfigModel>>(ft07.C000)
                        .Where(x => x.Name.Contains("Auto Rolling"))
                        .ToList();

                    if (GlobalVariable.RevoConfigs.Count == 0)
                    {
                        MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                            "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Text = $"Chương trình giám sát thời gian chạy - Máy Auto Rolling";
                }
                else
                {
                    MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                        "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Khởi tạo model realtime
                foreach (var item in GlobalVariable.RevoConfigs)
                {
                    // FIX: tạo list Steps RIÊNG cho từng RevoRealtimeModel
                    var stepsForThisRevo = new List<RevoStep>();
                    for (int i = 1; i <= 15; i++)
                    {
                        stepsForThisRevo.Add(new RevoStep()
                        {
                            StepIndex = i,
                            StepName = $"Step {i}",
                            Visible = true,
                            Enable = false
                        });
                    }

                    GlobalVariable.RevoRealtimeModels.Add(new RevoRealtimeModel()
                    {
                        RevoId = item.Id.Value,
                        RevoName = item.Name,
                        Path = item.Path,
                        Steps = stepsForThisRevo,
                        // ShaftNum mặc định Guid.Empty — TaskResetShaftAsync sẽ gán từ event PLC
                    });

                    _tagsValueRealtime.Add(new AutoRollingTagChangedModel()
                    {
                        RevoId = item.Id.Value,
                        RevoName = item.Name,
                        Path = item.Path,
                    });
                }
            }

            // 3. Khởi tạo MC Protocol — load tags.json
            try
            {
                _plcManager.LoadFromConfig("tags.json");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không đọc được tags.json:\n{ex.Message}",
                    "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3b. Chuẩn hóa Path trong models để khớp với Name trong tags.json
            //     (RevoConfig.Path trong DB có thể khác format, vd: "Auto Rolling 1" vs "Auto_Rolling_1"
            //      hoặc "Local Station/Channel_Auto_Rolling_1/Device1" tùy cấu hình EasyDriver cũ)
            //     → Dùng số máy (1/2/3) ở cuối tên để khớp, sau đó ghi đè Path = plcName.
            foreach (var plcNameNorm in _plcManager.GetAllPlcNames())
            {
                var normParts = plcNameNorm.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                var machineNum = normParts.Length > 0 ? normParts[normParts.Length - 1] : "";

                int dummy;
                if (string.IsNullOrEmpty(machineNum) || !int.TryParse(machineNum, out dummy)) continue;

                // Cập nhật RevoRealtimeModel.Path
                foreach (var rm in GlobalVariable.RevoRealtimeModels)
                {
                    var nameParts = (rm.RevoName ?? "").Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length > 0 && nameParts[nameParts.Length - 1] == machineNum)
                    {
                        rm.Path = plcNameNorm;
                        break;
                    }
                }

                // Cập nhật AutoRollingTagChangedModel.Path
                foreach (var tm in _tagsValueRealtime)
                {
                    var nameParts = (tm.RevoName ?? "").Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length > 0 && nameParts[nameParts.Length - 1] == machineNum)
                    {
                        tm.Path = plcNameNorm;
                        break;
                    }
                }
            }

            // 4. Kết nối tất cả PLC song song
            //    plcName trong tags.json ("Auto_Rolling_1"...) = RevoConfig.Path (sau khi chuẩn hóa ở 3b).
            var plcNames = _plcManager.GetAllPlcNames().ToList();
            var connectTasks = plcNames
                .Select(async plcName =>
            {
                var runtime = _plcManager.GetPlc(plcName);
                var path = plcName; // Path đã được chuẩn hóa = plcName ở bước 3b

                // StateChanged → cập nhật PlcConnected trong model
                runtime.Client.StateChanged += state =>
                {
                    bool connected = (state == PlcConnectionState.Connected);
                    var tagItem = _tagsValueRealtime.FirstOrDefault(x => x.Path == path);
                    if (tagItem != null) tagItem.PlcConnected = connected;

                    var realtimeModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.Path == path);
                    if (realtimeModel != null) realtimeModel.PlcConnected = connected;

                    Log.Information($"[{plcName}] PLC state → {state}");
                };

                runtime.Reader.OnReadError += msg => Log.Warning($"[{plcName}] ReadError: {msg}");
                runtime.Writer.OnWriteError += msg => Log.Warning($"[{plcName}] WriteError: {msg}");

                // Subscription
                var sub = new PlcSubscriptionManager(runtime.Reader);
                sub.OnValueChanged += tag => Plc_OnValueChanged(plcName, tag);
                _sessions[plcName] = (runtime, sub);

                bool isConnected = false;
                try { isConnected = await runtime.Client.ConnectAsync(); }
                catch (Exception ex) { Log.Error(ex, $"[{plcName}] Connect error"); }

                // Watchdog ICMP — không chiếm connection slot PLC (Q series giới hạn ~8 slots)
                runtime.Client.StartWatchdog(10000);

                if (isConnected)
                {
                    // Đọc giá trị khởi tạo và fire event ngay lập tức (như _easyDriverConnector_Started)
                    try
                    {
                        await runtime.Reader.ReadGroupAsync(runtime.Tags);
                        foreach (var tag in runtime.Tags)
                        {
                            Plc_OnValueChanged(plcName, tag);
                        }
                    }
                    catch (Exception ex) { Log.Error(ex, $"[{plcName}] Initial read error"); }
                }

                sub.Subscribe(runtime.Tags, intervalMs: 200);
            }).ToList();

            await Task.WhenAll(connectTasks);

            // 5b. Fire lại TimeRunStep tags sau khi TẤT CẢ PLC đã connect xong.
            //     Lý do: trong initial fire ở bước 5 (bên trong connectTasks), các tag được fire
            //     theo thứ tự tags.json → TimeRunStep1-15 fire TRƯỚC RecipeSettingStep
            //     → step.Enable chưa được set → HandleTimeRunStep cập nhật TotalRunTime nhưng
            //     step chưa enable. Sau Task.WhenAll, RecipeSettingStep đã được xử lý xong
            //     → fire lại TimeRunStep để model có giá trị đúng ngay khi form mở.
            foreach (var kv in _sessions)
            {
                var plcName2 = kv.Key;
                var runtime2 = kv.Value.Runtime;
                foreach (var tag in runtime2.Tags)
                {
                    if (tag.Name.StartsWith("TimeRunStep") && tag.NewValue != null)
                        Plc_OnValueChanged(plcName2, tag);
                }
            }

            // 6. Khởi động timer và task reset shaft
            _timerCts = new CancellationTokenSource();
            _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

            _resetShaftCts = new CancellationTokenSource();
            _ = TaskResetShaftAsync(_resetShaftCts.Token);
        }

        #region Tasks
        /// <summary>
        /// Task chạy vòng lặp 200ms: cập nhật UI status bar, log realtime (FT08), log step run (FT09).
        /// </summary>
        private async Task TaskTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var totalShaft = await GetTotalShaftAsync();
                    _totalCurrentHour = totalShaft?.CurrentHour ?? 0;
                    _totalLastHour = totalShaft?.LastHour ?? 0;

                    // Tổng hợp trạng thái từng PLC
                    var plcStatus = string.Join(" | ",
                        _sessions.Select(kv => $"{kv.Key}:{kv.Value.Runtime.Client.State}"));

                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        _labStatus.Text = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {plcStatus} - DB Server:";
                    });

                    await LogDataRealtime();

                    // Cập nhật TotalTime FT09 — chỉ UPDATE, không INSERT (INSERT do InsertShaftAsync đảm nhận)
                    await UpdateShaftTimesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "TaskTimerAsync");
                }
                await Task.Delay(200, token);
            }
        }

        /// <summary>
        /// Task xử lý reset shaft khi có sự kiện Part đổi hoặc Step_Run = 0.
        /// </summary>
        private async Task TaskResetShaftAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await _triggerResetShaft.WaitAsync(token);

                    Debug.WriteLine($"TaskResetShaftAsync triggered.");

                    var snapshot = _resetShaftQueue.ToArray();
                    foreach (var kv in snapshot)
                        _resetShaftQueue.TryRemove(kv.Key, out _);

                    foreach (var kv in snapshot)
                    {
                        var currentPath = kv.Key;
                        var reason = kv.Value;

                        if (_lastShaftResetAt.TryGetValue(currentPath, out var lastAt)
                            && (DateTime.Now - lastAt) < SHAFT_RESET_DEBOUNCE)
                        {
                            Debug.WriteLine($"Skip {reason} for {currentPath} (debounced, last reset {lastAt:HH:mm:ss.fff})");
                            continue;
                        }

                        var targetItem = _tagsValueRealtime.FirstOrDefault(x => x.Path == currentPath);
                        if (targetItem == null) continue;

                        var realtimeItem = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.Path == currentPath);
                        if (realtimeItem == null) continue;

                        Debug.WriteLine($"Processing {reason} for Path: {currentPath}");

                        bool isFirstShaft = !realtimeItem.ShaftNum.HasValue
                                            || realtimeItem.ShaftNum.Value == Guid.Empty;

                        if (!isFirstShaft)
                        {
                            // Reset bình thường (shaft đang chạy kết thúc):
                            // flush giá trị cuối rồi reset về 0 cho shaft mới.
                            await FlushShaftTimesAsync(realtimeItem);

                            foreach (var step in realtimeItem.Steps)
                                step.TotalRunTime = 0;

                            if (targetItem != null)
                            {
                                targetItem.TimeRunStep1 = targetItem.TimeRunStep2 = targetItem.TimeRunStep3 =
                                targetItem.TimeRunStep4 = targetItem.TimeRunStep5 = targetItem.TimeRunStep6 =
                                targetItem.TimeRunStep7 = targetItem.TimeRunStep8 = targetItem.TimeRunStep9 =
                                targetItem.TimeRunStep10 = targetItem.TimeRunStep11 = targetItem.TimeRunStep12 =
                                targetItem.TimeRunStep13 = targetItem.TimeRunStep14 = targetItem.TimeRunStep15 = 0;
                            }
                        }
                        // else: shaft đầu tiên khi app vừa mở — GIỮ NGUYÊN TotalRunTime
                        // (đã được đọc từ PLC qua initial ReadGroupAsync + second-pass fire).
                        // InsertShaftAsync sẽ lưu đúng giá trị hiện tại thay vì 0.

                        realtimeItem.ShaftNum = Guid.NewGuid();
                        _lastShaftResetAt[currentPath] = DateTime.Now;

                        await InsertShaftAsync(realtimeItem);
                    }
                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
        }
        #endregion

        #region MC PROTOCOL EVENTS

        /// <summary>
        /// Dispatcher chính: nhận OnValueChanged từ PlcSubscriptionManager và chuyển tới handler tương ứng.
        /// Tương đương nhóm EASY DRIVER EVENTS cũ: Part_Code_ValueChanged, Recipe_Settings_ValueChanged,
        /// Step_Run_ValueChanged, TimeRunStep_ValueChanged, Part_Name_ValueChanged.
        /// </summary>
        private void Plc_OnValueChanged(string plcName, PlcTag tag)
        {
            try
            {
                // plcName == Path trực tiếp (tags.json Name = RevoConfig.Path)
                var path = plcName;

                string tagName = tag.Name;
                string newValueStr = Convert.ToString(tag.NewValue) ?? "";

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path != path) continue;

                    switch (tagName)
                    {
                        case "Part":
                            // Tương đương Part_Code_ValueChanged
                            item.Part_Code = newValueStr;
                            UpdateGrid();
                            break;

                        case "RecipeSettingStep":
                            // Tương đương Recipe_Settings_ValueChanged
                            item.Recipe_Settings = int.TryParse(newValueStr, out int rval) ? rval : 0;
                            item.StepsIsRun = ConvertWordToBitArray((ushort)item.Recipe_Settings);

                            var revoModelR = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);
                            if (revoModelR != null)
                            {
                                for (int i = 1; i <= 15; i++)
                                {
                                    var step = revoModelR.Steps.FirstOrDefault(s => s.StepIndex == i);
                                    if (step == null)
                                    {
                                        step = new RevoStep { StepIndex = i };
                                        revoModelR.Steps.Add(step);
                                    }
                                    step.Enable = item.StepsIsRun[i];
                                }
                            }
                            UpdateGrid();
                            break;

                        case "StepRun":
                            // Tương đương Step_Run_ValueChanged
                            item.StepRun = int.TryParse(newValueStr, out int sval) ? sval : 0;

                            if (item.StepRun == 0)
                            {
                                // StepRun về 0 → kết thúc shaft cũ, sang cây shaft mới.
                                // CHÚ Ý: KHÔNG dùng (== 0 || == Recipe_Settings) — PLC sẽ đi qua Recipe_Settings
                                // rồi về 0 trong cùng một chu kỳ, nếu cả hai đều trigger sẽ tạo 2 ShaftNum
                                // cho cùng 1 lần reset (double-fire).
                                EnqueueResetShaft(item.Path, ShaftActionReason.StepRunZero);
                            }
                            break;

                        case "PartName":
                            // Thay thế Part_Name1-8: MC Protocol String type trả về string trực tiếp,
                            // không cần decode từng word register như EasyDriver cũ.
                            item.Part_Name = (newValueStr ?? "").Trim('\0').Trim();

                            var revoModelP = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);
                            if (revoModelP != null)
                                revoModelP.Part = item.Part_Name;

                            // Đổi part → reset shaft mới
                            EnqueueResetShaft(item.Path, ShaftActionReason.PartChanged);
                            UpdateGrid();
                            break;

                        default:
                            // TimeRunStep1 → TimeRunStep15
                            if (tagName.StartsWith("TimeRunStep"))
                            {
                                HandleTimeRunStep(item, tagName, newValueStr);
                            }
                            break;
                    }
                    return; // found matching item — done
                }
            }
            catch (Exception ex) { Log.Error(ex, $"Plc_OnValueChanged [{plcName}] {tag?.Name}"); }
        }

        /// <summary>
        /// Xử lý tag TimeRunStep1-15: cập nhật model và RevoStep.TotalRunTime.
        /// Tương đương TimeRunStep_ValueChanged cũ.
        /// </summary>
        private void HandleTimeRunStep(AutoRollingTagChangedModel item, string tagName, string newValueStr)
        {
            var timeRunStep = double.TryParse(newValueStr, out double dval) ? dval : 0;
            var revoModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);

            // Parse index từ "TimeRunStep{N}"
            if (!int.TryParse(tagName.Replace("TimeRunStep", ""), out int stepIdx)
                || stepIdx < 1 || stepIdx > 15) return;

            // Cập nhật trường tương ứng trong model
            SetTimeRunStep(item, stepIdx, timeRunStep);

            if (revoModel != null)
            {
                var step = revoModel.Steps.FirstOrDefault(s => s.StepIndex == stepIdx);
                if (step == null)
                {
                    step = new RevoStep { StepIndex = stepIdx, StepName = $"Step {stepIdx}" };
                    revoModel.Steps.Add(step);
                }
                step.TotalRunTime = GetTimeRunStepByIndex(item, stepIdx);
            }

            UpdateGrid();
        }

        /// <summary>
        /// Gán giá trị TimeRunStepN vào đúng property của model theo index.
        /// </summary>
        private static void SetTimeRunStep(AutoRollingTagChangedModel item, int index, double value)
        {
            switch (index)
            {
                case 1: item.TimeRunStep1 = value; break;
                case 2: item.TimeRunStep2 = value; break;
                case 3: item.TimeRunStep3 = value; break;
                case 4: item.TimeRunStep4 = value; break;
                case 5: item.TimeRunStep5 = value; break;
                case 6: item.TimeRunStep6 = value; break;
                case 7: item.TimeRunStep7 = value; break;
                case 8: item.TimeRunStep8 = value; break;
                case 9: item.TimeRunStep9 = value; break;
                case 10: item.TimeRunStep10 = value; break;
                case 11: item.TimeRunStep11 = value; break;
                case 12: item.TimeRunStep12 = value; break;
                case 13: item.TimeRunStep13 = value; break;
                case 14: item.TimeRunStep14 = value; break;
                case 15: item.TimeRunStep15 = value; break;
            }
        }

        #endregion

        #region Method helper
        private void UpdateGrid()
        {
            GlobalVariable.InvokeIfRequired(this, () =>
            {
                if (_grv.DataSource == null)
                    _grv.DataSource = _tagsValueRealtime;
                else
                    _grv.Refresh();
            });
        }

        /// <summary>
        /// Chuyển đổi giá trị ushort (16 bit) thành mảng bool 16 phần tử.
        /// </summary>
        public bool[] ConvertWordToBitArray(ushort wordValue)
        {
            bool[] bits = new bool[16];
            bits[0] = false; // bit 0 luôn = 0 theo convention

            for (int i = 1; i < 16; i++)
                bits[i] = (wordValue & (1 << i)) != 0;

            return bits;
        }

        /// <summary>
        /// Cập nhật UI (unused — giữ lại để backward compat với code khác có thể gọi).
        /// </summary>
        public void UpdateStepUI(RevoStep step, int isFirst = 0) { }

        /// <summary>
        /// Flush TotalTime hiện tại của shaft đang chạy vào DB trước khi reset.
        /// Đảm bảo bước cuối cùng của shaft cũ được lưu đúng giá trị thực tế.
        /// Gọi từ TaskResetShaftAsync TRƯỚC khi reset TotalRunTime = 0.
        /// </summary>
        private async Task FlushShaftTimesAsync(RevoRealtimeModel item)
        {
            if (!item.ShaftNum.HasValue || item.ShaftNum.Value == Guid.Empty) return;

            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    var shaftNum = item.ShaftNum;
                    var dbRecords = await dbContext.FT09_RevoDatalogs
                        .Where(x => x.ShaftNum == shaftNum)
                        .ToListAsync();

                    if (dbRecords.Count == 0) return;

                    // Flush lần cuối: update TẤT CẢ step enabled (không dùng delta check)
                    // để đảm bảo mọi bước — kể cả bước chưa từng thay đổi — đều được ghi đúng giá trị
                    // thực tế trước khi shaft bị reset về 0.
                    bool hasChanges = false;
                    foreach (var step in item.Steps.Where(x => x.Enable == true))
                    {
                        var record = dbRecords.FirstOrDefault(r => r.StepId == step.StepIndex);
                        if (record == null) continue;

                        record.TotalTime = step.TotalRunTime ?? 0;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        await dbContext.SaveChangesAsync();
                        Log.Information($"[FT09] Flushed shaft times for {item.RevoName}, ShaftNum={item.ShaftNum:D}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[FT09] FlushShaftTimesAsync error — {item.RevoName}");
            }
        }

        /// <summary>
        /// Insert tất cả step enabled với TotalTime=0 vào FT09 khi shaft mới bắt đầu.
        /// Gọi MỘT LẦN từ TaskResetShaftAsync ngay sau khi tạo ShaftNum mới.
        /// </summary>
        private async Task InsertShaftAsync(RevoRealtimeModel item)
        {
            if (!item.ShaftNum.HasValue || item.ShaftNum.Value == Guid.Empty) return;

            var enabledSteps = item.Steps.Where(x => x.Enable == true).ToList();
            if (enabledSteps.Count == 0)
            {
                Log.Warning($"[FT09] InsertShaftAsync: không có step enabled cho {item.RevoName} — bỏ qua (RecipeSettingStep=0?)");
                return;
            }

            try
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    var now = DateTime.Now;
                    var toInsert = new List<FT09_RevoDatalog>();
                    foreach (var step in enabledSteps)
                    {
                        toInsert.Add(new FT09_RevoDatalog
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = now,
                            CreatedMachine = Environment.MachineName,
                            RevoId = item.RevoId,
                            RevoName = item.RevoName,
                            Work = item.Work,
                            Part = item.Part,
                            ShaftNum = item.ShaftNum,
                            StepId = step.StepIndex,
                            StepName = step.StepName,
                            // Dùng TotalRunTime hiện tại thay vì hardcode 0:
                            // - Shaft đầu tiên (app vừa mở): lưu giá trị thực tế đọc từ PLC
                            // - Shaft reset bình thường: model đã được reset về 0 trước khi gọi hàm này
                            TotalTime = step.TotalRunTime ?? 0
                        });
                    }

                    dbContext.FT09_RevoDatalogs.AddRange(toInsert);
                    await dbContext.SaveChangesAsync();

                    Log.Information($"[FT09] Inserted {toInsert.Count} steps for {item.RevoName}, ShaftNum={item.ShaftNum:D}, Part={item.Part}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[FT09] InsertShaftAsync error — {item.RevoName}");
            }
        }

        /// <summary>
        /// Update TotalTime cho các step đang active trong DB.
        /// Gọi định kỳ từ TaskTimerAsync (200ms). Chỉ SaveChanges khi có thay đổi thực sự.
        /// </summary>
        private async Task UpdateShaftTimesAsync()
        {
            await _logStepRunLock.WaitAsync();
            try
            {
                var validModels = GlobalVariable.RevoRealtimeModels
                    .Where(x => x.ShaftNum.HasValue && x.ShaftNum.Value != Guid.Empty)
                    .ToList();

                if (validModels.Count == 0) return;

                using (var dbContext = new ApplicationDbContext())
                {
                    var shaftNums = validModels.Select(x => x.ShaftNum).Distinct().ToList();

                    var dbRecords = await dbContext.FT09_RevoDatalogs
                        .Where(x => x.ShaftNum.HasValue && shaftNums.Contains(x.ShaftNum))
                        .ToListAsync();

                    bool hasChanges = false;
                    foreach (var item in validModels)
                    {
                        foreach (var step in item.Steps.Where(x => x.Enable == true))
                        {
                            var record = dbRecords.FirstOrDefault(r =>
                                r.RevoId == item.RevoId &&
                                r.ShaftNum == item.ShaftNum &&
                                r.StepId == step.StepIndex);

                            if (record == null) continue; // InsertShaftAsync chưa chạy xong, bỏ qua

                            double newTime = step.TotalRunTime ?? 0;
                            if (Math.Abs((record.TotalTime ?? 0) - newTime) > 0.001)
                            {
                                record.TotalTime = newTime;
                                hasChanges = true;
                            }
                        }
                    }

                    if (hasChanges)
                        await dbContext.SaveChangesAsync();
                }
            }
            finally
            {
                _logStepRunLock.Release();
            }
        }

        private async Task LogDataRealtime()
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var listIds = GlobalVariable.RevoConfigs.Select(x => x.Id).Distinct().ToList();
                var dbRecords = dbContext.FT08_RevoRealtimes
                    .Where(x => listIds.Contains(x.C000_RevoId.Value))
                    .ToList();

                foreach (var item in GlobalVariable.RevoRealtimeModels)
                {
                    var dbRecord = dbRecords.FirstOrDefault(x => x.C000_RevoId == item.RevoId);
                    if (dbRecord != null)
                    {
                        dbRecord.C001_Data = JsonConvert.SerializeObject(item);
                    }
                    else
                    {
                        dbContext.FT08_RevoRealtimes.Add(new FT08_RevoRealtime
                        {
                            Id = Guid.NewGuid(),
                            C000_RevoId = item.RevoId,
                            C001_Data = JsonConvert.SerializeObject(item)
                        });
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public double GetTimeRunStepByIndex(AutoRollingTagChangedModel model, int index)
        {
            return index switch
            {
                1 => model.TimeRunStep1,
                2 => model.TimeRunStep2,
                3 => model.TimeRunStep3,
                4 => model.TimeRunStep4,
                5 => model.TimeRunStep5,
                6 => model.TimeRunStep6,
                7 => model.TimeRunStep7,
                8 => model.TimeRunStep8,
                9 => model.TimeRunStep9,
                10 => model.TimeRunStep10,
                11 => model.TimeRunStep11,
                12 => model.TimeRunStep12,
                13 => model.TimeRunStep13,
                14 => model.TimeRunStep14,
                15 => model.TimeRunStep15,
                _ => 0
            };
        }

        private async Task<GroupShaftModel> GetTotalShaftAsync()
        {
            using var dbContext = new ApplicationDbContext();

            var maxCreatedAt = dbContext.FT09_RevoDatalogs
                .Where(x => x.CreatedAt != null)
                .Max(x => x.CreatedAt);

            if (maxCreatedAt == null) return null;

            var currentHour = new DateTime(
                maxCreatedAt.Value.Year, maxCreatedAt.Value.Month, maxCreatedAt.Value.Day,
                maxCreatedAt.Value.Hour, 0, 0);

            var nextHour = currentHour.AddHours(1);
            var lastHour = currentHour.AddHours(-1);

            var currentCount = await dbContext.FT09_RevoDatalogs
                .Where(x => x.CreatedAt >= currentHour && x.CreatedAt < nextHour)
                .GroupBy(x => x.ShaftNum!)
                .Where(g => g.All(r => r.StartedAt != null && r.EndedAt != null))
                .Select(g => g.Key)
                .CountAsync();

            var lastCount = await dbContext.FT09_RevoDatalogs
                .Where(x => x.CreatedAt >= lastHour && x.CreatedAt < currentHour)
                .GroupBy(x => x.ShaftNum!)
                .Where(g => g.All(r => r.StartedAt != null && r.EndedAt != null))
                .Select(g => g.Key)
                .CountAsync();

            return new GroupShaftModel
            {
                CurrentHour = currentCount,
                LastHour = lastCount
            };
        }
        #endregion

        #region Events
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnMaitenance_Click(object sender, EventArgs e)
        {
            using (var nf = new frmConfig())
            {
                nf.ShowDialog();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        #endregion
    }
}
