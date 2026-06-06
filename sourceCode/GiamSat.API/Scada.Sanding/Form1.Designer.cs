using System.Drawing;
using System.Windows.Forms;

namespace Scada.Sanding
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private EasyScada.Winforms.Controls.EasyDriverConnector easyDriverConnector1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing && (easyDriverConnector1 != null))
            {
                easyDriverConnector1.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.easyDriverConnector1 = new EasyScada.Winforms.Controls.EasyDriverConnector(this.components);
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
            this.grpPlcSettings = new System.Windows.Forms.GroupBox();
            this.lblShaftNumSanding = new System.Windows.Forms.Label();
            this.lblShaftNumSandingVal = new System.Windows.Forms.Label();
            this.lblShaftNumOd = new System.Windows.Forms.Label();
            this.lblShaftNumOdVal = new System.Windows.Forms.Label();
            this.lblSetFreqTarget = new System.Windows.Forms.Label();
            this.lblSetFreqTargetVal = new System.Windows.Forms.Label();
            this.lblSetFreqOffsetLow = new System.Windows.Forms.Label();
            this.lblSetFreqOffsetLowVal = new System.Windows.Forms.Label();
            this.lblSetFreqOffsetHigh = new System.Windows.Forms.Label();
            this.lblSetFreqOffsetHighVal = new System.Windows.Forms.Label();
            this.lblSetFormulaF = new System.Windows.Forms.Label();
            this.lblSetFormulaFVal = new System.Windows.Forms.Label();
            this.lblSetA = new System.Windows.Forms.Label();
            this.lblSetAVal = new System.Windows.Forms.Label();
            this.lblSetB = new System.Windows.Forms.Label();
            this.lblSetBVal = new System.Windows.Forms.Label();
            this.lblSetC = new System.Windows.Forms.Label();
            this.lblSetCVal = new System.Windows.Forms.Label();
            this.lblSetD = new System.Windows.Forms.Label();
            this.lblSetDVal = new System.Windows.Forms.Label();
            this.lblSetShaftLength = new System.Windows.Forms.Label();
            this.lblSetShaftLengthVal = new System.Windows.Forms.Label();
            this.lblSetTipOdLength1 = new System.Windows.Forms.Label();
            this.lblSetTipOdLength1Val = new System.Windows.Forms.Label();
            this.lblSetTipOdLength2 = new System.Windows.Forms.Label();
            this.lblSetTipOdLength2Val = new System.Windows.Forms.Label();
            this.lblSetTipOdLength3 = new System.Windows.Forms.Label();
            this.lblSetTipOdLength3Val = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSetOD_BOD_Val = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.easyDriverConnector1)).BeginInit();
            this.pnlTitle.SuspendLayout();
            this.grpProduction.SuspendLayout();
            this.grpParameters.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.grpPlcSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // easyDriverConnector1
            // 
            this.easyDriverConnector1.CollectionName = null;
            this.easyDriverConnector1.CommunicationMode = EasyScada.Core.CommunicationMode.ReceiveFromServer;
            this.easyDriverConnector1.DatabaseName = null;
            this.easyDriverConnector1.MongoDb_ConnectionString = null;
            this.easyDriverConnector1.Port = ((ushort)(8800));
            this.easyDriverConnector1.RefreshRate = 1000;
            this.easyDriverConnector1.ServerAddress = "127.0.0.1";
            this.easyDriverConnector1.StationName = null;
            this.easyDriverConnector1.Timeout = 30;
            this.easyDriverConnector1.UseMongoDb = false;
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(248)))));
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
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(40)))), ((int)(((byte)(80)))));
            this.lblTitle.Location = new System.Drawing.Point(16, 13);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(239, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "AUTO SANDING MONITORING";
            // 
            // grpProduction
            // 
            this.grpProduction.BackColor = System.Drawing.Color.White;
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
            this.grpProduction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(160)))));
            this.grpProduction.Location = new System.Drawing.Point(16, 64);
            this.grpProduction.Name = "grpProduction";
            this.grpProduction.Size = new System.Drawing.Size(430, 260);
            this.grpProduction.TabIndex = 1;
            this.grpProduction.TabStop = false;
            this.grpProduction.Text = "PRODUCTION INFORMATION";
            // 
            // lblLogStyleVal
            // 
            this.lblLogStyleVal.AutoSize = true;
            this.lblLogStyleVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogStyleVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblLogStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblLogStyle.Location = new System.Drawing.Point(20, 205);
            this.lblLogStyle.Name = "lblLogStyle";
            this.lblLogStyle.Size = new System.Drawing.Size(70, 19);
            this.lblLogStyle.TabIndex = 8;
            this.lblLogStyle.Text = "Log_Style:";
            // 
            // lblSandingModeVal
            // 
            this.lblSandingModeVal.AutoSize = true;
            this.lblSandingModeVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSandingModeVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblSandingMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSandingMode.Location = new System.Drawing.Point(20, 163);
            this.lblSandingMode.Name = "lblSandingMode";
            this.lblSandingMode.Size = new System.Drawing.Size(94, 19);
            this.lblSandingMode.TabIndex = 6;
            this.lblSandingMode.Text = "Auto_Manual:";
            // 
            // lblWorkOrderVal
            // 
            this.lblWorkOrderVal.AutoSize = true;
            this.lblWorkOrderVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkOrderVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblWorkOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblWorkOrder.Location = new System.Drawing.Point(20, 121);
            this.lblWorkOrder.Name = "lblWorkOrder";
            this.lblWorkOrder.Size = new System.Drawing.Size(44, 19);
            this.lblWorkOrder.TabIndex = 4;
            this.lblWorkOrder.Text = "Work:";
            // 
            // lblPartNameVal
            // 
            this.lblPartNameVal.AutoSize = true;
            this.lblPartNameVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPartNameVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblPartName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblPartName.Location = new System.Drawing.Point(20, 79);
            this.lblPartName.Name = "lblPartName";
            this.lblPartName.Size = new System.Drawing.Size(73, 19);
            this.lblPartName.TabIndex = 2;
            this.lblPartName.Text = "PartName:";
            // 
            // lblPlcStatusVal
            // 
            this.lblPlcStatusVal.AutoSize = true;
            this.lblPlcStatusVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlcStatusVal.ForeColor = System.Drawing.Color.Red;
            this.lblPlcStatusVal.Location = new System.Drawing.Point(140, 37);
            this.lblPlcStatusVal.Name = "lblPlcStatusVal";
            this.lblPlcStatusVal.Size = new System.Drawing.Size(98, 19);
            this.lblPlcStatusVal.TabIndex = 1;
            this.lblPlcStatusVal.Text = "Disconnected";
            // 
            // lblPlcStatus
            // 
            this.lblPlcStatus.AutoSize = true;
            this.lblPlcStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlcStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblPlcStatus.Location = new System.Drawing.Point(20, 37);
            this.lblPlcStatus.Name = "lblPlcStatus";
            this.lblPlcStatus.Size = new System.Drawing.Size(95, 19);
            this.lblPlcStatus.TabIndex = 0;
            this.lblPlcStatus.Text = "PlcConnected:";
            // 
            // grpParameters
            // 
            this.grpParameters.BackColor = System.Drawing.Color.White;
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
            this.grpParameters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(160)))));
            this.grpParameters.Location = new System.Drawing.Point(462, 64);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Size = new System.Drawing.Size(470, 260);
            this.grpParameters.TabIndex = 2;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "SANDING & MEASUREMENT PARAMETERS";
            // 
            // lblDiam3
            // 
            this.lblDiam3.AutoSize = true;
            this.lblDiam3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiam3.ForeColor = System.Drawing.Color.Black;
            this.lblDiam3.Location = new System.Drawing.Point(240, 163);
            this.lblDiam3.Name = "lblDiam3";
            this.lblDiam3.Size = new System.Drawing.Size(202, 20);
            this.lblDiam3.TabIndex = 16;
            this.lblDiam3.Text = "OD 3: -- mm [LL: -- / UL: --]";
            // 
            // lblDiam2
            // 
            this.lblDiam2.AutoSize = true;
            this.lblDiam2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiam2.ForeColor = System.Drawing.Color.Black;
            this.lblDiam2.Location = new System.Drawing.Point(240, 121);
            this.lblDiam2.Name = "lblDiam2";
            this.lblDiam2.Size = new System.Drawing.Size(202, 20);
            this.lblDiam2.TabIndex = 15;
            this.lblDiam2.Text = "OD 2: -- mm [LL: -- / UL: --]";
            // 
            // lblDiam1
            // 
            this.lblDiam1.AutoSize = true;
            this.lblDiam1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDiam1.ForeColor = System.Drawing.Color.Black;
            this.lblDiam1.Location = new System.Drawing.Point(240, 79);
            this.lblDiam1.Name = "lblDiam1";
            this.lblDiam1.Size = new System.Drawing.Size(202, 20);
            this.lblDiam1.TabIndex = 14;
            this.lblDiam1.Text = "OD 1: -- mm [LL: -- / UL: --]";
            // 
            // lblOkNgSandingVal
            // 
            this.lblOkNgSandingVal.AutoSize = true;
            this.lblOkNgSandingVal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOkNgSandingVal.ForeColor = System.Drawing.Color.Black;
            this.lblOkNgSandingVal.Location = new System.Drawing.Point(340, 31);
            this.lblOkNgSandingVal.Name = "lblOkNgSandingVal";
            this.lblOkNgSandingVal.Size = new System.Drawing.Size(28, 25);
            this.lblOkNgSandingVal.TabIndex = 13;
            this.lblOkNgSandingVal.Text = "--";
            // 
            // lblOkNgSanding
            // 
            this.lblOkNgSanding.AutoSize = true;
            this.lblOkNgSanding.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOkNgSanding.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblOkNgSanding.Location = new System.Drawing.Point(240, 37);
            this.lblOkNgSanding.Name = "lblOkNgSanding";
            this.lblOkNgSanding.Size = new System.Drawing.Size(112, 19);
            this.lblOkNgSanding.TabIndex = 12;
            this.lblOkNgSanding.Text = "OK_NG_Sanding:";
            // 
            // lblSpineHighVal
            // 
            this.lblSpineHighVal.AutoSize = true;
            this.lblSpineHighVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineHighVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblSpineHigh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSpineHigh.Location = new System.Drawing.Point(20, 226);
            this.lblSpineHigh.Name = "lblSpineHigh";
            this.lblSpineHigh.Size = new System.Drawing.Size(85, 19);
            this.lblSpineHigh.TabIndex = 10;
            this.lblSpineHigh.Text = "Spine_Hight:";
            // 
            // lblSpineLowVal
            // 
            this.lblSpineLowVal.AutoSize = true;
            this.lblSpineLowVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineLowVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblSpineLow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSpineLow.Location = new System.Drawing.Point(20, 195);
            this.lblSpineLow.Name = "lblSpineLow";
            this.lblSpineLow.Size = new System.Drawing.Size(76, 19);
            this.lblSpineLow.TabIndex = 8;
            this.lblSpineLow.Text = "Spine_Low:";
            // 
            // lblSpineTargetVal
            // 
            this.lblSpineTargetVal.AutoSize = true;
            this.lblSpineTargetVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineTargetVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblSpineTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSpineTarget.Location = new System.Drawing.Point(20, 163);
            this.lblSpineTarget.Name = "lblSpineTarget";
            this.lblSpineTarget.Size = new System.Drawing.Size(88, 19);
            this.lblSpineTarget.TabIndex = 6;
            this.lblSpineTarget.Text = "Spine_Target:";
            // 
            // lblSpineBVal
            // 
            this.lblSpineBVal.AutoSize = true;
            this.lblSpineBVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineBVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblSpineB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSpineB.Location = new System.Drawing.Point(20, 121);
            this.lblSpineB.Name = "lblSpineB";
            this.lblSpineB.Size = new System.Drawing.Size(59, 19);
            this.lblSpineB.TabIndex = 4;
            this.lblSpineB.Text = "Spine_B:";
            // 
            // lblSpineAVal
            // 
            this.lblSpineAVal.AutoSize = true;
            this.lblSpineAVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpineAVal.ForeColor = System.Drawing.Color.Black;
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
            this.lblSpineA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSpineA.Location = new System.Drawing.Point(20, 79);
            this.lblSpineA.Name = "lblSpineA";
            this.lblSpineA.Size = new System.Drawing.Size(60, 19);
            this.lblSpineA.TabIndex = 2;
            this.lblSpineA.Text = "Spine_A:";
            // 
            // lblMotorSpeedVal
            // 
            this.lblMotorSpeedVal.AutoSize = true;
            this.lblMotorSpeedVal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotorSpeedVal.ForeColor = System.Drawing.Color.Black;
            this.lblMotorSpeedVal.Location = new System.Drawing.Point(175, 36);
            this.lblMotorSpeedVal.Name = "lblMotorSpeedVal";
            this.lblMotorSpeedVal.Size = new System.Drawing.Size(19, 21);
            this.lblMotorSpeedVal.TabIndex = 1;
            this.lblMotorSpeedVal.Text = "0";
            // 
            // lblMotorSpeed
            // 
            this.lblMotorSpeed.AutoSize = true;
            this.lblMotorSpeed.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotorSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblMotorSpeed.Location = new System.Drawing.Point(20, 37);
            this.lblMotorSpeed.Name = "lblMotorSpeed";
            this.lblMotorSpeed.Size = new System.Drawing.Size(154, 19);
            this.lblMotorSpeed.TabIndex = 0;
            this.lblMotorSpeed.Text = "Mortor_Sanding_Speed:";
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(235)))));
            this.pnlFooter.Controls.Add(this.lblFooterStatus);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 570);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(950, 30);
            this.pnlFooter.TabIndex = 3;
            // 
            // lblFooterStatus
            // 
            this.lblFooterStatus.AutoSize = true;
            this.lblFooterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFooterStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblFooterStatus.Location = new System.Drawing.Point(16, 7);
            this.lblFooterStatus.Name = "lblFooterStatus";
            this.lblFooterStatus.Size = new System.Drawing.Size(427, 15);
            this.lblFooterStatus.TabIndex = 0;
            this.lblFooterStatus.Text = "EasyDriver Status: Disconnected | PLC Status: Disconnected | DB Server Status: --" +
    "";
            // 
            // grpPlcSettings
            // 
            this.grpPlcSettings.BackColor = System.Drawing.Color.White;
            this.grpPlcSettings.Controls.Add(this.label1);
            this.grpPlcSettings.Controls.Add(this.lblSetOD_BOD_Val);
            this.grpPlcSettings.Controls.Add(this.lblShaftNumSanding);
            this.grpPlcSettings.Controls.Add(this.lblShaftNumSandingVal);
            this.grpPlcSettings.Controls.Add(this.lblShaftNumOd);
            this.grpPlcSettings.Controls.Add(this.lblShaftNumOdVal);
            this.grpPlcSettings.Controls.Add(this.lblSetFreqTarget);
            this.grpPlcSettings.Controls.Add(this.lblSetFreqTargetVal);
            this.grpPlcSettings.Controls.Add(this.lblSetFreqOffsetLow);
            this.grpPlcSettings.Controls.Add(this.lblSetFreqOffsetLowVal);
            this.grpPlcSettings.Controls.Add(this.lblSetFreqOffsetHigh);
            this.grpPlcSettings.Controls.Add(this.lblSetFreqOffsetHighVal);
            this.grpPlcSettings.Controls.Add(this.lblSetFormulaF);
            this.grpPlcSettings.Controls.Add(this.lblSetFormulaFVal);
            this.grpPlcSettings.Controls.Add(this.lblSetA);
            this.grpPlcSettings.Controls.Add(this.lblSetAVal);
            this.grpPlcSettings.Controls.Add(this.lblSetB);
            this.grpPlcSettings.Controls.Add(this.lblSetBVal);
            this.grpPlcSettings.Controls.Add(this.lblSetC);
            this.grpPlcSettings.Controls.Add(this.lblSetCVal);
            this.grpPlcSettings.Controls.Add(this.lblSetD);
            this.grpPlcSettings.Controls.Add(this.lblSetDVal);
            this.grpPlcSettings.Controls.Add(this.lblSetShaftLength);
            this.grpPlcSettings.Controls.Add(this.lblSetShaftLengthVal);
            this.grpPlcSettings.Controls.Add(this.lblSetTipOdLength1);
            this.grpPlcSettings.Controls.Add(this.lblSetTipOdLength1Val);
            this.grpPlcSettings.Controls.Add(this.lblSetTipOdLength2);
            this.grpPlcSettings.Controls.Add(this.lblSetTipOdLength2Val);
            this.grpPlcSettings.Controls.Add(this.lblSetTipOdLength3);
            this.grpPlcSettings.Controls.Add(this.lblSetTipOdLength3Val);
            this.grpPlcSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpPlcSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(160)))));
            this.grpPlcSettings.Location = new System.Drawing.Point(16, 340);
            this.grpPlcSettings.Name = "grpPlcSettings";
            this.grpPlcSettings.Size = new System.Drawing.Size(916, 210);
            this.grpPlcSettings.TabIndex = 4;
            this.grpPlcSettings.TabStop = false;
            this.grpPlcSettings.Text = "PLC CONFIGURATION PARAMETERS";
            // 
            // lblShaftNumSanding
            // 
            this.lblShaftNumSanding.AutoSize = true;
            this.lblShaftNumSanding.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShaftNumSanding.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblShaftNumSanding.Location = new System.Drawing.Point(20, 35);
            this.lblShaftNumSanding.Name = "lblShaftNumSanding";
            this.lblShaftNumSanding.Size = new System.Drawing.Size(134, 19);
            this.lblShaftNumSanding.TabIndex = 0;
            this.lblShaftNumSanding.Text = "Shaft_Num_Sanding:";
            // 
            // lblShaftNumSandingVal
            // 
            this.lblShaftNumSandingVal.AutoSize = true;
            this.lblShaftNumSandingVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShaftNumSandingVal.ForeColor = System.Drawing.Color.Black;
            this.lblShaftNumSandingVal.Location = new System.Drawing.Point(180, 35);
            this.lblShaftNumSandingVal.Name = "lblShaftNumSandingVal";
            this.lblShaftNumSandingVal.Size = new System.Drawing.Size(21, 19);
            this.lblShaftNumSandingVal.TabIndex = 1;
            this.lblShaftNumSandingVal.Text = "--";
            // 
            // lblShaftNumOd
            // 
            this.lblShaftNumOd.AutoSize = true;
            this.lblShaftNumOd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShaftNumOd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblShaftNumOd.Location = new System.Drawing.Point(20, 70);
            this.lblShaftNumOd.Name = "lblShaftNumOd";
            this.lblShaftNumOd.Size = new System.Drawing.Size(106, 19);
            this.lblShaftNumOd.TabIndex = 2;
            this.lblShaftNumOd.Text = "Shaft_Num_OD:";
            // 
            // lblShaftNumOdVal
            // 
            this.lblShaftNumOdVal.AutoSize = true;
            this.lblShaftNumOdVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShaftNumOdVal.ForeColor = System.Drawing.Color.Black;
            this.lblShaftNumOdVal.Location = new System.Drawing.Point(180, 70);
            this.lblShaftNumOdVal.Name = "lblShaftNumOdVal";
            this.lblShaftNumOdVal.Size = new System.Drawing.Size(21, 19);
            this.lblShaftNumOdVal.TabIndex = 3;
            this.lblShaftNumOdVal.Text = "--";
            // 
            // lblSetFreqTarget
            // 
            this.lblSetFreqTarget.AutoSize = true;
            this.lblSetFreqTarget.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFreqTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetFreqTarget.Location = new System.Drawing.Point(20, 105);
            this.lblSetFreqTarget.Name = "lblSetFreqTarget";
            this.lblSetFreqTarget.Size = new System.Drawing.Size(107, 19);
            this.lblSetFreqTarget.TabIndex = 4;
            this.lblSetFreqTarget.Text = "Set_Freq_Target:";
            // 
            // lblSetFreqTargetVal
            // 
            this.lblSetFreqTargetVal.AutoSize = true;
            this.lblSetFreqTargetVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFreqTargetVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetFreqTargetVal.Location = new System.Drawing.Point(180, 105);
            this.lblSetFreqTargetVal.Name = "lblSetFreqTargetVal";
            this.lblSetFreqTargetVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetFreqTargetVal.TabIndex = 5;
            this.lblSetFreqTargetVal.Text = "--";
            // 
            // lblSetFreqOffsetLow
            // 
            this.lblSetFreqOffsetLow.AutoSize = true;
            this.lblSetFreqOffsetLow.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFreqOffsetLow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetFreqOffsetLow.Location = new System.Drawing.Point(20, 140);
            this.lblSetFreqOffsetLow.Name = "lblSetFreqOffsetLow";
            this.lblSetFreqOffsetLow.Size = new System.Drawing.Size(138, 19);
            this.lblSetFreqOffsetLow.TabIndex = 6;
            this.lblSetFreqOffsetLow.Text = "Set_Freq_Target_Low:";
            this.lblSetFreqOffsetLow.Click += new System.EventHandler(this.lblSetFreqOffsetLow_Click);
            // 
            // lblSetFreqOffsetLowVal
            // 
            this.lblSetFreqOffsetLowVal.AutoSize = true;
            this.lblSetFreqOffsetLowVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFreqOffsetLowVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetFreqOffsetLowVal.Location = new System.Drawing.Point(180, 140);
            this.lblSetFreqOffsetLowVal.Name = "lblSetFreqOffsetLowVal";
            this.lblSetFreqOffsetLowVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetFreqOffsetLowVal.TabIndex = 7;
            this.lblSetFreqOffsetLowVal.Text = "--";
            // 
            // lblSetFreqOffsetHigh
            // 
            this.lblSetFreqOffsetHigh.AutoSize = true;
            this.lblSetFreqOffsetHigh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFreqOffsetHigh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetFreqOffsetHigh.Location = new System.Drawing.Point(20, 175);
            this.lblSetFreqOffsetHigh.Name = "lblSetFreqOffsetHigh";
            this.lblSetFreqOffsetHigh.Size = new System.Drawing.Size(147, 19);
            this.lblSetFreqOffsetHigh.TabIndex = 8;
            this.lblSetFreqOffsetHigh.Text = "Set_Freq_Target_Hight:";
            // 
            // lblSetFreqOffsetHighVal
            // 
            this.lblSetFreqOffsetHighVal.AutoSize = true;
            this.lblSetFreqOffsetHighVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFreqOffsetHighVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetFreqOffsetHighVal.Location = new System.Drawing.Point(180, 175);
            this.lblSetFreqOffsetHighVal.Name = "lblSetFreqOffsetHighVal";
            this.lblSetFreqOffsetHighVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetFreqOffsetHighVal.TabIndex = 9;
            this.lblSetFreqOffsetHighVal.Text = "--";
            // 
            // lblSetFormulaF
            // 
            this.lblSetFormulaF.AutoSize = true;
            this.lblSetFormulaF.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFormulaF.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetFormulaF.Location = new System.Drawing.Point(320, 35);
            this.lblSetFormulaF.Name = "lblSetFormulaF";
            this.lblSetFormulaF.Size = new System.Drawing.Size(100, 19);
            this.lblSetFormulaF.TabIndex = 10;
            this.lblSetFormulaF.Text = "Set_Formula_F:";
            // 
            // lblSetFormulaFVal
            // 
            this.lblSetFormulaFVal.AutoSize = true;
            this.lblSetFormulaFVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetFormulaFVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetFormulaFVal.Location = new System.Drawing.Point(455, 35);
            this.lblSetFormulaFVal.Name = "lblSetFormulaFVal";
            this.lblSetFormulaFVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetFormulaFVal.TabIndex = 11;
            this.lblSetFormulaFVal.Text = "--";
            // 
            // lblSetA
            // 
            this.lblSetA.AutoSize = true;
            this.lblSetA.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetA.Location = new System.Drawing.Point(320, 70);
            this.lblSetA.Name = "lblSetA";
            this.lblSetA.Size = new System.Drawing.Size(46, 19);
            this.lblSetA.TabIndex = 12;
            this.lblSetA.Text = "Set_A:";
            // 
            // lblSetAVal
            // 
            this.lblSetAVal.AutoSize = true;
            this.lblSetAVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetAVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetAVal.Location = new System.Drawing.Point(455, 70);
            this.lblSetAVal.Name = "lblSetAVal";
            this.lblSetAVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetAVal.TabIndex = 13;
            this.lblSetAVal.Text = "--";
            // 
            // lblSetB
            // 
            this.lblSetB.AutoSize = true;
            this.lblSetB.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetB.Location = new System.Drawing.Point(320, 105);
            this.lblSetB.Name = "lblSetB";
            this.lblSetB.Size = new System.Drawing.Size(45, 19);
            this.lblSetB.TabIndex = 14;
            this.lblSetB.Text = "Set_B:";
            // 
            // lblSetBVal
            // 
            this.lblSetBVal.AutoSize = true;
            this.lblSetBVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetBVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetBVal.Location = new System.Drawing.Point(455, 105);
            this.lblSetBVal.Name = "lblSetBVal";
            this.lblSetBVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetBVal.TabIndex = 15;
            this.lblSetBVal.Text = "--";
            // 
            // lblSetC
            // 
            this.lblSetC.AutoSize = true;
            this.lblSetC.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetC.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetC.Location = new System.Drawing.Point(320, 140);
            this.lblSetC.Name = "lblSetC";
            this.lblSetC.Size = new System.Drawing.Size(46, 19);
            this.lblSetC.TabIndex = 16;
            this.lblSetC.Text = "Set_C:";
            // 
            // lblSetCVal
            // 
            this.lblSetCVal.AutoSize = true;
            this.lblSetCVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetCVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetCVal.Location = new System.Drawing.Point(455, 140);
            this.lblSetCVal.Name = "lblSetCVal";
            this.lblSetCVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetCVal.TabIndex = 17;
            this.lblSetCVal.Text = "--";
            // 
            // lblSetD
            // 
            this.lblSetD.AutoSize = true;
            this.lblSetD.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetD.Location = new System.Drawing.Point(320, 175);
            this.lblSetD.Name = "lblSetD";
            this.lblSetD.Size = new System.Drawing.Size(47, 19);
            this.lblSetD.TabIndex = 18;
            this.lblSetD.Text = "Set_D:";
            // 
            // lblSetDVal
            // 
            this.lblSetDVal.AutoSize = true;
            this.lblSetDVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetDVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetDVal.Location = new System.Drawing.Point(455, 175);
            this.lblSetDVal.Name = "lblSetDVal";
            this.lblSetDVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetDVal.TabIndex = 19;
            this.lblSetDVal.Text = "--";
            // 
            // lblSetShaftLength
            // 
            this.lblSetShaftLength.AutoSize = true;
            this.lblSetShaftLength.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetShaftLength.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetShaftLength.Location = new System.Drawing.Point(620, 35);
            this.lblSetShaftLength.Name = "lblSetShaftLength";
            this.lblSetShaftLength.Size = new System.Drawing.Size(117, 19);
            this.lblSetShaftLength.TabIndex = 20;
            this.lblSetShaftLength.Text = "Set_Shaft_Length:";
            // 
            // lblSetShaftLengthVal
            // 
            this.lblSetShaftLengthVal.AutoSize = true;
            this.lblSetShaftLengthVal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetShaftLengthVal.ForeColor = System.Drawing.Color.Black;
            this.lblSetShaftLengthVal.Location = new System.Drawing.Point(785, 35);
            this.lblSetShaftLengthVal.Name = "lblSetShaftLengthVal";
            this.lblSetShaftLengthVal.Size = new System.Drawing.Size(21, 19);
            this.lblSetShaftLengthVal.TabIndex = 21;
            this.lblSetShaftLengthVal.Text = "--";
            // 
            // lblSetTipOdLength1
            // 
            this.lblSetTipOdLength1.AutoSize = true;
            this.lblSetTipOdLength1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetTipOdLength1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetTipOdLength1.Location = new System.Drawing.Point(620, 70);
            this.lblSetTipOdLength1.Name = "lblSetTipOdLength1";
            this.lblSetTipOdLength1.Size = new System.Drawing.Size(145, 19);
            this.lblSetTipOdLength1.TabIndex = 22;
            this.lblSetTipOdLength1.Text = "Set_Tip_OD_Length_1:";
            // 
            // lblSetTipOdLength1Val
            // 
            this.lblSetTipOdLength1Val.AutoSize = true;
            this.lblSetTipOdLength1Val.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetTipOdLength1Val.ForeColor = System.Drawing.Color.Black;
            this.lblSetTipOdLength1Val.Location = new System.Drawing.Point(785, 70);
            this.lblSetTipOdLength1Val.Name = "lblSetTipOdLength1Val";
            this.lblSetTipOdLength1Val.Size = new System.Drawing.Size(21, 19);
            this.lblSetTipOdLength1Val.TabIndex = 23;
            this.lblSetTipOdLength1Val.Text = "--";
            // 
            // lblSetTipOdLength2
            // 
            this.lblSetTipOdLength2.AutoSize = true;
            this.lblSetTipOdLength2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetTipOdLength2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetTipOdLength2.Location = new System.Drawing.Point(620, 105);
            this.lblSetTipOdLength2.Name = "lblSetTipOdLength2";
            this.lblSetTipOdLength2.Size = new System.Drawing.Size(145, 19);
            this.lblSetTipOdLength2.TabIndex = 24;
            this.lblSetTipOdLength2.Text = "Set_Tip_OD_Length_2:";
            // 
            // lblSetTipOdLength2Val
            // 
            this.lblSetTipOdLength2Val.AutoSize = true;
            this.lblSetTipOdLength2Val.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetTipOdLength2Val.ForeColor = System.Drawing.Color.Black;
            this.lblSetTipOdLength2Val.Location = new System.Drawing.Point(785, 105);
            this.lblSetTipOdLength2Val.Name = "lblSetTipOdLength2Val";
            this.lblSetTipOdLength2Val.Size = new System.Drawing.Size(21, 19);
            this.lblSetTipOdLength2Val.TabIndex = 25;
            this.lblSetTipOdLength2Val.Text = "--";
            // 
            // lblSetTipOdLength3
            // 
            this.lblSetTipOdLength3.AutoSize = true;
            this.lblSetTipOdLength3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetTipOdLength3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblSetTipOdLength3.Location = new System.Drawing.Point(620, 140);
            this.lblSetTipOdLength3.Name = "lblSetTipOdLength3";
            this.lblSetTipOdLength3.Size = new System.Drawing.Size(145, 19);
            this.lblSetTipOdLength3.TabIndex = 26;
            this.lblSetTipOdLength3.Text = "Set_Tip_OD_Length_3:";
            // 
            // lblSetTipOdLength3Val
            // 
            this.lblSetTipOdLength3Val.AutoSize = true;
            this.lblSetTipOdLength3Val.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetTipOdLength3Val.ForeColor = System.Drawing.Color.Black;
            this.lblSetTipOdLength3Val.Location = new System.Drawing.Point(785, 140);
            this.lblSetTipOdLength3Val.Name = "lblSetTipOdLength3Val";
            this.lblSetTipOdLength3Val.Size = new System.Drawing.Size(21, 19);
            this.lblSetTipOdLength3Val.TabIndex = 27;
            this.lblSetTipOdLength3Val.Text = "--";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.label1.Location = new System.Drawing.Point(621, 175);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 19);
            this.label1.TabIndex = 28;
            this.label1.Text = "Set_OD_BOD:";
            // 
            // lblSetOD_BOD_Val
            // 
            this.lblSetOD_BOD_Val.AutoSize = true;
            this.lblSetOD_BOD_Val.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSetOD_BOD_Val.ForeColor = System.Drawing.Color.Black;
            this.lblSetOD_BOD_Val.Location = new System.Drawing.Point(786, 175);
            this.lblSetOD_BOD_Val.Name = "lblSetOD_BOD_Val";
            this.lblSetOD_BOD_Val.Size = new System.Drawing.Size(21, 19);
            this.lblSetOD_BOD_Val.TabIndex = 29;
            this.lblSetOD_BOD_Val.Text = "--";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.grpPlcSettings);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.grpParameters);
            this.Controls.Add(this.grpProduction);
            this.Controls.Add(this.pnlTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "AUTO SANDING MONITORING CLIENT";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.easyDriverConnector1)).EndInit();
            this.pnlTitle.ResumeLayout(false);
            this.pnlTitle.PerformLayout();
            this.grpProduction.ResumeLayout(false);
            this.grpProduction.PerformLayout();
            this.grpParameters.ResumeLayout(false);
            this.grpParameters.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.grpPlcSettings.ResumeLayout(false);
            this.grpPlcSettings.PerformLayout();
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
        private System.Windows.Forms.GroupBox grpPlcSettings;
        private System.Windows.Forms.Label lblShaftNumSanding;
        private System.Windows.Forms.Label lblShaftNumSandingVal;
        private System.Windows.Forms.Label lblShaftNumOd;
        private System.Windows.Forms.Label lblShaftNumOdVal;
        private System.Windows.Forms.Label lblSetFreqTarget;
        private System.Windows.Forms.Label lblSetFreqTargetVal;
        private System.Windows.Forms.Label lblSetFreqOffsetLow;
        private System.Windows.Forms.Label lblSetFreqOffsetLowVal;
        private System.Windows.Forms.Label lblSetFreqOffsetHigh;
        private System.Windows.Forms.Label lblSetFreqOffsetHighVal;
        private System.Windows.Forms.Label lblSetFormulaF;
        private System.Windows.Forms.Label lblSetFormulaFVal;
        private System.Windows.Forms.Label lblSetA;
        private System.Windows.Forms.Label lblSetAVal;
        private System.Windows.Forms.Label lblSetB;
        private System.Windows.Forms.Label lblSetBVal;
        private System.Windows.Forms.Label lblSetC;
        private System.Windows.Forms.Label lblSetCVal;
        private System.Windows.Forms.Label lblSetD;
        private System.Windows.Forms.Label lblSetDVal;
        private System.Windows.Forms.Label lblSetShaftLength;
        private System.Windows.Forms.Label lblSetShaftLengthVal;
        private System.Windows.Forms.Label lblSetTipOdLength1;
        private System.Windows.Forms.Label lblSetTipOdLength1Val;
        private System.Windows.Forms.Label lblSetTipOdLength2;
        private System.Windows.Forms.Label lblSetTipOdLength2Val;
        private System.Windows.Forms.Label lblSetTipOdLength3;
        private System.Windows.Forms.Label lblSetTipOdLength3Val;
        private Label label1;
        private Label lblSetOD_BOD_Val;
    }
}
