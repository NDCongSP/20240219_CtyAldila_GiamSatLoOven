using Dapper;
using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using GiamSat.Models.NotTable;
using Microsoft.Win32;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
//using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace GiamSat.Scada
{
    public partial class Form1 : Form
    {
        private List<FT05> _alarm = new List<FT05>();
        private List<FT06> _ft06 = new List<FT06>();

        private OvensInfo _ovensInfo = new OvensInfo();//cau hinh cac lo oven
        private RealtimeDisplays _displayRealtime = new RealtimeDisplays();
        private List<ControlPlcModel> _controlPlcModel = new List<ControlPlcModel>();//đọc các tín hiệu điều khiển tắt đèn cảnh báo từ web.

        //private Timer _timer = new Timer();

        private DateTime _startTime, _endTime, _startTimeDisplay, _endTimeDisplay;
        private double _totalTime = 0, _totalTimeDisplay = 0;

        private Task _taskDataLog, _taskDataLogProfile, _taskAlarm;

        private DateTime _beginTimeCheckAlarm, _endTimeCheckAlarm;
        private double _totalTimeCheckAlarm = 0;

        #region Email
        private Email _email = new Email();
        AlarmSetting _alarmSetting = new AlarmSetting();
        #endregion

        private bool[] _alarmEnable, _alarmEnableFlag;
        private string[] _alarmValue;

        private bool _isLoaded = false;
        private bool _isReOpenApp = false;//biến dùng để báo ap bị tắt mở lại, vào lấy lại ZIndex cũ trước khi tắt phần mềm để log profile tiếp

        private CancellationTokenSource _timerCts;
        private Task _timerTask;

        private EasyDriverConnector _easyDriverConnector;
        private ConnectionStatus _easyStatus;

        #region update giám sát thời gian chạy máy auto rolling
        private CancellationTokenSource _checkingTimeStepCts;
        private readonly AsyncAutoResetEvent _triggerCheckTimeStep = new AsyncAutoResetEvent();

        private CancellationTokenSource _resetShaftCts;
        private readonly AsyncAutoResetEvent _triggerResetShaft = new AsyncAutoResetEvent();

        private bool PC_ALLOW_RUN_TO_PLC = false;
        private bool START_STOP_STEP = false;

        private int _totalCurrentHour = 0, _totalLastHour = 0;

        //chỉ lấy máy autorolling
        private RevoConfigs _revoConfigs { get; set; } = new RevoConfigs();
        private List<RevoRealtimeModel> _revoRealtimeModel { get; set; } = new List<RevoRealtimeModel>();
        private List<AutoRollingTagChangedModel> _autoRollingTagChangeds = new List<AutoRollingTagChangedModel>();

        private int[] _partNameAscii = new int[8];
        #endregion

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show("Không tắt app này!", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            try
            {
                //easyDriverConnector1.Started -= EasyDriverConnector1_Started;
                //easyDriverConnector1.Stop();

                _timerCts.Cancel();
                _timerTask.Wait(1000);

                //e.Cancel = true;
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

        private async void Form1_Load(object sender, EventArgs e)
        {
            #region Serilog initial
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            #endregion

            #region Get thong tin chuong
            GetOvensInfo();

            //get thông tin Revo config để hiển thị lên web
            using (var dbContext = new ApplicationDbContext())
            {
                var constring = dbContext.Database.Connection.ConnectionString;

                var ft07 = await dbContext.FT07_RevoConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft07 != null)
                {
                    var configs = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);

                    _revoConfigs.AddRange(
                                    configs.Where(x => x.Name.Contains("Auto Rolling"))
                                );


                    if (_revoConfigs?.Count <= 0)
                    {
                        MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                            "CẢNH BÁO", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                            );

                        return;
                    }

                    //Text = $"Chương trình giám sát thời gian chạy - Máy {GlobalVariable.RevoConfig.Name}";
                }
                else
                {
                    MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.",
                        "CẢNH BÁO", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                        );
                    return;
                }
            }

            //khởi tạo model realtime data
            foreach (var item in _revoConfigs)
            {
                _revoRealtimeModel.Add(new RevoRealtimeModel()
                {
                    RevoId = item.Id.Value,
                    RevoName = item.Name,
                    Path = item.Path,
                    PlcConnected = false,
                    Steps = new List<RevoStep>()
                });

                _autoRollingTagChangeds.Add(new AutoRollingTagChangedModel());
            }
            #endregion

            easyDriverConnector1.Started += EasyDriverConnector1_Started;

            //_timer.Interval = 5000;
            //_timer.Tick += _timer_Tick;
            //_timer.Enabled = true;

            _startTime = _startTimeDisplay = _endTime = _endTimeDisplay = DateTime.Now;

            _taskDataLog = new Task(() => LogData());
            _taskDataLog.Start();

            //update revo realtime model
            _taskDataLogProfile = new Task(() => LogDataProfile());
            _taskDataLogProfile.Start();

            _timerCts = new CancellationTokenSource();
            _timerTask = Task.Run(async () => await TaskTimerAsync(_timerCts.Token));

            _checkingTimeStepCts = new CancellationTokenSource();
            _ = TaskCheckTimeStepAsync(_checkingTimeStepCts.Token);
            //_resetShaftCts = new CancellationTokenSource();
            //_ = TaskResetShaftAsync(_resetShaftCts.Token);
        }

        #region Methods
        /// <summary>
        /// Khởi tạo data ban đầu.
        /// </summary>
        private void GetOvensInfo()
        {
            try
            {
                using (var con = GlobalVariable.GetDbConnection())
                {
                    var para = new DynamicParameters();

                Loop:
                    var ft01 = con.Query<FT01>("sp_FT01GetAll", commandType: CommandType.StoredProcedure).ToList();

                    if (ft01.Count <= 0)
                    {
                        #region khoi tao data
                        var c = new ConfigModel()
                        {
                            TimeTempChange = 5000,//5s
                            Gain = 1,
                            DataLogInterval = 5000,//5s
                            DataLogWhenRunProfileInterval = 5000//1s
                            ,
                            DisplayRealtimeInterval = 1000
                            ,
                            RefreshInterval = 1000
                            ,
                            ChartRefreshInterval = 1000
                            ,
                            CountSecondStop = 5//5 giay
                            ,
                            ToleranceTempForRampDown = 2
                            ,
                            ToleranceTempOutForRampDown = 1
                            ,
                            ToleranceTempForRampUp = 3
                            ,
                            ToleranceTempOutForRampUp = 0
                            ,
                            ChartPointNum = 30
                            ,
                            Smooth = true
                            ,
                            ShowDataLabels = true
                            ,
                            ShowMarkers = true
                        };

                        OvensInfo ov = new OvensInfo();

                        for (int i = 1; i <= 13; i++)
                        {
                            var steps = new List<StepModel>();
                            steps.Add(new StepModel()
                            {
                                Id = 1,
                                StepType = EnumProfileStepType.RampTime,
                                Hours = 1,
                                Minutes = 30,
                                Seconds = 1,
                                SetPoint = 150
                            });
                            steps.Add(new StepModel()
                            {
                                Id = 2,
                                StepType = EnumProfileStepType.Soak,
                                Hours = 0,
                                Minutes = 50,
                                Seconds = 10,
                                SetPoint = 150
                            });
                            steps.Add(new StepModel()
                            {
                                Id = 3,
                                StepType = EnumProfileStepType.RampTime,
                                Hours = 1,
                                Minutes = 30,
                                Seconds = 1,
                                SetPoint = 250
                            });
                            steps.Add(new StepModel()
                            {
                                Id = 4,
                                StepType = EnumProfileStepType.Soak,
                                Hours = 0,
                                Minutes = 50,
                                Seconds = 10,
                                SetPoint = 250
                            });
                            steps.Add(new StepModel()
                            {
                                Id = 5,
                                StepType = EnumProfileStepType.RampTime,
                                Hours = 1,
                                Minutes = 30,
                                Seconds = 1,
                                SetPoint = 80
                            });
                            steps.Add(new StepModel()
                            {
                                Id = 6,
                                StepType = EnumProfileStepType.End,
                                Hours = 0,
                                Minutes = 0,
                                Seconds = 0,
                                SetPoint = 0
                            });

                            var profiles = new List<ProfileModel>();
                            profiles.Add(new ProfileModel()
                            {
                                Id = 1,
                                Name = $"Profile 1",
                                LevelUp = 140,
                                LevelDown = 50,
                                Steps = steps
                            });
                            profiles.Add(new ProfileModel()
                            {
                                Id = 2,
                                Name = $"Profile 2",
                                LevelUp = 140,
                                LevelDown = 50,
                                Steps = steps
                            });

                            var chanel = i <= 5 ? 1 : i > 5 && i <= 10 ? 2 : 3;
                            ov.Add(new OvenInfoModel()
                            {
                                Id = i,
                                Name = $"Oven {i}",
                                Profiles = profiles,
                                Path = $"Local Station/Channel{chanel}/Oven{i}"
                            });

                        }

                        var ft01Insert = new FT01()
                        {
                            Id = Guid.NewGuid(),
                            C000 = JsonConvert.SerializeObject(c),
                            C001 = JsonConvert.SerializeObject(ov)
                        };

                        var p = new DynamicParameters();
                        p.Add("c000", ft01Insert.C000);
                        p.Add("c001", ft01Insert.C001);

                        var r = con.Execute("sp_FT01Insert", param: p, commandType: CommandType.StoredProcedure);

                        if (r > 0) goto Loop;

                        #endregion
                    }

                    if (this.InvokeRequired)
                    {
                        this?.Invoke(new Action(() =>
                        {
                            _labDBServer.Text = "DB Connected";
                        }));
                    }
                    else
                    {
                        _labDBServer.Text = "DB Connected";
                    }

                    GlobalVariable.ConfigSystem = JsonConvert.DeserializeObject<ConfigModel>(ft01.FirstOrDefault().C000);
                    _ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(ft01.FirstOrDefault().C001);

                    #region Khởi tạo các biến bật tắt cảnh báo
                    _alarmEnable = new bool[_ovensInfo.Count];
                    _alarmEnableFlag = new bool[_ovensInfo.Count];
                    _alarmValue = new string[_ovensInfo.Count];

                    for (int i = 0; i < _ovensInfo.Count; i++)
                    {
                        _alarmEnable[i] = false;
                        _alarmEnableFlag[i] = false;
                        _alarmValue[i] = "0";
                    }
                    #endregion

                Loop1:
                    _ft06 = con.Query<FT06>("sp_FT06GetAll", commandType: CommandType.StoredProcedure).ToList();

                    if (_ft06.Count <= 0)
                    {
                        foreach (var item in _ovensInfo)
                        {
                            _controlPlcModel.Add(new ControlPlcModel()
                            {
                                OvenId = item.Id,
                                OvenName = item.Name,
                                OffSerien = 0,
                                IsDoFlag = false
                            });
                        }

                        var p = new DynamicParameters();
                        p.Add("c000", JsonConvert.SerializeObject(_controlPlcModel));
                        con.Execute("sp_FT06Insert", param: p, commandType: CommandType.StoredProcedure);

                        goto Loop1;
                    }

                    _controlPlcModel = JsonConvert.DeserializeObject<List<ControlPlcModel>>(_ft06.FirstOrDefault().C000);

                    var ft02 = con.Query<FT02>("sp_FT02GetAll", commandType: CommandType.StoredProcedure).ToList();

                    if (ft02.Count <= 0)
                    {
                        foreach (var item in _ovensInfo)
                        {
                            var dt = DateTime.Now;
                            _displayRealtime.Add(new RealtimeDisplayModel()
                            {
                                OvenId = item.Id,
                                OvenName = item.Name,
                                Path = item.Path,
                                OvenInfo = item,
                                StatusFlag = false,
                                AlarmFlag = false,
                                ResetAlarmFlag = false,
                                AlarmFlagLastStep = false,
                                Status = 0,
                                Alarm = 0,
                                ConnectionStatus = 0,
                                ZIndex = Guid.Empty,
                                IsLoaded = false,
                                CountSecondTagChange = 0,
                            });
                        }

                        #region initial display table                
                        con.Execute("Truncate table FT02");
                        var displayData = JsonConvert.SerializeObject(_displayRealtime);
                        para = null;
                        para = new DynamicParameters();
                        para.Add("C000", displayData);
                        para.Add("createdDate", DateTime.Now);
                        con.Execute("sp_FT02Insert", param: para, commandType: CommandType.StoredProcedure);
                        #endregion
                    }
                    else
                    {
                        _displayRealtime = JsonConvert.DeserializeObject<RealtimeDisplays>(ft02.FirstOrDefault().C000);

                        //set giá trị setPoint = 0 để làm điều kiện set là app mới mở hay đang chạy, khi tag chuyển bước valurChanged
                        foreach (var item in _displayRealtime)
                        {
                            item.SetPoint = 0;
                            item.ResetAlarmFlag = false;
                            item.AlarmFlag = false;
                            item.StatusFlag = false;
                            item.AlarmFlagLastStep = false;
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, "From GetOvensInfo."); }
        }

        private void RefreshConfig()
        {
            try
            {
                using (var con = GlobalVariable.GetDbConnection())
                {
                    var para = new DynamicParameters();

                    var ft01 = con.Query<FT01>("sp_FT01GetAll", commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds).ToList();

                    GlobalVariable.ConfigSystem = JsonConvert.DeserializeObject<ConfigModel>(ft01.FirstOrDefault().C000);
                    _ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(ft01.FirstOrDefault().C001);

                    //Debug.WriteLine($"Time counr second: {GlobalVariable.ConfigSystem.CountSecondStop}");

                    _ft06 = con.Query<FT06>("sp_FT06GetAll", commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds).ToList();

                    _controlPlcModel = JsonConvert.DeserializeObject<List<ControlPlcModel>>(_ft06.FirstOrDefault().C000);

                    foreach (var item in _ovensInfo)
                    {
                        var d = _displayRealtime.FirstOrDefault(x => x.OvenId == item.Id);
                        d.OvenName = item.Name;
                        d.OvenInfo = item;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, "From RefreshConfig."); }
        }

        private void LogDataProfile()
        {
            while (true)
            {
                try
                {
                    if (_isLoaded)
                    {
                        //log data
                        using (var con = GlobalVariable.GetDbConnection())
                        {
                            con.Open();
                            using (var tran = con.BeginTransaction())
                            {
                                try
                                {
                                    foreach (var item in _displayRealtime)
                                    {
                                        if (item.Status == 1 && item.ProfileStepType_CurrentStatus != EnumProfileStepType.End && item.ZIndex != Guid.Empty && item.Temperature > 0 && item.CountSecondTagChange >= 2)
                                        {
                                            var para = new DynamicParameters();
                                            para.Add("ovenId", item.OvenId);
                                            para.Add("ovenName", item.OvenName);
                                            para.Add("temperature  ", item.Temperature);
                                            para.Add("startTime", item.BeginTime);
                                            //para.Add("endTime", null);
                                            para.Add("zIndex", item.ZIndex);
                                            para.Add("hours", item.Hours);
                                            para.Add("minutes", item.Minutes);
                                            para.Add("seconds", item.Seconds);
                                            para.Add("profileId", item.ProfileNumber_CurrentStatus);
                                            para.Add("profileName", item.ProfileName);
                                            para.Add("stepId", item.ProfileStepNumber_CurrentStatus);
                                            para.Add("stepName", item.StepName.ToString());
                                            para.Add("setPoint", item.SetPoint);
                                            para.Add("status", item.Status);
                                            para.Add("createdDate", DateTime.Now);
                                            para.Add("details", JsonConvert.SerializeObject(item));

                                            con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds);
                                        }
                                        else if (item.Status == 0 && item.ProfileStepType_CurrentStatus == EnumProfileStepType.End && item.ZIndex != Guid.Empty && item.Temperature > 0 && item.CountSecondTagChange >= 2)
                                        {
                                            var para = new DynamicParameters();
                                            para.Add("ovenId", item.OvenId);
                                            para.Add("ovenName", item.OvenName);
                                            para.Add("temperature  ", item.Temperature);
                                            para.Add("startTime", item.BeginTime);
                                            para.Add("endTime", DateTime.Now);
                                            para.Add("zIndex", item.ZIndex);
                                            para.Add("hours", item.Hours);
                                            para.Add("minutes", item.Minutes);
                                            para.Add("seconds", item.Seconds);
                                            para.Add("profileId", item.ProfileNumber_CurrentStatus);
                                            para.Add("profileName", item.ProfileName);
                                            para.Add("stepId", item.ProfileStepNumber_CurrentStatus);
                                            para.Add("stepName", item.StepName.ToString());
                                            para.Add("setPoint", item.SetPoint);
                                            para.Add("status", item.Status);
                                            para.Add("createdDate", DateTime.Now);
                                            para.Add("details", JsonConvert.SerializeObject(item));

                                            con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds);

                                            item.ZIndex = Guid.Empty;
                                            //item.ProfileName = null;
                                            //item.BeginTime = null;
                                            //item.TotalTimeRunMinute = 0;
                                            //item.SetPointLastStep = 0;
                                            //item.TempRange = 0;
                                            //item.TotalTimeRunMinute = 0;
                                            //item.SetPoint = 0;
                                            //item.ProfileStepNumber_CurrentStatus = 0;
                                        }
                                    }
                                    tran.Commit();
                                }
                                catch (Exception ex)
                                {
                                    tran.Rollback();
                                    Log.Error(ex, "From LogDataProfile DB transaction.");
                                    return;
                                }
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(GlobalVariable.ConfigSystem.DataLogWhenRunProfileInterval);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "From _taskDataLogProfile");
                }
            }
        }

        private void LogData()
        {
            while (true)
            {
                try
                {
                    if (_isLoaded)
                    {

                        _endTime = _endTimeDisplay = DateTime.Now;
                        _totalTime = (_endTime - _startTime).TotalMilliseconds;
                        _totalTimeDisplay = (_endTimeDisplay - _startTimeDisplay).TotalMilliseconds;

                        #region realtime display
                        if (_totalTimeDisplay >= GlobalVariable.ConfigSystem.DisplayRealtimeInterval)
                        {
                            _startTimeDisplay = DateTime.Now;
                            //log data
                            using (var con = GlobalVariable.GetDbConnection())
                            {
                                var para = new DynamicParameters();
                                con.Execute("Truncate table FT02");
                                var displayData = JsonConvert.SerializeObject(_displayRealtime);
                                para = null;
                                para = new DynamicParameters();
                                para.Add("C000", displayData);
                                para.Add("createdDate", DateTime.Now);
                                con.Execute("sp_FT02Insert", param: para, commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds);
                            }
                        }
                        #endregion

                        #region data log
                        if (_totalTime >= GlobalVariable.ConfigSystem.DataLogInterval)
                        {
                            _startTime = DateTime.Now;
                            //log data
                            using (var con = GlobalVariable.GetDbConnection())
                            {
                                foreach (var item in _displayRealtime)
                                {
                                    var para = new DynamicParameters();
                                    para.Add("ovenId", item.OvenId);
                                    para.Add("ovenName", item.OvenName);
                                    para.Add("temperature", item.Temperature);
                                    para.Add("details", JsonConvert.SerializeObject(item));

                                    con.Execute("sp_FT03Insert", param: para, commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds);
                                }
                            }
                        }
                        #endregion

                        #region Kiểm tra xem lò chạy hay dùng để log data và cảnh báo
                        foreach (var item in _displayRealtime)
                        {
                            if (item.OvenId == 13)
                            {
                                var a = 12;
                            }
                            item.StatusTimeEnd = DateTime.Now;
                            if ((item.StatusTimeEnd - item.StatusTimeBegin).TotalMilliseconds <= GlobalVariable.ConfigSystem.CountSecondStop
                                && item.StatusFlag == false)
                            {
                                item.Status = 1;//báo lò đang chạy.
                                item.ZIndex = !_isReOpenApp ? Guid.NewGuid() : item.ZIndex;
                                item.BeginTime = !_isReOpenApp ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : item.BeginTime;
                                item.StatusFlag = true;

                                item.BeginTimeAlarm = DateTime.Now;
                            }
                            if ((item.StatusTimeEnd - item.StatusTimeBegin).TotalMilliseconds > GlobalVariable.ConfigSystem.CountSecondStop
                                && item.StatusFlag == true)
                            {
                                item.Status = 0;
                                item.StatusFlag = false;
                            }

                            //Debug.WriteLine($"{item.OvenId} status: {item.Status}");
                        }
                        #endregion
                    }

                    System.Threading.Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "From _taskDataLog");
                }
            }
        }

        private async void OnOffSerien()
        {
            try
            {
                for (int i = 0; i < _alarmEnable.Length; i++)
                {
                    //Debug.WriteLine($"Before Oven {i+1}{_alarmEnable[i]}");

                    if (_alarmEnable[i])
                    {
                        await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{i + 1}", _alarmValue[i], WritePiority.High);
                        //_alarmEnableFlag[i] = false;
                        _alarmEnable[i] = false;
                    }
                    //Debug.WriteLine($"After Oven {i + 1}{_alarmEnable[i]}");
                }

                //for (int i = 0; i < _alarmEnable.Length; i++)
                //{
                //    if (_alarmEnable[i] && _alarmEnableFlag[i] == false)
                //    {
                //        await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{i + 1}", _alarmValue[i], WritePiority.High);
                //        _alarmEnableFlag[i] = true;
                //    }
                //    else if (!_alarmEnable[i] && _alarmEnableFlag[i] == true)
                //    {
                //        await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{i + 1}", "0", WritePiority.High);
                //        _alarmEnableFlag[i] = false;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Log.Error(ex, "From OnOffSerien");
            }
        }

        #endregion

        #region Events
        private async void _timer_Tick(object sender, EventArgs e)
        {
            //Timer t = (Timer)sender;
            //try
            //{
            //    t.Enabled = false;

            //    if (_isLoaded)
            //    {
            //        if (this.InvokeRequired)
            //        {
            //            this?.Invoke(new Action(() =>
            //            {
            //                _labTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            //            }));
            //        }
            //        else
            //        {
            //            _labTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            //        }

            //        #region check alarm
            //        int index = 1;
            //        foreach (var item in _displayRealtime)
            //        {
            //            if (item.Status == 1 && item.CountSecondTagChange >= 2)
            //            {
            //                //Cảnh báo toàn thời gian

            //                //rampTime tăng nhiệt
            //                if (item.SetPointLastStep < item.SetPoint)
            //                {
            //                    if (item.EndStep == 0)//khi không có kết thúc bước thì vào kiểm tra nhiệt độ theo khoảng thời gian
            //                    {
            //                        if (
            //                            item.Temperature <= (item.SetPoint + GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp)
            //                          //&& item.Temperature >= (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp))
            //                          && item.AlarmFlag == false
            //                           )
            //                        {
            //                            //if (item.OvenId == 13)
            //                            //{
            //                            //    var a = 10;
            //                            //}

            //                            item.AlarmFlag = true;
            //                            item.Alarm = 0;
            //                            item.SerienStatus = 0;
            //                            item.AlarmDescription = null;
            //                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
            //                            _alarmEnable[item.OvenId - 1] = true;
            //                            _alarmValue[item.OvenId - 1] = "0";

            //                            //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //                            item.AlarmFlagLastStep = false;
            //                            item.ResetAlarmFlag = true;

            //                            Debug.WriteLine($"Off Alarm {item.StepName.ToString()} up|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                        }
            //                        else if (item.AlarmFlag && !item.ResetAlarmFlag)
            //                        {
            //                            if (item.OvenId == 13)
            //                            {
            //                                var a = 10;
            //                            }

            //                            item.AlarmFlag = false;
            //                            item.ResetAlarmFlag = true;
            //                        }
            //                    }
            //                    else//so sánh của bước cũ, khi vừa kết thúc bước, chuyển qua bước khác.
            //                    {
            //                        if (
            //                            (item.Temperature < (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTempForRampUp)
            //                           || item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempForRampUp))
            //                           && item.AlarmFlagLastStep == false
            //                           )
            //                        {
            //                            //if (item.OvenId == 13)
            //                            //{
            //                            //    var a = 10;
            //                            //}

            //                            item.Alarm = 1;
            //                            item.SerienStatus = 1;
            //                            item.AlarmDescription = $"Nhiệt độ chưa đạt";
            //                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
            //                            _alarmEnable[item.OvenId - 1] = true;
            //                            _alarmValue[item.OvenId - 1] = "1";

            //                            //log DB alarm
            //                            //var p = new DynamicParameters();
            //                            //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //                            item.AlarmFlagLastStep = true;
            //                            item.AlarmFlag = false;
            //                            item.ResetAlarmFlag = true;

            //                            Debug.WriteLine($"On Alarm {item.StepName.ToString()} Up|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                        }
            //                    }
            //                }
            //                //soak-ngâm
            //                else if (item.SetPointLastStep == item.SetPoint)
            //                {
            //                    if (item.OvenId == 13)
            //                    {
            //                        var a = 10;
            //                    }
            //                    if (item.EndStep == 0)
            //                    {
            //                        item.EndTimeAlarm = DateTime.Now;
            //                        var tAlarm = (item.EndTimeAlarm - item.BeginTimeAlarm).TotalMilliseconds;

            //                        //tính ra khoảng nhiệt cần tăng đến thời điểm hiện tại
            //                        item.TempRequired = item.SetPoint;

            //                        if (tAlarm > GlobalVariable.ConfigSystem.TimeTempChange)
            //                        {
            //                            item.IsCheckAlarm = true;
            //                        }

            //                        if (item.IsCheckAlarm && !item.AlarmFlag
            //                            && (item.Temperature < item.TempRequired - GlobalVariable.ConfigSystem.ToleranceTempForRampUp
            //                                || item.Temperature > item.TempRequired + GlobalVariable.ConfigSystem.ToleranceTempForRampUp
            //                                )
            //                           )
            //                        {
            //                            //if (item.OvenId == 13)
            //                            //{
            //                            //    var a = 10;
            //                            //}

            //                            item.Alarm = 1;
            //                            item.SerienStatus = 1;
            //                            item.AlarmDescription = $"Nhiệt độ chưa đạt";
            //                            _alarmEnable[item.OvenId - 1] = true;
            //                            _alarmValue[item.OvenId - 1] = "1";
            //                            item.AlarmFlag = true;
            //                            item.ResetAlarmFlag = true;
            //                            item.IsCheckAlarm = false;

            //                            Debug.WriteLine($"On Alarm {item.StepName.ToString()}|Check nhiet do thoi gian thuc|Status={item.Status}|EndStep={item.EndStep}|OvenId={item.OvenId} - T={item.Temperature}|tRequiered={item.TempRequired}|isCheckAlarm={item.IsCheckAlarm}|AlarmFlag={item.AlarmFlag}");

            //                        }
            //                        else if (item.IsCheckAlarm && item.AlarmFlag
            //                            && (item.Temperature >= item.TempRequired - GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp
            //                                && item.Temperature <= item.TempRequired + GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp
            //                                )
            //                           )
            //                        {
            //                            //if (item.OvenId == 13)
            //                            //{

            //                            //    var a = 10;
            //                            //}

            //                            item.Alarm = 0;
            //                            item.SerienStatus = 0;
            //                            item.AlarmDescription = null;

            //                            _alarmEnable[item.OvenId - 1] = true;
            //                            _alarmValue[item.OvenId - 1] = "0";
            //                            item.AlarmFlag = false;

            //                            item.IsCheckAlarm = false;
            //                            item.ResetAlarmFlag = true;

            //                            Debug.WriteLine($"Off Alarm {item.StepName.ToString()}|Check nhiet do thoi gian thuc|Status={item.Status}|EndStep={item.EndStep}|OvenId={item.OvenId} - T={item.Temperature}|tRequiered={item.TempRequired}|isCheckAlarm={item.IsCheckAlarm}|AlarmFlag={item.AlarmFlag}");
            //                        }
            //                        else if (!item.AlarmFlag && !item.ResetAlarmFlag)
            //                        {
            //                            if (item.OvenId == 13)
            //                            {
            //                                var a = 10;
            //                            }

            //                            item.AlarmFlag = true;
            //                            item.ResetAlarmFlag = true;
            //                        }
            //                    }
            //                    else//so sánh của bước cũ, khi vừa kết thúc bước, chuyển qua bước khác.
            //                    {
            //                        if (item.OvenId == 13)
            //                        {
            //                            var a = 10;
            //                        }

            //                        if (
            //                            (item.Temperature < (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTempForRampUp)
            //                           || item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempForRampUp))
            //                           && item.AlarmFlagLastStep == false
            //                           )
            //                        {
            //                            item.Alarm = 1;
            //                            item.SerienStatus = 1;
            //                            item.AlarmDescription = $"Nhiệt độ chưa đạt";
            //                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
            //                            _alarmEnable[item.OvenId - 1] = true;
            //                            _alarmValue[item.OvenId - 1] = "1";
            //                            //log DB alarm
            //                            //var p = new DynamicParameters();
            //                            //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //                            item.AlarmFlagLastStep = true;
            //                            item.AlarmFlag = false;
            //                            item.ResetAlarmFlag = true;

            //                            Debug.WriteLine($"On Alarm {item.StepName.ToString()}|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                        }
            //                        else
            //                        {

            //                        }
            //                    }
            //                }
            //                //rampTime giảm nhiệt(khi kết thúc bước soak) và bước end (khi kết thúc bước ramptime down) sẽ nhảy vào đây
            //                else if (item.SetPointLastStep > item.SetPoint)
            //                {
            //                    if (item.OvenId == 13)
            //                    {
            //                        var a = 10;
            //                    }

            //                    if (item.EndStep == 0)
            //                    {
            //                        //bước kế cuối, thường là bước ramp time down
            //                        if (item.StepName != EnumProfileStepType.End)
            //                        {
            //                            if (item.Temperature >= (item.SetPoint + GlobalVariable.ConfigSystem.ToleranceTempOutForRampDown)
            //                                && item.AlarmFlag == false
            //                               )
            //                            {
            //                                //if (item.OvenId == 13)
            //                                //{
            //                                //    var a = 10;
            //                                //}
            //                                item.AlarmFlag = true;
            //                                item.Alarm = 0;
            //                                item.SerienStatus = 0;
            //                                item.AlarmDescription = null;
            //                                //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
            //                                _alarmEnable[item.OvenId - 1] = true;
            //                                _alarmValue[item.OvenId - 1] = "0";
            //                                //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //                                item.AlarmFlagLastStep = false;
            //                                item.ResetAlarmFlag = true;

            //                                Debug.WriteLine($"Off Alarm {item.StepName.ToString()} - down and end|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                    $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                            }
            //                            else if (!item.AlarmFlag && !item.ResetAlarmFlag)
            //                            {
            //                                if (item.OvenId == 13)
            //                                {
            //                                    var a = 10;
            //                                }

            //                                item.AlarmFlag = true;
            //                                item.ResetAlarmFlag = true;
            //                            }
            //                        }
            //                        else
            //                        {
            //                            if (item.Temperature <= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOutForRampDown)
            //                              && item.AlarmFlag == false
            //                              )
            //                            {
            //                                //if (item.OvenId == 13)
            //                                //{
            //                                //    var a = 10;
            //                                //}
            //                                item.AlarmFlag = true;
            //                                item.Alarm = 0;
            //                                item.SerienStatus = 0;
            //                                item.AlarmDescription = null;
            //                                //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
            //                                _alarmEnable[item.OvenId - 1] = true;
            //                                _alarmValue[item.OvenId - 1] = "0";
            //                                //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //                                item.AlarmFlagLastStep = false;
            //                                item.ResetAlarmFlag = true;

            //                                Debug.WriteLine($"Off Alarm {item.StepName.ToString()} - down and end|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                    $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                            }
            //                            else if (!item.AlarmFlag && !item.ResetAlarmFlag)
            //                            {
            //                                if (item.OvenId == 13)
            //                                {
            //                                    var a = 10;
            //                                }

            //                                item.AlarmFlag = true;
            //                                item.ResetAlarmFlag = true;
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempForRampDown)
            //                            && item.AlarmFlagLastStep == false
            //                           )
            //                        {
            //                            //if (item.OvenId == 13)
            //                            //{
            //                            //    var a = 10;
            //                            //}
            //                            item.Alarm = 1;
            //                            item.SerienStatus = 1;
            //                            item.AlarmDescription = $"Nhiệt độ cao";
            //                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
            //                            _alarmEnable[item.OvenId - 1] = true;
            //                            _alarmValue[item.OvenId - 1] = "1";
            //                            //log DB alarm
            //                            //var p = new DynamicParameters();
            //                            item.AlarmFlagLastStep = true;
            //                            item.AlarmFlag = false;
            //                            item.ResetAlarmFlag = true;

            //                            Debug.WriteLine($"On Alarm {item.StepName.ToString()} - down and end|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                        }

            //                    }
            //                }
            //            }
            //            else if (item.Status == 0
            //                   && item.AlarmFlag == false)
            //            {
            //                if (item.OvenId == 13)
            //                {
            //                    var a = 10;
            //                }

            //                double setPoint = 0;
            //                if (item.StepName == EnumProfileStepType.End) setPoint = item.SetPointLastStep;
            //                else setPoint = item.SetPoint;

            //                if (item.Temperature <= (setPoint + GlobalVariable.ConfigSystem.ToleranceTempOutForRampDown))
            //                {
            //                    //if (item.OvenId == 13)
            //                    //{
            //                    //    var a = 10;
            //                    //}

            //                    item.Alarm = 0;
            //                    item.SerienStatus = 0;
            //                    item.AlarmDescription = null;
            //                    //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
            //                    _alarmEnable[item.OvenId - 1] = true;
            //                    _alarmValue[item.OvenId - 1] = "0";
            //                    item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //                    item.AlarmFlagLastStep = false;
            //                    item.AlarmFlag = true;

            //                    Debug.WriteLine($"Off Alarm {item.StepName.ToString()}|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
            //                                                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
            //                }
            //            }

            //            #region kiểm tra xem có tín hiệu tắt còi từ server thì tắt còi
            //            var c = _controlPlcModel.FirstOrDefault(x => x.OvenId == item.OvenId);
            //            if (c.OffSerien == 1)
            //            {
            //                _alarmEnable[item.OvenId - 1] = true;
            //                _alarmValue[item.OvenId - 1] = "0";
            //                c.OffSerien = 0;
            //                item.SerienStatus = 0;//gui tin hieu bao bat tat coi

            //                //cập nhật tắt tín hiệu tắt còi.
            //                using (var con = GlobalVariable.GetDbConnection())
            //                {
            //                    var p = new DynamicParameters();
            //                    p.Add("id", _ft06.FirstOrDefault().Id);
            //                    p.Add("c000", JsonConvert.SerializeObject(_controlPlcModel));

            //                    con.Execute("sp_FT06Update", param: p, commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds);
            //                }
            //            }

            //            #endregion

            //            index += 1;
            //            item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
            //        }
            //        #endregion

            //        #region Đọc DB để lấy tín hiệu tắt còi từ web
            //        using (var con = GlobalVariable.GetDbConnection())
            //        {
            //            _ft06 = con.Query<FT06>("sp_FT06GetAll", commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds).ToList();
            //            if (_ft06 != null)
            //            {
            //                _controlPlcModel = JsonConvert.DeserializeObject<List<ControlPlcModel>>(_ft06.FirstOrDefault().C000);
            //            }
            //        }
            //        #endregion

            //        OnOffSerien();//gọi method kiểm tra bật tắt còi

            //        //lấy các thông số config
            //        RefreshConfig();

            //        #region Kiểm tra kết nối đến easy driver server
            //        GlobalVariable.InvokeIfRequired(this, () =>
            //        {
            //            _labSriverStatus.Text = easyDriverConnector1.ConnectionStatus.ToString();
            //            if (easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected)
            //            {
            //                _pnStatus.BackColor = Color.Green;
            //            }
            //            else
            //            {
            //                _pnStatus.BackColor = Color.Red;
            //            }
            //        });
            //        #endregion
            //    }
            //}
            //catch (Exception ex) { Log.Error(ex, "From _timer_Tick"); }
            //finally
            //{
            //    t.Enabled = true;
            //}
        }

        private async Task TaskTimerAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_isLoaded)
                    {
                        if (this.InvokeRequired)
                        {
                            this?.Invoke(new Action(() =>
                            {
                                _labTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            }));
                        }
                        else
                        {
                            _labTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        }

                        #region check alarm
                        int index = 1;
                        foreach (var item in _displayRealtime)
                        {
                            if (item.Status == 1 && item.CountSecondTagChange >= 2)
                            {
                                //Cảnh báo toàn thời gian

                                //rampTime tăng nhiệt
                                if (item.SetPointLastStep < item.SetPoint)
                                {
                                    if (item.EndStep == 0)//khi không có kết thúc bước thì vào kiểm tra nhiệt độ theo khoảng thời gian
                                    {
                                        if (
                                            item.Temperature <= (item.SetPoint + GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp)
                                          //&& item.Temperature >= (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp))
                                          && item.AlarmFlag == false
                                           )
                                        {
                                            //if (item.OvenId == 13)
                                            //{
                                            //    var a = 10;
                                            //}

                                            item.AlarmFlag = true;
                                            item.Alarm = 0;
                                            item.SerienStatus = 0;
                                            item.AlarmDescription = null;
                                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                            _alarmEnable[item.OvenId - 1] = true;
                                            _alarmValue[item.OvenId - 1] = "0";

                                            //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                            item.AlarmFlagLastStep = false;
                                            item.ResetAlarmFlag = true;

                                            Debug.WriteLine($"Off Alarm {item.StepName.ToString()} up|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                        }
                                        else if (item.AlarmFlag && !item.ResetAlarmFlag)
                                        {
                                            if (item.OvenId == 13)
                                            {
                                                var a = 10;
                                            }

                                            item.AlarmFlag = false;
                                            item.ResetAlarmFlag = true;
                                        }
                                    }
                                    else//so sánh của bước cũ, khi vừa kết thúc bước, chuyển qua bước khác.
                                    {
                                        if (
                                            (item.Temperature < (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTempForRampUp)
                                           || item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempForRampUp))
                                           && item.AlarmFlagLastStep == false
                                           )
                                        {
                                            //if (item.OvenId == 13)
                                            //{
                                            //    var a = 10;
                                            //}

                                            item.Alarm = 1;
                                            item.SerienStatus = 1;
                                            item.AlarmDescription = $"Nhiệt độ chưa đạt";
                                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
                                            _alarmEnable[item.OvenId - 1] = true;
                                            _alarmValue[item.OvenId - 1] = "1";

                                            //log DB alarm
                                            //var p = new DynamicParameters();
                                            //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                            item.AlarmFlagLastStep = true;
                                            item.AlarmFlag = false;
                                            item.ResetAlarmFlag = true;

                                            Debug.WriteLine($"On Alarm {item.StepName.ToString()} Up|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                        }
                                    }
                                }
                                //soak-ngâm
                                else if (item.SetPointLastStep == item.SetPoint)
                                {
                                    if (item.OvenId == 13)
                                    {
                                        var a = 10;
                                    }
                                    if (item.EndStep == 0)
                                    {
                                        item.EndTimeAlarm = DateTime.Now;
                                        var tAlarm = (item.EndTimeAlarm - item.BeginTimeAlarm).TotalMilliseconds;

                                        //tính ra khoảng nhiệt cần tăng đến thời điểm hiện tại
                                        item.TempRequired = item.SetPoint;

                                        if (tAlarm > GlobalVariable.ConfigSystem.TimeTempChange)
                                        {
                                            item.IsCheckAlarm = true;
                                        }

                                        if (item.IsCheckAlarm && !item.AlarmFlag
                                            && (item.Temperature < item.TempRequired - GlobalVariable.ConfigSystem.ToleranceTempForRampUp
                                                || item.Temperature > item.TempRequired + GlobalVariable.ConfigSystem.ToleranceTempForRampUp
                                                )
                                           )
                                        {
                                            //if (item.OvenId == 13)
                                            //{
                                            //    var a = 10;
                                            //}

                                            item.Alarm = 1;
                                            item.SerienStatus = 1;
                                            item.AlarmDescription = $"Nhiệt độ chưa đạt";
                                            _alarmEnable[item.OvenId - 1] = true;
                                            _alarmValue[item.OvenId - 1] = "1";
                                            item.AlarmFlag = true;
                                            item.ResetAlarmFlag = true;
                                            item.IsCheckAlarm = false;

                                            Debug.WriteLine($"On Alarm {item.StepName.ToString()}|Check nhiet do thoi gian thuc|Status={item.Status}|EndStep={item.EndStep}|OvenId={item.OvenId} - T={item.Temperature}|tRequiered={item.TempRequired}|isCheckAlarm={item.IsCheckAlarm}|AlarmFlag={item.AlarmFlag}");

                                        }
                                        else if (item.IsCheckAlarm && item.AlarmFlag
                                            && (item.Temperature >= item.TempRequired - GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp
                                                && item.Temperature <= item.TempRequired + GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp
                                                )
                                           )
                                        {
                                            //if (item.OvenId == 13)
                                            //{

                                            //    var a = 10;
                                            //}

                                            item.Alarm = 0;
                                            item.SerienStatus = 0;
                                            item.AlarmDescription = null;

                                            _alarmEnable[item.OvenId - 1] = true;
                                            _alarmValue[item.OvenId - 1] = "0";
                                            item.AlarmFlag = false;

                                            item.IsCheckAlarm = false;
                                            item.ResetAlarmFlag = true;

                                            Debug.WriteLine($"Off Alarm {item.StepName.ToString()}|Check nhiet do thoi gian thuc|Status={item.Status}|EndStep={item.EndStep}|OvenId={item.OvenId} - T={item.Temperature}|tRequiered={item.TempRequired}|isCheckAlarm={item.IsCheckAlarm}|AlarmFlag={item.AlarmFlag}");
                                        }
                                        else if (!item.AlarmFlag && !item.ResetAlarmFlag)
                                        {
                                            if (item.OvenId == 13)
                                            {
                                                var a = 10;
                                            }

                                            item.AlarmFlag = true;
                                            item.ResetAlarmFlag = true;
                                        }
                                    }
                                    else//so sánh của bước cũ, khi vừa kết thúc bước, chuyển qua bước khác.
                                    {
                                        if (item.OvenId == 13)
                                        {
                                            var a = 10;
                                        }

                                        if (
                                            (item.Temperature < (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTempForRampUp)
                                           || item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempForRampUp))
                                           && item.AlarmFlagLastStep == false
                                           )
                                        {
                                            item.Alarm = 1;
                                            item.SerienStatus = 1;
                                            item.AlarmDescription = $"Nhiệt độ chưa đạt";
                                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
                                            _alarmEnable[item.OvenId - 1] = true;
                                            _alarmValue[item.OvenId - 1] = "1";
                                            //log DB alarm
                                            //var p = new DynamicParameters();
                                            //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                            item.AlarmFlagLastStep = true;
                                            item.AlarmFlag = false;
                                            item.ResetAlarmFlag = true;

                                            Debug.WriteLine($"On Alarm {item.StepName.ToString()}|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                                //rampTime giảm nhiệt(khi kết thúc bước soak) và bước end (khi kết thúc bước ramptime down) sẽ nhảy vào đây
                                else if (item.SetPointLastStep > item.SetPoint)
                                {
                                    if (item.OvenId == 13)
                                    {
                                        var a = 10;
                                    }

                                    if (item.EndStep == 0)
                                    {
                                        //bước kế cuối, thường là bước ramp time down
                                        if (item.StepName != EnumProfileStepType.End)
                                        {
                                            if (item.Temperature >= (item.SetPoint + GlobalVariable.ConfigSystem.ToleranceTempOutForRampDown)
                                                && item.AlarmFlag == false
                                               )
                                            {
                                                //if (item.OvenId == 13)
                                                //{
                                                //    var a = 10;
                                                //}
                                                item.AlarmFlag = true;
                                                item.Alarm = 0;
                                                item.SerienStatus = 0;
                                                item.AlarmDescription = null;
                                                //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                                _alarmEnable[item.OvenId - 1] = true;
                                                _alarmValue[item.OvenId - 1] = "0";
                                                //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                                item.AlarmFlagLastStep = false;
                                                item.ResetAlarmFlag = true;

                                                Debug.WriteLine($"Off Alarm {item.StepName.ToString()} - down and end|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                    $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                            }
                                            else if (!item.AlarmFlag && !item.ResetAlarmFlag)
                                            {
                                                if (item.OvenId == 13)
                                                {
                                                    var a = 10;
                                                }

                                                item.AlarmFlag = true;
                                                item.ResetAlarmFlag = true;
                                            }
                                        }
                                        else
                                        {
                                            if (item.Temperature <= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOutForRampDown)
                                              && item.AlarmFlag == false
                                              )
                                            {
                                                //if (item.OvenId == 13)
                                                //{
                                                //    var a = 10;
                                                //}
                                                item.AlarmFlag = true;
                                                item.Alarm = 0;
                                                item.SerienStatus = 0;
                                                item.AlarmDescription = null;
                                                //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                                _alarmEnable[item.OvenId - 1] = true;
                                                _alarmValue[item.OvenId - 1] = "0";
                                                //item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                                item.AlarmFlagLastStep = false;
                                                item.ResetAlarmFlag = true;

                                                Debug.WriteLine($"Off Alarm {item.StepName.ToString()} - down and end|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                    $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                            }
                                            else if (!item.AlarmFlag && !item.ResetAlarmFlag)
                                            {
                                                if (item.OvenId == 13)
                                                {
                                                    var a = 10;
                                                }

                                                item.AlarmFlag = true;
                                                item.ResetAlarmFlag = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempForRampDown)
                                            && item.AlarmFlagLastStep == false
                                           )
                                        {
                                            //if (item.OvenId == 13)
                                            //{
                                            //    var a = 10;
                                            //}
                                            item.Alarm = 1;
                                            item.SerienStatus = 1;
                                            item.AlarmDescription = $"Nhiệt độ cao";
                                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
                                            _alarmEnable[item.OvenId - 1] = true;
                                            _alarmValue[item.OvenId - 1] = "1";
                                            //log DB alarm
                                            //var p = new DynamicParameters();
                                            item.AlarmFlagLastStep = true;
                                            item.AlarmFlag = false;
                                            item.ResetAlarmFlag = true;

                                            Debug.WriteLine($"On Alarm {item.StepName.ToString()} - down and end|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                        }

                                    }
                                }
                            }
                            else if (item.Status == 0
                                   && item.AlarmFlag == false)
                            {
                                if (item.OvenId == 13)
                                {
                                    var a = 10;
                                }

                                double setPoint = 0;
                                if (item.StepName == EnumProfileStepType.End) setPoint = item.SetPointLastStep;
                                else setPoint = item.SetPoint;

                                if (item.Temperature <= (setPoint + GlobalVariable.ConfigSystem.ToleranceTempOutForRampDown))
                                {
                                    //if (item.OvenId == 13)
                                    //{
                                    //    var a = 10;
                                    //}

                                    item.Alarm = 0;
                                    item.SerienStatus = 0;
                                    item.AlarmDescription = null;
                                    //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                    _alarmEnable[item.OvenId - 1] = true;
                                    _alarmValue[item.OvenId - 1] = "0";
                                    item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                    item.AlarmFlagLastStep = false;
                                    item.AlarmFlag = true;

                                    Debug.WriteLine($"Off Alarm {item.StepName.ToString()}|Status={item.Status}|EndStep = {item.EndStep}| T: {item.Temperature}|SetPointLastStep={item.SetPointLastStep}" +
                                                                                $"|setpoint={item.SetPoint}|RampUpOn={GlobalVariable.ConfigSystem.ToleranceTempForRampUp}RampUpOff={GlobalVariable.ConfigSystem.ToleranceTempOutForRampUp}");
                                }
                            }

                            #region kiểm tra xem có tín hiệu tắt còi từ server thì tắt còi
                            var c = _controlPlcModel.FirstOrDefault(x => x.OvenId == item.OvenId);
                            if (c.OffSerien == 1)
                            {
                                _alarmEnable[item.OvenId - 1] = true;
                                _alarmValue[item.OvenId - 1] = "0";
                                c.OffSerien = 0;
                                item.SerienStatus = 0;//gui tin hieu bao bat tat coi

                                //cập nhật tắt tín hiệu tắt còi.
                                using (var con = GlobalVariable.GetDbConnection())
                                {
                                    var p = new DynamicParameters();
                                    p.Add("id", _ft06.FirstOrDefault().Id);
                                    p.Add("c000", JsonConvert.SerializeObject(_controlPlcModel));

                                    con.Execute("sp_FT06Update", param: p, commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds);
                                }
                            }

                            #endregion

                            index += 1;
                            item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                        }
                        #endregion

                        #region Đọc DB để lấy tín hiệu tắt còi từ web
                        using (var con = GlobalVariable.GetDbConnection())
                        {
                            _ft06 = con.Query<FT06>("sp_FT06GetAll", commandType: CommandType.StoredProcedure, commandTimeout: (int)TimeSpan.FromMinutes(30).TotalMilliseconds).ToList();
                            if (_ft06 != null)
                            {
                                _controlPlcModel = JsonConvert.DeserializeObject<List<ControlPlcModel>>(_ft06.FirstOrDefault().C000);
                            }
                        }
                        #endregion

                        OnOffSerien();//gọi method kiểm tra bật tắt còi

                        //lấy các thông số config
                        RefreshConfig();

                        #region Kiểm tra kết nối đến easy driver server
                        GlobalVariable.InvokeIfRequired(this, () =>
                        {
                            _labSriverStatus.Text = easyDriverConnector1.ConnectionStatus.ToString();
                            if (easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected)
                            {
                                _pnStatus.BackColor = Color.Green;
                            }
                            else
                            {
                                _pnStatus.BackColor = Color.Red;
                            }
                        });
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    Log.Error($"Lỗi trong TaskTimerAsync: {ex.Message}");
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

                    //    // === xử lý 1 lần duy nhất mỗi lần được kích ===
                    //    Debug.WriteLine($"TaskCheckTimeStepAsync triggered {START_STOP_STEP}");

                    //    await Task.Delay(GlobalVariable.RevoConfig.IntervalResetShaft, token); // Chờ 1 giây trước khi lặp lại

                    //    _revoRealtimeModel.Steps.ForEach(x =>
                    //    {
                    //        x.StartAt = null;
                    //        x.EndAt = null;
                    //        x.TotalRunTime = 0;
                    //    });
                    //    _revoRealtimeModel.ShaftNum = Guid.NewGuid();

                    //    LoadStepsToFlowPanel();

                    //    var step = _revoRealtimeModel.Steps
                    //        .FirstOrDefault(s =>
                    //            s.Enable == true &&
                    //            s.StartAt == null
                    //        );

                    //    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
                    //                                          , step.Speed_Hz.HasValue ? step.Speed_Hz.Value.ToString() : "0"
                    //                                           , WritePiority.High);

                    //    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SO_LUONG_XUNG"
                    //                                           , step.SoLuongXung.HasValue ? step.SoLuongXung.Value.ToString() : "0"
                    //                                            , WritePiority.High);

                    //Loop:
                    //    await _easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/SENT"
                    //                                        , "1"
                    //                                         , WritePiority.High);
                    //    if (!SENT)
                    //    {
                    //        goto Loop;
                    //    }

                    //    UpdateStepUI(step, 1);

                    //    LogDb(isNew: true);
                    //    // xong việc → quay lại vòng chờ (không cần Delay hay poll)
                }
            }
            catch (OperationCanceledException) { /* thoát êm */ }
        }

        private void EasyDriverConnector1_Started(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
            #region Oven
            foreach (var item in _ovensInfo)
            {
                easyDriverConnector1.GetTag($"{item.Path}/Temperature").QualityChanged += Temperature_QualityChanged;

                easyDriverConnector1.GetTag($"{item.Path}/Temperature").ValueChanged += Temperature_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/DigitalInput1Status").ValueChanged += DigitalInput1Status_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus").ValueChanged += ProfileNumber_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus").ValueChanged += ProfileStepNumber_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/ProfileStepType_CurrentStatus").ValueChanged += ProfileStepType_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/HoursRemaining_CurrentStatus").ValueChanged += HoursRemaining_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/MinutesRemaining_CurrentStatus").ValueChanged += MinutesRemaining_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/SecondsRemaining_CurrentStatus").ValueChanged += SecondsRemaining_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/EndSetPointCh1_CurrentStatus").ValueChanged += EndSetPointCh1_CurrentStatus_ValueChanged;

                #region Comment
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C1").ValueChanged += Profile1Name_C1_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C2").ValueChanged += Profile1Name_C2_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C3").ValueChanged += Profile1Name_C3_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C4").ValueChanged += Profile1Name_C4_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C5").ValueChanged += Profile1Name_C5_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C6").ValueChanged += Profile1Name_C6_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C7").ValueChanged += Profile1Name_C7_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C8").ValueChanged += Profile1Name_C8_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C9").ValueChanged += Profile1Name_C9_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C10").ValueChanged += Profile1Name_C10_ValueChanged;

                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C1").ValueChanged += Profile2Name_C1_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C2").ValueChanged += Profile2Name_C2_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C3").ValueChanged += Profile2Name_C3_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C4").ValueChanged += Profile2Name_C4_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C5").ValueChanged += Profile2Name_C5_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C6").ValueChanged += Profile2Name_C6_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C7").ValueChanged += Profile2Name_C7_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C8").ValueChanged += Profile2Name_C8_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C9").ValueChanged += Profile2Name_C9_ValueChanged;
                //easyDriverConnector1.GetTag($"{item.Path}/Profile2Name_C10").ValueChanged += Profile2Name_C10_ValueChanged;

                //Profile1Name_C1_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C1")
                //   , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C1")
                //   , "", easyDriverConnector1.GetTag($"{item.Path}/Profile1Name_C1").Value));
                #endregion

                Temperature_QualityChanged(easyDriverConnector1.GetTag($"{item.Path}/Temperature")
             , new TagQualityChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Temperature")
             , Quality.Uncertain, easyDriverConnector1.GetTag($"{item.Path}/Temperature").Quality));

                Temperature_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Temperature")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Temperature")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/Temperature").Value));

                DigitalInput1Status_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/DigitalInput1Status")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/DigitalInput1Status")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/DigitalInput1Status").Value));

                ProfileNumber_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus").Value));

                ProfileStepType_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/ProfileStepType_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/ProfileStepType_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/ProfileStepType_CurrentStatus").Value));

                HoursRemaining_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/HoursRemaining_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/HoursRemaining_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/HoursRemaining_CurrentStatus").Value));

                MinutesRemaining_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/MinutesRemaining_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/MinutesRemaining_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/MinutesRemaining_CurrentStatus").Value));

                SecondsRemaining_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/SecondsRemaining_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/SecondsRemaining_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/SecondsRemaining_CurrentStatus").Value));

                EndSetPointCh1_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/EndSetPointCh1_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/EndSetPointCh1_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/EndSetPointCh1_CurrentStatus").Value));

                ProfileStepNumber_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus")
                   , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus")
                   , "", easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus").Value));
            }
            #endregion

            #region auto rolling
            foreach (var item in _revoConfigs)
            {
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name1").QualityChanged += Part_Name1_QualityChanged;

                easyDriverConnector1.GetTag($"{item.Path}/Part_Name1").ValueChanged += Part_Name1_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name2").ValueChanged += Part_Name2_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name3").ValueChanged += Part_Name3_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name4").ValueChanged += Part_Name4_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name5").ValueChanged += Part_Name5_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name6").ValueChanged += Part_Name6_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name7").ValueChanged += Part_Name7_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/Part_Name8").ValueChanged += Part_Name8_ValueChanged;

                Part_Name1_QualityChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name1")
             , new TagQualityChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name1")
             , Quality.Uncertain, easyDriverConnector1.GetTag($"{item.Path}/Part_Name1").Quality));
                Part_Name1_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name1")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name1")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name1").Value));
                Part_Name2_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name2")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name2")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name2").Value));
                Part_Name3_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name3")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name3")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name3").Value));
                Part_Name4_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name4")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name4")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name4").Value));
                Part_Name5_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name5")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name5")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name5").Value));
                Part_Name6_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name6")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name6")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name6").Value));
                Part_Name7_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name7")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name7")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name7").Value));
                Part_Name8_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/Part_Name8")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/Part_Name8")
                , "", easyDriverConnector1.GetTag($"{item.Path}/Part_Name8").Value));
            }
            #endregion


            //GlobalVariable.InvokeIfRequired(this, () =>
            //{
            //    _labSriverStatus.Text = easyDriverConnector1.ConnectionStatus.ToString();
            //    if (easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected)
            //    {
            //        _pnStatus.BackColor = Color.Green;
            //    }
            //    else
            //    {
            //        _pnStatus.BackColor = Color.Red;
            //    }
            //});
        }

        #region Event tag value change
        #region Oven
        private async void Temperature_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var al = deviceName.Substring(4);

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        item.ConnectionStatus = e.NewQuality == Quality.Good ? 1 : 0;

                        if (item.ConnectionStatus == 0)
                        {
                            item.AlarmDescription = "Mất kết nối đến lò";
                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{al}", "2", WritePiority.High);
                            _alarmEnable[item.OvenId - 1] = true;
                            _alarmValue[item.OvenId - 1] = "2";
                            item.SerienStatus = 1;
                        }
                        else
                        {
                            item.AlarmDescription = null;
                            //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{al}", "0", WritePiority.High);
                            _alarmEnable[item.OvenId - 1] = true;
                            _alarmValue[item.OvenId - 1] = "0";
                            item.SerienStatus = 0;
                        }

                        Debug.WriteLine($"Alarm description {item.OvenId}:{item.AlarmDescription}");
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Temperature_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        //Debug.WriteLine($"{path}/Tempperature: {e.NewValue}");
                        item.Temperature = double.TryParse(e.NewValue, out double value) ? Math.Round(value * GlobalVariable.ConfigSystem.Gain, 1) : item.Temperature;

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private async void DigitalInput1Status_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var al = deviceName.Substring(4);

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        //Watlow tra tin hieu ve khi co tác động là 0, còn không tác động là 1.
                        item.DoorStatus = e.NewValue == "1" ? 1 : 0;

                        if (e.NewValue == "0")
                        {
                            item.AlarmDescription = "Cửa lò mở";

                            if (item.Status == 1)
                            {
                                //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{al}", "1", WritePiority.High);
                                _alarmEnable[item.OvenId - 1] = true;
                            }
                        }
                        else
                        {
                            item.AlarmDescription = null;

                            if (item.Status == 1)
                            {
                                //await easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{al}", "0", WritePiority.High);
                                _alarmEnable[item.OvenId - 1] = false;
                            }
                        }
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void EndSetPointCh1_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        //item.SetPoint = double.TryParse(e.NewValue, out double value) ? value : item.SetPoint;
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void SecondsRemaining_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        item.StatusTimeBegin = DateTime.Now;

                        item.SecondsRemaining_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.SecondsRemaining_CurrentStatus;

                        if (item.CountSecondTagChange > 1000) item.CountSecondTagChange = 1;

                        item.CountSecondTagChange += 1;

                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void MinutesRemaining_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        item.MinutesRemaining_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.MinutesRemaining_CurrentStatus;
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void HoursRemaining_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        item.HoursRemaining_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.HoursRemaining_CurrentStatus;
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void ProfileStepType_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        item.ProfileStepType_CurrentStatus = e.NewValue == "1" ? EnumProfileStepType.RampTime
                                                            : e.NewValue == "2" ? EnumProfileStepType.RampRate
                                                            : e.NewValue == "3" ? EnumProfileStepType.Soak
                                                            : e.NewValue == "4" ? EnumProfileStepType.Jump
                                                            : EnumProfileStepType.End;
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void ProfileStepNumber_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        if (item.OvenId == 11)
                        {
                            var a = 11;
                        }

                        item.ProfileStepNumber_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.ProfileStepNumber_CurrentStatus;
                        var profile = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus);
                        item.ProfileName = profile?.Name;
                        var step = profile?.Steps.FirstOrDefault(x => x.Id == item.ProfileStepNumber_CurrentStatus);

                        if (item.SetPoint > 0)
                        {
                            if (item.ProfileStepNumber_CurrentStatus != 1)
                            {
                                item.SetPointLastStep = item.SetPoint;
                                item.LastStepType = item.StepName;
                                item.EndStep = 1;
                            }
                            else
                            {
                                item.SetPointLastStep = item.Temperature;
                                item.LastStepType = 0;
                                item.EndStep = 0;
                            }

                            _isReOpenApp = false;
                        }
                        else
                        {
                            _isReOpenApp = true;
                            var lastStep = profile?.Steps.FirstOrDefault(x => x.Id == item.ProfileStepNumber_CurrentStatus - 1);

                            if (lastStep != null)
                            {
                                item.SetPointLastStep = lastStep.SetPoint;
                                item.LastStepType = lastStep.StepType;
                            }
                            else
                            {
                                item.SetPointLastStep = item.Temperature;
                                item.LastStepType = 0;
                            }
                        }

                        item.BeginTimeAlarm = DateTime.Now;
                        item.BeginTimeOfStep = DateTime.Now;
                        item.TempBeginStep = item.Temperature;
                        item.IsCheckAlarm = false;
                        item.AlarmFlag = false;
                        item.AlarmFlagLastStep = false;
                        //item.StatusFlag = false;

                        //cập nhật các thông số cảu step mới vào để chạy

                        if (step != null)
                        {
                            item.StepName = step.StepType;
                            item.Hours = (int)(step?.Hours); item.Minutes = (int)(step?.Minutes); item.Seconds = (int)(step?.Seconds);
                            item.SetPoint = (double)(step?.SetPoint);

                            item.TempRange = Math.Round(Math.Abs((item.SetPoint - item.Temperature)), 2);
                        }

                        _isLoaded = true;//báo đã khởi tạo xong cho phép các task chạy

                        Debug.WriteLine($"{item.OvenName} - {item.StepName} - EndStep: {item.EndStep} - Last set point: {item.SetPointLastStep} - set point: {item.SetPoint}");
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void ProfileNumber_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        item.ProfileNumber_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.ProfileNumber_CurrentStatus;

                        var profile = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus);
                        item.ProfileName = profile?.Name;
                        item.LevelUp = (double)(profile?.LevelUp);
                        item.LevelDown = (double)(profile?.LevelDown);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Profile1Name_C1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _displayRealtime)
                {
                    if (item.Path == path)
                    {
                        var c = (char)(int.TryParse(e.NewValue, out int value) ? value : 32);

                        Debug.WriteLine($"{e.Tag.Path}:{c}");
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        #endregion

        #region Revo autorolling
        private async void Part_Name1_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;
                var deviceName = e.Tag.Parent.Name;
                var al = deviceName.Substring(4);

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        item.PlcConnected = e.NewQuality == Quality.Good ? true : false;

                        Log.Warning($"Alarm description {item.RevoName}: loss connection to PLC.");
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Part_Name1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name1 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }

        private void Part_Name2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name2 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        private void Part_Name3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name3 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        private void Part_Name4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name4 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        private void Part_Name5_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name5 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        private void Part_Name6_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name6 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        private void Part_Name7_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name7 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        private void Part_Name8_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            try
            {
                var path = e.Tag.Parent.Path;

                foreach (var item in _revoRealtimeModel)
                {
                    if (item.Path == path)
                    {
                        var tagChange = _autoRollingTagChangeds.FirstOrDefault(x => x.RevoId == item.RevoId);
                        tagChange.Part_Name8 = int.TryParse(e.NewValue, out int value) ? value : tagChange.Part_Name1;

                        var words = new int[]
                        {
                            tagChange.Part_Name1,
                            tagChange.Part_Name2,
                            tagChange.Part_Name3,
                            tagChange.Part_Name4,
                            tagChange.Part_Name5,
                            tagChange.Part_Name6,
                            tagChange.Part_Name7,
                            tagChange.Part_Name8,
                        };
                        item.Part = GlobalVariable.DecodeGotString(words);

                        Debug.WriteLine(item.Part);
                        return;
                    }
                }
            }
            catch (Exception ex) { Log.Error(ex, $"From TagValueChanged {e.Tag.Path}"); }
        }
        #endregion
        #endregion

        #region Auto rolling
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

                foreach (var itemRealtime in _revoRealtimeModel)
                {
                    if (isNew)
                    {
                        var dataLogs = new List<FT09_RevoDatalog>();

                        foreach (var itemStep in itemRealtime.Steps.Where(x => x.Enable == true))
                        {
                            var nl = new FT09_RevoDatalog()
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = createdAt,
                                CreatedMachine = createdMachine,
                                RevoId = itemRealtime.RevoId,
                                RevoName = itemRealtime.RevoName,
                                Work = itemRealtime.Work,
                                Part = itemRealtime.Part,
                                Rev = itemRealtime.Rev,
                                ColorCode = itemRealtime.ColorCode,
                                Mandrel = itemRealtime.Mandrel,
                                MandrelStart = itemRealtime.MandrelStart,
                                ShaftNum = itemRealtime.ShaftNum,
                                StepId = itemStep.StepIndex,
                                StepName = itemStep.StepName,
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
                                x.ShaftNum == itemRealtime.ShaftNum &&
                                x.RevoId == itemRealtime.RevoId
                            )
                            .ToListAsync();

                        if (dataLogUpdate != null && dataLogUpdate.Count > 0)
                        {
                            foreach (var itemUpdate in dataLogUpdate)
                            {
                                var lineUpdate = itemRealtime.Steps.FirstOrDefault(x => x.StepIndex == itemUpdate.StepId);

                                itemUpdate.StartedAt = lineUpdate?.StartAt;
                                itemUpdate.EndedAt = lineUpdate?.EndAt;
                                itemUpdate.StartedAt = lineUpdate?.StartAt;
                                itemUpdate.TotalTime = lineUpdate?.TotalRunTime;
                                itemUpdate.Work = itemRealtime.Work;
                            }

                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                #endregion

                #region realtime log  
                var rl = await dbContext.FT08_RevoRealtimes.FirstOrDefaultAsync(x => x.C000_RevoId == GlobalVariable.RevoId);

                if (rl != null)
                {
                    rl.C001_Data = JsonConvert.SerializeObject(_revoRealtimeModel);
                }
                else
                {
                    var nl = new FT08_RevoRealtime()
                    {
                        Id = Guid.NewGuid(),
                        C000_RevoId = GlobalVariable.RevoId,
                        C001_Data = JsonConvert.SerializeObject(_revoRealtimeModel)
                    };

                    dbContext.FT08_RevoRealtimes.Add(nl);
                }
                #endregion

                await dbContext.SaveChangesAsync();
            }
        }
        #endregion

        #endregion
    }
}
