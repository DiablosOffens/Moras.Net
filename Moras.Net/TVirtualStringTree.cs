using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightIdeasSoftware;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.Collections;
using DelphiClasses;
using System.Drawing;

namespace Moras.Net
{
    public class GetCellTextEventArgs : EventArgs
    {
        private object model;
        private int columnIndex;
        private string cellText;

        public GetCellTextEventArgs(object model, int columnIndex)
        {
            this.model = model;
            this.columnIndex = columnIndex;
        }

        public object Model { get { return model; } }
        public int ColumnIndex { get { return columnIndex; } }
        public string CellText { get { return cellText; } set { cellText = value; } }
    }

    public class CompareModelsEventArgs : EventArgs
    {
        private object left;
        private object right;
        private int columnIndex;

        public CompareModelsEventArgs(object left, object right, int columnIndex)
        {
            this.left = left;
            this.right = right;
            this.columnIndex = columnIndex;
        }

        public object LeftModel { get { return left; } }
        public object RightModel { get { return right; } }
        public int ColumnIndex { get { return columnIndex; } }
        public int Result { get; set; }
    }

    public class GetNodeModelTypeEventArgs : EventArgs
    {
        public Type ModelType { get; set; }
    }

    //HACK: realy hacky event args class, but it's the best managed way to capture type-safe ref parameters for the event.
    public class InitModelEventArgs : EventArgs
    {
        public delegate void InitModelValueDelegate<T>(ref T model) where T : struct;
        public delegate void InitModelObjectDelegate<T>(ref T model) where T : class;

        private object parentModel;
        private object refmodel;
        private int index;
        private bool modelIsValueRef;

        public object ParentModel { get { return parentModel; } }
        public int Index { get { return index; } }

        private InitModelEventArgs(object parentModel, object refmodel, int index, bool modelIsValueRef = false)
        {
            this.parentModel = parentModel;
            this.refmodel = refmodel;
            this.index = index;
            this.modelIsValueRef = modelIsValueRef;
        }

        internal static InitModelEventArgs CreateForModelValue<T>(T? parentModel, ValueRef<T> refmodel, int index) where T : struct
        {
            return new InitModelEventArgs(parentModel, refmodel, index, true);
        }

        internal static InitModelEventArgs CreateForModelObject<T>(T parentModel, T refmodel, int index) where T : class
        {
            return new InitModelEventArgs(parentModel, refmodel, index);
        }

        internal T GetModelObject<T>() where T : class
        {
            if (modelIsValueRef)
                throw new InvalidCastException();
            return (T)refmodel;
        }

        public void InitModelValue<T>(InitModelValueDelegate<T> initializer) where T : struct
        {
            if (!modelIsValueRef || refmodel == null)
                throw new InvalidCastException();
            ValueRef<T> modelref = (ValueRef<T>)refmodel;
            initializer(ref modelref.Value);
        }

        public void InitModelObject<T>(InitModelObjectDelegate<T> initializer) where T : class
        {
            if (modelIsValueRef)
                throw new InvalidCastException();
            T modelref = (T)refmodel;
            initializer(ref modelref);
            refmodel = modelref;
        }
    }

    public class BackColorHighlightTextRenderer : HighlightTextRenderer
    {
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color BackColor { get; set; }

        public override Color GetBackgroundColor()
        {
            if (!ListView.Enabled || (this.IsItemSelected && !this.ListView.UseTranslucentSelection && this.ListView.FullRowSelect)
                || BackColor.IsEmpty)
                return base.GetBackgroundColor();
            return BackColor;
        }

        public override Color GetSelectedBackgroundColor()
        {
            if (ListView.Focused || !ListView.HideSelection || BackColor.IsEmpty)
                return base.GetSelectedBackgroundColor();
            return BackColor;
        }
    }

    public class TVirtualStringTree : TreeListView
    {
        private class OLVColumnCollection : ColumnHeaderCollection
        {
            private TVirtualStringTree owner;

            public OLVColumnCollection(TVirtualStringTree owner)
                : base(owner)
            {
                this.owner = owner;
            }

            public override int Add(ColumnHeader value)
            {
                int result = base.Add(value);
                SetAutoAspectGetter((OLVColumn)value);
                return result;
            }

            public override void AddRange(ColumnHeader[] values)
            {
                base.AddRange(values);
                foreach (OLVColumn column in values)
                {
                    SetAutoAspectGetter(column);
                }
            }

            private void SetAutoAspectGetter(OLVColumn column)
            {
                if (/*!column.AspectGetterAutoGenerated &&*/ column.AspectGetter == null)
                {
                    //column.AspectGetterAutoGenerated = true;
                    int colIndex = column.Index;
                    column.AspectGetter = ro => owner.RaiseGetCellText(ro, colIndex);
                }
            }
        }

