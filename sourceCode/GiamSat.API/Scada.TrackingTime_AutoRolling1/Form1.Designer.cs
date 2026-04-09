using System.Drawing;
using System.Windows.Forms;

namespace Scada.TrackingTime_AutoRolling1
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
            _lab = new Label();
            _labTime = new Label();
            _labStatus = new Label();
            _btnMaintenance = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // _lab
            // 
            _lab.AutoSize = true;
            _lab.Location = new Point(14, 819);
            _lab.Margin = new Padding(4, 0, 4, 0);
            _lab.Name = "_lab";
            _lab.Size = new Size(63, 15);
            _lab.TabIndex = 0;
            _lab.Text = "TT kết nối:";
            // 
            // _labTime
            // 
            _labTime.AutoSize = true;
            _labTime.Location = new Point(1437, 819);
            _labTime.Margin = new Padding(4, 0, 4, 0);
            _labTime.Name = "_labTime";
            _labTime.Size = new Size(110, 15);
            _labTime.TabIndex = 1;
            _labTime.Text = "2026-01-20 00:00:00";
            // 
            // _labStatus
            // 
            _labStatus.AutoSize = true;
            _labStatus.Location = new Point(90, 819);
            _labStatus.Margin = new Padding(4, 0, 4, 0);
            _labStatus.Name = "_labStatus";
            _labStatus.Size = new Size(63, 15);
            _labStatus.TabIndex = 3;
            _labStatus.Text = "TT kết nối:";
            // 
            // _btnMaintenance
            // 
            _btnMaintenance.BackColor = Color.FromArgb(255, 192, 128);
            _btnMaintenance.Location = new Point(1310, 811);
            _btnMaintenance.Margin = new Padding(4, 3, 4, 3);
            _btnMaintenance.Name = "_btnMaintenance";
            _btnMaintenance.Size = new Size(91, 27);
            _btnMaintenance.TabIndex = 18;
            _btnMaintenance.Text = "Maintenance";
            _btnMaintenance.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(67, 50);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 19;
            label1.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1575, 841);
            Controls.Add(label1);
            Controls.Add(_btnMaintenance);
            Controls.Add(_labStatus);
            Controls.Add(_labTime);
            Controls.Add(_lab);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "AUTO ROLLING- Production";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label _lab;
        private Label _labTime;
        private Label _labStatus;
        private Button _btnMaintenance;
        private Label label1;
    }
}

