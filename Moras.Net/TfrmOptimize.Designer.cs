namespace Moras.Net
{
    partial class TfrmOptimize
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
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.chNoChange = new System.Windows.Forms.CheckBox();
            this.cbFrom = new DelphiClasses.TComboBox();
            this.Label1 = new DelphiClasses.TLabel();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.stCombinations = new DelphiClasses.TLabel();
            this.stItems = new DelphiClasses.TLabel();
            this.stPositions = new DelphiClasses.TLabel();
            this.Label4 = new DelphiClasses.TLabel();
            this.Label3 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.btStart = new System.Windows.Forms.Button();
            this.Status = new System.Windows.Forms.GroupBox();
            this.stCraftCount0 = new DelphiClasses.TLabel();
            this.stCraftCount1 = new DelphiClasses.TLabel();
            this.stCraftCount2 = new DelphiClasses.TLabel();
            this.stCraftCount3 = new DelphiClasses.TLabel();
            this.stCraftCount4 = new DelphiClasses.TLabel();
            this.stCraftCount5 = new DelphiClasses.TLabel();
            this.stCraftCount7 = new DelphiClasses.TLabel();
            this.stCraftCount6 = new DelphiClasses.TLabel();
            this.stBest5 = new DelphiClasses.TLabel();
            this.stBest4 = new DelphiClasses.TLabel();
            this.stBest3 = new DelphiClasses.TLabel();
            this.stBest2 = new DelphiClasses.TLabel();
            this.stBest1 = new DelphiClasses.TLabel();
            this.stJumped = new DelphiClasses.TLabel();
            this.stChecked = new DelphiClasses.TLabel();
            this.stTime = new DelphiClasses.TLabel();
            this.stDone = new DelphiClasses.TLabel();
            this.Label6 = new DelphiClasses.TLabel();
            this.Label5 = new DelphiClasses.TLabel();
            this.btCancel = new System.Windows.Forms.Button();
            this.btAccept = new System.Windows.Forms.Button();
            this.Grid = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.TimerRefresh = new System.Windows.Forms.Timer(this.components);
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.Status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.chNoChange);
            this.GroupBox1.Controls.Add(this.cbFrom);
            this.GroupBox1.Controls.Add(this.Label1);
            this.GroupBox1.Location = new System.Drawing.Point(8, 8);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(385, 65);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Einstellungen";
            // 
            // chNoChange
            // 
            this.chNoChange.Location = new System.Drawing.Point(8, 32);
            this.chNoChange.Name = "chNoChange";
            this.chNoChange.Size = new System.Drawing.Size(257, 25);
            this.chNoChange.TabIndex = 1;
            this.chNoChange.Text = "Eingetragener Schmuck gilt als festgelegt";
            this.chNoChange.UseVisualStyleBackColor = true;
            this.chNoChange.Click += new System.EventHandler(this.chNoChangeClick);
            // 
            // cbFrom
            // 
            this.cbFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFrom.FormattingEnabled = true;
            this.cbFrom.Items.AddRange(new object[] {
            "diesem Spieler",
            "diesem Account",
            "diesem Server"});
            this.cbFrom.Location = new System.Drawing.Point(136, 12);
            this.cbFrom.Name = "cbFrom";
            this.cbFrom.Size = new System.Drawing.Size(97, 21);
            this.cbFrom.TabIndex = 0;
            this.cbFrom.Change += new System.EventHandler(this.cbFromChange);
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(8, 16);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(127, 13);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Benutze Gegenstände von";
            this.Label1.Transparent = false;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.stCombinations);
            this.GroupBox2.Controls.Add(this.stItems);
            this.GroupBox2.Controls.Add(this.stPositions);
            this.GroupBox2.Controls.Add(this.Label4);
            this.GroupBox2.Controls.Add(this.Label3);
            this.GroupBox2.Controls.Add(this.Label2);
            this.GroupBox2.Location = new System.Drawing.Point(8, 80);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(385, 89);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Werte";
            // 
            // stCombinations
            // 
            this.stCombinations.AutoSize = false;
            this.stCombinations.BackColor = System.Drawing.SystemColors.Control;
            this.stCombinations.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCombinations.Location = new System.Drawing.Point(152, 64);
            this.stCombinations.Name = "stCombinations";
            this.stCombinations.Size = new System.Drawing.Size(113, 17);
            this.stCombinations.TabIndex = 2;
            this.stCombinations.Text = "0";
            this.stCombinations.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCombinations.Transparent = false;
            // 
            // stItems
            // 
            this.stItems.AutoSize = false;
            this.stItems.BackColor = System.Drawing.SystemColors.Control;
            this.stItems.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stItems.Location = new System.Drawing.Point(152, 40);
            this.stItems.Name = "stItems";
            this.stItems.Size = new System.Drawing.Size(41, 17);
            this.stItems.TabIndex = 1;
            this.stItems.Text = "0";
            this.stItems.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stItems.Transparent = false;
            // 
            // stPositions
            // 
            this.stPositions.AutoSize = false;
            this.stPositions.BackColor = System.Drawing.SystemColors.Control;
            this.stPositions.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stPositions.Location = new System.Drawing.Point(152, 16);
            this.stPositions.Name = "stPositions";
            this.stPositions.Size = new System.Drawing.Size(41, 17);
            this.stPositions.TabIndex = 0;
            this.stPositions.Text = "0";
            this.stPositions.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stPositions.Transparent = false;
            // 
            // Label4
            // 
            this.Label4.AutoSize = false;
            this.Label4.BackColor = System.Drawing.SystemColors.Control;
            this.Label4.Location = new System.Drawing.Point(8, 64);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(137, 17);
            this.Label4.TabIndex = 0;
            this.Label4.Text = "Kombinationen:";
            this.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Label4.Transparent = false;
            // 
            // Label3
            // 
            this.Label3.AutoSize = false;
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Location = new System.Drawing.Point(8, 40);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(137, 17);
            this.Label3.TabIndex = 0;
            this.Label3.Text = "Anzahl der Gegenstände:";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Label3.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(8, 16);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(137, 13);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Zu optimierende Positionen:";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Label2.Transparent = false;
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(8, 320);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(97, 25);
            this.btStart.TabIndex = 2;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStartClick);
            // 
            // Status
            // 
            this.Status.Controls.Add(this.stCraftCount0);
            this.Status.Controls.Add(this.stCraftCount1);
            this.Status.Controls.Add(this.stCraftCount2);
            this.Status.Controls.Add(this.stCraftCount3);
            this.Status.Controls.Add(this.stCraftCount4);
            this.Status.Controls.Add(this.stCraftCount5);
            this.Status.Controls.Add(this.stCraftCount7);
            this.Status.Controls.Add(this.stCraftCount6);
            this.Status.Controls.Add(this.stBest5);
            this.Status.Controls.Add(this.stBest4);
            this.Status.Controls.Add(this.stBest3);
            this.Status.Controls.Add(this.stBest2);
            this.Status.Controls.Add(this.stBest1);
            this.Status.Controls.Add(this.stJumped);
            this.Status.Controls.Add(this.stChecked);
            this.Status.Controls.Add(this.stTime);
            this.Status.Controls.Add(this.stDone);
            this.Status.Controls.Add(this.Label6);
            this.Status.Controls.Add(this.Label5);
            this.Status.Location = new System.Drawing.Point(8, 176);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(385, 137);
            this.Status.TabIndex = 3;
            this.Status.TabStop = false;
            this.Status.Text = "Status";
            // 
            // stCraftCount0
            // 
            this.stCraftCount0.AutoSize = false;
            this.stCraftCount0.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount0.Location = new System.Drawing.Point(128, 88);
            this.stCraftCount0.Name = "stCraftCount0";
            this.stCraftCount0.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount0.TabIndex = 16;
            this.stCraftCount0.Text = "0";
            this.stCraftCount0.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount0.Transparent = false;
            // 
            // stCraftCount1
            // 
            this.stCraftCount1.AutoSize = false;
            this.stCraftCount1.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount1.Location = new System.Drawing.Point(192, 88);
            this.stCraftCount1.Name = "stCraftCount1";
            this.stCraftCount1.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount1.TabIndex = 15;
            this.stCraftCount1.Text = "0";
            this.stCraftCount1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount1.Transparent = false;
            // 
            // stCraftCount2
            // 
            this.stCraftCount2.AutoSize = false;
            this.stCraftCount2.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount2.Location = new System.Drawing.Point(256, 88);
            this.stCraftCount2.Name = "stCraftCount2";
            this.stCraftCount2.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount2.TabIndex = 14;
            this.stCraftCount2.Text = "0";
            this.stCraftCount2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount2.Transparent = false;
            // 
            // stCraftCount3
            // 
            this.stCraftCount3.AutoSize = false;
            this.stCraftCount3.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount3.Location = new System.Drawing.Point(320, 88);
            this.stCraftCount3.Name = "stCraftCount3";
            this.stCraftCount3.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount3.TabIndex = 13;
            this.stCraftCount3.Text = "0";
            this.stCraftCount3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount3.Transparent = false;
            // 
            // stCraftCount4
            // 
            this.stCraftCount4.AutoSize = false;
            this.stCraftCount4.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount4.Location = new System.Drawing.Point(128, 112);
            this.stCraftCount4.Name = "stCraftCount4";
            this.stCraftCount4.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount4.TabIndex = 12;
            this.stCraftCount4.Text = "0";
            this.stCraftCount4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount4.Transparent = false;
            // 
            // stCraftCount5
            // 
            this.stCraftCount5.AutoSize = false;
            this.stCraftCount5.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount5.Location = new System.Drawing.Point(192, 112);
            this.stCraftCount5.Name = "stCraftCount5";
            this.stCraftCount5.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount5.TabIndex = 11;
            this.stCraftCount5.Text = "0";
            this.stCraftCount5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount5.Transparent = false;
            // 
            // stCraftCount7
            // 
            this.stCraftCount7.AutoSize = false;
            this.stCraftCount7.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount7.Location = new System.Drawing.Point(320, 112);
            this.stCraftCount7.Name = "stCraftCount7";
            this.stCraftCount7.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount7.TabIndex = 10;
            this.stCraftCount7.Text = "0";
            this.stCraftCount7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount7.Transparent = false;
            // 
            // stCraftCount6
            // 
            this.stCraftCount6.AutoSize = false;
            this.stCraftCount6.BackColor = System.Drawing.SystemColors.Control;
            this.stCraftCount6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stCraftCount6.Location = new System.Drawing.Point(256, 112);
            this.stCraftCount6.Name = "stCraftCount6";
            this.stCraftCount6.Size = new System.Drawing.Size(57, 17);
            this.stCraftCount6.TabIndex = 9;
            this.stCraftCount6.Text = "0";
            this.stCraftCount6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stCraftCount6.Transparent = false;
            // 
            // stBest5
            // 
            this.stBest5.AutoSize = false;
            this.stBest5.BackColor = System.Drawing.SystemColors.Control;
            this.stBest5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stBest5.Location = new System.Drawing.Point(328, 64);
            this.stBest5.Name = "stBest5";
            this.stBest5.Size = new System.Drawing.Size(49, 17);
            this.stBest5.TabIndex = 8;
            this.stBest5.Text = "0";
            this.stBest5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stBest5.Transparent = false;
            // 
            // stBest4
            // 
            this.stBest4.AutoSize = false;
            this.stBest4.BackColor = System.Drawing.SystemColors.Control;
            this.stBest4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stBest4.Location = new System.Drawing.Point(272, 64);
            this.stBest4.Name = "stBest4";
            this.stBest4.Size = new System.Drawing.Size(49, 17);
            this.stBest4.TabIndex = 7;
            this.stBest4.Text = "0";
            this.stBest4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stBest4.Transparent = false;
            // 
            // stBest3
            // 
            this.stBest3.AutoSize = false;
            this.stBest3.BackColor = System.Drawing.SystemColors.Control;
            this.stBest3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stBest3.Location = new System.Drawing.Point(216, 64);
            this.stBest3.Name = "stBest3";
            this.stBest3.Size = new System.Drawing.Size(49, 17);
            this.stBest3.TabIndex = 6;
            this.stBest3.Text = "0";
            this.stBest3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stBest3.Transparent = false;
            // 
            // stBest2
            // 
            this.stBest2.AutoSize = false;
            this.stBest2.BackColor = System.Drawing.SystemColors.Control;
            this.stBest2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stBest2.Location = new System.Drawing.Point(152, 64);
            this.stBest2.Name = "stBest2";
            this.stBest2.Size = new System.Drawing.Size(49, 17);
            this.stBest2.TabIndex = 5;
            this.stBest2.Text = "0";
            this.stBest2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stBest2.Transparent = false;
            // 
            // stBest1
            // 
            this.stBest1.AutoSize = false;
            this.stBest1.BackColor = System.Drawing.SystemColors.Control;
            this.stBest1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stBest1.Location = new System.Drawing.Point(88, 64);
            this.stBest1.Name = "stBest1";
            this.stBest1.Size = new System.Drawing.Size(49, 17);
            this.stBest1.TabIndex = 4;
            this.stBest1.Text = "0";
            this.stBest1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stBest1.Transparent = false;
            // 
            // stJumped
            // 
            this.stJumped.AutoSize = false;
            this.stJumped.BackColor = System.Drawing.SystemColors.Control;
            this.stJumped.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stJumped.Location = new System.Drawing.Point(264, 40);
            this.stJumped.Name = "stJumped";
            this.stJumped.Size = new System.Drawing.Size(113, 17);
            this.stJumped.TabIndex = 3;
            this.stJumped.Text = "0";
            this.stJumped.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stJumped.Transparent = false;
            // 
            // stChecked
            // 
            this.stChecked.AutoSize = false;
            this.stChecked.BackColor = System.Drawing.SystemColors.Control;
            this.stChecked.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stChecked.Location = new System.Drawing.Point(264, 16);
            this.stChecked.Name = "stChecked";
            this.stChecked.Size = new System.Drawing.Size(113, 17);
            this.stChecked.TabIndex = 2;
            this.stChecked.Text = "0";
            this.stChecked.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stChecked.Transparent = false;
            // 
            // stTime
            // 
            this.stTime.AutoSize = false;
            this.stTime.BackColor = System.Drawing.SystemColors.Control;
            this.stTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stTime.Location = new System.Drawing.Point(72, 40);
            this.stTime.Name = "stTime";
            this.stTime.Size = new System.Drawing.Size(73, 17);
            this.stTime.TabIndex = 1;
            this.stTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stTime.Transparent = false;
            // 
            // stDone
            // 
            this.stDone.AutoSize = false;
            this.stDone.BackColor = System.Drawing.SystemColors.Control;
            this.stDone.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.stDone.Location = new System.Drawing.Point(16, 16);
            this.stDone.Name = "stDone";
            this.stDone.Size = new System.Drawing.Size(113, 17);
            this.stDone.TabIndex = 0;
            this.stDone.Text = "0";
            this.stDone.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.stDone.Transparent = false;
            // 
            // Label6
            // 
            this.Label6.AutoSize = false;
            this.Label6.BackColor = System.Drawing.SystemColors.Control;
            this.Label6.Location = new System.Drawing.Point(8, 64);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(73, 17);
            this.Label6.TabIndex = 0;
            this.Label6.Text = "Beste Utility";
            this.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Label6.Transparent = false;
            // 
            // Label5
            // 
            this.Label5.AutoSize = false;
            this.Label5.BackColor = System.Drawing.SystemColors.Control;
            this.Label5.Location = new System.Drawing.Point(16, 40);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(49, 17);
            this.Label5.TabIndex = 0;
            this.Label5.Text = "Dauer:";
            this.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Label5.Transparent = false;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(296, 320);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(97, 25);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "Abbruch";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btAccept
            // 
            this.btAccept.Location = new System.Drawing.Point(152, 320);
            this.btAccept.Name = "btAccept";
            this.btAccept.Size = new System.Drawing.Size(97, 25);
            this.btAccept.TabIndex = 5;
            this.btAccept.Text = "Übernehmen";
            this.btAccept.UseVisualStyleBackColor = true;
            this.btAccept.Click += new System.EventHandler(this.btAcceptClick);
            // 
            // Grid
            // 
            this.Grid.AllowUserToResizeRows = false;
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.Location = new System.Drawing.Point(8, 352);
            this.Grid.Name = "Grid";
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.RowTemplate.Height = 16;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.Grid.Size = new System.Drawing.Size(385, 161);
            this.Grid.StandardTab = true;
            this.Grid.TabIndex = 6;
            this.toolTip1.SetToolTip(this.Grid, "Ist nur zum test");
            // 
            // TimerRefresh
            // 
            this.TimerRefresh.Tick += new System.EventHandler(this.TimerRefreshTimer);
            // 
            // TfrmOptimize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(400, 520);
            this.Controls.Add(this.Grid);
            this.Controls.Add(this.btAccept);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.Status);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(403, 115);
            this.Name = "TfrmOptimize";
            this.Text = "Suche optimales Template";
            this.FormCreate += new System.EventHandler(this.TfrmOptimize_FormCreate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormClose);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.Status.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GroupBox1;
        private DelphiClasses.TLabel Label1;
        private DelphiClasses.TComboBox cbFrom;
        private System.Windows.Forms.CheckBox chNoChange;
        private System.Windows.Forms.GroupBox GroupBox2;
        private DelphiClasses.TLabel Label2;
        private DelphiClasses.TLabel Label3;
        private DelphiClasses.TLabel Label4;
        private DelphiClasses.TLabel stPositions;
        private DelphiClasses.TLabel stItems;
        private DelphiClasses.TLabel stCombinations;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.GroupBox Status;
        private DelphiClasses.TLabel Label5;
        private DelphiClasses.TLabel Label6;
        private DelphiClasses.TLabel stDone;
        private DelphiClasses.TLabel stTime;
        private DelphiClasses.TLabel stChecked;
        private DelphiClasses.TLabel stJumped;
        private DelphiClasses.TLabel stBest1;
        private DelphiClasses.TLabel stBest2;
        private DelphiClasses.TLabel stBest3;
        private DelphiClasses.TLabel stBest4;
        private DelphiClasses.TLabel stBest5;
        private DelphiClasses.TLabel stCraftCount6;
        private DelphiClasses.TLabel stCraftCount7;
        private DelphiClasses.TLabel stCraftCount5;
        private DelphiClasses.TLabel stCraftCount4;
        private DelphiClasses.TLabel stCraftCount3;
        private DelphiClasses.TLabel stCraftCount2;
        private DelphiClasses.TLabel stCraftCount1;
        private DelphiClasses.TLabel stCraftCount0;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btAccept;
        private System.Windows.Forms.DataGridView Grid;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer TimerRefresh;
    }
}