        private class CompareModelEventComparer : IComparer
        {
            private TVirtualStringTree treeView;
            private OLVColumn column;
            private SortOrder sortOrder;
            private CompareModelEventComparer secondComparer;

            public CompareModelEventComparer(TVirtualStringTree treeView, OLVColumn column, SortOrder sortOrder)
            {
                this.treeView = treeView;
                this.column = column;
                this.sortOrder = sortOrder;
            }

            public CompareModelEventComparer(TVirtualStringTree treeView, OLVColumn col, SortOrder order, OLVColumn col2, SortOrder order2)
                : this(treeView, col, order)
            {
                if (((col != col2) && (col2 != null)) && (order2 != SortOrder.None))
                {
                    this.secondComparer = new CompareModelEventComparer(treeView, col2, order2);
                }
            }


            #region IComparer Members

            public int Compare(object x, object y)
            {
                int result = 0;

                if (this.sortOrder == SortOrder.None)
                    return 0;

                // Handle nulls. Null values come last
                bool xIsNull = (x == null || x == System.DBNull.Value);
                bool yIsNull = (y == null || y == System.DBNull.Value);
                if (xIsNull || yIsNull)
                {
                    if (xIsNull && yIsNull)
                        result = 0;
                    else
                        result = (xIsNull ? -1 : 1);
                }
                else
                {
                    result = this.treeView.RaiseCompareModels(x, y, this.column.Index);
                }

                if (this.sortOrder == SortOrder.Descending)
                    result = -result;

                // If the result was equality, use the secondary comparer to resolve it
                if (result == 0 && this.secondComparer != null)
                    result = this.secondComparer.Compare(x, y);

                return result;
            }

            #endregion
        }

        private class VirtualTree : Tree
        {
            private static readonly MethodInfo createModelValueMethod = typeof(VirtualTree).GetMethod("CreateModelValue", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly MethodInfo createModelObjectMethod = typeof(VirtualTree).GetMethod("CreateModelObject", BindingFlags.Instance | BindingFlags.NonPublic);
            private delegate object CreateModelDelegate(object parentModel, int index);
            private int firstIndex;
            private Type modelType;
            private CreateModelDelegate createModel;
            private OLVColumn lastSortColumn;
            private SortOrder lastSortOrder;

            public VirtualTree(TreeListView view)
                : base(view)
            {
            }

            protected override BranchComparer GetBranchComparer()
            {
                if (((TVirtualStringTree)TreeView).HasCompareModelsHandler)
                {
                    if (this.lastSortColumn == null)
                        return null;

                    return new BranchComparer(new CompareModelEventComparer((TVirtualStringTree)TreeView,
                        this.lastSortColumn,
                        this.lastSortOrder,
                        this.TreeView.SecondarySortColumn ?? this.TreeView.GetColumn(0),
                        this.TreeView.SecondarySortColumn == null ? this.lastSortOrder : this.TreeView.SecondarySortOrder));
                }
                return base.GetBranchComparer();
            }

            public override object GetNthObject(int n)
            {
                if (n >= firstIndex && n < firstIndex + base.GetObjectCount())
                    return base.GetNthObject(n - firstIndex);
                EnsureCreateRootModelsDelegate();
                return createModel(null, n);
            }

            public override int GetObjectCount()
            {
                return base.GetObjectCount();
            }

            public override int GetObjectIndex(object model)
            {
                return base.GetObjectIndex(model) + firstIndex;
            }

            public override void PrepareCache(int first, int last)
            {
                int count = base.GetObjectCount();
                if (count != 0 && first >= firstIndex && last < firstIndex + count)
                    return;

                firstIndex = first;
                int newcount = last - first + 1;
                RootObjects = CreateRootObjects(newcount);
            }

            public override void Sort(OLVColumn column, SortOrder order)
            {
                firstIndex = 0;
                RootObjects = CreateRootObjects(TreeView.GetItemCount());
                this.lastSortColumn = column;
                this.lastSortOrder = order;
                base.Sort(column, order);
            }

            internal void ClearCache()
            {
                firstIndex = 0;
                RootObjects = new ArrayList();
            }

            private object CreateModelValue<T>(object parentModel, int index) where T : struct
            {
                ValueRef<T> refmodel = new ValueRef<T>();
                ((TVirtualStringTree)TreeView).RaiseInitModelValue((T?)parentModel, refmodel, index);
                return refmodel.Value;
            }

            private object CreateModelObject<T>(object parentModel, int index) where T : class
            {
                T refmodel = null;
                ((TVirtualStringTree)TreeView).RaiseInitModelObject((T)parentModel, ref refmodel, index);
                return refmodel;
            }

            private void EnsureCreateRootModelsDelegate()
            {
                if (modelType == null)
                {
                    modelType = ((TVirtualStringTree)TreeView).RaiseGetNodeModelType();
                    if (modelType == null)
                        return;
                    if (modelType.IsValueType)
                        createModel = (CreateModelDelegate)Delegate.CreateDelegate(typeof(CreateModelDelegate), this,
                            createModelValueMethod.MakeGenericMethod(modelType));
                    else
                        createModel = (CreateModelDelegate)Delegate.CreateDelegate(typeof(CreateModelDelegate), this,
                            createModelObjectMethod.MakeGenericMethod(modelType));
                }
            }

            private IEnumerable CreateRootObjects(int count)
            {
                EnsureCreateRootModelsDelegate();
                for (int i = firstIndex; i < firstIndex + count; i++)
                {
                    //TODO: implement child getter for each model+index
                    //ChildrenGetter
                    yield return createModel != null ? createModel(null, i) : i;
                }
            }
        }

