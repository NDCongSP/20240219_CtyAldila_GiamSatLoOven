
namespace GiamSat.Scada
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
            this.components = new System.ComponentModel.Container();
            this.easyDriverConnector1 = new EasyScada.Winforms.Controls.EasyDriverConnector(this.components);
            this._pnStatus = new System.Windows.Forms.Panel();
            this._labTime = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.easyDriverConnector1)).BeginInit();
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
            // _pnStatus
            // 
            this._pnStatus.Location = new System.Drawing.Point(12, 12);
            this._pnStatus.Name = "_pnStatus";
            this._pnStatus.Size = new System.Drawing.Size(123, 40);
            this._pnStatus.TabIndex = 1;
            // 
            // _labTime
            // 
            this._labTime.AutoSize = true;
            this._labTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labTime.Location = new System.Drawing.Point(654, 22);
            this._labTime.Name = "_labTime";
            this._labTime.Size = new System.Drawing.Size(78, 20);
            this._labTime.TabIndex = 2;
            this._labTime.Text = "DateTime";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 444);
            this.Controls.Add(this._labTime);
            this.Controls.Add(this._pnStatus);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.easyDriverConnector1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private EasyScada.Winforms.Controls.EasyDriverConnector easyDriverConnector1;
        private System.Windows.Forms.Panel _pnStatus;
        private System.Windows.Forms.Label _labTime;
    }
}

