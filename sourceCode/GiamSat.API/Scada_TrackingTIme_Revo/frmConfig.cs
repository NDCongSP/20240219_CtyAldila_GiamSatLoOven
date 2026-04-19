using EasyScada.Winforms.Controls;
using GiamSat.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace Scada_TrackingTIme_Revo
{
    public partial class frmConfig : Form
    {
        public EasyDriverConnector EasyDriverConnector { get; set; }

        EnumSaveMode _saveMode;

        public frmConfig()
        {
            InitializeComponent();

            Load += FrmConfig_Load;
            FormClosing += FrmConfig_FormClosing;
        }

        private void FrmConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void FrmConfig_Load(object sender, EventArgs e)
        {
           
        }

        private async void _btnSave_Click(object sender, EventArgs e)
        {
            using (var dbContext = new ApplicationDbContext())
            {
                var ft07 = await dbContext.FT07_RevoConfigs
                   .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft07 != null)
                {
                    var cf = JsonConvert.DeserializeObject<RevoConfigs>(ft07.C000);

                    var oven = cf.FirstOrDefault(x => x.Id == GlobalVariable.RevoId);

                    if (oven != null)
                    {
                        oven.SaveMode = _saveMode;
                    }

                    ft07.C000 = JsonConvert.SerializeObject(cf);

                    await dbContext.SaveChangesAsync();

                    MessageBox.Show("Successfuly", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GlobalVariable.RevoConfig.SaveMode = _saveMode;
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }
}
