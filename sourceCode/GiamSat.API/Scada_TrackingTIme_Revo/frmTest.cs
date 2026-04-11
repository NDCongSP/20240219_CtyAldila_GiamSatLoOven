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

namespace Scada_TrackingTIme_Revo
{
    public partial class frmTest : Form
    {
        private EasyDriverConnector _easyDriverConnector;

        public frmTest()
        {
            InitializeComponent();
            Load += FrmTest_Load;
            FormClosing += FrmTest_FormClosing;
        }

        private void FrmTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            _easyDriverConnector.Stop();
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            _easyDriverConnector = new EasyDriverConnector();
            _easyDriverConnector.BeginInit();
            _easyDriverConnector.EndInit();


            //_easyDriverConnector.WriteTagAsync($"{GlobalVariable.RevoConfig.Path}/TOC_DO_HZ"
            //                                                , nextStep.Speed_Hz.HasValue ? nextStep.Speed_Hz.Value.ToString() : "0"
            //                                                 , WritePiority.High);
        }
    }
}
