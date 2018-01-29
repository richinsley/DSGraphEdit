using System;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Core;

namespace DaggerLib.Interfaces
{
    public interface IDaggerUINode
    {
        DaggerNode Node
        {
            get;
            set;
        }

        IDaggerUIGraph ParentUIGraph
        {
            get;
        }

        bool IsProcessing
        {
            get;
            set;
        }

        int Width
        {
            get;
            set;
        }

        int Height
        {
            get;
            set;
        }

        int Left
        {
            get;
            set;
        }

        int Top
        {
            get;
            set;
        }

        int PinSize
        {
            get;
        }

        string CaptionText
        {
            get;
            set;
        }

        void CalculateLayout();

        void DoUIProcessing();
    }
}
