using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;

namespace DelphiClasses
{
    public enum TColumnLayout
    {
        clHorizontalThenVertical,
        clVerticalThenHorizontal
    }

    //TODO: implement layout handling like TableLayoutPanel
    public class TRadioGroup : GroupBox, ISupportInitialize
    {
        private static readonly object EventSelectionChanged = new object();
        private const string rsIndexOutOfBounds = "{0} Index {1} out of bounds 0 .. {2}";

        private class TRadioGroupStringList : TStringList
        {
            private TRadioGroup FRadioGroup;

            public TRadioGroupStringList(TRadioGroup ARadioGroup)
            {
                FRadioGroup = ARadioGroup;
            }

            protected override void OnChanged(EventArgs e)
            {
                base.OnChanged(e);
                if (UpdateCount == 0)
                    FRadioGroup.UpdateAll();
                else
                    FRadioGroup.UpdateInternalObjectList();
            }

            public override void Assign(TStringList Source)
            {
                int SavedIndex = FRadioGroup.ItemIndex;
                base.Assign(Source);
                if (SavedIndex < Count) FRadioGroup.ItemIndex = SavedIndex;
            }
        }

        private enum PanelLayout { cclLeftToRightThenTopToBottom, cclTopToBottomThenLeftToRight }
        private class ChildSizingPanel : TableLayoutPanel
        {
            private class ChildSizingControlCollection : TableLayoutControlCollection
            {
                ChildSizingPanel _container;
                public ChildSizingControlCollection(ChildSizingPanel container)
                    : base(container)
                {
                    _container = container;
                }

                public override void Add(Control value)
                {
                    value.Dock = DockStyle.Fill;
                    value.Margin = new Padding(_container.LeftRightSpacing / 2, _container.TopBottomSpacing / 2, _container.LeftRightSpacing / 2, _container.TopBottomSpacing / 2);
                    base.Add(value);
                    _container.SetLayoutSettings(value, Count - 1);
                }

                public override void Remove(Control value)
                {
                    int index = IndexOf(value);
                    base.Remove(value);
                    int count = Count;
                    _container.ResetRasterDimensions(count);
                    for (int i = index; i < count; i++)
                    {
                        _container.SetLayoutSettings(this[i], i);
                    }
                }
            }

            PanelLayout panelLayout;
            public PanelLayout PanelLayout
            {
                get { return panelLayout; }
                set
                {
                    if (panelLayout == value) return;
                    panelLayout = value;
                    UpdateLayoutSettings();
                }
            }
            int controlsPerLine;
            public int ControlsPerLine
            {
                get { return controlsPerLine; }
                set
                {
                    if (controlsPerLine == value) return;
                    controlsPerLine = value;
                    UpdateLayoutSettings();
                }
            }
            public int LeftRightSpacing { get; set; }
            public int TopBottomSpacing { get; set; }

            public ChildSizingPanel()
            {
                Dock = DockStyle.Fill;
            }

            protected override ControlCollection CreateControlsInstance()
            {
                return new ChildSizingControlCollection(this);
            }

            internal void ResetRasterDimensions(int newCount)
            {
                if (PanelLayout == PanelLayout.cclLeftToRightThenTopToBottom)
                {
                    ColumnCount = Math.Min(newCount, ControlsPerLine);
                    RowCount = newCount / ControlsPerLine;
                }
                else
                {
                    ColumnCount = newCount / ControlsPerLine;
                    RowCount = Math.Min(newCount, ControlsPerLine);
                }
            }

            internal void SetLayoutSettings(Control value, int index)
            {
                int column, row;
                if (PanelLayout == PanelLayout.cclLeftToRightThenTopToBottom)
                {
                    column = index % ControlsPerLine;
                    row = index / ControlsPerLine;
                }
                else
                {
                    column = index / ControlsPerLine;
                    row = index % ControlsPerLine;
                }
                if (column >= ColumnCount)
                    ColumnCount = column + 1;
                if (row >= RowCount)
                    RowCount = row + 1;
                SetColumn(value, column);
                SetRow(value, row);
            }

