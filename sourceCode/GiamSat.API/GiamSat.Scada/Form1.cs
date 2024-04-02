using Dapper;
using EasyScada.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GiamSat.Models;
using EasyScada.Winforms.Controls;
using System.Diagnostics;
using System.Linq.Expressions;
using GiamSat.Models.NotTable;


namespace GiamSat.Scada
{
    public partial class Form1 : Form
    {
        private List<FT03> _dataLog = new List<FT03>();
        private List<FT04> _dataLogProfile = new List<FT04>();
        private List<FT05> _alarm = new List<FT05>();
        private List<FT06> _ft06 = new List<FT06>();

        private OvensInfo _ovensInfo = new OvensInfo();//cau hinh cac lo oven
        private RealtimeDisplays _displayRealtime = new RealtimeDisplays();
        private List<ControlPlcModel> _controlPlcModel = new List<ControlPlcModel>();//đọc các tín hiệu điều khiển tắt đèn cảnh báo từ web.

        private Timer _timer = new Timer();
        private List<int> _OvenId = new List<int>();

        private DateTime _startTime, _endTime, _startTimeDisplay, _endTimeDisplay;
        private double _totalTime = 0, _totalTimeDisplay = 0;

        private Task _taskDataLog, _taskDataLogProfile, _taskAlarm;

        private DateTime _beginTimeCheckAlarm, _endTimeCheckAlarm;
        private double _totalTimeCheckAlarm = 0;

        #region Email
        private Email _email = new Email();
        AlarmSetting _alarmSetting = new AlarmSetting();
        #endregion

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region Get thong tin chuong
            GetOvensInfo();
            #endregion

            easyDriverConnector1.Started += EasyDriverConnector1_Started;

            _timer.Interval = 1000;
            _timer.Tick += _timer_Tick;
            _timer.Enabled = true;

            _startTime = _startTimeDisplay = _endTime = _endTimeDisplay = DateTime.Now;

            _taskDataLog = new Task(() => LogData());
            _taskDataLog.Start();

            _taskDataLogProfile = new Task(() => LogDataProfile());
            _taskDataLogProfile.Start();
        }

