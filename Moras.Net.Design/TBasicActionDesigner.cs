using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Collections;

namespace Moras.Net.Design
{
    public class TBasicActionDesigner : ComponentDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor)properties["Name"];
            if (oldPropertyDescriptor != null)
            {
                properties["Name"] = TypeDescriptor.CreateProperty(typeof(TBasicActionDesigner), oldPropertyDescriptor, new Attribute[0x0]);
            }
            //properties["Locked"] = TypeDescriptor.CreateProperty(typeof(TBasicActionDesigner), "Locked", typeof(bool), new Attribute[] { new DefaultValueAttribute(false), BrowsableAttribute.Yes, CategoryAttribute.Design, DesignOnlyAttribute.Yes, new SRDescriptionAttribute("lockedDescr") });
        }

        private string Name
        {
            get
            {
                return base.Component.Site.Name;
            }
            set
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service == null) || ((service != null) && !service.Loading))
                {
                    base.Component.Site.Name = value;
                }
            }
        }

    }
}
