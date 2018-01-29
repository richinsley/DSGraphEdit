using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Text;

using DaggerLib.Core;
using DaggerLib.Interfaces;
using DaggerLib.UI.AStar;

namespace DaggerLib.UI.Windows
{
    public class DaggerNoodle : IDaggerNoodle
    {
        private DaggerOutputPin _outputPin;
        private DaggerInputPin _inputPin;

        private Point _outputPoint;
        private Point _inputPoint;

        public GraphicsPath path;

        public DaggerNoodle(DaggerOutputPin output, DaggerInputPin input)
        {
            _inputPin = input;
            _outputPin = output;
            UpdateNoodlePath(NoodleStyle.Bezier, null);
        }

        public DaggerOutputPin OutputPin
        {
            get
            {
                return _outputPin;
            }
        }

        public DaggerInputPin InputPin
        {
            get
            {
                return _inputPin;
            }
        }

        public Point InputPoint
        {
            get
            {
                return _inputPoint;
            }
        }

        public Point OutputPoint
        {
            get
            {
                return _outputPoint;
            }
        }

        public void UpdateNoodlePath(NoodleStyle style, object helper)
        {
            if (path != null)
            {
                path.Dispose();
            }

            path = new GraphicsPath();

            //recalculate the two points relative to the parent container
            //and adjust for scrollbar positions
            if (_outputPin.ParentNode != null)
            {
                if (_outputPin.ParentUIGraph != null)
                {
                    _outputPoint = (_outputPin.ParentUIGraph as DaggerUIGraph).PointToClient((_outputPin.ParentNode.UINode as DaggerUINode).PointToScreen((_outputPin.PinUIElements as PinUI).PinLocation));
                    _outputPoint.X += (_outputPin.ParentNode.UINode.PinSize / 2) - (_outputPin.ParentUIGraph as DaggerUIGraph).AutoScrollPosition.X;
                    _outputPoint.Y += (_outputPin.ParentNode.UINode.PinSize / 2) - (_outputPin.ParentUIGraph as DaggerUIGraph).AutoScrollPosition.Y;
                }
            }
            else
            {
                //it's an imported pin
                _outputPoint = (_outputPin.PinUIElements as PinUI).PinLocation;
                if (_outputPin.ParentUIGraph != null)
                {
                    _outputPoint.X += (_outputPin.ParentUIGraph.PinSize / 2);
                    _outputPoint.Y += (_outputPin.ParentUIGraph.PinSize / 2);
                }
            }

            if (_inputPin.ParentNode != null)
            {
                if (_inputPin.ParentUIGraph != null)
                {
                    _inputPoint = (_inputPin.ParentUIGraph as DaggerUIGraph).PointToClient((_inputPin.ParentNode.UINode as DaggerUINode).PointToScreen((_inputPin.PinUIElements as PinUI).PinLocation));
                    _inputPoint.X += (_inputPin.ParentNode.UINode.PinSize / 2) - (_inputPin.ParentUIGraph as DaggerUIGraph).AutoScrollPosition.X;
                    _inputPoint.Y += (_inputPin.ParentNode.UINode.PinSize / 2) - (_inputPin.ParentUIGraph as DaggerUIGraph).AutoScrollPosition.Y;
                }
            }
            else
            {
                //it's an exported pin
                _inputPoint = (_inputPin.PinUIElements as PinUI).PinLocation;
                if (_inputPin.ParentUIGraph != null)
                {
                    _inputPoint.X += (_inputPin.ParentUIGraph.PinSize / 2);
                    _inputPoint.Y += (_inputPin.ParentUIGraph.PinSize / 2);
                }
            }

            switch (style)
            {
                case NoodleStyle.Bezier:
                    _bezierStyle();
                    break;
                case NoodleStyle.Lines:
                    _lineStyle();
                    break;
                case NoodleStyle.CircuitBoardFine:
                case NoodleStyle.CircuitBoardCoarse:
                    {
                        AstarHelper bshelper = (AstarHelper)helper;
                        path.AddLines(_pathFinder(bshelper.grid, bshelper.pathCost, bshelper.grain).ToArray());                        
                    }
                    break;
                case NoodleStyle.Ramen:
                    {
                        AstarHelper bshelper = (AstarHelper)helper;
                        path.AddCurve(_pathFinder(bshelper.grid, bshelper.pathCost, bshelper.grain).ToArray(),0.75f);
                        path.Flatten();
                    }
                    break;
                case NoodleStyle.BendyStraws:
                    {
                        AstarHelper bshelper = (AstarHelper)helper;
                        // traverse the points of the pathfinder and create 90 degree ellipses
                        List<Point> points = _pathFinder(bshelper.grid, bshelper.pathCost, bshelper.grain);
                        int pcount = 0;
                        Point lastpoint = points[0];
                        while (pcount < points.Count - 2)
                        {
                            float yi = 0, xi = 0;
                            float m1 = _slope(lastpoint, points[pcount + 1], ref yi, ref xi);
                            float m2 = _slope(points[pcount + 1], points[pcount + 2], ref yi, ref xi);
                            if (m1 != m2)
                            {
                                // an angle

                                // get the mid point of seg2 and create bezier curve
                                Point mpoint = (m2 == 0) ? _hmidPoint(points[pcount + 1], points[pcount + 2]) : _vmidPoint(points[pcount + 1], points[pcount + 2]);
                                path.AddBezier(lastpoint, points[pcount + 1], points[pcount + 1], mpoint);
                                lastpoint = mpoint;
                                pcount++;
                            }
                            else
                            {
                                // strait line
                                pcount++;
                            }
                        }
                        
                        // add the last line
                        path.AddLine(lastpoint, points[points.Count - 1]);
                        path.Flatten();
                    }
                    break;
                default:
                    break;
            }          
        }

