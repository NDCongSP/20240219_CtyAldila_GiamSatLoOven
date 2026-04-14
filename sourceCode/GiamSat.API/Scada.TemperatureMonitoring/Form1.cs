using EasyScada.Core;
using EasyScada.Winforms.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scada.TemperatureMonitoring
{
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector;
        private ConnectionStatus _easyStatus;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //dọc bảng cònig lên để lấy thông tin kết nối đến easy driver server    

            //#region Khởi tạo easy drirver connector
            //_easyDriverConnector = new EasyDriverConnector();
            //_easyDriverConnector.ConnectionStatusChaged += _easyDriverConnector_ConnectionStatusChaged;
            //_easyDriverConnector.BeginInit();
            //_easyDriverConnector.EndInit();
            ////_easyStatus = _easyDriverConnector.ConnectionStatus;

            //_easyDriverConnector.Started += _easyDriverConnector_Started;
            //if (_easyDriverConnector.IsStarted)
            //{
            //    _easyDriverConnector_Started(null, null);
            //}
            //#endregion
        }
    }
}
