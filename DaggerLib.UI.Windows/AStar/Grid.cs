using System;
using System.Drawing;

//
// Grid.cs
//
// this contains the basic Grid class used to map out the area
//
// First assumption is that you can't go outside the world.  This means no negative
// coordinates.  Second assumption is that diagonal movement is allowed.

namespace DaggerLib.UI.AStar
{
    public class Cell
    {
        public const int BLOCKED = int.MaxValue;
        public Point BackDirection;
        public int CellCost;
        public int CurrentCost;
        public int EstimatedCost;
        public int TurnCount;
        private Point pCenter;
        public int iHeapIndex;

        public int TotalCost
        {
            get
            {
                if (CurrentCost == int.MaxValue || EstimatedCost == int.MaxValue)
                    return int.MaxValue;
                return CurrentCost + EstimatedCost;
            }
        }

        public Point Center
        {
            get { return pCenter; }
        }

        public Cell(Point CellCenter)
        {
            BackDirection = Point.Empty;
            CurrentCost = int.MaxValue;
            CellCost = 0;
            EstimatedCost = 0;
            pCenter = CellCenter;
            TurnCount = 0;
            iHeapIndex = -1;
        }
    }

    public class Grid
    {
        Rectangle m_rWorkingRegion;
        Rectangle m_rWorld;

        Rectangle m_rGoal;
        Rectangle m_rStart;

        Cell[,] m_cCell;

        const int m_ciCost = 10;
        const int m_ciDiagonalCost = 14;

        int m_iDiagonalCost;
        int m_iCost;

        public readonly Rectangle InvalidRectangle = new Rectangle(-1, -1, 0, 0);

        public Grid(Rectangle WorkingRegion)
        {
            m_iDiagonalCost = m_ciDiagonalCost;
            m_iCost = m_ciCost;

            m_rGoal = InvalidRectangle;
            m_rStart = InvalidRectangle;

            m_rWorkingRegion = WorkingRegion;
            m_rWorld = m_rWorkingRegion;
            m_rWorld = new Rectangle(0, 0, m_rWorkingRegion.Width + m_rWorkingRegion.Left, m_rWorkingRegion.Height + m_rWorkingRegion.Top);

            m_cCell = new Cell[m_rWorkingRegion.Width + 1, m_rWorkingRegion.Height + 1];
            InitializeCells();
        }


        public Grid(Rectangle WorkingRegion, int StandardCost, int DiagonalCost)
        {
            m_iCost = StandardCost;
            m_iDiagonalCost = DiagonalCost;

            m_rGoal = InvalidRectangle;
            m_rStart = InvalidRectangle;

            m_rWorkingRegion = new Rectangle(Math.Max(0, WorkingRegion.Left), Math.Max(0, WorkingRegion.Top),
                                              Math.Max(0, WorkingRegion.Width), Math.Max(0, WorkingRegion.Height));

            m_rWorld = m_rWorkingRegion;
            m_rWorld = new Rectangle(0, 0, m_rWorkingRegion.Width + m_rWorkingRegion.Left, m_rWorkingRegion.Height + m_rWorkingRegion.Top);

            m_cCell = new Cell[m_rWorkingRegion.Width + 1, m_rWorkingRegion.Height + 1];
            InitializeCells();
        }

        public Grid(Rectangle WorkingRegion,
           int StandardCost, int DiagonalCost,
           Point WorldLowerRight)
        {
            m_iCost = StandardCost;
            m_iDiagonalCost = DiagonalCost;

            m_rGoal = InvalidRectangle;
            m_rStart = InvalidRectangle;

            m_rWorkingRegion = new Rectangle(Math.Max(0, WorkingRegion.Left), Math.Max(0, WorkingRegion.Top),
               Math.Max(0, WorkingRegion.Width), Math.Max(0, WorkingRegion.Height));

            m_rWorld = m_rWorkingRegion;
            m_rWorld = new Rectangle(0, 0,
                           Math.Max(WorldLowerRight.X, m_rWorkingRegion.Width + m_rWorkingRegion.Left),
                           Math.Max(WorldLowerRight.Y, m_rWorkingRegion.Height + m_rWorkingRegion.Top));

            m_cCell = new Cell[m_rWorkingRegion.Width + 1, m_rWorkingRegion.Height + 1];
            InitializeCells();
        }

        public Size Size
        {
            get
            {
                return m_rWorkingRegion.Size;
            }
        }

