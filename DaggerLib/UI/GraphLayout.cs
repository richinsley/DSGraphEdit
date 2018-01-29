using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

using DaggerLib.Interfaces;

namespace DaggerLib.UI
{
    /// <summary>
    /// Class to serialize UI layout with a DaggerGraph.  Make sure overidden class is marked Serializable.
    /// </summary>
    [Serializable]
    public class GraphLayout
    {
        public Guid targetNodeGuid;
        public int Width;
        public int Height;
        public int Top;
        public int Left;
        public bool Expanded = false;
     
        public GraphLayout(IDaggerUINode uinode)
        {
            targetNodeGuid = uinode.Node.InstanceGuid;
            Width = uinode.Width;
            Height = uinode.Height;
            Top = uinode.Top;
            Left = uinode.Left;
        }

        public virtual void Apply(IDaggerUINode uinode)
        {
            uinode.Width = Width;
            uinode.Height = Height;
            uinode.Top = Top;
            uinode.Left = Left;
        }

        public virtual void Apply(IDaggerUINode uinode, int offsetX, int offsetY)
        {
            uinode.Width = Width;
            uinode.Height = Height;
            uinode.Top = Top + offsetY;
            uinode.Left = Left + offsetX;
        }
    }
}
