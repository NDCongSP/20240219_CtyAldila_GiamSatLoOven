namespace Scada_TrackingTIme_Revo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this._lab = new System.Windows.Forms.Label();
            this._labTime = new System.Windows.Forms.Label();
            this.easyTextBox1 = new EasyScada.Winforms.Controls.EasyTextBox();
            this._labStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.easyTextBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // _lab
            // 
            this._lab.AutoSize = true;
            this._lab.Location = new System.Drawing.Point(12, 665);
            this._lab.Name = "_lab";
            this._lab.Size = new System.Drawing.Size(59, 13);
            this._lab.TabIndex = 0;
            this._lab.Text = "TT kết nối:";
            // 
            // _labTime
            // 
            this._labTime.AutoSize = true;
            this._labTime.Location = new System.Drawing.Point(1018, 665);
            this._labTime.Name = "_labTime";
            this._labTime.Size = new System.Drawing.Size(106, 13);
            this._labTime.TabIndex = 1;
            this._labTime.Text = "2026-01-20 00:00:00";
            // 
            // easyTextBox1
            // 
            this.easyTextBox1.DropDownBackColor = System.Drawing.SystemColors.Control;
            this.easyTextBox1.DropDownBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.easyTextBox1.DropDownDirection = EasyScada.Winforms.Controls.DropDownDirection.None;
            this.easyTextBox1.DropDownFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.easyTextBox1.DropDownForeColor = System.Drawing.SystemColors.ControlText;
            this.easyTextBox1.HightLightStatusTime = 3;
            this.easyTextBox1.Location = new System.Drawing.Point(352, 180);
            this.easyTextBox1.Name = "easyTextBox1";
            this.easyTextBox1.Role = null;
            this.easyTextBox1.Size = new System.Drawing.Size(100, 20);
            this.easyTextBox1.StringFormat = null;
            this.easyTextBox1.TabIndex = 2;
            this.easyTextBox1.TagPath = "Local Station/Channel_Revo_1/Device1/TOC_DO_HZ";
            this.easyTextBox1.Text = "easyTextBox1";
            this.easyTextBox1.WriteDelay = 200;
            this.easyTextBox1.WriteTrigger = EasyScada.Core.WriteTrigger.OnEnter;
            // 
            // _labStatus
            // 
            this._labStatus.AutoSize = true;
            this._labStatus.Location = new System.Drawing.Point(77, 665);
            this._labStatus.Name = "_labStatus";
            this._labStatus.Size = new System.Drawing.Size(59, 13);
            this._labStatus.TabIndex = 3;
            this._labStatus.Text = "TT kết nối:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 687);
            this.Controls.Add(this._labStatus);
            this.Controls.Add(this.easyTextBox1);
            this.Controls.Add(this._labTime);
            this.Controls.Add(this._lab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "REVO- Production";
            ((System.ComponentModel.ISupportInitialize)(this.easyTextBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lab;
        private System.Windows.Forms.Label _labTime;
        private EasyScada.Winforms.Controls.EasyTextBox easyTextBox1;
        private System.Windows.Forms.Label _labStatus;
    }
}

