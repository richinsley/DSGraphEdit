using System;
using System.Collections.Generic;
using System.Text;

//
// Heap.cs
//
// This is a basic heap class that is used to keep track of the shortest
// paths in the AStar algorithm

namespace DaggerLib.UI.AStar
{
    // I made this an abstract class thinking that I could create some common methods
    // for all heap implementations.  However, I didn't put in all the work needed to 
    // make this useful as a base class.  So, it's somewhat unnecessary.
    public abstract class Heap
    {
        public abstract bool Add(object o);
        public abstract bool Remove(object o);
        public abstract object Pop();
        public abstract object Peek();
    }

    /// <summary>
    /// CellArray is a dynamically grown array of Cells.  I use it in the heap implementation
    /// for A* because I don't know how many Cells I'll need to put on the heap.  It's implemented
    /// as an array of arrays but it works more or less like a single dimensional array to the user.
    /// </summary>
    public class CellArray
    {
        private int m_iNodeCount = 0;
        private int m_iBlockCount;
        private const int c_iBlockSize = 256;
        private int m_iBlockSize;

        private int m_iBlockBlockCount;
        private int m_iBlockBlockSize;

        private Cell[][] m_cCells;

        public CellArray()
        {
            CellArrayInit(c_iBlockSize);
        }

        public CellArray(int BlockSize)
        {
            CellArrayInit(BlockSize);
        }

        private void CellArrayInit(int BlockSize)
        {
            m_iBlockBlockSize = BlockSize;
            m_cCells = new Cell[m_iBlockBlockSize][];
            m_iBlockBlockCount = 1;

            m_iBlockSize = BlockSize;
            m_cCells[0] = new Cell[m_iBlockSize];
            m_iBlockCount = 1;
        }

        public void Add(Cell c)
        {
            // if we've reached the end of the block, then add a new one
            if (m_iNodeCount == m_iBlockCount * m_iBlockSize)
            {
                // if we've reached the end of the block of blocks, then increase 
                // the size of the block of blocks
                if (m_iBlockCount == m_iBlockBlockCount * m_iBlockBlockSize)
                {
                    Cell[][] caTemp = new Cell[(++m_iBlockBlockCount) * m_iBlockBlockSize][];
                    for (int i = 0; i < m_iBlockCount; i++)
                        caTemp[i] = m_cCells[i];

                    m_cCells = caTemp;
                }

                // Add a new block to the array
                m_cCells[m_iBlockCount++] = new Cell[m_iBlockSize];
            }

            this[m_iNodeCount] = c;
            m_iNodeCount++;
        }

        public int Count
        {
            get { return m_iNodeCount; }
        }

        public Cell this[int iNode]
        {
            get
            {
                return m_cCells[iNode / m_iBlockSize][iNode % m_iBlockSize];
            }
            set
            {
                m_cCells[iNode / m_iBlockSize][iNode % m_iBlockSize] = value;
                if (value == null && iNode == m_iNodeCount - 1)
                    m_iNodeCount--;
            }
        }
    }

    public class AStarHeap : Heap
    {
        CellArray m_cArray;

        public AStarHeap()
        {
            m_cArray = new CellArray();
        }

        // IsLower is used by the ReHeapUp and ReHeapDown methods
        // to determine if one heap element is lower than another.
        // The IsLower_Old method was my first implementation.  But because
        // I didn't take into account the amount of turns on the current path,
        // I ended up with shortest paths that had jagged sections unneccessarily.
        private bool IsLower_Old(Cell c1, Cell c2)
        {
            if (c1.TotalCost < c2.TotalCost)
                return true;
            return false;
        }

        private bool IsLower(Cell c1, Cell c2)
        {
            // either the total cost is less,
            // or the cost is equal AND Either (turncount is lower) or (turncount is equal and current cost is greater)
            // Note: a greater current cost basically means that the attempted path is further along in finding the
            // path
            if (c1.TotalCost < c2.TotalCost ||
               ((c1.TotalCost == c2.TotalCost) &&
               ((c1.TurnCount < c2.TurnCount) ||
               ((c1.TurnCount == c2.TurnCount) &&
               (c1.CurrentCost > c2.CurrentCost))
               ))
               )
                return true;
            return false;
        }