        /// <summary>
        /// Disconnects the pins that comprise this Noodle
        /// </summary>
        /// <returns>true if succeded</returns>
        public bool Disconnect()
        {
            return _outputPin.Disconnect(_inputPin,false);
        }

        private List<Point> _pathFinder(DaggerLib.UI.AStar.AStar grid,int pathCost,int grain)
        {
            Point autoScrollPos = (_inputPin.ParentUIGraph as DaggerUIGraph).AutoScrollPosition;

            int ix = (int)((float)_inputPoint.X / (float)grain);
            int iy = (int)((float)_inputPoint.Y / (float)grain);

            int ox = (int)((float)_outputPoint.X / (float)grain);
            int oy = (int)((float)_outputPoint.Y / (float)grain);

            // adjust horizontal pin positions to ensure they're not inside the node's grained rectangle
            if (_inputPin.ParentNode != null)
            {
                ix = (int)((float)(_inputPin.ParentNode.UINode.Left - autoScrollPos.X) / (float)grain);
            }
            else
            {
                ix--;
            }
            if (_outputPin.ParentNode != null)
            {
                ox = (int)((float)((_outputPin.ParentNode.UINode as DaggerUINode).Right - autoScrollPos.X) / (float)grain);
            }
            else
            {
                ox++;
            }

            // traverse the grid horizontally until start and end points have a clear line of sight
            int off = 0;
            Cell c = grid[ox + off,oy];
            while((ox + off) < grid.Size.Width)
            {
                if (c.CellCost < 500) break;
                off++;
                c = grid[ox + off, oy];
            }
            ox += off;

            off = 0;
            c = grid[ix - off, iy];
            while ((ix + off) > -1)
            {
                if (c.CellCost < 500) break;
                off--;
                c = grid[ix + off, iy];
            }
            ix += off;

            grid.Start = new Rectangle(ox, oy, 0, 0);
            grid.Goal = new Rectangle(ix, iy, 0, 0);

            int iCount = 0;
            Point[] p = null;
            try
            {
                p = grid.DeterminePath(0, ref iCount);
            }
            catch (Exception ex)
            {
                // AStar failed, just add a line
                List<Point> points = new List<Point>();
                points.Add(_outputPoint);
                points.Add(_inputPoint);
                return points;
            }

            if (p == null || p.Length < 3)
            {
                List<Point> points = new List<Point>();
                points.Add(_outputPoint);
                points.Add(_inputPoint);

                //make sure ix stays within the confines of the grid
                ix = Math.Max(0, ix);

                // increment the cost of the path
                grid.IncLine(new Point(ox, oy), new Point(ix, iy), pathCost);

                return points;
            }
            else
            {                
                List<Point> points = new List<Point>();

                // get the grain offset to properly build the line segments connecting the pins
                //int grainXOffset = _outputPoint.X - ((int)((float)_outputPoint.X / (float)grain) * grain);
                //int grainYOffset = _outputPoint.Y - ((int)((float)_outputPoint.Y / (float)grain) * grain);

                // add output pin point
                points.Add(_outputPoint);

                // build the initial path
                for (int i = 0; i < p.Length; i++)
                {
                    points.Add(new Point(p[i].X * grain, p[i].Y * grain));
                }

                // add input pin point
                points.Add(_inputPoint);

                // cull identical points
                for (int i = points.Count - 1; i > 0; i--)
                {
                    if (points[i].X == points[i - 1].X && points[i].Y == points[i - 1].Y)
                    {
                        points.Remove(points[i]);
                    }
                }

                // coerce the first and last points to be flush with the pins
                if (points[1].Y == points[2].Y)
                {
                    // it "L"s in
                    points[1] = new Point(points[1].X, points[0].Y);
                    points[2] = new Point(points[2].X, points[0].Y);
                }
                else
                {
                    points[1] = new Point(points[1].X, points[0].Y);
                }
                if (points[points.Count - 2].Y == points[points.Count - 3].Y)
                {
                    // it "L"s in
                    points[points.Count - 2] = new Point(points[points.Count - 2].X, points[points.Count - 1].Y);
                    points[points.Count - 3] = new Point(points[points.Count - 3].X, points[points.Count - 1].Y);
                }
                else
                {
                    points[points.Count - 2] = new Point(points[points.Count - 2].X, points[points.Count - 1].Y);
                }

                // increment the cost of this path in the grid
                for (int i = 0; i < points.Count - 1; i++)
                {
                    grid.IncLine(new Point(points[i].X / grain, points[i].Y / grain), new Point(points[i + 1].X / grain, points[i + 1].Y / grain), pathCost);
                }
                grid.IncLine(new Point(points[points.Count - 2].X / grain, points[points.Count - 2].Y / grain), new Point(points[points.Count - 1].X / grain, points[points.Count - 1].Y / grain), pathCost);

                return points;
            }
        }

