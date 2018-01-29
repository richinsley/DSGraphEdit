using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace DaggerLib.UI.Windows
{
    public partial class UIGraphNavigator : DoubleBufferedPanel
    {
        private DaggerUIGraph _uigraph;
        private Bitmap _canvasImage;
        private RectangleF _viewPort = new RectangleF(0, 0, 1, 1);
        private Rectangle _viewPortRect;
        private Rectangle _rzoom;
        private Color _puckColor = Color.Red;
        private bool _isMoving = false;
        private Point _dragOffset;

        public UIGraphNavigator()
        {
            InitializeComponent();
            BackgroundImageLayout = ImageLayout.Zoom;
        }

        public Color PuckColor
        {
            get
            {
                return _puckColor;
            }
            set
            {
                _puckColor = value;
                Invalidate();
            }
        }

        public DaggerUIGraph AssociatedUIGraph
        {
            get
            {
                return _uigraph;
            }
            set
            {
                // unhook previous events
                if (_uigraph != null)
                {
                    _uigraph.CanvasImageChanged -= new EventHandler(_uigraph_CanvasImageChanged);
                    _uigraph.ViewportChanged -=new EventHandler(_uigraph_ViewportChanged);
                }

                // clear the existing canvas image
                if (_canvasImage != null)
                {
                    _canvasImage.Dispose();
                    _canvasImage = null;
                }

                _uigraph = value;

                // set up events
                if (_uigraph != null)
                {
                    _uigraph.CanvasImageChanged += new EventHandler(_uigraph_CanvasImageChanged);
                    _uigraph.ViewportChanged += new EventHandler(_uigraph_ViewportChanged);
                }

                // reset the viewport
                _viewPort = new RectangleF(0, 0, 1, 1);

                // refresh the Navigator
                _uigraph_CanvasImageChanged(null, null);
            }
        }

        void _uigraph_ViewportChanged(object sender, EventArgs e)
        {
            _viewPort = _uigraph.ViewPort;
            _viewPortRect = new Rectangle((int)(_rzoom.Width * _viewPort.X + _rzoom.X),
                (int)(_rzoom.Height * _viewPort.Y + _rzoom.Y),
                (int)(_rzoom.Width * _viewPort.Width),
                (int)(_rzoom.Height * _viewPort.Height));

            Invalidate();
        }

        void _uigraph_CanvasImageChanged(object sender, EventArgs e)
        {
            if (_canvasImage != null)
            {
                _canvasImage.Dispose();
            }

            if (_uigraph != null)
            {
                // get the Canvas Image
                Bitmap tempb = _uigraph.CanvasImage;

                // Calculate a new zoom rectangle
                if (((float)tempb.Height / (float)tempb.Width) < ((float)Height / (float)Width))
                {
                    _rzoom = new Rectangle(0, 0, Width, (int)((float)Width * ((float)tempb.Height / (float)tempb.Width)));
                    _rzoom.Y = (Height - _rzoom.Height) / 2;
                }
                else
                {
                    _rzoom = new Rectangle(0, 0, (int)((float)Height * ((float)tempb.Width / (float)tempb.Height)), Height);
                    _rzoom.X = (Width - _rzoom.Width) / 2;
                }

                _rzoom.Width = Math.Max(1, _rzoom.Width);
                _rzoom.Height = Math.Max(1, _rzoom.Height);

                // stretch the image into the zoomed bitmap
                _canvasImage = new Bitmap(_rzoom.Width, _rzoom.Height);
                Graphics g = Graphics.FromImage(_canvasImage);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                g.DrawImage(tempb, new Rectangle(0, 0, _canvasImage.Width, _canvasImage.Height), new Rectangle(0, 0, tempb.Width, tempb.Height), GraphicsUnit.Pixel);
                g.Dispose();
                tempb.Dispose();

                // update the viewport position
                _uigraph_ViewportChanged(null, null);

                Invalidate(false);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_uigraph != null)
            {
                if (!_isMoving)
                {
                    if (_viewPortRect.Contains(e.Location))
                    {
                        Cursor = Cursors.SizeAll;
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }
                }
                else
                {
                    int offx = e.Location.X - _rzoom.X + _dragOffset.X;
                    int offy = e.Location.Y - _rzoom.Y + _dragOffset.Y;

                    float scalex = (float)offx / (float)_rzoom.Width;
                    float scaley = (float)offy / (float)_rzoom.Height;

                    _uigraph.ScrollToPosition(new Point((int)(scalex * (float)_uigraph.ActualCanvasSize.Width), (int)(scaley * (float)_uigraph.ActualCanvasSize.Height)));
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_uigraph != null)
            {
                if (_viewPortRect.Contains(e.Location))
                {
                    Capture = true;
                    _isMoving = true;
                    _dragOffset = new Point(_viewPortRect.X - e.Location.X, _viewPortRect.Y - e.Location.Y);
                }
            }
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (!Capture)
            {
                _isMoving = false;
                Cursor = Cursors.Default;
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            _uigraph_CanvasImageChanged(null, null);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (_canvasImage != null)
            {
                e.Graphics.DrawImage(_canvasImage, _rzoom.Location);
            }

            if (_viewPort.X != 0f || _viewPort.Y != 0f || _viewPort.Width != 1f || _viewPort.Height != 1f)
            {
                // overlay the ViewPort Rectangle
                using (Brush pb = new SolidBrush(Color.FromArgb(100, _puckColor)))
                {                    
                    e.Graphics.FillRectangle(pb, _viewPortRect);                    
                }
                using (Pen pp = new Pen(Color.FromArgb(175, _puckColor)))
                {
                    e.Graphics.DrawRectangle(pp, _viewPortRect);
                }
            }
        }
    }
}
