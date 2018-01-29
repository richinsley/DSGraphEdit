using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    public class PinUI
    {
        #region Fields

        // pin these UI elements belongs to
        private DaggerBasePin _pin;

        // style of tooltip to display for pin
        private PinToolTipStyle _pinToolTipStyle = PinToolTipStyle.NameShortType;

        // Pin's ToolTip text
        private string _toolTipText = string.Empty;

        // User defined context menu strip
        private ContextMenuStrip _userContextMenu;

        //the top left physical location of this pin in it's Parent UINode
        private Point _pinLocation = new Point(0, 0);

        //regions for hit testing and drawing
        internal Region targetRegion;

        #endregion

        #region ctor

        public PinUI(DaggerBasePin pin)
        {
            _pin = pin;
        }

        #endregion

        #region Events

        /// <summary>
        /// User defined ToolTip Popup Handler
        /// </summary>
        public event PopupEventHandler PinToolTipPopup;

        /// <summary>
        /// User defined ToolTip Draw Handler
        /// </summary>
        public event DrawToolTipEventHandler PinToolTipDraw;

        /// <summary>
        /// Allows DaggerNodeUI to call user defined PinToolTipPopup
        /// </summary>
        public void InvokeToolTipPopup(object sender, PopupEventArgs e)
        {
            if (PinToolTipPopup != null)
            {
                PinToolTipPopup(sender, e);
            }
        }

        /// <summary>
        /// Allows DaggerNodeUI to call user defined PinToolTipDraw
        /// </summary>
        public void InvokeToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            if (PinToolTipDraw != null)
            {
                PinToolTipDraw(sender, e);
            }
        }

        #endregion

        #region Properties

        public Region TargetRegion
        {
            get
            {
                return targetRegion;
            }
            set
            {
                targetRegion = value;
            }
        }

        public Point PinLocation
        {
            get
            {
                return _pinLocation;
            }
            set
            {
                _pinLocation = value;
            }
        }

        public Color NoodleColor
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].NoodleColor;
                }
                else
                {
                    return Color.Black;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Pin's ToolTip text
        /// </summary>
        public string ToolTipText
        {
            set
            {
                _toolTipText = value;
            }
            get
            {
                if (_toolTipText == string.Empty)
                {
                    return _pin.Name;
                }
                else
                {
                    return _toolTipText;
                }
            }
        }

        /// <summary>
        /// Gets or Sets how the ToolTip is Displayed
        /// </summary>
        public PinToolTipStyle PinToolTipStyle
        {
            get
            {
                return _pinToolTipStyle;
            }
            set
            {
                _pinToolTipStyle = value;
            }
        }

        /// <summary>
        /// Returns true if the user has defined PinToolTipPopup
        /// </summary>
        public bool UserDefinedToolTipPopup
        {
            get
            {
                return PinToolTipPopup != null;
            }
        }

        /// <summary>
        /// Returns true if the user has defined PinToolTipDraw
        /// </summary>
        public bool UserDefinedToolTipDraw
        {
            get
            {
                return PinToolTipDraw != null;
            }
        }

        /// <summary>
        /// Context Menu to show for this Pin
        /// </summary>
        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return _userContextMenu;
            }
            set
            {
                _userContextMenu = value;
            }
        }

        public Bitmap PinImageConnected
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    if (_pin is DaggerInputPin)
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].InputPinImageConnected;
                    }
                    else
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].OutputPinImageConnected;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public Bitmap PinImageDisconnected
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    if (_pin is DaggerInputPin)
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].InputPinImageDisconnected;
                    }
                    else
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].OutputPinImageDisconnected;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public Color PinImageConnectedTransparent
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    if (_pin is DaggerInputPin)
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].InputPinImageConnectedTransparent;
                    }
                    else
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].OutputPinImageConnectedTransparent;
                    }
                }
                else
                {
                    return Color.Red;
                }
            }
        }

        public Color PinImageDisconnectedTransparent
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    if (_pin is DaggerInputPin)
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].InputPinImageDisconnectedTransparent;
                    }
                    else
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].OutputPinImageDisconnectedTransparent;
                    }
                }
                else
                {
                    return Color.Red;
                }
            }
        }

        public Region PinConnectedRegion
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    if (_pin is DaggerInputPin)
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].InputPinRegionConnected;
                    }
                    else
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].OutputPinRegionConnected;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public Region PinDisconnectedRegion
        {
            get
            {
                if (_pin.ParentUIGraph != null)
                {
                    if (_pin is DaggerInputPin)
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].InputPinRegionDisconnected;
                    }
                    else
                    {
                        return (_pin.ParentUIGraph as DaggerUIGraph).PinLegend[_pin.DataType].OutputPinRegionDisconnected;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
