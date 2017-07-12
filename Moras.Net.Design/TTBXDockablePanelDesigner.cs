using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using DelphiClasses;

namespace Moras.Net.Design
{
    public class TTBXDockablePanelDesigner : ParentControlDesigner
    {
        Panel panel;

        public override bool CanBeParentedTo(IDesigner parentDesigner)
        {
            return base.CanBeParentedTo(parentDesigner);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            TTBXDockablePanel ctl = component as TTBXDockablePanel;
            if (ctl != null)
            {
                panel = ctl.ContentPanel;
            }
            else
            {
                var desc = TypeDescriptor.GetProperties(component)["ContentPanel"];
                if (desc != null)
                {
                    panel = desc.GetValue(component) as Panel;
                }
            }
            EnableDesignMode(panel, "ContentPanel");
        }

        //public override ICollection AssociatedComponents
        //{
        //    get
        //    {
        //        ArrayList list = new ArrayList();
        //        foreach (Control ctl in panel.Controls)
        //        {
        //            list.Add(ctl);
        //        }
        //        return list;
        //    }
        //}
    }
}
