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

        private List<PartModel> _parts = new List<PartModel>();
        private List<MandrelModel> _mandrels = new List<MandrelModel>();
        private List<ColorModel> _colors = new List<ColorModel>();
        private List<PatternModel> _patterns = new List<PatternModel>();

        private bool PC_ALLOW_RUN_TO_PLC = false;
        private bool START_STOP_STEP = false;
        private bool SENT = false;

        private int _totalCurrentHour = 0, _totalLastHour = 0;

        private List<AutoRollingTagChangedModel> _tagsValueRealtime = new List<AutoRollingTagChangedModel>();

        //// Giả sử bạn muốn truyền thông tin Device hoặc Path
        //public class ResetArgs
        //{
        //    public string Path { get; set; }
        //    public int StepValue { get; set; }
        //}
        //private readonly ConcurrentQueue<ResetArgs> _resetQueue = new ConcurrentQueue<ResetArgs>();
        private readonly ConcurrentQueue<string> _resetShaftQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> _resetPartQueue = new ConcurrentQueue<string>();


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
            this.Size = new Size(845, 514);

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
            titleText.Text = $"AUTO ROLLING  PRODUCTION";
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

                //khởi tạo các step mặc định (15 step) đối với máy auto rolling
                var steps = new List<RevoStep>();
                for (int i = 1; i <= 15; i++)
                {
                    var step = new RevoStep()
                    {
                        StepIndex = i
                    };

                    steps.Add(step);
                }


                //khởi tạo model realtime data
                foreach (var item in GlobalVariable.RevoConfigs)
                {

                    GlobalVariable.RevoRealtimeModels.Add(new RevoRealtimeModel()
                    {
                        RevoId = item.Id.Value,
                        RevoName = item.Name,
                        Path = item.Path,
                        Steps = steps
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

                _checkingTimeStepCts = new CancellationTokenSource();
                _ = TaskCheckTimeStepAsync(_checkingTimeStepCts.Token);
                _resetShaftCts = new CancellationTokenSource();
                _ = TaskResetShaftAsync(_resetShaftCts.Token);

                _resetPartCts = new CancellationTokenSource();
                _ = TaskResetPartAsync(_resetPartCts.Token);
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

                    #region Log data vào DB
                    //LogDb();
                    #endregion
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    //Log.Error($"Lỗi trong TaskTimerAsync: {ex.Message}");
                }
                await Task.Delay(200, token); // Chờ 1 giây trước khi lặp lại
            }
        }

        private async Task TaskCheckTimeStepAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    // chờ sự kiện → không tốn CPU, không Sleep
                    await _triggerCheckTimeStep.WaitAsync(token);

                    // === xử lý 1 lần duy nhất mỗi lần được kích ===
                    Debug.WriteLine($"TaskCheckTimeStepAsync triggered {START_STOP_STEP}");


                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
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
                    Debug.WriteLine($"TaskCheckTimeStepAsync triggered {START_STOP_STEP}");

                    // Lấy tất cả các Path đang có trong hàng đợi ra xử lý (đề phòng kích hoạt dồn dập)
                    while (_resetShaftQueue.TryDequeue(out string currentPath))
                    {
                        // Tìm item trong list của bạn dựa trên Path
                        var targetItem = _tagsValueRealtime.FirstOrDefault(x => x.Path == currentPath);

                        if (targetItem != null)
                        {
                            Debug.WriteLine($"Processing reset for Path: {currentPath}");

                            var realtimeItem = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.Path == currentPath);

                            realtimeItem.Steps.ForEach(s => s.TotalRunTime = 0);
                            realtimeItem.ShaftNum = Guid.NewGuid();

                            LogDb(isNew: true);

                            // Ví dụ: cập nhật một thuộc tính nào đó của targetItem
                            // targetItem.IsResetting = true; 
                        }
                    }
                    // xong việc → quay lại vòng chờ (không cần Delay hay poll)
                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
        }

        /// <summary>
        /// Task xử lý khi có sự kiện reset part được kích hoạt. Mỗi khi có sự kiện này.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task TaskResetPartAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    // chờ sự kiện → không tốn CPU, không Sleep
                    await _triggerResetShaft.WaitAsync(token);

                    // === xử lý 1 lần duy nhất mỗi lần được kích ===
                    Debug.WriteLine($"TaskCheckTimeStepAsync triggered {START_STOP_STEP}");

                    // Lấy tất cả các Path đang có trong hàng đợi ra xử lý (đề phòng kích hoạt dồn dập)
                    while (_resetPartQueue.TryDequeue(out string currentPath))
                    {
                        // Tìm item trong list của bạn dựa trên Path
                        var targetItem = _tagsValueRealtime.FirstOrDefault(x => x.Path == currentPath);

                        if (targetItem != null)
                        {
                            Debug.WriteLine($"Processing reset for Path: {currentPath}");

                            var steps = GlobalVariable.BuildSteps(targetItem);

                            var realtimeItem = GlobalVariable.RevoRealtimeModels.FirstOrDefault(x => x.Path == currentPath);

                            realtimeItem.Part = targetItem.Part_Name;
                            realtimeItem.Steps = steps;
                            realtimeItem.ShaftNum = Guid.NewGuid();

                            LogDb(isNew: true);

                            // Ví dụ: cập nhật một thuộc tính nào đó của targetItem
                            // targetItem.IsResetting = true; 
                        }
                    }
                    // xong việc → quay lại vòng chờ (không cần Delay hay poll)
                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
            catch (Exception ex) { Log.Error(ex, "Error in Reset Task"); }
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
                        TimeRunStep_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}")
                           , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}")
                           , "", _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}").Value));

                        Part_Name_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Part_Name{i}")
                         , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Part_Name{i}")
                         , "", _easyDriverConnector.GetTag($"{item.Path}/Part_Name{i}").Value));
                    }
                    else
                    {
                        TimeRunStep_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}")
                           , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}")
                           , "", _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep{i}").Value));
                    }
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

                        // Đưa Path vào hàng đợi để Task lấy ra dùng
                        _resetShaftQueue.Enqueue(item.Path);
                        _triggerResetShaft.Set(); // kích hoạt _triggerResetShaft chạy

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
                            // Đưa Path vào hàng đợi để Task lấy ra dùng
                            _resetShaftQueue.Enqueue(item.Path);
                            _triggerResetShaft.Set(); // kích hoạt _triggerResetShaft chạy
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
                        switch (tagName)
                        {
                            case "TimeRunStep1":
                                item.TimeRunStep1 = timeRunStep;
                                break;
                            case "TimeRunStep2":
                                item.TimeRunStep2 = timeRunStep;
                                break;
                            case "TimeRunStep3":
                                item.TimeRunStep3 = timeRunStep;
                                break;
                            case "TimeRunStep4":
                                item.TimeRunStep4 = timeRunStep;
                                break;
                            case "TimeRunStep5":
                                item.TimeRunStep5 = timeRunStep;
                                break;
                            case "TimeRunStep6":
                                item.TimeRunStep6 = timeRunStep;
                                break;
                            case "TimeRunStep7":
                                item.TimeRunStep7 = timeRunStep;
                                break;
                            case "TimeRunStep8":
                                item.TimeRunStep8 = timeRunStep;
                                break;
                            case "TimeRunStep9":
                                item.TimeRunStep9 = timeRunStep;
                                break;
                            case "TimeRunStep10":
                                item.TimeRunStep10 = timeRunStep;
                                break;
                            case "TimeRunStep11":
                                item.TimeRunStep11 = timeRunStep;
                                break;
                            case "TimeRunStep12":
                                item.TimeRunStep12 = timeRunStep;
                                break;
                            case "TimeRunStep13":
                                item.TimeRunStep13 = timeRunStep;
                                break;
                            case "TimeRunStep14":
                                item.TimeRunStep14 = timeRunStep;
                                break;
                            case "TimeRunStep15":
                                item.TimeRunStep15 = timeRunStep;
                                break;
                            default:
                                // code block
                                break;
                        }

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

                        GetCharFromInt(item.RevoId);

                        // Đưa Path vào hàng đợi để Task lấy ra dùng
                        _resetPartQueue.Enqueue(item.Path);
                        _triggerResetPart.Set(); // kích hoạt _triggerResetPart chạy

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
        private void GetCharFromInt(int revoId)
        {
            var machine = _tagsValueRealtime.FirstOrDefault(t => t.RevoId == revoId);
            // Tạo danh sách các giá trị từ Part_Name1 đến Part_Name8
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

            // Gán kết quả cuối cùng vào model (vì dùng ref nên bên ngoài sẽ nhận được luôn)
            machine.Part_Name = fullPartName.Trim();
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


        private System.Drawing.Color GetConnectionStatusColor(ConnectionStatus status)
        {
            switch (status)
            {
                case ConnectionStatus.Connected:
                    return System.Drawing.Color.Lime;
                case ConnectionStatus.Connecting:
                case ConnectionStatus.Reconnecting:
                    return System.Drawing.Color.Orange;
                case ConnectionStatus.Disconnected:
                    return System.Drawing.Color.Red;
                default:
                    return System.Drawing.Color.White;
            }

        }

        public async void LogDb(bool isNew = false)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var createdAt = DateTime.Now;
                var createdMachine = Environment.MachineName;

                #region realtime log  
                var listIds = GlobalVariable.RevoConfigs.Select(x => x.Id).Distinct().ToList();
                var dbRecords = await dbContext.FT08_RevoRealtimes
                    .Where(x => listIds.Contains(x.C000_RevoId.Value))
                    .ToListAsync();

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
                #endregion

                #region Data log
                if (isNew)
                {
                    var dataLogs = new List<FT09_RevoDatalog>();

                    foreach (var item in GlobalVariable.RevoRealtimeModels)
                    {
                        var nl = new FT09_RevoDatalog()
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = createdAt,
                            CreatedMachine = createdMachine,
                            //RevoId = GlobalVariable.RevoRealtimeModels.RevoId,
                            //RevoName = GlobalVariable.RevoRealtimeModels.RevoName,
                            //Work = GlobalVariable.RevoRealtimeModels.Work,
                            //Part = GlobalVariable.RevoRealtimeModels.Part,
                            //Rev = GlobalVariable.RevoRealtimeModels.Rev,
                            //ColorCode = GlobalVariable.RevoRealtimeModels.ColorCode,
                            //Mandrel = GlobalVariable.RevoRealtimeModels.Mandrel,
                            //MandrelStart = GlobalVariable.RevoRealtimeModels.MandrelStart,
                            //ShaftNum = GlobalVariable.RevoRealtimeModels.ShaftNum,
                            //StepId = item.StepIndex,
                            //StepName = item.StepName,
                        };

                        dataLogs.Add(nl);
                    }

                    dbContext.FT09_RevoDatalogs.AddRange(dataLogs);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    //var groups = GlobalVariable.RevoRealtimeModels
                    //    .GroupBy(g=> new { g.ShaftNum, g.RevoId })
                    //    .Select(s => new { s.Key.ShaftNum, s.Key.RevoId })
                    //    .ToList();
                    //var existSet = new HashSet<(string OcNum, string ProductCode, string BoxCode, string Unit)>(
                    // existListFT403.Select(x => (x.OcNum, x.ProductCode, x.BoxCode, x.Unit)));

                    //// 6. Tách riêng 2 luồng xử lý để loại bỏ những thùng đã đưuọc tạo order.
                    //var dataRequest = boxSkuAvailable
                    //    .Where(req => !existSet.Contains((req.C000_OrderNumber, req.C012_ProductCode, req.C001_BoxCode, req.C014_Unit)))
                    //    .ToList();

                    //var dataLogUpdate = await dbContext.FT09_RevoDatalogs
                    //    .Where(x =>
                    //        //x.ShaftNum == GlobalVariable.RevoRealtimeModels.ShaftNum &&
                    //        //x.RevoId == GlobalVariable.RevoRealtimeModels.RevoId
                    //        groups.Contains(x.ShaftNum , x.RevoId)
                    //    )
                    //    .ToListAsync();

                    //if (dataLogUpdate != null && dataLogUpdate.Count > 0)
                    //{
                    //    foreach (var item in dataLogUpdate)
                    //    {
                    //        var lineUpdate = GlobalVariable.RevoRealtimeModels.Steps.FirstOrDefault(x => x.StepIndex == item.StepId);

                    //        item.StartedAt = lineUpdate?.StartAt;
                    //        item.EndedAt = lineUpdate?.EndAt;
                    //        item.StartedAt = lineUpdate?.StartAt;
                    //        item.TotalTime = lineUpdate?.TotalRunTime;
                    //        item.Work = GlobalVariable.RevoRealtimeModels.Work;
                    //    }

                    //    await dbContext.SaveChangesAsync();
                    //}
                }
                #endregion

                await dbContext.SaveChangesAsync();
            }
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
