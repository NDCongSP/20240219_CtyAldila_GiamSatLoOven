using EasyScada.Core;
using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada_TrackingTIme_Revo
{
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector = new EasyDriverConnector();

        private RevoConfigModel _revoConfig = new RevoConfigModel();
        private RevoRealtimeModel _revoRealtimeModel = new RevoRealtimeModel();

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _easyDriverConnector.ConnectionStatusChaged -= _easyDriverConnector_ConnectionStatusChaged;
            _easyDriverConnector.Started -= _easyDriverConnector_Started;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //ddocj gias trij config
            using (var dbContext = new ApplicationDbContext())
            {
                var ft07 = await dbContext.FT07_RevoConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft07 != null)
                {
                    _revoConfig = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000).FirstOrDefault(x => x.Id == GlobalVariable.RevoId);

                    if (!_revoConfig.Id.HasValue)
                    {
                        MessageBox.Show("Không đọc được thông tin cấu hình, vui lòng kiểm tra lại kết nối đến server. Rồi tắt mở lại chương trình.", 
                            "CẢNH BÁO", MessageBoxButtons.OK, 
                            MessageBoxIcon.Warning
                            );

                        return;
                    }

                    Text = $"Chương trình giám sát thời gian chạy - Máy {_revoConfig.Name}";
                }
            }

            #region Khởi tạo easy drirver connector
            _easyDriverConnector = new EasyDriverConnector();
            _easyDriverConnector.ConnectionStatusChaged += _easyDriverConnector_ConnectionStatusChaged;
            _easyDriverConnector.BeginInit();
            _easyDriverConnector.EndInit();
            _labSriverStatus.Text = $"TT kết nối easy driver: {_easyDriverConnector.ConnectionStatus.ToString()}";

            _easyDriverConnector.Started += _easyDriverConnector_Started;
            if (_easyDriverConnector.IsStarted)
            {
                _easyDriverConnector_Started(null, null);
            }
            #endregion
        }

        private void _easyDriverConnector_ConnectionStatusChaged(object sender, ConnectionStatusChangedEventArgs e)
        {
            GlobalVariable.InvokeIfRequired(this, () =>
            {
                _labSriverStatus.BackColor = GetConnectionStatusColor(e.NewStatus);
                _labSriverStatus.Text = $"TT kết nối easy driver: {_easyDriverConnector.ConnectionStatus.ToString()}";
            });
        }

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

        private void _easyDriverConnector_Started(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
            //foreach (var item in _ovensInfo)
            {
                ////easyDriverConnector1.GetTag($"{item.Path}/Temperature").QualityChanged += Temperature_QualityChanged;
                //_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value1").ValueChanged += Value1_ValueChanged;
                //_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max1").ValueChanged += Max1_ValueChanged;
                //_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target1").ValueChanged += Target1_ValueChanged;

                //_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value2").ValueChanged += Value2_ValueChanged;
                //_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max2").ValueChanged += Max2_ValueChanged;
                //_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target2").ValueChanged += Target2_ValueChanged;

                //Value1_ValueChanged(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value1")
                //    , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value1")
                //    , "", _easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value1").Value));
                //Max1_ValueChanged(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max1")
                //   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max1")
                //   , "", _easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max1").Value));
                //Target1_ValueChanged(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target1")
                //   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target1")
                //   , "", _easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target1").Value));

                //Value2_ValueChanged(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value2")
                //   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value2")
                //   , "", _easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Value2").Value));
                //Max2_ValueChanged(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max2")
                //   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max2")
                //   , "", _easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Max2").Value));
                //Target2_ValueChanged(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target2")
                //   , new TagValueChangedEventArgs(_easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target2")
                //   , "", _easyDriverConnector.GetTag($"Local Station/Channel1/Device1/Target2").Value));
            }
        }
    }
}
