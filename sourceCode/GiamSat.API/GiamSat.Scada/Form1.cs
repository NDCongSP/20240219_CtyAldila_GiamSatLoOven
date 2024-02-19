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
        private List<DisplayRealTimeModel> _displayRealtime = new List<DisplayRealTimeModel>();
        //private List<RealtimeDisplayModel> _realtimeDisplay = new List<RealtimeDisplayModel>();
        private double[] _khoiLuongSilo = { 0, 0, 0, 0 };
        private RealtimeList _realtimeData = new RealtimeList();

        private List<DataLogModel> _dataLog = new List<DataLogModel>();
        private List<ChuongInfoModel> _chuongInfo = new List<ChuongInfoModel>();

        private Timer _timer = new Timer();
        private List<Guid> _chuongId = new List<Guid>();

        private DateTime _startTime, _endTime, _startTimeDisplay, _endTimeDisplay;
        private double _totalTime = 0, _totalTimeDisplay = 0;

        private bool[] _settingsFromHMI = { false, false, false, false };

        private string _auto = "0", _man = "0";

        private Task _task;

        #region Email
        private Email _email=new Email();
        AlarmSetting _alarmSetting=new AlarmSetting();
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
            GetChuongInfo();
            #endregion

            easyDriverConnector1.Started += EasyDriverConnector1_Started;

            _timer.Interval = 500;
            _timer.Enabled = true;
            _timer.Tick += _timer_Tick;

            _startTime = _startTimeDisplay = DateTime.Now;
            _task = new Task(() => LogData());
            _task.Start();
        }

        #region Methods
        private void GetChuongInfo()
        {
            using (var con = GlobalVariable.GetDbConnection())
            {
                var para = new DynamicParameters();

                _chuongInfo = con.Query<ChuongInfoModel>("sp_chuongInfoGetAll", commandType: CommandType.StoredProcedure).ToList();
                foreach (var item in _chuongInfo)
                {
                    _chuongId.Add(item.Id);

                    _dataLog.Add(new DataLogModel()
                    {
                        ChuongId = item.Id,
                        TenChuong = item.TenChuong,
                    });

                    var settingsItem = JsonConvert.DeserializeObject<SettingsModel>(item.ConfigSettings);

                    _displayRealtime.Add(new DisplayRealTimeModel()
                    {
                        ChuongId = item.Id,
                        TenChuong = item.TenChuong,
                        NumIndex = (int)item.NumIndex,
                        HightTemperature = settingsItem.Steps.FirstOrDefault().HightTemperature,
                        Lowtemperature = settingsItem.Steps.FirstOrDefault().Lowtemperature,
                    });

                    _realtimeData.Add(new RealtimeModel()
                    {
                        DisplayRealtime = new DisplayRealTimeModel()
                        {
                            ChuongId = item.Id,
                            TenChuong = item.TenChuong,
                            NumIndex = (int)item.NumIndex,
                            HightTemperature = settingsItem.Steps.FirstOrDefault().HightTemperature,
                            Lowtemperature = settingsItem.Steps.FirstOrDefault().Lowtemperature,
                        },
                        NangSuat = settingsItem.NangSuat
                    });
                }

                #region initial display table
                con.Execute("Truncate table DisplayRealtime");
                foreach (var item in _displayRealtime)
                {
                    para = null;
                    para = new DynamicParameters();
                    para.Add("chuongId", item.ChuongId);
                    para.Add("tenChuong", item.TenChuong);
                    para.Add("temperature", item.Temperature);
                    para.Add("humidity", item.Humidity);
                    para.Add("frequency", item.Frequency);
                    para.Add("fan1Status", item.Fan1Status);
                    para.Add("fan2Status", item.Fan2Status);
                    para.Add("fan3Status", item.Fan3Status);
                    para.Add("fan4Status", item.Fan4Status);
                    para.Add("coolerStatus", item.CoollerStatus);
                    para.Add("connectStatus", item.ConnectStatus);
                    para.Add("activeStatus", item.ActiveStatus);
                    para.Add("hightTemperature", item.HightTemperature);
                    para.Add("lowTemperature", item.Lowtemperature);
                    para.Add("numIndex", item.NumIndex);
                    para.Add("currentDay", item.CurrentDay);
                    para.Add("currentStep", item.CurrentStep);
                    para.Add("totalDay", item.TotalDay);
                    para.Add("chayDuPhong", item.ChayDuPhong);
                    para.Add("quaTai", item.QuaTai);

                    con.Execute("sp_displayRealtimeInsert", param: para, commandType: CommandType.StoredProcedure);
                }

                con.Execute("Truncate table RealtimeDisplay");
                var displayData = JsonConvert.SerializeObject(_realtimeData);
                para = null;
                para = new DynamicParameters();
                para.Add("displayData", displayData);
                con.Execute("sp_realtimeDisplayInsert", param: para, commandType: CommandType.StoredProcedure);
                #endregion
            }
        }

        private void LogData()
        {
            while (true)
            {
                //kiểm tra xem có cài đặt gì từ web thì cài xuống HMI
                UpdateSettingsFormWeb();

                _endTime = DateTime.Now;
                _totalTime = (_endTime - _startTime).TotalSeconds;
                _totalTimeDisplay = (_endTimeDisplay - _startTimeDisplay).TotalSeconds;

                //data log
                if (_totalTime >= GlobalVariable.LogInterval)
                {
                    _startTime = DateTime.Now;
                    //log data
                    using (var con = GlobalVariable.GetDbConnection())
                    {
                        foreach (var item in _dataLog)
                        {
                            var para = new DynamicParameters();
                            para.Add("chuongId", item.ChuongId);
                            para.Add("tenChuong", item.TenChuong);
                            para.Add("nhietDo", item.NhietDo);
                            para.Add("doAm", item.DoAm);
                            para.Add("frequency", item.Frequency);

                            con.Execute("sp_dataLogInsert", param: para, commandType: CommandType.StoredProcedure);
                        }
                    }
                }

                //realtime display
                if (_totalTimeDisplay < GlobalVariable.DisplayRealtimeInterval)
                {
                    _startTimeDisplay = DateTime.Now;
                    //log data
                    using (var con = GlobalVariable.GetDbConnection())
                    {
                        var para = new DynamicParameters();

                        foreach (var item in _displayRealtime)
                        {
                            para = null;
                            para = new DynamicParameters();
                            para.Add("chuongId", item.ChuongId);
                            para.Add("tenChuong", item.TenChuong);
                            para.Add("temperature", item.Temperature);
                            para.Add("humidity", item.Humidity);
                            para.Add("frequency", item.Frequency);
                            para.Add("fan1Status", item.Fan1Status);
                            para.Add("fan2Status", item.Fan2Status);
                            para.Add("fan3Status", item.Fan3Status);
                            para.Add("fan4Status", item.Fan4Status);
                            para.Add("coolerStatus", item.CoollerStatus);
                            para.Add("connectStatus", item.ConnectStatus);
                            para.Add("activeStatus", item.ActiveStatus);
                            para.Add("hightTemperature", item.HightTemperature);
                            para.Add("lowTemperature", item.Lowtemperature);
                            para.Add("numIndex", item.NumIndex);
                            para.Add("currentDay", item.CurrentDay);
                            para.Add("currentStep", item.CurrentStep);
                            para.Add("totalDay", item.TotalDay);
                            para.Add("chayDuPhong", item.ChayDuPhong);
                            para.Add("quaTai", item.QuaTai);

                            con.Execute("sp_displayRealtimeUpdate", param: para, commandType: CommandType.StoredProcedure);

                            var res = _realtimeData.FirstOrDefault(x => x.DisplayRealtime.ChuongId == item.ChuongId);

                            res.DisplayRealtime = item;
                            res.NangSuat.TongKhoiLuongThucTe = _khoiLuongSilo[item.NumIndex - 1];
                        }

                        con.Execute("Truncate table RealtimeDisplay");
                        var displayData = JsonConvert.SerializeObject(_realtimeData);
                        para = null;
                        para = new DynamicParameters();
                        para.Add("displayData", displayData);
                        con.Execute("sp_realtimeDisplayInsert", param: para, commandType: CommandType.StoredProcedure);
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }
        private void UpdateSettingsFormWeb()
        {
            using (var con = GlobalVariable.GetDbConnection())
            {
                var para = new DynamicParameters();


                _chuongInfo = con.Query<ChuongInfoModel>("sp_chuongInfoGetAll", commandType: CommandType.StoredProcedure).ToList();

                foreach (var item in _chuongInfo)
                {
                    if (item.FlagUpdate == 1)
                    {
                        #region add cac thong so moi vao list nang suat de  ghi len hien thi thoi gian thuc
                        var resNangSuat = _realtimeData.FirstOrDefault(x => x.NangSuat.ChuongId == item.Id);
                        resNangSuat.NangSuat = JsonConvert.DeserializeObject<SettingsModel>(item.ConfigSettings).NangSuat;
                        resNangSuat.NangSuat.TongKhoiLuongThucTe = _khoiLuongSilo[(int)(item.NumIndex - 1)];
                        #endregion

                        //cap nhat cai dat moi xuong HMI. sau đó resert flagUpdate
                        if (!_settingsFromHMI[0])//nếu ko có tín hiệu thay đổi giá trị cài đặt trực tiếp từ HMI thì vào get cai đặt từ web, nếu có thay đổi thì cập nhật xuống cho HMI
                        {
                            UpdateSettingsForHMI((int)item.NumIndex, item);
                            item.FlagUpdate = 0;

                            para = null;
                            para = new DynamicParameters();
                            para.Add("Id", item.Id);
                            para.Add("tenChuong", item.TenChuong);
                            para.Add("configSettings", item.ConfigSettings);
                            para.Add("numIndex", item.NumIndex);
                            para.Add("flagUpdate", item.FlagUpdate);

                            con.Execute("[sp_chuongInfoUpdate]", param: para, commandType: CommandType.StoredProcedure);
                        }
                    }
                }
            }
        }

        private bool UpdateSettingsForHMI(int numIndex, ChuongInfoModel model)
        {
            try
            {
                var d = JsonConvert.DeserializeObject<SettingsModel>(model.ConfigSettings);

                #region Step
                int index = 1;
                foreach (var item in d.Steps)
                {
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}TuNgay").WriteAsync(item.FromDate.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}DenNgay").WriteAsync(item.ToDate.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}SoQuatChayCoDinh").WriteAsync(item.StaticFanRun.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoCao").WriteAsync(item.HightTemperature.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoThap").WriteAsync(item.LowFrequency.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}TanSoCao").WriteAsync(item.HightFrequency.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}TanSoThap").WriteAsync(item.LowFrequency.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoQuat1").WriteAsync(item.TempRunFan1.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoQuat2").WriteAsync(item.TempRunFan2.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoQuat3").WriteAsync(item.TempRunFan3.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoQuat4").WriteAsync(item.TempRunFan4.ToString(), WritePiority.High);
                    easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/Moc{index}NhietDoBatDanMat").WriteAsync(item.TempRunCooler.ToString(), WritePiority.High);

                    index += 1;
                }
                #endregion

                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/DeadbandNhietDo").WriteAsync(d.GeneralSettings.DeadbandTemp.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/NgayHienTai").WriteAsync(d.GeneralSettings.CurrentDay.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/OffsetNhietDo").WriteAsync(d.GeneralSettings.OffsetTemp.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/ResetGiaiDoan").WriteAsync(d.GeneralSettings.ResetGiaiDoan.ToString(), WritePiority.High);

                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/QuatCoDinh1").WriteAsync(d.GeneralSettings.Fan1.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/QuatCoDinh2").WriteAsync(d.GeneralSettings.Fan2.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/QuatCoDinh3").WriteAsync(d.GeneralSettings.Fan3.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/QuatCoDinh4").WriteAsync(d.GeneralSettings.Fan4.ToString(), WritePiority.High);

                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/ThoiGianOnDanMat").WriteAsync(d.GeneralSettings.TimeOnCooler.ToString(), WritePiority.High);
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/ThoiGianOffDanMat").WriteAsync(d.GeneralSettings.TimeOffCooler.ToString(), WritePiority.High);

                //dùng để bùa cho trại ở vĩnh phúc, do đầu cân hiện chưa kết nối lên được
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/KhoiLuongSilo").WriteAsync(d.NangSuat.KhoiLuongThucAnTheoDauCon.ToString(), WritePiority.High);

                //bật bit này lên để HMI cap nhật lại thứ tự quạt
                easyDriverConnector1.GetTag($"Local Station/ChannelChuong{numIndex}/Device1/ChotCaiThuTuOutQuat").WriteAsync("1", WritePiority.High);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Write tag error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/OutQ1Auto").QualityChanged += Chuong1Status_QualityChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong1/Device1/InAuto").ValueChanged += InAutoChuong1_ValueChanged;
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

            #region Chuong 2
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/OutQ1Auto").QualityChanged += Chuong2Status_QualityChanged;
                                                                    
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InAuto").ValueChanged += InAutoChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InMan").ValueChanged += InManChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InChayDuPhong").ValueChanged += InChayDuPhongChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InQuaTai").ValueChanged += InQuaTaiChuong2_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ1").ValueChanged += InStatusQ1Chuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ2").ValueChanged += InStatusQ2Chuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ3").ValueChanged += InStatusQ3Chuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ4").ValueChanged += InStatusQ4Chuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusDanMat").ValueChanged += InStatusDanMatChuong2_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NhietDo").ValueChanged += NhietDoChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/DoAm").ValueChanged += DoAmChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/TanSo").ValueChanged += TanSoChuong2_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NgayHienTai").ValueChanged += NgayHienTaiChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/GiaiDoanHienTai").ValueChanged += GiaiDoanHienTaiChuong2_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/HmiThayDoiCaiDat").ValueChanged += HmiThayDoiCaiDatChuong2_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/KhoiLuongSilo").ValueChanged += KhoiLuongSiloChuong2_ValueChanged;

            Chuong2Status_QualityChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/OutQ1Auto")
              , new TagQualityChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/OutQ1Auto")
              , Quality.Uncertain, easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/OutQ1Auto").Quality));

            NhietDoChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NhietDo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NhietDo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NhietDo").Value));
            DoAmChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/DoAm")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/DoAm")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/DoAm").Value));
            TanSoChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/TanSo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/TanSo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/TanSo").Value));

            InAutoChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InAuto")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InAuto")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InAuto").Value));
            InManChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InMan")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InMan")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InMan").Value));
            InChayDuPhongChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InChayDuPhong")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InChayDuPhong")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InChayDuPhong").Value));
            InQuaTaiChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InQuaTai")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InQuaTai")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InQuaTai").Value));

            InStatusQ1Chuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ1")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ1")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ1").Value));
            InStatusQ2Chuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ2")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ2")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ2").Value));
            InStatusQ3Chuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ3")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ3")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ3").Value));
            InStatusQ4Chuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ4")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ4")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusQ4").Value));
            InStatusDanMatChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusDanMat")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusDanMat")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/InStatusDanMat").Value));

            NgayHienTaiChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NgayHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NgayHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/NgayHienTai").Value));
            GiaiDoanHienTaiChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/GiaiDoanHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/GiaiDoanHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/GiaiDoanHienTai").Value));

            HmiThayDoiCaiDatChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/HmiThayDoiCaiDat")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/HmiThayDoiCaiDat")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/HmiThayDoiCaiDat").Value));

            KhoiLuongSiloChuong2_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/KhoiLuongSilo")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/KhoiLuongSilo")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong2/Device1/KhoiLuongSilo").Value));
            #endregion

            #region Chuong 3
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/OutQ1Auto").QualityChanged += Chuong3Status_QualityChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InAuto").ValueChanged += InAutoChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InMan").ValueChanged += InManChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InChayDuPhong").ValueChanged += InChayDuPhongChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InQuaTai").ValueChanged += InQuaTaiChuong3_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ1").ValueChanged += InStatusQ1Chuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ2").ValueChanged += InStatusQ2Chuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ3").ValueChanged += InStatusQ3Chuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ4").ValueChanged += InStatusQ4Chuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusDanMat").ValueChanged += InStatusDanMatChuong3_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NhietDo").ValueChanged += NhietDoChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/DoAm").ValueChanged += DoAmChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/TanSo").ValueChanged += TanSoChuong3_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NgayHienTai").ValueChanged += NgayHienTaiChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/GiaiDoanHienTai").ValueChanged += GiaiDoanHienTaiChuong3_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/HmiThayDoiCaiDat").ValueChanged += HmiThayDoiCaiDatChuong3_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/KhoiLuongSilo").ValueChanged += KhoiLuongSiloChuong3_ValueChanged;

            Chuong3Status_QualityChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/OutQ1Auto")
              , new TagQualityChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/OutQ1Auto")
              , Quality.Uncertain, easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/OutQ1Auto").Quality));

            NhietDoChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NhietDo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NhietDo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NhietDo").Value));
            DoAmChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/DoAm")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/DoAm")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/DoAm").Value));
            TanSoChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/TanSo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/TanSo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/TanSo").Value));

            InAutoChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InAuto")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InAuto")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InAuto").Value));
            InManChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InMan")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InMan")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InMan").Value));
            InChayDuPhongChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InChayDuPhong")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InChayDuPhong")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InChayDuPhong").Value));
            InQuaTaiChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InQuaTai")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InQuaTai")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InQuaTai").Value));

            InStatusQ1Chuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ1")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ1")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ1").Value));
            InStatusQ2Chuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ2")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ2")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ2").Value));
            InStatusQ3Chuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ3")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ3")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ3").Value));
            InStatusQ4Chuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ4")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ4")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusQ4").Value));
            InStatusDanMatChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusDanMat")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusDanMat")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/InStatusDanMat").Value));

            NgayHienTaiChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NgayHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NgayHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/NgayHienTai").Value));
            GiaiDoanHienTaiChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/GiaiDoanHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/GiaiDoanHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/GiaiDoanHienTai").Value));

            HmiThayDoiCaiDatChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/HmiThayDoiCaiDat")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/HmiThayDoiCaiDat")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/HmiThayDoiCaiDat").Value));

            KhoiLuongSiloChuong3_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/KhoiLuongSilo")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/KhoiLuongSilo")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong3/Device1/KhoiLuongSilo").Value));
            #endregion

            #region Chuong 4
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/OutQ1Auto").QualityChanged += Chuong4Status_QualityChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InAuto").ValueChanged += InAutoChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InMan").ValueChanged += InManChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InChayDuPhong").ValueChanged += InChayDuPhongChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InQuaTai").ValueChanged += InQuaTaiChuong4_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ1").ValueChanged += InStatusQ1Chuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ2").ValueChanged += InStatusQ2Chuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ3").ValueChanged += InStatusQ3Chuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ4").ValueChanged += InStatusQ4Chuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusDanMat").ValueChanged += InStatusDanMatChuong4_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NhietDo").ValueChanged += NhietDoChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/DoAm").ValueChanged += DoAmChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/TanSo").ValueChanged += TanSoChuong4_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NgayHienTai").ValueChanged += NgayHienTaiChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/GiaiDoanHienTai").ValueChanged += GiaiDoanHienTaiChuong4_ValueChanged;

            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/HmiThayDoiCaiDat").ValueChanged += HmiThayDoiCaiDatChuong4_ValueChanged;
            easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/KhoiLuongSilo").ValueChanged += KhoiLuongSiloChuong4_ValueChanged;

            Chuong4Status_QualityChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/OutQ1Auto")
              , new TagQualityChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/OutQ1Auto")
              , Quality.Uncertain, easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/OutQ1Auto").Quality));

            NhietDoChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NhietDo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NhietDo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NhietDo").Value));
            DoAmChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/DoAm")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/DoAm")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/DoAm").Value));
            TanSoChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/TanSo")
                , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/TanSo")
                , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/TanSo").Value));

            InAutoChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InAuto")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InAuto")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InAuto").Value));
            InManChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InMan")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InMan")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InMan").Value));
            InChayDuPhongChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InChayDuPhong")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InChayDuPhong")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InChayDuPhong").Value));
            InQuaTaiChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InQuaTai")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InQuaTai")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InQuaTai").Value));

            InStatusQ1Chuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ1")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ1")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ1").Value));
            InStatusQ2Chuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ2")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ2")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ2").Value));
            InStatusQ3Chuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ3")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ3")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ3").Value));
            InStatusQ4Chuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ4")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ4")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusQ4").Value));
            InStatusDanMatChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusDanMat")
               , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusDanMat")
               , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/InStatusDanMat").Value));

            NgayHienTaiChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NgayHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NgayHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/NgayHienTai").Value));
            GiaiDoanHienTaiChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/GiaiDoanHienTai")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/GiaiDoanHienTai")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/GiaiDoanHienTai").Value));

            HmiThayDoiCaiDatChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/HmiThayDoiCaiDat")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/HmiThayDoiCaiDat")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/HmiThayDoiCaiDat").Value));

            KhoiLuongSiloChuong4_ValueChanged(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/KhoiLuongSilo")
              , new TagValueChangedEventArgs(easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/KhoiLuongSilo")
              , "", easyDriverConnector1.GetTag("Local Station/ChannelChuong4/Device1/KhoiLuongSilo").Value));
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

        #region Event tag value change chuong 1
        #region Chuong 1
        private void Chuong1Status_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);

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
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.CoollerStatus = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ4Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.Fan4Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ3Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.Fan3Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ2Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.Fan2Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ1Chuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.Fan1Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InQuaTaiChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.QuaTai = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InChayDuPhongChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
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
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Chạy tay";
                }
            }
            else if (e.NewValue == "1" && _auto == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void InAutoChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _auto = e.NewValue;

            if (e.NewValue == "1" && _man == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Tự động";
                }
            }
            else if (e.NewValue == "1" && _man == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void GiaiDoanHienTaiChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.CurrentStep = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void NgayHienTaiChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.CurrentDay = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void TanSoChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item != null)
            {
                item.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void DoAmChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item != null)
            {
                item.DoAm = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
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

            var item = _realtimeData.FirstOrDefault(x => x.NangSuat.ChuongId == _chuongId[0]);
            if (item != null)
            {
                item.NangSuat.TongKhoiLuongThucTe = Math.Round(_khoiLuongSilo[0], 2);
            }
        }

        private void NhietDoChuong1_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item != null)
            {
                item.NhietDo = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[0]);
            if (item1 != null)
            {
                item1.Temperature = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }
        #endregion

        #region Chuong 2
        private void Chuong2Status_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);

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

        private void InStatusDanMatChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.CoollerStatus = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ4Chuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Fan4Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ3Chuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Fan3Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ2Chuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Fan2Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ1Chuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Fan1Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InQuaTaiChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.QuaTai = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InChayDuPhongChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.ChayDuPhong = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InManChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _man = e.NewValue;

            if (e.NewValue == "1" && _auto == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Chạy tay";
                }
            }
            else if (e.NewValue == "1" && _auto == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void InAutoChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _auto = e.NewValue;

            if (e.NewValue == "1" && _man == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Tự động";
                }
            }
            else if (e.NewValue == "1" && _man == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void GiaiDoanHienTaiChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.CurrentStep = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void NgayHienTaiChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.CurrentDay = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void TanSoChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item != null)
            {
                item.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void DoAmChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item != null)
            {
                item.DoAm = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Humidity = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void HmiThayDoiCaiDatChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            if (e.NewValue == "1")
            {
                _settingsFromHMI[1] = true;
            }
            else
            {
                _settingsFromHMI[1] = false;
            }
        }

        private void KhoiLuongSiloChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //var item = _realtimeData.FirstOrDefault(x => x.NangSuat.ChuongId == _chuongId[0]);
            //if (item != null)
            //{
            //    item.NangSuat.TongKhoiLuongThucTe = double.TryParse(e.NewValue, out double value) ? value : 0;
            //}

            _khoiLuongSilo[1] = double.TryParse(e.NewValue, out double value) ? value : 0;
        }

        private void NhietDoChuong2_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item != null)
            {
                item.NhietDo = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[1]);
            if (item1 != null)
            {
                item1.Temperature = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }
        #endregion

        #region Chuong 3
        private void Chuong3Status_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);

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

        private void InStatusDanMatChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.CoollerStatus = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ4Chuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Fan4Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ3Chuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Fan3Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ2Chuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Fan2Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ1Chuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Fan1Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InQuaTaiChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.QuaTai = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InChayDuPhongChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.ChayDuPhong = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InManChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _man = e.NewValue;

            if (e.NewValue == "1" && _auto == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Chạy tay";
                }
            }
            else if (e.NewValue == "1" && _auto == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void InAutoChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _auto = e.NewValue;

            if (e.NewValue == "1" && _man == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Tự động";
                }
            }
            else if (e.NewValue == "1" && _man == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void GiaiDoanHienTaiChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.CurrentStep = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void NgayHienTaiChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.CurrentDay = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void TanSoChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item != null)
            {
                item.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void DoAmChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item != null)
            {
                item.DoAm = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Humidity = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void HmiThayDoiCaiDatChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            if (e.NewValue == "1")
            {
                _settingsFromHMI[2] = true;
            }
            else
            {
                _settingsFromHMI[2] = false;
            }
        }

        private void KhoiLuongSiloChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //var item = _realtimeData.FirstOrDefault(x => x.NangSuat.ChuongId == _chuongId[0]);
            //if (item != null)
            //{
            //    item.NangSuat.TongKhoiLuongThucTe = double.TryParse(e.NewValue, out double value) ? value : 0;
            //}

            _khoiLuongSilo[2] = double.TryParse(e.NewValue, out double value) ? value : 0;
        }

        private void NhietDoChuong3_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item != null)
            {
                item.NhietDo = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[2]);
            if (item1 != null)
            {
                item1.Temperature = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }
        #endregion

        #region Chuong 4
        private void Chuong4Status_QualityChanged(object sender, TagQualityChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);

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

        private void InStatusDanMatChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.CoollerStatus = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ4Chuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.Fan4Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ3Chuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.Fan3Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ2Chuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.Fan2Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InStatusQ1Chuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.Fan1Status = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InQuaTaiChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.QuaTai = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InChayDuPhongChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.ChayDuPhong = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void InManChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _man = e.NewValue;

            if (e.NewValue == "1" && _auto == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Chạy tay";
                }
            }
            else if (e.NewValue == "1" && _auto == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void InAutoChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            _auto = e.NewValue;

            if (e.NewValue == "1" && _man == "0")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Tự động";
                }
            }
            else if (e.NewValue == "1" && _man == "1")
            {
                //cập nhật giá trị cho DisplayRealtime
                var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
                if (item1 != null)
                {
                    item1.ActiveStatus = "Dừng";
                }
            }
        }

        private void GiaiDoanHienTaiChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.CurrentStep = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void NgayHienTaiChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.CurrentDay = int.TryParse(e.NewValue, out int value) ? value : 0;
            }
        }

        private void TanSoChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item != null)
            {
                item.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.Frequency = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void DoAmChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item != null)
            {
                item.DoAm = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item1 != null)
            {
                item1.Humidity = double.TryParse(e.NewValue, out double value) ? value : 0;
            }
        }

        private void HmiThayDoiCaiDatChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            if (e.NewValue == "1")
            {
                _settingsFromHMI[3] = true;
            }
            else
            {
                _settingsFromHMI[3] = false;
            }
        }

        private void KhoiLuongSiloChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //var item = _realtimeData.FirstOrDefault(x => x.NangSuat.ChuongId == _chuongId[0]);
            //if (item != null)
            //{
            //    item.NangSuat.TongKhoiLuongThucTe = double.TryParse(e.NewValue, out double value) ? value : 0;
            //}

            _khoiLuongSilo[3] = double.TryParse(e.NewValue, out double value) ? value : 0;
        }

        private void NhietDoChuong4_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            //cập nhật giá trị cho DataLog
            var item = _dataLog.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
            if (item != null)
            {
                item.NhietDo = double.TryParse(e.NewValue, out double value) ? value : 0;
            }

            //cập nhật giá trị cho DisplayRealtime
            var item1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == _chuongId[3]);
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