            private void UpdateLayoutSettings()
            {
                SuspendLayout();
                try
                {
                    ColumnCount = 0;
                    RowCount = 0;

                    for (int i = 0; i < Controls.Count; i++)
                    {
                        SetLayoutSettings(Controls[i], i);
                    }
                }
                finally
                {
                    ResumeLayout();
                }
            }
        }

        private bool FAutoFill;
        private List<RadioButton> FButtonList;
        private TColumnLayout FColumnLayout;
        private int FColumns;
        private bool FCreatingWnd;
        private RadioButton FHiddenButton;
        private bool FIgnoreClicks;
        private int FItemIndex;
        private TStringList FItems;
        private int FLastClickedItemIndex;
        private bool FUpdatingItems;
        private bool initializing;
        private ChildSizingPanel ChildSizing;

        public TRadioGroup()
        {
            SetStyle(ControlStyles.UserMouse | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            ChildSizing = new ChildSizingPanel();
            FItems = new TRadioGroupStringList(this);
            FAutoFill = true;
            FItemIndex = -1;
            FLastClickedItemIndex = -1;
            FButtonList = new List<RadioButton>();
            FColumns = 1;
            FColumnLayout = TColumnLayout.clHorizontalThenVertical;
            ChildSizing.PanelLayout = PanelLayout.cclLeftToRightThenTopToBottom;
            ChildSizing.ControlsPerLine = FColumns;
            //ChildSizing.ShrinkHorizontal = crsScaleChilds;
            //ChildSizing.ShrinkVertical = crsScaleChilds;
            //ChildSizing.EnlargeHorizontal = crsHomogenousChildResize;
            //ChildSizing.EnlargeVertical = crsHomogenousChildResize;
            ChildSizing.LeftRightSpacing = 6;
            ChildSizing.TopBottomSpacing = 0;
            this.Controls.Add(this.ChildSizing);
        }

        public TRadioGroup(IContainer cont)
            : this()
        {
            cont.Add(this);
        }

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            initializing = true;
            FItems.BeginUpdate();
        }

        void ISupportInitialize.EndInit()
        {
            initializing = false;
            FItems.EndUpdate();
            if (FItemIndex < -1 || FItemIndex >= FItems.Count) FItemIndex = -1;
            FLastClickedItemIndex = FItemIndex;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (FItems != null)
                {
                    FUpdatingItems = true; // disable UpdateItems()
                    FItems.BeginUpdate();
                    try
                    {
                        FItems.Clear();
                        FItems = null;
                        FButtonList.Clear();
                        FButtonList = null;
                        FHiddenButton = null;
                    }
                    finally
                    {
                        FUpdatingItems = false;
                    }
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            Action RealizeItemIndex = () =>
            {

                int i;
                if (FItemIndex != -1 && FItemIndex < FButtonList.Count)
                    FButtonList[FItemIndex].Checked = true;
                else if (FHiddenButton != null)
                    FHiddenButton.Checked = true;
                for (i = 0; i < FItems.Count; i++)
                {
                    FButtonList[i].Checked = FItemIndex == i;
                }
            };

            if (FCreatingWnd) throw new Exception("TCustomRadioGroup.InitializeWnd");
            FCreatingWnd = true;
            //DebugLn(['[TCustomRadioGroup.InitializeWnd] A ',DbgSName(Self),' FItems.Count=',FItems.Count,' HandleAllocated=',HandleAllocated,' ItemIndex=',ItemIndex]);
            UpdateItems();
            base.OnHandleCreated(e);
            RealizeItemIndex();
            //debugln(['TCustomRadioGroup.InitializeWnd END']);
            FCreatingWnd = false;
        }

        private void Changed(object Sender, EventArgs e)
        {
            CheckItemIndexChanged();
        }

        private void Clicked(object Sender, EventArgs e)
        {
            if (FIgnoreClicks) return;
            CheckItemIndexChanged();
        }

        private void ItemEnter(object Sender, EventArgs e)
        {
            OnEnter(e);
        }

        private void ItemExit(object Sender, EventArgs e)
        {
            OnLeave(e);
        }

        private void ItemKeyDown(object Sender, KeyEventArgs e)
        {
            Action<int, int> MoveSelection = (HorzDiff, VertDiff) =>
            {
                int Count;
                int StepSize;
                int BlockSize;
                int NewIndex;
                int WrapOffset;

                Count = FButtonList.Count;
                if (FColumnLayout == TColumnLayout.clHorizontalThenVertical)
                {
                    //add a row for ease wrapping
                    BlockSize = Columns * (Rows + 1);
                    StepSize = HorzDiff + VertDiff * Columns;
                    WrapOffset = VertDiff;
                }
                else
                {
                    //add a column for ease wrapping
                    BlockSize = (Columns + 1) * Rows;
                    StepSize = HorzDiff * Rows + VertDiff;
                    WrapOffset = HorzDiff;
                }
                NewIndex = ItemIndex + StepSize;
                if (NewIndex >= Count || NewIndex < 0)
                {
                    NewIndex = (NewIndex + WrapOffset + BlockSize) % BlockSize;
                    // Keep moving in the same direction until in valid range
                    while (NewIndex >= Count)
                        NewIndex = (NewIndex + StepSize) % BlockSize;
                }
                ItemIndex = NewIndex;
                FButtonList[ItemIndex].Focus();
                e.Handled = true;
            };
            if (!e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left: MoveSelection(-1, 0); break;
                    case Keys.Right: MoveSelection(1, 0); break;
                    case Keys.Up: MoveSelection(0, -1); break;
                    case Keys.Down: MoveSelection(0, 1); break;
                }
            }
            //if (e.KeyValue != 0)
            if (!e.Handled)
                OnKeyDown(e);
        }

