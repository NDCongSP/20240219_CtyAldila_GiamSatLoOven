using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada.TrackingTime_AutoRolling1
{
    public partial class frmProduction : Form
    {
        private EasyDriverConnector _easyDriverConnector;
        private ConnectionStatus _easyStatus;

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

        //// Giả sử bạn muốn truyền thông tin Device hoặc Path
        //public class ResetArgs
        //{
        //    public string Path { get; set; }
        //    public int StepValue { get; set; }
        //}
        //private readonly ConcurrentQueue<ResetArgs> _resetQueue = new ConcurrentQueue<ResetArgs>();

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
        // Nếu Path đã tồn tại, ghi đè reason mới (ưu tiên reason cuối cùng).
        private readonly ConcurrentDictionary<string, ShaftActionReason> _resetShaftQueue
            = new ConcurrentDictionary<string, ShaftActionReason>();
        private readonly ConcurrentQueue<string> _resetPartQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Helper: enqueue Path vào queue reset shaft, đảm bảo unique.
        /// Nếu Path đã có thì ghi đè reason mới.
        /// </summary>
        private void EnqueueResetShaft(string path, ShaftActionReason reason)
        {
            _resetShaftQueue[path] = reason; // overwrite-if-exists, atomic
            _triggerResetShaft.Set();
        }

        /// <summary>
        /// Lưu thời điểm reset shaft gần nhất theo Path để debounce cross-batch.
        /// Nếu Part_Code và Step_Run=0 fire cách nhau nhưng trong khoảng debounce thì chỉ reset 1 lần.
        /// </summary>
        private readonly Dictionary<string, DateTime> _lastShaftResetAt = new Dictionary<string, DateTime>();
        private static readonly TimeSpan SHAFT_RESET_DEBOUNCE = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Hash của snapshot dữ liệu FT09 lần log gần nhất.
        /// Nếu hash hiện tại == hash này -> data không đổi -> bỏ qua log để tránh DB write thừa.
        /// </summary>
        private string _lastLogStepRunHash = string.Empty;

        /// <summary>
        /// Lock để serialize các call vào LogDataStepRun (chống race condition giữa
        /// TaskTimerAsync và TaskResetShaftAsync gây INSERT duplicate ở FT09).
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
            // Cấu hình form
            this.Text = "Custom Title Bar";
            this.FormBorderStyle = FormBorderStyle.None; // Bỏ header mặc định
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1350, 514);

            // Tạo panel làm thanh tiêu đề
            titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 40;
            titleBar.BackColor = Color.Black;
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            //// Nút Close
            //btnClose = new Button();
            //btnClose.Text = "";
            //btnClose.ForeColor = Color.White;
            //btnClose.BackColor = Color.Black;
            //btnClose.FlatStyle = FlatStyle.Flat;
            //btnClose.FlatAppearance.BorderSize = 0;
            //btnClose.Size = new Size(40, 40);
            //btnClose.Location = new Point(this.Width - 40, 0);
            //btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            //// 1) Gán icon từ Resources (đặt tên hình là "updateVersion" như trong Resource)
            //btnClose.Image = Properties.Resources.close_window_30_white;  // PNG từ Resources
            //btnClose.ImageAlign = ContentAlignment.MiddleCenter;  // căn giữa
            //btnClose.Padding = new Padding(0);                    // tránh lệch
            //btnClose.TextImageRelation = TextImageRelation.Overlay; // chỉ icon
            //btnClose.Click += BtnClose_Click;
            //titleBar.Controls.Add(btnClose);

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
            // 1) Gán icon từ Resources (đặt tên hình là "updateVersion" như trong Resource)
            btnMaximize.Image = Properties.Resources.maximize_30_white;  // PNG từ Resources
            btnMaximize.ImageAlign = ContentAlignment.MiddleCenter;  // căn giữa
            btnMaximize.Padding = new Padding(0);                    // tránh lệch
            btnMaximize.TextImageRelation = TextImageRelation.Overlay; // chỉ icon
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
            // 1) Gán icon từ Resources (đặt tên hình là "updateVersion" như trong Resource)
            btnMinimize.Image = Properties.Resources.minimize_30_White;  // PNG từ Resources
            btnMinimize.ImageAlign = ContentAlignment.MiddleCenter;  // căn giữa
            btnMinimize.Padding = new Padding(0);                    // tránh lệch
            btnMinimize.TextImageRelation = TextImageRelation.Overlay; // chỉ icon
            btnMinimize.Click += BtnMinimize_Click;
            titleBar.Controls.Add(btnMinimize);

            // Nút update version
            btnMaintenance = new Button();
            btnMaintenance.Text = "";                      // Không cần chữ, chỉ hiển thị icon
            btnMaintenance.ForeColor = Color.White;
            btnMaintenance.BackColor = Color.Black;
            btnMaintenance.FlatStyle = FlatStyle.Flat;
            btnMaintenance.FlatAppearance.BorderSize = 0;
            btnMaintenance.Size = new Size(40, 40);
            btnMaintenance.Location = new Point(this.Width - 120, 0);
            btnMaintenance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaintenance.Cursor = Cursors.Hand;

            // 1) Gán icon từ Resources (đặt tên hình là "updateVersion" như trong Resource)
            btnMaintenance.Image = Properties.Resources.maintenance_30_white;  // PNG từ Resources
            btnMaintenance.ImageAlign = ContentAlignment.MiddleCenter;  // căn giữa
            btnMaintenance.Padding = new Padding(0);                    // tránh lệch
            btnMaintenance.TextImageRelation = TextImageRelation.Overlay; // chỉ icon

            // Tùy chọn: scale icon nếu quá lớn/nhỏ (WinForms Button không có ImageLayout)
            // => bạn có thể dùng phiên bản icon 24x24 hoặc 32x32 trong file PNG để vừa với nút 40x40.

            // 2) Tooltip khi hover
            var tip = new ToolTip();
            tip.AutoPopDelay = 5000;     // hiển thị tối đa 5 giây
            tip.InitialDelay = 300;      // trễ 300ms
            tip.ReshowDelay = 100;       // xuất hiện lại nhanh
            tip.ShowAlways = true;       // luôn hiển thị tooltip
            tip.SetToolTip(btnMaintenance, "MAINTENANCE");  // nội dung tooltip

            // Tùy chọn: hiệu ứng hover (đổi nền cho dễ nhìn)
            btnMaintenance.MouseEnter += (s, e) => btnMaintenance.BackColor = Color.FromArgb(30, 30, 30);
            btnMaintenance.MouseLeave += (s, e) => btnMaintenance.BackColor = Color.Black;

            // Sự kiện Click (giữ nguyên như bạn đã có)
            btnMaintenance.Click += btnMaitenance_Click; ; // hoặc sự kiện update version thực tế của bạn
            titleBar.Controls.Add(btnMaintenance);


            // Đảm bảo tất cả có cùng Height = 30 và Y = 5
            btnMaximize.Size = btnMinimize.Size = btnMaintenance.Size = new Size(30, 30);


            // Anchor cho cả 3 nút
            btnMaximize.Anchor = btnMinimize.Anchor = btnMaintenance.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Logo
            PictureBox logo = new PictureBox();
            logo.Image = Properties.Resources.logoAldila; // logo từ Resources
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.Size = new Size(100, 30); // kích thước logo
            logo.Location = new Point(0, 5); // vị trí bên trái
            titleBar.Controls.Add(logo);

            // Text
            titleText = new Label();
            titleText.Text = $"AUTO ROLLING PRODUCTION";
            titleText.ForeColor = Color.White;
            titleText.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleText.AutoSize = true;
            titleText.Location = new Point(100, 10); // ngay sau logo
            titleBar.Controls.Add(titleText);
            #endregion

            // 1. Đảm bảo Footer nằm dưới cùng
            _labStatus.Dock = DockStyle.Bottom;
            _labStatus.Height = 30; // Chỉnh độ cao phù hợp

            // 2. Ép nó phải hiển thị lên trên (Z-Order)
            _labStatus.BringToFront();

            // 3. Nếu flowMain đang là Dock Fill, nó sẽ tự động chừa chỗ cho _labStatus
            // flowMain.Dock = DockStyle.Fill;

            Load += frmProduction_Load;
            //FormClosing += Form1_FormClosing;
        }

        private void frmProduction_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                _easyDriverConnector.ConnectionStatusChaged -= _easyDriverConnector_ConnectionStatusChaged;
                _easyDriverConnector.Started -= _easyDriverConnector_Started;
                _easyDriverConnector.Stop();

                _timerCts.Cancel();
                _timerTask.Wait(1000);

                _checkingTimeStepCts.Cancel();
                _resetShaftCts.Cancel();
                _resetPartCts.Cancel();
            }
            catch
            {

            }
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
            //ddocj gias trij config
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
                            "CẢNH BÁO", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            );

                        return;
                    }

                    Text = $"Chương trình giám sát thời gian chạy - Máy Auto Rolling";
                }
                else
                {
                    MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                        "CẢNH BÁO", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                        );
                    return;
                }

                //khởi tạo model realtime data
                foreach (var item in GlobalVariable.RevoConfigs)
                {
                    // FIX: tạo list Steps RIÊNG cho từng RevoRealtimeModel.
                    // KHÔNG được share 1 list giữa các models, vì List<T> là reference type
                    // -> nếu share, event TimeRunStep của RevoId này sẽ ghi đè TotalRunTime
                    // của RevoId khác (trông giống như "bị reset về 0").
                    var stepsForThisRevo = new List<RevoStep>();
                    for (int i = 1; i <= 15; i++)
                    {
                        stepsForThisRevo.Add(new RevoStep()
                        {
                            StepIndex = i,
                            StepName = $"Step {i}",
                        });
                    }

                    GlobalVariable.RevoRealtimeModels.Add(new RevoRealtimeModel()
                    {
                        RevoId = item.Id.Value,
                        RevoName = item.Name,
                        Path = item.Path,
                        Steps = stepsForThisRevo,
                        // KHÔNG gán ShaftNum ở đây - để mặc định Guid.Empty.
                        // LogDataStepRun đã filter bỏ qua Guid.Empty -> không log gì
                        // cho tới khi TaskResetShaftAsync gán Guid thật từ event PLC đầu tiên.
                        // Như vậy mỗi transition (Part đổi / Step_Run=0) chỉ tạo đúng 1 ShaftNum.
                    });

                    _tagsValueRealtime.Add(new AutoRollingTagChangedModel()
                    {
                        RevoId = item.Id.Value,
                        RevoName = item.Name,
                        Path = item.Path,
                    });
                }

                #region Khởi tạo easy drirver connector
                _easyDriverConnector = new EasyDriverConnector();
                _easyDriverConnector.ConnectionStatusChaged += _easyDriverConnector_ConnectionStatusChaged;
                _easyDriverConnector.BeginInit();
                _easyDriverConnector.EndInit();
                //_easyStatus = _easyDriverConnector.ConnectionStatus;

                _easyDriverConnector.Started += _easyDriverConnector_Started;
                if (_easyDriverConnector.IsStarted)
                {
                    _easyDriverConnector_Started(null, null);
                }
                #endregion

                _timerCts = new CancellationTokenSource();
                _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

                _resetShaftCts = new CancellationTokenSource();
                _ = TaskResetShaftAsync(_resetShaftCts.Token);
            }
        }

        #region Tasks
        /// <summary>
        /// Task dùng để chạy vòng lặp check để luu data vào DB và update UI. Task này chạy liên tục với khoảng delay 0.1s, mỗi lần chạy sẽ thực hiện các công việc sau: show UI, Log data vào DB.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task TaskTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var totalShaft = await GetTotalShaftAsync();
                    _totalCurrentHour = totalShaft?.CurrentHour ?? 0;
                    _totalLastHour = totalShaft?.LastHour ?? 0;

                    //var ping = PingServer(GlobalVariable.IpDbServer);
                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        _labStatus.Text = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} - Easy Driver: {_easyStatus} - PLC: {_tagsValueRealtime.FirstOrDefault().PlcConnected} - DB Server:";
                    });

                    //Log data realtime (FT08) - mỗi 200ms
                    var t = GlobalVariable.RevoRealtimeModels;
                    await LogDataRealtime();

                    // Log step run (FT09). Bên trong LogDataStepRun có check hash:
                    // chỉ thực sự write DB khi data đổi -> gọi 200ms cũng OK.
                    await LogDataStepRun();
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    //Log.Error($"Lỗi trong TaskTimerAsync: {ex.Message}");
                }
                await Task.Delay(200, token); // Chờ 1 giây trước khi lặp lại
            }
        }

        /// <summary>
        /// Task xử lý khi có sự kiện reset trục (Shaft) được kích hoạt. Mỗi khi có sự kiện này.
        /// task sẽ thực hiện các công việc cần thiết để reset trạng thái của trục, cập nhật UI và ghi log vào database.
        /// Sau khi hoàn thành công việc, task sẽ quay lại trạng thái chờ để đợi sự kiện tiếp theo mà không cần phải sử dụng Sleep hay polling, giúp tiết kiệm tài nguyên CPU.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task TaskResetShaftAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    // chờ sự kiện → không tốn CPU, không Sleep
                    await _triggerResetShaft.WaitAsync(token);

                    // === xử lý 1 lần duy nhất mỗi lần được kích ===
                    Debug.WriteLine($"TaskResetShaftAsync triggered.");

                    // Snapshot các Path đang có trong queue rồi xóa khỏi dictionary.
                    // ConcurrentDictionary đã đảm bảo mỗi Path chỉ tồn tại 1 entry (unique).
                    var snapshot = _resetShaftQueue.ToArray();
                    foreach (var kv in snapshot)
                    {
                        _resetShaftQueue.TryRemove(kv.Key, out _);
                    }

                    foreach (var kv in snapshot)
                    {
                        var currentPath = kv.Key;
                        var reason = kv.Value;

                        // Debounce cross-batch: nếu vừa reset trong < SHAFT_RESET_DEBOUNCE thì bỏ qua
                        if (_lastShaftResetAt.TryGetValue(currentPath, out var lastAt)
                            && (DateTime.Now - lastAt) < SHAFT_RESET_DEBOUNCE)
                        {
                            Debug.WriteLine($"Skip {reason} for {currentPath} (debounced, last reset {lastAt:HH:mm:ss.fff})");
                            continue;
                        }

                        // Tìm item trong list của bạn dựa trên Path
                        var targetItem = _tagsValueRealtime.FirstOrDefault(x => x.Path == currentPath);
                        if (targetItem == null) continue;

                        var realtimeItem = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.Path == currentPath);
                        if (realtimeItem == null) continue;

                        Debug.WriteLine($"Processing {reason} for Path: {currentPath}");

                        // Cả 2 trường hợp (Part đổi / Step_Run = 0) đều coi là sang cây shaft mới.
                        // CHỈ cấp ShaftNum mới — KHÔNG reset TotalRunTime.
                        // TotalRunTime do PLC quản lý qua TimeRunStep_ValueChanged.
                        // (Nếu reset ở đây sẽ ghi đè giá trị PLC vừa gán, mà TimeRunStep
                        //  ValueChanged chỉ fire lại khi giá trị thật sự đổi -> bị stuck ở 0)
                        realtimeItem.ShaftNum = Guid.NewGuid();
                        _lastShaftResetAt[currentPath] = DateTime.Now;

                        await LogDataStepRun();
                    }
                    // xong việc → quay lại vòng chờ (không cần Delay hay poll)
                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
        }
        #endregion

        #region EASY DRIVER EVENTS
        private void _easyDriverConnector_ConnectionStatusChaged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _easyStatus = e.NewStatus;
        }

        private void _easyDriverConnector_Started(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
            foreach (var item in GlobalVariable.RevoConfigs)
            {
                _easyDriverConnector.GetTag($"{item.Path}/Part_Code").QualityChanged += Part_Code_QualityChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Code").ValueChanged += Part_Code_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings").ValueChanged += Recipe_Settings_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Step_Run").ValueChanged += Step_Run_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep1").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep2").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep3").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep4").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep5").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep6").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep7").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep8").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep9").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep10").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep11").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep12").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep13").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep14").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep15").ValueChanged += TimeRunStep_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name1").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name2").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name3").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name4").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name5").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name6").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name7").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Name8").ValueChanged += Part_Name_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_1").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_2").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_3").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_4").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_5").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_6").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_7").ValueChanged += Work_ValueChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Work_8").ValueChanged += Work_ValueChanged;

                Part_Code_QualityChanged(_easyDriverConnector.GetTag($"{item.Path}/Part_Code")
          , new TagQualityChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Part_Code")
          , Quality.Uncertain, _easyDriverConnector.GetTag($"{item.Path}/Part_Code").Quality));

                Part_Code_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Part_Code")
                    , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Part_Code")
                    , "", _easyDriverConnector.GetTag($"{item.Path}/Part_Code").Value));

                Recipe_Settings_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings")
                    , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings")
                    , "", _easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings").Value));

                Step_Run_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Step_Run")
                   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Step_Run")
                   , "", _easyDriverConnector.GetTag($"{item.Path}/Step_Run").Value));

                for (int i = 1; i <= 15; i++)
                {
                    if (i <= 8)
                    {
                        Part_Name_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Part_Name{i}")
                         , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Part_Name{i}")
                         , "", _easyDriverConnector.GetTag($"{item.Path}/Part_Name{i}").Value));

                        Work_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Work_{i}")
                      , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Work_{i}")
                      , "", _easyDriverConnector.GetTag($"{item.Path}/Work_{i}").Value));
                    }

                    TimeRunStep_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}")
                           , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}")
                           , "", _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}").Value));
                }
            }
        }

        private void Part_Code_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var al = deviceName.Substring(4);

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.PlcConnected = e.NewQuality == Quality.Good;

                        //cập nhật lại realtime model để tránh trường hợp tag mất kết nối rồi có giá trị mới vẫn cập nhật vào model, dẫn đến sai dữ liệu
                        var realtimeModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.Path == path);
                        realtimeModel.PlcConnected = item.PlcConnected;

                        Log.Warning($"Alarm description {item.RevoId} disconnect.");
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        /// <summary>
        /// Khi tag này thay đổi thì sẽ chuyển part code mới vào model realtime, đồng thời đưa path vào hàng đợi để TaskResetShaftAsync lấy ra xử lý reset trục (vì khi thay đổi part code thì sẽ reset trục). Sau khi TaskResetShaftAsync xử lý xong sẽ cập nhật lại UI và log vào DB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Part_Code_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var al = deviceName.Substring(4);
                var tagName = e.Tag.Name;

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.Part_Code = e.NewValue;

                        UpdateGrid();
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        /// <summary>
        /// Tag này sẽ thay đổi mỗi khi tag partCode thay đổi, nó quy định theo công thức thì các bước nào được sử dụng.
        /// Sẽ được mã hóa thành một số nguyên và gửi về PLC. Khi tag này thay đổi thì sẽ chuyển giá trị mới vào model realtime, đồng thời chuyển sang dạng bit để biết được step nào đang được sử dụng, sau đó cập nhật lại UI. Ngoài ra khi tag này thay đổi cũng sẽ đưa path vào hàng đợi để TaskResetShaftAsync lấy ra xử lý reset trục (vì khi thay đổi recipe setting thì sẽ reset trục). Sau khi TaskResetShaftAsync xử lý xong sẽ cập nhật lại UI và log vào DB.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Recipe_Settings_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var al = deviceName.Substring(4);

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.Recipe_Settings = int.TryParse(e.NewValue, out int value) ? value : 0;
                        item.StepsIsRun = ConvertWordToBitArray((ushort)item.Recipe_Settings);

                        //Cập nhật model realtime 
                        //mappign data
                        // 1. Tìm đối tượng tương ứng trong list RevoRealtimeModels
                        var revoModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);

                        if (revoModel != null)
                        {
                            // 2. Duyệt qua 15 bước (bỏ qua bit 0 theo yêu cầu của bạn)
                            for (int i = 1; i <= 15; i++)
                            {
                                // Tìm StepIndex tương ứng (Giả định StepIndex trong RevoStep khớp với số thứ tự)
                                var step = revoModel.Steps.FirstOrDefault(s => s.StepIndex == i);

                                if (step == null)
                                {
                                    // Nếu chưa có step trong list thì khởi tạo mới
                                    step = new RevoStep { StepIndex = i };
                                    revoModel.Steps.Add(step);
                                }

                                // 3. Cập nhật trạng thái chạy từ mảng bool[] StepsIsRun
                                // Giả định StepsIsRun có 16 phần tử, index 1 tương ứng Step 1
                                step.Enable = item.StepsIsRun[i];
                            }

                            // Có thể cập nhật thêm thông tin Part hiện tại nếu cần
                            // revoModel.CurrentPart = tagData.Part_Code; 
                        }

                        UpdateGrid();
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Step_Run_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var tagName = e.Tag.Name;

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.StepRun = int.TryParse(e.NewValue, out int value) ? value : 0;

                        if (item.StepRun == 0)
                        {
                            // Step_Run = 0 -> kết thúc shaft cũ, sang cây shaft mới
                            EnqueueResetShaft(item.Path, ShaftActionReason.StepRunZero);
                        }

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void TimeRunStep_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var tagName = e.Tag.Name;

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        var timeRunStep = double.TryParse(e.NewValue, out double value) ? value : 0;

                        // Tìm đối tượng tương ứng trong list RevoRealtimeModels (1 lần, dùng chung cho mọi case)
                        var revoModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);

                        switch (tagName)
                        {
                            case "TimeRunStep1":
                                {
                                    item.TimeRunStep1 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 1);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 1, StepName = "Step 1" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 1);
                                    break;
                                }
                            case "TimeRunStep2":
                                {
                                    item.TimeRunStep2 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 2);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 2, StepName = "Step 2" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 2);
                                    break;
                                }
                            case "TimeRunStep3":
                                {
                                    item.TimeRunStep3 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 3);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 3, StepName = "Step 3" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 3);
                                    break;
                                }
                            case "TimeRunStep4":
                                {
                                    item.TimeRunStep4 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 4);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 4, StepName = "Step 4" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 4);
                                    break;
                                }
                            case "TimeRunStep5":
                                {
                                    item.TimeRunStep5 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 5);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 5, StepName = "Step 5" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 5);
                                    break;
                                }
                            case "TimeRunStep6":
                                {
                                    item.TimeRunStep6 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 6);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 6, StepName = "Step 6" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 6);
                                    break;
                                }
                            case "TimeRunStep7":
                                {
                                    item.TimeRunStep7 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 7);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 7, StepName = "Step 7" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 7);
                                    break;
                                }
                            case "TimeRunStep8":
                                {
                                    item.TimeRunStep8 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 8);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 8, StepName = "Step 8" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 8);
                                    break;
                                }
                            case "TimeRunStep9":
                                {
                                    item.TimeRunStep9 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 9);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 9, StepName = "Step 9" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 9);
                                    break;
                                }
                            case "TimeRunStep10":
                                {
                                    item.TimeRunStep10 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 10);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 10, StepName = "Step 10" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepXm
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 10);
                                    break;
                                }
                            case "TimeRunStep11":
                                {
                                    item.TimeRunStep11 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 11);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 11, StepName = "Step 11" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 11);
                                    break;
                                }
                            case "TimeRunStep12":
                                {
                                    item.TimeRunStep12 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 12);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 12, StepName = "Step 12" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 12);
                                    break;
                                }
                            case "TimeRunStep13":
                                {
                                    item.TimeRunStep13 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 13);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 13, StepName = "Step 13" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 13);
                                    break;
                                }
                            case "TimeRunStep14":
                                {
                                    item.TimeRunStep14 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 14);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 14, StepName = "Step 14" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 14);
                                    break;
                                }
                            case "TimeRunStep15":
                                {
                                    item.TimeRunStep15 = timeRunStep;

                                    var step = revoModel?.Steps.FirstOrDefault(s => s.StepIndex == 15);
                                    if (step == null)
                                    {
                                        // Nếu chưa có step trong list thì khởi tạo mới
                                        step = new RevoStep { StepIndex = 15, StepName = "Step 15" };
                                        revoModel.Steps.Add(step);
                                    }

                                    // Cập nhật thời gian chạy từ các biến TimeRunStepX
                                    step.TotalRunTime = GetTimeRunStepByIndex(item, 15);
                                    break;
                                }
                            default:
                                // code block
                                break;
                        }

                        UpdateGrid();

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Part_Name_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var tagName = e.Tag.Name;

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        var charInt = int.TryParse(e.NewValue, out int value) ? value : 0;

                        switch (tagName)
                        {
                            case "Part_Name1":
                                item.Part_Name1 = charInt;
                                break;
                            case "Part_Name2":
                                item.Part_Name2 = charInt;
                                break;
                            case "Part_Name3":
                                item.Part_Name3 = charInt;
                                break;
                            case "Part_Name4":
                                item.Part_Name4 = charInt;
                                break;
                            case "Part_Name5":
                                item.Part_Name5 = charInt;
                                break;
                            case "Part_Name6":
                                item.Part_Name6 = charInt;
                                break;
                            case "Part_Name7":
                                item.Part_Name7 = charInt;
                                break;
                            case "Part_Name8":
                                item.Part_Name8 = charInt;
                                break;
                            default:
                                // code block
                                break;
                        }

                        GetCharFromInt(item.RevoId, "Part");

                        //Cập nhật model realtime
                        //mappign data
                        // 1. Tìm đối tượng tương ứng trong list RevoRealtimeModels
                        var revoModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);

                        if (revoModel != null)
                        {
                            revoModel.Part = item.Part_Name;
                        }
                        // Đưa Path vào hàng đợi để Task lấy ra dùng
                        // (Dictionary đảm bảo unique theo Path -> không lo bị enqueue trùng dù
                        //  Part_Name1..8 fire liên tục)
                        EnqueueResetShaft(item.Path, ShaftActionReason.PartChanged);

                        UpdateGrid();

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Work_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var tagName = e.Tag.Name;

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        var charInt = int.TryParse(e.NewValue, out int value) ? value : 0;

                        switch (tagName)
                        {
                            case "Work_1":
                                item.Work_1 = charInt;
                                break;
                            case "Work_2":
                                item.Work_2 = charInt;
                                break;
                            case "Work_3":
                                item.Work_3 = charInt;
                                break;
                            case "Work_4":
                                item.Work_4 = charInt;
                                break;
                            case "Work_5":
                                item.Work_5 = charInt;
                                break;
                            case "Work_6":
                                item.Work_6 = charInt;
                                break;
                            case "Work_7":
                                item.Work_7 = charInt;
                                break;
                            case "Work_8":
                                item.Work_8 = charInt;
                                break;
                            default:
                                // code block
                                break;
                        }

                        GetCharFromInt(item.RevoId, "Work");

                        //Cập nhật model realtime
                        //mappign data
                        // 1. Tìm đối tượng tương ứng trong list RevoRealtimeModels
                        var revoModel = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.RevoId == item.RevoId);

                        if (revoModel != null)
                        {
                            revoModel.Work = item.Work;
                        }

                        UpdateGrid();

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        #endregion

        #region Method helper
        private void UpdateGrid()
        {
            GlobalVariable.InvokeIfRequired(this, () =>
            {
                if (_grv.DataSource == null)
                {
                    _grv.DataSource = _tagsValueRealtime;
                }
                else _grv.Refresh();
            });
        }

        /// <summary>
        /// Hàm chuyển đổi giá trị kiểu ushort (16 bit) thành một mảng bool có 16 phần tử, mỗi phần tử đại diện cho trạng thái của một bit trong giá trị đó.
        /// </summary>
        /// <param name="wordValue"></param>
        /// <returns></returns>
        public bool[] ConvertWordToBitArray(ushort wordValue)
        {
            // Tạo mảng 16 phần tử (tương ứng 16 bit từ 0-15)
            bool[] bits = new bool[16];

            // 1. Gán bit đầu tiên (bit 0) luôn bằng false (0) theo yêu cầu
            bits[0] = false;

            // 2. Vòng lặp lấy giá trị từ bit 1 đến bit 15
            for (int i = 1; i < 16; i++)
            {
                // Sử dụng phép toán AND với mặt nạ bit (mask) được dịch chuyển
                // Kiểm tra xem bit tại vị trí i có đang bật (1) hay không
                bits[i] = (wordValue & (1 << i)) != 0;
            }

            return bits;
        }

        /// <summary>
        /// Chuyển đổi sữ liệu dec đọc từ các tag của HMI về thành chuỗi, rồi lưu vào biến Part_Name của model để hiển thị lên UI và lưu vào database.
        /// Vì PLC lưu dữ liệu kiểu int nên mỗi ký tự sẽ được mã hóa thành 2 byte (16 bit),
        /// Do đó cần phải tách int thành 2 byte rồi chuyển đổi sang ký tự tương ứng theo bảng mã ASCII.
        /// Sau đó ghép các ký tự lại với nhau để tạo thành chuỗi hoàn chỉnh.
        /// </summary>
        /// <param name="revoId"></param>
        /// <param name="partOrWork">Part or Work</param>
        private void GetCharFromInt(int revoId, string partOrWork = "Part")
        {
            var machine = _tagsValueRealtime.FirstOrDefault(t => t.RevoId == revoId);
            // Tạo danh sách các giá trị từ Part_Name1 đến Part_Name8


            // Gán kết quả cuối cùng vào model (vì dùng ref nên bên ngoài sẽ nhận được luôn)
            if (partOrWork == "Part")
            {
                int[] registers = new int[]
            {
        machine.Part_Name1, machine.Part_Name2, machine.Part_Name3, machine.Part_Name4,
        machine.Part_Name5, machine.Part_Name6, machine.Part_Name7, machine.Part_Name8
            };

                string fullPartName = string.Empty;

                foreach (int val in registers)
                {
                    // 1. Tách số int thành 2 byte (mỗi byte là 1 ký tự ASCII)
                    // val & 0xFF lấy byte thấp (Low Byte)
                    // (val >> 8) & 0xFF lấy byte cao (High Byte)

                    byte lowByte = (byte)(val & 0xFF);
                    byte highByte = (byte)((val >> 8) & 0xFF);

                    // 2. Đảo vị trí: Theo yêu cầu của bạn là đảo vị trí
                    // Thông thường PLC lưu Byte Cao trước, Byte Thấp sau hoặc ngược lại.
                    // Ở đây tôi giả định bạn muốn ghép ký tự từ LowByte trước rồi HighByte (hoặc ngược lại)
                    char firstChar = (char)lowByte;
                    char secondChar = (char)highByte;

                    // Loại bỏ các ký tự rác hoặc ký tự Null (mã ASCII là 0) 
                    // và khoảng trắng dư thừa nếu cần (mã ASCII là 32)
                    if (lowByte != 0 && lowByte != 32) fullPartName += firstChar;
                    if (highByte != 0 && highByte != 32) fullPartName += secondChar;
                }

                machine.Part_Name = fullPartName.Trim();
            }
            else //if (partOrWork == "Work")
            {
                int[] registers = new int[]
            {
        machine.Work_1, machine.Work_2, machine.Work_3, machine.Work_4,
        machine.Work_5, machine.Work_6, machine.Work_7, machine.Work_8
            };

                string fullPartName = string.Empty;

                foreach (int val in registers)
                {
                    // 1. Tách số int thành 2 byte (mỗi byte là 1 ký tự ASCII)
                    // val & 0xFF lấy byte thấp (Low Byte)
                    // (val >> 8) & 0xFF lấy byte cao (High Byte)

                    byte lowByte = (byte)(val & 0xFF);
                    byte highByte = (byte)((val >> 8) & 0xFF);

                    // 2. Đảo vị trí: Theo yêu cầu của bạn là đảo vị trí
                    // Thông thường PLC lưu Byte Cao trước, Byte Thấp sau hoặc ngược lại.
                    // Ở đây tôi giả định bạn muốn ghép ký tự từ LowByte trước rồi HighByte (hoặc ngược lại)
                    char firstChar = (char)lowByte;
                    char secondChar = (char)highByte;

                    // Loại bỏ các ký tự rác hoặc ký tự Null (mã ASCII là 0) 
                    // và khoảng trắng dư thừa nếu cần (mã ASCII là 32)
                    if (lowByte != 0 && lowByte != 32) fullPartName += firstChar;
                    if (highByte != 0 && highByte != 32) fullPartName += secondChar;
                }
                machine.Work = fullPartName.Trim();
            }
        }

        /// <summary>
        /// cập nhật UI.
        /// </summary>
        /// <param name="step">Chứa thông tin bước chạy. để lấy index ggeer lấy ra đúng control.</param>
        /// <param name="isFirst">0-nextStep; 1-new; 2-previous step.</param>
        public void UpdateStepUI(RevoStep step, int isFirst = 0)
        {
            //foreach (Control ctrl in flowMain.Controls)
            //{
            //    if (ctrl is Panel row && row.Tag is int idx && idx == step?.StepIndex)
            //    {
            //        Label lblIndex = row.Controls["lblIndex"] as Label;
            //        Label lblStep = row.Controls["lblStep"] as Label;

            //        lblIndex.Text = step.StepIndex.ToString();

            //        var startAtText = step.StartAt.HasValue ? ((DateTime)step.StartAt).ToString("HH:mm:ss") : "null";
            //        var endAtText = step.EndAt.HasValue ? ((DateTime)step.EndAt).ToString("HH:mm:ss") : "null";

            //        lblStep.Text = $"{step.StepName} - {startAtText} -> {endAtText}: {step.TotalRunTime}s" +
            //            $"{Environment.NewLine}" +
            //            $"Pul={step.SoLuongXung} - Speed = {step.Speed_Hz}";

            //        if ((step.StartAt.HasValue && !step.EndAt.HasValue) || isFirst == 1)
            //        {
            //            lblStep.BackColor = Color.Red;
            //            lblStep.ForeColor = Color.White;

            //            lblIndex.BackColor = Color.Red;
            //            lblIndex.ForeColor = Color.White;
            //        }
            //        else if (step.StartAt.HasValue && step.EndAt.HasValue)
            //        {
            //            lblStep.BackColor = Color.Gray;
            //            lblStep.ForeColor = Color.White;

            //            lblIndex.BackColor = Color.Gray;
            //            lblIndex.ForeColor = Color.White;
            //        }
            //        else if (isFirst == 2)
            //        {
            //            lblStep.BackColor = Color.LightBlue;
            //            lblStep.ForeColor = Color.Black;

            //            lblIndex.BackColor = Color.LightBlue;
            //            lblIndex.ForeColor = Color.Black;
            //        }

            //        break;
            //    }
            //}
        }

        public async Task LogDataStepRun()
        {
            // Lock serialize: nếu có call khác đang chạy thì CHỜ (không skip).
            // Hash check phải ở BÊN TRONG lock để chống race condition - nếu để ngoài,
            // 2 call đồng thời cùng thấy hash khác _lastHash -> cùng INSERT -> duplicate.
            await _logStepRunLock.WaitAsync();
            try
            {
                // Tạo snapshot hash của data hiện tại.
                // Chỉ thực sự write DB khi hash khác lần trước -> tránh DB write thừa khi
                // gọi liên tục (200ms) mà data không đổi.
                var currentHash = BuildLogStepRunHash();
                if (currentHash == _lastLogStepRunHash) return; // không có gì thay đổi -> bỏ qua

                using (var dbContext = new ApplicationDbContext())
                {
                    var createdAt = DateTime.Now;
                    var createdMachine = Environment.MachineName;

                    // Bỏ qua các model có ShaftNum null hoặc Guid.Empty
                    var validModels = GlobalVariable.RevoRealtimeModels
                        .Where(x => x.ShaftNum.HasValue && x.ShaftNum.Value != Guid.Empty)
                        .ToList();

                    if (validModels.Count == 0) return;

                    // Lấy danh sách ShaftNum để giới hạn truy vấn
                    var shaftNums = validModels
                        .Select(x => x.ShaftNum)
                        .Distinct()
                        .ToList();

                    // Load tất cả record hiện có theo ShaftNum (1 query duy nhất)
                    var existingDataLogs = await dbContext.FT09_RevoDatalogs
                        .Where(x => x.ShaftNum.HasValue && shaftNums.Contains(x.ShaftNum))
                        .ToListAsync();

                    // Build dictionary để tra cứu O(1) theo bộ key (RevoId, StepId, ShaftNum)
                    var existingMap = existingDataLogs
                        .ToDictionary(x => (x.RevoId, x.StepId, x.ShaftNum));

                    var toInsert = new List<FT09_RevoDatalog>();

                    foreach (var item in validModels)
                    {
                        // Rule UPDATE:
                        // - Nếu TẤT CẢ step Enable=true đều có TotalRunTime != 0
                        //   -> coi như shaft đã "đầy đủ" data -> KHÔNG cho UPDATE (lock data lại).
                        // - Ngược lại (có ít nhất 1 step Enable=true mà TotalRunTime = 0)
                        //   -> shaft chưa đầy đủ, vẫn đang chạy -> CHO UPDATE.
                        // INSERT luôn cho phép (cần khởi tạo data cho shaft mới).
                        bool allEnabledNonZero = existingDataLogs
                            .All(s => (s.TotalTime ?? 0) != 0);

                        var steps = item.Steps.Where(x => x.Enable == true).ToList();

                        foreach (var itemStep in steps)
                        {
                            var key = ((int?)item.RevoId, (int?)itemStep.StepIndex, item.ShaftNum);

                            if (existingMap.TryGetValue(key, out var existing))
                            {
                                // ĐÃ CÓ -> chỉ UPDATE khi shaft CHƯA đầy đủ data
                                // (tức là vẫn còn step Enable=true đang ở 0).
                                if (allEnabledNonZero) continue;

                                existing.Work = item.Work;
                                existing.Part = item.Part;
                                existing.TotalTime = itemStep.TotalRunTime;
                                // existing đang được tracked -> EF tự detect change
                            }
                            else
                            {
                                // CHƯA CÓ -> Insert mới (không bị rule chặn)
                                toInsert.Add(new FT09_RevoDatalog
                                {
                                    Id = Guid.NewGuid(),
                                    CreatedAt = createdAt,
                                    CreatedMachine = createdMachine,
                                    RevoId = item.RevoId,
                                    RevoName = item.RevoName,
                                    Work = item.Work,
                                    Part = item.Part,
                                    ShaftNum = item.ShaftNum,
                                    StepId = itemStep.StepIndex,
                                    StepName = itemStep.StepName,
                                    TotalTime = itemStep.TotalRunTime
                                });
                            }
                        }
                    }

                    if (toInsert.Count > 0)
                    {
                        dbContext.FT09_RevoDatalogs.AddRange(toInsert);
                    }

                    // Chỉ 1 lần SaveChanges cho cả Insert + Update
                    await dbContext.SaveChangesAsync();

                    // Lưu hash sau khi save thành công.
                    // Đặt sau SaveChanges để nếu save lỗi thì lần sau sẽ retry (hash chưa đổi).
                    _lastLogStepRunHash = currentHash;
                }
            }
            finally
            {
                _logStepRunLock.Release();
            }
        }

        /// <summary>
        /// Build hash từ snapshot dữ liệu sẽ log vào FT09.
        /// Bao gồm các field thực sự được upsert: RevoId, StepId, ShaftNum, Work, Part, TotalRunTime.
        /// Nếu hash giữa 2 lần gọi giống nhau -> data không đổi -> bỏ qua DB write.
        /// </summary>
        private string BuildLogStepRunHash()
        {
            var sb = new StringBuilder();
            foreach (var item in GlobalVariable.RevoRealtimeModels)
            {
                if (!item.ShaftNum.HasValue || item.ShaftNum.Value == Guid.Empty) continue;

                foreach (var step in item.Steps.Where(x => x.Enable == true))
                {
                    sb.Append(item.RevoId).Append('|')
                      .Append(step.StepIndex).Append('|')
                      .Append(item.ShaftNum).Append('|')
                      .Append(item.Work).Append('|')
                      .Append(item.Part).Append('|')
                      .Append(step.TotalRunTime).Append(';');
                }
            }
            return sb.ToString();
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
                        var nl = new FT08_RevoRealtime()
                        {
                            Id = Guid.NewGuid(),
                            C000_RevoId = item.RevoId,
                            C001_Data = JsonConvert.SerializeObject(item)
                        };
                        dbContext.FT08_RevoRealtimes.Add(nl);
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

            // 1. Lấy giờ mới nhất
            var maxCreatedAt = dbContext.FT09_RevoDatalogs
                .Where(x => x.CreatedAt != null)
                .Max(x => x.CreatedAt);

            if (maxCreatedAt == null)
                return null;

            // 2. Chuẩn hóa giờ chẵn
            var currentHour = new DateTime(
                maxCreatedAt.Value.Year,
                maxCreatedAt.Value.Month,
                maxCreatedAt.Value.Day,
                maxCreatedAt.Value.Hour,
                0, 0);

            var nextHour = currentHour.AddHours(1);
            var lastHour = currentHour.AddHours(-1);

            // 3) Đếm theo giờ hiện tại:
            //    Group theo ShaftNum trong window giờ hiện tại
            //    và CHỈ giữ group mà TẤT CẢ các dòng đều có StartedAt & EndedAt khác null
            var currentCount = await dbContext.FT09_RevoDatalogs
                .Where(x => x.CreatedAt >= currentHour && x.CreatedAt < nextHour)
                .GroupBy(x => x.ShaftNum!)
                .Where(g => g.All(r => r.StartedAt != null && r.EndedAt != null))
                .Select(g => g.Key)
                .CountAsync();

            // 4) Đếm theo giờ trước đó:
            var lastCount = await dbContext.FT09_RevoDatalogs
                .Where(x => x.CreatedAt >= lastHour && x.CreatedAt < currentHour)
                .GroupBy(x => x.ShaftNum!)
                .Where(g => g.All(r => r.StartedAt != null && r.EndedAt != null))
                .Select(g => g.Key)
                .CountAsync();

            // 5. Kết quả
            var result = new GroupShaftModel
            {
                CurrentHour = currentCount,
                LastHour = lastCount
            };

            return result;
        }
        #endregion

        #region Events
        // Cho phép kéo form bằng panel
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnMaitenance_Click(object sender, EventArgs e)
        {
            using (var nf = new frmConfig())
            {
                nf.EasyDriverConnector = _easyDriverConnector;
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
