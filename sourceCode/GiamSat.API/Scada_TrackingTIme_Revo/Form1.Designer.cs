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
            this._labSriverStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _labSriverStatus
            // 
            this._labSriverStatus.AutoSize = true;
            this._labSriverStatus.Location = new System.Drawing.Point(12, 665);
            this._labSriverStatus.Name = "_labSriverStatus";
            this._labSriverStatus.Size = new System.Drawing.Size(87, 13);
            this._labSriverStatus.TabIndex = 0;
            this._labSriverStatus.Text = "TT kết nối Driver";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 687);
            this.Controls.Add(this._labSriverStatus);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "REVO";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _labSriverStatus;
    }
}

