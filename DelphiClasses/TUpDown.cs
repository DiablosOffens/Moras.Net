using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace DelphiClasses
{
    public class TUpDown : NumericUpDown
    {
        private const string rsIsAlreadyAssociatedWith = "{0} is already associated with {1}";
        private Control associate;

        [DefaultValue(true)]
        public bool ArrowKeys { get; set; }

        public Control Associate
        {
            get { return associate; }
            set
            {

                // check that no other updown component is associated to the new Associate
                if (value != associate && value != null)
                {
                    if (Parent != null && Parent.Controls != null)
                    {
                        for (int i = 0; i < Parent.Controls.Count; i++)
                        {
                            Control otherControl = Parent.Controls[i];
                            if (otherControl is TUpDown && otherControl != this)
                                if (((TUpDown)otherControl).Associate == value)
                                    throw new Exception(string.Format(rsIsAlreadyAssociatedWith, value.Name, otherControl.Name));
                        }
                    }
                }

                // disconnect old Associate
                if (associate != null)
                {
                    RemoveAllHandlersOfObject(associate);
                    associate = null;
                }

                // connect new Associate
                if (value != null && value.Parent == this.Parent &&
                    !(value is TUpDown)/* && !(value is TCustomTreeView) &&
                    !(value is TCustomListView)*/)
                {
                    associate = value;
                    UpdateUpDownPositionText();
                    UpdateAlignButtonPos();
                    //HACK: delphi adds all these handlers as first handler, but to keep it simple
                    //we just append them
                    associate.KeyDown += new KeyEventHandler(AssociateKeyDown);
                    associate.LocationChanged += new EventHandler(OnAssociateChangeBounds);
                    associate.SizeChanged += new EventHandler(OnAssociateChangeBounds);
                    associate.TextChanged += new EventHandler(AssociateTextChanged);
                }
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(15, 20);
            }
        }

        public override Size MaximumSize
        {
            get
            {
                return new Size(15, base.MaximumSize.Height);
            }
            set
            {
                base.MaximumSize = value;
            }
        }

        public TUpDown()
        {
            ArrowKeys = true;
        }

        private void OnAssociateChangeBounds(object sender, EventArgs e)
        {
            UpdateAlignButtonPos();
        }

        private void AssociateKeyDown(object sender, KeyEventArgs e)
        {
            if (ArrowKeys && e.Modifiers == Keys.None)
            {
                //TODO: implement other orientations, but needs complete rewrite of base class
                //switch (FOrientation)
                //{
                //    case udVertical:
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        UpButton();
                        break;
                    case Keys.Down:
                        DownButton();
                        break;
                }
                //    break;
                //case udHorizontal:
                //switch (e.KeyCode)
                //{
                //    case Keys.Left:
                //        DownButton();
                //    case Keys.Right:
                //        UpButton();
                //}
                //        break;
                //}
            }
        }

        void AssociateTextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(associate.Text))
                associate.Text = "0";
            else
                Value = decimal.Parse(associate.Text);
        }

        private void UpdateUpDownPositionText()
        {
            if (!DesignMode && associate != null)
            {
                if (ThousandsSeparator)
                    associate.Text = Value.ToString("N");
                else
                    associate.Text = Value.ToString("F0");
            }
        }

        public override void DownButton()
        {
            base.DownButton();
            UpdateUpDownPositionText();
        }

        public override void UpButton()
        {
            base.UpButton();
            UpdateUpDownPositionText();
        }

        private void UpdateAlignButtonPos()
        {
            int newWidth = 0;
            int newLeft = 0;
            int newHeight = 0;
            int newTop = 0;

            if (Associate != null)
            {
                if (UpDownAlign == LeftRightAlignment.Left || UpDownAlign == LeftRightAlignment.Right)
                {
                    newWidth = Width;
                    newHeight = Associate.Height;
                    if (UpDownAlign == LeftRightAlignment.Left)
                        newLeft = Associate.Left - newWidth;
                    else
                        newLeft = Associate.Left + Associate.Width;
                    newTop = Associate.Top;
                }
                //TODO: implement other orientations, but needs complete rewrite of base class
                //else
                //{
                //    NewWidth = Associate.Width;
                //    NewHeight = Height;
                //    NewLeft = Associate.Left;
                //    if (FAlignButton = udTop)
                //        NewTop = Associate.Top - NewHeight;
                //    else
                //        NewTop = Associate.Top + Associate.Height;
                //}
                SetBounds(newLeft, newTop, newWidth, newHeight);
            }
        }

        private void RemoveAllHandlersOfObject(Control ctl)
        {
            ctl.KeyDown -= new KeyEventHandler(AssociateKeyDown);
            ctl.LocationChanged -= new EventHandler(OnAssociateChangeBounds);
            ctl.SizeChanged -= new EventHandler(OnAssociateChangeBounds);
            ctl.TextChanged -= new EventHandler(AssociateTextChanged);
        }
    }
}