        private void _lineStyle()
        {
            // throw points that initially extend the noodle horizontally away from the pin
            Point outputThrowPoint = new Point(_outputPoint.X, _outputPoint.Y);
            Point inputThrowPoint = new Point(_inputPoint.X, _inputPoint.Y);

            Point autoscrollOffset = (_outputPin.ParentUIGraph as DaggerUIGraph).AutoScrollPosition;

            // if the segment is too small, just do a single line
            float dist = _distance(_outputPoint, _inputPoint);
            if (dist < 40)
            {
                path.AddLine(_outputPoint, _inputPoint);
                return;
            }

            List<Point> outputPoints = new List<Point>(6);
            outputPoints.Add(_outputPoint);

            List<Point> inputPoints = new List<Point>(6);
            inputPoints.Add(_inputPoint);

            bool isReversed = OutputPoint.X > InputPoint.X;

            // the collections the pins belong to
            List<DaggerOutputPin> outputPins = null;
            List<DaggerInputPin> inputPins = null;

            int inputPinCount = 0;

            // get the output pins and add the initial output pin throw point
            if (_outputPin.ParentNode != null)
            {
                outputPins = _outputPin.ParentNode.OutputPins.MutexAvailablePins;
                outputPoints.Add(new Point(outputThrowPoint.X + outputPins.IndexOf(_outputPin) * 10 + 10, outputThrowPoint.Y));
            }
            else
            {
                outputPins = _outputPin.ParentUIGraph.Graph.ImportedPins.List;
                outputPoints.Add(new Point(outputThrowPoint.X + outputPins.IndexOf(_outputPin) * 10 + 10, outputThrowPoint.Y));
            }

            // get the input pins and add the initial input pin throw point
            if (_inputPin.ParentNode != null)
            {
                inputPins = _inputPin.ParentNode.InputPins.MutexAvailablePins;
                inputPoints.Add(new Point(inputThrowPoint.X - (inputPins.Count - inputPins.IndexOf(_inputPin)) * 10 - 10, inputThrowPoint.Y));
            }
            else
            {
                inputPins = _inputPin.ParentUIGraph.Graph.ExportedPins.List;
                inputPoints.Add(new Point(inputThrowPoint.X + inputPins.IndexOf(_inputPin) * - 10 - 10, inputThrowPoint.Y));
            }

            //swivel the points under the node until they no longer intersect with the node's client rect
            if (_outputPin.ParentNode != null)
            {
                Rectangle rect = new Rectangle(_outputPin.ParentNode.UINode.Left - autoscrollOffset.X,
                                                _outputPin.ParentNode.UINode.Top - autoscrollOffset.Y, 
                                                _outputPin.ParentNode.UINode.Width, 
                                                _outputPin.ParentNode.UINode.Height);

                // swivel around the top or bottom
                int direction = _outputPoint.Y >= _inputPoint.Y ? rect.Top : rect.Bottom;

                if (Selector.lineRectangleIntersection(rect,outputPoints[outputPoints.Count - 1], inputPoints[inputPoints.Count - 1]))
                {
                    outputPoints.Add(new Point(outputPoints[outputPoints.Count -1].X, direction));
                }
            }

            if (_inputPin.ParentNode != null)
            {
                Rectangle rect = new Rectangle(_inputPin.ParentNode.UINode.Left - autoscrollOffset.X,
                                                _inputPin.ParentNode.UINode.Top - autoscrollOffset.Y,
                                                _inputPin.ParentNode.UINode.Width,
                                                _inputPin.ParentNode.UINode.Height);

                // swivel around the top or bottom
                int direction = _outputPoint.Y <= _inputPoint.Y ? rect.Top : rect.Bottom;

                if (Selector.lineRectangleIntersection(rect,outputPoints[outputPoints.Count - 1], inputPoints[inputPoints.Count - 1]))
                {
                    inputPoints.Add(new Point(inputPoints[inputPoints.Count - 1].X, direction));
                }
            }

            //reverse the inputPoints list and append to outputPoints
            inputPoints.Reverse();
            outputPoints.AddRange(inputPoints);

            path.AddLines(outputPoints.ToArray());
        }

