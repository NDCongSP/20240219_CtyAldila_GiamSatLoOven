using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using Scada_TrackingTime_AutoRollingkd;
using Serilog;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Net.NetworkInformation;
using static GiamSat.Models.ApiRoutes;

namespace Scada_TrackingTime_AutoRolling
{
    public partial class Form1 : Form
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

        private List<AutoRollingTagChangedModel> _tagsValueRealtime = new List<AutoRollingTagChangedModel>();

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
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

        private async void Form1_Load(object? sender, EventArgs e)
        {
            //ddocj gias trij config
            using (var dbContext = new ApplicationDbContext(GlobalVariable.ConnectionString))
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
                    GlobalVariable.RevoRealtimeModels.Add(new RevoRealtimeModel()
                    {
                        RevoId = item.Id.Value,
                        RevoName = item.Name,
                        Path = item.Path,
                        Steps = new List<RevoStep>()
                    });

                    _tagsValueRealtime.Add(new AutoRollingTagChangedModel()
                    {
                        RevoId = item.Id.Value,
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

                _btnMaintenance.Click += (s, o) =>
                {
                    using (var nf = new frmConfig())
                    {
                        nf.ShowDialog();
                    }
                };
            }
        }

        private async void _txtPart_KeyDown(object sender, KeyEventArgs e)
        {
            //try
            //{
            //    if (e.KeyCode == Keys.Enter)
            //    {
            //        var part = _parts.FirstOrDefault(p => p.PN == _txtPart.Text.Trim());

            //        if (part == null)
            //        {
            //            MessageBox.Show("Không tìm thấy mã hàng trong database. Vui lòng kiểm tra lại.", "CẢNH BÁO",
            //                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            //            return;
            //        }

            //        //Luu path va part vao bien toan cuc de su dung sau nay
            //        if (GlobalVariable.Part != part.PN)
            //        {
            //            GlobalVariable.Part = part.PN;
            //        }

            //        var steps = GlobalVariable.BuildSteps(part);

            //        GlobalVariable.RevoRealtimeModels = new RevoRealtimeModel()
            //        {
            //            RevoId = item.Id.Value,
            //            RevoName = item.Name,
            //            Path = item.Path,
            //            Part = part.PN,
            //            Rev = part.Blank_Rev,
            //            ColorCode = part.Flex_Color,
            //            Mandrel = _mandrels.FirstOrDefault(x => x.ID == part.Mandrel)?.PN,
            //            MandrelStart = part.Mandrel_Start.ToString(),
            //            Steps = steps,
            //            ShaftNum = Guid.NewGuid()
            //        };

            //        //taojj ui step lan dau tien
            //        LoadStepsToFlowPanel();

            //        LogDb(isNew: true);

            //        //ghi step đầu tiên xuống plc
            //        var currentStep = GlobalVariable.RevoRealtimeModels.Steps
            //       .FirstOrDefault(s =>
            //           s.Enable == true &&
            //           !s.StartAt.HasValue
            //       );

            //        if (currentStep == null)
            //        {
            //            MessageBox.Show($"Part {GlobalVariable.Part} khong co thong tin step.");
            //            Log.WA($"Part {GlobalVariable.Part} khong co thong tin step.");
            //            return;
            //        }

            //    Loop1:
            //        await _easyDriverConnector.WriteTagAsync($"{item.Path}/PC_ALLOW_RUN_TO_PLC"
            //                                                     , "1"
            //                                                      , WritePiority.High);
            //        if (!PC_ALLOW_RUN_TO_PLC)
            //        {
            //            goto Loop1;
            //        }

            //        await _easyDriverConnector.WriteTagAsync($"{item.Path}/TOC_DO_HZ"
            //                                                , currentStep?.Speed_Hz.ToString() ?? "0"
            //                                                 , WritePiority.High);

            //        await _easyDriverConnector.WriteTagAsync($"{item.Path}/SO_LUONG_XUNG"
            //                                               , currentStep.SoLuongXung.Value.ToString() ?? "0"
            //                                                , WritePiority.High);

            //    Loop2:
            //        await _easyDriverConnector.WriteTagAsync($"{item.Path}/SENT"
            //                                               , "1"
            //                                                , WritePiority.High);

            //        if (!SENT)
            //        {
            //            goto Loop2;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Error($"Lỗi khi nhập mã hàng: {ex.Message}");
            //}
        }

        #region Tasks
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
                        _labTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");


                        _labStatus.Text = $"Easy Driver: {_easyStatus} - PLC: {_tagsValueRealtime.FirstOrDefault().PlcConnected} " +
                        $"- DB Server: - Save type: {GlobalVariable.RevoConfigs.FirstOrDefault().SaveMode.ToString()}";
                    });


                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    //Log.Error($"Lỗi trong TaskTimerAsync: {ex.Message}");
                }
                await Task.Delay(100, token); // Chờ 1 giây trước khi lặp lại
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



                    UpdateStepUI(new RevoStep(), 1);

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
            foreach (var item in GlobalVariable.RevoConfigs)
            {
                _easyDriverConnector.GetTag($"{item.Path}/Part_Code").QualityChanged += Part_Code_QualityChanged;
                _easyDriverConnector.GetTag($"{item.Path}/Part_Code").ValueChanged += Recipe_Settings_ValueChanged;
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

                Recipe_Settings_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings")
                    , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings")
                    , "", _easyDriverConnector.GetTag($"{item.Path}/Recipe_Settings").Value));

                Step_Run_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/Step_Run")
                   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/Step_Run")
                   , "", _easyDriverConnector.GetTag($"{item.Path}/Step_Run").Value));

                TimeRunStep_ValueChanged(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep1")
                   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"{item.Path}/TimeRunStep1")
                   , "", _easyDriverConnector.GetTag($"{item.Path}/TimeRunStep1").Value));
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

                        Log.Warning($"Alarm description {item.RevoId} disconnect.");
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

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
                var al = deviceName.Substring(4);

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.StepRun = int.TryParse(e.NewValue, out int value) ? value : 0;

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
                var al = deviceName.Substring(4);

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.TimeRunStep1 = int.TryParse(e.NewValue, out int value) ? value : 0;

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
                var al = deviceName.Substring(4);

                foreach (var item in _tagsValueRealtime)
                {
                    if (item.Path == path)
                    {
                        item.Part_Name1 = int.TryParse(e.NewValue, out int value) ? value : 0;

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        #endregion

        #region Method helper
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
            using (var dbContext = new ApplicationDbContext(GlobalVariable.ConnectionString))
            {
                #region Data log
                var createdAt = DateTime.Now;
                var createdMachine = Environment.MachineName;

                if (isNew)
                {
                    var dataLogs = new List<FT09_RevoDatalog>();


                    //foreach (var item in GlobalVariable.RevoRealtimeModels.Steps.Where(x => x.Enable == true))
                    //{
                    //    var nl = new FT09_RevoDatalog()
                    //    {
                    //        Id = Guid.NewGuid(),
                    //        CreatedAt = createdAt,
                    //        CreatedMachine = createdMachine,
                    //        //RevoId = GlobalVariable.RevoRealtimeModels.RevoId,
                    //        //RevoName = GlobalVariable.RevoRealtimeModels.RevoName,
                    //        //Work = GlobalVariable.RevoRealtimeModels.Work,
                    //        //Part = GlobalVariable.RevoRealtimeModels.Part,
                    //        //Rev = GlobalVariable.RevoRealtimeModels.Rev,
                    //        //ColorCode = GlobalVariable.RevoRealtimeModels.ColorCode,
                    //        //Mandrel = GlobalVariable.RevoRealtimeModels.Mandrel,
                    //        //MandrelStart = GlobalVariable.RevoRealtimeModels.MandrelStart,
                    //        //ShaftNum = GlobalVariable.RevoRealtimeModels.ShaftNum,
                    //        StepId = item.StepIndex,
                    //        StepName = item.StepName,
                    //    };

                    //    dataLogs.Add(nl);
                    //}

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

                #region realtime log  
                var rl = await dbContext.FT08_RevoRealtimes.FirstOrDefaultAsync(x => x.C000_RevoId == GlobalVariable.RevoId);

                if (rl != null)
                {
                    rl.C001_Data = JsonConvert.SerializeObject(GlobalVariable.RevoRealtimeModels);
                }
                else
                {
                    var nl = new FT08_RevoRealtime()
                    {
                        Id = Guid.NewGuid(),
                        C000_RevoId = GlobalVariable.RevoId,
                        C001_Data = JsonConvert.SerializeObject(GlobalVariable.RevoRealtimeModels)
                    };

                    dbContext.FT08_RevoRealtimes.Add(nl);
                }
                #endregion

                await dbContext.SaveChangesAsync();
            }
        }

        private async Task<GroupShaftModel> GetTotalShaftAsync()
        {
            using var dbContext = new ApplicationDbContext(GlobalVariable.ConnectionString);

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


            //// 3. Đếm ShaftNum giờ hiện tại
            //var currentCount = dbContext.FT09_RevoDatalogs
            //    .Where(x => x.CreatedAt >= currentHour
            //             && x.CreatedAt < nextHour
            //             && x.ShaftNum != null)
            //    .Select(x => x.ShaftNum)
            //    .Distinct()
            //    .Count();


            //// 4. Đếm ShaftNum giờ trước
            //var lastCount = dbContext.FT09_RevoDatalogs
            //    .Where(x => x.CreatedAt >= lastHour
            //             && x.CreatedAt < currentHour
            //             && x.ShaftNum != null)
            //    .Select(x => x.ShaftNum)
            //    .Distinct()
            //    .Count();


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
    }
}