        private void ItemKeyUp(object Sender, KeyEventArgs e)
        {
            //if (e.KeyValue != 0)
            OnKeyUp(e);
        }

        private void ItemKeyPress(object Sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar != 0x0)
            OnKeyPress(e);
        }

        private void ItemResize(object Sender, EventArgs e)
        {
        }

        private void SetAutoFill(Boolean AValue)
        {
            if (FAutoFill == AValue) return;
            FAutoFill = AValue;
            SuspendLayout();
            try
            {
                //if (FAutoFill)
                //{
                //    ChildSizing.EnlargeHorizontal = crsHomogenousChildResize;
                //    ChildSizing.EnlargeVertical = crsHomogenousChildResize;
                //}
                //else
                //{
                //    ChildSizing.EnlargeHorizontal = crsAnchorAligning;
                //    ChildSizing.EnlargeVertical = crsAnchorAligning;
                //}
            }
            finally
            {
                ResumeLayout();
            }
        }

        private void SetColumnLayout(TColumnLayout AValue)
        {
            if (FColumnLayout == AValue) return;
            FColumnLayout = AValue;
            if (FColumnLayout == TColumnLayout.clHorizontalThenVertical)
                ChildSizing.PanelLayout = PanelLayout.cclLeftToRightThenTopToBottom;
            else
                ChildSizing.PanelLayout = PanelLayout.cclTopToBottomThenLeftToRight;
            UpdateControlsPerLine();
        }

        private void UpdateControlsPerLine()
        {
            int NewControlsPerLine;
            if (ChildSizing.PanelLayout == PanelLayout.cclLeftToRightThenTopToBottom)
                NewControlsPerLine = Math.Max(1, FColumns);
            else
                NewControlsPerLine = Math.Max(1, Rows);
            ChildSizing.ControlsPerLine = NewControlsPerLine;
            //DebugLn('TCustomRadioGroup.UpdateControlsPerLine ',dbgs(ChildSizing.ControlsPerLine),' ',dbgs(NewControlsPerLine),' FColumns=',dbgs(FColumns),' FItems.Count=',dbgs(FItems.Count),' ',dbgs(ChildSizing.Layout=cclLeftToRightThenTopToBottom));
        }