        private void InitializeCells()
        {
            for (int i = 0; i < m_rWorkingRegion.Width + 1; i++)
            {
                for (int j = 0; j < m_rWorkingRegion.Height + 1; j++)
                {
                    Cell c = new Cell(new Point(m_rWorkingRegion.Left + i, m_rWorkingRegion.Top + j));
                    c.CellCost = m_iCost;
                    m_cCell[i, j] = c;
                }
            }
        }

        public virtual void Reset()
        {
            for (int i = 0; i <= m_rWorkingRegion.Width; i++)
            {
                for (int j = 0; j <= m_rWorkingRegion.Height; j++)
                {
                    m_cCell[i, j].BackDirection = Point.Empty;
                    m_cCell[i, j].CurrentCost = int.MaxValue;
                    m_cCell[i, j].EstimatedCost = 0;
                    m_cCell[i, j].TurnCount = 0;
                    m_cCell[i, j].iHeapIndex = -1;
                }
            }
            m_rGoal = InvalidRectangle;
        }

        protected int StandardCost
        {
            get { return m_iCost; }
        }

        protected int DiagonalCost
        {
            get { return m_iDiagonalCost; }
        }

        /// <summary>
        /// Determines whether or not a given point is on the current grid
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        protected bool IsOnGrid(int X, int Y)
        {
            if ((X >= m_rWorkingRegion.Left) && (X <= m_rWorkingRegion.Right) &&
                  (Y >= m_rWorkingRegion.Top) && (Y <= m_rWorkingRegion.Bottom))
                return true;
            return false;
        }

        /// <summary>
        /// Adds a totally blocking item at that location
        /// </summary>
        public virtual void Add(Rectangle Obstacle)
        {
            Add(Obstacle, Cell.BLOCKED);
        }

        /// <summary>
        /// Add an item to the grid that costs CellCost per cell to traverse.
        /// </summary>
        public virtual void Add(Rectangle Obstacle, int CellCost)
        {
            int iLeft = Math.Max(Obstacle.Left, m_rWorkingRegion.Left);
            int iRight = Math.Min(Obstacle.Right, m_rWorkingRegion.Right);
            int iTop = Math.Max(Obstacle.Top, m_rWorkingRegion.Top);
            int iBottom = Math.Min(Obstacle.Bottom, m_rWorkingRegion.Bottom);
            for (int i = iLeft; i <= iRight; i++)
            {
                for (int j = iTop; j <= iBottom; j++)
                {
                    //this[i,j].CellCost = Math.Max (this [i,j].CellCost, CellCost);
                    this[i, j].CellCost = CellCost;
                }
            }
        }

        public void IncLine(Point p1, Point p2, int cost)
        {
            if (p1.X == p2.X)
            {
                // is vertical
                if (p1.Y < p2.Y)
                {
                    for (int y = p1.Y; y < p2.Y; y++)
                    {
                        if (this[p1.X, y].CellCost != int.MaxValue)
                        {
                            this[p1.X, y].CellCost += cost;
                        }
                    }
                }
                else
                {
                    for (int y = p2.Y; y < p1.Y; y++)
                    {
                        if (this[p1.X, y].CellCost != int.MaxValue)
                        {
                            this[p1.X, y].CellCost += cost;
                        }
                    }
                }
                return;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X < p2.X)
                {
                    for (int x = p1.X; x < p2.X; x++)
                    {
                        if (this[x, p1.Y].CellCost != int.MaxValue)
                        {
                            this[x, p1.Y].CellCost += cost;
                        }
                    }
                }
                else
                {
                    for (int x = p2.X; x < p1.X; x++)
                    {
                        if (this[x, p1.Y].CellCost != int.MaxValue)
                        {
                            this[x, p1.Y].CellCost += cost;
                        }
                    }
                }
                return;
            }

            float by = 0;
            float bx = 0;
            float m = _slope(p1, p2, ref by, ref bx);

