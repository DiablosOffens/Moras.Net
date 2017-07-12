using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.ComponentModel;

namespace Moras.Net.Design
{
    public class IndyComponentDesigner : ComponentDesigner
    {
        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor)properties["Name"];
            if (oldPropertyDescriptor != null)
            {
                properties["Name"] = TypeDescriptor.CreateProperty(typeof(IndyComponentDesigner), oldPropertyDescriptor, BrowsableAttribute.No);
            }
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
