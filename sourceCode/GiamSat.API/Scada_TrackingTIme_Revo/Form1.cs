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
    public partial class Form1 : Form
    {
        private EasyDriverConnector _easyDriverConnector = new EasyDriverConnector();


        public Form1()
        {
            InitializeComponent();
        }
    }
}