        #region Methods
        /// <summary>
        /// Khởi tạo data ban đầu.
        /// </summary>
        private void GetOvensInfo()
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
                        ToleranceTemp = 2
                        ,
                        ToleranceTempOut = 1
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
                            Steps = steps
                        });
                        profiles.Add(new ProfileModel()
                        {
                            Id = 2,
                            Name = $"Profile 2",
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
                    return;
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

                var index = 0;

                foreach (var item in _ovensInfo)
                {
                    index += 1;

                    _OvenId.Add(item.Id);

                    _dataLog.Add(new FT03()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                    });

                    _dataLogProfile.Add(new FT04()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                    });

                    var dt = DateTime.Now;
                    _displayRealtime.Add(new RealtimeDisplayModel()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                        Path = item.Path,
                        OvenInfo = item,
                        StatusFlag = false,
                        AlarmFlag = false,
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
        }

        private void LogDataProfile()
        {
            while (true)
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
                                if (item.Status == 1 && item.ZIndex != Guid.Empty && item.Temperature > 0 && item.CountSecondTagChange >= 2)
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

                                    con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);
                                }
                                else if (item.Status == 0 && item.ZIndex != Guid.Empty && item.Temperature > 0 && item.CountSecondTagChange >= 2)
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

                                    con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);

                                    item.ZIndex = Guid.Empty;
                                    item.ProfileName = null;
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
                        catch
                        {
                            tran.Rollback();
                            return;
                        }
                    }
                }

                System.Threading.Thread.Sleep(GlobalVariable.ConfigSystem.DataLogWhenRunProfileInterval);
            }
        }

        private void LogData()
        {
            while (true)
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
                        con.Execute("sp_FT02Insert", param: para, commandType: CommandType.StoredProcedure);
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

                            con.Execute("sp_FT03Insert", param: para, commandType: CommandType.StoredProcedure);
                        }
                    }
                }
                #endregion

                #region Kiểm tra xem lò chạy hay dùng để log data và cảnh báo
                foreach (var item in _displayRealtime)
                {
                    item.StatusTimeEnd = DateTime.Now;
                    if ((item.StatusTimeEnd - item.StatusTimeBegin).TotalSeconds <= GlobalVariable.ConfigSystem.CountSecondStop && item.StatusFlag == false)
                    {
                        item.Status = 1;//báo lò đang chạy.
                        item.ZIndex = Guid.NewGuid();
                        item.BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        item.StatusFlag = true;
                    }
                    if ((item.StatusTimeEnd - item.StatusTimeBegin).TotalSeconds > GlobalVariable.ConfigSystem.CountSecondStop && item.StatusFlag == true)
                    {
                        item.Status = 0;
                        //item.ZIndex = Guid.Empty;//được xóa khi đã log profile trạng thái dừng thành công LogProfile
                        item.StatusFlag = false;
                    }

                    //Debug.WriteLine($"{item.OvenName} status: {item.Status}");
                }
                #endregion

                #region Đọc DB để lấy tín hiệu tắt còi từ web
                using (var con = GlobalVariable.GetDbConnection())
                {
                    _ft06 = con.Query<FT06>("sp_FT06GetAll", commandType: CommandType.StoredProcedure).ToList();
                    if (_ft06 != null)
                    {
                        _controlPlcModel = JsonConvert.DeserializeObject<List<ControlPlcModel>>(_ft06.FirstOrDefault().C000);
                    }
                }
                #endregion

                System.Threading.Thread.Sleep(100);
            }
        }

        #endregion

        #region Events
        private void _timer_Tick(object sender, EventArgs e)
        {
            Timer t = (Timer)sender;
            t.Enabled = false;

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
                    //if (item.EndStep == 0)
                    {
                        //rampTime tăng nhiệt
                        if (item.SetPointLastStep < item.SetPoint)
                        {
                            if (item.EndStep == 0)
                            {

                            }
                            else//so xánh cảu bước cũ, khi vừa kết thúc bước, chuyển qua bước khác.
                            {
                                if (item.Temperature < (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTemp)
                                   //|| item.Temperature >= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp))
                                   && item.AlarmFlagLastStep == false
                                   )
                                {
                                    item.Alarm = 1;
                                    item.AlarmDescription = $"Nhiệt độ chưa đạt";
                                    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);

                                    //log DB alarm
                                    var p = new DynamicParameters();

                                    item.AlarmFlagLastStep = true;
                                    Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                }
                                else if (item.Temperature >= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOut)
                                    // && item.Temperature < (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp)
                                    //&& item.AlarmFlagLastStep == true
                                    )
                                {
                                    item.Alarm = 0;
                                    item.AlarmDescription = null;
                                    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                    item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                    item.AlarmFlagLastStep = false;

                                    Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                }
                            }
                        }
                        //soak-ngâm
                        else if (item.SetPointLastStep == item.SetPoint)
                        {
                            if (item.EndStep == 0)
                            {

                            }
                            else//so xánh cảu bước cũ, khi vừa kết thúc bước, chuyển qua bước khác.
                            {
                                if (item.Temperature < (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTemp)
                                   //|| item.Temperature >= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp))
                                   && item.AlarmFlagLastStep == false
                                   )
                                {
                                    item.Alarm = 1;
                                    item.AlarmDescription = $"Nhiệt độ chưa đạt";
                                    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);

                                    //log DB alarm
                                    var p = new DynamicParameters();

                                    item.AlarmFlagLastStep = true;
                                    Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                }
                                else if (item.Temperature >= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOut)
                                    //&& item.Temperature < (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp)
                                    //&& item.AlarmFlagLastStep == true
                                    )
                                {
                                    item.Alarm = 0;
                                    item.AlarmDescription = null;
                                    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                    item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                    item.AlarmFlagLastStep = false;

                                    Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                }
                            }
                        }
                        //rampTime giảm nhiệt
                        else if (item.SetPointLastStep > item.SetPoint)
                        {
                            if (item.EndStep == 0)
                            {

                            }
                            else
                            {
                                if (item.StepName != EnumProfileStepType.End)
                                {
                                    if (item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp)
                                   //|| item.Temperature >= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp))
                                   && item.AlarmFlagLastStep == false
                                   )
                                    {
                                        item.Alarm = 1;
                                        item.AlarmDescription = $"Nhiệt độ cao";
                                        easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);

                                        //log DB alarm
                                        var p = new DynamicParameters();


                                        item.AlarmFlagLastStep = true;
                                        Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                    }
                                    else if (item.Temperature <= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOut)
                                        //&& item.Temperature < (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp)
                                        //&& item.AlarmFlagLastStep == true
                                        )
                                    {
                                        item.Alarm = 0;
                                        item.AlarmDescription = null;
                                        easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                        item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                        item.AlarmFlagLastStep = false;
                                        Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                    }
                                }
                                else
                                {
                                    if (item.Temperature > (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTemp)
                                   //|| item.Temperature <= (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTemp))
                                   && item.AlarmFlagLastStep == false
                                   )
                                    {
                                        item.Alarm = 1;
                                        item.AlarmDescription = $"Nhiệt độ cao";
                                        easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);

                                        //log DB alarm
                                        var p = new DynamicParameters();

                                        item.AlarmFlagLastStep = true;
                                        Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                    }
                                    else if (item.Temperature <= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOut)
                                        //&& item.Temperature > (item.SetPointLastStep - GlobalVariable.ConfigSystem.ToleranceTemp)
                                        //&& item.AlarmFlagLastStep == true
                                        )
                                    {
                                        item.Alarm = 0;
                                        item.AlarmDescription = null;
                                        easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                                        item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                                        item.AlarmFlagLastStep = false;
                                        Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                                    }
                                }
                            }
                        }
                    }
                }
                else if (item.Status == 0 && item.CountSecondTagChange >= 2
                       && item.Temperature <= (item.SetPointLastStep + GlobalVariable.ConfigSystem.ToleranceTempOut) && item.AlarmFlagLastStep == true)
                {
                    item.Alarm = 0;
                    item.AlarmDescription = null;
                    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                    item.EndStep = 0;//tắt tín hiệu này thì mới vào cảnh báo toàn thời gian được.
                    item.AlarmFlagLastStep = false;
                    Debug.WriteLine($"{item.OvenName}-  EndStep: {item.EndStep} - alarm: {item.Alarm} - alarmFlag: {item.AlarmFlag}- alarmFlagLastStep: {item.AlarmFlagLastStep}");
                }

                #region kiểm tra xem có tín hiệu tắt còi từ server thì tắt còi
                //var c = _controlPlcModel.FirstOrDefault(x => x.OvenId == item.OvenId);
                //if (c.OffSerien == 1)
                //{
                //    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                //    c.OffSerien = 0;

                //    using (var con = GlobalVariable.GetDbConnection())
                //    {
                //        var p = new DynamicParameters();
                //        p.Add("id", _ft06.FirstOrDefault().Id);
                //        p.Add("c000", JsonConvert.SerializeObject(_controlPlcModel));

                //        con.Execute("sp_FT06Update", param: p, commandType: CommandType.StoredProcedure);
                //    }
                //}

                #endregion

                index += 1;
            }
            #endregion

            t.Enabled = true;
        }

        private void EasyDriverConnector1_Started(object sender, EventArgs e)
        {
            int index = 1;

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

                ProfileStepNumber_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus")
                    , new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus")
                    , "", easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus").Value));

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

                easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "0", WritePiority.High);
                index += 1;
            }

            if (_pnStatus.InvokeRequired)
            {
                _pnStatus?.Invoke(new Action(() =>
                {
                    if (easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected)
                    {
                        _pnStatus.BackColor = Color.Green;
                    }
                    else
                    {
                        _pnStatus.BackColor = Color.Red;
                    }
                }));
            }
            else
            {
                if (easyDriverConnector1.ConnectionStatus == ConnectionStatus.Connected)
                {
                    _pnStatus.BackColor = Color.Green;
                }
                else
                {
                    _pnStatus.BackColor = Color.Red;
                }
            }
        }

        #region Event tag value change
        private void Temperature_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            var path = e.Tag.Parent.Path;

            foreach (var item in _displayRealtime)
            {
                if (item.Path == path)
                {
                    item.ConnectionStatus = e.NewQuality == Quality.Good ? 1 : 0;

                    if (item.ConnectionStatus == 0)
                    {
                        item.AlarmDescription = "Mất kết nối đến lò";
                    }
                    return;
                }
            }
        }

        private void Temperature_ValueChanged(object sender, TagValueChangedEventArgs e)
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

        private void DigitalInput1Status_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            var path = e.Tag.Parent.Path;

            foreach (var item in _displayRealtime)
            {
                if (item.Path == path)
                {
                    //Watlow tra tin hieu ve khi co tác động là 0, còn không tác động là 1.
                    item.DoorStatus = e.NewValue == "1" ? 1 : 0;

                    if (e.NewValue == "0")
                    {
                        item.AlarmDescription = "Cửa lò mở";
                    }
                    else
                    {
                        item.AlarmDescription = null;
                    }
                    return;
                }
            }
        }

        private void EndSetPointCh1_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
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

        private void SecondsRemaining_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
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

        private void MinutesRemaining_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
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

        private void HoursRemaining_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
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

        private void ProfileStepType_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
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

        private void ProfileStepNumber_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            var path = e.Tag.Parent.Path;

            foreach (var item in _displayRealtime)
            {
                if (item.Path == path)
                {
                    //if (item.ProfileStepNumber_CurrentStatus == 0) item.SetPointLastStep = item.Temperature;
                    //else item.SetPointLastStep = item.SetPoint;
                    item.SetPointLastStep = item.SetPoint;
                    item.LastStepType = item.StepName;

                    item.ProfileStepNumber_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.ProfileStepNumber_CurrentStatus;

                    //cập nhật các thông số cảu step mới vào để chạy
                    var step = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus)?
                        .Steps.FirstOrDefault(x => x.Id == item.ProfileStepNumber_CurrentStatus);

                    if (step != null)
                    {
                        item.StepName = step.StepType;
                        item.Hours = (int)(step?.Hours); item.Minutes = (int)(step?.Minutes); item.Seconds = (int)(step?.Seconds);
                        item.SetPoint = (double)(step?.SetPoint);

                        if (item.SetPointLastStep == 0)
                        {
                            item.SetPointLastStep = item.SetPoint;
                        }

                        item.TempRange = Math.Round(Math.Abs((item.SetPoint - item.SetPointLastStep)), 2);
                    }

                    var profile = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus);
                    item.ProfileName = profile?.Name;

                    if (item.CountSecondTagChange >= 2 && item.ProfileStepNumber_CurrentStatus > 0 && item.LastStepType != EnumProfileStepType.End)
                    {
                        item.EndStep = 1;
                    }

                    Debug.WriteLine($"{item.OvenName} - {item.StepName} - EndStep: {item.EndStep} - Last set point: {item.SetPointLastStep} - set point: {item.SetPoint}");
                    return;
                }
            }
        }

        private void ProfileNumber_CurrentStatus_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            var path = e.Tag.Parent.Path;

            foreach (var item in _displayRealtime)
            {
                if (item.Path == path)
                {
                    item.ProfileNumber_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.ProfileNumber_CurrentStatus;

                    var profile = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus);
                    item.ProfileName = profile?.Name;
                    return;
                }
            }
        }

        private void Profile1Name_C1_ValueChanged(object sender, TagValueChangedEventArgs e)
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
        #endregion
        #endregion
    }
}
