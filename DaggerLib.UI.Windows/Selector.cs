using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

using DaggerLib.Core;
using DaggerLib.Interfaces;

namespace DaggerLib.UI.Windows
{
    /// <summary>
    /// Class to multi-select Noodles and Nodes
    /// </summary>
    public class Selector : ISelector
    {
        private Point _startPoint;
        private Point _previousPoint;
        private Point _currentPoint;
        private Panel _parent;
        private bool _initialPoint = true;

        //all the nodes in the panel we are tracking
        private List<IDaggerUINode> _allNodes;

        //all the noodle in the panel we are tracking
        private List<IDaggerNoodle> _allNoodles;

        //the nodes we have selected
        private List<IDaggerUINode> _selectedNodes;

        //the noodles we have selected
        private List<IDaggerNoodle> _selectedNoodles;

        //are we currently dragging the rectangle with the mouse?
        private bool _tracking = true;

        public Selector(Panel parent, Point startPoint, List<IDaggerNoodle> noodles, List<IDaggerUINode> nodes)
        {
            _parent = parent;
            CurrentPoint = _previousPoint = _startPoint = startPoint;
            _allNodes = nodes;
            _allNoodles = noodles;
            _selectedNodes = new List<IDaggerUINode>();
            _selectedNoodles = new List<IDaggerNoodle>();
        }

        public Selector(DaggerUIGraph graph)
        {
            _tracking = false;
            _selectedNodes = new List<IDaggerUINode>(graph.AllNodes);
            _selectedNoodles = new List<IDaggerNoodle>(graph.AllNoodles);
        }

        public Selector(List<IDaggerUINode> nodes, List<IDaggerNoodle> noodles)
        {
            _tracking = false;
            _selectedNodes = new List<IDaggerUINode>(nodes);
            _selectedNoodles = new List<IDaggerNoodle>(noodles);
        }

        /// <summary>
        /// Get's the rectangle that is selected
        /// </summary>
        public Rectangle SelectionArea
        {
            get
            {
                int left = Math.Min(_startPoint.X, _currentPoint.X);
                int right = Math.Max(_startPoint.X, _currentPoint.X);
                int top = Math.Min(_startPoint.Y, _currentPoint.Y);
                int bottom = Math.Max(_startPoint.Y, _currentPoint.Y);

                return new Rectangle(left, top, right - left, bottom - top);
            }
        }

        //the nodes we have selected
        public List<IDaggerUINode> SelectedNodes
        {
            get
            {
                return _selectedNodes;
            }
            set
            {
                _selectedNodes = value;
            }
        }

        //the noodles we have selected
        public List<IDaggerNoodle> SelectedNoodles
        {
            get
            {
                return _selectedNoodles;
            }
            set
            {
                _selectedNoodles = value;
            }
        }

