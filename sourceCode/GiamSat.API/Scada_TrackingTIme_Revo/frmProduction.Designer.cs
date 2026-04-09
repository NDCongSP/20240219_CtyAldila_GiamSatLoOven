namespace Scada_TrackingTIme_Revo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProduction));
            this._lab = new System.Windows.Forms.Label();
            this._labTime = new System.Windows.Forms.Label();
            this._labStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._txtPart = new System.Windows.Forms.TextBox();
            this._txtWork = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._labRev = new System.Windows.Forms.Label();
            this._labShaftCurent = new System.Windows.Forms.Label();
            this._labTotalShaftCurrentHour = new System.Windows.Forms.Label();
            this._labMandrel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this._labMandrelStart = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.flowMain = new System.Windows.Forms.FlowLayoutPanel();
            this._btnMaintenance = new System.Windows.Forms.Button();
            this._labTotalShaftLastHour = new System.Windows.Forms.Label();
            this._labShaftLastHour = new System.Windows.Forms.Label();
            this._btnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _lab
            // 
            this._lab.AutoSize = true;
            this._lab.Location = new System.Drawing.Point(12, 710);
            this._lab.Name = "_lab";
            this._lab.Size = new System.Drawing.Size(59, 13);
            this._lab.TabIndex = 0;
            this._lab.Text = "TT kết nối:";
            // 
            // _labTime
            // 
            this._labTime.AutoSize = true;
            this._labTime.Location = new System.Drawing.Point(1232, 710);
            this._labTime.Name = "_labTime";
            this._labTime.Size = new System.Drawing.Size(106, 13);
            this._labTime.TabIndex = 1;
            this._labTime.Text = "2026-01-20 00:00:00";
            // 
            // _labStatus
            // 
            this._labStatus.AutoSize = true;
            this._labStatus.Location = new System.Drawing.Point(77, 710);
            this._labStatus.Name = "_labStatus";
            this._labStatus.Size = new System.Drawing.Size(59, 13);
            this._labStatus.TabIndex = 3;
            this._labStatus.Text = "TT kết nối:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Part:";
            // 
            // _txtPart
            // 
            this._txtPart.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtPart.Location = new System.Drawing.Point(110, 17);
            this._txtPart.Name = "_txtPart";
            this._txtPart.Size = new System.Drawing.Size(203, 30);
            this._txtPart.TabIndex = 5;
            // 
            // _txtWork
            // 
            this._txtWork.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtWork.Location = new System.Drawing.Point(110, 61);
            this._txtWork.Name = "_txtWork";
            this._txtWork.Size = new System.Drawing.Size(203, 30);
            this._txtWork.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "Work:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(451, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "Rev:";
            // 
            // _labRev
            // 
            this._labRev.AutoSize = true;
            this._labRev.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labRev.ForeColor = System.Drawing.SystemColors.Highlight;
            this._labRev.Location = new System.Drawing.Point(575, 20);
            this._labRev.Name = "_labRev";
            this._labRev.Size = new System.Drawing.Size(26, 25);
            this._labRev.TabIndex = 9;
            this._labRev.Text = "--";
            // 
            // _labShaftCurent
            // 
            this._labShaftCurent.AutoSize = true;
            this._labShaftCurent.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labShaftCurent.Location = new System.Drawing.Point(451, 64);
            this._labShaftCurent.Name = "_labShaftCurent";
            this._labShaftCurent.Size = new System.Drawing.Size(183, 25);
            this._labShaftCurent.TabIndex = 10;
            this._labShaftCurent.Text = "Total Shaft Current:";
            // 
            // _labTotalShaftCurrentHour
            // 
            this._labTotalShaftCurrentHour.AutoSize = true;
            this._labTotalShaftCurrentHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labTotalShaftCurrentHour.Location = new System.Drawing.Point(640, 64);
            this._labTotalShaftCurrentHour.Name = "_labTotalShaftCurrentHour";
            this._labTotalShaftCurrentHour.Size = new System.Drawing.Size(26, 25);
            this._labTotalShaftCurrentHour.TabIndex = 11;
            this._labTotalShaftCurrentHour.Text = "--";
            // 
            // _labMandrel
            // 
            this._labMandrel.AutoSize = true;
            this._labMandrel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labMandrel.Location = new System.Drawing.Point(892, 20);
            this._labMandrel.Name = "_labMandrel";
            this._labMandrel.Size = new System.Drawing.Size(26, 25);
            this._labMandrel.TabIndex = 13;
            this._labMandrel.Text = "--";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(751, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 25);
            this.label8.TabIndex = 12;
            this.label8.Text = "Mandrel:";
            // 
            // _labMandrelStart
            // 
            this._labMandrelStart.AutoSize = true;
            this._labMandrelStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labMandrelStart.Location = new System.Drawing.Point(892, 64);
            this._labMandrelStart.Name = "_labMandrelStart";
            this._labMandrelStart.Size = new System.Drawing.Size(26, 25);
            this._labMandrelStart.TabIndex = 15;
            this._labMandrelStart.Text = "--";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(751, 64);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(135, 25);
            this.label10.TabIndex = 14;
            this.label10.Text = "Mandrel Start:";
            // 
            // flowMain
            // 
            this.flowMain.Location = new System.Drawing.Point(17, 117);
            this.flowMain.Name = "flowMain";
            this.flowMain.Size = new System.Drawing.Size(1320, 580);
            this.flowMain.TabIndex = 17;
            // 
            // _btnMaintenance
            // 
            this._btnMaintenance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this._btnMaintenance.Location = new System.Drawing.Point(1123, 703);
            this._btnMaintenance.Name = "_btnMaintenance";
            this._btnMaintenance.Size = new System.Drawing.Size(78, 23);
            this._btnMaintenance.TabIndex = 18;
            this._btnMaintenance.Text = "Maintenance";
            this._btnMaintenance.UseVisualStyleBackColor = false;
            // 
            // _labTotalShaftLastHour
            // 
            this._labTotalShaftLastHour.BackColor = System.Drawing.Color.Red;
            this._labTotalShaftLastHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Bold);
            this._labTotalShaftLastHour.ForeColor = System.Drawing.Color.White;
            this._labTotalShaftLastHour.Location = new System.Drawing.Point(1079, 37);
            this._labTotalShaftLastHour.Name = "_labTotalShaftLastHour";
            this._labTotalShaftLastHour.Size = new System.Drawing.Size(258, 68);
            this._labTotalShaftLastHour.TabIndex = 19;
            this._labTotalShaftLastHour.Text = "--";
            this._labTotalShaftLastHour.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _labShaftLastHour
            // 
            this._labShaftLastHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labShaftLastHour.Location = new System.Drawing.Point(1079, 6);
            this._labShaftLastHour.Name = "_labShaftLastHour";
            this._labShaftLastHour.Size = new System.Drawing.Size(258, 25);
            this._labShaftLastHour.TabIndex = 20;
            this._labShaftLastHour.Text = "Total Shafts (-- – --)";
            // 
            // _btnStart
            // 
            this._btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this._btnStart.Location = new System.Drawing.Point(330, 16);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(103, 38);
            this._btnStart.TabIndex = 21;
            this._btnStart.Text = "Start";
            this._btnStart.UseVisualStyleBackColor = true;
            // 
            // frmProduction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this._btnStart);
            this.Controls.Add(this._labShaftLastHour);
            this.Controls.Add(this._labTotalShaftLastHour);
            this.Controls.Add(this._btnMaintenance);
            this.Controls.Add(this.flowMain);
            this.Controls.Add(this._labMandrelStart);
            this.Controls.Add(this.label10);
            this.Controls.Add(this._labMandrel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this._labTotalShaftCurrentHour);
            this.Controls.Add(this._labShaftCurent);
            this.Controls.Add(this._labRev);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._txtWork);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._txtPart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._labStatus);
            this.Controls.Add(this._labTime);
            this.Controls.Add(this._lab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmProduction";
            this.Text = "REVO- Production";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lab;
        private System.Windows.Forms.Label _labTime;
        private System.Windows.Forms.Label _labStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _txtPart;
        private System.Windows.Forms.TextBox _txtWork;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label _labRev;
        private System.Windows.Forms.Label _labShaftCurent;
        private System.Windows.Forms.Label _labTotalShaftCurrentHour;
        private System.Windows.Forms.Label _labMandrel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label _labMandrelStart;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.FlowLayoutPanel flowMain;
        private System.Windows.Forms.Button _btnMaintenance;
        private System.Windows.Forms.Label _labShaftLastHour;
        private System.Windows.Forms.Label _labTotalShaftLastHour;
        private System.Windows.Forms.Button _btnStart;
    }
}

