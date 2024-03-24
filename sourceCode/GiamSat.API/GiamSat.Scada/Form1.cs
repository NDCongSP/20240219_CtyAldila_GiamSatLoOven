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

        private Task _taskDataLog, _taskDataLogProfile;

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

            _timer.Interval = 500;
            _timer.Enabled = true;
            _timer.Tick += _timer_Tick;

            _startTime = _startTimeDisplay = _endTime = _endTimeDisplay = DateTime.Now;
            _taskDataLog = new Task(() => LogData());
            _taskDataLog.Start();

            //_taskDataLogProfile = new Task(() => LogDataProfile());
            //_taskDataLog.Start();
        }

        #region Methods
        private void GetOvensInfo()
        {
            using (var con = GlobalVariable.GetDbConnection())
            {
                var para = new DynamicParameters();

                var ft01 = con.Query<FT01>("sp_FT01GetAll", commandType: CommandType.StoredProcedure).ToList();

                if (ft01.Count <= 0)
                    return;

                GlobalVariable.ConfigSystem = JsonConvert.DeserializeObject<ConfigModel>(ft01.FirstOrDefault().C000);
                _ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(ft01.FirstOrDefault().C001);

                _alarmFlag = new bool[_ovensInfo.Count];

                foreach (var item in _ovensInfo)
                {
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

                    _displayRealtime.Add(new RealtimeDisplayModel()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                        Path = item.Path
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
                var datetime = DateTime.Now;//get thoi gian bắt đầu của 

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
                                if (item.Status == 1)
                                {
                                    var para = new DynamicParameters();
                                    para.Add("OvenId", item.OvenId);
                                    para.Add("OvenName", item.OvenName);
                                    para.Add("Temperature", item.Temperature);
                                    para.Add("StartDate", datetime);

                                    con.Execute("sp_FT04Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);
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

            //UpdateSettingsFormWeb();

            t.Enabled = true;
        }

        private void EasyDriverConnector1_Started(object sender, EventArgs e)
        {
            #region Chuong 1
            foreach (var item in _ovensInfo)
            {
                easyDriverConnector1.GetTag($"{item.Path}/Temperature").ValueChanged += Temperature_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/DigitalInput1Status").ValueChanged += DigitalInput1Status_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/ProfileNumber_CurrentStatus").ValueChanged += ProfileNumber_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/ProfileStepNumber_CurrentStatus").ValueChanged += ProfileStepNumber_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/ProfileStepType_CurrentStatus").ValueChanged += ProfileStepType_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/HoursRemaining_CurrentStatus").ValueChanged += HoursRemaining_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/MinutesRemaining_CurrentStatus").ValueChanged += MinutesRemaining_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/SecondsRemaining_CurrentStatus").ValueChanged += SecondsRemaining_CurrentStatus_ValueChanged;
                easyDriverConnector1.GetTag($"{item.Path}/EndSetPointCh1_CurrentStatus").ValueChanged += EndSetPointCh1_CurrentStatus_ValueChanged;

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


            }
            #endregion

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
        private void Temperature_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            var path = e.Tag.Parent.Path;

            foreach (var item in _displayRealtime)
            {
                if (item.Path == path)
                {
                    //Debug.WriteLine($"{path}/Tempperature: {e.NewValue}");
                    item.Temperature = double.TryParse(e.NewValue, out double value) ? value : item.Temperature;
                    item.ConnectionStatus = e.Tag.Quality == Quality.Good ? 1 : 0;
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
                    item.DoorStatus = e.NewValue == "1" ? 1 : 0;
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
                    item.TemperatureHighLevel = double.TryParse(e.NewValue, out double value) ? value : item.TemperatureHighLevel;
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
                    item.ProfileStepNumber_CurrentStatus = int.TryParse(e.NewValue, out int value) ? value : item.ProfileStepNumber_CurrentStatus;
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
