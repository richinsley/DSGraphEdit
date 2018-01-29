using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DaggerLib.Core;
using DaggerLib.Interfaces;
using System.Runtime.Serialization.Formatters.Binary;

namespace DaggerLib.UI.Windows
{
    [ToolboxItem(true)]
    public class DaggerUIGraph : DoubleBufferedPanel , IDaggerUIGraph
    {
        #region Fields 

        private DaggerGraph _graph;

        internal ImageList CursorImages;

        private IContainer components;

        // Pin that is currently being dragged for connection
        internal DraggingNoodle _trackingConnectPin;

        // List of Noodles on Container
        internal DaggerNoodleContainer _noodles;

        // How the noodles are drawn
        private NoodleStyle _noodleStyle = NoodleStyle.Default;

        // Selection rectangle that is currently being dragged
        internal Selector _trackingSelector;

        // how shall we do autoarranging of nodes after topology changes
        private AutoArrangeStyle _autoArrangeStyle = AutoArrangeStyle.None;

        // the spacing between nodes when autoarranging
        private int _autoArrangeWidthOffset = 50;
        private int _autoArrangeHeightOffSet = 10;

        // invisible "pucks" that lock the autoscroll
        internal DaggerEventListener _focusPuck;

        // the spacing between the exported/imported pins
        private int _pinSpacing = 2;

        /// <summary>
        /// The size of noodle connectors
        /// </summary>
        private int _pinSize = 11;

        // used for positioning of exported pins
        private float _widestImportName;
        private float _widestExportName;
        private float _highestImportName;
        private float _highestExportName;

        // the previous mouse location durring a Canvas drag operation
        private Point _lastCanvasDragPosition;

        // flag indicating the user is dragging the canvas
        private bool _isDraggingCanvas;

        // the state of the exported pin we can alter depending on mouse locations or other conditions
        private DaggerNodeAlterState _canAlterState = DaggerNodeAlterState.None;

        // the exported/imported pin that the cursor is currently over
        private DaggerBasePin _mouseOverPin;

        // the DaggerNodeTreeView that we can export SubNodes to
        private DaggerNodeTreeView _associatedTreeView;

        // flag indicating if we can drop a node on a noodle and automatically bisect it
        private bool _allowNoodleBisecting = true;

        // flag indicating if we can perform Delete and Relink operations on a node
        private bool _allowDeleteRelink = true;

        // flag indicating if we can create subnodes from a selection
        private bool _allowSubNodes = true;

        // flag to indicate if we can manually trigger Node Processing
        private bool _allowNodeProcessing = true;

        // noodle that may be bisected by dragged DaggerNode
        private DaggerNoodleBisector _noodleBisector;

        // can pins be exported/imported
        internal bool _allowPinExport = true;

        // can the user set the value of input pins
        internal bool _allowPinSetValue = true;

        // can the user append a Constant editor for an input pin?
        internal bool _allowAttachConstantEditor = true;

        // Collection of pin images and thier associated data types
        private DaggerPinLegend _pinLegend;

        // drop shadow options
        private bool _dropShadowVisible = false;
        private float _dropShadowAlpha = 0.5f;
        private int _dropShadowXoffset = 10;
        private int _dropShadowYoffset = 13;
        internal ContextMenuStrip contextMenuStrip;

        // display ordinal in Node ToolTips?
        private bool _showOrdinal = true;

        // flag to prevent code reentry in DeleteSelection
        volatile bool _deletingSelection = false;

        // the current state of the control key.  Set by the KeyBoard hook proc function
        internal bool _ctrlKey;

        // flag to indicate the user is scrolling or resizing the canvas
        // prevents Node moved events from multiple calls to CreateCanvasImage
        internal bool _isScrolling = false;

        // the background bitmap
        private Bitmap _backgroundImage;

        // the canvas bitmap
        private Bitmap _canvasImage;

        // flag to indicate if the Control scrolls when connecting pins and the mouse is outside the canvas ViewPort
        private bool _scrollConnect = false;

        // ref count to keep track of BeginCanvasUpdate and EndCanvasUpdate
        private int _updateRef = 0;

        // Most recent DaggerNode that was drag/dropped onto the control
        protected DaggerNode _droppedNode;

        #endregion

        #region ctor

        public DaggerUIGraph()
        {
            _noodles = new DaggerNoodleContainer(this,NoodleStyle.Default);

            _pinLegend = new DaggerPinLegend(_pinSize);

            _graph = new DaggerGraph();
            _graph.ParentUIGraph = (IDaggerUIGraph)this;

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            InitializeComponent();

            AutoScroll = true;
            AllowDrop = true;

            // Panels have an insanely annoying habit of moving the control that has focus
            // during Resizing if it's autoscroll is true.  We'll create an innocuous control that will 
            // get focus during resizing operations to prevent any nodes from being repositioned.
            _focusPuck = new DaggerEventListener();
            _focusPuck.Width = 1;
            _focusPuck.Height = 1;
            this.Controls.Add(_focusPuck);

            // connect events for the graph
            _graph.NodeAdded += new EventHandler(_graph_NodeAdded);
            _graph.AfterPinsConnected += new PinAfterConnectedHandler(_graph_AfterPinsConnected);
            _graph.AfterPinsDisconnected += new PinAfterDisconnectedHandler(_graph_AfterPinsDisconnected);

            // hook the keyboard
            _setKeyboardHook();
        }