        private void _bezierStyle()
        {
            int slopeOffset = 0;
            int bezcontroloffset = (InputPoint.X - OutputPoint.X) / 2;
            
            //if the slope is less than 0.1 and the points are inversed, lower the bezier control points
            float by = 0, bx = 0;
            float m = Math.Abs(_slope(InputPoint, OutputPoint,ref by, ref bx));

            if (m < 0.1 && _outputPin.ParentNode != null && (OutputPoint.X > InputPoint.X))
            {
                slopeOffset = _outputPin.ParentNode.OutputPins.MutexAvailablePins.IndexOf(_outputPin) * 10 + 50;
            }

            // if the slope is less than 0.1 and the points are NOT inversed, just use a strait line
            if (m < 0.1 && slopeOffset == 0)
            {
                path.AddLine(_outputPoint, _inputPoint);
            }
            else
            {
                // create a bezier curve
                if (OutputPoint.X <= InputPoint.X)
                {
                    if (_outputPin.ParentNode != null)
                    {
                        bezcontroloffset += _outputPin.ParentNode.OutputPins.MutexAvailablePins.IndexOf(_outputPin) * 20;
                    }
                    path.AddBezier(OutputPoint,
                       new Point(OutputPoint.X + bezcontroloffset, OutputPoint.Y + slopeOffset),
                       new Point(InputPoint.X - bezcontroloffset, InputPoint.Y + slopeOffset),
                       InputPoint);
                }
                else
                {
                    if (_outputPin.ParentNode != null)
                    {
                        bezcontroloffset += _outputPin.ParentNode.OutputPins.MutexAvailablePins.IndexOf(_outputPin) * -20;
                    }
                    path.AddBezier(OutputPoint,
                       new Point(OutputPoint.X - bezcontroloffset, OutputPoint.Y + slopeOffset),
                       new Point(InputPoint.X + bezcontroloffset, InputPoint.Y + slopeOffset),
                       InputPoint);
                }
                path.Flatten();
            }
        }

