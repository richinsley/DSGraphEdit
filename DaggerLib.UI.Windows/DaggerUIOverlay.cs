using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DaggerLib.UI.Windows
{
    internal class DaggerOverlay : Form
    {
        private Control _control;
        private List<Control> _multiControls;
        private Size _multiControlSize;
        private Point _multiControlCurrentLocation;
        private Region _multiControlRegion;

        private DaggerNodeAlterState editstate;
        private Point origin;
        private int _minWidth;
        private int _minHeight;
        private int _resizeOffsetX;
        private int _resizeOffsetY;

        private bool operationsBegin = false;

        /// <summary>
        /// Constructor for moving multiple controls
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="MouseOrigin"></param>
        public DaggerOverlay(List<Control> controls, Point MouseOrigin)
        {
            _multiControls = controls;
            editstate = DaggerNodeAlterState.Move;

            // get the upperleft-most and lowerright-most control positions
            Point ulcornerPoint = _multiControls[0].PointToScreen(new Point(0, 0));
            Point lrcornerPoint = _multiControls[0].PointToScreen(new Point(_multiControls[0].Width, _multiControls[0].Height));
            foreach (Control c in _multiControls)
            {
                Point tp1 = c.PointToScreen(new Point(0, 0));
                Point tp2 = c.PointToScreen(new Point(c.Width, c.Height));
                ulcornerPoint = new Point(Math.Min(ulcornerPoint.X, tp1.X), Math.Min(ulcornerPoint.Y, tp1.Y));
                lrcornerPoint = new Point(Math.Max(lrcornerPoint.X, tp2.X), Math.Max(lrcornerPoint.Y, tp2.Y));
            }

            // calc the size needed from the previous points
            _multiControlSize = new Size(lrcornerPoint.X - ulcornerPoint.X, lrcornerPoint.Y - ulcornerPoint.Y);
            origin = new Point(MouseOrigin.X - ulcornerPoint.X, MouseOrigin.Y - ulcornerPoint.Y);
            _multiControlCurrentLocation = ulcornerPoint;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Blue;
            this.ForeColor = System.Drawing.Color.LightGreen;
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.Name = "EditOverlay";
            this.Opacity = 0;

            this.BackgroundImage = new Bitmap(_multiControlSize.Width, _multiControlSize.Height);
            Graphics g = Graphics.FromImage(this.BackgroundImage);

            // composite the regions and bitmaps of all the controls into the overlay window
            foreach (Control c in _multiControls)
            {
                Region rr = new Region(c.Region.GetRegionData());
                Point screenPoint = c.PointToScreen(new Point(0, 0)); 

                rr.Translate(screenPoint.X - _multiControlCurrentLocation.X, screenPoint.Y - _multiControlCurrentLocation.Y);
                if (Region != null)
                {
                    Region.Union(rr);
                }
                else
                {
                    _multiControlRegion = Region = new Region(rr.GetRegionData());
                }
                rr.Dispose();

                // copy the controls bitmap onto this one
                Bitmap b = new Bitmap(c.Width, c.Height);
                c.DrawToBitmap(b, c.ClientRectangle);
                g.DrawImage(b, screenPoint.X - _multiControlCurrentLocation.X, screenPoint.Y - _multiControlCurrentLocation.Y);
            }

            g.Dispose();
        }

        /// <summary>
        /// Constructor for moving or resizing a single control
        /// </summary>
        /// <param name="c"></param>
        /// <param name="state"></param>
        /// <param name="MouseOrigin"></param>
        /// <param name="minWidth"></param>
        /// <param name="minHeight"></param>
        public DaggerOverlay(Control c, DaggerNodeAlterState state, Point MouseOrigin, int minWidth, int minHeight)
        {
            _control = c;
            editstate = state;
            Point ulcornerPoint = c.PointToScreen(new Point(0, 0));
            origin = new Point(MouseOrigin.X - ulcornerPoint.X, MouseOrigin.Y - ulcornerPoint.Y);

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Blue;
            this.ForeColor = System.Drawing.Color.LightGreen;
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.Name = "EditOverlay";
            this.Opacity = 0;            

            //if we are just moving it, copy the region of the control
            if (state == DaggerNodeAlterState.Move)
            {
                Region = c.Region;
                Bitmap b = new Bitmap(c.Width, c.Height);
                c.DrawToBitmap(b, c.ClientRectangle);
                this.BackgroundImage = b;
            }
            else
            {
                //if we are resizing, make adjustments for mouse origin
                _minWidth = minWidth;
                _minHeight = minHeight;
                Point lrCornerPoint = c.PointToScreen(new Point(c.Width, c.Height));
                _resizeOffsetX = lrCornerPoint.X - MouseOrigin.X;
                _resizeOffsetY = lrCornerPoint.Y - MouseOrigin.Y;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (_control != null)
            {
                this.Location = this._control.PointToScreen(new Point(0, 0));
                this.Size = _control.Size;
            }
            else
            {
                this.Location = _multiControlCurrentLocation;
                this.Size = _multiControlSize;
            }

            this.Opacity = 0.55;
            this.BringToFront();
            this.Capture = true;
            base.OnShown(e);
            this.operationsBegin = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // was EndOperations already called ?
            if (this.operationsBegin)
            {
                this.operationsBegin = false;
                EndOperations();
            }
            base.OnMouseUp(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            // was EndOperations already called ?
            if (this.operationsBegin)
            {
                this.operationsBegin = false;
                EndOperations();
            }
            base.OnLostFocus(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            switch (editstate)
            {
                case DaggerNodeAlterState.Move:
                    {
                        Point oldPos = PointToScreen(e.Location);
                        Point newPos = new Point(oldPos.X - origin.X, oldPos.Y - origin.Y);
                        this.Location = newPos;
                    }
                    break;
                case DaggerNodeAlterState.SouthEast:
                    {
                        int neww = Math.Max(e.X, _minWidth) + _resizeOffsetX;
                        int newh = Math.Max(e.Y, _minHeight) + _resizeOffsetY;

                        //transform the region to new size
                        int pinSize = (_control as DaggerUINode).PinSize;

                        switch ((_control as DaggerUINode).PinPlacement)
                        {
                            case DaggerNodePinPlacement.Indent:
                            case DaggerNodePinPlacement.Inset:
                                break;
                            case DaggerNodePinPlacement.Outset:
                                {
                                    Region = new Region(new Rectangle(pinSize, 0, neww - pinSize * 2, newh));
                                }
                                break;
                            default:
                                break;
                        }

                        this.Width = neww;
                        this.Height = newh;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Ends drag/resize operations and commits changes
        /// </summary>
        private void EndOperations()
        {
            this.Visible = false;
            Capture = false;

            if (_control != null)
            {
                Point newP = _control.Parent.PointToClient(this.Location);

                //clamp it to (0,0)
                newP.X = Math.Max((_control.Parent as Panel).AutoScrollPosition.X, newP.X);
                newP.Y = Math.Max((_control.Parent as Panel).AutoScrollPosition.Y, newP.Y);

                (_control.Parent as DaggerUIGraph).BeginCanvasUpdate();
                _control.Location = newP;
                _control.Size = this.Size;
                (_control.Parent as DaggerUIGraph).EndCanvasUpdate();
            }
            else
            {
                Point oldP = _multiControls[0].Parent.PointToClient(_multiControlCurrentLocation);
                Point newP = _multiControls[0].Parent.PointToClient(this.Location);

                //clamp it to (0,0)
                newP.X = Math.Max((_multiControls[0].Parent as Panel).AutoScrollPosition.X, newP.X);
                newP.Y = Math.Max((_multiControls[0].Parent as Panel).AutoScrollPosition.Y, newP.Y);

                // translate the controls to new locations
                int offx = newP.X - oldP.X;
                int offy = newP.Y - oldP.Y;

                // pause updating of noodles and nodes
                (_multiControls[0].Parent as DaggerUIGraph)._noodles.BeginUpdate();
                (_multiControls[0].Parent as DaggerUIGraph).BeginCanvasUpdate();

                foreach (Control c in _multiControls)
                {
                    c.Location = new Point(c.Location.X + offx, c.Location.Y + offy);
                }

                // resume updating of noodles and nodes and refresh graph
                (_multiControls[0].Parent as DaggerUIGraph)._noodles.EndUpdate();
                (_multiControls[0].Parent as DaggerUIGraph).EndCanvasUpdate();

                // we made our own temp region for multi controls so dispose of it here
                Region = null;
                _multiControlRegion.Dispose();
                _multiControlRegion = null;
            }

            // dispose of the Background image if we created one
            if (BackgroundImage != null)
            {
                BackgroundImage.Dispose();
                BackgroundImage = null;
            }

            this.Close();
        }

        public void BeginOperations()
        {
            this.Visible = true;
        }
    }
}

