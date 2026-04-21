using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Scada.TemperatureMonitoring
{
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector;
        private ConnectionStatus _easyStatus;
        private List<TemperatureConfigsModel> _configs = new List<TemperatureConfigsModel>();

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
            
            // Invoke changing UI status if we had a Label for status
            // if (this.InvokeRequired) this.Invoke(new Action(() => { labelStatus.Text = e.NewStatus.ToString(); }));
        }
    }
}