        /// <summary>
        /// ReHeapUp starts at the last node and reheaps upward until reaching a node that is less than or 
        /// equal to the cost of the current node.
        /// Note: this is sort of 1-based.  This was done to make the calculations a bit easier.
        /// </summary>
        private void ReHeapUp()
        {
            for (int i = m_cArray.Count; i > 1; i = i / 2)
            {
                if (IsLower(m_cArray[i - 1], m_cArray[i / 2 - 1]))
                {
                    Cell pTemp = m_cArray[i - 1];
                    m_cArray[i - 1] = m_cArray[i / 2 - 1];
                    m_cArray[i / 2 - 1] = pTemp;

                    // Set the Heap Indexes
                    m_cArray[i - 1].iHeapIndex = i - 1;
                    m_cArray[i / 2 - 1].iHeapIndex = i / 2 - 1;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// ReHeapUp starts at the provided node and reheaps upward until reaching a node that is less than or 
        /// equal to the cost of the current node.
        /// 
        /// A parameterized ReHeapUp is required for cases where we need to change the cost of an item already in the 
        /// heap.  This is done when there are two different ways found of getting to the same intermediate point.
        /// Because of the nature of this implementation it is possible to reach the same node via two different 
        /// routes and have the second route be the cheaper way to get there.  Hence we need to adjust the value
        /// for a particular node and reheap up.
        /// 
        /// Note: the StartAt parameter is 0 based, but we sort of switch to a 1-base in order to 
        /// make the calculations a bit easier.
        /// </summary>
        /// <param name="iStartAt">A zero based integer used to indicate where to start the ReHeapUp</param>
        /// <returns></returns>
        public void ReHeapUp(int iStartAt)
        {
            for (int i = iStartAt + 1; i > 1; i = i / 2)
            {
                if (IsLower(m_cArray[i - 1], m_cArray[i / 2 - 1]))
                {
                    Cell pTemp = m_cArray[i - 1];
                    m_cArray[i - 1] = m_cArray[i / 2 - 1];
                    m_cArray[i / 2 - 1] = pTemp;

                    // Set the Heap Indexes
                    m_cArray[i - 1].iHeapIndex = i - 1;
                    m_cArray[i / 2 - 1].iHeapIndex = i / 2 - 1;
                }
                else
                    break;
            }
        }

        public void ReHeapDown(int iNode)
        {
            // Note: this is 1-based.  I don't know why I did that.  But it's done.
            int iLeft;
            int iRight;
            int iMin;
            int iCount = m_cArray.Count;
            Cell cLeft, cRight, cMin;

            if (iNode < 1)
                return;

            iMin = iNode;

            while (iNode < iCount) // This is just a catch all.  The real break comes
            // when iMin is on the bottom.
            {
                iLeft = iNode << 1;
                iRight = (iNode << 1) + 1;

                if (iLeft <= iCount)
                {
                    cLeft = m_cArray[iLeft - 1];
                    cMin = m_cArray[iMin - 1];
                    if (IsLower(cLeft, cMin))
                        iMin = iLeft;
                }

                if (iRight <= iCount)
                {
                    cRight = m_cArray[iRight - 1];
                    cMin = m_cArray[iMin - 1];
                    if (IsLower(cRight, cMin))
                        iMin = iRight;
                }

                // If they're equal, then we're done - it's moved to the bottom.
                if (iMin == iNode)
                    break;

                // Otherwise, we need to swap and go through the loop again.
                Cell pTemp = m_cArray[iNode - 1];
                m_cArray[iNode - 1] = m_cArray[iMin - 1];
                m_cArray[iMin - 1] = pTemp;

                // Set the heap indexes so that we know where to reheap from
                // if we just need to change the priority of a node.
                m_cArray[iNode - 1].iHeapIndex = iNode - 1;
                m_cArray[iMin - 1].iHeapIndex = iMin - 1;

                // Start from the min node
                iNode = iMin;
            }
        }

        // ReHeapDownOld is recursive.  I changed over to an iterative
        // version to make it faster.
        // Note: this is 1-based.  I don't know why I did that.  But it's done.
        public bool ReHeapDownOld(int iNode)
        {
            int iLeft = iNode << 1;
            int iRight = (iNode << 1) + 1;
            int iMin = iNode;

            if (iNode < 1)
                return false;

            if ((iLeft <= m_cArray.Count) &&
                  (m_cArray[iLeft - 1].TotalCost < m_cArray[iMin - 1].TotalCost))
                iMin = iLeft;

            if ((iRight <= m_cArray.Count) &&
                  (m_cArray[iRight - 1].TotalCost < m_cArray[iMin - 1].TotalCost))
                iMin = iRight;

            if (iMin != iNode)
            {
                Cell pTemp = m_cArray[iNode - 1];
                m_cArray[iNode - 1] = m_cArray[iMin - 1];
                m_cArray[iMin - 1] = pTemp;

                // Set the heap indexes so that we know where to reheap from
                m_cArray[iNode - 1].iHeapIndex = iNode - 1;
                m_cArray[iMin - 1].iHeapIndex = iMin - 1;

                //return ReHeapDown (iMin);
                return ReHeapDownOld(iMin);
            }
            return true;
        }

        public override bool Add(object o)
        {
            if (!(o is Cell))
                throw new ArgumentException("Object must be of type Cell");
            Add((Cell)o);
            return false;
        }

        public void Add(Cell p)
        {
            if (p == null)
                throw new ArgumentNullException("Cell p", "p cannont be null");

            // Set the index of the heap item.
            p.iHeapIndex = m_cArray.Count;

            m_cArray.Add(p);

            ReHeapUp();
        }

        public override bool Remove(object o)
        {
            if (!(o is Cell))
                throw new ArgumentException("Object must be of type Cell");
            return Remove((Cell)o);
        }

        public bool Remove(Cell p)
        {
            return false;
        }

        public override object Pop()
        {
            return PopCell();
        }

        public Cell PopCell()
        {
            if (m_cArray.Count == 0)
                return null;

            // Swap First and Last Nodes.
            Cell pTemp = m_cArray[0];
            m_cArray[0] = m_cArray[m_cArray.Count - 1];
            // Release the object
            m_cArray[m_cArray.Count - 1] = null;

            // Set the heap index for the new top node
            if (m_cArray[0] != null)
                m_cArray[0].iHeapIndex = 0;

            ReHeapDown(1);

            return pTemp;
        }

        public override object Peek()
        {
            return PeekCell();
        }

        public Cell PeekCell()
        {
            return null;
        }
    }
}