        private void UpdateItems()
        {
            int i;
            RadioButton ARadioButton;
            if (FUpdatingItems) return;
            FUpdatingItems = true;
            try
            {
                // destroy radiobuttons, if there are too many
                while (FButtonList.Count > FItems.Count)
                {
                    FButtonList[FButtonList.Count - 1].Dispose();
                    FButtonList.RemoveAt(FButtonList.Count - 1);
                }

                // create as many TRadioButton as needed
                while (FButtonList.Count < FItems.Count)
                {
                    ARadioButton = new RadioButton
                    {
                        Parent = ChildSizing,
                        Name = "RadioButton" + (FButtonList.Count).ToString(),
                        AutoSize = true,
                        Font = null,
                        //BorderSpacing.CellAlignHorizontal = ccaLeftTop,
                        //BorderSpacing.CellAlignVertical = ccaCenter
                        //TODO: restrict selection in form designer
                        //ControlStyle = ControlStyle | csNoDesignSelectable
                    };
                    //events can't be set in initializers yet
                    ARadioButton.Click += this.Clicked;
                    ARadioButton.CheckedChanged += this.Changed;
                    ARadioButton.Enter += this.ItemEnter;
                    ARadioButton.Leave += this.ItemExit;
                    ARadioButton.KeyDown += this.ItemKeyDown;
                    ARadioButton.KeyUp += this.ItemKeyUp;
                    ARadioButton.KeyPress += this.ItemKeyPress;
                    ARadioButton.Resize += this.ItemResize;
                    FButtonList.Add(ARadioButton);
                }
                if (FHiddenButton == null)
                {
                    FHiddenButton = new RadioButton()
                    {
                        Name = "HiddenRadioButton",
                        Visible = false
                        //TODO: do not show in form designer
                        //ControlStyle = ControlStyle + [csNoDesignSelectable, csNoDesignVisible]
                    };
                }

                if (FItemIndex >= FItems.Count && !initializing) FItemIndex = FItems.Count - 1;

                if (FItems.Count > 0)
                {
                    // to reduce overhead do it in several steps

                    bool makeNamesUnique = !string.IsNullOrEmpty(this.Name);
                    if (makeNamesUnique)
                        FHiddenButton.Name = this.Name + "_HiddenRadioButton";

                    // assign Caption and then Parent
                    for (i = 0; i < FItems.Count; i++)
                    {
                        ARadioButton = FButtonList[i];
                        if (makeNamesUnique)
                            ARadioButton.Name = this.Name + "_RadioButton" + (i).ToString();
                        ARadioButton.Text = FItems[i];
                        ARadioButton.Parent = ChildSizing;
                    }
                    FHiddenButton.Parent = ChildSizing;
                    if (IsHandleCreated)
                    {
                        IntPtr hwnd = FHiddenButton.Handle;
                    }

                    // the checked and unchecked states can be applied only after all other
                    for (i = 0; i < FItems.Count; i++)
                    {
                        ARadioButton = FButtonList[i];
                        ARadioButton.Checked = (i == FItemIndex);
                        ARadioButton.Visible = true;
                    }
                    //FHiddenButton must remain the last item in Controls[], so that Controls[] is in sync with Items[]
                    ChildSizing.Controls.Remove(FHiddenButton);
                    ChildSizing.Controls.Add(FHiddenButton);
                    FHiddenButton.Checked = (FItemIndex == -1);
                    UpdateTabStops();
                }
            }
            finally
            {
                FUpdatingItems = false;
            }
        }

        private void UpdateTabStops()
        {
            int i;
            RadioButton RadioBtn;
            for (i = 0; i < FButtonList.Count; i++)
            {
                RadioBtn = FButtonList[i];
                RadioBtn.TabStop = RadioBtn.Checked;
            }
        }

        protected internal void UpdateInternalObjectList()
        {
            UpdateItems();
        }

        protected internal void UpdateAll()
        {
            UpdateItems();
            UpdateControlsPerLine();
            Invalidate();
        }

        protected internal virtual void UpdateRadioButtonStates()
        {
            FItemIndex = -1;
            //FHiddenButton.Checked;
            for (int i = 0; i < FButtonList.Count; i++)
                if (FButtonList[i].Checked) { FItemIndex = i; break; }
            UpdateTabStops();
        }

        protected internal void SetItems(TStringList Value)
        {
            if (Value != FItems)
            {
                FItems.Assign(Value);
                UpdateItems();
                UpdateControlsPerLine();
            }
        }

        protected internal void SetColumns(int Value)
        {
            if (Value != FColumns)
            {
                if (Value < 1)
                    throw new Exception("TCustomRadioGroup: Columns must be >= 1");
                FColumns = Value;
                UpdateControlsPerLine();
            }
        }

