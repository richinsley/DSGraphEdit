using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using DaggerLib.UI.Windows;
using System.Drawing;

namespace DaggerLib.DSGraphEdit
{
    [Serializable]
    public class DSGraphEditPanelProperties
    {
        public bool DropShadowVisible = true;
        public DaggerNodePinPlacement PinPlacement = DaggerNodePinPlacement.Outset;
        public bool ModalProperties = true;
        public Color CanvasBackColor = Color.Teal;
        public bool ShowTimeSlider = true;
        public bool ShowPinNames = false;
        public NoodleStyle NoodleStyle = NoodleStyle.Default;
    }
}