            if (m >= -1 && m <= 1)
            {
                if (p1.X < p2.X)
                {
                    for (int x = p1.X; x < p2.X; x++)
                    {
                        int y = (int)(m * (float)x + by);
                        if (this[x, y].CellCost != int.MaxValue)
                        {
                            this[x, y].CellCost += cost;
                        }
                    }
                }
                else
                {
                    for (int x = p2.X; x < p1.X; x++)
                    {
                        int y = (int)(m * (float)x + by);
                        if (this[x, y].CellCost != int.MaxValue)
                        {
                            this[x, y].CellCost += cost;
                        }
                    }
                }
            }
            else
            {
                if (p1.Y < p2.Y)
                {
                    for (int y = p1.Y; y < p2.Y; y++)
                    {
                        int x = (int)((float)y / m + bx);
                        if (this[x, y].CellCost != int.MaxValue)
                        {
                            this[x, y].CellCost += cost;
                        }
                    }
                }
                else
                {
                    for (int y = p2.Y; y < p1.Y; y++)
                    {
                        int x = (int)((float)y / m + bx);
                        if (this[x, y].CellCost != int.MaxValue)
                        {
                            this[x, y].CellCost += cost;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the cost for the specified region to the specified cost.
        /// </summary>
        protected virtual void SetCurrCost(Rectangle CostRegion, int iCost)
        {
            int iLeft = Math.Max(CostRegion.Left, m_rWorkingRegion.Left);
            int iRight = Math.Min(CostRegion.Right, m_rWorkingRegion.Right);
            int iTop = Math.Max(CostRegion.Top, m_rWorkingRegion.Top);
            int iBottom = Math.Min(CostRegion.Bottom, m_rWorkingRegion.Bottom);
            for (int i = iLeft; i <= iRight; i++)
            {
                for (int j = iTop; j <= iBottom; j++)
                {
                    this[i, j].CurrentCost = iCost;
                }
            }
        }

        public virtual void Clear(Rectangle Obstacle)
        {
            Add(Obstacle, m_iCost);
        }

        public virtual Rectangle Goal
        {
            get { return this.m_rGoal; }
            set
            {
                m_rGoal = value;

                Rectangle rectTemp;
                rectTemp = m_rGoal;
                rectTemp.Intersect(m_rWorkingRegion);

                Clear(rectTemp);
            }
        }

        public virtual Rectangle Start
        {
            get { return m_rStart; }
            set
            {
                if (m_rStart != InvalidRectangle)
                {
                    // reset the old costs
                    SetCurrCost(new Rectangle(m_rStart.Location, new Size(0, 0)), int.MaxValue);
                }

                m_rStart = value;

                // set the current cost to zero
                SetCurrCost(new Rectangle(m_rStart.Location, new Size(0, 0)), 0);
            }
        }

        public virtual bool GoalReached(Rectangle r)
        {
            /*
             *       r.Intersect (this.m_rGoal);

                  if (r != Rectangle.Empty)
                     return true;
            */
            // I changed this from the above because if the goal is actually at the origin, 
            // it's possible to have the intersection return Rectangle.Empty and actually be the
            // Goal.  This new code causes the app to be maybe 1% slower (based on minimal ad hoc testing).
            if (this.m_rGoal.Left <= r.Right && this.m_rGoal.Right >= r.Left &&
                this.m_rGoal.Top <= r.Bottom && this.m_rGoal.Bottom >= r.Top)
                return true;

            return false;
        }

        protected struct PointCost
        {
            public Point P;
            public int Cost;
        };

        protected virtual PointCost[] GetAdjacentCosts(Point StartCell, Size ObjSize)
        {
            PointCost[] pcTemp = new PointCost[8];

            int iX = 0;
            int iY = 0;
            int iX2 = 0;
            int iY2 = 0;
            int iWidth = ObjSize.Width;
            int iHeight = ObjSize.Height;

            int iLeft = 0;
            int iRight = 0;
            int iTop = 0;
            int iBottom = 0;
            int iUL = 0;
            int iUR = 0;
            int iLL = 0;
            int iLR = 0;

            if (!IsOnGrid(StartCell.X, StartCell.Y))
                return null;

            Rectangle rectInteriorTest = new Rectangle(StartCell.X - 1, StartCell.Y - 1, ObjSize.Width + 2, ObjSize.Height + 2);
            Rectangle rectTemp = rectInteriorTest;
            rectTemp.Intersect(this.m_rWorkingRegion);
            bool bIsInterior = rectInteriorTest == rectTemp;

            iX = StartCell.X - 1;
            iX2 = StartCell.X + iWidth + 1;
            iY = StartCell.Y;
            for (int i = 0; i <= iHeight; i++, iY++)
            {
                // left edge
                if (iLeft != int.MaxValue)
                {
                    if (bIsInterior || IsOnGrid(iX, iY))
                        iLeft = Math.Max(iLeft, this[iX, iY].CellCost);
                    else
                        iLeft = int.MaxValue;
                }

                // right edge
                if (iRight != int.MaxValue)
                {
                    if (bIsInterior || IsOnGrid(iX2, iY))
                        iRight = Math.Max(iRight, this[iX2, iY].CellCost);
                    else
                        iRight = int.MaxValue;
                }
            }

            iY = StartCell.Y - 1;
            iY2 = StartCell.Y + iHeight + 1;
            iX = StartCell.X;
            for (int i = 0; i <= iWidth; i++, iX++)
            {
                // top edge
                if (iTop != int.MaxValue)
                {
                    if (bIsInterior || IsOnGrid(iX, iY))
                        iTop = Math.Max(iTop, this[iX, iY].CellCost);
                    else
                        iTop = int.MaxValue;
                }

                // bottom edge
                if (iBottom != int.MaxValue)
                {
                    if (bIsInterior || IsOnGrid(iX, iY2))
                        iBottom = Math.Max(iBottom, this[iX, iY2].CellCost);
                    else
                        iBottom = int.MaxValue;
                }
            }

            // Check the corners
            // UpperLeft
            iX = StartCell.X - 1;
            iY = StartCell.Y - 1;
            if (bIsInterior || IsOnGrid(iX, iY))
                iUL = this[iX, iY].CellCost;
            else
                iUL = int.MaxValue;

            // UpperRight
            iX = StartCell.X + iWidth + 1;
            // dup, set when checking upper left
            //iY = StartCell.Y - 1;
            if (bIsInterior || IsOnGrid(iX, iY))
                iUR = this[iX, iY].CellCost;
            else
                iUR = int.MaxValue;

            // LowerLeft
            iX = StartCell.X - 1;
            iY = StartCell.Y + iHeight + 1;
            if (bIsInterior || IsOnGrid(iX, iY))
                iLL = this[iX, iY].CellCost;
            else
                iLL = int.MaxValue;

            // LowerRight
            iX = StartCell.X + iWidth + 1;
            // dup, set when checking lower left
            // iY = StartCell.Y+ObjSize.Height + 1;
            if (bIsInterior || IsOnGrid(iX, iY))
                iLR = this[iX, iY].CellCost;
            else
                iLR = int.MaxValue;

            // Now calculate all costs.
            int iTemp = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        pcTemp[iTemp].P.X = StartCell.X + i;
                        pcTemp[iTemp].P.Y = StartCell.Y + j;
                        iTemp++;
                    }
                }
            }

            // Upper Left
            pcTemp[0].Cost = Math.Max(iLeft, Math.Max(iTop, iUL));
            if (pcTemp[0].Cost != int.MaxValue)
                pcTemp[0].Cost = (pcTemp[0].Cost * this.m_iDiagonalCost) / m_iCost;
            // Middle Left
            pcTemp[1].Cost = iLeft;
            // Lower Left
            pcTemp[2].Cost = Math.Max(iLeft, Math.Max(iBottom, iLL));
            if (pcTemp[2].Cost != int.MaxValue)
                pcTemp[2].Cost = (pcTemp[2].Cost * this.m_iDiagonalCost) / m_iCost;
            // Top Center
            pcTemp[3].Cost = iTop;
            // Bottom Center
            pcTemp[4].Cost = iBottom;
            // Upper Right
            pcTemp[5].Cost = Math.Max(iRight, Math.Max(iTop, iUR));
            if (pcTemp[5].Cost != int.MaxValue)
                pcTemp[5].Cost = (pcTemp[5].Cost * this.m_iDiagonalCost) / m_iCost;
            // Middle Right
            pcTemp[6].Cost = iRight;
            // Lower Right
            pcTemp[7].Cost = Math.Max(iRight, Math.Max(iBottom, iLR));
            if (pcTemp[7].Cost != int.MaxValue)
                pcTemp[7].Cost = (pcTemp[7].Cost * this.m_iDiagonalCost) / m_iCost;

            return pcTemp;
        }

        protected virtual int EstRemainingCost(Point C1, Size ObjSize)
        {
            if (!IsOnGrid(C1.X, C1.Y))
                return Cell.BLOCKED;

            int dX = Math.Max(0,
               Math.Abs(m_rGoal.Left - C1.X) -
               Math.Max(ObjSize.Width, m_rGoal.Width));

            int dY = Math.Max(0,
               Math.Abs(m_rGoal.Top - C1.Y) -
               Math.Max(ObjSize.Height, m_rGoal.Height));

            int iEstCost =
               (m_iCost) * (Math.Max(dX, dY) - Math.Min(dX, dY)) +
               m_iDiagonalCost * Math.Min(dX, dY);

            return iEstCost;
        }

        public virtual Cell this[int i, int j]
        {
            get
            {
                try
                {
                    return m_cCell[i - m_rWorkingRegion.Left, j - m_rWorkingRegion.Top];
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
            }
        }

        public virtual Cell this[Point P]
        {
            get
            {
                try
                {
                    return m_cCell[P.X - m_rWorkingRegion.Left, P.Y - m_rWorkingRegion.Top];
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
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
    }
}
