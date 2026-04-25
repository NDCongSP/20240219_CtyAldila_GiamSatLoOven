using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Text;

namespace Scada.TemperatureMonitoring
{
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector;
        private ConnectionStatus _easyStatus;
        private List<TemperatureConfigsModel> _configs = new List<TemperatureConfigsModel>();
        private Timer _timer;
        private int _datalogTickCounter = 0;

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
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Retrieve configs using API (instead of direct DB Connection to keep WinForm decoupled)
                string apiUrl = "http://localhost:5000/api/TemperatureConfig"; // Need to bind properly based on deployment
                
                using (HttpClient client = new HttpClient())
                {
                    // For dev purposes, assuming API matches base
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        _configs = JsonConvert.DeserializeObject<List<TemperatureConfigsModel>>(json) ?? new List<TemperatureConfigsModel>();
                    }
                }

                if (_configs.Count == 0)
                {
                    MessageBox.Show("Không đọc được thông tin cấu hình FT10. Hệ thống có thể chưa được cấu hình. Vui lòng kiểm tra lại cấu hình qua màn hình quản trị WEB.", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #region Khởi tạo easy driver connector
                _easyDriverConnector = new EasyDriverConnector();
                _easyDriverConnector.ConnectionStatusChaged += _easyDriverConnector_ConnectionStatusChaged;

                // Configure tag bindings according to _configs
                foreach (var config in _configs)
                {
                    // Wait for start to access tags or add paths dynamically if necessary
                    // _easyDriverConnector will connect to Local Station based on those paths.
                    Console.WriteLine($"Initialized Config: {config.Name} - Path: {config.Path}");
                }

                _easyDriverConnector.BeginInit();
                _easyDriverConnector.EndInit();

                _easyDriverConnector.Started += _easyDriverConnector_Started;
                if (_easyDriverConnector.IsStarted)
                {
                    _easyDriverConnector_Started(null, null);
                }

                // Start Timers
                _timer = new Timer();
                _timer.Interval = 1000; // 1 second
                _timer.Tick += Timer_Tick;
                _timer.Start();

                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo DataCollector: {ex.Message}", "LỖI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _easyDriverConnector_Started(object sender, EventArgs e)
        {
            // Triggered when connector is started
        }

        private void _easyDriverConnector_ConnectionStatusChaged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _easyStatus = e.NewStatus;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            try
            {
                if (_easyStatus != ConnectionStatus.Connected) return;

                var payload = new List<TemperatureRealtimeModel>();

                foreach (var config in _configs)
                {
                    ITag pvTag = _easyDriverConnector.GetTag($"{config.Path}/PV");
                    double pvValue = 0;

                    if (pvTag != null && pvTag.Value != null)
                    {
                        double.TryParse(pvTag.Value.ToString(), out pvValue);
                    }

                    payload.Add(new TemperatureRealtimeModel
                    {
                        Id = (int)config.Id,
                        Name = config.Name,
                        Path = config.Path,
                        PV = pvValue + config.Offset,
                        ConnectionStatus = pvTag?.Quality == Quality.Good,
                        Config = config
                    });
                }

                using (var client = new HttpClient())
                {
                    string json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Gửi Realtime (Update FT11, FT13)
                    await client.PostAsync("http://localhost:5000/api/TemperatureData/SyncRealtime", content);

                    // Gửi Datalog (Insert FT12 mỗi 60s)
                    _datalogTickCounter++;
                    if (_datalogTickCounter >= 60)
                    {
                        _datalogTickCounter = 0;
                        await client.PostAsync("http://localhost:5000/api/TemperatureData/SyncDatalog", content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ticking: {ex.Message}");
            }
            finally
            {
                _timer.Start();
            }
        }
    }
}
