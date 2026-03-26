namespace Scada_TrackingTime_AutoRolling
{
    partial class frmConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfig));
            this._radioSave = new System.Windows.Forms.RadioButton();
            this._radioSaveAll = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._btnSave = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _radioSave
            // 
            this._radioSave.AutoSize = true;
            this._radioSave.Location = new System.Drawing.Point(65, 52);
            this._radioSave.Name = "_radioSave";
            this._radioSave.Size = new System.Drawing.Size(137, 29);
            this._radioSave.TabIndex = 0;
            this._radioSave.TabStop = true;
            this._radioSave.Text = "Save (shaft)";
            this._radioSave.UseVisualStyleBackColor = true;
            // 
            // _radioSaveAll
            // 
            this._radioSaveAll.AutoSize = true;
            this._radioSaveAll.Location = new System.Drawing.Point(65, 93);
            this._radioSaveAll.Name = "_radioSaveAll";
            this._radioSaveAll.Size = new System.Drawing.Size(196, 29);
            this._radioSaveAll.TabIndex = 1;
            this._radioSaveAll.TabStop = true;
            this._radioSaveAll.Text = "Save (shaft + step)";
            this._radioSaveAll.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._radioSave);
            this.groupBox1.Controls.Add(this._radioSaveAll);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.groupBox1.Location = new System.Drawing.Point(12, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 146);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Save Type";
            // 
            // _btnSave
            // 
            this._btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this._btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this._btnSave.Location = new System.Drawing.Point(178, 157);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(164, 47);
            this._btnSave.TabIndex = 3;
            this._btnSave.Text = "Save";
            this._btnSave.UseVisualStyleBackColor = false;
            // 
            // frmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 216);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Config";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RadioButton _radioSave;
        private System.Windows.Forms.RadioButton _radioSaveAll;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button _btnSave;
    }
}