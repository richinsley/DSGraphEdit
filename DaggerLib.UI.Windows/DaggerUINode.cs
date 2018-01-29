using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using DaggerLib.Interfaces;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    public class DaggerUINode : UserControl , IDaggerUINode
    {
        #region Fields

        //the parent UIGraph that holds this UINode
        internal DaggerUIGraph _parentGraph;

        //flag indicating the Control is fully initialized
        private bool _isIntialized = false;

        //the node this UI element represents
        internal DaggerNode _node;

        //the node that was created by the designer
        private DaggerNode _designtimeNode;

        //collection of user defined buttons in the caption
        private CaptionButtonCollection _captionButtons;

        //the state of the node we can alter depending on mouse locations or other conditions
        private DaggerNodeAlterState _canAlterState = DaggerNodeAlterState.None;

        /// <summary>
        /// Where we should place the pins on the panels
        /// </summary>
        private DaggerNodePinPlacement _pinPlacement = DaggerNodePinPlacement.Outset;

        /// <summary>
        /// Size in pixels to shrink internal Control
        /// </summary>
        private int _internalControlPadding = 5;

        /// <summary>
        /// Radius for the rounded corners
        /// </summary>
        private float _panelRadius = 9f;

        /// <summary>
        /// The height in Pixels of the caption
        /// </summary>
        private int _captionSize = 14;

        /// <summary>
        /// How far in the indentation curve for the pins should be
        /// </summary>
        private float _pinIndent = 1f;

        /// <summary>
        /// Can we resize the panel?
        /// </summary>
        private bool _resizable = false;

        /// <summary>
        /// The caption
        /// </summary>
        private string _captionText;

        /// <summary>
        /// Caption Colors
        /// </summary>
        private Color _captionColor = Color.Blue;
        private Color _captionColorUnfocused = Color.LightSteelBlue;
        private bool _isFocused;

        /// <summary>
        /// The Backgound image for the node
        /// </summary>
        private Bitmap _backImage;

        /// <summary>
        /// Paths for drawing the Caption and Panel
        /// </summary>
        private GraphicsPath _clientPath;
        private GraphicsPath _captionPath;

        // path for resize widget
        private GraphicsPath _resizePath;

        //the spacing between the pins
        private int _pinSpacing = 2;

        //the pin that the cursor is currently over
        private DaggerBasePin _mouseOverPin;

        //the internal panel of the UINode
        protected Panel _internalControl;

        //the internal control centered in the node (defaults to a panel)

        //panel icon
        private Bitmap _panelIcon;

        //the pin context menu
        private BasePinContextMenuStrip _pinContextMenu;

        //the user defined context menu
        private ContextMenuStrip _userContextMenu;

        //our internal ContextMenuStrip items and the list that holds them
        private List<ToolStripItem> _internalContextMenuItems;
        private ToolStripSeparator _ictxtSeparator;
        private ToolStripMenuItem _ictxtCreateSubNode;
        private ToolStripMenuItem _ictxtDeleteRelink;
        private ToolStripMenuItem _ictxtProcessNode;

        //our ToolTip thingy
        private System.Windows.Forms.ToolTip _wndToolTip;

        //our ToolTip text
        private string _tooltiptext = "";

        private bool _isProcessing = false;


        #endregion

        private IContainer components;

        #region Target Regions

        /// <summary>
        /// The client area for the caption
        /// </summary>
        private Region _targetRegionCaption;

        /// <summary>
        /// Target region for resize widget
        /// </summary>
        private Region _targetRegionResize;

        #endregion

        #region ctor

        public DaggerUINode()
        {
            //set the usercontrol up for doublebuffered drawing
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            _captionButtons = new CaptionButtonCollection();
            _captionButtons.ButtonAdded += new EventHandler(_captionButtons_ButtonAdded);
            _captionButtons.ButtonRemoved += new EventHandler(_captionButtons_ButtonRemoved);
            _captionButtons.ButtonVisibleChanged += new EventHandler(_captionButtons_ButtonVisibleChanged);

            //create the context menu to interact with pins
            _pinContextMenu = new BasePinContextMenuStrip();

            //create and set the Design Time node.
            //this will most likely be replaced by a decendent, but we never want it to be null
            _designtimeNode = new DaggerNode();
            Node = _designtimeNode;

            //create the ToolTip control
            _wndToolTip = new ToolTip();
            _wndToolTip.Active = true;

            InitializeComponent();

            //set up our ToolTip control            
            _wndToolTip.SetToolTip(this, "DaggerNode");
            _wndToolTip.SetToolTip(_internalControl, "DaggerNode");            
            _wndToolTip.AutomaticDelay = 500;
            _wndToolTip.ReshowDelay = 100;
            _wndToolTip.OwnerDraw = true;
            _wndToolTip.Popup += new PopupEventHandler(m_wndToolTip_Popup);
            _wndToolTip.Draw += new DrawToolTipEventHandler(m_wndToolTip_Draw);

            //create the internal contextmenustrip items
            _internalContextMenuItems = new List<ToolStripItem>();
            _ictxtSeparator = new ToolStripSeparator();
            _internalContextMenuItems.Add(_ictxtSeparator);

            _ictxtCreateSubNode = new ToolStripMenuItem("Create SubNode");
            _ictxtCreateSubNode.Click += new EventHandler(_ictxtCreateSubNode_Click);
            _internalContextMenuItems.Add(_ictxtCreateSubNode);

            _ictxtDeleteRelink = new ToolStripMenuItem("Delete and Relink");
            _ictxtDeleteRelink.Click += new EventHandler(_ictxtDeleteRelink_Click);
            _internalContextMenuItems.Add(_ictxtDeleteRelink);

            _ictxtProcessNode = new ToolStripMenuItem("Process Node");
            _ictxtProcessNode.Click += new EventHandler(_ictxtProcessNode_Click);
            _internalContextMenuItems.Add(_ictxtProcessNode);

            // we want this true so we can drag pins to connect them
            this.AllowDrop = true;

            // add the internal Control
            this.Controls.Add(_internalControl);

            // hook up to internal panel's click so we can track focus
            _internalControl.Click += new EventHandler(_internalPanel_Click);

            // generate all the paths, regions, and backimage
            CalculateLayout();

            _isIntialized = true;
        }

        private void InitializeComponent()
        {
            this._internalControl = new DoubleBufferedPanel();
            this.SuspendLayout();
            // 
            // _internalControl
            // 
            this._internalControl.Location = new System.Drawing.Point(0, 0);
            this._internalControl.Name = "_internalControl";
            this._internalControl.Size = new System.Drawing.Size(611, 418);
            this._internalControl.TabIndex = 0;
            this._internalControl.Tag = "Internal";
            // 
            // DaggerUINode
            // 
            this.Name = "DaggerUINode";
            this.Size = new System.Drawing.Size(103, 72);
            this.ContextMenuStripChanged += new System.EventHandler(this.DaggerUINode_ContextMenuStripChanged);
            this.ResumeLayout(false);

        }

        #endregion

        #region Events and Handlers

        /// <summary>
        ///  Event that is raised when the UI Node is Activated
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// Event thay is raised when the UI Node is Deactivated
        /// </summary>
        public event EventHandler Deactivated;

        void _captionButtons_ButtonRemoved(object sender, EventArgs e)
        {
            Controls.Remove(sender as SimpleImageButton);
            ArrangeCaptionButtons();
        }

        void _captionButtons_ButtonAdded(object sender, EventArgs e)
        {
            (sender as SimpleImageButton).Size = new Size(_captionSize, _captionSize);
            Controls.Add(sender as SimpleImageButton);
            ArrangeCaptionButtons();
        }

        void _captionButtons_ButtonVisibleChanged(object sender, EventArgs e)
        {
            ArrangeCaptionButtons();
        }

        void _ictxtProcessNode_Click(object sender, EventArgs e)
        {
            _node.Process();
        }

        void _ictxtStackSelectedNodes_Click(object sender, EventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Event that is raised when the user Clicks "Create SubNode"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ictxtCreateSubNode_Click(object sender, EventArgs e)
        {
            byte[] subnode = _node.ParentGraph.SerializeSubGraph(_node.SubgraphAffiliation);

            if (subnode != null)
            {
                if (_parentGraph.DaggerNodeTreeView != null)
                {
                    ValueEditorDialog vue = new ValueEditorDialog("Subnode Name","Subnode");
                    if (vue.ShowDialog() == DialogResult.OK)
                    {
                        _parentGraph.DaggerNodeTreeView.AddSubNode("User functions", (string)vue.Data, false, subnode);
                    }
                }
                else
                {
                    ValueEditorDialog vue = new ValueEditorDialog("Subnode Name", "Subnode");
                    if (vue.ShowDialog() == DialogResult.OK)
                    {
                        _node.ParentGraph.AddNode(new DaggerSubNode((string)vue.Data,subnode));
                    }
                }
            }
        }

        /// <summary>
        /// Event that is raised when user selects "Delete and Relink"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ictxtDeleteRelink_Click(object sender, EventArgs e)
        {
            _parentGraph.DeleteAndRelink(_node);
        }

        /// <summary>
        /// Event that is raised when a DaggerNode is attached to this UINode
        /// </summary>
        public event DaggerNodeAttachedHandler DaggerNodeAttached;

        /// <summary>
        /// Event that is raised before a pin shows it's context menu
        /// </summary>
        public event DaggerBasePinBeforeShowContextMenuHandler BeforePinContextShown;

        /// <summary>
        /// Set the focus of the Node if clicked
        /// </summary>
        void _internalPanel_Click(object sender, EventArgs e)
        {
            this.Focus();
        }

        //Events to Handle the Adding/Removing of pins from the collections
        void _outputPins_PinRemoved(object sender, DaggerBasePin pin)
        {
            CalculateLayout();
        }
        void _outputPins_PinAdded(object sender, DaggerBasePin pin)
        {
            CreatePinUIElements(pin);
            CalculateLayout();
        }
        void _inputPins_PinRemoved(object sender, DaggerBasePin pin)
        {
            CalculateLayout();
        }
        void _inputPins_PinAdded(object sender, DaggerBasePin pin)
        {
            CreatePinUIElements(pin);
            CalculateLayout();
        }

        /// <summary>
        /// Adjust ToolTip size to reflect desired text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_wndToolTip_Popup(object sender, PopupEventArgs e)
        {
            using (Font f = new Font("Tahoma", 9))
            {
                if (_mouseOverPin != null)
                {
                    //if the user has defined PinToolTipPopup, call that instead
                    if ((_mouseOverPin.PinUIElements as PinUI).UserDefinedToolTipPopup)
                    {
                        (_mouseOverPin.PinUIElements as PinUI).InvokeToolTipPopup(_mouseOverPin, e);
                    }
                    else
                    {
                        //measure the strings for pin name and pin data type
                        Size tts = TextRenderer.MeasureText(_mouseOverPin.Name, f);
                        Size typeSize = new Size();

                        switch ((_mouseOverPin.PinUIElements as PinUI).PinToolTipStyle)
                        {
                            case PinToolTipStyle.Name:
                                break;
                            case PinToolTipStyle.NameShortType:
                                typeSize = TextRenderer.MeasureText(_mouseOverPin.DataType.Name, f);
                                tts.Width = Math.Max(tts.Width, typeSize.Width);
                                tts.Height = tts.Height * 2 + 5;
                                break;
                            case PinToolTipStyle.NameLongType:
                                typeSize = TextRenderer.MeasureText( _mouseOverPin.DataType.FullName, f);
                                tts.Width = Math.Max(tts.Width, typeSize.Width);
                                tts.Height = tts.Height * 2 + 5;
                                break;
                            default:
                                break;
                        }

                        e.ToolTipSize = tts;
                    }
                }
                else if (_tooltiptext != "")
                {
                    e.ToolTipSize = TextRenderer.MeasureText(
                        _tooltiptext + (_parentGraph.ShowOrdinal?": Ordinal " + _node.Ordinal.ToString():""), f);
                }
                else
                {
                    e.ToolTipSize = TextRenderer.MeasureText(
                        _captionText + (_parentGraph.ShowOrdinal ? ": Ordinal " + _node.Ordinal.ToString() : ""), f);
                }
            }
        }

        /// <summary>
        /// Draw the ToolTip with the desired text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_wndToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            //if the user has defined PinToolTipPopup, call that instead
            if (_mouseOverPin != null && (_mouseOverPin.PinUIElements as PinUI).UserDefinedToolTipDraw)
            {
                (_mouseOverPin.PinUIElements as PinUI).InvokeToolTipDraw(_mouseOverPin, e);
            }
            else
            {
                // what text shall we draw?
                string theText = (_mouseOverPin != null) ? _mouseOverPin.Name :
                    ((_tooltiptext == "") ? _captionText : _tooltiptext) + (_parentGraph.ShowOrdinal ? ": Ordinal " + _node.Ordinal.ToString() : "");

                Color textColor = SystemColors.ActiveCaptionText;

                if (_mouseOverPin != null)
                {
                    textColor = ControlPaint.Light((_mouseOverPin.PinUIElements as PinUI).NoodleColor);

                    using (Pen ddp = new Pen((_mouseOverPin.PinUIElements as PinUI).NoodleColor))
                    {
                        using (Brush ddb = new SolidBrush(ControlPaint.Dark((_mouseOverPin.PinUIElements as PinUI).NoodleColor)))
                        {

                            e.Graphics.FillRectangle(ddb, e.Bounds);

                            switch ((_mouseOverPin.PinUIElements as PinUI).PinToolTipStyle)
                            {
                                case PinToolTipStyle.Name:
                                    break;
                                case PinToolTipStyle.NameShortType:
                                    theText += "\n" + _mouseOverPin.DataType.Name;
                                    //draw serperator line
                                    e.Graphics.DrawLine(ddp, 0, e.Bounds.Height / 2, e.Bounds.Width, e.Bounds.Height / 2);
                                    break;
                                case PinToolTipStyle.NameLongType:
                                    theText += "\n" + _mouseOverPin.DataType.FullName;
                                    //draw serperator line
                                    e.Graphics.DrawLine(ddp, 0, e.Bounds.Height / 2, e.Bounds.Width, e.Bounds.Height / 2);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    // Draw the custom background.
                    e.Graphics.FillRectangle(SystemBrushes.ActiveCaption, e.Bounds);
                }

                // Draw the standard border.
                e.DrawBorder();

                // Draw the custom text.
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                    using (Font f = new Font("Tahoma", 9))
                    {
                        using (Brush textBrush = new SolidBrush(textColor))
                        {
                            e.Graphics.DrawString(theText, f,
                                textBrush, e.Bounds, sf);
                        }
                    }
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this node has focus (use instead of this.Focused)
        /// </summary>
        [Browsable(false)]
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public CaptionButtonCollection CaptionButtons
        {
            get
            {
                return _captionButtons;
            }
        }

        /// <summary>
        /// Gets the parent ui graph this ui node belongs to
        /// </summary>
        [Browsable(false)]
        public IDaggerUIGraph ParentUIGraph
        {
            get
            {
                return (IDaggerUIGraph)_parentGraph;
            }
        }

        /// <summary>
        /// Gets or sets the underlying DaggerNode for this UI element
        /// </summary>
        [Browsable(false)]
        public DaggerNode Node
        {
            get
            {
                return _node;
            }
            set
            {
                if (value != null)
                {
                    _node = value;

                    // merge the designtime node's pins with this node
                    foreach (DaggerOutputPin pin in _designtimeNode.OutputPins)
                    {
                        if (!_node.OutputPins.Contains(pin))
                        {
                            _node.OutputPins.Add(pin);
                        }
                    }
                    foreach (DaggerInputPin pin in _designtimeNode.InputPins)
                    {
                        if (!_node.InputPins.Contains(pin))
                        {
                            _node.InputPins.Add(pin);
                        }
                    }

                    // create the UI Elements for the pins
                    foreach (DaggerOutputPin pin in _node.OutputPins)
                    {
                        CreatePinUIElements(pin);
                    }
                    foreach (DaggerInputPin pin in _node.InputPins)
                    {
                        CreatePinUIElements(pin);
                    }

                    //set the default caption
                    CaptionText = _node.ToString();

                    //let the node know this UI is controlling it
                    _node.UINode = this as IDaggerUINode;

                    //hook the events to the Pin Collections
                    _node.InputPins.PinAdded += new DaggerPinAdded(_inputPins_PinAdded);
                    _node.InputPins.PinRemoved += new DaggerPinRemoved(_inputPins_PinRemoved);

                    _node.OutputPins.PinAdded += new DaggerPinAdded(_outputPins_PinAdded);
                    _node.OutputPins.PinRemoved += new DaggerPinRemoved(_outputPins_PinRemoved);

                    //only do this if we are fully initialized
                    if (_isIntialized)
                    {
                        CalculateLayout();

                        //raise the Attached Events
                        if (DaggerNodeAttached != null)
                        {
                            DaggerNodeAttached(_node);
                        }
                        _node.InvokeUINodeAttached(this as IDaggerUINode);
                    }
                }
            }
        }

        [Browsable(false)]
        public int PinSize
        {
            get
            {
                if (_parentGraph != null)
                {
                    return _parentGraph.PinLegend.PinSize;
                }
                else
                {
                    return 11;
                }
            }
        }

        /// <summary>
        /// The Internal Control or Container
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Panel InternalControl
        {
            get
            {
                return this._internalControl;
            }
        }

        public bool IsProcessing
        {
            get
            {
                return _isProcessing;
            }
            set
            {
                _isProcessing = value;
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new MethodInvoker(delegate()
                    {
                        DrawCaption(null);
                        //only update the caption area
                        Invalidate(_targetRegionCaption);
                    }));
                }
                else
                {
                    DrawCaption(null);
                    //only update the caption area
                    Invalidate(_targetRegionCaption);
                }
            }
        }

        /// <summary>
        /// Nodes that are directed into this node
        /// </summary>
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DaggerPinCollection<DaggerInputPin> InputPins
        {
            get
            {
                return _node.InputPins;
            }
        }

        /// <summary>
        /// Adjacency list for this node
        /// </summary>
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DaggerPinCollection<DaggerOutputPin> OutputPins
        {
            get
            {
                return _node.OutputPins;
            }
        }

        public Bitmap CaptionIcon
        {
            get
            {
                return _panelIcon;
            }
            set
            {
                _panelIcon = value;

                DrawCaption(null);
                //only update the caption area
                this.Invalidate(_targetRegionCaption);
            }
        }

        public Color CaptionColor
        {
            get
            {
                return _captionColor;
            }
            set
            {
                _captionColor = value;
                if (this.Focused)
                {
                    DrawCaption(null);
                    //only update the caption area
                    this.Invalidate(_targetRegionCaption);
                }
            }
        }

        public Color CaptionColorUnfocused
        {
            get
            {
                return _captionColorUnfocused;
            }
            set
            {
                _captionColorUnfocused = value;
                if (!this.Focused)
                {
                    DrawCaption(null);
                    //only update the caption area
                    this.Invalidate(_targetRegionCaption);
                }
            }
        }

        public int CaptionSize
        {
            get
            {
                return _captionSize;
            }
            set
            {
                _captionSize = value;
                CalculateLayout();
            }
        }

        /// <summary>
        /// Get or Set the radius of the rounded edges
        /// </summary>
        public float PanelRadius
        {
            get
            {
                return _panelRadius;
            }
            set
            {
                _panelRadius = value;
                CalculateLayout();
            }
        }

        public DaggerNodePinPlacement PinPlacement
        {
            get
            {
                return _pinPlacement;
            }
            set
            {
                _pinPlacement = value;
                CalculateLayout();
            }
        }

        /// <summary>
        /// Text that is displayed in the Caption
        /// </summary>
        public string CaptionText
        {
            get
            {
                return _captionText;
            }
            set
            {
                _captionText = value;

                try
                {
                    //recreate caption
                    DrawCaption(null);

                    //only update the caption area
                    this.Invalidate(_targetRegionCaption);
                }
                catch (Exception ex)
                {
                    // the uinode may not be fully formed yet
                }
            }
        }

        /// <summary>
        /// Gets the minimum size of control to allow pins, caption, and caption buttons to be visible
        /// </summary>
        public Size NodeMinimumSize
        {
            get
            {
                int neededPinSize = InputPins.Count * (_parentGraph.PinLegend.PinSize + _pinSpacing) + (_captionSize) + _internalControlPadding;
                neededPinSize = Math.Max(neededPinSize, OutputPins.Count * (_parentGraph.PinLegend.PinSize + _pinSpacing) + (_captionSize)) + _internalControlPadding;

                Graphics g = Graphics.FromHwnd(Handle);
                int capsize = (int)g.MeasureString(_captionText, Font).Width + 2 + _captionButtons.Count * _captionSize;
                switch (_pinPlacement)
                {
                    case DaggerNodePinPlacement.Indent:
                    case DaggerNodePinPlacement.Inset:
                        {
                            capsize += _pinSpacing * 2;
                        }
                        break;
                    case DaggerNodePinPlacement.Outset:
                        {
                            capsize += _pinSpacing * 2 + (_parentGraph.PinLegend.PinSize * 2);
                        }
                        break;
                    default:
                        break;
                }
                if (CaptionIcon != null)
                {
                    capsize += _captionSize + 2;
                }

                return new Size((int)Math.Max(capsize , InternalControlMinimumSize.Width + (PinSize * 2)), neededPinSize + InternalControlMinimumSize.Height);
            }
        }

        /// <summary>
        /// Get the minimum size for the internal control
        /// </summary>
        public virtual Size InternalControlMinimumSize
        {
            get
            {
                return new Size(0, 0);
            }
        }

        /// <summary>
        /// Gets the area the node occupies to calculate auto-arranging
        /// </summary>
        public virtual Size AutoArrangeSize
        {
            get
            {
                return this.Size;
            }
        }

        /// <summary>
        /// Gets the offset the node should placed during auto-arranging
        /// </summary>
        public virtual Point AutoArrangeOffset
        {
            get
            {
                return new Point(0, 0);
            }
        }

        public bool ToolTipActive
        {
            get
            {
                return _wndToolTip.Active;
            }
            set
            {
                _wndToolTip.Active = value;
            }
        }

        public string ToolTipText
        {
            get
            {
                return _tooltiptext;
            }
            set
            {
                _tooltiptext = value;
            }
        }

        /// <summary>
        /// Distance between the internal panel and eddges of node
        /// </summary>
        public int InternalControlPadding
        {
            get
            {
                return _internalControlPadding;
            }
            set
            {
                _internalControlPadding = value;
                CalculateLayout();
            }
        }

        /// <summary>
        /// The vertical padding between pins
        /// </summary>
        public int PinSpacing
        {
            get
            {
                return _pinSpacing;
            }
            set
            {
                _pinSpacing = value;
                CalculateLayout();
            }
        }

        /// <summary>
        /// How far to curve the node's sides when pins are present
        /// </summary>
        public float PinBevelIndent
        {
            get
            {
                return _pinIndent;
            }
            set
            {
                _pinIndent = Math.Max(0.6f, value);
                CalculateLayout();
            }
        }

        /// <summary>
        /// Gets or sets if the UINode can be resized
        /// </summary>
        public bool Resizable
        {
            get
            {
                return _resizable;
            }
            set
            {
                _resizable = value;
                CalculateLayout();
            }
        }

        /// <summary>
        /// Menu strip to show for this Node
        /// </summary>
        public new ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return _userContextMenu;
            }
            set
            {
                _userContextMenu = value;
                _userContextMenu.Opening += new CancelEventHandler(_userContextMenu_Opening);
                _userContextMenu.Closed += new ToolStripDropDownClosedEventHandler(_userContextMenu_Closed);
            }
        }

        #region ContextMenuStrip Merging

        // we want to merge our menu items with the User defined ctxt menu
        // but we don't want to keep them there when not displaying it.
        // This is to prevent the end-user from getting confused if they use indexers
        // on the ToolStripItemCollection and are unaware that we shoe-horned in some extra menu items.
        void _userContextMenu_Opening(object sender, CancelEventArgs e)
        {
            //add our menu items
            foreach (ToolStripItem ti in _internalContextMenuItems)
            {
                _userContextMenu.Items.Add(ti);
            }

            ValidateContextMenuStrip(_userContextMenu);
        }

        void _userContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            //remove our menu items
            foreach (ToolStripItem ti in _internalContextMenuItems)
            {
                _userContextMenu.Items.Remove(ti);
            }
        }

        #endregion

        #endregion

        #region Overrides

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            _parentGraph = Parent as DaggerUIGraph;

            //refresh to use the PinLegend of this the new UIGraph
            CalculateLayout();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //see if we are over a pin
            _canAlterState = CanConnectToPin(e.Location);

            //move zone
            if (_targetRegionCaption.IsVisible(e.Location))
            {
                _canAlterState = DaggerNodeAlterState.Move;
            }

            if (_resizable && _targetRegionResize.IsVisible(e.Location))
            {
                _canAlterState = DaggerNodeAlterState.SouthEast;
            }

            switch (_canAlterState)
            {
                case DaggerNodeAlterState.CanConnectToInput:
                    {
                        Bitmap cursorimage = (Bitmap)_parentGraph.CursorImages.Images[1];
                        Cursor = new Cursor(cursorimage.GetHicon());
                    }
                    break;
                case DaggerNodeAlterState.CanConnectToOutput:
                    {
                        Bitmap cursorimage = (Bitmap)_parentGraph.CursorImages.Images[0];
                        Cursor = new Cursor(cursorimage.GetHicon());
                    }
                    break;
                case DaggerNodeAlterState.ConnectFromOutput:
                    {
                        Bitmap cursorimage = (Bitmap)_parentGraph.CursorImages.Images[1];
                        Cursor = new Cursor(cursorimage.GetHicon());
                    }
                    break;
                case DaggerNodeAlterState.ConnectFromInput:
                    {
                        Bitmap cursorimage = (Bitmap)_parentGraph.CursorImages.Images[0];
                        Cursor = new Cursor(cursorimage.GetHicon());
                    }
                    break;
                case DaggerNodeAlterState.None:
                    Cursor = Cursors.Default;
                    break;
                case DaggerNodeAlterState.Context:
                    Cursor = Cursors.Default;
                    break;
                case DaggerNodeAlterState.Move:
                    Cursor = Cursors.Default;
                    break;
                case DaggerNodeAlterState.SouthEast:
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    Cursor = Cursors.Default;
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //execute the _canAlterState
                switch (_canAlterState)
                {
                    case DaggerNodeAlterState.Move:
                        {
                            DaggerOverlay eol = null;

                            // if more than one control is selected do a multi-control DaggerOverlay
                            if ((ParentUIGraph as DaggerUIGraph)._trackingSelector != null && (ParentUIGraph as DaggerUIGraph)._trackingSelector.SelectedNodes.Contains(this as IDaggerUINode))
                            {
                                // convert the list of DaggerUINodes to Controls
                                List<Control> tl = new List<Control>();
                                foreach (DaggerUINode nn in (ParentUIGraph as DaggerUIGraph)._trackingSelector.SelectedNodes)
                                {
                                    tl.Add((Control)nn);
                                }
                                eol = new DaggerOverlay(tl, PointToScreen(e.Location));
                            }
                            else
                            {
                                eol = new DaggerOverlay(this, _canAlterState, PointToScreen(e.Location), NodeMinimumSize.Width, NodeMinimumSize.Height);
                            }

                            Cursor = Cursors.SizeAll;
                            eol.BeginOperations();
                        }
                        break;
                    case DaggerNodeAlterState.SouthEast:
                        {
                            DaggerOverlay eol = new DaggerOverlay(this, _canAlterState, PointToScreen(e.Location), NodeMinimumSize.Width, NodeMinimumSize.Height);
                            eol.BeginOperations();
                        }
                        break;
                    case DaggerNodeAlterState.ConnectFromOutput:
                        if (_parentGraph != null)
                        {
                            _parentGraph.BeginOutputPinConnect((DaggerOutputPin)_mouseOverPin);
                            DoDragDrop(this, DragDropEffects.All);
                        }
                        break;
                    case DaggerNodeAlterState.ConnectFromInput:
                        if (_parentGraph != null)
                        {
                            _parentGraph.BeginInputPinConnect((DaggerInputPin)_mouseOverPin);
                            DoDragDrop(this, DragDropEffects.All);
                        }
                        break;
                    case DaggerNodeAlterState.CanConnectToOutput:
                    case DaggerNodeAlterState.CanConnectToInput:
                        {
                            if (_mouseOverPin is DaggerInputPin)
                            {
                                _parentGraph.EndPinConnect((DaggerInputPin)_mouseOverPin);
                            }
                            else
                            {
                                _parentGraph.EndPinConnect((DaggerOutputPin)_mouseOverPin);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //musta been a right click
                if (_mouseOverPin != null)
                {
                    _pinContextMenu.Tag = _mouseOverPin;
                    if (BeforePinContextShown != null)
                    {
                        BeforePinContextShown(_mouseOverPin);
                    }

                    // stop pin connections
                    _parentGraph.StopPinConnect();
                    _mouseOverPin = null;
                    Cursor = Cursors.Default;
                    _canAlterState = DaggerNodeAlterState.None;

                    _pinContextMenu.Show(PointToScreen(e.Location));
                }
                else
                {
                    if (_userContextMenu != null)
                    {
                        _userContextMenu.Show(PointToScreen(e.Location));
                    }
                    else
                    {
                        //no user defined ContextMenu so create and show a temp one with _internalContextMenuItems 
                        ContextMenuStrip tcon = new ContextMenuStrip();
                        foreach (ToolStripItem ti in _internalContextMenuItems)
                        {
                            tcon.Items.Add(ti);
                        }
                                                
                        ValidateContextMenuStrip(tcon);

                        tcon.Show(PointToScreen(e.Location));
                    }
                }
            }

            base.OnMouseDown(e);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // We use a combination of MouseDown, MouseMove AND DragDrop to allow the user 
        // the choice of clicking pins or dragging them to connect.
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// See if the user is dragging a pin into this node
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            // since we cancel Noodle dragging whenever the topmost form loses focus,
            // we know that if TrackingPin != null, we are dragging a pin and don't
            // have to bother with serializing and type conversion of Pins.

            if (_parentGraph != null)
            {
                if (_parentGraph.TrackingPin != null)
                {
                    // Disallow a node from connecting to itself
                    if ((_parentGraph.TrackingPin.ParentNode != null && _parentGraph.TrackingPin.ParentNode.UINode != this) || _parentGraph.TrackingPin.ParentNode == null)
                    {
                        //convert the screen space to client space
                        Point clientPoint = PointToClient(new Point(drgevent.X, drgevent.Y));

                        //get the alterState
                        DaggerNodeAlterState alterState = CanConnectToPin(clientPoint);

                        //can we allow the dragged pin to be dropped here?
                        if (_parentGraph.TrackingPin is DaggerOutputPin)
                        {
                            if (alterState == DaggerNodeAlterState.CanConnectToInput)
                            {
                                _canAlterState = alterState;
                                drgevent.Effect = DragDropEffects.All;
                            }
                            else
                            {
                                drgevent.Effect = DragDropEffects.None;
                            }
                        }
                        else
                        {
                            if (alterState == DaggerNodeAlterState.CanConnectToOutput)
                            {
                                _canAlterState = alterState;
                                drgevent.Effect = DragDropEffects.All;
                            }
                            else
                            {
                                drgevent.Effect = DragDropEffects.None;
                            }
                        }
                    }
                    else
                    {
                        drgevent.Effect = DragDropEffects.None;
                    }
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            switch (_canAlterState)
            {
                case DaggerNodeAlterState.CanConnectToOutput:
                case DaggerNodeAlterState.CanConnectToInput:
                    {
                        if (_parentGraph != null)
                        {
                            if (_mouseOverPin is DaggerInputPin)
                            {
                                _parentGraph.EndPinConnect((DaggerInputPin)_mouseOverPin);
                            }
                            else
                            {
                                _parentGraph.EndPinConnect((DaggerOutputPin)_mouseOverPin);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called when resizing, or when we need to Calcluate Layout
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CalculateLayout();

            if (_parentGraph != null)
            {
                _parentGraph.NodeResized(_node);
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);

            if (_parentGraph != null)
            {
                _parentGraph.NodeMoved(this._node);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_backImage == null)
            {
                CreateBackImage(e.ClipRectangle);
            }
            e.Graphics.DrawImage(_backImage, 0, 0);
        }

        private void DaggerUINode_ContextMenuStripChanged(object sender, EventArgs e)
        {
            //if it's not PinContextMenuStrip, store it
            if (sender != _pinContextMenu && sender != _userContextMenu)
            {
                _userContextMenu = sender as ContextMenuStrip;
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            // the AutoScroll properies are too AUTO.  Prevent the DaggerUIGraph from scrolling a UINode
            // into view when it's entered.  This is a clever hack by Jim Nakashima:
            // http://seewinapp.blogspot.com/2005/09/is-your-autoscroll-too-auto.html
            if (Parent is DaggerUIGraph)
            {
                (Parent as DaggerUIGraph)._isScrolling = true;
                Point p = (Parent as DaggerUIGraph).AutoScrollPosition;
                AutoScrollPositionDelegate del = new AutoScrollPositionDelegate(SetAutoScrollPosition);
                Object[] args = { (DaggerUIGraph)Parent, p };
                BeginInvoke(del, args);
            }

            base.OnEnter(e);
                        
            _isFocused = true;
            DrawCaption(null);
            //only update the caption area
            this.Invalidate(_targetRegionCaption);
            BringToFront();

            if (Activated != null)
            {
                Activated(this, new EventArgs());
            }
        }


        delegate void AutoScrollPositionDelegate(DaggerUIGraph sender, Point p);
        private void SetAutoScrollPosition(DaggerUIGraph sender, Point p)
        {
            p.X = Math.Abs(p.X);
            p.Y = Math.Abs(p.Y);
            sender.AutoScrollPosition = p;
            sender._isScrolling = false;
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLostFocus(e);

            _isFocused = false;

            DrawCaption(null);

            //only update the caption area
            this.Invalidate(_targetRegionCaption);

            if (Deactivated != null)
            {
                Deactivated(this, new EventArgs());
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Allign the location of the caption buttons
        /// </summary>
        private void ArrangeCaptionButtons()
        {
            SuspendLayout();

            //set the initial offset
            int offset = Width - _captionSize - 5;
            if (_pinPlacement == DaggerNodePinPlacement.Outset)
            {
                if (_parentGraph != null)
                {
                    offset -= _parentGraph.PinLegend.PinSize;
                }
                else
                {
                    offset -= 11;
                }
            }

            //go through reverse order and position the buttons in the caption
            for (int i = _captionButtons.Count - 1; i > -1; i--)
            {
                SimpleImageButton button = (SimpleImageButton)_captionButtons[i];
                if (button.Width != _captionSize)
                {
                    button.Size = new Size(_captionSize, _captionSize);
                }

                if (button.Visible)
                {
                    button.Location = new Point(offset, 0);
                    offset -= _captionSize;
                }
            }

            ResumeLayout(true);
        }

        /// <summary>
        /// Modifies the Menu Strip Items based on the context and state of DaggerNode
        /// </summary>
        /// <param name="strip"></param>
        private void ValidateContextMenuStrip(ContextMenuStrip strip)
        {
            // can we trigger node processing
            _ictxtProcessNode.Visible = _parentGraph.AllowNodeProcessing;

            // can we create subnodes?
            _ictxtCreateSubNode.Visible = _parentGraph.AllowSubNodes;

            //can we perform delete and relink?
            if (_parentGraph.AllowDeleteRelink)
            {
                _ictxtDeleteRelink.Visible = false;
                List<DaggerInputPin> inpins = InputPins.ConnectedPins;
                List<DaggerOutputPin> outpins = OutputPins.ConnectedPins;
                bool canRelink = true;
                if (inpins.Count == outpins.Count && inpins.Count > 0)
                {
                    for (int i = 0; i < inpins.Count; i++)
                    {
                        if (inpins[i].ConnectedToOutput.ParentNode == null)
                        {
                            //can't relink to imported pins
                            canRelink = false;
                            break;
                        }

                        foreach (DaggerInputPin pin in outpins[i].ConnectedTo)
                        {
                            if (pin.ParentNode == null)
                            {
                                //can't relink to exported pins
                                canRelink = false;
                                break;
                            }
                        }

                        if (!inpins[i].DataType.IsAssignableFrom(outpins[i].DataType))
                        {
                            //not compatible
                            canRelink = false;
                            break;
                        }
                    }
                }
                else
                {
                    canRelink = false;
                }
                _ictxtDeleteRelink.Visible = canRelink;
            }
            else
            {
                _ictxtDeleteRelink.Visible = false;
            }
        }

        /// <summary>
        /// Tests clientMousePosition to see if we can connect to/from a pin
        /// </summary>
        /// <param name="clientMousePosition">Client position to hit test</param>
        /// <returns>AlterState of allowed connection</returns>
        private DaggerNodeAlterState CanConnectToPin(Point clientMousePosition)
        {
            DaggerNodeAlterState alterState = DaggerNodeAlterState.None;

            //reset the _mouseOverPin field
            DaggerBasePin oldOverPin = _mouseOverPin;
            _mouseOverPin = null;

            //Check if over Input pin
            foreach (DaggerInputPin pin in _node.InputPins)
            {
                if (pin.MutexAvailable && (pin.PinUIElements as PinUI).TargetRegion.IsVisible(clientMousePosition))
                {
                    alterState = DaggerNodeAlterState.ConnectFromInput;
                    _mouseOverPin = pin;
                    if (oldOverPin != _mouseOverPin)
                    {
                        //the pin has changed, reset the tooltip
                        _wndToolTip.SetToolTip(this, "DaggerNode");
                    }
                    break;
                }
            }

            //Check if over Output pin
            foreach (DaggerOutputPin pin in _node.OutputPins)
            {
                if (pin.MutexAvailable && (pin.PinUIElements as PinUI).TargetRegion.IsVisible(clientMousePosition))
                {
                    alterState = DaggerNodeAlterState.ConnectFromOutput;
                    _mouseOverPin = pin;
                    if (oldOverPin != _mouseOverPin)
                    {
                        //the pin has changed, reset the tooltip
                        _wndToolTip.SetToolTip(this, "DaggerNode");
                    }
                    break;
                }
            }

            // if the parent is currently dragging a pin and over the proper type of pin then allow connect
            // check precedence to make sure we don't loopback!!!
            if (_parentGraph.TrackingPin != null)
            {
                if (_mouseOverPin != null)
                {
                    //get the pin that is being dragged
                    DaggerBasePin trackingPin = _parentGraph.TrackingPin;
                    int trackingPrecedence = 0;

                    if (trackingPin.ParentNode != null)
                    {
                        //get the precedence of the pin that is being tracked
                        trackingPrecedence = trackingPin.ParentNode.Ordinal;
                    }

                    //are all the conditions met to allow the Tracking Pin to connect here?
                    if (trackingPin is DaggerOutputPin)
                    {
                        alterState = trackingPin.CanConnectToPin(_mouseOverPin) ? DaggerNodeAlterState.CanConnectToInput : DaggerNodeAlterState.None;
                    }
                    else
                    {
                        alterState = trackingPin.CanConnectToPin(_mouseOverPin) ? DaggerNodeAlterState.CanConnectToOutput : DaggerNodeAlterState.None;
                    }
                }
            }
            return alterState;
        }

        /// <summary>
        /// Initializes the UI elements for a DaggerBasePin
        /// </summary>
        /// <param name="pin"></param>
        private void CreatePinUIElements(DaggerBasePin pin)
        {
            // only create the UI elements if they haven't been instantiated yet
            if (pin.PinUIElements == null)
            {
                pin.PinUIElements = (PinUI)new PinUI(pin);
            }
        }

        /// <summary>
        /// Generates the background image of the UI Node
        /// </summary>
        /// <param name="clipRectangle"></param>
        private void CreateBackImage(Rectangle clipRectangle)
        {
            if (_backImage != null)
            {
                _backImage.Dispose();
            }

            _backImage = new Bitmap(Width, Height);

            Graphics g = Graphics.FromImage(_backImage);

            //if called from OnPaint, set the clip rectangle
            if (clipRectangle != null)
            {
                g.SetClip(clipRectangle);
            }

            g.SmoothingMode = SmoothingMode.HighQuality;

            //draw the resize widget
            if (_resizable)
            {
                using (Brush db = new SolidBrush(ControlPaint.Light(BackColor)))
                {
                    g.FillPath(db, _resizePath);
                    using (Pen dp = new Pen(ControlPaint.Dark(BackColor)))
                    {
                        g.DrawPath(dp, _resizePath);
                    }
                }
            }

            //draw the client border 
            if (_clientPath != null)
            {
                g.DrawPath(Pens.Black, _clientPath);
            }

            //draw the pins
            Rectangle sourceRect = new Rectangle(0, 0, PinSize, PinSize);
            if ((_node.InputPins != null) && (_node.OutputPins != null))
            {
                int inputpintop = (Height - ((_node.InputPins.MutexAvailablePins.Count * PinSize) + ((_node.InputPins.MutexAvailablePins.Count - 1) * _pinSpacing))) / 2;
                int outputpintop = (Height - ((_node.OutputPins.MutexAvailablePins.Count * PinSize) + ((_node.OutputPins.MutexAvailablePins.Count - 1) * _pinSpacing))) / 2;
                for (int i = 0; i < _node.InputPins.MutexAvailablePins.Count; i++)
                {
                    Image pinimage = _node.InputPins.MutexAvailablePins[i].IsConnected ? (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageConnected : (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageDisconnected;
                    ImageAttributes att = new ImageAttributes();
                    Color alphakey = _node.InputPins.MutexAvailablePins[i].IsConnected ? (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageConnectedTransparent : (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageDisconnectedTransparent;
                    att.SetColorKey(alphakey, alphakey);
                    g.DrawImage(pinimage, new Rectangle(0, inputpintop, PinSize, PinSize), 0, 0, PinSize, PinSize, GraphicsUnit.Pixel, att);
                    inputpintop += PinSize + _pinSpacing;
                }
                for (int i = 0; i < _node.OutputPins.MutexAvailablePins.Count; i++)
                {
                    Image pinimage = _node.OutputPins.MutexAvailablePins[i].IsConnected ? (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageConnected : (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageDisconnected;
                    ImageAttributes att = new ImageAttributes();
                    Color alphakey = _node.OutputPins.MutexAvailablePins[i].IsConnected ? (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageConnectedTransparent : (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinImageDisconnectedTransparent;
                    att.SetColorKey(alphakey, alphakey);
                    g.DrawImage(pinimage, new Rectangle(Width - PinSize, outputpintop, PinSize, PinSize), 0, 0, PinSize, PinSize, GraphicsUnit.Pixel, att);
                    outputpintop += PinSize + _pinSpacing;
                }
            }

            //draw the caption (it will dispose of g)
            DrawCaption(g);
        }

        /// <summary>
        /// Draws the caption onto the BackImage
        /// </summary>
        private void DrawCaption(Graphics g)
        {
            if (g == null)
            {
                //DrawCaption was called from somewhere besides CreateBackImage so make sure the Backimage
                //was created.
                if (_backImage == null)
                {
                    CreateBackImage(ClientRectangle);
                    return;
                }
                else
                {
                    g = Graphics.FromImage(_backImage);
                }
            }

            //clip to the caption region
            Color capColor = _isProcessing ? Color.Red : (_isFocused ? _captionColor : _captionColorUnfocused);
            if (_captionPath != null)
            {
                g.SetClip(_captionPath, CombineMode.Replace);

                //Fill the Caption
                using (LinearGradientBrush b = new LinearGradientBrush(new Point(0, 0), new Point(0, _captionSize), ControlPaint.Dark(capColor), ControlPaint.Light(capColor)))
                {
                    b.SetBlendTriangularShape(0.5f, 1.0f);
                    g.FillPath(b, _captionPath);
                }
            }

            //measure text length
            StringFormat stringFormat = null;
            float xAxis = 0;

            //offset text if pins are outset
            if (_pinPlacement == DaggerNodePinPlacement.Outset)
            {
                xAxis = PinSize;
            }

            //offset if there is an Icon
            if (_panelIcon != null)
            {
                xAxis += _captionSize;
            }

            float yAxis = 0;
            stringFormat = new StringFormat();
            SizeF ssize = g.MeasureString(_captionText, this.Font, new PointF(0, 0), stringFormat);
            yAxis = (_captionSize - ssize.Height) * 0.5f;

            //draw the text
            g.DrawString(_captionText, this.Font, new SolidBrush(ForeColor), new PointF(xAxis + 3, yAxis), stringFormat);

            //draw the caption icon if one is set
            if (_panelIcon != null)
            {
                Rectangle iconDest = new Rectangle((int)(xAxis - _captionSize + 3), 0, _captionSize, _captionSize);
                Rectangle iconSrc = new Rectangle(0,0,_panelIcon.Width,_panelIcon.Height);
                g.DrawImage(_panelIcon, iconDest, iconSrc, GraphicsUnit.Pixel);
            }

            //unclip so the caption border can be drawn
            g.SetClip(ClientRectangle, CombineMode.Replace);

            //draw the caption border
            if (_captionPath != null)
            {
                using (Pen capPen = new Pen(ControlPaint.Dark(capColor)))
                {
                    g.DrawPath(capPen, _captionPath);
                }
            }

            g.Dispose();
        }

        /// <summary>
        /// Updates the graphics path of the caption and the panel
        /// </summary>
        private void UpdatePaths()
        {
            RectangleF r;
            if (_pinPlacement == DaggerNodePinPlacement.Outset)
            {
                //we want the pins completely outside the panel, so make the client rectangle a little smaller
                r = new RectangleF(PinSize, 0, (Width - 1) - PinSize * 2, Height - 1);
            }
            else
            {
                r = new RectangleF(0, 0, Width - 1, Height - 1);
            }

            _clientPath = GetTopClippedRoundedRect(r, _panelRadius, _captionSize);
            _captionPath = GetRoundedRect(new Rectangle((int)r.X, 0, (int)Math.Abs(r.Width), _captionSize), _panelRadius);

            if (_resizable)
            {
                Point lr = new Point();

                //get the lower right point based on pin placement
                switch (_pinPlacement)
                {
                    case DaggerNodePinPlacement.Indent:
                    case DaggerNodePinPlacement.Inset:
                        lr = new Point(Width - 1, Height - 1);
                        break;
                    case DaggerNodePinPlacement.Outset:
                        if (_parentGraph != null)
                        {
                            lr = new Point(Width - 1 - _parentGraph.PinLegend.PinSize, Height - 1);
                        }
                        else
                        {
                            lr = new Point(Width - 11, Height - 1);
                        }
                        break;
                    default:
                        break;
                }

                //create a small triangle in lower right corner
                _resizePath = new GraphicsPath();
                _resizePath.StartFigure();
                _resizePath.AddLine(lr.X, lr.Y, lr.X - 12, lr.Y);
                _resizePath.AddLine(lr.X - 12, lr.Y, lr.X, lr.Y - 12);
                _resizePath.CloseFigure();
            }

            if (_pinPlacement == DaggerNodePinPlacement.Outset)
            {
                //offset the client path by the pin size
                Matrix translateMatrix = new Matrix();
                translateMatrix.Translate(PinSize, 0);
                _clientPath.Transform(translateMatrix);
            }
        }

        /// <summary>
        /// Adjust positions and target click regions of the pins, caption, resizer
        /// </summary>
        private void UpdateTargetRegions()
        {
            _targetRegionCaption = new Region(new Rectangle(0, 0, Width - 1, _captionSize));

            // if resizable update the resize widget
            if (_resizable)
            {
                if (_targetRegionResize != null)
                {
                    _targetRegionResize.Dispose();                    
                }
                _targetRegionResize = new Region(_resizePath);
            }

            //calculate targets for the Pins
            if ((_node.InputPins != null) && (_node.OutputPins != null))
            {
                int inputpintop = (Height - ((_node.InputPins.MutexAvailablePins.Count * PinSize) + ((_node.InputPins.MutexAvailablePins.Count - 1) * _pinSpacing))) / 2;
                int outputpintop = (Height - ((_node.OutputPins.MutexAvailablePins.Count * PinSize) + ((_node.OutputPins.MutexAvailablePins.Count - 1) * _pinSpacing))) / 2;
                for (int i = 0; i < _node.InputPins.MutexAvailablePins.Count; i++)
                {
                    if ((_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion != null)
                    {
                        (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion.Dispose();
                        (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion = null;
                    }
                    (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion = new Region(new Rectangle(0, inputpintop, PinSize, PinSize));
                    (_node.InputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinLocation = new Point(0, inputpintop);
                    inputpintop += PinSize + _pinSpacing;
                }
                for (int i = 0; i < _node.OutputPins.MutexAvailablePins.Count; i++)
                {
                    if ((_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion != null)
                    {
                        (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion.Dispose();
                        (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion = null;
                    }
                    (_node.OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).TargetRegion = new Region(new Rectangle(Width - PinSize, outputpintop, PinSize, PinSize));
                    (OutputPins.MutexAvailablePins[i].PinUIElements as PinUI).PinLocation = new Point(Width - PinSize, outputpintop);
                    outputpintop += PinSize + _pinSpacing;
                }
            }
        }

        /// <summary>
        /// Adjust size of UINode to fit pins
        /// </summary>
        /// <returns>true if UINode was resized</returns>
        private bool AdjustSize()
        {
            if (_parentGraph == null)
            {
                //not added to a graph yet
                return false;
            }

            bool wasResized = false;
            Size newSize = NodeMinimumSize;

            if (newSize.Height > Height)
            {
                Height = newSize.Height;
                wasResized = true;
            }
            if (newSize.Width > Width)
            {
                Width = newSize.Width;
                wasResized = true;
            }

            return wasResized;
        }

        /// <summary>
        /// Updates the region of the DaggerNode
        /// </summary>
        private void UpdateRegion()
        {
            try
            {
                //widen the paths to prevent pixel clipping
                Pen widepen = new Pen(Color.Black, 0.0f);
                widepen.MiterLimit = 0;

                GraphicsPath wideClientPath = (GraphicsPath)_clientPath.Clone();
                wideClientPath.Widen(widepen);
                GraphicsPath wideCaptionPath = (GraphicsPath)_captionPath.Clone();
                wideCaptionPath.Widen(widepen);

                //create and merge all the paths
                Region capreg = new Region(wideCaptionPath);
                capreg.Union(_captionPath);
                Region clientregion = new Region(wideClientPath);
                clientregion.Union(_clientPath);
                clientregion.Union(capreg);

                if ((_node.InputPins != null) && (_node.OutputPins != null))
                {
                    foreach (DaggerBasePin pin in _node.OutputPins.MutexAvailablePins)
                    {
                        if ((pin.PinUIElements as PinUI).PinConnectedRegion != null)
                        {
                            Region tempr = new Region(pin.IsConnected ? (pin.PinUIElements as PinUI).PinConnectedRegion.GetRegionData() : (pin.PinUIElements as PinUI).PinDisconnectedRegion.GetRegionData());
                            tempr.Translate((pin.PinUIElements as PinUI).PinLocation.X, (pin.PinUIElements as PinUI).PinLocation.Y);
                            clientregion.Union(tempr);
                            tempr.Dispose();
                            tempr = null;
                        }
                    }

                    foreach (DaggerBasePin pin in _node.InputPins.MutexAvailablePins)
                    {
                        if ((pin.PinUIElements as PinUI).PinConnectedRegion != null)
                        {
                            Region tempr = new Region(pin.IsConnected ? (pin.PinUIElements as PinUI).PinConnectedRegion.GetRegionData() : (pin.PinUIElements as PinUI).PinDisconnectedRegion.GetRegionData());
                            tempr.Translate((pin.PinUIElements as PinUI).PinLocation.X, (pin.PinUIElements as PinUI).PinLocation.Y);
                            clientregion.Union(tempr);
                            tempr.Dispose();
                            tempr = null;
                        }
                    }
                }

                if (this.Region != null)
                {
                    this.Region.Dispose();
                }

                this.Region = clientregion;

                widepen.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Updating Region");
            }
        }

        private GraphicsPath GetTopClippedRoundedRect(RectangleF baseRect, float radius, float captionSize)
        {
            // if corner radius is less than or equal to zero,
            // return the original rectangle
            if (radius <= 0.0F)
            {
                GraphicsPath mPath = new GraphicsPath();
                mPath.AddRectangle(baseRect);
                mPath.CloseFigure();
                return mPath;
            }

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddLine(0, captionSize / 2, baseRect.Width, captionSize / 2);

            if (_pinPlacement == DaggerNodePinPlacement.Indent)
            {
                //indent right if any output pins
                if ((_node.OutputPins != null) && (_node.OutputPins.Count > 0))
                {
                    gp.AddLine(baseRect.Width, captionSize / 2, baseRect.Width, captionSize);

                    PointF point1 = new PointF(baseRect.Width, captionSize);
                    PointF point2 = new PointF(baseRect.Width - PinSize * _pinIndent, captionSize + PinSize);
                    PointF point3 = new PointF(baseRect.Width - PinSize * _pinIndent, baseRect.Height - radius - PinSize);
                    PointF point4 = new PointF(baseRect.Width, baseRect.Height - radius);
                    gp.AddBezier(point1, point2, point3, point4);
                }
            }

            gp.AddArc(baseRect.Width - radius, baseRect.Height - radius, radius, radius, 0, 90);
            gp.AddArc(0, baseRect.Height - radius, radius, radius, 90, 90);

            if (_pinPlacement == DaggerNodePinPlacement.Indent)
            {
                //indent left if any input pins
                if ((_node.InputPins != null) && (_node.InputPins.Count > 0))
                {
                    PointF point4 = new PointF(0, captionSize);
                    PointF point3 = new PointF(PinSize * _pinIndent, captionSize + PinSize);
                    PointF point2 = new PointF(PinSize * _pinIndent, baseRect.Height - radius - PinSize);
                    PointF point1 = new PointF(0, baseRect.Height - radius);
                    gp.AddBezier(point1, point2, point3, point4);

                    gp.AddLine(0, captionSize, 0, captionSize / 2);
                }
            }

            gp.CloseFigure();
            return gp;
        }

        private GraphicsPath GetRoundedRect(RectangleF baseRect, float radius)
        {
            // if corner radius is less than or equal to zero,
            // return the original rectangle
            if (radius <= 0.0F)
            {
                GraphicsPath mPath = new GraphicsPath();
                mPath.AddRectangle(baseRect);
                mPath.CloseFigure();
                return mPath;
            }

            // if the corner radius is greater than or equal to
            // half the baseRect.Width, or baseRect.Height (whichever is shorter)
            // then return a capsule instead of a lozenge
            if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.5)
                return GetCapsule(baseRect);

            // create the arc for the rectangle sides and declare
            // a graphics path object for the drawing

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddArc(baseRect.X, baseRect.Y, radius, radius, 180, 90);
            gp.AddArc(baseRect.X + baseRect.Width - radius, baseRect.Y, radius, radius, 270, 90);
            gp.AddArc(baseRect.X + baseRect.Width - radius, baseRect.Y + baseRect.Height - radius, radius, radius, 0, 90);
            gp.AddArc(baseRect.X, baseRect.Y + baseRect.Height - radius, radius, radius, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        private GraphicsPath GetCapsule(RectangleF baseRect)
        {
            float diameter;
            RectangleF arc;
            GraphicsPath path = new GraphicsPath();
            try
            {
                if (baseRect.Width > baseRect.Height)
                {
                    // return horizontal capsule 
                    diameter = baseRect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 90, 180);
                    arc.X = baseRect.Right - diameter;
                    path.AddArc(arc, 270, 180);
                }
                else if (baseRect.Width < baseRect.Height)
                {
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180);
                    arc.Y = baseRect.Bottom - diameter;
                    path.AddArc(arc, 0, 180);
                }
                else
                {
                    // return circle 
                    path.AddEllipse(baseRect);
                }
            }
            catch (Exception)
            {
                path.AddEllipse(baseRect);
            }
            finally
            {
                path.CloseFigure();
            }
            return path;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the Layout of the node and refreshes all child controls
        /// </summary>
        public void CalculateLayout()
        {
            if (AdjustSize())
            {
                //if we resized return, because OnResize will call CalculateLayout again
                return;
            }

            // make sure the pins have UI elements
            foreach (DaggerBasePin pin in _node.InputPins)
            {
                CreatePinUIElements(pin);
            }
            foreach (DaggerBasePin pin in _node.OutputPins)
            {
                CreatePinUIElements(pin);
            }

            //recreate the back image and update the control region
            UpdatePaths();
            UpdateTargetRegions();            
            UpdateRegion();
            CreateBackImage(ClientRectangle);

            _internalControl.Location = new Point(_internalControlPadding + PinSize, _internalControlPadding + _captionSize);
            _internalControl.Size = new Size(this.Width - (_internalControlPadding * 2) - (PinSize * 2), this.Height - (_internalControlPadding * 2) - _captionSize);

            ArrangeCaptionButtons();

            Refresh();
        }

        /// <summary>
        /// Override to provide processing of UI elements.  Called AFTER the node's DoProcessing event has been raised
        /// </summary>
        public virtual void DoUIProcessing()
        {

        }

        #endregion
    }
}