        static readonly FieldInfo fiColumnHeaderCollection = typeof(ListView).GetField("columnHeaderCollection", BindingFlags.NonPublic | BindingFlags.Instance);
        private OLVColumn autoSizeColumn;
        private string defaultCellText = "Node";
        private bool showAutomaticTreeRenderer;

        public TVirtualStringTree()
        {
            if (fiColumnHeaderCollection != null)
                fiColumnHeaderCollection.SetValue(this, new OLVColumnCollection(this));
        }

        [Category("Action")]
        public event EventHandler<ColumnClickEventArgs> BeforeHandleColumnClick;

        [Category("Behavior")]
        public event EventHandler<CompareModelsEventArgs> CompareModels;

        [Category("Behavior")]
        public event EventHandler<GetCellTextEventArgs> GetCellText;

        [Category("Data")]
        public event EventHandler<GetNodeModelTypeEventArgs> GetNodeModelType;

        [Category("Data")]
        public event EventHandler<InitModelEventArgs> InitModel;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Behavior")]
        [Description("Which column did we last sort by")]
        [TypeConverter(typeof(OLVColumnConverter))]
        [DefaultValue(null)]
        public OLVColumn AutoSizeColumn
        {
            get { return autoSizeColumn; }
            set { autoSizeColumn = value; }
        }