        /// <summary>
        /// Get the slope of 2 points
        /// </summary>
        private float _slope(Point p1, Point p2, ref float yintercept, ref float xintercept)
        {
            float m = (float)(p1.Y - p2.Y) / (float)(p1.X - p2.X);
            yintercept = p1.Y - (m * p1.X);
            xintercept = -1 * (yintercept / m);
            return m;
        }

        /// <summary>
        /// Get the midpoint of a horizontal line
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Point _hmidPoint(Point p1, Point p2)
        {
            if (p1.X < p2.X)
            {
                return new Point((int)((float)(p2.X - p1.X) / 2f) + p1.X, p1.Y);
            }
            else
            {
                return new Point((int)((float)(p1.X - p2.X) / 2f) + p2.X, p1.Y);
            }
        }

        /// <summary>
        /// Get the midpoint of a vertical line
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private Point _vmidPoint(Point p1, Point p2)
        {
            if (p1.Y < p2.Y)
            {
                return new Point(p1.X, (int)((float)(p2.Y - p1.Y) / 2f) + p1.Y);
            }
            else
            {
                return new Point(p1.X, (int)((float)(p1.Y - p2.Y) / 2f) + p2.Y);
            }
        }

        /// <summary>
        /// Get the distance between 2 points
        /// </summary>
        private float _distance(Point a, Point b)
        {
            float xdiff = a.X - b.X;
            float ydiff = a.Y - b.Y;
            return (float)Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
        }
    }

    public class DaggerNoodleContainer
    {
        private DaggerUIGraph _uigraph;
        private List<DaggerNoodle> _noodles;
        private NoodleStyle _style = NoodleStyle.Default;
        private int _updateRef = 0;

        public DaggerNoodleContainer(DaggerUIGraph uigraph,NoodleStyle style)
        {
            _uigraph = uigraph;
            _noodles = new List<DaggerNoodle>();
        }

        public List<IDaggerNoodle> Noodles
        {
            get
            {
                List<IDaggerNoodle> noodles = new List<IDaggerNoodle>();
                foreach (DaggerNoodle n in _noodles)
                {
                    noodles.Add(n);
                }
                return noodles;
            }
        }