        internal DaggerUIGraph(DaggerGraph graph, DaggerPinLegend pinLegend)
        {
            _noodles = new DaggerNoodleContainer(this,NoodleStyle.Default);

            //assign or create the pin legend
            if (pinLegend == null)
            {
                _pinLegend = new DaggerPinLegend(_pinSize);
            }
            else
            {
                _pinLegend = pinLegend;
            }

            _graph = graph;
            _graph.ParentUIGraph = this as IDaggerUIGraph;

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            InitializeComponent();

            AutoScroll = true;
            AllowDrop = true;

            _focusPuck = new DaggerEventListener();
            _focusPuck.Width = 1;
            _focusPuck.Height = 1;
            this.Controls.Add(_focusPuck);

            _graph.NodeAdded += new EventHandler(_graph_NodeAdded);
            _graph.AfterPinsConnected += new PinAfterConnectedHandler(_graph_AfterPinsConnected);
            _graph.AfterPinsDisconnected += new PinAfterDisconnectedHandler(_graph_AfterPinsDisconnected);

            // create and add ui nodes for all the nodes
            foreach (DaggerNode node in _graph.AllNodes)
            {
                _graph_NodeAdded(node, null);
            }

            // create the noodles
            foreach (DaggerNode node in _graph.AllNodes)
            {
                foreach (DaggerOutputPin outpin in node.OutputPins)
                {
                    foreach (DaggerInputPin inpin in outpin.ConnectedTo)
                    {
                        _graph_AfterPinsConnected(outpin, inpin);
                    }
                }
            }
            foreach (DaggerOutputPin outpin in _graph.ImportedPins)
            {
                foreach (DaggerInputPin inpin in outpin.ConnectedTo)
                {
                    _graph_AfterPinsConnected(outpin, inpin);
                }
            }

            // if there is a layout stored in the graph, reconstruct it
            if (graph.UILayout != null)
            {
                foreach (GraphLayout gl in graph.UILayout)
                {
                    DaggerUINode uinode = null;

                    foreach (DaggerNode n in _graph.AllNodes)
                    {
                        if (n.InstanceGuid == gl.targetNodeGuid)
                        {
                            uinode = n.UINode as DaggerUINode;
                            break;
                        }
                    }

                    if (uinode != null)
                    {
                        gl.Apply(uinode as IDaggerUINode);
                    }
                }
            }

            //hook the keyboard
            _setKeyboardHook();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DaggerUIGraph));
            this.CursorImages = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // CursorImages
            // 
            this.CursorImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("CursorImages.ImageStream")));
            this.CursorImages.TransparentColor = System.Drawing.Color.Transparent;
            this.CursorImages.Images.SetKeyName(0, "CursorLeft.png");
            this.CursorImages.Images.SetKeyName(1, "CursorRight.png");
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(146, 48);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            this.ResumeLayout(false);

        }

        #endregion

        #region Dispose Method

        protected override void Dispose(bool disposing)
        {
            //unhook the keyboard
            _unsetKeyboardHook();

            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default Associated UINode type
        /// </summary>
        [Browsable(false)]
        public string DefaultAssociatedDaggerUINode
        {
            get
            {
                return typeof(DaggerUINode).AssemblyQualifiedName;
            }
        }


        /// <summary>
        /// Gets or sets if the canvas scrolls when connecting pins, and the mouse is outside the ViewPort area
        /// </summary>
        public bool ScrollConnect
        {
            get
            {
                return _scrollConnect;
            }
            set
            {
                _scrollConnect = value;
            }
        }

        /// <summary>
        /// Gets or sets if the DaggerGraph allows Deleting and Relinking of DaggerNodes
        /// </summary>
        public bool AllowDeleteRelink
        {
            get
            {
                return _allowDeleteRelink;
            }
            set
            {
                _allowDeleteRelink = value;
            }
        }

        /// <summary>
        /// Gets or sets if the DaggerGraph allow creating subnodes from a selection or subgraph
        /// </summary>
        public bool AllowSubNodes
        {
            get
            {
                return _allowSubNodes;
            }
            set
            {
                _allowSubNodes = value;
            }
        }

        /// <summary>
        /// Gets or sets if the user can manually trigger node processing
        /// </summary>
        public bool AllowNodeProcessing
        {
            get
            {
                return _allowNodeProcessing;
            }
            set
            {
                _allowNodeProcessing = value;
            }
        }

        /// <summary>
        /// Gets or sets if the graph allows drag-drop bisecting of Noodles
        /// </summary>
        public bool AllowNoodleBisecting
        {
            get
            {
                return _allowNoodleBisecting;
            }
            set
            {
                _allowNoodleBisecting = value;
            }
        }

        /// <summary>
        /// Gets or sets if pins can be Imported/Exported
        /// </summary>
        public bool AllowPinExport
        {
            get
            {
                return _allowPinExport;
            }
            set
            {
                _allowPinExport = value;
            }
        }

        /// <summary>
        /// Gets or sets if user can set the value of an Input Pin
        /// </summary>
        public bool AllowPinSetValue
        {
            get
            {
                return _allowPinSetValue;
            }
            set
            {
                _allowPinSetValue = value;
            }
        }

        /// <summary>
        /// Gets or sets if user can attach a default constant editor to an Input Pin
        /// </summary>
        public bool AllowAttachConstantEditor
        {
            get
            {
                return _allowAttachConstantEditor;
            }
            set
            {
                _allowAttachConstantEditor = value;
            }
        }

        /// <summary>
        /// Gets or sets if UINodes display Ordinal Number in the Tooltip
        /// </summary>
        public bool ShowOrdinal
        {
            get
            {
                return _showOrdinal;
            }
            set
            {
                _showOrdinal = value;
            }
        }

        public NoodleStyle NoodleStyle
        {
            get
            {
                return _noodleStyle;
            }
            set
            {
                _noodleStyle = value;
                if (_noodles != null)
                {
                    _noodles.NoodleStyle = value;
                    _noodles.UpdateNoodles();
                    RefreshGraph();
                }
            }
        }

        public int PinSize
        {
            get
            {
                return _pinSize;
            }
            set
            {
                _pinSize = Math.Max(8, value);
                UpdateExportPins();
                UpdateImportPins();
            }
        }

        /// <summary>
        /// Get the pin that is currently being dragged
        /// </summary>
        internal DaggerBasePin TrackingPin
        {
            get
            {
                if (_trackingConnectPin != null)
                {
                    if (_trackingConnectPin.OutputPin != null)
                    {
                        return _trackingConnectPin.OutputPin;
                    }
                    else
                    {
                        return _trackingConnectPin.InputPin;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the actual size of the canvas based on positions and sizes of the nodes
        /// </summary>
        /// <returns></returns>
        public Size ActualCanvasSize
        {
            get
            {
                int w = 1;
                int h = 1;

                foreach (DaggerUINode node in AllNodes)
                {
                    w = Math.Max(w, node.Right - AutoScrollPosition.X);
                    h = Math.Max(h, node.Bottom - AutoScrollPosition.Y);
                }

                w = Math.Max(w, Math.Max(AutoScrollMinSize.Width, Width));
                h = Math.Max(h, Math.Max(AutoScrollMinSize.Height, Height));

                return new Size(w, h);
            }
        }

        [Browsable(false)]
        public DaggerGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Gets the reactangular region occupied by all DaggerUINodes
        /// </summary>
        [Browsable(false)]
        public Rectangle OccupiedRegion
        {
            get
            {
                int top = CanvasSize.Height, left = CanvasSize.Width, bottom = 0, right = 0;

                foreach (DaggerUINode node in AllNodes)
                {
                    top = Math.Min(top, node.Top);
                    left = Math.Min(left, node.Left);
                    bottom = Math.Max(bottom, node.Bottom);
                    right = Math.Max(right, node.Right);
                }

                return new Rectangle(top, left, right - left, bottom - top );
            }
        }

        /// <summary>
        /// The DaggerNodeTreeView to export SubNodes to
        /// </summary>
        public DaggerNodeTreeView DaggerNodeTreeView
        {
            get
            {
                return _associatedTreeView;
            }
            set
            {
                _associatedTreeView = value;
            }
        }

        /// <summary>
        /// Get a list of all nodes selected by the user
        /// </summary>
        [Browsable(false)]
        public List<IDaggerUINode> SelectedNodes
        {
            get
            {
                List<IDaggerUINode> snodes = new List<IDaggerUINode>();

                if (_trackingSelector != null)
                {
                    snodes.AddRange(_trackingSelector.SelectedNodes);
                }

                return snodes;
            }
        }

        /// <summary>
        /// Get a list of all nodes contained in the panel
        /// </summary>
        [Browsable(false)]
        public List<IDaggerUINode> AllNodes
        {
            get
            {
                List<IDaggerUINode> newlist = new List<IDaggerUINode>();
                foreach (DaggerNode node in _graph.AllNodes)
                {
                    if (node.UINode != null)
                    {
                        newlist.Add(node.UINode);
                    }
                }
                return newlist;
            }
        }

        /// <summary>
        /// Get a list of all noodles contained in the panel
        /// </summary>
        [Browsable(false)]
        public List<IDaggerNoodle> AllNoodles
        {
            get
            {
                // copy the list to a list of IDaggerNoodle
                List<IDaggerNoodle> allnoodles = new List<IDaggerNoodle>();
                foreach (DaggerNoodle n in _noodles.Noodles)
                {
                    allnoodles.Add(n);
                }
                return allnoodles;
            }
        }

        /// <summary>
        /// Gets or sets the collection of pin images used by this Graph's UINodes
        /// </summary>
        [Browsable(false)]
        public DaggerPinLegend PinLegend
        {
            get
            {
                return _pinLegend;
            }
            set
            {
                if (value != null)
                {
                    _pinLegend = value;
                }
                else
                {
                    _pinLegend = new DaggerPinLegend(_pinSize);
                }

                //redo the layout for all UINodes
                foreach (DaggerUINode node in AllNodes)
                {
                    node.CalculateLayout();

                    //propogate PinLegend to SubNodes
                    if (node is DaggerUISubNode)
                    {
                        (node as DaggerUISubNode).PinLegend = value;
                    }
                }

                UpdateExportPins();
                UpdateImportPins();
            }
        }

        [Browsable(true)]
        public AutoArrangeStyle AutoArrange
        {
            get
            {
                return _autoArrangeStyle;
            }
            set
            {
                _autoArrangeStyle = value;
                ArrangeNodes();
            }
        }

        public int AutoArrangeWidthOffset
        {
            get
            {
                return _autoArrangeWidthOffset;
            }
            set
            {
                _autoArrangeWidthOffset = value;
                ArrangeNodes();
            }
        }

        public int AutoArrangeHeightOffset
        {
            get
            {
                return _autoArrangeHeightOffSet;
            }
            set
            {
                _autoArrangeHeightOffSet = value;
                ArrangeNodes();
            }
        }

        public bool DropShadowVisible
        {
            get
            {
                return _dropShadowVisible;
            }
            set
            {
                if (value != _dropShadowVisible)
                {
                    _dropShadowVisible = value;
                    RefreshGraph();
                }
            }
        }

        public float DropShadowAlpha
        {
            get
            {
                return _dropShadowAlpha;
            }
            set
            {
                _dropShadowAlpha = Math.Min(1.0f, Math.Max(0f, value));
                RefreshGraph();
            }
        }

        public int DropShadowXOffset
        {
            get
            {
                return _dropShadowXoffset;
            }
            set
            {
                _dropShadowXoffset = value;
                RefreshGraph();
            }
        }

        public int DropShadowYOffset
        {
            get
            {
                return _dropShadowYoffset;
            }
            set
            {
                _dropShadowYoffset = value;
                RefreshGraph();
            }
        }

        public int FocusLocationX
        {
            get
            {
                return _focusPuck.Location.X;
            }
        }

        public int FocusLocationY
        {
            get
            {
                return _focusPuck.Location.Y;
            }
        }
        
        public new Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
                RefreshGraph();
            }
        }

        public Size CanvasSize
        {
            get
            {
                int w = 0;
                int h = 0;
                Size actual = ActualCanvasSize;

                w = Math.Max(AutoScrollMinSize.Width, actual.Width);
                h = Math.Max(AutoScrollMinSize.Height, actual.Height);

                return new Size(w, h);
            }
            set
            {
                AutoScrollMinSize = value;
            }
        }

        /// <summary>
        /// Gets the area of the Viewport in 0.0 - 1.0 coords
        /// </summary>
        public RectangleF ViewPort
        {
            get
            {
                float left = (float)(AutoScrollPosition.X * -1) / (float)_canvasImage.Width;
                float top = (float)(AutoScrollPosition.Y * -1) / (float)_canvasImage.Height;
                float width = (float)Width / (float)_canvasImage.Width;
                float height = (float)Height / (float)_canvasImage.Height;

                return new RectangleF(left, top, width, height);
            }
        }

        public Bitmap CanvasImage
        {
            get
            {
                if (_canvasImage != null)
                {
                    Bitmap canvas = new Bitmap(_canvasImage.Width, _canvasImage.Height);
                    Graphics g = Graphics.FromImage(canvas);
                    g.Clear(BackColor);
                    Rectangle rect = new Rectangle(0, 0, canvas.Width, canvas.Height);
                    if (_backgroundImage != null)
                    {
                        g.DrawImage(_backgroundImage, rect, rect, GraphicsUnit.Pixel);
                    }
                    g.DrawImage(_canvasImage, rect, rect, GraphicsUnit.Pixel);

                    foreach (Control c in this.Controls)
                    {
                        if (c is DaggerUINode)
                        {
                            Bitmap b = new Bitmap(c.Width, c.Height);                            
                            c.DrawToBitmap(b, c.ClientRectangle);
                            Region r1 = new Region(c.ClientRectangle);
                            r1.Exclude(c.Region);
                            Graphics g2 = Graphics.FromImage(b);
                            g2.FillRegion(Brushes.Red, r1);
                            g2.Dispose();
                            r1.Dispose();
                            ImageAttributes atr = new ImageAttributes();
                            atr.SetColorKey(Color.Red, Color.Red);
                            g.DrawImage(b, new Rectangle(c.Left - AutoScrollPosition.X, c.Top - AutoScrollPosition.Y, c.Width, c.Height), 0, 0, c.Width, c.Height, GraphicsUnit.Pixel, atr);
                            b.Dispose();
                        }
                    }

                    g.Dispose();
                    return canvas;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Events and Handlers

        /// <summary>
        /// Event that is raised before a selection is deleted
        /// </summary>
        public event BeforeDeleteSelected BeforeSelectionDeleted;

        /// <summary>
        /// Event that is raised after a selection has been deleted
        /// </summary>
        public event AfterDeleteSelected AfterSelectionDeleted;

        /// <summary>
        /// Event that is raised when the canvas image is updated
        /// </summary>
        public event EventHandler CanvasImageChanged;

        /// <summary>
        /// Event that is rasied when the Canvas Viewport area has changed
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Event that is raised after a noodle is added
        /// </summary>
        public event PinAfterConnectedHandler NoodleAdded;

        /// <summary>
        /// Event that is raised after a noodle is removed
        /// </summary>
        public event PinAfterDisconnectedHandler NoodleRemoved;

        void _graph_NodeAdded(object sender, EventArgs e)
        {
            DaggerNode node = sender as DaggerNode;

            if (node != null)
            {
                if (node.UINode == null)
                {
                    //a ui node hasn't been made for this yet
                    DaggerUINode uinode = null;
                    if (node.AssociatedUINode == "IDaggerUISubNode")
                    {
                        // create a DaggerUISubNode
                        uinode = (DaggerUINode)Activator.CreateInstance(typeof(DaggerUISubNode));
                    }
                    else
                    {
                        if (node.AssociatedUINode == "" || node.AssociatedUINode == "IDaggerUINode")
                        {
                            // create a default DaggerUINode
                            Type t = Type.GetType(this.DefaultAssociatedDaggerUINode);
                            uinode = (DaggerUINode)Activator.CreateInstance(t);
                        }
                        else
                        {
                            // create an AssociatedUINode
                            Type t = null;
                            try
                            {
                                t = Type.GetType(node.AssociatedUINode, true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error " + ex.Message + " getting type: " + node.AssociatedUINode);
                            }
                            uinode = (DaggerUINode)Activator.CreateInstance(t);
                        }
                    }

                    if (uinode == null)
                    {
                        //couldn't make a ui element for this node
                        throw new InvalidOperationException("Unable to create a " + node.AssociatedUINode.ToString());
                    }

                    try
                    {
                        // if the node was drag/dropped onto the canvas, mark it not visible
                        if (_droppedNode == node) uinode.Visible = false;

                        // attach the node to the uinode and add to controls
                        uinode.Node = node;
                        Controls.Add(uinode);
                        uinode.Size = uinode.NodeMinimumSize;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    //if the node added is a DaggerSubNode, give it our PinLegend
                    if (node is DaggerSubNode)
                    {
                        (uinode as DaggerUISubNode).PinLegend = _pinLegend;
                    }
                }
            }
        }

        void _graph_AfterPinsDisconnected(DaggerOutputPin output, DaggerInputPin input)
        {
            //delete thier noodle
            DeleteNoodle(output, input);
            
            //arrange the graph if not bisecting
            if (_noodleBisector == null)
            {
                ArrangeNodes();
            }
        }

        void _graph_AfterPinsConnected(DaggerOutputPin output, DaggerInputPin input)
        {
            // make sure these pins have UIElements before creating a noodle
            if (output.PinUIElements == null) output.PinUIElements = new PinUI(output);
            if (input.PinUIElements == null) input.PinUIElements = new PinUI(input);

            //create noodle to represent this connection
            AddNoodle(output, input);

            //arrange the graph if not bisecting
            if (_noodleBisector == null)
            {
                ArrangeNodes();
            }
        }

        void _renameImportedPin_Click(object sender, EventArgs e)
        {
            DaggerOutputPin pin = (DaggerOutputPin)(sender as ToolStripMenuItem).Tag;

            ValueEditorDialog vue = new ValueEditorDialog("Rename Imported Pin", pin.Name);
            if (vue.ShowDialog() == DialogResult.OK)
            {
                pin.Name = (string)vue.Data;

                BeginCanvasUpdate();
                UpdateImportPins();                
                EndCanvasUpdate();

                if (_graph.ParentSubNode != null)
                {
                    //reflect pin name to outter subnode
                    _graph.ParentSubNode.InputPins[_graph.ImportedPins.IndexOf(pin)].Name = (string)vue.Data;
                    RefreshGraph();
                }
            }
        }

        void _renameExportedPin_Click(object sender, EventArgs e)
        {
            DaggerInputPin pin = (DaggerInputPin)(sender as ToolStripMenuItem).Tag;

            ValueEditorDialog vue = new ValueEditorDialog("Rename Exported Pin", pin.Name);
            if (vue.ShowDialog() == DialogResult.OK)
            {
                pin.Name = (string)vue.Data;

                BeginCanvasUpdate();
                UpdateExportPins();
                EndCanvasUpdate();

                if (_graph.ParentSubNode != null)
                {
                    //reflect pin name to outter subnode
                    _graph.ParentSubNode.OutputPins[_graph.ExportedPins.IndexOf(pin)].Name = (string)vue.Data;
                    RefreshGraph();
                }
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (_mouseOverPin != null)
            {
                // show the context menu for the pin instead
                e.Cancel = true;
                if ((_mouseOverPin.PinUIElements as PinUI).ContextMenuStrip != null)
                {
                    (_mouseOverPin.PinUIElements as PinUI).ContextMenuStrip.Show();
                }
            }
            else
            {
                // create context menu items based on state of graph
                contextMenuStrip.Items.Clear();

                // edit items
                bool editAdded = false;
                if (_trackingSelector != null)
                {
                    if (_trackingSelector.SelectedNodes.Count > 0)
                    {
                        ToolStripMenuItem cutMenuItem = new ToolStripMenuItem("Cut");
                        cutMenuItem.Click += new EventHandler(cutMenuItem_Click);
                        contextMenuStrip.Items.Add(cutMenuItem);

                        ToolStripMenuItem copyMenuItem = new ToolStripMenuItem("Copy");
                        copyMenuItem.Click += new EventHandler(copyMenuItem_Click);
                        contextMenuStrip.Items.Add(copyMenuItem);

                        editAdded = true;
                    }
                }

                if (Clipboard.GetDataObject().GetDataPresent(typeof(DaggerGraph)))
                {
                    ToolStripMenuItem pasteMenuItem = new ToolStripMenuItem("Paste");
                    pasteMenuItem.Click += new EventHandler(pasteMenuItem_Click);
                    contextMenuStrip.Items.Add(pasteMenuItem);
                    editAdded = true;
                }

                if (editAdded)
                {
                    contextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                if (AllowNodeProcessing && _graph.GraphScheduler != null)
                {
                    ToolStripMenuItem processGraphMenuItem = new ToolStripMenuItem("Process Graph");
                    processGraphMenuItem.Click += new EventHandler(processGraphMenuItem_Click);
                    contextMenuStrip.Items.Add(processGraphMenuItem);
                }

                if (_trackingSelector != null && _trackingSelector.SelectedNodes.Count != 0)
                {
                    ToolStripMenuItem stackNodesMenuItem = new ToolStripMenuItem("Stack Selected Nodes");
                    stackNodesMenuItem.Click += new EventHandler(stackNodesMenuItem_Click);
                    contextMenuStrip.Items.Add(stackNodesMenuItem);
                }

                if (AllNodes.Count > 0)
                {
                    ToolStripMenuItem autoArrangeMenuItem = new ToolStripMenuItem("Arrange Nodes");
                    autoArrangeMenuItem.Click += new EventHandler(autoArrangeMenuItem_Click);
                    contextMenuStrip.Items.Add(autoArrangeMenuItem);
                }

                ToolStripMenuItem noodleStyleMenuItem = new ToolStripMenuItem("Noodle Style");
                contextMenuStrip.Items.Add(noodleStyleMenuItem);

                // noodle style items
                noodleStyleMenuItem.DropDown.Items.Clear();
                for (int i = 0; i < 7; i++)
                {
                    ToolStripMenuItem tmi = new ToolStripMenuItem(((NoodleStyle)i).ToString());
                    tmi.Tag = i;
                    tmi.Click += new EventHandler(tmi_Click);
                    if (i == (int)_noodleStyle) tmi.Checked = true;
                    noodleStyleMenuItem.DropDown.Items.Add(tmi);
                }
            }
        }

        void stackNodesMenuItem_Click(object sender, EventArgs e)
        {
            //sort the selected uinode list by the vertical positions
            _trackingSelector.SelectedNodes.Sort(new UINodePositionComparer());

            foreach (IDaggerUINode node in _trackingSelector.SelectedNodes)
            {
                (node as DaggerUINode).Location = new Point(FocusLocationX, FocusLocationY);
                _focusPuck.Location = new Point(FocusLocationX, FocusLocationY + (node as DaggerUINode).Height + AutoArrangeHeightOffset);
            }
        }

        void processGraphMenuItem_Click(object sender, EventArgs e)
        {
            _graph.GraphScheduler.ProcessGraph();
        }

        void copyMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        void cutMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        void pasteMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        void tmi_Click(object sender, EventArgs e)
        {
            NoodleStyle = (NoodleStyle)(sender as ToolStripMenuItem).Tag;
        }

        private void autoArrangeMenuItem_Click(object sender, EventArgs e)
        {
            ArrangeNodes(AutoArrangeStyle.All);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Copy the setting of this UI Graph into another UI Graph
        /// </summary>
        /// <param name="toGraph"></param>
        internal void CopySettings(DaggerUIGraph toGraph)
        {
            toGraph.AllowDeleteRelink = this.AllowDeleteRelink;
            toGraph.AllowNodeProcessing = this.AllowNodeProcessing;
            toGraph.AllowNoodleBisecting = this.AllowNoodleBisecting;
            toGraph.AllowPinExport = this.AllowPinExport;
            toGraph.AllowPinSetValue = this.AllowPinSetValue;
            toGraph.AllowSubNodes = this.AllowSubNodes;
            toGraph.AutoArrangeHeightOffset = this.AutoArrangeHeightOffset;
            toGraph.AutoArrangeWidthOffset = this.AutoArrangeWidthOffset;
            toGraph.DropShadowAlpha = this.DropShadowAlpha;
            toGraph.DropShadowVisible = this.DropShadowVisible;
            toGraph.DropShadowXOffset = this.DropShadowXOffset;
            toGraph.DropShadowYOffset = this.DropShadowYOffset;
            toGraph.NoodleStyle = this.NoodleStyle;
            toGraph.PinLegend = this.PinLegend;
            toGraph.ShowOrdinal = this.ShowOrdinal;
        }

        /// <summary>
        /// Delete a Node and relink it's pin connections
        /// </summary>
        /// <param name="node"></param>
        internal void DeleteAndRelink(DaggerNode node)
        {
            //store the pins that we will be relinking
            List<DaggerOutputPin> fromPins = new List<DaggerOutputPin>();
            foreach (DaggerInputPin pin in node.InputPins.ConnectedPins)
            {
                fromPins.Add(pin.ConnectedToOutput);
            }
            List<DaggerInputPin>[] toPins = new List<DaggerInputPin>[fromPins.Count];
            for (int i = 0; i < fromPins.Count; i++)
            {
                toPins[i] = new List<DaggerInputPin>();
                foreach (DaggerInputPin pin in node.OutputPins.ConnectedPins[i].ConnectedTo)
                {
                    toPins[i].Add(pin);
                }
            }

            //kill the node
            if (!_graph.DeleteNode(node))
            {
                //failed to delete node or one of it's connections
                RefreshGraph();
                return;
            }

            //perform relinking
            for (int i = 0; i < fromPins.Count; i++)
            {
                foreach(DaggerInputPin pin in toPins[i])
                {
                    if(!fromPins[i].ConnectToInput(pin))
                    {
                        //failed to reconnect a pin
                        RefreshGraph();
                        return;
                    }
                }
            }

            //all done, refresh the UIGraph to show new connections
            RefreshGraph();
        }

        public IDaggerNoodle AddNoodle(DaggerOutputPin output, DaggerInputPin input)
        {
            if (output.PinUIElements == null) output.PinUIElements = new PinUI(output);
            if (input.PinUIElements == null) input.PinUIElements = new PinUI(input);
            IDaggerNoodle noodle = new DaggerNoodle(output,input);
            _noodles.Add(noodle as DaggerNoodle);

            if (NoodleAdded != null)
            {
                NoodleAdded(noodle.OutputPin, noodle.InputPin);
            }

            return noodle;
        }

        internal void BeginOutputPinConnect(DaggerOutputPin pin)
        {
            int pinSize = (pin.ParentNode == null) ? _pinSize : pin.ParentNode.UINode.PinSize;
            Control parentControl = (pin.ParentNode == null) ? (Control)this : (Control)pin.ParentNode.UINode;
            
            //convert pin location to our client space
            Point pinCenter = new Point((pin.PinUIElements as PinUI).PinLocation.X + (pinSize / 2), (pin.PinUIElements as PinUI).PinLocation.Y + (pinSize / 2));
            Point screenPoint = parentControl.PointToScreen(pinCenter);

            //if we are already tracking a noodle, cancel it
            StopPinConnect();

            if (parentControl == this)
            {
                screenPoint = new Point(screenPoint.X + AutoScrollPosition.X, screenPoint.Y + AutoScrollPosition.Y);
            }

            _trackingConnectPin = new DraggingNoodle(PointToClient(screenPoint), pin);

            //hook the mouse so we can redraw the noodle even when the mouse is over another control
            _setMouseHook();

            Invalidate(false);
        }

        internal void BeginInputPinConnect(DaggerInputPin pin)
        {
            int pinSize = (pin.ParentNode == null) ? _pinSize : pin.ParentNode.UINode.PinSize;
            Control parentControl = (pin.ParentNode == null) ? (Control)this : (Control)pin.ParentNode.UINode;

            //convert pin location to our client space
            Point pinCenter = new Point((pin.PinUIElements as PinUI).PinLocation.X + (pinSize / 2), (pin.PinUIElements as PinUI).PinLocation.Y + (pinSize / 2));
            Point screenPoint = parentControl.PointToScreen(pinCenter);

            //if we are already tracking a noodle, cancel it
            StopPinConnect();

            if (parentControl == this)
            {
                screenPoint = new Point(screenPoint.X + AutoScrollPosition.X, screenPoint.Y + AutoScrollPosition.Y);
            }

            _trackingConnectPin = new DraggingNoodle(PointToClient(screenPoint), pin);

            //hook the mouse so we can redraw the noodle even when the mouse is over another control
            _setMouseHook();

            Invalidate(false);
        }

        /// <summary>
        /// Sever a noodle and insert a type compatible node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="noodle"></param>
        private void BisectNoodle(DaggerNode node, DaggerNoodle noodle)
        {
            //try to disconnection exisiting noodle
            if (!noodle.OutputPin.Disconnect(noodle.InputPin,false))
            {
                //failed to disconnect noodle
                return;
            }

            if (!noodle.OutputPin.ConnectToInput(node.InputPins[0]))
            {
                //failed to reconnect to input pin
                return;
            }

            if (!node.OutputPins[0].ConnectToInput(noodle.InputPin))
            {
                //failed to connect output
                return;
            }
        }

        internal void EndPinConnect(DaggerInputPin connectTo)
        {
            if (_trackingConnectPin != null)
            {
                //store the pin that this Input Pin may already be connected to
                DaggerOutputPin alreadyconnectedpin = connectTo.ConnectedToOutput;

                //if this Input pin was already connected, try to disconnect it now
                if (alreadyconnectedpin != null)
                {
                    if (!alreadyconnectedpin.Disconnect(connectTo,false))
                    {
                        //we failed to disconnect existing connection
                        //cancel connection operations
                        StopPinConnect();
                        Invalidate(false);
                        return;
                    }
                }

                //connect the two pins, add the noodle to collection
                _trackingConnectPin.OutputPin.ConnectToInput(connectTo);
                StopPinConnect();

                //if this Input pin was originally connected, perform a reconnect on previous Output pin
                if (alreadyconnectedpin != null)
                {
                    BeginOutputPinConnect(alreadyconnectedpin);
                }

                RefreshGraph();
            }
        }

        internal void EndPinConnect(DaggerOutputPin connectTo)
        {
            if (_trackingConnectPin != null)
            {
                //connect the two pins, add the noodle to collection
                connectTo.ConnectToInput(_trackingConnectPin.InputPin);
                StopPinConnect();
                RefreshGraph();
            }
        }

        /// <summary>
        /// Stops a pin dragging operation and unhooks mouse/keyboard
        /// </summary>
        public void StopPinConnect()
        {
            if (_trackingConnectPin != null)
            {
                _trackingConnectPin = null;

                //unhook the Mouse Proc
                _unsetMouseHook();
            }
        }

        /// <summary>
        /// Called from a DaggerNode when it has been resized
        /// </summary>
        /// <param name="bn"></param>
        internal void NodeResized(DaggerNode bn)
        {
            BeginCanvasUpdate();

            if (AutoSizeMode == AutoSizeMode.GrowOnly)
            {
                Size actual = ActualCanvasSize;
                Size newSize = new Size(Math.Max(actual.Width, CanvasSize.Width), Math.Max(actual.Height, CanvasSize.Height));
                if (newSize.Width != actual.Width || newSize.Height != actual.Height)
                {
                    CanvasSize = newSize;
                }
            }

            UpdateExportPins();
            UpdateImportPins();
            UpdateNoodles(bn);

            EndCanvasUpdate();
        }

        /// <summary>
        /// Called from a DaggerNode when it has been moved
        /// </summary>
        /// <param name="bn"></param>
        internal void NodeMoved(DaggerNode bn)
        {
            if (!_isScrolling)
            {
                BeginCanvasUpdate();
                if (AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    Size actual = ActualCanvasSize;
                    Size newSize = new Size(Math.Max(actual.Width, CanvasSize.Width), Math.Max(actual.Height, CanvasSize.Height));
                    if (newSize.Width != actual.Width || newSize.Height != actual.Height)
                    {
                        CanvasSize = newSize;
                    }
                }

                UpdateExportPins();
                UpdateImportPins();
                UpdateNoodles(bn);
                EndCanvasUpdate();
            }
        }

        public void OnImportPinAdded(DaggerOutputPin pin)
        {
            // create context menu for imported pin
            ContextMenuStrip csi = new ContextMenuStrip();
            ToolStripMenuItem tsmi = new ToolStripMenuItem("Rename");
            tsmi.Tag = pin;
            tsmi.Click += new EventHandler(_renameImportedPin_Click);
            csi.Items.Add(tsmi);
            if (pin.PinUIElements == null) pin.PinUIElements = new PinUI(pin);
            (pin.PinUIElements as PinUI).ContextMenuStrip = csi;

            //recalculate Imorted pin layout
            BeginCanvasUpdate();
            UpdateImportPins();
            EndCanvasUpdate();
        }

        public void OnImportPinRemoved(DaggerOutputPin pin)
        {
            BeginCanvasUpdate();
            UpdateImportPins();
            EndCanvasUpdate();
        }

        public void OnExportPinAdded(DaggerInputPin pin)
        {
            // create context menu for exported pin
            ContextMenuStrip csi = new ContextMenuStrip();
            ToolStripMenuItem tsmi = new ToolStripMenuItem("Rename");
            tsmi.Tag = pin;
            tsmi.Click += new EventHandler(_renameExportedPin_Click);
            csi.Items.Add(tsmi);
            if (pin.PinUIElements == null) pin.PinUIElements = new PinUI(pin);
            (pin.PinUIElements as PinUI).ContextMenuStrip = csi;

            BeginCanvasUpdate();
            UpdateExportPins();
            EndCanvasUpdate();
        }

        public void OnExportPinRemoved(DaggerInputPin pin)
        {
            BeginCanvasUpdate();
            UpdateExportPins();
            EndCanvasUpdate();
        }

        public void UpdateImportPins()
        {
            _widestImportName = 0;

            Graphics g = Graphics.FromHwnd(this.Handle);
         
            //measure each name and find the widest one
            foreach (DaggerOutputPin pin in _graph.ImportedPins)
            {
                if (pin.PinUIElements == null) pin.PinUIElements = new PinUI(pin);
                SizeF s = g.MeasureString(pin.Name, Font);
                _widestImportName = Math.Max(_widestImportName, s.Width);
                _highestImportName = Math.Max(_highestImportName, s.Height);
            }

            int topOffset = 20;

            //if this is the internal graph of a subnode, reflect outside pin locations to inside
            if (_graph.ParentSubNode != null)
            {
                DaggerUINode node = _graph.ParentSubNode.UINode as DaggerUINode;

                if (node._node.InputPins.Count > 0)
                {
                    topOffset = PointToClient(node.PointToScreen((node._node.InputPins[0].PinUIElements as PinUI).PinLocation)).Y - this._pinLegend.PinSize / 2;
                }
            }

            //update the pin positions
            foreach (DaggerOutputPin pin in _graph.ImportedPins)
            {
                (pin.PinUIElements as PinUI).PinLocation = new Point((int)_widestImportName + _pinSpacing, topOffset + (int)(_highestImportName / 2));
                (pin.PinUIElements as PinUI).TargetRegion = new Region(new Rectangle((pin.PinUIElements as PinUI).PinLocation, new Size(_pinSize, _pinSize)));

                topOffset += (int)_highestImportName + _pinSpacing;
            }

            _noodles.UpdateNoodles();

            g.Dispose();
        }

        public void UpdateExportPins()
        {
            _widestExportName = 0;

            Graphics g = Graphics.FromHwnd(this.Handle);

            //measure each name and find the widest one
            foreach (DaggerInputPin pin in _graph.ExportedPins)
            {
                if (pin.PinUIElements == null) pin.PinUIElements = new PinUI(pin);
                SizeF s = g.MeasureString(pin.Name, Font);
                _widestExportName = Math.Max(_widestExportName, s.Width);
                _highestExportName = Math.Max(_highestExportName, s.Height);
            }

            int topOffset = 20;

            //if this is the internal graph of a subnode, reflect outside pin locations to inside
            if (_graph.ParentSubNode != null)
            {
                DaggerUINode node = _graph.ParentSubNode.UINode as DaggerUINode;

                if (node._node.OutputPins.Count > 0)
                {
                    topOffset = PointToClient(node.PointToScreen((node._node.OutputPins[0].PinUIElements as PinUI).PinLocation)).Y - this._pinLegend.PinSize / 2;
                }
            }

            //update the pin positions
            foreach (DaggerInputPin pin in _graph.ExportedPins)
            {
                (pin.PinUIElements as PinUI).PinLocation = new Point(CanvasSize.Width - (int)_widestExportName - _pinSpacing - _pinSize, topOffset + (int)(_highestExportName / 2));
                (pin.PinUIElements as PinUI).TargetRegion = new Region(new Rectangle((pin.PinUIElements as PinUI).PinLocation, new Size(_pinSize, _pinSize)));

                topOffset += (int)_highestExportName + _pinSpacing;
            }

            _noodles.UpdateNoodles();
            g.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called from DaggerNode.OnAfterRemoved.  Remove node from panel and _trackingSelector
        /// </summary>
        public virtual void OnAfterNodeRemoved(IDaggerUINode uinode)
        {
            if (_trackingSelector != null)
            {
                _trackingSelector.SelectedNodes.Remove(uinode);
            }

            this.Controls.Remove(uinode as DaggerUINode);
        }

        /// <summary>
        /// Recreate all the noodle paths
        /// </summary>
        /// <param name="bn"></param>
        public void UpdateNoodles(DaggerNode bn)
        {
            _noodles.UpdateNoodles();
        }

        /// <summary>
        /// Find and delete noodle connecting these two pins
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        private void DeleteNoodle(DaggerOutputPin output, DaggerInputPin input)
        {
            foreach (DaggerNoodle noodle in _noodles.Noodles)
            {
                if ((noodle.InputPin == input) && (noodle.OutputPin == output))
                {
                    DeleteNoodle(noodle);
                    break;
                }
            }
        }

        private void DeleteNoodle(DaggerNoodle noodle)
        {
            //remove from the list
            _noodles.Remove(noodle);

            //remove from the _trackingSelector
            if (_trackingSelector != null)
            {
                _trackingSelector.SelectedNoodles.Remove(noodle);
            }

            if (NoodleRemoved != null)
            {
                NoodleRemoved(noodle.OutputPin, noodle.InputPin);
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
            _mouseOverPin = null;

            //offset clientMousePosition to account for scroll bars
            Point actualPosition = new Point(clientMousePosition.X - AutoScrollPosition.X, clientMousePosition.Y - AutoScrollPosition.Y);

            //Check if over Input pin
            foreach (DaggerInputPin pin in _graph.ExportedPins)
            {
                if ((pin.PinUIElements as PinUI).TargetRegion.IsVisible(actualPosition))
                {
                    alterState = DaggerNodeAlterState.ConnectFromInput;
                    _mouseOverPin = pin;
                    break;
                }
            }

            //Check if over Output pin
            foreach (DaggerOutputPin pin in _graph.ImportedPins)
            {
                if ((pin.PinUIElements as PinUI).TargetRegion.IsVisible(actualPosition))
                {
                    alterState = DaggerNodeAlterState.ConnectFromOutput;
                    _mouseOverPin = pin;
                    break;
                }
            }
            return alterState;
        }

        /// <summary>
        /// Called when the scroll positions have changed to update the tracking points of a connecting noodle
        /// </summary>
        private void UpdateTrackingNoodle()
        {
            if (_trackingConnectPin != null)
            {
                Point startPoint = new Point(0, 0);
                //translate the pin locations to the uigraph client coords
                if (_trackingConnectPin.InputPin != null)
                {
                    if (_trackingConnectPin.InputPin.ParentNode != null)
                    {   
                        // Tracking pin is from a UI Node
                        startPoint = (_trackingConnectPin.InputPin.ParentNode.UINode as DaggerUINode).PointToScreen((_trackingConnectPin.InputPin.PinUIElements as PinUI).PinLocation);
                        startPoint.X += _trackingConnectPin.InputPin.ParentNode.UINode.PinSize / 2;
                        startPoint.Y += _trackingConnectPin.InputPin.ParentNode.UINode.PinSize / 2;                        
                    }
                    else
                    {
                        // Tracking pin is an exported pin
                        startPoint = (_trackingConnectPin.InputPin.ParentGraph.ParentUIGraph as DaggerUIGraph).PointToScreen((_trackingConnectPin.InputPin.PinUIElements as PinUI).PinLocation);
                        startPoint.X += _trackingConnectPin.InputPin.ParentGraph.ParentUIGraph.PinSize / 2 + AutoScrollPosition.X;
                        startPoint.Y += _trackingConnectPin.InputPin.ParentGraph.ParentUIGraph.PinSize / 2 + AutoScrollPosition.Y
                            ;  
                    }
                }
                else if (_trackingConnectPin.OutputPin != null)
                {
                    if (_trackingConnectPin.OutputPin.ParentNode != null)
                    {
                        // Tracking pin is from a UI Node
                        startPoint = (_trackingConnectPin.OutputPin.ParentNode.UINode as DaggerUINode).PointToScreen((_trackingConnectPin.OutputPin.PinUIElements as PinUI).PinLocation);
                        startPoint.X += _trackingConnectPin.OutputPin.ParentNode.UINode.PinSize / 2;
                        startPoint.Y += _trackingConnectPin.OutputPin.ParentNode.UINode.PinSize / 2;
                    }
                    else
                    {
                        // Tracking pin is an imported pin
                        startPoint = (_trackingConnectPin.OutputPin.ParentGraph.ParentUIGraph as DaggerUIGraph).PointToScreen((_trackingConnectPin.OutputPin.PinUIElements as PinUI).PinLocation);
                        startPoint.X += _trackingConnectPin.OutputPin.ParentGraph.ParentUIGraph.PinSize / 2 + AutoScrollPosition.X;
                        startPoint.Y += _trackingConnectPin.OutputPin.ParentGraph.ParentUIGraph.PinSize / 2 + AutoScrollPosition.Y;  
                    }
                }
                // update the start point
                _trackingConnectPin.StartPoint = PointToClient(new Point(startPoint.X + AutoScrollOffset.X, startPoint.Y + AutoScrollOffset.Y));
            }
        }

        #endregion

        #region Public Methods

        public int BeginNoodleUpdate()
        {
            return _noodles.BeginUpdate();
        }

        public int EndNoodleUpdate()
        {
            return _noodles.EndUpdate();
        }

        public void ScrollToPosition(Point newPosition)
        {
            // constrain the scroll position to the actual canvas size
            Size actual = this.ActualCanvasSize;
            newPosition = new Point(Math.Min(actual.Width, newPosition.X), Math.Min(actual.Height, newPosition.Y));

            _isScrolling = true;
            AutoScrollPosition = newPosition;
            _focusPuck.Location = newPosition;
            _isScrolling = false;

            if (_trackingConnectPin != null)
            {
                UpdateTrackingNoodle();
            }

            if (ViewportChanged != null)
            {
                ViewportChanged(this, new EventArgs());
            }
        }

        public int BeginCanvasUpdate()
        {
            return ++_updateRef;
        }

        public int EndCanvasUpdate()
        {
            _updateRef--;

            if (_updateRef <= 0)
            {
                _updateRef = 0;
                RefreshGraph();
            }

            return _updateRef;
        }

        /// <summary>
        /// Save the DaggerGraph to a File
        /// </summary>
        /// <param name="path"></param>
        public void SaveGraph(string path)
        {
            byte[] buffer = null;
            buffer = Graph.SerializeGraph();
            Stream bufStream = null;
            if ((bufStream = File.OpenWrite(path)) != null)
            {
                bufStream.Write(buffer, 0, (int)buffer.Length);
                // Code to write the stream goes here.
                bufStream.Close();
            }
        }

        /// <summary>
        /// Clear the existing DaggerGraph and load a new one in it's place
        /// </summary>
        /// <param name="path"></param>
        public void LoadGraph(string path)
        {
            if (!Graph.ClearGraph())
            {
                throw new InvalidOperationException("Error clearing DaggerGraph");
            }

            Stream filestream = File.OpenRead(path);
            BinaryFormatter bformatter = new BinaryFormatter();

            // deserialize into a new graph
            DaggerGraph newgraph = (DaggerGraph)bformatter.Deserialize(filestream);

            if (newgraph != null)
            {
                _isScrolling = true;
                _noodles.BeginUpdate();
                BeginCanvasUpdate();
                Graph.AppendGraph(newgraph);
                _noodles.EndUpdate();
                EndCanvasUpdate();
                _isScrolling = false;
            }
        }

        public void Select(List<IDaggerUINode> nodes, List<IDaggerNoodle> noodles)
        {
            _trackingSelector = new Selector(nodes, noodles);
        }

        public void AddNode(DaggerNode node)
        {
            //try to create an instance of the UI element
            Type t = Type.GetType(node.AssociatedUINode);
            DaggerUINode uinode = (DaggerUINode)Activator.CreateInstance(t);

            if (uinode == null)
            {
                //couldn't make a ui element for this node
                throw new InvalidOperationException("Unable to create a " + node.AssociatedUINode.ToString());
            }

            uinode.Node = node;
            _graph.AddNode(node);
            Controls.Add(uinode);
        }

        /// <summary>
        /// Recreate and redraw graph background
        /// </summary>
        public void RefreshGraph()
        {
            CreateCanvasImage();
            Invalidate(false);
        }

        private void ArrangeNodes()
        {
            ArrangeNodes(_autoArrangeStyle);
        }

        public void ArrangeNodes(AutoArrangeStyle style)
        {
            if (style == AutoArrangeStyle.None || (_graph == null) )
            {
                return;
            }

            if (_graph.AllNodes.Count == 0)
            {
                return;
            }

            int subgraphoffset = 0;
            for (int sgraph = 0; sgraph < _graph.SubGraphCount; sgraph++)
            {
                int maxPrcHeight = 0;
                int numOrdinals = _graph.OrdinalCount(sgraph);
                int ordOffset = 0;

                int[] precHeights = new int[numOrdinals];
                int[] precWidths = new int[numOrdinals];

                //find the width and height of each ordinal and the largest ordinal height
                for (int i = 0; i < numOrdinals; i++)
                {
                    foreach (DaggerNode node in _graph[sgraph, i])
                    {
                        if (node.UINode != null)
                        {
                            precHeights[i] += (node.UINode as DaggerUINode).AutoArrangeSize.Height + 10;
                            precWidths[i] = Math.Max(precWidths[i], (node.UINode as DaggerUINode).AutoArrangeSize.Width);
                            maxPrcHeight = Math.Max(maxPrcHeight, precHeights[i]);
                        }
                    }
                }

                //go through each node and set it's new position
                for (int i = 0; i < numOrdinals; i++)
                {
                    int ordTop = 0;
                    List<DaggerNode> ordinalList = _graph[sgraph, i];

                    //sort the ordinal list by thier vertical positions
                    ordinalList.Sort(new NodePositionComparer());
                    foreach (DaggerNode node in ordinalList)
                    {
                        if (node.UINode != null)
                        {
                            Point loc = loc = new Point(ordOffset, node.UINode.Top);
                            if (style == AutoArrangeStyle.All)
                            {
                                //Adjust for vertical positioning also
                                loc.Y = subgraphoffset + ordTop;
                                ordTop += node.UINode.Height + _autoArrangeHeightOffSet;

                                // adjust for user defined offset
                                loc.X += (node.UINode as DaggerUINode).AutoArrangeOffset.X;
                                loc.Y += (node.UINode as DaggerUINode).AutoArrangeOffset.Y;
                            }
                            (node.UINode as DaggerUINode).Location = loc;
                        }
                    }

                    ordOffset += precWidths[i] + _autoArrangeWidthOffset;
                }

                subgraphoffset += maxPrcHeight;
            }

            CanvasSize = ActualCanvasSize;
        }

        public void Paste()
        {
            if (Clipboard.GetDataObject().GetDataPresent(typeof(DaggerGraph)))
            {
                try
                {
                    DaggerGraph g = (DaggerGraph)Clipboard.GetDataObject().GetData(typeof(DaggerGraph));
                    if (g != null)
                    {
                        _noodles.BeginUpdate();
                        BeginCanvasUpdate();
                        _graph.AppendGraph(g);
                        _noodles.EndUpdate();
                        EndCanvasUpdate();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show("Error " + ex.Message + " deserializing DaggerGraph");
#endif
                }
            }
        }

        public void Copy()
        {
            if (_trackingSelector == null || _trackingSelector.IsEmpty)
            {
                //copy the entire graph to clip board
                Clipboard.SetDataObject(_graph, true);
            }
            else
            {
                // set the graph to only serialize the selection
                _graph.SelectionToSerialize = _trackingSelector;
                Clipboard.SetDataObject(_graph, true);
                _graph.SelectionToSerialize = null;
            }
        }

        public void Cut()
        {
            // copy selected (or entire graph)
            Copy();

            // if copying entire graph, select all, then delete
            if (_trackingSelector == null || _trackingSelector.IsEmpty)
            {
                _trackingSelector = new Selector(this);
            }
            DeleteSelected();
        }

        public void SelectAll()
        {
            _trackingSelector = new Selector(this);
            RefreshGraph();
        }


        /// <summary>
        /// Delete objects selected in _trackingSelector
        /// </summary>
        public void DeleteSelected()
        {
            // is there is anything to delete?
            if (_trackingSelector == null)
            {
                return;
            }

            // code reentry here can be devistating and the delete key handler in KeyboardProc is finicky
            // we'll use a volatile flag instead of lock to prevent reentry from the same process
            if (!_deletingSelection)
            {                
                _deletingSelection = true;

                if (_trackingSelector.SelectedNodes.Count == 0 && _trackingSelector.SelectedNoodles.Count == 0)
                {
                    _deletingSelection = false;
                    return;
                }

                // are we allowed to delete right now?
                if (BeforeSelectionDeleted != null && !BeforeSelectionDeleted(this))
                {
                    _deletingSelection = false;
                    return;
                }

                // prevent the noodles from updating on every deletion call
                _noodles.BeginUpdate();
                BeginCanvasUpdate();

                //disconnect and delete all selected Nodes first
                for (int i = _trackingSelector.SelectedNodes.Count - 1; i > -1; i--)
                {
                    IDaggerUINode node = _trackingSelector.SelectedNodes[i];

                    if (!_graph.DeleteNode(node.Node))
                    {
                        //failed to delete a node
                        _noodles.EndUpdate();
                        EndCanvasUpdate();

                        _deletingSelection = false;
                        return;
                    }
                }

                //disconnect and delete remaining selected noodels
                //Go in reverse because we are going to be deleteing them from an iterated list
                for (int i = _trackingSelector.SelectedNoodles.Count - 1; i > -1; i--)
                {
                    IDaggerNoodle noodle = _trackingSelector.SelectedNoodles[i];

                    if (!noodle.Disconnect())
                    {
                        //we failed to disconnect a noodle so break operations
                        break;
                    }
                }

                _trackingSelector = null;

                _noodles.EndUpdate();
                EndCanvasUpdate();

                // raise event too show selection has been deleted
                if (AfterSelectionDeleted != null)
                {
                    AfterSelectionDeleted(this);
                }
                _deletingSelection = false;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// creates and overlays the dropshadow on the Canvas Image
        /// </summary>
        private void CreateDropShadowImage()
        {
            Bitmap tempb = new Bitmap(ActualCanvasSize.Width, ActualCanvasSize.Height);
            Graphics g = Graphics.FromImage(tempb);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (DaggerUINode node in AllNodes)
            {
                Region r = new Region(node.Region.GetRegionData());
                Matrix translateMatrix = new Matrix();
                translateMatrix.Translate((node.Left + _dropShadowXoffset) - AutoScrollPosition.X, (node.Top + _dropShadowYoffset) - AutoScrollPosition.Y);
                r.Transform(translateMatrix);
                g.FillRegion(Brushes.Black, r);
                r.Dispose();
            }

            Pen blp = new Pen(Color.Black);

            blp.Width = 5;
            blp.StartCap = LineCap.Round;
            blp.EndCap = LineCap.Round;

            //translate the path to account for drop shadow offset
            Matrix autoscrollMatrix = new Matrix();
            autoscrollMatrix.Translate(_dropShadowXoffset, _dropShadowYoffset);

            //draw all the Noodles currently in the graph
            foreach (DaggerNoodle noodle in _noodles.Noodles)
            {
                GraphicsPath newpath = (GraphicsPath)noodle.path.Clone();
                newpath.Transform(autoscrollMatrix);
                g.DrawPath(blp, newpath);
                newpath.Dispose();
            }

            g.Dispose();
            blp.Dispose();

            g = Graphics.FromImage(_canvasImage);

            //overlay the dropshadow bitmap with transparency
            float[][] transparentElements = { 
                new float[] {0.75F,0,0,0,0},
                new float[] {0,0.75F,0,0,0},
                new float[] {0,0,0.75F,0,0},
                new float[] {0,0,0,_dropShadowAlpha,0},
                new float[] {0,0,0,0,1} };
            ColorMatrix transMatrix = new ColorMatrix(transparentElements);
            ImageAttributes imgAttr = new ImageAttributes();
            imgAttr.SetColorMatrix(transMatrix);

            g.DrawImage(tempb,
                new Rectangle(0,0,tempb.Width,tempb.Height),
                0,
                0,
                tempb.Width,
                tempb.Height,
                GraphicsUnit.Pixel,
                imgAttr);

            g.Dispose();
            tempb.Dispose();
        }

        private void CreateCanvasImage()
        {
            if (_canvasImage != null)
            {
                _canvasImage.Dispose();
                _canvasImage = null;
            }

            _canvasImage = new Bitmap(ActualCanvasSize.Width, ActualCanvasSize.Height);
            Graphics g = Graphics.FromImage(_canvasImage);

            // clear the canvas with BackColor or BackgroundImage
            if (BackgroundImage != null)
            {
                // Tile the background image
                TextureBrush myTBrush = new TextureBrush(BackgroundImage);
                g.FillRectangle(myTBrush, 0, 0, _canvasImage.Width, _canvasImage.Height);
                myTBrush.Dispose();
            }
            else
            {
                g.Clear(BackColor);
            }

            if (_dropShadowVisible)
            {
                CreateDropShadowImage();
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            //noodle non-selected pen
            Pen blp = new Pen(ControlPaint.DarkDark(BackColor));

            blp.Width = 5;
            blp.StartCap = LineCap.Round;
            blp.EndCap = LineCap.Round;

            //noodle selected pen
            Pen wp = new Pen(ControlPaint.LightLight(BackColor));
            wp.Width = 5;
            wp.StartCap = LineCap.Round;
            wp.EndCap = LineCap.Round;

            //draw all the Noodles currently in the graph
            foreach (DaggerNoodle noodle in _noodles.Noodles)
            {
                Pen yp;
                if ((noodle.OutputPin.PinUIElements as PinUI).NoodleColor == (noodle.InputPin.PinUIElements as PinUI).NoodleColor)
                {
                    yp = new Pen((noodle.OutputPin.PinUIElements as PinUI).NoodleColor);
                }
                else
                {
                    //create a linear gradient from output to input

                    Point outputpoint = new Point(noodle.OutputPoint.X + AutoScrollPosition.X, noodle.OutputPoint.Y + AutoScrollPosition.Y);
                    Point inputpoint = new Point(noodle.InputPoint.X + AutoScrollPosition.X, noodle.InputPoint.Y + AutoScrollPosition.Y);

                    if (inputpoint.X < outputpoint.X)
                    {
                        //adjust points for curve throw
                        RectangleF pathbounds = noodle.path.GetBounds();
                        inputpoint.X = (int)pathbounds.X;
                        outputpoint.X = (int)pathbounds.Right;
                    }

                    Brush lb = new LinearGradientBrush(outputpoint, inputpoint, (noodle.OutputPin.PinUIElements as PinUI).NoodleColor, (noodle.InputPin.PinUIElements as PinUI).NoodleColor);

                    yp = new Pen(lb);
                    lb.Dispose();
                }

                yp.StartCap = LineCap.Round;
                yp.EndCap = LineCap.Round;
                yp.Width = 2.0f;

                //if this noodle is marked as "Bisectable" set the pen to dashed
                if (_noodleBisector != null && _noodleBisector.Noodle == noodle)
                {
                    yp.DashStyle = DashStyle.DashDot;
                }

                //draw the outline of the noodle depending on selcted/not selected
                if (_trackingSelector != null)
                {
                    g.DrawPath(_trackingSelector.SelectedNoodles.Contains(noodle) ? wp : blp, noodle.path);
                }
                else
                {
                    g.DrawPath(blp, noodle.path);
                }

                //draw the noodle's color
                g.DrawPath(yp, noodle.path);

                yp.Dispose();
            }

            blp.Dispose();
            wp.Dispose();

            // outline all the selected nodes
            List<IDaggerUINode> snodes = SelectedNodes;
            if (snodes.Count > 0)
            {
                GraphicsPath _selectedNodesPath = new GraphicsPath();
                foreach (IDaggerUINode node in snodes)
                {
                    Rectangle noderect = new Rectangle((node as DaggerUINode).Left - AutoScrollPosition.X, (node as DaggerUINode).Top - AutoScrollPosition.Y, (node as DaggerUINode).Width, (node as DaggerUINode).Height);
                    noderect.Inflate(new Size(2, 2));
                    _selectedNodesPath.AddRectangle(noderect);
                }

                using (Pen selectpen = new Pen(Color.Black))
                {
                    selectpen.Width = 1;
                    selectpen.DashStyle = DashStyle.Dash;

                    using (Brush selectbrush = new SolidBrush(Color.FromArgb(100, ControlPaint.LightLight(BackColor))))
                    {
                        g.FillPath(selectbrush, _selectedNodesPath);
                        g.DrawPath(selectpen, _selectedNodesPath);
                    }
                }
                _selectedNodesPath.Dispose();
            }

            if (_graph.ImportedPins.Count > 0 || _graph.ExportedPins.Count > 0)
            {
                Brush backB = new SolidBrush(Color.FromArgb(100, Color.Blue));
                Brush fontB = new SolidBrush(ForeColor);

                //draw imported pins
                if (_graph.ImportedPins.Count > 0)
                {
                    g.FillRectangle(backB, 0, 0, _widestImportName + _pinSpacing, ActualCanvasSize.Height);

                    foreach (DaggerOutputPin pin in _graph.ImportedPins)
                    {
                        if (pin.PinUIElements != null)
                        {
                            g.DrawString(pin.Name, 
                                Font, 
                                fontB, 
                                new RectangleF(0, (pin.PinUIElements as PinUI).PinLocation.Y, 
                                _widestImportName, 
                                20));

                            ImageAttributes att = new ImageAttributes();
                            Color alphakey = (pin.PinUIElements as PinUI).PinImageConnectedTransparent;
                            att.SetColorKey(alphakey, alphakey);

                            g.DrawImage((pin.PinUIElements as PinUI).PinImageConnected, 
                                new Rectangle((pin.PinUIElements as PinUI).PinLocation.X, 
                                (pin.PinUIElements as PinUI).PinLocation.Y, 
                                _pinLegend.PinSize, _pinLegend.PinSize), 
                                0, 0, 
                                _pinLegend.PinSize, 
                                _pinLegend.PinSize, 
                                GraphicsUnit.Pixel, 
                                att);
                        }
                    }
                }

                //draw exported pins
                if (_graph.ExportedPins.Count > 0)
                {
                    g.FillRectangle(backB, 
                        CanvasSize.Width - _widestExportName - _pinSpacing, 
                        0, 
                        _widestExportName,
                        ActualCanvasSize.Height);

                    foreach (DaggerInputPin pin in _graph.ExportedPins)
                    {
                        if (pin.PinUIElements != null)
                        {
                            g.DrawString(pin.Name,
                                Font,
                                fontB,
                                new RectangleF(CanvasSize.Width - _widestExportName - _pinSpacing, (pin.PinUIElements as PinUI).PinLocation.Y, _widestExportName, 20));
                            
                            ImageAttributes att = new ImageAttributes();
                            Color alphakey = (pin.PinUIElements as PinUI).PinImageConnectedTransparent;
                            att.SetColorKey(alphakey, alphakey);

                            g.DrawImage((pin.PinUIElements as PinUI).PinImageConnected, 
                                new Rectangle((pin.PinUIElements as PinUI).PinLocation.X, 
                                (pin.PinUIElements as PinUI).PinLocation.Y, 
                                _pinLegend.PinSize, 
                                _pinLegend.PinSize), 
                                0, 0, 
                                _pinLegend.PinSize, 
                                _pinLegend.PinSize, 
                                GraphicsUnit.Pixel,
                                att);
                        }
                    }
                }
                backB.Dispose();
            }

            if (CanvasImageChanged != null)
            {
                CanvasImageChanged(this, new EventArgs());
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_canvasImage == null)
            {
                CreateCanvasImage();
            }

            Rectangle sourceRect = new Rectangle(AutoScrollPosition.X * -1, 
                AutoScrollPosition.Y * -1, 
                Width, 
                Height);

            e.Graphics.DrawImage(_canvasImage, new Rectangle(0, 0, Width, Height), sourceRect, GraphicsUnit.Pixel);

            // draw the noodle that is currently being dragged
            if (_trackingConnectPin != null)
            {
                Pen bp = new Pen(Color.Black);
                bp.Width = 5;
                bp.StartCap = LineCap.Round;
                bp.EndCap = LineCap.Round;

                Pen np = new Pen((_trackingConnectPin.OutputPin != null) ? (_trackingConnectPin.OutputPin.PinUIElements as PinUI).NoodleColor : (_trackingConnectPin.InputPin.PinUIElements as PinUI).NoodleColor);
                np.DashStyle = DashStyle.Dash;
                np.Width = 3;
                np.StartCap = LineCap.Round;
                np.EndCap = LineCap.Round;

                e.Graphics.DrawPath(bp, _trackingConnectPin.Path);
                e.Graphics.DrawPath(np, _trackingConnectPin.Path);

                bp.Dispose();
                np.Dispose();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            _isScrolling = true;
            _noodles.BeginUpdate();
            base.OnMouseWheel(e);
            _isScrolling = false;
            _noodles.EndUpdate();
            UpdateTrackingNoodle();

            if (ViewportChanged != null)
            {
                ViewportChanged(this, new EventArgs());
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            Size curent = ActualCanvasSize;

            if (curent.Width != ActualCanvasSize.Width || curent.Height != ActualCanvasSize.Height)
            {
                // prevent multiple updates from NodeMoved events
                BeginCanvasUpdate();

                // move focus to the _focusPuck to prevent repositioning of any nodes
                _focusPuck.Focus();

                base.OnResize(eventargs);

                // move the exported pins
                UpdateExportPins();
                UpdateImportPins();

                // resume the updates
                EndCanvasUpdate();
            }
            else
            {
                // we're not actually changing the canvas size, so prevent CreateCanvasImage from being called
                _isScrolling = true;

                // move focus to the _focusPuck to prevent repositioning of any nodes
                _focusPuck.Focus();

                base.OnResize(eventargs);

                _isScrolling = false;
            }

            // raise the ViewportChanged event
            if (ViewportChanged != null)
            {
                ViewportChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Attach a Deactivate event to the top level Form of this Container
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentChanged(EventArgs e)
        {
            StopPinConnect();
            base.OnParentChanged(e);

            Form topform = this.TopLevelControl as Form;
            if (topform != null)
            {
                topform.Deactivate += new EventHandler(topform_Deactivate);
            }
        }

        /// <summary>
        /// If the top level Form ever loses Focus, Cancel any pending Noodle dragging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void topform_Deactivate(object sender, EventArgs e)
        {
            StopPinConnect();
            Invalidate(false);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // move the _focusPuck and give it focus to prevent the canvas from jumping
            _focusPuck.Location = e.Location;
            _focusPuck.Focus();
            
            if (_canAlterState != DaggerNodeAlterState.None)
            {
                // execute the _canAlterState
                switch (_canAlterState)
                {
                    case DaggerNodeAlterState.ConnectFromOutput:
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                BeginOutputPinConnect((DaggerOutputPin)_mouseOverPin);
                                DoDragDrop(this, DragDropEffects.All);
                            }
                            else
                            {
                                // cancel pin dragging operations
                                _canAlterState = DaggerNodeAlterState.None;
                                Cursor = Cursors.Default;
                                StopPinConnect();

                                // show the pin menu
                                if ((_mouseOverPin.PinUIElements as PinUI).ContextMenuStrip != null)
                                {
                                    (_mouseOverPin.PinUIElements as PinUI).ContextMenuStrip.Show(PointToScreen(e.Location));
                                }
                            }
                        }
                        break;
                    case DaggerNodeAlterState.ConnectFromInput:
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                BeginInputPinConnect((DaggerInputPin)_mouseOverPin);
                                DoDragDrop(this, DragDropEffects.All);
                            }
                            else
                            {
                                // cancel pin dragging operations
                                _canAlterState = DaggerNodeAlterState.None;
                                Cursor = Cursors.Default;
                                StopPinConnect();

                                // show the pin menu
                                if ((_mouseOverPin.PinUIElements as PinUI).ContextMenuStrip != null)
                                {
                                    (_mouseOverPin.PinUIElements as PinUI).ContextMenuStrip.Show(PointToScreen(e.Location));
                                }
                            }
                        }
                        break;
                    case DaggerNodeAlterState.CanConnectToOutput:
                        break;
                    case DaggerNodeAlterState.CanConnectToInput:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if(e.Button == MouseButtons.Middle)
                {
                    // middle mouse button.  Begin dragging canvas and exit to allow pin
                    // connections durring Canvas Drag operations
                    _isDraggingCanvas = true;
                    _lastCanvasDragPosition = e.Location;
                    Capture = true;
                    Invalidate(false);
                    base.OnMouseDown(e);
                    return;
                }

                // cancel pin dragging operations
                StopPinConnect();

                // begin dragging a selection rectangle
                if (e.Button == MouseButtons.Left)
                {
                    _trackingSelector = new Selector(this, new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y), _noodles.Noodles, AllNodes);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // cancel a dragging selection
                    if (_trackingSelector != null && _trackingSelector.Tracking)
                    {
                        _trackingSelector = null;
                    }

                    contextMenuStrip.Show(PointToScreen(e.Location));
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // update the SelectionArea if we are tracking
            if (_trackingSelector != null && _trackingSelector.Tracking)
            {
                _trackingSelector.CurrentPoint = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
                return;
            }

            if (_isDraggingCanvas)
            {
                // we're currently dragging the canvas
                if (e.X != _lastCanvasDragPosition.X && e.Y != _lastCanvasDragPosition.Y)
                {
                    int offx = _lastCanvasDragPosition.X - e.X;
                    int offy = _lastCanvasDragPosition.Y - e.Y;
                    ScrollToPosition(new Point((AutoScrollPosition.X + offx) * -1, (AutoScrollPosition.Y + offy) * -1));
                    _lastCanvasDragPosition = e.Location;
                }
            }

            // see if we are over a pin
            _canAlterState = CanConnectToPin(e.Location);

            switch (_canAlterState)
            {
                case DaggerNodeAlterState.None:
                    Cursor = Cursors.Default;
                    break;
                case DaggerNodeAlterState.ConnectFromOutput:
                    {
                        Bitmap cursorimage = (Bitmap)CursorImages.Images[1];
                        Cursor = new Cursor(cursorimage.GetHicon());
                    }
                    break;
                case DaggerNodeAlterState.ConnectFromInput:
                    {
                        Bitmap cursorimage = (Bitmap)CursorImages.Images[0];
                        Cursor = new Cursor(cursorimage.GetHicon());
                    }
                    break;
                case DaggerNodeAlterState.CanConnectToOutput:
                    break;
                case DaggerNodeAlterState.CanConnectToInput:
                    break;
                default:
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // stop tracking the mouse if we are currently selecting an area
            if (_trackingSelector != null && _trackingSelector.Tracking)
            {
                _trackingSelector.Tracking = false;

                // refresh the Panel to show new selection
                RefreshGraph();
            }

            _canAlterState = DaggerNodeAlterState.None;
            _isDraggingCanvas = false;
            Cursor = Cursors.Default;
            Capture = false;
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            // see if it is a TypeOf DaggerNode
            if(drgevent.Data.GetDataPresent(typeof(TreeNode)))
            {
                // it's a tree node, see if the node's tag holds a daggernode type or serialized subnode
                TreeNode tn = (TreeNode)drgevent.Data.GetData(typeof(TreeNode));
                if (tn.Tag != null)
                {
                    if (tn.Tag is DaggerNodeTreeViewSubnodeItem)
                    {
                        drgevent.Effect = DragDropEffects.Move;
                    }
                }
            }

            base.OnDragEnter(drgevent);
        }

        /// <summary>
        /// If dragging a DaggerNode, see if we can bisect a noodle
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            // is Noodle-Bisecting enabled?
            if (_allowNoodleBisecting)
            {
                // see if we are over a type-compatible noodle to bisect
                DaggerNodeTreeViewSubnodeItem tn = (DaggerNodeTreeViewSubnodeItem)((TreeNode)drgevent.Data.GetData(typeof(TreeNode))).Tag;

                // is this node type even bisectable?
                if ((tn.InputpinTypes != null) && (tn.InputpinTypes.Count > 0) && (tn.OutputPinTypes.Count > 0))
                {
                    Point clientSpace = PointToClient(new Point(drgevent.X - AutoScrollPosition.X, drgevent.Y - AutoScrollPosition.Y));
                    Selector sr = new Selector(this, clientSpace, _noodles.Noodles, null);
                    sr.CurrentPoint = clientSpace;
                    sr.Tracking = false;


                    DaggerNoodle foundNoodle = null;

                    // work backwards to get the top-most noodle
                    for (int i = sr.SelectedNoodles.Count - 1; i > -1; i--)
                    {
                        DaggerNoodle noodle = sr.SelectedNoodles[i] as DaggerNoodle;
                        if(DaggerBasePin.IsCompatibleDataTypes(tn.InputpinTypes[0],noodle.OutputPin.DataType) && DaggerBasePin.IsCompatibleDataTypes(noodle.InputPin.DataType,tn.OutputPinTypes[0]))
                        {
                            //it's a type match
                            foundNoodle = noodle;
                            break;
                        }
                    }

                    // if we found one, see if it's of any use to us
                    if (foundNoodle != null)
                    {
                        if (_noodleBisector == null || _noodleBisector.Noodle != foundNoodle)
                        {
                            //create a new DaggerNoodleBisector and redraw
                            _noodleBisector = new DaggerNoodleBisector(foundNoodle, clientSpace);
                            RefreshGraph();
                        }
                    }
                    else if (_noodleBisector != null)
                    {
                        // we moved away from a bisectable noodle, so clear _noodleBisector and redraw
                        _noodleBisector = null;
                        RefreshGraph();
                    }
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            DaggerNodeTreeViewSubnodeItem tn = (DaggerNodeTreeViewSubnodeItem)((TreeNode)drgevent.Data.GetData(typeof(TreeNode))).Tag;

            DaggerNode snode = null;
            if (tn.DaggerNodeType != null)
            {
                // create a node from a type
                try
                {
                    snode = (DaggerNode)Activator.CreateInstance(tn.DaggerNodeType);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Creating Node");
                    if (_noodleBisector != null)
                    {
                        _noodleBisector = null;
                        RefreshGraph();
                        return;
                    }
                }
            }
            else
            {
                // create a node from a serialized subnode
                try
                {
                    snode = new DaggerSubNode(tn.SubnodeName, tn.DaggerNodeSerializedBuffer);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Creating Subnode");
                    if (_noodleBisector != null)
                    {
                        _noodleBisector = null;
                        RefreshGraph();
                        return;
                    }
                }
            }

            if (snode != null)
            {
                // record the node that was dropped and add it to the graph
                _droppedNode = snode;
                Graph.AddNode(snode);
                _droppedNode = null;

                DaggerUINode uinode = (DaggerUINode)Controls[Controls.Count - 1];

                if (_noodleBisector == null)
                {
                    // find that node and move it to the DragDrop coords
                    uinode.Location = PointToClient(new Point(drgevent.X, drgevent.Y));
                    uinode.Visible = true;
                }
                else
                {
                    // we bisected, so center the node on the drop position
                    uinode.Location = PointToClient(new Point(drgevent.X - (uinode.Width / 2), drgevent.Y - (uinode.Height / 2)));
                
                    // perform the bisection
                    BisectNoodle(snode, _noodleBisector.Noodle);
                    _noodleBisector = null;
                    RefreshGraph();
                }

                // mark the node visible
                uinode.Visible = true;
            }

            base.OnDragDrop(drgevent);
        }

        protected override void WndProc(ref Message m)
        {
            // prevent the ScrollBars from re-calculating the noodle paths for 
            // every uinode that is repositioned during scrolling
            if (m.Msg == 0x114 || m.Msg == 0x115)
            {                
                if ((int)m.WParam == 2 || (int)m.WParam == 3)
                {
                    // page up/down/left/right
                    _noodles.BeginUpdate();
                    _isScrolling = true;
                    base.WndProc(ref m);
                }
                else if ((int)m.WParam == 8)
                {
                    // end scroll
                    _noodles.EndUpdate();
                    _isScrolling = false;
                    base.WndProc(ref m);
                }
                else
                {
                    // dragging the scroll thumb
                    _isScrolling = true;
                    _noodles.BeginUpdate();
                    base.WndProc(ref m);
                    _noodles.EndUpdate();
                    _isScrolling = false;
                }

                if (ViewportChanged != null)
                {
                    ViewportChanged(this, new EventArgs());
                }
            }
            else if (m.Msg == 0x7d) // WM_STYLECHANGED
            {
                // Scroll bars are being shown/hidden
                BeginCanvasUpdate();
                _noodles.BeginUpdate();
                base.WndProc(ref m);
                _noodles.EndUpdate();
                EndCanvasUpdate();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion

        #region Keyboard and Mouse Hook Goodies

        // From the Article "How to set a Windows hook in Visual C# .NET"
        // http://support.microsoft.com/kb/318804

        // Fields, constants, and structures used by the keyboard and mouse hook
        private const int WH_KEYBOARD = 2;
        private const int WH_MOUSE = 7;
        private const int HC_ACTION = 0;

        // key state constants and flags
        public const int VK_CONTROL = 0x11;
        public const int VK_LWIN = 0x5B;
        public const int VK_RWIN = 0x5C;
        public const int VK_APPS = 0x5D;
        public const int VK_LSHIFT = 0xA0;
        public const int VK_RSHIFT = 0xA1;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int KF_UP = 0x8000;
        public const long KB_TRANSITION_FLAG = 0x80000000;
        public const long KB_PREVIOUS_FLAG = 0x40000000;
        public const int VK_V = 0x56;

        // Handles that will hold the hooks
        private int _mouseHookHandle = 0;
        private int _keyboardHookHandle = 0;

        // Hook delegate
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        // The callbacks
        private HookProc keyboardCB = null;
        private HookProc mouseCB = null;

        // Declare the wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        private class POINT
        {
            public int x;
            public int y;
        }

        // Declare the wrapper managed MouseHookStruct class.
        [StructLayout(LayoutKind.Sequential)]
        private class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        // This is the Import for the SetWindowsHookEx function.
        // Use this function to install a thread-specific hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
        IntPtr hInstance, int threadId);

        // This is the Import for the UnhookWindowsHookEx function.
        // Call this function to uninstall the hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // This is the Import for the CallNextHookEx function.
        // Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,
        IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetKeyState(int vKey);

        /// <summary>
        /// Set the Mouse Hook Proc
        /// </summary>
        private void _setMouseHook()
        {
            // Create an instance of HookProc.
            mouseCB = new HookProc(MouseProc);

            _mouseHookHandle = SetWindowsHookEx(WH_MOUSE,
                        mouseCB,
                        (IntPtr)0,
                        AppDomain.GetCurrentThreadId());

            //If the SetWindowsHookEx function fails.
            if (_mouseHookHandle == 0)
            {
                MessageBox.Show("SetWindowsHookEx Failed");
                return;
            }
        }

        /// <summary>
        /// Unset the Mouse Hook Proc
        /// </summary>
        private void _unsetMouseHook()
        {
            if (_mouseHookHandle != 0)
            {
                UnhookWindowsHookEx(_mouseHookHandle);
                _mouseHookHandle = 0;
            }
        }

        /// <summary>
        /// Set the Keyboard Hook Proc
        /// </summary>
        private void _setKeyboardHook()
        {
            // Create an instance of HookProc.
            keyboardCB = new HookProc(KeyboardProc);

            _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD,
                        keyboardCB,
                        (IntPtr)0,
                        AppDomain.GetCurrentThreadId());

            //If the SetWindowsHookEx function fails.
            if (_keyboardHookHandle == 0)
            {
                MessageBox.Show("SetWindowsHookEx Failed");
                return;
            }
        }

        /// <summary>
        /// Unset the Keyboard Hook Proc
        /// </summary>
        private void _unsetKeyboardHook()
        {
            if (_keyboardHookHandle != 0)
            {
                UnhookWindowsHookEx(_keyboardHookHandle);
                _keyboardHookHandle = 0;
            }
        }

        // Mouse Hook callback method
        internal int MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Marshall the data from the callback.
            MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

            if (nCode < 0)
            {
                return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
            }
            else
            {
                if (_trackingConnectPin != null)
                {

                    // get the new track point
                    _trackingConnectPin.TrackPoint = PointToClient(new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y));

                    // if the trackpoint is outside of visible horizontal scrollable area, scroll it into view
                    if (_scrollConnect)
                    {
                        _isScrolling = true;

                        int newscrollpos;
                        if (_trackingConnectPin.TrackPoint.X < 0)
                        {
                            newscrollpos = Math.Max(0, HorizontalScroll.Value + _trackingConnectPin.TrackPoint.X);
                            if (newscrollpos != HorizontalScroll.Value)
                            {
                                HorizontalScroll.Value = newscrollpos;
                                UpdateTrackingNoodle();
                            }
                        }
                        else if (_trackingConnectPin.TrackPoint.X > Width)
                        {
                            newscrollpos = Math.Min(HorizontalScroll.Maximum, HorizontalScroll.Value + (_trackingConnectPin.TrackPoint.X - Width));
                            if (newscrollpos != HorizontalScroll.Value)
                            {
                                HorizontalScroll.Value = newscrollpos;
                                UpdateTrackingNoodle();
                            }
                        }

                        // if the trackpoint is outside of visible vertical scrollable area, scroll it into view
                        if (_trackingConnectPin.TrackPoint.Y < 0)
                        {
                            newscrollpos = Math.Max(0, VerticalScroll.Value + _trackingConnectPin.TrackPoint.Y);
                            if (newscrollpos != VerticalScroll.Value)
                            {
                                VerticalScroll.Value = newscrollpos;
                                UpdateTrackingNoodle();
                            }
                        }
                        else if (_trackingConnectPin.TrackPoint.Y > Height)
                        {
                            newscrollpos = Math.Min(VerticalScroll.Maximum, VerticalScroll.Value + (_trackingConnectPin.TrackPoint.Y - Height));
                            if (newscrollpos != VerticalScroll.Value)
                            {
                                VerticalScroll.Value = newscrollpos;
                                UpdateTrackingNoodle();
                            }
                        }

                        _isScrolling = false;                        
                    }

                    // redraw canvas to show tracking connection
                    Invalidate(false);
                }

                //call the next hook in the mouse hook chain
                return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
            }
        }

        // Keyboard Hook callback method
        internal int KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            UInt32 _wParam =(UInt32) wParam.ToInt32();
            UInt32 _lParam = (UInt32)wParam.ToInt32();

            // get the key up state
            bool keyUp = ((_lParam & 0x80000000) != 0);

            // get the state of the control key.
            _ctrlKey = (((GetKeyState(VK_CONTROL) & 0x8000) != 0));

            if (keyUp)
            {
                if (_wParam == (int)Keys.Delete)
                {
                    // We only want to respond to delete, copy, paste, etc. if the _focusPuck has focus.
                    // This helps to differentiate between the context of graph editing
                    // and editing internal controls of a node
                    if (_focusPuck.Focused)
                    {
                        DeleteSelected();
                    }
                }
                else if (_wParam == (int)Keys.Escape)
                {
                    if (_trackingConnectPin != null)
                    {
                        StopPinConnect();
                        Invalidate(false);
                    }
                }
            }
            else
            {
                // handle control key functions
                if (_ctrlKey)
                {
                    if (_focusPuck.Focused)
                    {
                        if (_wParam == (int)Keys.X)
                        {
                            Cut();
                            // consume the key press
                            return -1;
                        }
                        if (_wParam == (int)Keys.C)
                        {
                            Copy();
                            return -1;
                        }
                        if (_wParam == (int)Keys.V)
                        {
                            Paste();
                            return -1;
                        }
                        if (_wParam == (int)Keys.A)
                        {
                            SelectAll();
                            return -1;
                        }
                    }
                }
            }

            //call the next hook in the keyboard hook chain
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        #endregion

        #region UINodePositionComparer

        /// <summary>
        /// Comparer class to sort DaggerUINodes by thier Vertical Position in a UIGraph
        /// </summary>
        internal class UINodePositionComparer : IComparer<IDaggerUINode>
        {
            public int Compare(IDaggerUINode x, IDaggerUINode y)
            {
                int result = 0;

                if (x == null && y == null)
                {
                    result = 0;
                }
                else if (x == null)
                {
                    result = -1;
                }
                else if (y == null)
                {
                    result = 1;
                }
                else if (x == null && y == null)
                {
                    result = 0;
                }
                else if (x == null)
                {
                    result = -1;
                }
                else if (y == null)
                {
                    result = 1;
                }
                else
                {
                    if (x.Top < y.Top)
                    {
                        result = -1;
                    }
                    else if (x.Top == y.Top)
                    {
                        result = 0;
                    }
                    else result = 1;
                }

                return result;
            }
        }

        /// <summary>
        /// Comparer class to sort DaggerNodes by thier Vertical Position in a UIGraph
        /// </summary>
        internal class NodePositionComparer : IComparer<DaggerNode>
        {
            public int Compare(DaggerNode x, DaggerNode y)
            {
                int result = 0;

                if (x == null && y == null) 
                {
                    result = 0;
                }
                else if (x == null)
                {
                    result = -1;
                }
                else if (y == null)
                {
                    result = 1;
                }
                else if (x.UINode == null && y.UINode == null)
                {
                    result = 0;
                }
                else if (x.UINode == null)
                {
                    result = -1;
                }
                else if (y.UINode == null)
                {
                    result = 1;
                }
                else
                {
                    if (x.UINode.Top < y.UINode.Top)
                    {
                        result = -1;
                    }
                    else if (x.UINode.Top == y.UINode.Top)
                    {
                        result = 0;
                    }
                    else result = 1;
                }

                return result;
            }
        }
        #endregion
    }

    #region DraggingNoodle Class

    /// <summary>
    /// Represents a noodle currently being dragged by the user or browsed via BasePinContextMenuStrip
    /// </summary>
    internal class DraggingNoodle
    {
        public Point StartPoint;
        public Point _trackPoint;
        public DaggerOutputPin OutputPin;
        public DaggerInputPin InputPin;
        public GraphicsPath Path = new GraphicsPath();
        public int MinimumPrecedence = -1;

        public DraggingNoodle(DaggerInputPin InputPin, DaggerOutputPin OutputPin)
        {
            DaggerUIGraph uigraph = InputPin.ParentNode.ParentGraph.ParentUIGraph as DaggerUIGraph;

            this.OutputPin = OutputPin;
            this.InputPin = InputPin;

            //translate the pin locations to the uigraph client coords
            StartPoint = (InputPin.ParentNode.UINode as DaggerUINode).PointToScreen((InputPin.PinUIElements as PinUI).PinLocation);
            StartPoint.X += InputPin.ParentNode.UINode.PinSize / 2;
            StartPoint.Y += InputPin.ParentNode.UINode.PinSize / 2;
            StartPoint = uigraph.PointToClient(new Point(StartPoint.X + uigraph.AutoScrollOffset.X, StartPoint.Y + uigraph.AutoScrollOffset.Y));

            Point _trackPoint = (OutputPin.ParentNode.UINode as DaggerUINode).PointToScreen((OutputPin.PinUIElements as PinUI).PinLocation);
            _trackPoint.X += OutputPin.ParentNode.UINode.PinSize / 2;
            _trackPoint.Y += OutputPin.ParentNode.UINode.PinSize / 2;
            TrackPoint = uigraph.PointToClient(new Point(_trackPoint.X + uigraph.AutoScrollOffset.X, _trackPoint.Y + uigraph.AutoScrollOffset.Y));
        }

        public DraggingNoodle(Point StartPoint, DaggerOutputPin OutputPin)
        {
            this.StartPoint = StartPoint;
            this.OutputPin = OutputPin;

            if (OutputPin.ParentNode == null)
            {
                //exported pins have a minimum precedence of 0
                MinimumPrecedence = 0;
            }
            else
            {
                MinimumPrecedence = OutputPin.ParentNode.Ordinal;
            }
        }

        public DraggingNoodle(Point StartPoint, DaggerInputPin InputPin)
        {
            this.StartPoint = StartPoint;
            this.InputPin = InputPin;

            if (InputPin.ParentNode == null)
            {
                //exported pins have a minimum precedence of 0
                MinimumPrecedence = 0;
            }
            else
            {
                this.MinimumPrecedence = InputPin.ParentNode.Ordinal;
            }
        }

        public Point TrackPoint
        {
            get
            {
                return _trackPoint;
            }
            set
            {
                _trackPoint = value;

                //recreate the graphics path
                Path.Dispose();
                Path = new GraphicsPath();
                int bezcontroloffset = (_trackPoint.X - StartPoint.X) / 2;

                Path.AddBezier(StartPoint,
                   new Point(StartPoint.X + bezcontroloffset, StartPoint.Y),
                   new Point(_trackPoint.X - bezcontroloffset, _trackPoint.Y),
                   _trackPoint);
            }
        }
    }
    #endregion
}