        [DefaultValue("Node")]
        [Category("Appearance")]
        [Description("Text to show if there's no GetCellText event handler (e.g. at design time).")]
        public string DefaultCellText
        {
            get { return defaultCellText; }
            set
            {
                if (defaultCellText != value)
                {
                    defaultCellText = value;
                    if (!DesignMode)
                        Invalidate();
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Behavior")]
        [Description("Which column did we last sort by")]
        [TypeConverter(typeof(OLVColumnConverter))]
        [DefaultValue(null)]
        public override OLVColumn PrimarySortColumn
        {
            get
            {
                return base.PrimarySortColumn;
            }
            set
            {
                base.PrimarySortColumn = value;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Behavior")]
        [Description("Which direction did we last sort")]
        [DefaultValue(SortOrder.None)]
        public override SortOrder PrimarySortOrder
        {
            get
            {
                return base.PrimarySortOrder;
            }
            set
            {
                base.PrimarySortOrder = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ObjectsCount
        {
            get { return base.VirtualListSize; }
            set { SetVirtualListSize(value); }
        }

        [Category("Appearance")]
        [Description("Decides wether current TreeRenderer is automaticaly shown on first column")]
        [DefaultValue(false)]
        public bool ShowAutomaticTreeRenderer
        {
            get { return showAutomaticTreeRenderer; }
            set { showAutomaticTreeRenderer = value; }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [RefreshProperties(RefreshProperties.Repaint)]
        protected override int VirtualListSize
        {
            get
            {
                return base.VirtualListSize;
            }
            set
            {
                int cachecount = TreeModel.GetObjectCount();
                base.VirtualListSize = value;
                if (cachecount != value)
                    Sort();
            }
        }

        public bool ShouldSerializeVirtualListSize()
        {
            return false;
        }

        private int RaiseCompareModels(object left, object right, int columnIndex)
        {
            CompareModelsEventArgs args = new CompareModelsEventArgs(left, right, columnIndex);
            OnCompareModels(args);
            return args.Result;
        }

        private string RaiseGetCellText(object rowObject, int columnIndex)
        {
            GetCellTextEventArgs args = new GetCellTextEventArgs(rowObject, columnIndex);
            args.CellText = defaultCellText;
            OnGetCellText(args);
            return args.CellText;
        }

        private Type RaiseGetNodeModelType()
        {
            GetNodeModelTypeEventArgs args = new GetNodeModelTypeEventArgs();
            OnGetNodeModelType(args);
            return args.ModelType;
        }

        private void RaiseInitModelValue<T>(T? parentModel, ValueRef<T> refmodel, int index) where T : struct
        {
            OnInitModel(InitModelEventArgs.CreateForModelValue(parentModel, refmodel, index));
        }

        private void RaiseInitModelObject<T>(T parentModel, ref T refmodel, int index) where T : class
        {
            InitModelEventArgs args = InitModelEventArgs.CreateForModelObject(parentModel, refmodel, index);
            OnInitModel(args);
            refmodel = args.GetModelObject<T>();
        }

        protected virtual void OnBeforeHandleColumnClick(ColumnClickEventArgs args)
        {
            if (BeforeHandleColumnClick != null)
                BeforeHandleColumnClick(this, args);
        }

        private bool HasCompareModelsHandler { get { return CompareModels != null; } }

        protected virtual void OnCompareModels(CompareModelsEventArgs args)
        {
            if (CompareModels != null)
                CompareModels(this, args);
        }

        protected virtual void OnGetCellText(GetCellTextEventArgs args)
        {
            if (GetCellText != null)
                GetCellText(this, args);
        }

        protected virtual void OnGetNodeModelType(GetNodeModelTypeEventArgs args)
        {
            if (GetNodeModelType != null)
                GetNodeModelType(this, args);
        }

        protected virtual void OnInitModel(InitModelEventArgs args)
        {
            if (InitModel != null)
                InitModel(this, args);
        }

        protected override void OnItemsChanged(ItemsChangedEventArgs e)
        {
            this.AutoSizeColumns();
            base.OnItemsChanged(e);
        }

        protected override void HandleColumnClick(object sender, ColumnClickEventArgs e)
        {
            OnBeforeHandleColumnClick(e);
            base.HandleColumnClick(sender, e);
        }

        protected override void RegenerateTree()
        {
            if (TreeFactory == null) TreeFactory = view => new VirtualTree(view);
            base.RegenerateTree();
        }

        protected override void EnsureTreeRendererPresent(TreeListView.TreeRenderer renderer)
        {
            if (showAutomaticTreeRenderer)
                base.EnsureTreeRendererPresent(renderer);
        }

        protected override void ShowSortIndicator(OLVColumn columnToSort, SortOrder sortOrder)
        {
            base.ShowSortIndicator(columnToSort, sortOrder);
        }

        public override void AutoSizeColumns()
        {
            if (AutoSizeColumn != null && IsHandleCreated)
                // If handle is created, setting Width to -1 will be translated to LVSCW_AUTOSIZE and
                // actualy does the auto sizing on column contents. But Width also returns only the actual resulting size in this case,
                // so AutoSizeColumns() don't accidentally resize it again. If handle was not created, there will be no change either.
                AutoSizeColumn.Width = -1;
            base.AutoSizeColumns();
        }

        public override void ClearCachedInfo()
        {
            base.ClearCachedInfo();
        }

        /// <summary>
        /// Remove all items from this list
        /// </summary>
        /// <remark>This method can safely be called from background threads.</remark>
        public override void ClearObjects()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(this.ClearObjects));
            else
            {
                if (TreeModel != null)
                    ((VirtualTree)TreeModel).ClearCache();
                SetVirtualListSize(0);
                this.DiscardAllState();
            }
        }

        public override void UpdateVirtualListSize()
        {
            if (DesignMode)
                SetVirtualListSize(20);
            //TreeModel is only the cache, actual size is always VirtualListSize
            //base.UpdateVirtualListSize();
        }
    }

    public class OLVColumnConverter : ComponentConverter
    {
        public OLVColumnConverter()
            : base(typeof(OLVColumn))
        { }

        // Methods
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(InstanceDescriptor)) || !(value is OLVColumn))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            OLVColumn header = (OLVColumn)value;
            Type reflectionType = TypeDescriptor.GetReflectionType(value);
            ConstructorInfo constructor;
            if (!string.IsNullOrEmpty(header.Text) || !string.IsNullOrEmpty(header.AspectName))
            {
                constructor = reflectionType.GetConstructor(new Type[] { typeof(string), typeof(string) });
                if (constructor != null)
                    return new InstanceDescriptor(constructor, new object[] { header.Text, header.AspectName }, false);
            }
            constructor = reflectionType.GetConstructor(new Type[0]);
            if (constructor != null)
                return new InstanceDescriptor(constructor, new object[0], false);
            throw new ArgumentException(string.Format("The type {0} has no default constructor.", reflectionType.FullName));
        }
    }
}
