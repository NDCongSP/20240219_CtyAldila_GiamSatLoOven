﻿using Dapper;
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


namespace GiamSat.Scada
{
    public partial class Form1 : Form
    {
        private List<FT03> _dataLog = new List<FT03>();
        private List<FT04> _dataLogProfile = new List<FT04>();
        private List<FT05> _alarm = new List<FT05>();

        private bool[] _alarmFlag;

        private OvensInfo _ovensInfo = new OvensInfo();//cau hinh cac lo oven
        private RealtimeDisplays _displayRealtime = new RealtimeDisplays();

        private Timer _timer = new Timer();
        private List<int> _OvenId = new List<int>();

        private DateTime _startTime, _endTime, _startTimeDisplay, _endTimeDisplay;
        private double _totalTime = 0, _totalTimeDisplay = 0;

        private Task _taskDataLog, _taskDataLogProfile, _taskAlarm;


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

            //_timer.Interval = 500;
            //_timer.Tick += _timer_Tick;
            //_timer.Enabled = true;

            _startTime = _startTimeDisplay = _endTime = _endTimeDisplay = DateTime.Now;

            _taskDataLog = new Task(() => LogData());
            _taskDataLog.Start();

            //_taskDataLogProfile = new Task(() => LogDataProfile());
            //_taskDataLogProfile.Start();

            //_taskAlarm = new Task(() => CheckAlarm());
            //_taskAlarm.Start();
        }

