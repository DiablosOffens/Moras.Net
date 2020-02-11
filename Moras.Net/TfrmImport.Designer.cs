namespace Moras.Net
{
    partial class TfrmImport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmImport));
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.lbUpdated = new DelphiClasses.TLabel();
            this.lbNew = new DelphiClasses.TLabel();
            this.lbCount = new DelphiClasses.TLabel();
            this.lbItems = new DelphiClasses.TLabel();
            this.Label3 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.Label4 = new DelphiClasses.TLabel();
            this.Label1 = new DelphiClasses.TLabel();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.pbUpdate = new System.Windows.Forms.ProgressBar();
            this.stStatus = new DelphiClasses.TLabel();
            this.stFrom = new DelphiClasses.TLabel();
            this.Label5 = new DelphiClasses.TLabel();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.IdHTTP = new Moras.Net.IndyCustom.TIdHTTP();
            this.IdAntiFreeze1 = new Moras.Net.IndyCustom.TIdAntiFreeze();
            this.webClient = new System.Net.WebClient();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox1.Controls.Add(this.lbUpdated);
            this.GroupBox1.Controls.Add(this.lbNew);
            this.GroupBox1.Controls.Add(this.lbCount);
            this.GroupBox1.Controls.Add(this.lbItems);
            this.GroupBox1.Controls.Add(this.Label3);
            this.GroupBox1.Controls.Add(this.Label2);
            this.GroupBox1.Controls.Add(this.Label4);
            this.GroupBox1.Controls.Add(this.Label1);
            this.GroupBox1.Location = new System.Drawing.Point(8, 8);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(459, 65);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Datenbank";
            // 
            // lbUpdated
            // 
            this.lbUpdated.AutoSize = false;
            this.lbUpdated.BackColor = System.Drawing.SystemColors.Control;
            this.lbUpdated.Location = new System.Drawing.Point(408, 40);
            this.lbUpdated.Name = "lbUpdated";
            this.lbUpdated.Size = new System.Drawing.Size(41, 17);
            this.lbUpdated.TabIndex = 2;
            this.lbUpdated.Text = "0";
            this.lbUpdated.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbUpdated.Transparent = false;
            // 
            // lbNew
            // 
            this.lbNew.AutoSize = false;
            this.lbNew.BackColor = System.Drawing.SystemColors.Control;
            this.lbNew.Location = new System.Drawing.Point(408, 20);
            this.lbNew.Name = "lbNew";
            this.lbNew.Size = new System.Drawing.Size(41, 17);
            this.lbNew.TabIndex = 2;
            this.lbNew.Text = "0";
            this.lbNew.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbNew.Transparent = false;
            // 
            // lbCount
            // 
            this.lbCount.AutoSize = false;
            this.lbCount.BackColor = System.Drawing.SystemColors.Control;
            this.lbCount.Location = new System.Drawing.Point(172, 40);
            this.lbCount.Name = "lbCount";
            this.lbCount.Size = new System.Drawing.Size(41, 17);
            this.lbCount.TabIndex = 2;
            this.lbCount.Text = "0";
            this.lbCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbCount.Transparent = false;
            // 
            // lbItems
            // 
            this.lbItems.AutoSize = false;
            this.lbItems.BackColor = System.Drawing.SystemColors.Control;
            this.lbItems.Location = new System.Drawing.Point(172, 20);
            this.lbItems.Name = "lbItems";
            this.lbItems.Size = new System.Drawing.Size(41, 17);
            this.lbItems.TabIndex = 2;
            this.lbItems.Text = "0";
            this.lbItems.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbItems.Transparent = false;
            // 
            // Label3
            // 
            this.Label3.AutoSize = false;
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Location = new System.Drawing.Point(240, 40);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(133, 17);
            this.Label3.TabIndex = 1;
            this.Label3.Text = "Aktualisierte Gegenstände:";
            this.Label3.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(240, 19);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(157, 18);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "Neu eingetragene Gegenstände:";
            this.Label2.Transparent = false;
            // 
            // Label4
            // 
            this.Label4.AutoSize = false;
            this.Label4.BackColor = System.Drawing.SystemColors.Control;
            this.Label4.Location = new System.Drawing.Point(8, 40);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(125, 17);
            this.Label4.TabIndex = 0;
            this.Label4.Text = "Gegenstände im Update:";
            this.Label4.Transparent = false;
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(8, 20);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(157, 13);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Gegenstände in der Datenbank:";
            this.Label1.Transparent = false;
            // 
            // btOK
            // 
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Image = ((System.Drawing.Image)(resources.GetObject("btOK.Image")));
            this.btOK.Location = new System.Drawing.Point(93, 168);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(108, 25);
            this.btOK.TabIndex = 2;
            this.btOK.Text = "&Speichern";
            this.btOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOKClick);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Image = ((System.Drawing.Image)(resources.GetObject("btCancel.Image")));
            this.btCancel.Location = new System.Drawing.Point(273, 168);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(108, 25);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "&Abbrechen";
            this.btCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancelClick);
            // 
            // GroupBox2
            // 
            this.GroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox2.Controls.Add(this.pbUpdate);
            this.GroupBox2.Controls.Add(this.stStatus);
            this.GroupBox2.Controls.Add(this.stFrom);
            this.GroupBox2.Controls.Add(this.Label5);
            this.GroupBox2.Location = new System.Drawing.Point(8, 76);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(459, 81);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Status";
            // 
            // pbUpdate
            // 
            this.pbUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbUpdate.Location = new System.Drawing.Point(6, 56);
            this.pbUpdate.Name = "pbUpdate";
            this.pbUpdate.Size = new System.Drawing.Size(445, 17);
            this.pbUpdate.TabIndex = 1;
            // 
            // stStatus
            // 
            this.stStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stStatus.AutoSize = false;
            this.stStatus.BackColor = System.Drawing.SystemColors.Control;
            this.stStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stStatus.Location = new System.Drawing.Point(6, 35);
            this.stStatus.Name = "stStatus";
            this.stStatus.Size = new System.Drawing.Size(445, 17);
            this.stStatus.TabIndex = 0;
            this.stStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.stStatus.Transparent = false;
            // 
            // stFrom
            // 
            this.stFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stFrom.AutoSize = false;
            this.stFrom.BackColor = System.Drawing.SystemColors.Control;
            this.stFrom.Location = new System.Drawing.Point(75, 16);
            this.stFrom.Name = "stFrom";
            this.stFrom.Size = new System.Drawing.Size(374, 13);
            this.stFrom.TabIndex = 0;
            this.stFrom.Transparent = false;
            this.stFrom.Click += new System.EventHandler(this.stFromClick);
            this.stFrom.MouseEnter += new System.EventHandler(this.stFromMouseEnter);
            this.stFrom.MouseLeave += new System.EventHandler(this.stFromMouseLeave);
            // 
            // Label5
            // 
            this.Label5.AutoSize = false;
            this.Label5.BackColor = System.Drawing.SystemColors.Control;
            this.Label5.Location = new System.Drawing.Point(8, 16);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(59, 13);
            this.Label5.TabIndex = 0;
            this.Label5.Text = "Update von:";
            this.Label5.Transparent = false;
            // 
            // dlgOpen
            // 
            this.dlgOpen.Filter = "Moras Itemdatei (*.xml)|*.xml";
            this.dlgOpen.InitialDirectory = ".";
            // 
            // IdHTTP
            // 
            this.IdHTTP.AllowCookies = true;
            this.IdHTTP.AuthenticationManager = null;
            this.IdHTTP.BoundIP = null;
            this.IdHTTP.BoundPort = ((ushort)(0));
            this.IdHTTP.BoundPortMax = ((ushort)(0));
            this.IdHTTP.BoundPortMin = ((ushort)(0));
            this.IdHTTP.Compressor = null;
            this.IdHTTP.ConnectTimeout = 0;
            this.IdHTTP.CookieManager = null;
            this.IdHTTP.HandleRedirects = false;
            this.IdHTTP.HTTPOptions = Indy.Sockets.HTTPOptions.hoForceEncodeParams;
            this.IdHTTP.Intercept = null;
            this.IdHTTP.IOHandler = null;
            this.IdHTTP.ManagedIOHandler = false;
            this.IdHTTP.MaxHeaderLines = 255;
            this.IdHTTP.MaxLineAction = Indy.Sockets.IndyMaxLineAction.maException;
            this.IdHTTP.Name = "IdHTTP";
            this.IdHTTP.ProtocolVersion = Indy.Sockets.HTTPProtocolVersion.pv1_1;
            this.IdHTTP.ProxyParams.Authentication = null;
            this.IdHTTP.ProxyParams.BasicAuthentication = false;
            this.IdHTTP.ProxyParams.ProxyPassword = "";
            this.IdHTTP.ProxyParams.ProxyPort = 0;
            this.IdHTTP.ProxyParams.ProxyServer = "";
            this.IdHTTP.ProxyParams.ProxyUsername = "";
            this.IdHTTP.ReadTimeout = -1;
            this.IdHTTP.Request.Accept = "text/html, */*";
            this.IdHTTP.Request.AcceptCharSet = "";
            this.IdHTTP.Request.AcceptEncoding = null;
            this.IdHTTP.Request.AcceptLanguage = null;
            this.IdHTTP.Request.Authentication = null;
            this.IdHTTP.Request.BasicAuthentication = false;
            this.IdHTTP.Request.CacheControl = null;
            this.IdHTTP.Request.CharSet = "";
            this.IdHTTP.Request.Connection = "";
            this.IdHTTP.Request.ContentDisposition = "";
            this.IdHTTP.Request.ContentEncoding = "";
            this.IdHTTP.Request.ContentLanguage = "";
            this.IdHTTP.Request.ContentLength = ((long)(-1));
            this.IdHTTP.Request.ContentRangeEnd = ((long)(0));
            this.IdHTTP.Request.ContentRangeInstanceLength = ((long)(0));
            this.IdHTTP.Request.ContentRangeStart = ((long)(0));
            this.IdHTTP.Request.ContentRangeUnits = "";
            this.IdHTTP.Request.ContentType = "";
            this.IdHTTP.Request.ContentVersion = "";
            this.IdHTTP.Request.Destination = null;
            this.IdHTTP.Request.ETag = "";
            this.IdHTTP.Request.From = null;
            this.IdHTTP.Request.Host = null;
            this.IdHTTP.Request.IPVersion = Indy.Sockets.IndyIPVersion.Id_IPv4;
            this.IdHTTP.Request.Method = null;
            this.IdHTTP.Request.MethodOverride = "";
            this.IdHTTP.Request.Password = null;
            this.IdHTTP.Request.Pragma = null;
            this.IdHTTP.Request.ProxyConnection = null;
            this.IdHTTP.Request.Range = "";
            this.IdHTTP.Request.Referer = null;
            this.IdHTTP.Request.Source = null;
            this.IdHTTP.Request.TransferEncoding = null;
            this.IdHTTP.Request.URL = null;
            this.IdHTTP.Request.UserAgent = "Mozilla/3.0 (compatible; Indy Library)";
            this.IdHTTP.Request.Username = null;
            this.IdHTTP.ReuseSocket = Indy.Sockets.ReuseSocket.rsOSDependent;
            this.IdHTTP.Tag = null;
            this.IdHTTP.UseNagle = true;
            this.IdHTTP.WorkTarget = null;
            this.IdHTTP.OnConnected += new Borland.Vcl.TNotifyEvent(this.IdHTTPConnected);
            this.IdHTTP.OnWork += new Indy.Sockets.TWorkEvent(this.IdHTTPWork);
            this.IdHTTP.OnWorkBegin += new Indy.Sockets.TWorkBeginEvent(this.IdHTTPWorkBegin);
            // 
            // IdAntiFreeze1
            // 
            this.IdAntiFreeze1.IdleTimeOut = 150;
            this.IdAntiFreeze1.Name = "IdAntiFreeze1";
            this.IdAntiFreeze1.Tag = null;
            // 
            // webClient
            // 
            this.webClient.BaseAddress = "";
            this.webClient.CachePolicy = null;
            this.webClient.Credentials = null;
            this.webClient.Encoding = ((System.Text.Encoding)(resources.GetObject("webClient.Encoding")));
            this.webClient.Headers = ((System.Net.WebHeaderCollection)(resources.GetObject("webClient.Headers")));
            this.webClient.QueryString = ((System.Collections.Specialized.NameValueCollection)(resources.GetObject("webClient.QueryString")));
            this.webClient.UseDefaultCredentials = false;
            // 
            // TfrmImport
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(475, 202);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.GroupBox1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Importiere Gegenstände...";
            this.FormCreate += new System.EventHandler(this.TfrmImport_FormCreate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormClose);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GroupBox1;
        private DelphiClasses.TLabel Label1;
        private DelphiClasses.TLabel Label2;
        private DelphiClasses.TLabel Label3;
        private DelphiClasses.TLabel lbItems;
        private DelphiClasses.TLabel lbNew;
        private DelphiClasses.TLabel lbUpdated;
        private DelphiClasses.TLabel Label4;
        private DelphiClasses.TLabel lbCount;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox GroupBox2;
        private DelphiClasses.TLabel Label5;
        private DelphiClasses.TLabel stFrom;
        private DelphiClasses.TLabel stStatus;
        private System.Windows.Forms.ProgressBar pbUpdate;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
        private IndyCustom.TIdHTTP IdHTTP;
        private IndyCustom.TIdAntiFreeze IdAntiFreeze1;
        private System.Net.WebClient webClient;
    }
}