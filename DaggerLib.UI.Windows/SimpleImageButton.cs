using System;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace DaggerLib.UI.Windows
{
    [Designer(typeof(SimpleImageButtonDesigner))]
    [ToolboxItem(false)]
    public class SimpleImageButton : UserControl
    {
        private System.Windows.Forms.ToolTip m_wndToolTip;

        public event EventHandler Clicked;

        public SimpleImageButton()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            m_wndToolTip = new ToolTip();
            m_wndToolTip.SetToolTip(this, ToolTipText);
            m_wndToolTip.Active = this.ToolTipEnabled;
            m_wndToolTip.AutomaticDelay = 1000;

            Width = 10;
            Height = 10;

            BackColor = Color.Transparent;
        }

        public delegate void StateChangedEventHandler(object sender, EventArgs e);
        [Description("Event fires when the Value property changes")]
        [Category("Action")]
        public event StateChangedEventHandler StateChanged;

        /// <summary>
        /// flag to indicate we were pasted from a Desginer
        /// we have to skip the first one to avoid confusing vs2005
        /// </summary>
        internal bool pasted = false;

        /// <summary>
        /// Indicates this button is not part of a collection
        /// </summary>
        public bool IsInternal = false;

        /// <summary>
        /// Instance of the image being painted inside the control
        /// </summary>
        private Image image = null;

        /// <summary>
        /// Instance of 2nd state image
        /// </summary>
        private Image image2 = null;

        private Color outsideTint = Color.FromArgb(191, 191, 191, 191);
        private float[][] outsideMatrixElements = { 
                    new float[] {0.75F,0,0,0,0},
                    new float[] {0,0.75F,0,0,0},
                    new float[] {0,0,0.75F,0,0},
                    new float[] {0,0,0,.75F,0},
                    new float[] {0,0,0,0,1} };

        private Color insideTint = Color.White;
        private float[][] insideMatrixElements = { 
                    new float[] {1,0,0,0,0},
                    new float[] {0,1,0,0,0},
                    new float[] {0,0,1,0,0},
                    new float[] {0,0,0,1,0},
                    new float[] {0,0,0,0,1} };

        private float[][] negativeMatrixElements = {
                    new float[] {1f, .5f , .5f, 0f, 0f},
                     new float[] {.5f, 1f, .5f, 0f, 0f},
                     new float[] {.5f, .5f, 1f, 0f, 0f},
                     new float[] {0f, 0f , 0f, 1f, 0f},
                     new float[] {0f , 0f , 0f , 0f, 1f} };

        /// <summary>
        /// Instance of a bool variable telling wheather the mouse is inside this control or not
        /// </summary>
        private bool mouseInside = false;
        private bool mouseDown = false;

        #region Properties
        private bool _state;
        public bool State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_multistate)
                {
                    if (_state != value)
                    {
                        _state = value;

                        //call event handlers
                        if (StateChanged != null)
                        {
                            StateChanged(this, new EventArgs());
                        }
                    }
                    this.Refresh();
                }
            }
        }

        private string _tooltiptext = "ToolTip";
        public string ToolTipText
        {
            get
            {
                return _tooltiptext;
            }
            set
            {
                _tooltiptext = value;
                if (m_wndToolTip != null)
                {
                    m_wndToolTip.SetToolTip(this, value);
                }
            }
        }

        private bool _toolTipEnabled = false;
        public bool ToolTipEnabled
        {
            get
            {
                return _toolTipEnabled;
            }
            set
            {
                if (m_wndToolTip != null)
                {
                    m_wndToolTip.Active = value;
                }
                _toolTipEnabled = value;
            }
        }

        private bool _multistate;
        public bool MultiState
        {
            get
            {
                return _multistate;
            }
            set
            {
                _multistate = value;
                if (!_multistate)
                {
                    _state = false;
                }
                this.Refresh();
            }
        }

        public Image ButtonImage
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                this.Refresh();
            }
        }

        public Image ButtonImage2
        {
            get
            {
                return image2;
            }
            set
            {
                image2 = value;
                this.Refresh();
            }
        }

        public Color MouseInsideTint
        {
            get
            {
                return this.insideTint;
            }
            set
            {
                this.insideTint = value;
                insideMatrixElements[0][0] = (float)value.R / (float)255;
                insideMatrixElements[1][1] = (float)value.G / (float)255;
                insideMatrixElements[2][2] = (float)value.B / (float)255;
                insideMatrixElements[3][3] = (float)value.A / (float)255;
                Refresh();
            }
        }

        public Color MouseOutsideTint
        {
            get
            {
                return this.outsideTint;
            }
            set
            {
                this.outsideTint = value;
                outsideMatrixElements[0][0] = (float)value.R / (float)255;
                outsideMatrixElements[1][1] = (float)value.G / (float)255;
                outsideMatrixElements[2][2] = (float)value.B / (float)255;
                outsideMatrixElements[3][3] = (float)value.A / (float)255;
                Refresh();
            }
        }

        #endregion

        #region Overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            if (image != null)
            {
                if (mouseDown)
                {
                    ColorMatrix colorMatrix = new ColorMatrix(negativeMatrixElements);
                    ImageAttributes ImgAttr = new ImageAttributes();
                    ImgAttr.SetColorMatrix(colorMatrix);
                    e.Graphics.DrawImage(_state ? image2 : image, new Rectangle(0, 0, this.Width, this.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ImgAttr);
                }
                else
                {
                    if (mouseInside)
                    {
                        ColorMatrix colorMatrix = new ColorMatrix(insideMatrixElements);
                        ImageAttributes ImgAttr = new ImageAttributes();
                        ImgAttr.SetColorMatrix(colorMatrix);
                        e.Graphics.DrawImage(_state ? image2 : image, new Rectangle(0, 0, this.Width, this.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ImgAttr);
                    }
                    else
                    {
                        ColorMatrix colorMatrix = new ColorMatrix(outsideMatrixElements);
                        ImageAttributes ImgAttr = new ImageAttributes();
                        ImgAttr.SetColorMatrix(colorMatrix);
                        e.Graphics.DrawImage(_state ? image2 : image, new Rectangle(0, 0, this.Width, this.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ImgAttr);
                    }
                }
            }
            base.OnPaint(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.mouseInside = true;
            this.Refresh();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.mouseInside = false;
            this.Refresh();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.mouseDown = true;
            this.Refresh();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_multistate)
            {
                State = !_state;
            }

            this.mouseDown = false;
            this.Refresh();
            base.OnMouseUp(e); 
            if (Clicked != null)
            {
                Clicked(this, new EventArgs());
            }

            if (ContextMenuStrip != null)
            {
                ContextMenuStrip.Show(this, new Point(0, 0));
            }
        }

        //We want to ignore right and middle mouse buttons
        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == 0x204) || (m.Msg == 0x207))
            {
                return;
            }
            base.WndProc(ref m);
        }
        #endregion
    }

    /// <summary>
    /// Collection Class to hold a list of SimpleImageButtons
    /// </summary>
    public class CaptionButtonCollection : CollectionBase
    {
        public event EventHandler ButtonAdded;
        public event EventHandler ButtonRemoved;
        public event EventHandler ButtonVisibleChanged;

        protected override void OnInsert(int index, object value)
        {
            if (value is SimpleImageButton)
            {
                base.OnInsert(index, value);
                (value as SimpleImageButton).Anchor = AnchorStyles.Top | AnchorStyles.Right;
                (value as SimpleImageButton).VisibleChanged += new EventHandler(CaptionButtonCollection_VisibleChanged);
            }
            else
            {
                throw new InvalidOperationException("Item added was not a SimpleImageButton");
            }
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            if (newValue is SimpleImageButton)
            {
                base.OnSet(index, oldValue, newValue);
                (newValue as SimpleImageButton).Anchor = AnchorStyles.Top | AnchorStyles.Right;
                (newValue as SimpleImageButton).VisibleChanged += new EventHandler(CaptionButtonCollection_VisibleChanged);
            }
            else
            {
                throw new InvalidOperationException("Item added was not a SimpleImageButton");
            }
        }

        public void Add(SimpleImageButton button)
        {
            List.Add(button);
        }

        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            if (ButtonAdded != null)
            {
                ButtonAdded(value, new EventArgs());
            }
        }

        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            if (ButtonRemoved != null)
            {
                ButtonRemoved(value, new EventArgs());
            }
        }

        void CaptionButtonCollection_VisibleChanged(object sender, EventArgs e)
        {
            if (ButtonVisibleChanged != null)
            {
                ButtonVisibleChanged(sender, e);
            }
        }

        public ArrayList AllButtons
        {
            get
            {
                return new ArrayList(List);
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SimpleImageButton this[int index]
        {
            get
            {
                return (SimpleImageButton)List[index];
            }
        }
    }

    internal class SimpleImageButtonDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            //remove properties not needed for the buttons
            base.PreFilterProperties(properties);

            properties.Remove("AccessibleDescription");
            properties.Remove("AccessibleName");
            properties.Remove("AccessibleRole");
            properties.Remove("BackColor");
            properties.Remove("Margin");
            properties.Remove("Dock");
            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("Font");
            properties.Remove("RightToLeft");
            properties.Remove("AutoValidate");
            properties.Remove("ImeMode");
            properties.Remove("CausesValidation");
            properties.Remove("Anchor");
            properties.Remove("AutoScroll");
            properties.Remove("AutoScrollMargin");
            properties.Remove("AutoScrollMinSize");
            properties.Remove("AutoSize");
            properties.Remove("AutoSizeMode");
            properties.Remove("Location");
            properties.Remove("MaximumSize");
            properties.Remove("MinimumSize");
            properties.Remove("Padding");
            properties.Remove("Size");
        }
    }
}