        public NoodleStyle NoodleStyle
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
                UpdateNoodles();
            }
        }

        public void Add(DaggerNoodle noodle)
        {
            _noodles.Add(noodle);
        }

        public void Remove(DaggerNoodle noodle)
        {
            _noodles.Remove(noodle);
        }

        public int BeginUpdate()
        {
            return _updateRef++;
        }

        public int EndUpdate()
        {
            _updateRef--;

            if (_updateRef <= 0)
            {
                _updateRef = 0;
                UpdateNoodles();
            }

            return _updateRef;
        }

        public void UpdateNoodles()
        {
            if (_updateRef == 0)
            {

                switch (_style)
                {
                    case NoodleStyle.Default:
                    case NoodleStyle.Bezier:
                        {
                            foreach (DaggerNoodle noodle in _noodles)
                            {
                                noodle.UpdateNoodlePath(NoodleStyle.Bezier, null);
                            }
                        }
                        break;
                    case NoodleStyle.Lines:
                        {
                            foreach (DaggerNoodle noodle in _noodles)
                            {
                                noodle.UpdateNoodlePath(NoodleStyle.Lines, null);
                            }
                        }
                        break;
                    case NoodleStyle.CircuitBoardCoarse:
                        {
                            // create the helper
                            AstarHelper helper = new AstarHelper();
                            helper.grain = 24;
                            helper.pathCost = 100;
                            helper.diagonalCost = 50;

                            // create the AStar path finder
                            helper.grid = _createPathGrid(_uigraph, helper.grain, helper.diagonalCost);
                            if (helper.grid == null) return;

                            foreach (DaggerNoodle noodle in _noodles)
                            {
                                noodle.UpdateNoodlePath(NoodleStyle.CircuitBoardCoarse, helper);
                                helper.grid.Reset();
                            }
                        }
                        break;
                    case NoodleStyle.CircuitBoardFine:
                        {
                            // create the helper
                            AstarHelper helper = new AstarHelper();
                            helper.grain = 16;
                            helper.pathCost = 100;
                            helper.diagonalCost = 20;

                            // create the AStar path finder
                            helper.grid = _createPathGrid(_uigraph, helper.grain, helper.diagonalCost);
                            if (helper.grid == null) return;

                            foreach (DaggerNoodle noodle in _noodles)
                            {
                                noodle.UpdateNoodlePath(NoodleStyle.CircuitBoardFine, helper);
                                helper.grid.Reset();
                            }
                        }
                        break;
                    case NoodleStyle.Ramen:
                        {
                            // create the helper
                            AstarHelper helper = new AstarHelper();
                            helper.grain = 16;
                            helper.pathCost = 100;
                            helper.diagonalCost = 20;

                            // create the AStar path finder
                            helper.grid = _createPathGrid(_uigraph, helper.grain, helper.diagonalCost);
                            if (helper.grid == null) return;

                            foreach (DaggerNoodle noodle in _noodles)
                            {
                                noodle.UpdateNoodlePath(NoodleStyle.Ramen, helper);
                                helper.grid.Reset();
                            }
                        }
                        break;
                    case NoodleStyle.BendyStraws:
                        {
                            // create the helper
                            AstarHelper helper = new AstarHelper();
                            helper.grain = 16;
                            helper.pathCost = 200;
                            helper.diagonalCost = 100;

                            // create the AStar path finder
                            helper.grid = _createPathGrid(_uigraph, helper.grain, helper.diagonalCost);
                            if (helper.grid == null) return;

                            foreach (DaggerNoodle noodle in _noodles)
                            {
                                noodle.UpdateNoodlePath(NoodleStyle.BendyStraws, helper);
                                helper.grid.Reset();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private DaggerLib.UI.AStar.AStar _createPathGrid(DaggerUIGraph uigraph, int grain, int diagonalCost)
        {
            int w = (int)((float)uigraph.CanvasSize.Width / (float)grain) + 1;
            int h = (int)((float)uigraph.CanvasSize.Height / (float)grain) + 1;
            DaggerLib.UI.AStar.AStar grid = new DaggerLib.UI.AStar.AStar(
                new Rectangle(0, 0, w, h),
                10,
                diagonalCost,
                new Point(w - 1, h - 1));

            foreach (DaggerUINode node in uigraph.AllNodes)
            {
                int top = (int)((float)(node.Top - uigraph.AutoScrollPosition.Y) / (float)grain);
                int left = (int)((float)(node.Left - uigraph.AutoScrollPosition.X) / (float)grain);
                int right = (int)((float)(node.Right - uigraph.AutoScrollPosition.X) / (float)grain + 1);
                int bottom = (int)((float)(node.Bottom - uigraph.AutoScrollPosition.Y) / (float)grain + 1);

                if (top < 0 || bottom > h || left < 0 || right > w)
                {
                    //autoscroll has jumped the gun on updating the node positions
                    return null;
                }

                for (int u = left; u < right; u++)
                {
                    for (int v = top; v < bottom; v++)
                    {
                        grid[u, v].CellCost = int.MaxValue;
                    }
                }
            }

            // increase the cost of the imported/exported pin regions
            if (uigraph.Graph.ImportedPins.Count > 0)
            {
                int right = (int)((float)((uigraph.Graph.ImportedPins[0].PinUIElements as PinUI).PinLocation.X) / (float)grain + 1);
                for (int u = 0; u < right; u++)
                {
                    for (int v = 0; v < grid.Size.Height; v++)
                    {
                        grid[u, v].CellCost = int.MaxValue;
                    }
                }
            }

            if (uigraph.Graph.ExportedPins.Count > 0)
            {
                int left = (int)((float)((uigraph.Graph.ExportedPins[0].PinUIElements as PinUI).PinLocation.X) / (float)grain);
                for (int u = left; u <= grid.Size.Width; u++)
                {
                    for (int v = 0; v < grid.Size.Height; v++)
                    {
                        grid[u, v].CellCost = int.MaxValue;
                    }
                }
            }

            return grid;
        }
    }

    internal class AstarHelper
    {
        public DaggerLib.UI.AStar.AStar grid;
        public int pathCost = 100;
        public int grain = 24;
        public int diagonalCost = 50;
    }

    public enum NoodleStyle
    {
        Default,
        Bezier,
        Lines,
        CircuitBoardCoarse,
        CircuitBoardFine,
        Ramen,
        BendyStraws
    }
}