        #region Methods
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
                        DataLogWhenRunProfileInterval = 1000//1s
                        ,
                        DisplayRealtimeInterval = 1000
                        ,
                        RefreshInterval = 1000
                        ,
                        ChartRefreshInterval = 1000
                        ,
                        TimeCheckOvenRunStatus = 5//5s
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
                            SetPoint = 170
                        });
                        steps.Add(new StepModel()
                        {
                            Id = 2,
                            StepType = EnumProfileStepType.Soak,
                            Hours = 0,
                            Minutes = 50,
                            Seconds = 10,
                            SetPoint = 171
                        });
                        steps.Add(new StepModel()
                        {
                            Id = 3,
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

                _alarmFlag = new bool[_ovensInfo.Count];
                var index = 0;

                foreach (var item in _ovensInfo)
                {
                    _alarmFlag[index] = false;
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
                        StartTime = dt,
                        StopTime = dt,
                        OvenInfo = item
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
                            foreach (var item in _dataLogProfile)
                            {
                                if (item.Status == 1)
                                {
                                    var para = new DynamicParameters();
                                    para.Add("ovenId", item.OvenId);
                                    para.Add("ovenName", item.OvenName);
                                    para.Add("temperature  ", item.Temperature);
                                    para.Add("startTime", item.StartTime);
                                    para.Add("endTime", item.EndTime);
                                    para.Add("zIndex", item.ZIndex);
                                    para.Add("hours", item.Hours);
                                    para.Add("minutes", item.Minutes);
                                    para.Add("seconds", item.Seconds);
                                    para.Add("profileId", item.ProfileId);
                                    para.Add("profileName", item.ProfileName);
                                    para.Add("stepId", item.StepId);
                                    para.Add("stepName", item.StepName);
                                    para.Add("stepName", item.StepName);
                                    para.Add("setPoint", item.Setpoint);
                                    para.Add("status", item.Status);
                                    para.Add("createdDate", DateTime.Now);

                                    con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);
                                }
                                else if (item.Status == 0 && item.ZIndex != Guid.Empty)
                                {
                                    item.Status = item.Status;
                                    item.EndTime = DateTime.Now;

                                    var para = new DynamicParameters();
                                    para.Add("ovenId", item.OvenId);
                                    para.Add("ovenName", item.OvenName);
                                    para.Add("temperature  ", item.Temperature);
                                    para.Add("startTime", item.StartTime);
                                    para.Add("endTime", item.EndTime);
                                    para.Add("zIndex", item.ZIndex);
                                    para.Add("hours", item.Hours);
                                    para.Add("minutes", item.Minutes);
                                    para.Add("seconds", item.Seconds);
                                    para.Add("profileId", item.ProfileId);
                                    para.Add("profileName", item.ProfileName);
                                    para.Add("stepId", item.StepId);
                                    para.Add("stepName", item.StepName);
                                    para.Add("stepName", item.StepName);
                                    para.Add("setPoint", item.Setpoint);
                                    para.Add("status", item.Status);
                                    para.Add("createdDate", DateTime.Now);

                                    con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);

                                    item.ZIndex = Guid.Empty;
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

                //realtime display
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

                //data log
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

                            con.Execute("sp_FT03Insert", param: para, commandType: CommandType.StoredProcedure);
                        }
                    }
                }

                #region Kiểm tra xem lò chạy hay dùng để log data và cảnh báo
                foreach (var item in _displayRealtime)
                {
                    if (item.IsLoaded)
                    {
                        item.StopTime = DateTime.Now;
                        if ((item.StopTime - item.StartTime).TotalSeconds <= GlobalVariable.ConfigSystem.TimeCheckOvenRunStatus
                            && (item.StopTime - item.StartTime).TotalSeconds > 0)
                        {
                            if (item.Status == 0)
                            {
                                //goij lai su kien tag chacge de get profile id hien tai
                                //        ProfileNumber_CurrentStatus_ValueChanged(easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus")
                                //, new TagValueChangedEventArgs(easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus")
                                //, "", easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus").Value));

                                //lấy các thông tin còn thiếu cho model realtimeDisplay
                                item.BeginTime = DateTime.Now;//lấy thời gian bắt đầu chạy profile
                                item.Status = 1;
                                item.ZIndex = Guid.NewGuid();

                                var profile = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus);
                                item.ProfileName = profile?.Name;

                                //khoi tao model để lưu data run profile, chay owr task dataLogProfile
                                var du = _dataLogProfile.FirstOrDefault(x => x.OvenId == item.OvenId);
                                du.ProfileId = item.ProfileNumber_CurrentStatus;
                                du.ProfileName = item.ProfileName;
                                du.StepId = item.ProfileStepNumber_CurrentStatus;
                                du.StepName = item.ProfileStepType_CurrentStatus.ToString();
                                du.Setpoint = item.SetPoint;
                                du.ZIndex = item.ZIndex;
                                du.StartTime = item.BeginTime;
                                du.Status = item.Status;
                            }
                        }
                        else
                        {
                            item.Status = 0;
                            item.ZIndex = Guid.Empty;
                            item.ProfileName = null;
                        }
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
            var index = 1;
            foreach (var item in _displayRealtime)
            {
                if (item.Status == 1)
                {
                    //rampTime tăng nhiệt
                    if (item.SetPointLastStep < item.SetPoint)
                    {
                        if (true)
                        {

                        }
                    }
                    //soak
                    else if (item.SetPointLastStep == item.SetPoint)
                    {

                    }
                    //rampTime giảm nhiệt
                    else if (item.SetPointLastStep > item.SetPoint)
                    {

                    }

                    #region Check cảnh báo khi kết thúc step
                    if (item.EndStep == 1 && item.Temperature < (item.SetPoint - GlobalVariable.ConfigSystem.ToleranceTemp)
                        && item.Temperature > (item.SetPoint + GlobalVariable.ConfigSystem.ToleranceTemp))
                    {
                        easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
                        _alarmFlag[index - 1] = true;

                        //log DB alarm

                        item.EndStep = 0;
                    }
                    #endregion

                    easyDriverConnector1.WriteTagAsync($"Local Station/Channel4/PLC/AL{index}", "1", WritePiority.High);
                    _alarmFlag[index - 1] = true;
                }

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
                    item.Temperature = double.TryParse(e.NewValue, out double value) ? value : item.Temperature;

                    var logProfie = _dataLogProfile.FirstOrDefault(x => x.OvenId == item.OvenId);
                    logProfie.Temperature = item.Temperature;

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
                    item.DoorStatus = e.NewValue == "1" ? 0 : 1;
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
                    item.SetPoint = double.TryParse(e.NewValue, out double value) ? value : item.SetPoint;
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
                    item.SecondsRemaining_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.SecondsRemaining_CurrentStatus;
                    item.StartTime = DateTime.Now;

                    if (!item.IsLoaded)
                    {
                        item.StopTime = item.StartTime;
                        item.IsLoaded = true;
                    }

                    if (item.HoursRemaining_CurrentStatus == 0 && item.MinutesRemaining_CurrentStatus == 0 && item.SecondsRemaining_CurrentStatus == 0)
                    {
                        item.EndStep = 1;
                    }

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

                    if (item.HoursRemaining_CurrentStatus == 0 && item.MinutesRemaining_CurrentStatus == 0 && item.SecondsRemaining_CurrentStatus == 0)
                    {
                        item.EndStep = 1;
                    }
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

                    if (item.HoursRemaining_CurrentStatus == 0 && item.MinutesRemaining_CurrentStatus == 0 && item.SecondsRemaining_CurrentStatus == 0)
                    {
                        item.EndStep = 1;
                    }
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
                    item.ProfileStepNumber_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.ProfileStepNumber_CurrentStatus;

                    if (item.ProfileStepNumber_CurrentStatus <= 1) item.SetPointLastStep = item.Temperature;
                    else item.SetPointLastStep = item.SetPoint;

                    //cập nhật các thông số cảu step mới vào để chạy
                    var step = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus)
                        .Steps.FirstOrDefault(x => x.Id == item.ProfileStepNumber_CurrentStatus);

                    if (step!=null)
                    {
                        item.Hours = (int)(step?.Hours); item.Minutes = (int)(step?.Minutes); item.Seconds = (int)(step?.Seconds);
                        item.SetPoint = (double)(step?.SetPoint);
                        item.TempRange = Math.Round(Math.Abs((item.SetPoint - item.SetPointLastStep)), 2);
                    }

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

                    //var profile = item.OvenInfo.Profiles.FirstOrDefault(x => x.Id == item.ProfileNumber_CurrentStatus);
                    //item.ProfileName = profile?.Name;
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