        protected internal void SetItemIndex(int Value)
        {
            int OldItemIndex;
            Boolean OldIgnoreClicks;
            //DebugLn('TCustomRadioGroup.SetItemIndex ',dbgsName(Self),' Old=',dbgs(FItemIndex),' New=',dbgs(Value));
            if (Value == FItemIndex) return;
            // needed later if handle isn't allocated
            OldItemIndex = FItemIndex;
            if (initializing)
                FItemIndex = Value;
            else
            {
                if (Value < -1 || Value >= FItems.Count)
                    throw new Exception(string.Format(rsIndexOutOfBounds, this.GetType().Name, Value, FItems.Count - 1));

                if (IsHandleCreated)
                {
                    // the radiobuttons are grouped by the widget interface
                    // and some does not allow to uncheck all buttons in a group
                    // Therefore there is a hidden button
                    FItemIndex = Value;
                    OldIgnoreClicks = FIgnoreClicks;
                    FIgnoreClicks = true;
                    try
                    {
                        if (FItemIndex != -1)
                            FButtonList[FItemIndex].Checked = true;
                        else
                            FHiddenButton.Checked = true;
                        // uncheck old radiobutton
                        if (OldItemIndex != -1)
                        {
                            if (OldItemIndex >= 0 && OldItemIndex < FButtonList.Count)
                                FButtonList[OldItemIndex].Checked = false;
                        }
                        else
                            FHiddenButton.Checked = false;
                    }
                    finally
                    {
                        FIgnoreClicks = OldIgnoreClicks;
                    }
                    // this has automatically unset the old button. But they do not recognize
                    // it. Update the states.
                    CheckItemIndexChanged();
                    UpdateTabStops();

                    Invalidate();
                }
                else
                {
                    FItemIndex = Value;
                    // maybe handle was recreated. issue #26714
                    FLastClickedItemIndex = -1;

                    // trigger event to be delphi compat, even if handle isn't allocated.
                    // issue #15989
                    if (Value != OldItemIndex && !FCreatingWnd)
                    {
                        OnClick(EventArgs.Empty);
                        OnSelectionChanged(EventArgs.Empty);
                        FLastClickedItemIndex = FItemIndex;
                    }
                }
            }
            //DebugLn('TCustomRadioGroup.SetItemIndex ',dbgsName(Self),' END Old=',dbgs(FItemIndex),' New=',dbgs(Value));
        }

        protected internal int GetItemIndex()
        {
            return FItemIndex;
        }

        protected internal virtual void CheckItemIndexChanged()
        {
            if (FCreatingWnd || FUpdatingItems)
                return;
            if (initializing || Disposing) return;
            UpdateRadioButtonStates();
            if (DesignMode) return;
            if (FLastClickedItemIndex == FItemIndex) return;
            FLastClickedItemIndex = FItemIndex;
            // for Delphi compatibility: OnClick should be invoked, whenever ItemIndex
            // has changed
            OnClick(EventArgs.Empty);
            // And a better named LCL equivalent
            OnSelectionChanged(EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EventSelectionChanged];
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler SelectionChanged { add { Events.AddHandler(EventSelectionChanged, value); } remove { Events.RemoveHandler(EventSelectionChanged, value); } }
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler Click { add { base.Click += value; } remove { base.Click -= value; } }

        [Category("Layout")]
        [DefaultValue(true)]
        public bool AutoFill { get { return FAutoFill; } set { SetAutoFill(value); } }
        [DefaultValue(-1)]
        public int ItemIndex { get { return GetItemIndex(); } set { SetItemIndex(value); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Category("Data")]
        public TStringList Items { get { return FItems; } set { SetItems(value); } }
        [DefaultValue(1)]
        [Category("Layout")]
        [RefreshProperties(RefreshProperties.All)]
        public int Columns { get { return FColumns; } set { SetColumns(value); } }
        [DefaultValue(TColumnLayout.clHorizontalThenVertical)]
        [Category("Layout")]
        public TColumnLayout ColumnLayout { get { return FColumnLayout; } set { SetColumnLayout(value); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ControlCollection Controls { get { return base.Controls; } }
        [Category("Layout")]
        public int Rows
        {
            get
            {
                if (FItems.Count > 0)
                    return ((FItems.Count - 1) / Columns) + 1;
                else
                    return 0;
            }
        }
    }
}
