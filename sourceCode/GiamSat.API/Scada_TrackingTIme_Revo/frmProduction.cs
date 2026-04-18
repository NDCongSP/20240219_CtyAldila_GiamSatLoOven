using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using Scada_TrackingTIme_Revokd;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada_TrackingTIme_Revo
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

        private List<PartModel> _parts = new List<PartModel>();
        private List<MandrelModel> _mandrels = new List<MandrelModel>();
        private List<ColorModel> _colors = new List<ColorModel>();
        private List<PatternModel> _patterns = new List<PatternModel>();

        private bool PC_ALLOW_RUN_TO_PLC = false;
        private bool START_STOP_STEP = false;
        private bool SENT = false;

        private int _totalCurrentHour = 0, _totalLastHour = 0;

        private RevoGetTotalShaftCountDto _currentShaftCount = new RevoGetTotalShaftCountDto();

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
            this.Size = new Size(1350, 688);

            // Tạo panel làm thanh tiêu đề
            titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 40;
            titleBar.BackColor = Color.Black;
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            // Nút Close
            btnClose = new Button();
            btnClose.Text = "";
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.Black;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(this.Width - 40, 0);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            // 1) Gán icon từ Resources (đặt tên hình là "updateVersion" như trong Resource)
            btnClose.Image = Properties.Resources.close_window_30_white;  // PNG từ Resources
            btnClose.ImageAlign = ContentAlignment.MiddleCenter;  // căn giữa
            btnClose.Padding = new Padding(0);                    // tránh lệch
            btnClose.TextImageRelation = TextImageRelation.Overlay; // chỉ icon
            btnClose.Click += BtnClose_Click;
            titleBar.Controls.Add(btnClose);

            // Nút Maximize
            btnMaximize = new Button();
            btnMaximize.Text = "";
            btnMaximize.ForeColor = Color.White;
            btnMaximize.BackColor = Color.Black;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.Size = new Size(40, 40);
            btnMaximize.Location = new Point(this.Width - 80, 0);
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
            btnMinimize.Location = new Point(this.Width - 120, 0);
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
            btnMaintenance.Location = new Point(this.Width - 160, 0);
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
            btnClose.Size = btnMaximize.Size = btnMinimize.Size = btnMaintenance.Size = new Size(30, 30);


            // Anchor cho cả 3 nút
            btnClose.Anchor = btnMaximize.Anchor = btnMinimize.Anchor = btnMaintenance.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Logo
            PictureBox logo = new PictureBox();
            logo.Image = Properties.Resources.logoAldila; // logo từ Resources
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.Size = new Size(100, 30); // kích thước logo
            logo.Location = new Point(0, 5); // vị trí bên trái
            titleBar.Controls.Add(logo);

            // Text
            titleText = new Label();
            titleText.Text = $"REVO GOFT  PRODUCTION";
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
            FormClosing += frmProduction_FormClosing;
        }

        private void frmProduction_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _txtPart.KeyDown -= _txtPart_KeyDown;
                _easyDriverConnector.ConnectionStatusChaged -= _easyDriverConnector_ConnectionStatusChaged;
                _easyDriverConnector.Started -= _easyDriverConnector_Started;
                _easyDriverConnector.Stop();

                _timerCts.Cancel();
                _timerTask.Wait(1000);

                _checkingTimeStepCts.Cancel();
                _resetShaftCts.Cancel();
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
            }

        }

        private async void frmProduction_Load(object sender, EventArgs e)
        {
            GlobalVariable.RevoId = Properties.Settings.Default.RevoId;
            GlobalVariable.Part = Properties.Settings.Default.Part;
            //ddocj gias trij config
            using (var dbContext = new ApplicationDbContext())
            {
                var constring = dbContext.Database.Connection.ConnectionString;
                GlobalVariable.IpDbServer = constring.Split(';')[0].Split('=')[1];

                var ft07 = await dbContext.FT07_RevoConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft07 != null)
                {
                    GlobalVariable.RevoConfig = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000).FirstOrDefault(x => x.Id == GlobalVariable.RevoId);

                    if (!GlobalVariable.RevoConfig.Id.HasValue)
                    {
                        MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                            "CẢNH BÁO", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            );

                        return;
                    }

                    titleText.Text = $"Chương trình giám sát thời gian chạy - Máy {GlobalVariable.RevoConfig.Name}";
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
                GlobalVariable.RevoRealtimeModel = new RevoRealtimeModel()
                {
                    RevoId = GlobalVariable.RevoConfig.Id.Value,
                    RevoName = GlobalVariable.RevoConfig.Name,
                    Path = GlobalVariable.RevoConfig.Path,
                    Steps = new List<RevoStep>()
                };

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

                //khởi tạo panel chứa các step
                //flowMain.Dock = DockStyle.Fill;
                flowMain.FlowDirection = FlowDirection.TopDown;
                flowMain.WrapContents = true;      // CHO PHÉP CHẠY SANG CỘT
                flowMain.AutoScroll = false;       // KHÔNG DÙNG SCROLL
                flowMain.Dock = DockStyle.None;    // hoặc Fill nếu đủ rộng
                flowMain.Width = 1323;             // đủ rộng cho 3 cột, 1 cột 480 margin 5
                flowMain.Height = 580;              // cố định chiều cao, 10 step, 1 hàng 58

                //đọc data master từ access
                using (OleDbConnection conn = new OleDbConnection(GlobalVariable.RevoConfig.ConstringAccessDb))
                {
                    conn.Open();

                    DataTable dt = new DataTable();
                    string sql = $"SELECT * FROM Part";
                    OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                    da.Fill(dt);

                    _parts = GlobalVariable.ToModels<PartModel>(dt);

                    dt = new DataTable();
                    sql = $"SELECT * FROM Mandrel";
                    da = new OleDbDataAdapter(sql, conn);
                    da.Fill(dt);

                    _mandrels = GlobalVariable.ToModels<MandrelModel>(dt);

                    dt = new DataTable();
                    sql = $"SELECT * FROM Color";
                    da = new OleDbDataAdapter(sql, conn);
                    da.Fill(dt);

                    _colors = GlobalVariable.ToModels<ColorModel>(dt);

                    dt = new DataTable();
                    sql = $"SELECT * FROM Pattern";
                    da = new OleDbDataAdapter(sql, conn);
                    da.Fill(dt);

                    _patterns = GlobalVariable.ToModels<PatternModel>(dt);
                }

                //_txtPart.KeyDown += _txtPart_KeyDown;

                _btnStart.Click += _btnStart_Click;

                if (!string.IsNullOrEmpty(GlobalVariable.Part))
                {
                    _txtPart.Text = GlobalVariable.Part;

                    _txtPart_KeyDown(this, new KeyEventArgs(Keys.Enter));
                }


                _txtWork.KeyDown += (s, o) =>
                {
                    if (o.KeyCode == Keys.Enter)
                    {
                        var t = s as TextBox;
                        GlobalVariable.RevoRealtimeModel.Work = t.Text;
                    }
                };

                _timerCts = new CancellationTokenSource();
                _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

                _checkingTimeStepCts = new CancellationTokenSource();
                _ = TaskCheckTimeStepAsync(_checkingTimeStepCts.Token);
                _resetShaftCts = new CancellationTokenSource();
                _ = TaskResetShaftAsync(_resetShaftCts.Token);
            }

            this.KeyPreview = true;

            this.KeyDown += frmProduction_KeyDown;
        }

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

        private void _btnStart_Click(object sender, EventArgs e)
        {
            _txtPart_KeyDown(this, new KeyEventArgs(Keys.Enter));

            _txtWork.Focus();
        }

        private async void frmProduction_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                //MessageBox.Show("Nhấn mũi tên lên");

                var currentStep = GlobalVariable.RevoRealtimeModel.Steps
                              .FirstOrDefault(s =>
                                      s.Enable == true &&
                                      (s.StartAt == null || (s.StartAt.HasValue) && !s.EndAt.HasValue)
                                  );

                if (currentStep != null)
                {
                    currentStep.StartAt = null;
                    currentStep.EndAt = null;

                    //lấy bước trước đó ghi xuống để chạy lại
                    var previousStep = GlobalVariable.RevoRealtimeModel.Steps
                    .Where(s => s.Enable == true && s.StepIndex < currentStep.StepIndex)
                    .OrderByDescending(s => s.StepIndex)
                    .FirstOrDefault();

                    if (previousStep != null)
                    {
                        previousStep.StartAt = previousStep.EndAt = null;

                        Debug.WriteLine($"Previous step: {previousStep.StepIndex} - Speed:{previousStep.Speed_Hz} - Pul:{previousStep.SoLuongXung}");

                        await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                               , previousStep.Speed_Hz.HasValue ? previousStep.Speed_Hz.Value.ToString() : "0"
                                                                , WritePiority.High);

                        await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                             , previousStep.SoLuongXung.HasValue ? previousStep.SoLuongXung.Value.ToString() : "0"
                                                              , WritePiority.High);

                    Loop:
                        await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                            , "1"
                                                             , WritePiority.High);
                        if (!SENT)
                        {
                            goto Loop;
                        }

                        UpdateStepUI(currentStep, 2);
                        UpdateStepUI(previousStep, 1);
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    // MessageBox.Show("Nhấn mũi tên xuống");
                }
                else if (e.KeyCode == Keys.Left)
                {
                    //MessageBox.Show("Nhấn mũi tên trái");
                }
                else if (e.KeyCode == Keys.Right)
                {
                    //MessageBox.Show("Nhấn mũi tên phải");
                }
            }
        }

        private async void _txtPart_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    var part = _parts.FirstOrDefault(p => p.PN == _txtPart.Text.Trim());

                    if (part == null)
                    {
                        MessageBox.Show("Không tìm thấy mã hàng trong database. Vui lòng kiểm tra lại.", "CẢNH BÁO",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        return;
                    }

                    //Luu path va part vao bien toan cuc de su dung sau nay
                    if (GlobalVariable.Part != part.PN)
                    {
                        GlobalVariable.Part = part.PN;

                        Properties.Settings.Default.Part = GlobalVariable.Part;
                        Properties.Settings.Default.Save();
                    }

                    var steps = GlobalVariable.BuildSteps(part);

                    GlobalVariable.RevoRealtimeModel = new RevoRealtimeModel()
                    {
                        RevoId = GlobalVariable.RevoConfig.Id.Value,
                        RevoName = GlobalVariable.RevoConfig.Name,
                        Path = GlobalVariable.RevoConfig.Path,
                        Part = part.PN,
                        Rev = part.Blank_Rev,
                        ColorCode = part.Flex_Color,
                        Mandrel = _mandrels.FirstOrDefault(x => x.ID == part.Mandrel)?.PN,
                        MandrelStart = part.Mandrel_Start.ToString(),
                        Steps = steps,
                        ShaftNum = Guid.NewGuid()
                    };

                    //taojj ui step lan dau tien
                    LoadStepsToFlowPanel();

                    LogDb(isNew: true);

                    //ghi step đầu tiên xuống plc
                    var currentStep = GlobalVariable.RevoRealtimeModel.Steps
                   .FirstOrDefault(s =>
                       s.Enable == true &&
                       !s.StartAt.HasValue
                   );

                    if (currentStep == null)
                    {
                        MessageBox.Show($"Part {GlobalVariable.Part} khong co thong tin step.");
                        Log.Warning($"Part {GlobalVariable.Part} khong co thong tin step.");
                        return;
                    }

                Loop1:
                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/PC_ALLOW_RUN_TO_PLC"
                                                                 , "1"
                                                                  , WritePiority.High);
                    if (!PC_ALLOW_RUN_TO_PLC)
                    {
                        goto Loop1;
                    }

                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                            , currentStep?.Speed_Hz.ToString() ?? "0"
                                                             , WritePiority.High);

                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                           , currentStep.SoLuongXung.Value.ToString() ?? "0"
                                                            , WritePiority.High);

                Loop2:
                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                           , "1"
                                                            , WritePiority.High);

                    if (!SENT)
                    {
                        goto Loop2;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Lỗi khi nhập mã hàng: {ex.Message}");
            }
        }
        #endregion


        #region Tasks
        private async Task TaskTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var totalShaft = await GetTotalShaftAsync();
                    _totalCurrentHour = totalShaft?.TotalShaftFinishCurrentHour ?? 0;
                    _totalLastHour = totalShaft?.TotalShaftFinshLastHour ?? 0;

                    //var ping = PingServer(GlobalVariable.IpDbServer);
                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        _labRev.Text = GlobalVariable.RevoRealtimeModel.Rev;
                        _labTotalShaftCurrentHour.Text = _totalCurrentHour.ToString();
                        _labTotalShaftLastHour.Text = _totalLastHour.ToString();
                        _labShaftLastHour.Text = $"Total Shafts ({DateTime.Now.Hour - 1}:00–{DateTime.Now.Hour - 1}:59)";
                        _labMandrel.Text = GlobalVariable.RevoRealtimeModel.Mandrel;
                        _labMandrelStart.Text = GlobalVariable.RevoRealtimeModel.MandrelStart;

                        //_labStatus.BackColor = GetConnectionStatusColor(_easyStatus);

                        _labStatus.Text = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} - Easy Driver: {_easyStatus} - PLC: {GlobalVariable.RevoRealtimeModel.PlcConnected} - DB Server: - Save type: {GlobalVariable.RevoConfig.SaveMode.ToString()}";
                    });


                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    Log.Error($"Lỗi trong TaskTimerAsync: {ex.Message}");
                }
                await Task.Delay(100, token); // Chờ 1 giây trước khi lặp lại
            }
        }
        string PingServer(string host)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(host, 3000); // timeout 3s

                return reply.Status == IPStatus.Success ? "Good" : "Bad";
            }
            catch
            {
                return "Bad";
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

                    if (GlobalVariable.RevoRealtimeModel.Steps?.Count > 0)
                    {
                        var currentStep = GlobalVariable.RevoRealtimeModel.Steps
                                .FirstOrDefault(s =>
                                    s.Enable == true &&
                                    (s.StartAt == null || (s.StartAt.HasValue) && !s.EndAt.HasValue)
                                );
                        if (START_STOP_STEP)
                        {
                            //bắt đầu step
                            if (currentStep != null && !currentStep.StartAt.HasValue && !currentStep.EndAt.HasValue)
                            {
                                currentStep.StartAt = DateTime.Now;
                                Log.Information($"Bắt đầu step {currentStep.StepIndex} - {currentStep.StepName} lúc {currentStep.StartAt}");
                                Debug.WriteLine($"Bắt đầu step {currentStep.StepIndex} - {currentStep.StepName} lúc {currentStep.StartAt}");
                            }
                        }
                        else
                        {
                            //kết thúc step
                            if (currentStep != null && currentStep.StartAt.HasValue && !currentStep.EndAt.HasValue)
                            {
                                currentStep.EndAt = DateTime.Now;
                                currentStep.TotalRunTime = Math.Round((double)((currentStep.EndAt - currentStep.StartAt)?.TotalSeconds), 2);

                                Log.Information($"Kết thúc step {currentStep.StepIndex} - {currentStep.StepName} lúc {currentStep.EndAt}");
                                Debug.WriteLine($"Kết thúc step {currentStep.StepIndex} - {currentStep.StepName} lúc {currentStep.EndAt}");

                                //ghi xuống step tiếp theo nếu chưa  chạy hết bước. Nếu chạy hết bước thì rết step chạy lại bước đầu tiên
                                var nextStep = GlobalVariable.RevoRealtimeModel.Steps
                                        .FirstOrDefault(s =>
                                            s.Enable == true &&
                                            s.StartAt == null
                                        );

                                if (nextStep != null)
                                {
                                    Debug.WriteLine($"step: {nextStep.StepIndex} - Speed:{nextStep.Speed_Hz} - Pul:{nextStep.SoLuongXung}");

                                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                                           , nextStep.Speed_Hz.HasValue ? nextStep.Speed_Hz.Value.ToString() : "0"
                                                                            , WritePiority.High);

                                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                                         , nextStep.SoLuongXung.HasValue ? nextStep.SoLuongXung.Value.ToString() : "0"
                                                                          , WritePiority.High);

                                Loop:
                                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                                        , "1"
                                                                         , WritePiority.High);
                                    if (!SENT)
                                    {
                                        goto Loop;
                                    }

                                    UpdateStepUI(nextStep, 1);
                                }
                                else//da xong quy trinh.
                                {
                                    _triggerResetShaft.Set();
                                }
                            }
                        }

                        //Log data
                        LogDb();

                        //cập nhật lại giao diện
                        UpdateStepUI(currentStep);
                    }
                    // xong việc → quay lại vòng chờ (không cần Delay hay poll)
                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
        }

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

                    await Task.Delay(GlobalVariable.RevoConfig.IntervalResetShaft, token); // Chờ 1 giây trước khi lặp lại

                    GlobalVariable.RevoRealtimeModel.Steps.ForEach(x =>
                    {
                        x.StartAt = null;
                        x.EndAt = null;
                        x.TotalRunTime = 0;
                    });
                    GlobalVariable.RevoRealtimeModel.ShaftNum = Guid.NewGuid();

                    LoadStepsToFlowPanel();

                    var step = GlobalVariable.RevoRealtimeModel.Steps
                        .FirstOrDefault(s =>
                            s.Enable == true &&
                            s.StartAt == null
                        );

                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                          , step.Speed_Hz.HasValue ? step.Speed_Hz.Value.ToString() : "0"
                                                           , WritePiority.High);

                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                           , step.SoLuongXung.HasValue ? step.SoLuongXung.Value.ToString() : "0"
                                                            , WritePiority.High);

                Loop:
                    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                        , "1"
                                                         , WritePiority.High);
                    if (!SENT)
                    {
                        goto Loop;
                    }

                    UpdateStepUI(step, 1);

                    LogDb(isNew: true);
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

            //GlobalVariable.InvokeIfRequired(this, () =>
            //{
            //    _labSriverStatus.BackColor = GetConnectionStatusColor(e.NewStatus);
            //    _labSriverStatus.Text = $"TT kết nối easy driver: {_easyDriverConnector.ConnectionStatus.ToString()}";
            //});
        }

        private void _easyDriverConnector_Started(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(2000);

            _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ").QualityChanged += TOC_DO_HZ_QualityChanged;
            _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP").ValueChanged += START_STOP_STEP_ValueChanged;
            _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/PC_ALLOW_RUN_TO_PLC").ValueChanged += PC_ALLOW_RUN_TO_PLC_ValueChanged;
            _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/SENT").ValueChanged += SENT_ValueChanged;

            START_STOP_STEP_ValueChanged(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP")
                , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP")
                , "", _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP").Value));
            PC_ALLOW_RUN_TO_PLC_ValueChanged(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/PC_ALLOW_RUN_TO_PLC")
               , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/PC_ALLOW_RUN_TO_PLC")
               , "", _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/PC_ALLOW_RUN_TO_PLC").Value));
            SENT_ValueChanged(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/SENT")
               , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/SENT")
               , "", _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/SENT").Value));
        }

        private void SENT_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            SENT = e.NewValue == "1" ? true : false;
        }

        private void TOC_DO_HZ_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            GlobalVariable.RevoRealtimeModel.PlcConnected = e.NewQuality == Quality.Good ? true : false;
        }

        private void START_STOP_STEP_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            START_STOP_STEP = e.NewValue == "1" ? true : false;
            GlobalVariable.RevoRealtimeModel.PlcConnected = e.Tag.Quality == Quality.Good ? true : false;
            Debug.WriteLine($"START_STOP_STEP = {START_STOP_STEP}");

            //bât trigger chay để log thời gian bắt đầu hoặc kết thúc của step tương ứng.
            _triggerCheckTimeStep.Set();
        }

        private void PC_ALLOW_RUN_TO_PLC_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            PC_ALLOW_RUN_TO_PLC = e.NewValue == "1" ? true : false;
            GlobalVariable.RevoRealtimeModel.PlcConnected = e.Tag.Quality == Quality.Good ? true : false;

            GlobalVariable.InvokeIfRequired(this, () =>
            {
                //_btnStartStop.BackColor = PC_ALLOW_RUN_TO_PLC ? Color.Green : Color.Gray;
                //_btnStartStop.Text = PC_ALLOW_RUN_TO_PLC ? "RUN" : "STOP";
            });
        }
        #endregion

        #region Method helper
        private void LoadStepsToFlowPanel()
        {
            flowMain.Controls.Clear();

            int stepsPerColumn = 10;
            int count = 0;

            foreach (var step in GlobalVariable.RevoRealtimeModel.Steps)
            {
                if (count > 0 && count % stepsPerColumn == 0)
                {
                    flowMain.Controls.Add(CreateColumnSpacer(40));
                }

                Color c = (bool)step.Enable ? Color.LightBlue : Color.Black;

                Panel row = CreateStepRow(step);

                flowMain.Controls.Add(row);

                count++;
            }

            //lấy step đầu tiên để cập nhật giao diện ngay khi load xong
            var stepF = GlobalVariable.RevoRealtimeModel
                .Steps
                .FirstOrDefault(x => x.Enable == true);

            UpdateStepUI(stepF, 1);
        }
        private Panel CreateStepRow(RevoStep step)
        {
            Panel row = new Panel();
            row.Width = 660;
            row.Height = 50;
            row.Margin = new Padding(5, 0, 5, 0);

            row.Tag = step.StepIndex; // ← KEY

            // Số thứ tự
            Label lblIndex = new Label();
            lblIndex.Name = "lblIndex";
            lblIndex.Text = step.StepIndex.ToString();
            lblIndex.Width = 50;
            lblIndex.Height = 50;
            lblIndex.TextAlign = ContentAlignment.MiddleCenter;
            lblIndex.Location = new Point(0, 3);
            lblIndex.Font = new Font("Microsoft Sans Serif", 15, FontStyle.Bold);
            lblIndex.BackColor = (bool)step.Enable ? Color.LightBlue : Color.Black;
            lblIndex.ForeColor = (bool)step.Enable ? Color.Black : Color.White;

            // Thanh REVO
            Label lblStep = new Label();
            lblStep.Name = "lblStep";
            lblStep.Text = $"{step.StepName} - {step.StartAt} -> {step.EndAt}: {step.TotalRunTime}s{Environment.NewLine}Pul={step.SoLuongXung} - Speed = {step.Speed_Hz}";
            lblStep.Width = 610;
            lblStep.Height = 50;
            lblStep.TextAlign = ContentAlignment.MiddleCenter;
            lblStep.Location = new Point(60, 3);
            lblStep.BackColor = (bool)step.Enable ? Color.LightBlue : Color.Black;
            lblStep.ForeColor = (bool)step.Enable ? Color.Black : Color.White;
            lblStep.TextAlign = ContentAlignment.TopLeft;
            lblStep.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Regular);

            row.Controls.Add(lblIndex);
            row.Controls.Add(lblStep);

            return row;
        }

        private Panel CreateColumnSpacer(int width)
        {
            return new Panel
            {
                Width = width,                 // KHOẢNG CÁCH GIỮA 2 CỘT
                Height = flowMain.Height,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
        }

        /// <summary>
        /// cập nhật UI.
        /// </summary>
        /// <param name="step">Chứa thông tin bước chạy. để lấy index ggeer lấy ra đúng control.</param>
        /// <param name="isFirst">0-nextStep; 1-new; 2-previous step.</param>
        public void UpdateStepUI(RevoStep step, int isFirst = 0)
        {
            foreach (Control ctrl in flowMain.Controls)
            {
                if (ctrl is Panel row && row.Tag is int idx && idx == step?.StepIndex)
                {
                    Label lblIndex = row.Controls["lblIndex"] as Label;
                    Label lblStep = row.Controls["lblStep"] as Label;

                    lblIndex.Text = step.StepIndex.ToString();

                    var startAtText = step.StartAt.HasValue ? ((DateTime)step.StartAt).ToString("HH:mm:ss") : "null";
                    var endAtText = step.EndAt.HasValue ? ((DateTime)step.EndAt).ToString("HH:mm:ss") : "null";

                    lblStep.Text = $"{step.StepName} - {startAtText} -> {endAtText}: {step.TotalRunTime}s" +
                        $"{Environment.NewLine}" +
                        $"Pul={step.SoLuongXung}({step.SoLuongXung/GlobalVariable.RevoConfig.Pulse_Rev} v) " +
                        $"- Speed = {step.Speed_Hz} ({step.Speed_Hz/GlobalVariable.RevoConfig.Pulse_Rev}v/s)";

                    if ((step.StartAt.HasValue && !step.EndAt.HasValue) || isFirst == 1)
                    {
                        lblStep.BackColor = Color.Red;
                        lblStep.ForeColor = Color.White;

                        lblIndex.BackColor = Color.Red;
                        lblIndex.ForeColor = Color.White;
                    }
                    else if (step.StartAt.HasValue && step.EndAt.HasValue)
                    {
                        lblStep.BackColor = Color.Gray;
                        lblStep.ForeColor = Color.White;

                        lblIndex.BackColor = Color.Gray;
                        lblIndex.ForeColor = Color.White;
                    }
                    else if (isFirst == 2)
                    {
                        lblStep.BackColor = Color.LightBlue;
                        lblStep.ForeColor = Color.Black;

                        lblIndex.BackColor = Color.LightBlue;
                        lblIndex.ForeColor = Color.Black;
                    }

                    break;
                }
            }
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
                #region Data log
                var createdAt = DateTime.Now;
                var createdMachine = Environment.MachineName;

                if (isNew)
                {
                    var dataLogs = new List<FT09_RevoDatalog>();


                    foreach (var item in GlobalVariable.RevoRealtimeModel.Steps.Where(x => x.Enable == true))
                    {
                        var nl = new FT09_RevoDatalog()
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = createdAt,
                            CreatedMachine = createdMachine,
                            RevoId = GlobalVariable.RevoRealtimeModel.RevoId,
                            RevoName = GlobalVariable.RevoRealtimeModel.RevoName,
                            Work = GlobalVariable.RevoRealtimeModel.Work,
                            Part = GlobalVariable.RevoRealtimeModel.Part,
                            Rev = GlobalVariable.RevoRealtimeModel.Rev,
                            ColorCode = GlobalVariable.RevoRealtimeModel.ColorCode,
                            Mandrel = GlobalVariable.RevoRealtimeModel.Mandrel,
                            MandrelStart = GlobalVariable.RevoRealtimeModel.MandrelStart,
                            ShaftNum = GlobalVariable.RevoRealtimeModel.ShaftNum,
                            StepId = item.StepIndex,
                            StepName = item.StepName,
                        };

                        dataLogs.Add(nl);
                    }

                    dbContext.FT09_RevoDatalogs.AddRange(dataLogs);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    var dataLogUpdate = await dbContext.FT09_RevoDatalogs
                        .Where(x =>
                            x.ShaftNum == GlobalVariable.RevoRealtimeModel.ShaftNum &&
                            x.RevoId == GlobalVariable.RevoRealtimeModel.RevoId
                        )
                        .ToListAsync();

                    if (dataLogUpdate != null && dataLogUpdate.Count > 0)
                    {
                        foreach (var item in dataLogUpdate)
                        {
                            var lineUpdate = GlobalVariable.RevoRealtimeModel.Steps.FirstOrDefault(x => x.StepIndex == item.StepId);

                            item.StartedAt = lineUpdate?.StartAt;
                            item.EndedAt = lineUpdate?.EndAt;
                            item.StartedAt = lineUpdate?.StartAt;
                            item.TotalTime = lineUpdate?.TotalRunTime;
                            item.Work = GlobalVariable.RevoRealtimeModel.Work;
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }
                #endregion

                #region realtime log  
                var rl = await dbContext.FT08_RevoRealtimes.FirstOrDefaultAsync(x => x.C000_RevoId == GlobalVariable.RevoId);

                if (rl != null)
                {
                    rl.C001_Data = JsonConvert.SerializeObject(GlobalVariable.RevoRealtimeModel);
                }
                else
                {
                    var nl = new FT08_RevoRealtime()
                    {
                        Id = Guid.NewGuid(),
                        C000_RevoId = GlobalVariable.RevoId,
                        C001_Data = JsonConvert.SerializeObject(GlobalVariable.RevoRealtimeModel)
                    };

                    dbContext.FT08_RevoRealtimes.Add(nl);
                }
                #endregion

                await dbContext.SaveChangesAsync();
            }
        }

        private async Task<RevoGetTotalShaftCountDto> GetTotalShaftAsync()
        {
            using var dbContext = new ApplicationDbContext();

            var data =await dbContext.Database
                .SqlQuery<RevoGetTotalShaftCountDto>($"EXEC sp_GetTotalShaft @RevoId = {GlobalVariable.RevoConfig.Id}")
                .FirstOrDefaultAsync();

            return data;
        }
        #endregion
    }
}
