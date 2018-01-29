using System;
using System.Drawing;

namespace DaggerLib.UI.AStar
{
    /// <summary>
    /// 
    /// </summary>
    public class AStar : Grid
    {
        private AStarHeap m_asHeap;

        // m_pCurrent is used to keep track of where the step is
        // for StepPath
        private Point m_pCurrent;
        private bool m_bStepInit;

        public AStar(Rectangle WorkingRegion,
           int StandardCost, int DiagonalCost,
           Point WorldLowerRight)
            : base(WorkingRegion,
                       StandardCost, DiagonalCost,
                       WorldLowerRight)
        {
            m_pCurrent = Point.Empty;
            m_bStepInit = false;
            m_asHeap = new AStarHeap();
        }

        public override void Reset()
        {
            m_pCurrent = Point.Empty;
            m_bStepInit = false;
            m_asHeap = new AStarHeap();
            base.Reset();
        }

        public Point[] DeterminePath(int iStopCount, ref int iCount)
        {
            int iCurrentCount = 0;
            //Point [] pCells;
            PointCost[] pcCellCosts;

            if (Start == InvalidRectangle)
                return null;

            Size ObjectSize = Start.Size;

            if (!m_bStepInit)  // if the solution was previously started
            {
                m_bStepInit = true;
                // Add all the accessible points surrounding the start point to the heap.
                // then start with the cheapest and work your way down to the target
                pcCellCosts = GetAdjacentCosts(Start.Location, ObjectSize);

                for (int i = 0; i < 8; i++)
                {
                    if (pcCellCosts[i].Cost == int.MaxValue)
                        continue;
                    this[pcCellCosts[i].P].CurrentCost = pcCellCosts[i].Cost;
                    this[pcCellCosts[i].P].BackDirection = Start.Location;
                    this[pcCellCosts[i].P].EstimatedCost = this.EstRemainingCost(pcCellCosts[i].P, ObjectSize);

                    m_asHeap.Add(this[pcCellCosts[i].P]);
                }
            }

            // Go Through every item on the heap until you find the path or hit the iStopCount.
            // Once you find the path to the goal, then break out of the loop.
            Cell c = null;
            for (iCurrentCount = 0, c = m_asHeap.PopCell();
                 c != null;
                 iCurrentCount++, c = m_asHeap.PopCell())
            {
                // if the current count is equal to the stop count, then push the cell and return
                if ((iStopCount > 0) && (iCurrentCount >= iStopCount))
                {
                    m_asHeap.Add(c);
                    break;
                }

                m_pCurrent = c.Center;

                // if we've reached the goal, then break out of the loop.
                // - added check for estimated cost == 0.  That will short circuit
                // the more expensive GoalReached function most of the time.
                if ((c.EstimatedCost == 0) && (GoalReached(new Rectangle(c.Center, ObjectSize))))
                    break;

                pcCellCosts = this.GetAdjacentCosts(c.Center, ObjectSize);
                Cell cNewCell;
                for (int i = 0; i < 8; i++)
                {
                    if ((pcCellCosts[i].Cost == int.MaxValue) ||
                        (!IsOnGrid(pcCellCosts[i].P.X, pcCellCosts[i].P.Y)))
                        continue;

                    cNewCell = this[pcCellCosts[i].P];
                    // If that node is being used, check to see if the Current Cost of that
                    // Node is greater than the cost of the current route to get there
                    int iCurrentCellCost = this[pcCellCosts[i].P].CurrentCost;
                    int iNewPathCost = c.CurrentCost + pcCellCosts[i].Cost;
                    int iTurnCount = c.TurnCount;
                    if (!IsSameLine(c.Center, c.BackDirection, pcCellCosts[i].P))
                        iTurnCount++;

                    // I needed to change this from >= to > in order to
                    // only swap those where the turn count is actually less
                    // if ((iCurrentCellCost >= iNewPathCost) ||
                    if ((iCurrentCellCost > iNewPathCost) ||
                       ((iCurrentCellCost == iNewPathCost) &&
                       (iTurnCount < cNewCell.TurnCount)))
                    {
                        cNewCell.CurrentCost = c.CurrentCost + pcCellCosts[i].Cost;
                        cNewCell.EstimatedCost = this.EstRemainingCost(pcCellCosts[i].P, ObjectSize);

                        // If this cell is in the same direction as the previous cell, then
                        // reuse the target cell's back pointer
                        if (IsSameLine(pcCellCosts[i].P, c.Center, c.BackDirection))
                        {
                            cNewCell.BackDirection = c.BackDirection;
                            cNewCell.TurnCount = this[c.Center.X, c.Center.Y].TurnCount;
                        }
                        else
                        {
                            cNewCell.BackDirection = c.Center;
                            cNewCell.TurnCount = this[c.Center.X, c.Center.Y].TurnCount + 1;
                        }

                        // test to see if this is a new cell or if we're reusing a cell
                        if (iCurrentCellCost == int.MaxValue)
                            m_asHeap.Add(cNewCell);
                        else
                            //DO REHEAPUP since the item is already in the heap
                            m_asHeap.ReHeapUp(cNewCell.iHeapIndex);
                    }
                }
            }

            iCount = iCurrentCount;

            if (c == null || this[m_pCurrent] == null)
                return null;
            
            Point[] initial = BuildPath(this[m_pCurrent]);

            // append the start and goal points
            Point[] ret = new Point[initial.Length + 2];
            ret[0] = Start.Location;
            ret[ret.Length - 1] = Goal.Location;
            for (int i = 0; i < initial.Length; i++)
            {
                ret[i + 1] = initial[i];
            }

            return ret;
        }

        private bool IsSameLine(Point P1, Point P2, Point P3)
        {
            int dX2 = P3.X - P2.X;
            int dX1 = P2.X - P1.X;
            int dY2 = P3.Y - P2.Y;
            int dY1 = P2.Y - P1.Y;
            if (
                // both slopes are infinite
               ((dX2 == 0) && (dX1 == 0)) ||
                // or both slopes are equal
               ((dX2 != 0) && (dX1 != 0) && (dY2 / dX2) == (dY1 / dX1) && (dY2 % dX2) == (dY1) % (dX1))
               )
                return true;
            return false;
        }

        private Point[] BuildPath(Cell c)
        {
            const int ciBlockSize = 32;
            Point[] pBuiltPath = null;

            Point bp = c.Center;
            int iPointCount;
            for (iPointCount = 0; bp != Start.Location; bp = this[bp.X, bp.Y].BackDirection, iPointCount++)
            {
                if (iPointCount % ciBlockSize == 0)
                {
                    Point[] pTemp = new Point[iPointCount + ciBlockSize];
                    for (int j = 0; j < iPointCount; j++)
                        pTemp[j] = pBuiltPath[j];

                    pBuiltPath = pTemp;
                }

                pBuiltPath[iPointCount] = bp;
            }

            // reverse the list
            Point[] pReversedBuiltPath = new Point[iPointCount];
            for (int i = 0; i < iPointCount; i++)
            {
                pReversedBuiltPath[i] = pBuiltPath[iPointCount - i - 1];
            }
            return pReversedBuiltPath;
        }
    }
}