        /// <summary>
        /// Gets if the selection if currently empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (SelectedNodes.Count == 0 && SelectedNoodles.Count == 0);
            }
        }

        /// <summary>
        /// Gets or sets the current mouse position of selection
        /// </summary>
        public Point CurrentPoint
        {
            get
            {
                return _currentPoint;
            }
            set
            {
                _previousPoint = _currentPoint;
                _currentPoint = value;

                Rectangle newrect = new Rectangle(SelectionArea.X, SelectionArea.Y, SelectionArea.Width, SelectionArea.Height);
                newrect.Offset(_parent.AutoScrollPosition.X, _parent.AutoScrollPosition.Y);

                //if this is not the initial point, erase previous frame
                if (!_initialPoint)
                {
                    int left = Math.Min(_startPoint.X, _previousPoint.X);
                    int right = Math.Max(_startPoint.X, _previousPoint.X);
                    int top = Math.Min(_startPoint.Y, _previousPoint.Y);
                    int bottom = Math.Max(_startPoint.Y, _previousPoint.Y);
                    Rectangle oldrect = new Rectangle(left, top, right - left, bottom - top);
                    oldrect.Offset(_parent.AutoScrollPosition.X, _parent.AutoScrollPosition.Y);

                    ControlPaint.DrawReversibleFrame(_parent.RectangleToScreen(oldrect), Color.Black, FrameStyle.Dashed);
                }

                ControlPaint.DrawReversibleFrame(_parent.RectangleToScreen(newrect), Color.Black, FrameStyle.Dashed);

                //show that we have moved at least once
                _initialPoint = false;
            }
        }

        /// <summary>
        /// Returns true if we are currently tracking the mouse position.
        /// If setting to false, search and add items to selection lists.
        /// </summary>
        public bool Tracking
        {
            get
            {
                return _tracking;
            }
            set
            {
                _tracking = value;
                if (!_tracking)
                {
                    //we've stopped tracking, so calculate the intersection of all Nodes and Noodles

                    //if it was just a click, expand the selected area to 7x7 and ignore Node testing
                    if ((SelectionArea.Width == 0) && (SelectionArea.Height == 0))
                    {
                        _startPoint.X -= 3;
                        _currentPoint.X += 3;
                        _startPoint.Y -= 3;
                        _currentPoint.Y += 3;

                        UpdateSelected(true);
                    }
                    else
                    {
                        //wasn't just a click, so erase the Dragging Frame and perform hit tests on noodles AND nodes
                        Rectangle newrect = new Rectangle(SelectionArea.X, SelectionArea.Y, SelectionArea.Width, SelectionArea.Height);
                        newrect.Offset(_parent.AutoScrollPosition.X, _parent.AutoScrollPosition.Y);
                        ControlPaint.DrawReversibleFrame(_parent.RectangleToScreen(newrect), Color.Black, FrameStyle.Dashed);

                        UpdateSelected(false);
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the nodes and noodles that fall within the selction area
        /// </summary>
        /// <param name="IsClick">true if the user clicked without dragging</param>
        private void UpdateSelected(bool IsClick)
        {
            SelectedNoodles.Clear();

            //hit test Nodes if it wasn't just a click

            if (_allNodes != null)
            {
                foreach (DaggerUINode node in _allNodes)
                {
                    if (!IsClick)
                    {
                        Rectangle noderect = new Rectangle(node.Location, node.Size);

                        if (_parent is ScrollableControl)
                        {
                            // modify noderect to account for scrollbar positions
                            noderect.Offset((_parent as ScrollableControl).AutoScrollPosition.X * -1, (_parent as ScrollableControl).AutoScrollPosition.Y * -1);
                        }

                        if (noderect.IntersectsWith(SelectionArea))
                        {
                            SelectedNodes.Add((IDaggerUINode)node);
                        }
                    }
                }
            }

            //hit test Noodles
            foreach (DaggerNoodle noodle in _allNoodles)
            {
                //get the points from the noodle path
                PointF[] pathpoints = noodle.path.PathPoints;

                //check each segment to see if it intersects with the selction area
                for (int i = 0; i < pathpoints.Length - 1; i++)
                {
                    if (lineRectangleIntersection(SelectionArea, pathpoints[i], pathpoints[i + 1]))
                    {
                        SelectedNoodles.Add(noodle);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Determine if a Rectangle contains or intersects a line segment
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="sega"></param>
        /// <param name="segb"></param>
        /// <returns></returns>
        internal static bool lineRectangleIntersection(Rectangle rect, PointF sega, PointF segb)
        {
            //are either segment points actually inside the rectangle?
            if (rect.Contains((int)sega.X, (int)sega.Y) || rect.Contains((int)segb.X, (int)segb.Y))
            {
                return true;
            }

            PointF ul = new PointF(rect.X, rect.Y);
            PointF ur = new PointF(rect.Right, rect.Y);
            PointF bl = new PointF(rect.X, rect.Bottom);
            PointF br = new PointF(rect.Right, rect.Bottom);

            if (lineSegmentIntersection(sega.X, sega.Y, segb.X, segb.Y, ul.X, ul.Y, ur.X, ur.Y))
            {
                return true;
            }

            if (lineSegmentIntersection(sega.X, sega.Y, segb.X, segb.Y, bl.X, bl.Y, br.X, br.Y))
            {
                return true;
            }

            if (lineSegmentIntersection(sega.X, sega.Y, segb.X, segb.Y, ul.X, ul.Y, br.X, br.Y))
            {
                return true;
            }

            if (lineSegmentIntersection(sega.X, sega.Y, segb.X, segb.Y, ur.X, ur.Y, br.X, br.Y))
            {
                return true;
            }

            return false;
        }

        internal static bool lineRectangleIntersection(Rectangle rect,Point p1, Point p2)
        {
            //top
            if(lineSegmentIntersection(p1,p2,new Point(rect.Left,rect.Top),new Point(rect.Right,rect.Top)))
            {
                return true;
            }

            //bottom
            if(lineSegmentIntersection(p1,p2,new Point(rect.Left,rect.Bottom),new Point(rect.Right,rect.Bottom)))
            {
                return true;
            }

            //left
            if(lineSegmentIntersection(p1,p2,new Point(rect.Left,rect.Top),new Point(rect.Left,rect.Bottom)))
            {
                return true;
            }

            //right
            if (lineSegmentIntersection(p1, p2, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom)))
            {
                return true;
            }

            // no intersection
            return false;
        }

        /// <summary>
        /// Returns true if segment p1,p2 intersects segment p3,p4
        /// </summary>
        internal static bool lineSegmentIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            return lineSegmentIntersection(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
        }

        /// <summary>
        /// Returns true if segment AB and CD intersect
        /// </summary>
        internal static bool lineSegmentIntersection(
            float Ax, float Ay,
            float Bx, float By,
            float Cx, float Cy,
            float Dx, float Dy)
        {

            float distAB, theCos, theSin, newX, ABpos;

            //  Fail if either line segment is zero-length.
            if (Ax == Bx && Ay == By || Cx == Dx && Cy == Dy) return false;

            //  (1) Translate the system so that point A is on the origin.
            Bx -= Ax; By -= Ay;
            Cx -= Ax; Cy -= Ay;
            Dx -= Ax; Dy -= Ay;

            //  Discover the length of segment A-B.
            distAB = (float)Math.Sqrt(Bx * Bx + By * By);

            //  (2) Rotate the system so that point B is on the positive X axis.
            theCos = Bx / distAB;
            theSin = By / distAB;
            newX = Cx * theCos + Cy * theSin;
            Cy = Cy * theCos - Cx * theSin; Cx = newX;
            newX = Dx * theCos + Dy * theSin;
            Dy = Dy * theCos - Dx * theSin; Dx = newX;

            //  Fail if segment C-D doesn't cross line A-B.
            if (Cy < 0f && Dy < 0f || Cy >= 0f && Dy >= 0f) return false;

            //  (3) Discover the position of the intersection point along line A-B.
            ABpos = Dx + (Cx - Dx) * Dy / (Dy - Cy);

            //  Fail if segment C-D crosses line A-B outside of segment A-B.
            if (ABpos < 0f || ABpos > distAB) return false;

            //  Success.
            return true;
        }
    }
}
