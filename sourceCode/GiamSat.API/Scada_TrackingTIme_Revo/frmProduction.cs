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


        private List<PartModel> _parts = new List<PartModel>();
        private List<MandrelModel> _mandrels = new List<MandrelModel>();
        private List<ColorModel> _colors = new List<ColorModel>();
        private List<PatternModel> _patterns = new List<PatternModel>();

        private bool PC_ALLOW_RUN_TO_PLC = false;
        private bool START_STOP_STEP = false;

        public frmProduction()
        {
            InitializeComponent();

            Load += frmProduction_Load;
            FormClosing += frmProduction_FormClosing;
        }

        private void frmProduction_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _txtPart.KeyDown += _txtPart_KeyDown;
                _easyDriverConnector.ConnectionStatusChaged -= _easyDriverConnector_ConnectionStatusChaged;
                _easyDriverConnector.Started -= _easyDriverConnector_Started;
                _easyDriverConnector.Stop();

                _timerCts.Cancel();
                _timerTask.Wait(1000);

                _checkingTimeStepCts.Cancel();
            }
            catch
            {

            }
            finally
            {
                _timerCts?.Dispose();
                _timerTask = null;

                _checkingTimeStepCts?.Dispose();
            }

        }

        private async void frmProduction_Load(object sender, EventArgs e)
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
                    GlobalVariable.RevoConfig = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000).FirstOrDefault(x => x.Id == GlobalVariable.RevoId);

                    if (!GlobalVariable.RevoConfig.Id.HasValue)
                    {
                        MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                            "CẢNH BÁO", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            );

                        return;
                    }

                    Text = $"Chương trình giám sát thời gian chạy - Máy {GlobalVariable.RevoConfig.Name}";
                }
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
            DataTable dt = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(GlobalVariable.RevoConfig.ConstringAccessDb))
            {
                conn.Open();

                string sql = $"SELECT * FROM Part";
                OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                da.Fill(dt);

                _parts = GlobalVariable.ToModels<PartModel>(dt);

                sql = $"SELECT * FROM Mandrel";
                da = new OleDbDataAdapter(sql, conn);
                da.Fill(dt);

                _mandrels = GlobalVariable.ToModels<MandrelModel>(dt);

                sql = $"SELECT * FROM Color";
                da = new OleDbDataAdapter(sql, conn);
                da.Fill(dt);

                _colors = GlobalVariable.ToModels<ColorModel>(dt);

                sql = $"SELECT * FROM Pattern";
                da = new OleDbDataAdapter(sql, conn);
                da.Fill(dt);

                _patterns = GlobalVariable.ToModels<PatternModel>(dt);
            }

            _txtPart.KeyDown += _txtPart_KeyDown;
            _txtWork.KeyDown += (s, o) =>
            {
                if (o.KeyCode == Keys.Enter)
                {
                    GlobalVariable.RevoRealtimeModel.Work = s.ToString();
                }
            };

            _timerCts = new CancellationTokenSource();
            _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

            _checkingTimeStepCts = new CancellationTokenSource();
            _ = TaskCheckTimeStepAsync(_checkingTimeStepCts.Token);
        }

        private void _btnStartStop_Click(object sender, EventArgs e)
        {
            var newValue = PC_ALLOW_RUN_TO_PLC ? "0" : "1";
            _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/PC_ALLOW_RUN_TO_PLC"
                                                        , newValue
                                                         , WritePiority.High);

            if (!PC_ALLOW_RUN_TO_PLC)//nếu đang ở trạng thái dừng thì cho phép chạy, và lấy thông tin bước đang chạy hiện tại để nạp xuống lại PLC
            {

            }
        }

        private void _txtPart_KeyDown(object sender, KeyEventArgs e)
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
                        Steps = steps
                    };

                    //taojj ui step lan dau tien
                    LoadStepsToFlowPanel();

                    //ghi step đầu tiên xuống plc
                    var currentStep = GlobalVariable.RevoRealtimeModel.Steps
                   .FirstOrDefault(s =>
                       s.Enable == true &&
                       !s.StartAt.HasValue
                   );

                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                          , currentStep?.Speed_Hz.ToString() ?? "0"
                                                           , WritePiority.High);

                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                         , currentStep.SoLuongXung.Value.ToString() ?? "0"
                                                          , WritePiority.High);

                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                        , "1"
                                                         , WritePiority.High);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Lỗi khi nhập mã hàng: {ex.Message}");
            }
        }

        #region Tasks
        private async Task TaskTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    //var ping = PingServer(GlobalVariable.IpDbServer);
                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        _labTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        _labRev.Text = GlobalVariable.RevoRealtimeModel.Rev;
                        _labColorCode.Text = GlobalVariable.RevoRealtimeModel.ColorCode;
                        _labMandrel.Text = GlobalVariable.RevoRealtimeModel.Mandrel;
                        _labMandrelStart.Text = GlobalVariable.RevoRealtimeModel.MandrelStart;

                        //_labStatus.BackColor = GetConnectionStatusColor(_easyStatus);

                        _labStatus.Text = $"Easy Driver: {_easyStatus} - PLC: {GlobalVariable.RevoRealtimeModel.PlcConnected} - DB Server:";
                    });
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
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
                                currentStep.TotalRunTime = (currentStep.EndAt - currentStep.StartAt)?.TotalSeconds;

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
                                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                                          , currentStep.Speed_Hz.HasValue ? currentStep.Speed_Hz.Value.ToString() : "0"
                                                                           , WritePiority.High);

                                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                                         , currentStep.SoLuongXung.HasValue ? currentStep.SoLuongXung.Value.ToString() : "0"
                                                                          , WritePiority.High);

                                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                                        , "1"
                                                                         , WritePiority.High);
                                }
                                else
                                {
                                    GlobalVariable.RevoRealtimeModel.Steps.ForEach(x =>
                                    {
                                        x.StartAt = null;
                                        x.EndAt = null;
                                        x.TotalRunTime = 0;
                                    });

                                    var step = GlobalVariable.RevoRealtimeModel.Steps
                                        .FirstOrDefault(s =>
                                            s.Enable == true &&
                                            s.StartAt == null
                                        );

                                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                                                                         , currentStep.Speed_Hz.HasValue ? currentStep.Speed_Hz.Value.ToString() : "0"
                                                                          , WritePiority.High);

                                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                                                                         , currentStep.SoLuongXung.HasValue ? currentStep.SoLuongXung.Value.ToString() : "0"
                                                                          , WritePiority.High);

                                    _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                                                                        , "1"
                                                                         , WritePiority.High);
                                }
                            }
                        }

                        //cập nhật lại giao diện
                        UpdateStepUI(currentStep);


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

            START_STOP_STEP_ValueChanged(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP")
                , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP")
                , "", _easyDriverConnector.GetTag($"{GlobalVariable.RevoConfig.Path}/START_STOP_STEP").Value));
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
        }
        private Panel CreateStepRow(RevoStep step)
        {
            Panel row = new Panel();
            row.Width = 660;
            row.Height = 58;
            row.Margin = new Padding(5, 0, 5, 0);

            row.Tag = step.StepIndex; // ← KEY

            // Số thứ tự
            Label lblIndex = new Label();
            lblIndex.Name = "lblIndex";
            lblIndex.Text = step.StepIndex.ToString();
            lblIndex.Width = 50;
            lblIndex.Height = 58;
            lblIndex.TextAlign = ContentAlignment.MiddleCenter;
            lblIndex.Location = new Point(0, 5);
            lblIndex.Font = new Font("Microsoft Sans Serif", 15, FontStyle.Regular);

            // Thanh REVO
            Label lblStep = new Label();
            lblStep.Name = "lblStep";
            lblStep.Text = $"{step.StepName} - {step.StartAt} -> {step.EndAt}: {step.TotalRunTime}s{Environment.NewLine}Pul={step.SoLuongXung} - Speed = {step.Speed_Hz}";
            lblStep.Width = 610;
            lblStep.Height = 58;
            lblStep.Location = new Point(60, 5);
            lblStep.BackColor = (bool)step.Enable ? Color.LightBlue : Color.Black;
            lblStep.ForeColor = (bool)step.Enable ? Color.Black : Color.White;
            lblStep.TextAlign = ContentAlignment.TopLeft;
            lblStep.Font = new Font("Microsoft Sans Serif", 13, FontStyle.Regular);

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

        public void UpdateStepUI(RevoStep step)
        {
            foreach (Control ctrl in flowMain.Controls)
            {
                if (ctrl is Panel row && row.Tag is int idx && idx == step.StepIndex)
                {
                    Label lblIndex = row.Controls["lblIndex"] as Label;
                    Label lblStep = row.Controls["lblStep"] as Label;

                    lblIndex.Text = step.StepIndex.ToString();

                    lblStep.Text = $"{step.StepName} - {(DateTime)step.StartAt} -> {step.EndAt}: {step.TotalRunTime}s" +
                        $"{Environment.NewLine}" +
                        $"Pul={step.SoLuongXung} - Speed = {step.Speed_Hz}";

                    if (step.StartAt.HasValue && !step.EndAt.HasValue)
                    {
                        lblStep.BackColor = Color.Green;
                        lblStep.ForeColor = Color.Black;
                    }
                    else if (step.StartAt.HasValue && step.EndAt.HasValue)
                    {
                        lblStep.BackColor = Color.Gray;
                        lblStep.ForeColor = Color.White;
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
        #endregion
    }
}
