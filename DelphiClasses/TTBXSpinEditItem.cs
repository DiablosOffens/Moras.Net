using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.MenuStrip)]
    public class TTBXSpinEditItem : ToolStripControlHost
    {
        private static readonly object EventValueChanged = new object();
        public TTBXSpinEditItem()
            : base(new NumericUpDown())
        {
        }

        [Category("Property Changed")]
        public event EventHandler ValueChanged
        {
            add
            {
                base.Events.AddHandler(EventValueChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValueChanged, value);
            }
        }

        protected NumericUpDown UpDownControl { get { return (NumericUpDown)Control; } }

        [DefaultValue(false), Category("Behavior")]
        public bool ReadOnly { get { return UpDownControl.ReadOnly; } set { UpDownControl.ReadOnly = value; } }

        [Browsable(false)]
        public override string Text { get { return base.Text; } set { base.Text = value; } }

        [DefaultValue(typeof(decimal), "1"), Category("Data")]
        public decimal Increment { get { return UpDownControl.Increment; } set { UpDownControl.Increment = value; } }

        [DefaultValue(typeof(decimal), "100"), Category("Data"), RefreshProperties(RefreshProperties.All)]
        public decimal Maximum { get { return UpDownControl.Maximum; } set { UpDownControl.Maximum = value; } }

        [DefaultValue(typeof(decimal), "0"), Category("Data"), RefreshProperties(RefreshProperties.All)]
        public decimal Minimum { get { return UpDownControl.Minimum; } set { UpDownControl.Minimum = value; } }

        [DefaultValue(typeof(decimal), "0"), Category("Appearance"), Bindable(true)]
        public decimal Value { get { return UpDownControl.Value; } set { UpDownControl.Value = value; } }

        private void HandleValueChanged(object sender, EventArgs e)
        {
            this.OnValueChanged(e);
        }

        internal void RaiseEvent(object key, EventArgs e)
        {
            EventHandler handler = (EventHandler)base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            RaiseEvent(EventValueChanged, e);
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);

            if (control != null)
            {
                ((NumericUpDown)control).ValueChanged += HandleValueChanged;
            }
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);

            if (control != null)
            {
                ((NumericUpDown)control).ValueChanged -= HandleValueChanged;
            }
        }
    }
}
