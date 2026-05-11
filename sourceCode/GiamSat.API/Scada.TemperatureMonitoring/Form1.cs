using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using System;
using System.ComponentModel;
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
        private TemperatureRepository _repository;
        private TemperatureMonitorService _monitorService;
        
        private TemperatureConfigsModel _configWrapper = new TemperatureConfigsModel();
        private CancellationTokenSource _cts;
        private Task _monitorTask;

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

                if (_monitorTask != null)
                {
                    _monitorTask.Wait(2000);
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
                _repository = new TemperatureRepository();
                _configWrapper = await _repository.GetConfigAsync();

                if (_configWrapper.LocationsConfig == null || _configWrapper.LocationsConfig.Count == 0)
                {
                    MessageBox.Show("Không đọc được thông tin cấu hình FT10. Hệ thống có thể chưa được cấu hình.", "THÔNG BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Setup DataGridView DataBinding
                dgvDevices.AutoGenerateColumns = false;
                UpdateDataGrid();
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

                // Khởi tạo Service
                _monitorService = new TemperatureMonitorService(_repository, _easyDriverConnector);
                _monitorService.SetConfig(_configWrapper);

                _monitorService.OnDeviceUpdated += UpdateDeviceUI;
                _monitorService.OnAlarmBlink += SetRowColor;
                _monitorService.OnConfigUpdated += HandleConfigUpdated;

                // Bắt đầu các Background Tasks
                _cts = new CancellationTokenSource();
                _monitorTask = _monitorService.StartTasksAsync(_cts.Token);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo: {ex.Message}");
            }
        }

        private void HandleConfigUpdated(TemperatureConfigsModel newConfig)
        {
            _configWrapper = newConfig;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateDataGrid));
            }
            else
            {
                UpdateDataGrid();
            }
        }

        private void UpdateDataGrid()
        {
            if (_configWrapper?.LocationsConfig != null)
            {
                dgvDevices.DataSource = new BindingList<TemperatureLocationModel>(_configWrapper.LocationsConfig.ToList());
            }
        }

        private async void DgvDevices_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // check if the edited column is "Offset"
                if (dgvDevices.Columns[e.ColumnIndex].Name == "Offset")
                {
                    var config = (TemperatureLocationModel)dgvDevices.Rows[e.RowIndex].DataBoundItem;

                    // Write to PLC
                    if (_easyDriverConnector.IsStarted)
                    {
                        var tag = _easyDriverConnector.GetTag($"{config.Path}/Offset");
                        if (tag != null)
                        {
                            await tag.WriteAsync(config.Offset.ToString(System.Globalization.CultureInfo.InvariantCulture), WritePiority.High);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Tag not found '{config.Path}/Offset'. Cannot write to PLC.");
                        }
                    }

                    // Save to DB
                    await _repository.UpdateOffsetAsync(config.Id, config.Offset);
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
            // Service handles subscribing to PV changes now.
            // Re-apply config to hook PV changes for driver start.
            if (_monitorService != null)
            {
                _monitorService.SetConfig(_configWrapper);
            }
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
                var config = row.DataBoundItem as TemperatureLocationModel;
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

        private void SetRowColor(string path, Color color)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetRowColor(path, color)));
                return;
            }

            foreach (DataGridViewRow row in dgvDevices.Rows)
            {
                var config = row.DataBoundItem as TemperatureLocationModel;
                if (config != null && config.Path == path)
                {
                    row.DefaultCellStyle.BackColor = color;
                    row.DefaultCellStyle.ForeColor = color == Color.Red ? Color.White : Color.Black;
                    break;
                }
            }
        }
    }
}
