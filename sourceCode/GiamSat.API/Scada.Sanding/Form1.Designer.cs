using System.Drawing;
using System.Windows.Forms;

namespace Scada.Sanding
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpProduction = new System.Windows.Forms.GroupBox();
            this.lblLogStyleVal = new System.Windows.Forms.Label();
            this.lblLogStyle = new System.Windows.Forms.Label();
            this.lblSandingModeVal = new System.Windows.Forms.Label();
            this.lblSandingMode = new System.Windows.Forms.Label();
            this.lblWorkOrderVal = new System.Windows.Forms.Label();
            this.lblWorkOrder = new System.Windows.Forms.Label();
            this.lblPartNameVal = new System.Windows.Forms.Label();
            this.lblPartName = new System.Windows.Forms.Label();
            this.lblPlcStatusVal = new System.Windows.Forms.Label();
            this.lblPlcStatus = new System.Windows.Forms.Label();
            this.grpParameters = new System.Windows.Forms.GroupBox();
            this.lblDiam3 = new System.Windows.Forms.Label();
            this.lblDiam2 = new System.Windows.Forms.Label();
            this.lblDiam1 = new System.Windows.Forms.Label();
            this.lblOkNgSandingVal = new System.Windows.Forms.Label();
            this.lblOkNgSanding = new System.Windows.Forms.Label();
            this.lblSpineHighVal = new System.Windows.Forms.Label();
            this.lblSpineHigh = new System.Windows.Forms.Label();
            this.lblSpineLowVal = new System.Windows.Forms.Label();
            this.lblSpineLow = new System.Windows.Forms.Label();
            this.lblSpineTargetVal = new System.Windows.Forms.Label();
            this.lblSpineTarget = new System.Windows.Forms.Label();
            this.lblSpineBVal = new System.Windows.Forms.Label();
            this.lblSpineB = new System.Windows.Forms.Label();
            this.lblSpineAVal = new System.Windows.Forms.Label();
            this.lblSpineA = new System.Windows.Forms.Label();
            this.lblMotorSpeedVal = new System.Windows.Forms.Label();
            this.lblMotorSpeed = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblFooterStatus = new System.Windows.Forms.Label();
            this.lstEvents = new System.Windows.Forms.ListBox();
            this.pnlTitle.SuspendLayout();
            this.grpProduction.SuspendLayout();
            this.grpParameters.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(950, 48);
            this.pnlTitle.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 13);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(232, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "AUTO SANDING MONITORING";
            // 
            // grpProduction
            // 
            this.grpProduction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.grpProduction.Controls.Add(this.lblLogStyleVal);
            this.grpProduction.Controls.Add(this.lblLogStyle);
            this.grpProduction.Controls.Add(this.lblSandingModeVal);
            this.grpProduction.Controls.Add(this.lblSandingMode);
            this.grpProduction.Controls.Add(this.lblWorkOrderVal);
            this.grpProduction.Controls.Add(this.lblWorkOrder);
            this.grpProduction.Controls.Add(this.lblPartNameVal);
            this.grpProduction.Controls.Add(this.lblPartName);
            this.grpProduction.Controls.Add(this.lblPlcStatusVal);
            this.grpProduction.Controls.Add(this.lblPlcStatus);
            this.grpProduction.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpProduction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.grpProduction.Location = new System.Drawing.Point(16, 64);
            this.grpProduction.Name = "grpProduction";
            this.grpProduction.Size = new System.Drawing.Size(430, 260);
            this.grpProduction.TabIndex = 1;
            this.grpProduction.TabStop = false;
            this.grpProduction.Text = "THÔNG TIN SẢN XUẤT";
            // 
            // lblLogStyleVal
            // 
            this.lblLogStyleVal.AutoSize = true;
            this.lblLogStyleVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogStyleVal.ForeColor = System.Drawing.Color.White;
            this.lblLogStyleVal.Location = new System.Drawing.Point(140, 205);
            this.lblLogStyleVal.Name = "lblLogStyleVal";
            this.lblLogStyleVal.Size = new System.Drawing.Size(21, 19);
            this.lblLogStyleVal.TabIndex = 9;
            this.lblLogStyleVal.Text = "--";
            // 
            // lblLogStyle
            // 
            this.lblLogStyle.AutoSize = true;
            this.lblLogStyle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblLogStyle.Location = new System.Drawing.Point(20, 205);
            this.lblLogStyle.Name = "lblLogStyle";
            this.lblLogStyle.Size = new System.Drawing.Size(68, 19);
            this.lblLogStyle.TabIndex = 8;
            this.lblLogStyle.Text = "Log Style:";
            // 
            // lblSandingModeVal
            // 
            this.lblSandingModeVal.AutoSize = true;
            this.lblSandingModeVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSandingModeVal.ForeColor = System.Drawing.Color.White;
            this.lblSandingModeVal.Location = new System.Drawing.Point(140, 163);
            this.lblSandingModeVal.Name = "lblSandingModeVal";
            this.lblSandingModeVal.Size = new System.Drawing.Size(21, 19);
            this.lblSandingModeVal.TabIndex = 7;
            this.lblSandingModeVal.Text = "--";
            // 
            // lblSandingMode
            // 
            this.lblSandingMode.AutoSize = true;
            this.lblSandingMode.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSandingMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblSandingMode.Location = new System.Drawing.Point(20, 163);
            this.lblSandingMode.Name = "lblSandingMode";
            this.lblSandingMode.Size = new System.Drawing.Size(99, 19);
            this.lblSandingMode.TabIndex = 6;
            this.lblSandingMode.Text = "Sanding Mode:";
            // 
            // lblWorkOrderVal
            // 
            this.lblWorkOrderVal.AutoSize = true;
            this.lblWorkOrderVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkOrderVal.ForeColor = System.Drawing.Color.White;
            this.lblWorkOrderVal.Location = new System.Drawing.Point(140, 121);
            this.lblWorkOrderVal.Name = "lblWorkOrderVal";
            this.lblWorkOrderVal.Size = new System.Drawing.Size(21, 19);
            this.lblWorkOrderVal.TabIndex = 5;
            this.lblWorkOrderVal.Text = "--";
            // 
            // lblWorkOrder
            // 
            this.lblWorkOrder.AutoSize = true;
            this.lblWorkOrder.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblWorkOrder.Location = new System.Drawing.Point(20, 121);
            this.lblWorkOrder.Name = "lblWorkOrder";
            this.lblWorkOrder.Size = new System.Drawing.Size(83, 19);
            this.lblWorkOrder.TabIndex = 4;
            this.lblWorkOrder.Text = "Work Order:";
            // 
            // lblPartNameVal
            // 
            this.lblPartNameVal.AutoSize = true;
            this.lblPartNameVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPartNameVal.ForeColor = System.Drawing.Color.White;
            this.lblPartNameVal.Location = new System.Drawing.Point(140, 79);
            this.lblPartNameVal.Name = "lblPartNameVal";
            this.lblPartNameVal.Size = new System.Drawing.Size(21, 19);
            this.lblPartNameVal.TabIndex = 3;
            this.lblPartNameVal.Text = "--";
            // 
            // lblPartName
            // 
            this.lblPartName.AutoSize = true;
            this.lblPartName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPartName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblPartName.Location = new System.Drawing.Point(20, 79);
            this.lblPartName.Name = "lblPartName";
            this.lblPartName.Size = new System.Drawing.Size(75, 19);
            this.lblPartName.TabIndex = 2;
            this.lblPartName.Text = "Part Name:";
            // 
            // lblPlcStatusVal
            // 
            this.lblPlcStatusVal.AutoSize = true;
            this.lblPlcStatusVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlcStatusVal.ForeColor = System.Drawing.Color.Red;
            this.lblPlcStatusVal.Location = new System.Drawing.Point(140, 37);
            this.lblPlcStatusVal.Name = "lblPlcStatusVal";
            this.lblPlcStatusVal.Size = new System.Drawing.Size(107, 19);
            this.lblPlcStatusVal.TabIndex = 1;
            this.lblPlcStatusVal.Text = "Disconnected";
            // 
            // lblPlcStatus
            // 
            this.lblPlcStatus.AutoSize = true;
            this.lblPlcStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlcStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblPlcStatus.Location = new System.Drawing.Point(20, 37);
            this.lblPlcStatus.Name = "lblPlcStatus";
            this.lblPlcStatus.Size = new System.Drawing.Size(81, 19);
            this.lblPlcStatus.TabIndex = 0;
            this.lblPlcStatus.Text = "TT kết nối:";
            // 
            // grpParameters
            // 
            this.grpParameters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.grpParameters.Controls.Add(this.lblDiam3);
            this.grpParameters.Controls.Add(this.lblDiam2);
            this.grpParameters.Controls.Add(this.lblDiam1);
            this.grpParameters.Controls.Add(this.lblOkNgSandingVal);
            this.grpParameters.Controls.Add(this.lblOkNgSanding);
            this.grpParameters.Controls.Add(this.lblSpineHighVal);
            this.grpParameters.Controls.Add(this.lblSpineHigh);
            this.grpParameters.Controls.Add(this.lblSpineLowVal);
            this.grpParameters.Controls.Add(this.lblSpineLow);
            this.grpParameters.Controls.Add(this.lblSpineTargetVal);
            this.grpParameters.Controls.Add(this.lblSpineTarget);
            this.grpParameters.Controls.Add(this.lblSpineBVal);
            this.grpParameters.Controls.Add(this.lblSpineB);
            this.grpParameters.Controls.Add(this.lblSpineAVal);
            this.grpParameters.Controls.Add(this.lblSpineA);
            this.grpParameters.Controls.Add(this.lblMotorSpeedVal);
            this.grpParameters.Controls.Add(this.lblMotorSpeed);
            this.grpParameters.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpParameters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.grpParameters.Location = new System.Drawing.Point(462, 64);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Size = new System.Drawing.Size(470, 260);
            this.grpParameters.TabIndex = 2;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "THÔNG SỐ ĐO & MÀI";
            // 
            // lblDiam3
            // 
            this.lblDiam3.AutoSize = true;
            this.lblDiam3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiam3.ForeColor = System.Drawing.Color.White;
            this.lblDiam3.Location = new System.Drawing.Point(240, 163);
            this.lblDiam3.Name = "lblDiam3";
            this.lblDiam3.Size = new System.Drawing.Size(187, 20);
            this.lblDiam3.TabIndex = 16;
            this.lblDiam3.Text = "OD 3: -- mm [LL: -- / UL: --]";
            // 
            // lblDiam2
            // 
            this.lblDiam2.AutoSize = true;
            this.lblDiam2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiam2.ForeColor = System.Drawing.Color.White;
            this.lblDiam2.Location = new System.Drawing.Point(240, 121);
            this.lblDiam2.Name = "lblDiam2";
            this.lblDiam2.Size = new System.Drawing.Size(187, 20);
            this.lblDiam2.TabIndex = 15;
            this.lblDiam2.Text = "OD 2: -- mm [LL: -- / UL: --]";
            // 
            // lblDiam1
            // 
            this.lblDiam1.AutoSize = true;
            this.lblDiam1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiam1.ForeColor = System.Drawing.Color.White;
            this.lblDiam1.Location = new System.Drawing.Point(240, 79);
            this.lblDiam1.Name = "lblDiam1";
            this.lblDiam1.Size = new System.Drawing.Size(187, 20);
            this.lblDiam1.TabIndex = 14;
            this.lblDiam1.Text = "OD 1: -- mm [LL: -- / UL: --]";
            // 
            // lblOkNgSandingVal
            // 
            this.lblOkNgSandingVal.AutoSize = true;
            this.lblOkNgSandingVal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOkNgSandingVal.ForeColor = System.Drawing.Color.White;
            this.lblOkNgSandingVal.Location = new System.Drawing.Point(340, 31);
            this.lblOkNgSandingVal.Name = "lblOkNgSandingVal";
            this.lblOkNgSandingVal.Size = new System.Drawing.Size(29, 25);
            this.lblOkNgSandingVal.TabIndex = 13;
            this.lblOkNgSandingVal.Text = "--";
            // 
            // lblOkNgSanding
            // 
            this.lblOkNgSanding.AutoSize = true;
            this.lblOkNgSanding.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOkNgSanding.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblOkNgSanding.Location = new System.Drawing.Point(240, 37);
            this.lblOkNgSanding.Name = "lblOkNgSanding";
            this.lblOkNgSanding.Size = new System.Drawing.Size(73, 19);
            this.lblOkNgSanding.TabIndex = 12;
            this.lblOkNgSanding.Text = "Sanding R:";
            // 
            // lblSpineHighVal
            // 
            this.lblSpineHighVal.AutoSize = true;
            this.lblSpineHighVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineHighVal.ForeColor = System.Drawing.Color.White;
            this.lblSpineHighVal.Location = new System.Drawing.Point(120, 226);
            this.lblSpineHighVal.Name = "lblSpineHighVal";
            this.lblSpineHighVal.Size = new System.Drawing.Size(17, 19);
            this.lblSpineHighVal.TabIndex = 11;
            this.lblSpineHighVal.Text = "0";
            // 
            // lblSpineHigh
            // 
            this.lblSpineHigh.AutoSize = true;
            this.lblSpineHigh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineHigh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblSpineHigh.Location = new System.Drawing.Point(20, 226);
            this.lblSpineHigh.Name = "lblSpineHigh";
            this.lblSpineHigh.Size = new System.Drawing.Size(77, 19);
            this.lblSpineHigh.TabIndex = 10;
            this.lblSpineHigh.Text = "Spine High:";
            // 
            // lblSpineLowVal
            // 
            this.lblSpineLowVal.AutoSize = true;
            this.lblSpineLowVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineLowVal.ForeColor = System.Drawing.Color.White;
            this.lblSpineLowVal.Location = new System.Drawing.Point(120, 195);
            this.lblSpineLowVal.Name = "lblSpineLowVal";
            this.lblSpineLowVal.Size = new System.Drawing.Size(17, 19);
            this.lblSpineLowVal.TabIndex = 9;
            this.lblSpineLowVal.Text = "0";
            // 
            // lblSpineLow
            // 
            this.lblSpineLow.AutoSize = true;
            this.lblSpineLow.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineLow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblSpineLow.Location = new System.Drawing.Point(20, 195);
            this.lblSpineLow.Name = "lblSpineLow";
            this.lblSpineLow.Size = new System.Drawing.Size(73, 19);
            this.lblSpineLow.TabIndex = 8;
            this.lblSpineLow.Text = "Spine Low:";
            // 
            // lblSpineTargetVal
            // 
            this.lblSpineTargetVal.AutoSize = true;
            this.lblSpineTargetVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineTargetVal.ForeColor = System.Drawing.Color.White;
            this.lblSpineTargetVal.Location = new System.Drawing.Point(120, 163);
            this.lblSpineTargetVal.Name = "lblSpineTargetVal";
            this.lblSpineTargetVal.Size = new System.Drawing.Size(17, 19);
            this.lblSpineTargetVal.TabIndex = 7;
            this.lblSpineTargetVal.Text = "0";
            // 
            // lblSpineTarget
            // 
            this.lblSpineTarget.AutoSize = true;
            this.lblSpineTarget.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblSpineTarget.Location = new System.Drawing.Point(20, 163);
            this.lblSpineTarget.Name = "lblSpineTarget";
            this.lblSpineTarget.Size = new System.Drawing.Size(86, 19);
            this.lblSpineTarget.TabIndex = 6;
            this.lblSpineTarget.Text = "Spine Target:";
            // 
            // lblSpineBVal
            // 
            this.lblSpineBVal.AutoSize = true;
            this.lblSpineBVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineBVal.ForeColor = System.Drawing.Color.White;
            this.lblSpineBVal.Location = new System.Drawing.Point(120, 120);
            this.lblSpineBVal.Name = "lblSpineBVal";
            this.lblSpineBVal.Size = new System.Drawing.Size(19, 21);
            this.lblSpineBVal.TabIndex = 5;
            this.lblSpineBVal.Text = "0";
            // 
            // lblSpineB
            // 
            this.lblSpineB.AutoSize = true;
            this.lblSpineB.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblSpineB.Location = new System.Drawing.Point(20, 121);
            this.lblSpineB.Name = "lblSpineB";
            this.lblSpineB.Size = new System.Drawing.Size(57, 19);
            this.lblSpineB.TabIndex = 4;
            this.lblSpineB.Text = "Spine B:";
            // 
            // lblSpineAVal
            // 
            this.lblSpineAVal.AutoSize = true;
            this.lblSpineAVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineAVal.ForeColor = System.Drawing.Color.White;
            this.lblSpineAVal.Location = new System.Drawing.Point(120, 78);
            this.lblSpineAVal.Name = "lblSpineAVal";
            this.lblSpineAVal.Size = new System.Drawing.Size(19, 21);
            this.lblSpineAVal.TabIndex = 3;
            this.lblSpineAVal.Text = "0";
            // 
            // lblSpineA
            // 
            this.lblSpineA.AutoSize = true;
            this.lblSpineA.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblSpineA.Location = new System.Drawing.Point(20, 79);
            this.lblSpineA.Name = "lblSpineA";
            this.lblSpineA.Size = new System.Drawing.Size(58, 19);
            this.lblSpineA.TabIndex = 2;
            this.lblSpineA.Text = "Spine A:";
            // 
            // lblMotorSpeedVal
            // 
            this.lblMotorSpeedVal.AutoSize = true;
            this.lblMotorSpeedVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotorSpeedVal.ForeColor = System.Drawing.Color.White;
            this.lblMotorSpeedVal.Location = new System.Drawing.Point(120, 36);
            this.lblMotorSpeedVal.Name = "lblMotorSpeedVal";
            this.lblMotorSpeedVal.Size = new System.Drawing.Size(19, 21);
            this.lblMotorSpeedVal.TabIndex = 1;
            this.lblMotorSpeedVal.Text = "0";
            // 
            // lblMotorSpeed
            // 
            this.lblMotorSpeed.AutoSize = true;
            this.lblMotorSpeed.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotorSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblMotorSpeed.Location = new System.Drawing.Point(20, 37);
            this.lblMotorSpeed.Name = "lblMotorSpeed";
            this.lblMotorSpeed.Size = new System.Drawing.Size(91, 19);
            this.lblMotorSpeed.TabIndex = 0;
            this.lblMotorSpeed.Text = "Motor Speed:";
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.pnlFooter.Controls.Add(this.lblFooterStatus);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 560);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(950, 30);
            this.pnlFooter.TabIndex = 3;
            // 
            // lblFooterStatus
            // 
            this.lblFooterStatus.AutoSize = true;
            this.lblFooterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFooterStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.lblFooterStatus.Location = new System.Drawing.Point(16, 7);
            this.lblFooterStatus.Name = "lblFooterStatus";
            this.lblFooterStatus.Size = new System.Drawing.Size(437, 15);
            this.lblFooterStatus.TabIndex = 0;
            this.lblFooterStatus.Text = "EasyDriver Status: Disconnected | PLC Status: Disconnected | DB Server Status: --";
            // 
            // lstEvents
            // 
            this.lstEvents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.lstEvents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstEvents.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstEvents.ForeColor = System.Drawing.Color.White;
            this.lstEvents.FormattingEnabled = true;
            this.lstEvents.ItemHeight = 15;
            this.lstEvents.Location = new System.Drawing.Point(16, 340);
            this.lstEvents.Name = "lstEvents";
            this.lstEvents.Size = new System.Drawing.Size(916, 210);
            this.lstEvents.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(950, 590);
            this.Controls.Add(this.lstEvents);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.grpParameters);
            this.Controls.Add(this.grpProduction);
            this.Controls.Add(this.pnlTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "AUTO SANDING MONITORING CLIENT";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlTitle.ResumeLayout(false);
            this.pnlTitle.PerformLayout();
            this.grpProduction.ResumeLayout(false);
            this.grpProduction.PerformLayout();
            this.grpParameters.ResumeLayout(false);
            this.grpParameters.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpProduction;
        private System.Windows.Forms.Label lblPlcStatus;
        private System.Windows.Forms.Label lblPlcStatusVal;
        private System.Windows.Forms.Label lblPartName;
        private System.Windows.Forms.Label lblPartNameVal;
        private System.Windows.Forms.Label lblWorkOrder;
        private System.Windows.Forms.Label lblWorkOrderVal;
        private System.Windows.Forms.Label lblSandingMode;
        private System.Windows.Forms.Label lblSandingModeVal;
        private System.Windows.Forms.Label lblLogStyle;
        private System.Windows.Forms.Label lblLogStyleVal;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.Label lblMotorSpeed;
        private System.Windows.Forms.Label lblMotorSpeedVal;
        private System.Windows.Forms.Label lblSpineA;
        private System.Windows.Forms.Label lblSpineAVal;
        private System.Windows.Forms.Label lblSpineB;
        private System.Windows.Forms.Label lblSpineBVal;
        private System.Windows.Forms.Label lblSpineTarget;
        private System.Windows.Forms.Label lblSpineTargetVal;
        private System.Windows.Forms.Label lblSpineLow;
        private System.Windows.Forms.Label lblSpineLowVal;
        private System.Windows.Forms.Label lblSpineHigh;
        private System.Windows.Forms.Label lblSpineHighVal;
        private System.Windows.Forms.Label lblOkNgSanding;
        private System.Windows.Forms.Label lblOkNgSandingVal;
        private System.Windows.Forms.Label lblDiam1;
        private System.Windows.Forms.Label lblDiam2;
        private System.Windows.Forms.Label lblDiam3;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblFooterStatus;
        private System.Windows.Forms.ListBox lstEvents;
    }
}
