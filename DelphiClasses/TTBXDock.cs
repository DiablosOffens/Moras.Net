using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DelphiClasses
{
    public class TTBXDock : ToolStripPanel
    {

        protected override void OnControlAdded(ControlEventArgs e)
        {
            TTBXDockablePanel dockpanel = e.Control as TTBXDockablePanel;
            if (dockpanel != null)
                dockpanel.InsideControlAdded = true;
            try
            {
                base.OnControlAdded(e);
            }
            finally
            {
                if (dockpanel != null)
                    dockpanel.InsideControlAdded = false;
            }
        }
    }
}
