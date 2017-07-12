using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms.VisualStyles;

namespace DelphiClasses
{
    public class TSpeedButton : RadioButton
    {
        private static PropertyInfo piMouseIsDown = typeof(RadioButton).GetProperty("MouseIsDown", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo piMouseIsOver = typeof(RadioButton).GetProperty("MouseIsOver", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo piMouseIsPressed = typeof(RadioButton).GetProperty("MouseIsPressed", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly VisualStyleElement ButtonElement = VisualStyleElement.Button.PushButton.Normal;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private static void InitializeRenderer(int state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(ButtonElement.ClassName, ButtonElement.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(ButtonElement.ClassName, ButtonElement.Part, state);
            }
        }

        private int numImages;
        private bool allowAllUp;

        public TSpeedButton()
        {
            Appearance = Appearance.Button;
            numImages = 1;
        }

        [DefaultValue(false)]
        [Category("Behavior")]
        public bool AllowAllUp
        {
            get { return allowAllUp; }
            set { allowAllUp = value; }
        }

        public new Image Image
        {
            get { return base.Image; }
            set
            {
                if (base.Image != value)
                {
                    CheckImageSize(value, NumImages);
                    base.Image = value;
                }
            }
        }

        public new int ImageIndex
        {
            get { return base.ImageIndex; }
            set
            {
                if (base.ImageIndex != value)
                {
                    if (value >= 0 && ImageList != null)
                    {
                        int newindex = Math.Min(value, ImageList.Images.Count - 1);
                        if (newindex != -1)
                            CheckImageSize(ImageList.Images[newindex], NumImages);
                    }
                    base.ImageIndex = value;
                }
            }
        }

        public new string ImageKey
        {
            get { return base.ImageKey; }
            set
            {
                if (base.ImageKey != value)
                {
                    if (!string.IsNullOrEmpty(value) && ImageList != null &&
                        ImageList.Images.ContainsKey(value))
                    {
                        int index = ImageList.Images.IndexOfKey(value);
                        if (index != -1)
                            CheckImageSize(ImageList.Images[index], NumImages);
                    }
                    base.ImageKey = value;
                }
            }
        }

        [DefaultValue(1)]
        [Category("Appearance")]
        public int NumImages
        {
            get { return numImages; }
            set
            {
                if (numImages != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("value", "The value must be greater then zero.");
                    CheckImageSize(Image, value);
                    numImages = value;
                    Invalidate();
                }
            }
        }

        private void CheckImageSize(Image img, int count)
        {
            if (img != null && (img.Width % count) != 0)
                throw new ArgumentException("The width of the Image must be a multiple of NumImages.");
        }

        private void ResetImage()
        {
            Image = null;
        }

        private bool ShouldSerializeImage()
        {
            return ImageIndex == -1 && string.IsNullOrEmpty(ImageKey) &&
                Image != null;
        }

        private static Rectangle HAlign(Size alignThis, Rectangle withinThis, System.Drawing.ContentAlignment align)
        {
            if ((align &
                (System.Drawing.ContentAlignment.BottomRight |
                System.Drawing.ContentAlignment.MiddleRight |
                System.Drawing.ContentAlignment.TopRight)) != ((System.Drawing.ContentAlignment)0x0))
            {
                withinThis.X += withinThis.Width - alignThis.Width;
            }
            else if ((align &
                (System.Drawing.ContentAlignment.BottomCenter |
                System.Drawing.ContentAlignment.MiddleCenter |
                System.Drawing.ContentAlignment.TopCenter)) != ((System.Drawing.ContentAlignment)0x0))
            {
                withinThis.X += (withinThis.Width - alignThis.Width) / 0x2;
            }
            withinThis.Width = alignThis.Width;
            return withinThis;
        }

        private static Rectangle VAlign(Size alignThis, Rectangle withinThis, System.Drawing.ContentAlignment align)
        {
            if ((align & (System.Drawing.ContentAlignment.BottomRight | System.Drawing.ContentAlignment.BottomCenter | System.Drawing.ContentAlignment.BottomLeft)) != ((System.Drawing.ContentAlignment)0x0))
            {
                withinThis.Y += withinThis.Height - alignThis.Height;
            }
            else if ((align & (System.Drawing.ContentAlignment.MiddleRight | System.Drawing.ContentAlignment.MiddleCenter | System.Drawing.ContentAlignment.MiddleLeft)) != ((System.Drawing.ContentAlignment)0x0))
            {
                withinThis.Y += (withinThis.Height - alignThis.Height) / 0x2;
            }
            withinThis.Height = alignThis.Height;
            return withinThis;
        }

        private static Rectangle Align(Size alignThis, Rectangle withinThis, System.Drawing.ContentAlignment align)
        {
            return VAlign(alignThis, HAlign(alignThis, withinThis, align), align);
        }

        private PushButtonState DetermineState()
        {
            bool mouseisdown = false;
            if (piMouseIsDown != null)
                mouseisdown = (bool)piMouseIsDown.GetValue(this, null);
            if (mouseisdown || Checked)
            {
                return PushButtonState.Pressed;
            }

            bool mouseisover = false;
            if (piMouseIsOver != null)
                mouseisover = (bool)piMouseIsOver.GetValue(this, null);
            if (mouseisover)
            {
                return PushButtonState.Hot;
            }

            bool enabled = Enabled;
            if (DesignMode)
                enabled = (bool)TypeDescriptor.GetProperties(this)["Enabled"].GetValue(this);
            if (!enabled)
            {
                return PushButtonState.Disabled;
            }

            // speedbutton should not show focus
            if (/*Focused ||*/ IsDefault)
            {
                return PushButtonState.Default;
            }
            return PushButtonState.Normal;
        }

        protected override void OnClick(EventArgs e)
        {
            bool autocheck = AutoCheck;
            if (AllowAllUp && autocheck)
            {
                Checked = !Checked;
                AutoCheck = false;
            }
            base.OnClick(e);

            if (AllowAllUp && autocheck)
                AutoCheck = true;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Image imagestripe = Image;
            if (NumImages == 1 || imagestripe == null)
                base.OnPaint(pevent);
            else
            {
                Color transparentColor = Color.LightGray;
                Size imgsize = new Size(imagestripe.Width / NumImages, imagestripe.Height);
                Point imgstart = new Point();
                if (imagestripe is Bitmap)
                    transparentColor = ((Bitmap)imagestripe).GetPixel(0, imgsize.Height - 1);

                PushButtonState state = DetermineState();
                if (NumImages >= 2 && state == PushButtonState.Disabled)
                    imgstart.X = imgsize.Width;
                else if (NumImages >= 3 && state == PushButtonState.Pressed)
                {
                    if (!Checked)
                        imgstart.X = imgsize.Width * 2;
                    else if (NumImages >= 4)
                        imgstart.X = imgsize.Width * 3;
                }

                Rectangle destRect = ClientRectangle;
                if (ButtonRenderer.IsBackgroundPartiallyTransparent(state))
                {
                    ButtonRenderer.DrawParentBackground(pevent.Graphics, destRect, this);
                }
                ButtonRenderer.DrawButton(pevent.Graphics, destRect, false, state);
                InitializeRenderer((int)state);
                destRect = visualStyleRenderer.GetBackgroundContentRectangle(pevent.Graphics, destRect);

                Padding imgmargin = Padding;
                destRect.X += imgmargin.Left;
                destRect.Width -= imgmargin.Horizontal;
                destRect.Y += imgmargin.Top;
                destRect.Height -= imgmargin.Vertical;
                destRect = Align(imgsize, destRect, ImageAlign);

                using (ImageAttributes attr = new ImageAttributes())
                {
                    if (transparentColor.A == 255)
                        attr.SetColorKey(transparentColor, transparentColor);
                    pevent.Graphics.DrawImage(imagestripe, destRect, imgstart.X, imgstart.Y, imgsize.Width, imgsize.Height, GraphicsUnit.Pixel, attr);
                }
            }
        }
    }
}
