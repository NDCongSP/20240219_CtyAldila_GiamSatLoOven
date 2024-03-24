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


namespace GiamSat.Scada
{
    public partial class Form1 : Form
    {
        private List<FT03> _dataLog = new List<FT03>();
        private List<FT04> _dataLogProfile = new List<FT04>();
        private List<FT05> _alarm = new List<FT05>();

        private OvensInfo _ovensInfo = new OvensInfo();
        private ConfigModel _config = new ConfigModel();
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

            _startTime = _startTimeDisplay = DateTime.Now;
            _taskDataLog = new Task(() => LogData());
            _taskDataLog.Start();

            _taskDataLogProfile = new Task(() => LogDataProfile());
            _taskDataLog.Start();
        }

        #region Methods
        private void GetOvensInfo()
        {
            using (var con = GlobalVariable.GetDbConnection())
            {
                var para = new DynamicParameters();

                var ft01 = con.Query<FT01>("sp_FT01GetAll", commandType: CommandType.StoredProcedure).ToList();

                _ovensInfo = JsonConvert.DeserializeObject<OvensInfo>(ft01.FirstOrDefault().C001);

                foreach (var item in _ovensInfo)
                {
                    _OvenId.Add(item.Id);

                    _dataLog.Add(new FT03()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                    });

                    _config = JsonConvert.DeserializeObject<ConfigModel>(ft01.FirstOrDefault().C000);

                    _displayRealtime.Add(new RealtimeDisplayModel()
                    {
                        OvenId = item.Id,
                        OvenName = item.Name,
                    });
                }

                #region initial display table
                con.Execute("Truncate table FT02");
                var displayData = JsonConvert.SerializeObject(_displayRealtime);
                para = null;
                para = new DynamicParameters();
                para.Add("C000", displayData);
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

                                    con.Execute("sp_FT05Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);
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
                _endTime = DateTime.Now;
                _totalTime = (_endTime - _startTime).TotalSeconds;
                _totalTimeDisplay = (_endTimeDisplay - _startTimeDisplay).TotalSeconds;

                //data log
                if (_totalTime >= GlobalVariable.ConfigSystem.DataLogInterval)
                {
                    _startTime = DateTime.Now;
                    //log data
                    using (var con = GlobalVariable.GetDbConnection())
                    {
                        foreach (var item in _dataLog)
                        {
                            var para = new DynamicParameters();
                            para.Add("OvenId", item.OvenId);
                            para.Add("OvenName", item.OvenName);
                            para.Add("Temperature", item.Temperature);

                            con.Execute("sp_FT03Insert", param: para, commandType: CommandType.StoredProcedure, transaction: tran);
                        }
                    }
                }

                //realtime display
                if (_totalTimeDisplay < GlobalVariable.ConfigSystem.DisplayRealtimeInterval)
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
                        con.Execute("sp_FT02Insert", param: para, commandType: CommandType.StoredProcedure);
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
            easyDriverConnector1.GetTag("Local Station/Channel1/Lo1/Temperature1").QualityChanged += Chuong1Status_QualityChanged;
            easyDriverConnector1.GetTag("Local Station/Channel1/Lo1/Temperature").QualityChanged += Temperature_QualityChanged;

            easyDriverConnector1.GetTag("Local Station/Channel1/Lo1/Temperature").ValueChanged += InAutoChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InMan").ValueChanged += InManChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InChayDuPhong").ValueChanged += InChayDuPhongChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InQuaTai").ValueChanged += InQuaTaiChuong1_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ1").ValueChanged += InStatusQ1Chuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ2").ValueChanged += InStatusQ2Chuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ3").ValueChanged += InStatusQ3Chuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ4").ValueChanged += InStatusQ4Chuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusDanMat").ValueChanged += InStatusDanMatChuong1_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NhietDo").ValueChanged += NhietDoChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/DoAm").ValueChanged += DoAmChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/TanSo").ValueChanged += TanSoChuong1_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NgayHienTai").ValueChanged += NgayHienTaiChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/GiaiDoanHienTai").ValueChanged += GiaiDoanHienTaiChuong1_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/HmiThayDoiCaiDat").ValueChanged += HmiThayDoiCaiDatChuong1_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/KhoiLuongSilo").ValueChanged += KhoiLuongSiloChuong1_ValueChanged;

            Chuong1Status_QualityChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/OutQ1Auto")
              , new TagQualityChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/OutQ1Auto")
              , Quality.Uncertain, easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/OutQ1Auto").Quality));

            NhietDoChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NhietDo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NhietDo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NhietDo").Value));
            DoAmChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/DoAm")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/DoAm")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/DoAm").Value));
            TanSoChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/TanSo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/TanSo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/TanSo").Value));

            InAutoChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InAuto")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InAuto")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InAuto").Value));
            InManChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InMan")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InMan")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InMan").Value));
            InChayDuPhongChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InChayDuPhong")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InChayDuPhong")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InChayDuPhong").Value));
            InQuaTaiChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InQuaTai")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InQuaTai")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InQuaTai").Value));

            InStatusQ1Chuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ1")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ1")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ1").Value));
            InStatusQ2Chuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ2")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ2")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ2").Value));
            InStatusQ3Chuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ3")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ3")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ3").Value));
            InStatusQ4Chuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ4")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ4")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusQ4").Value));
            InStatusDanMatChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusDanMat")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusDanMat")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InStatusDanMat").Value));

            NgayHienTaiChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NgayHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NgayHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/NgayHienTai").Value));
            GiaiDoanHienTaiChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/GiaiDoanHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/GiaiDoanHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/GiaiDoanHienTai").Value));

            HmiThayDoiCaiDatChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/HmiThayDoiCaiDat")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/HmiThayDoiCaiDat")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/HmiThayDoiCaiDat").Value));

            KhoiLuongSiloChuong1_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/KhoiLuongSilo")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/KhoiLuongSilo")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/KhoiLuongSilo").Value));
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

        private void Temperature_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            var s = e.Tag.Path;
            if (s.Contains("Channel1"))
            {

            }
        }

        #region Event tag value change chuong 1
        #region Chuong 1
        private void Chuong1Status_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);

            if (e.NewQuality == Quality.Good)
            {
                if (item1 != null)
                {
                    item1.ConnectStatus = "Connected";
                }
            }
            else
            {
                if (item1 != null)
                {
                    item1.ConnectStatus = "Disconnect";
                }
            }
        }

        private void InStatusDanMatChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.CoollerStatus = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ4Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Fan4Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ3Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Fan3Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ2Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Fan2Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ1Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Fan1Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InQuaTaiChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.QuaTai = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InChayDuPhongChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.ChayDuPhong = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InManChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _man = e.NewValue;

            if (e.NewValue == "1" && _auto == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Chạy tay";
                }
            }
            else if (e.NewValue == "1" && _auto == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void Temperature_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _displayRealtime.FirstOrDefault(x => x.OvenId == 1).Temperature = double.TryParse(e.NewValue, out double value) ? value : 0;

           
        }

        private void GiaiDoanHienTaiChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.CurrentStep = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void NgayHienTaiChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.CurrentDay = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void TanSoChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item != null)
            {
                item.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void DoAmChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item != null)
            {
                item.DoAm = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Humidity = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void HmiThayDoiCaiDatChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            if (e.NewValue == "1")
            {
                _settingsFromHMI[0] = true;
            }
            else
            {
                _settingsFromHMI[0] = false;
            }
        }

        private void KhoiLuongSiloChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _khoiLuongSilo[0] = double.TryParse(e.NewValue, out double value) ? value : 0;

            var item = _realtimeData.FirstOrDefault(x => x.NangSuat.ChuongId == _OvenId[0]);
            if (item != null)
            {
                item.NangSuat.TongKhoiLuongThucTe = Math.Round(_khoiLuongSilo[0], 2);
            }
        }

        private void NhietDoChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item != null)
            {
                item.NhietDo = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _OvenId[0]);
            if (item1 != null)
            {
                item1.Temperature = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }
        #endregion
        #endregion

        #endregion
    }
}
