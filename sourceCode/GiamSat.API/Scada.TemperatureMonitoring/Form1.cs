using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada.TemperatureMonitoring
{
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector;
        private List<TemperatureConfigsModel> _configs = new List<TemperatureConfigsModel>();

        private CancellationTokenSource _cts;
        private Task _realtimeTask;
        private Task _dataLogTask;
        private Task _alarmTask;
        private Task _configTask;

        private ConnectionStatus _easyStatus;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_easyDriverConnector != null)
                {
                    _easyDriverConnector.ConnectionStatusChaged -= _easyDriverConnector_ConnectionStatusChaged;
                    _easyDriverConnector.Started -= _easyDriverConnector_Started;
                    _easyDriverConnector.Stop();
                }

                _cts?.Cancel();

                // Wait for tasks to complete
                if (_realtimeTask != null && _dataLogTask != null && _alarmTask != null && _configTask != null)
                {
                    Task.WaitAll(new Task[] { _realtimeTask, _dataLogTask, _alarmTask, _configTask }, 2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _cts?.Dispose();
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadConfigFromDb();

                if (_configs.Count == 0)
                {
                    MessageBox.Show("Không đọc được thông tin cấu hình FT10. Hệ thống có thể chưa được cấu hình.", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Setup DataGridView DataBinding
                dgvDevices.AutoGenerateColumns = false;
                dgvDevices.DataSource = new System.ComponentModel.BindingList<TemperatureConfigsModel>(_configs);
                dgvDevices.CellEndEdit += DgvDevices_CellEndEdit;

                #region Khởi tạo easy driver connector
                _easyDriverConnector = new EasyDriverConnector();
                _easyDriverConnector.ConnectionStatusChaged += _easyDriverConnector_ConnectionStatusChaged;

                _easyDriverConnector.BeginInit();
                _easyDriverConnector.EndInit();

                _easyDriverConnector.Started += _easyDriverConnector_Started;
                if (_easyDriverConnector.IsStarted)
                {
                    _easyDriverConnector_Started(null, null);
                }
                #endregion

                // Bắt đầu các Background Tasks
                _cts = new CancellationTokenSource();
                _realtimeTask = Task.Run(() => TaskRealtimeLogAsync(_cts.Token));
                _dataLogTask = Task.Run(() => TaskDataLogAsync(_cts.Token));
                _alarmTask = Task.Run(() => TaskAlarmMonitorAsync(_cts.Token));
                _configTask = Task.Run(() => TaskCheckConfigChangesAsync(_cts.Token));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo: {ex.Message}");
            }
        }

        private async Task LoadConfigFromDb()
        {
            using (var db = new ApplicationDbContext())
            {
                var ft10 = await db.FT10_TemperatureConfigs.FirstOrDefaultAsync(x => x.Actived == true);
                if (ft10 != null && !string.IsNullOrEmpty(ft10.C000))
                {
                    _configs = JsonConvert.DeserializeObject<List<TemperatureConfigsModel>>(ft10.C000) ?? new List<TemperatureConfigsModel>();
                }
            }
        }

        private async void DgvDevices_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // check if the edited column is "Offset"
                if (dgvDevices.Columns[e.ColumnIndex].Name == "Offset")
                {
                    var config = (TemperatureConfigsModel)dgvDevices.Rows[e.RowIndex].DataBoundItem;

                    // Write to PLC
                    if (_easyDriverConnector.IsStarted)
                    {
                        await _easyDriverConnector.GetTag($"{config.Path}/Offset").WriteAsync(config.Offset.ToString(), WritePiority.High);
                    }

                    // Save to DB
                    using (var db = new ApplicationDbContext())
                    {
                        var ft10 = await db.FT10_TemperatureConfigs.FirstOrDefaultAsync(x => x.Actived == true);
                        if (ft10 != null)
                        {
                            var existingConfigs = JsonConvert.DeserializeObject<List<TemperatureConfigsModel>>(ft10.C000) ?? new List<TemperatureConfigsModel>();
                            var c = existingConfigs.FirstOrDefault(x => x.Id == config.Id);
                            if (c != null)
                            {
                                c.Offset = config.Offset;
                                ft10.C000 = JsonConvert.SerializeObject(existingConfigs);
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void _easyDriverConnector_Started(object sender, EventArgs e)
        {
            await Task.Delay(2000); // Đợi 2s để driver kết nối thực sự tới trạm

            // Đăng ký sự kiện thay đổi giá trị PV từ EasyDriver
            foreach (var config in _configs)
            {
                var tagPv = _easyDriverConnector.GetTag($"{config.Path}/PV");
                if (tagPv != null)
                {
                    tagPv.ValueChanged += TagPv_ValueChanged;
                }
            }
        }

        private void TagPv_ValueChanged(object sender, TagValueChangedEventArgs e)
        {
            var tag = sender as ITag;
            if (tag == null) return;

            string pathPrefix = tag.Path.Substring(0, tag.Path.LastIndexOf('/'));
            var value = e.NewValue;
            var quality = tag.Quality;

            UpdateDeviceUI(pathPrefix, value, quality);
        }

        private void UpdateDeviceUI(string pathPrefix, string value, Quality quality)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDeviceUI(pathPrefix, value, quality)));
                return;
            }

            foreach (DataGridViewRow row in dgvDevices.Rows)
            {
                var config = row.DataBoundItem as TemperatureConfigsModel;
                if (config != null && config.Path == pathPrefix)
                {
                    row.Cells["PV"].Value = value;
                    row.Cells["ConnectionStatus"].Value = quality.ToString();
                    break;
                }
            }
        }

        private void _easyDriverConnector_ConnectionStatusChaged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _easyStatus = e.NewStatus;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    labelStatus.Text = $"Connection Status: {e.NewStatus}";
                }));
            }
            else
            {
                labelStatus.Text = $"Connection Status: {e.NewStatus}";
            }
        }

        #region Background Tasks

        private async Task TaskRealtimeLogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_easyDriverConnector.IsStarted && _configs.Count > 0)
                    {
                        var realtimeModels = new List<TemperatureRealtimeModel>();
                        var now = DateTime.Now;

                        foreach (var config in _configs)
                        {
                            string pvValueStr = _easyDriverConnector.GetTag($"{config.Path}/PV")?.Value;
                            double pv = 0;
                            double.TryParse(pvValueStr, out pv);

                            var realtimeData = new TemperatureRealtimeModel
                            {
                                Id = config.Id ?? 0,
                                Name = config.Name,
                                Path = config.Path,
                                Status = true,
                                ConnectionStatus = _easyDriverConnector.IsStarted,
                                PV = pv,
                                Config = config
                            };
                            realtimeModels.Add(realtimeData);
                        }

                        using (var db = new ApplicationDbContext())
                        {
                            var existingFt11 = await db.FT11_TemperatureRealtimes.FirstOrDefaultAsync();
                            if (existingFt11 != null)
                            {
                                existingFt11.C001_Data = JsonConvert.SerializeObject(realtimeModels);
                                existingFt11.CreatedAt = now;
                            }
                            else
                            {
                                db.FT11_TemperatureRealtimes.Add(new FT11_TemperatureRealtime
                                {
                                    Id = Guid.NewGuid(),
                                    C001_Data = JsonConvert.SerializeObject(realtimeModels),
                                    CreatedAt = now
                                });
                            }
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskRealtimeLogAsync: " + ex.Message);
                }

                int delay = (_configs.FirstOrDefault()?.TimerRealtimeLog ?? 10) * 1000;
                if (delay <= 0) delay = 10000;
                await Task.Delay(delay, token);
            }
        }

        private async Task TaskDataLogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_easyDriverConnector.IsStarted && _configs.Count > 0)
                    {
                        var datalogs = new List<FT12_TemperatureDatalog>();
                        var now = DateTime.Now;

                        foreach (var config in _configs)
                        {
                            string pvValueStr = _easyDriverConnector.GetTag($"{config.Path}/PV")?.Value;
                            double pv = 0;
                            double.TryParse(pvValueStr, out pv);

                            datalogs.Add(new FT12_TemperatureDatalog
                            {
                                Id = Guid.NewGuid(),
                                LocationId = config.Id,
                                LocationName = config.Name,
                                PV = pv,
                                CreatedAt = now
                            });
                        }

                        if (datalogs.Count > 0)
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                db.FT12_TemperatureDatalogs.AddRange(datalogs);
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskDataLogAsync: " + ex.Message);
                }

                int delay = (_configs.FirstOrDefault()?.TimerDataLog ?? 300) * 1000;
                if (delay <= 0) delay = 300000;
                await Task.Delay(delay, token);
            }
        }

        private async Task TaskAlarmMonitorAsync(CancellationToken token)
        {
            bool isBlinkOn = false;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_easyDriverConnector.IsStarted && _configs.Count > 0)
                    {
                        isBlinkOn = !isBlinkOn;
                        var now = DateTime.Now;

                        using (var db = new ApplicationDbContext())
                        {
                            foreach (var config in _configs)
                            {
                                string pvValueStr = _easyDriverConnector.GetTag($"{config.Path}/PV")?.Value;
                                double pv = 0;
                                double.TryParse(pvValueStr, out pv);

                                double currentTemp = pv + config.Offset;

                                bool isAlarm = currentTemp > config.HightLevel || currentTemp < config.LowLevel;
                                
                                // Kiểm tra báo động đang mở trong DB
                                var activeAlarm = await db.FT13_TemperatureAlarmLogs
                                    .Where(x => x.LocationId == config.Id && x.EndedAt == null)
                                    .OrderByDescending(x => x.CreatedAt)
                                    .FirstOrDefaultAsync();

                                if (isAlarm)
                                {
                                    // Bật nhấp nháy UI
                                    SetRowColor(config.Path, isBlinkOn ? Color.Red : Color.White);

                                    // Nếu chưa có báo động trong DB thì tạo mới
                                    if (activeAlarm == null)
                                    {
                                        string desc = currentTemp > config.HightLevel ? "Quá nhiệt độ cao" : "Dưới nhiệt độ thấp";
                                        db.FT13_TemperatureAlarmLogs.Add(new FT13_TemperatureAlarmLog
                                        {
                                            Id = Guid.NewGuid(),
                                            LocationId = config.Id,
                                            LocationName = config.Name,
                                            Path = config.Path,
                                            PV_Alarm = pv,
                                            SV_High = config.HightLevel,
                                            SV_Low = config.LowLevel,
                                            Description = desc,
                                            CreatedAt = now,
                                            CreatedMachine = Environment.MachineName
                                        });
                                        await db.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    // Tắt nhấp nháy UI
                                    SetRowColor(config.Path, Color.White);

                                    // Nếu có báo động cũ chưa kết thúc, thì cập nhật kết thúc
                                    if (activeAlarm != null)
                                    {
                                        activeAlarm.EndedAt = now;
                                        activeAlarm.PV_Normal = pv;
                                        activeAlarm.UpdateddAt = now;
                                        await db.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskAlarmMonitorAsync: " + ex.Message);
                }

                // Nhấp nháy 1 lần mỗi giây hoặc theo config
                int delay = (int)(_configs.FirstOrDefault()?.TimeBlinkAlarm ?? 1000);
                if (delay <= 0) delay = 1000;
                await Task.Delay(delay, token);
            }
        }

        private async Task TaskCheckConfigChangesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var ft10 = await db.FT10_TemperatureConfigs.FirstOrDefaultAsync(x => x.Actived == true);
                        if (ft10 != null && ft10.IsConfigChanged == true)
                        {
                            // Reload config
                            if (!string.IsNullOrEmpty(ft10.C000))
                            {
                                _configs = JsonConvert.DeserializeObject<List<TemperatureConfigsModel>>(ft10.C000) ?? new List<TemperatureConfigsModel>();
                                
                                // Reset flag
                                ft10.IsConfigChanged = false;
                                await db.SaveChangesAsync();

                                // Update DataGridView
                                this.Invoke(new Action(() =>
                                {
                                    dgvDevices.DataSource = new System.ComponentModel.BindingList<TemperatureConfigsModel>(_configs);
                                }));

                                // Đăng ký lại sự kiện
                                if (_easyDriverConnector.IsStarted)
                                {
                                    foreach (var config in _configs)
                                    {
                                        var tagPv = _easyDriverConnector.GetTag($"{config.Path}/PV");
                                        if (tagPv != null)
                                        {
                                            tagPv.ValueChanged -= TagPv_ValueChanged; // avoid duplicate
                                            tagPv.ValueChanged += TagPv_ValueChanged;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in TaskCheckConfigChangesAsync: " + ex.Message);
                }

                await Task.Delay(5000, token); // Check mỗi 5s
            }
        }

        private void SetRowColor(string path, Color color)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetRowColor(path, color)));
                return;
            }

            foreach (DataGridViewRow row in dgvDevices.Rows)
            {
                var config = row.DataBoundItem as TemperatureConfigsModel;
                if (config != null && config.Path == path)
                {
                    row.DefaultCellStyle.BackColor = color;
                    row.DefaultCellStyle.ForeColor = color == Color.Red ? Color.White : Color.Black;
                    break;
                }
            }
        }

        #endregion
    }
}
