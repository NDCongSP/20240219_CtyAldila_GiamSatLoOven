using System.Drawing;
using System.Windows.Forms;

namespace Scada.TrackingTime_AutoRolling1
{
    partial class frmProduction
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
            this._labStatus = new System.Windows.Forms.Label();
            this._grv = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this._grv)).BeginInit();
            this.SuspendLayout();
            // 
            // _labStatus
            // 
            this._labStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._labStatus.Location = new System.Drawing.Point(0, 411);
            this._labStatus.Name = "_labStatus";
            this._labStatus.Size = new System.Drawing.Size(1350, 20);
            this._labStatus.TabIndex = 20;
            this._labStatus.Text = "TT kết nối:";
            this._labStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _grv
            // 
            this._grv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._grv.Location = new System.Drawing.Point(12, 36);
            this._grv.Name = "_grv";
            this._grv.Size = new System.Drawing.Size(1326, 364);
            this._grv.TabIndex = 21;
            // 
            // frmProduction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 431);
            this.Controls.Add(this._grv);
            this.Controls.Add(this._labStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmProduction";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AUTO ROLLING- Production";
            ((System.ComponentModel.ISupportInitialize)(this._grv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Label _labStatus;
        private DataGridView _grv;
    }
}