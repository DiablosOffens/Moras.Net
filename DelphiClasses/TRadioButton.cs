using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DelphiClasses
{
    public class TRadioButton : RadioButton
    {
        private bool inOnClick;
        protected override void OnClick(EventArgs e)
        {
            inOnClick = true;
            try
            {
                base.OnClick(e);
            }
            finally
            {
                inOnClick = false;
            }
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            if (!inOnClick && Checked)
                OnClick(e);
        }
    }
}
