namespace Moras.Net.Mougdl
{
    partial class TfoDownload
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
            this.Label1 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.lbFile = new DelphiClasses.TLabel();
            this.lbStatus = new DelphiClasses.TLabel();
            this.pbFile = new System.Windows.Forms.ProgressBar();
            this.pbAll = new System.Windows.Forms.ProgressBar();
            this.bnCancel = new System.Windows.Forms.Button();
            this.xmlUpdate = new DelphiClasses.TXMLDocument(this.components);
            this.IdHTTP = new Moras.Net.IndyCustom.TIdHTTP();
            this.IdAntiFreeze = new Moras.Net.IndyCustom.TIdAntiFreeze();
            ((System.ComponentModel.ISupportInitialize)(this.xmlUpdate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(12, 12);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(66, 13);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Aktuelle Datei";
            this.Label1.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(12, 68);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(36, 13);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Gesamt";
            this.Label2.Transparent = false;
            // 
            // lbFile
            // 
            this.lbFile.AutoSize = false;
            this.lbFile.BackColor = System.Drawing.SystemColors.Control;
            this.lbFile.Location = new System.Drawing.Point(84, 28);
            this.lbFile.Name = "lbFile";
            this.lbFile.Size = new System.Drawing.Size(329, 13);
            this.lbFile.TabIndex = 0;
            this.lbFile.Transparent = false;
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = false;
            this.lbStatus.BackColor = System.Drawing.SystemColors.Control;
            this.lbStatus.Location = new System.Drawing.Point(12, 28);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(69, 13);
            this.lbStatus.TabIndex = 0;
            this.lbStatus.Text = "Prüfe:";
            this.lbStatus.Transparent = false;
            // 
            // pbFile
            // 
            this.pbFile.Location = new System.Drawing.Point(12, 44);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(401, 17);
            this.pbFile.TabIndex = 0;
            // 
            // pbAll
            // 
            this.pbAll.Location = new System.Drawing.Point(12, 84);
            this.pbAll.Name = "pbAll";
            this.pbAll.Size = new System.Drawing.Size(401, 17);
            this.pbAll.Step = 1;
            this.pbAll.TabIndex = 1;
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(176, 116);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(75, 25);
            this.bnCancel.TabIndex = 2;
            this.bnCancel.Text = "Abbrechen";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancelClick);
            // 
            // xmlUpdate
            // 
            this.xmlUpdate.DOMVendor = DelphiClasses.TDOMVendor.MSXML;
            this.xmlUpdate.FileName = null;
            this.xmlUpdate.Options = ((DelphiClasses.TXMLDocument.TXMLDocOptions)(((DelphiClasses.TXMLDocument.TXMLDocOptions.doAttrNull | DelphiClasses.TXMLDocument.TXMLDocOptions.doAutoPrefix) 
            | DelphiClasses.TXMLDocument.TXMLDocOptions.doNamespaceDecl)));
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
            this.IdHTTP.MaxAuthRetries = 0;
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
            this.IdHTTP.ReadTimeout = 0;
            this.IdHTTP.RedirectMaximum = 0;
            this.IdHTTP.Request.Accept = "text/html, */*";
            this.IdHTTP.Request.AcceptCharSet = "";
            this.IdHTTP.Request.AcceptEncoding = null;
            this.IdHTTP.Request.AcceptLanguage = null;
            this.IdHTTP.Request.Authentication = null;
            this.IdHTTP.Request.BasicAuthentication = false;
            this.IdHTTP.Request.CacheControl = null;
            this.IdHTTP.Request.CharSet = "ISO-8859-1";
            this.IdHTTP.Request.Connection = "";
            this.IdHTTP.Request.ContentDisposition = "";
            this.IdHTTP.Request.ContentEncoding = "";
            this.IdHTTP.Request.ContentLanguage = "";
            this.IdHTTP.Request.ContentLength = ((long)(-1));
            this.IdHTTP.Request.ContentRangeEnd = ((long)(0));
            this.IdHTTP.Request.ContentRangeInstanceLength = ((long)(0));
            this.IdHTTP.Request.ContentRangeStart = ((long)(0));
            this.IdHTTP.Request.ContentRangeUnits = "";
            this.IdHTTP.Request.ContentType = "text/html";
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
            this.IdHTTP.OnWork += new Indy.Sockets.TWorkEvent(this.IdHTTPWork);
            this.IdHTTP.OnWorkBegin += new Indy.Sockets.TWorkBeginEvent(this.IdHTTPWorkBegin);
            // 
            // IdAntiFreeze
            // 
            this.IdAntiFreeze.Name = "IdAntiFreeze";
            this.IdAntiFreeze.Tag = null;
            // 
            // TfoDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(426, 153);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.pbAll);
            this.Controls.Add(this.pbFile);
            this.Controls.Add(this.lbFile);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(194, 115);
            this.Name = "TfoDownload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TfoDownload";
            this.FormCreate += new System.EventHandler(this.TfoDownload_FormCreate);
            this.FormDestroy += new System.EventHandler(this.TfoDownload_FormDestroy);
            ((System.ComponentModel.ISupportInitialize)(this.xmlUpdate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DelphiClasses.TLabel Label1;
        private DelphiClasses.TLabel Label2;
        private DelphiClasses.TLabel lbFile;
        private DelphiClasses.TLabel lbStatus;
        private System.Windows.Forms.ProgressBar pbFile;
        private System.Windows.Forms.ProgressBar pbAll;
        private System.Windows.Forms.Button bnCancel;
        private DelphiClasses.TXMLDocument xmlUpdate;
        private IndyCustom.TIdHTTP IdHTTP;
        private IndyCustom.TIdAntiFreeze IdAntiFreeze;
    }
}
