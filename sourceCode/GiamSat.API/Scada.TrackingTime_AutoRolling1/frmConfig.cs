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

namespace Scada.TrackingTime_AutoRolling1
{
    public partial class frmConfig : Form
    {
        EnumSaveMode _saveMode;

        public frmConfig()
        {
            InitializeComponent();

            Load += FrmConfig_Load;
            FormClosing += FrmConfig_FormClosing;
        }

        private void FrmConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            _btnSave.Click -= _btnSave_Click;
        }

        private void FrmConfig_Load(object sender, EventArgs e)
        {
            _btnSave.Click += _btnSave_Click;

            _radioSave.CheckedChanged += (s, o) =>
            {
                var sender = s as RadioButton;
                if (sender.Checked)
                {
                    _saveMode = EnumSaveMode.Save;

                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        _radioSaveAll.Checked = false;
                    });
                }
            };

            _radioSaveAll.CheckedChanged += (s, o) =>
            {
                var sender = s as RadioButton;
                if (sender.Checked)
                {
                    _saveMode = EnumSaveMode.SaveAll;

                    GlobalVariable.InvokeIfRequired(this, () =>
                    {
                        _radioSave.Checked = false;
                    });
                }
            };

            if (GlobalVariable.RevoConfigs.FirstOrDefault().SaveMode == GiamSat.Models.EnumSaveMode.Save)
            {
                _radioSave.Checked = true;
                _radioSaveAll.Checked = false;
            }
            else
            {
                _radioSave.Checked = false;
                _radioSaveAll.Checked = true;
            }
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

                    var autoRollingMachines = cf.Where(x => x.Name.Contains("Auto Rolling")).ToList();

                    autoRollingMachines.ForEach(x => x.SaveMode = _saveMode);

                    ft07.C000 = JsonConvert.SerializeObject(cf);

                    await dbContext.SaveChangesAsync();

                    MessageBox.Show("Successfuly", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    GlobalVariable.RevoConfigs.ForEach(x => x.SaveMode = _saveMode);
                }
            }
        }
    }
}
