using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    //HINT: Doesn't work with current framework. There is a bug in ScrollableControl
    // which prevents full control over the scrollbars.
    //Maybe this bug was initially in the design of the class, so maybe there is never
    // a fix for it. Only way would be to completely roll our own class for this purpose.
    public class TScrollBox : Panel
    {
        public TScrollBox()
        {
        }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(NestedPropertiesTypeConverter<HScrollProperties>))]
        public new HScrollProperties HorizontalScroll
        {
            get { return base.HorizontalScroll; }
        }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(NestedPropertiesTypeConverter<VScrollProperties>))]
        public new VScrollProperties VerticalScroll
        {
            get { return base.VerticalScroll; }
        }
    }
}
