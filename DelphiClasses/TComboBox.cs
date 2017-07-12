using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    public class TComboBox : ComboBox
    {
        private static readonly object EventChange = new object();

        [Category("Behavior"), Description("Occurs when the user changes the text displayed in the edit region or selects an item from the list.")]
        public event EventHandler Change { add { base.Events.AddHandler(EventChange, value); } remove { base.Events.RemoveHandler(EventChange, value); } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(-1)]
        public override int SelectedIndex
        {
            get
            {
                return base.SelectedIndex;
            }
            set
            {
                base.SelectedIndex = value;
            }
        }

        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            base.OnSelectionChangeCommitted(e);
            OnChange(e);
        }

        protected override void OnTextUpdate(EventArgs e)
        {
            base.OnTextUpdate(e);
            OnChange(e);
        }

        protected virtual void OnChange(EventArgs e)
        {
            EventHandler eh = Events[EventChange] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }
    }
}
