using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;
using DaggerLib.Core;
using DaggerLib.Interfaces;
using DaggerLib.UI.Windows;
using DaggerLib.DSGraphEdit.PinTypes;
using DirectShowLib;
using DirectShowLib.Dvd;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public class DSDaggerUIGraph : DaggerUIGraph
    {
        #region Fields

        private System.ComponentModel.IContainer components;

        /// <summary>
        /// the filterGraph this DaggerGraph represents
        /// </summary>
        public IFilterGraph _Graph;

        /// <summary>
        /// Flag indicating the IFilterGraph was created internally
        /// </summary>
        internal bool _filterGraphCreated;

        /// <summary>
        /// The images for the input and output pins
        /// </summary>
        private ImageList _pinImages;

        /// <summary>
        /// The last location on the canvas a node was added
        /// </summary>
        internal Point _dropLocation;

        /// <summary>
        /// Flag to indicate if the pin names are drawn on the canvas
        /// </summary>
        private bool _showPinNames;

        /// <summary>
        /// Flag to indicate if property pages are to be shown in modal dialogs
        /// </summary>
        private bool _modalProperties;

        #endregion

        #region ctor

        public DSDaggerUIGraph()
            : base()
        {
            InitializeComponent();

            // we dont want the user to have access to these UI pin menu properties
            AllowAttachConstantEditor = false;
            AllowNoodleBisecting = false;
            AllowPinExport = false;
            AllowPinSetValue = false;
            AllowNodeProcessing = false;
            AllowSubNodes = false;

            // create the pin legend and add the default pin images
            PinLegend = new DaggerLib.UI.Windows.DaggerPinLegend(13);
            PinLegend.AddPinType(typeof(object),
                (Bitmap)_pinImages.Images[0],
                (Bitmap)_pinImages.Images[1],
                (Bitmap)_pinImages.Images[2],
                (Bitmap)_pinImages.Images[3], Color.White, Color.Red);

            // create and add pintypes for the Major Media types to the pin legend
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Null), Color.LightGray);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Unknown), Color.Gray);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Video), Color.Yellow);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Audio), Color.Blue);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Interleaved), Color.LightGreen);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Text), Color.LightPink);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Stream), Color.LightSeaGreen);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.VBI), Color.MediumAquamarine);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Midi), Color.MediumBlue);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.File), Color.MediumPurple);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.ScriptCommand), Color.MidnightBlue);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.AuxLine21Data), Color.Pink);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.LMRT), Color.PeachPuff);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.URLStream), Color.Purple);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.AnalogVideo), Color.RoyalBlue);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.AnalogAudio), Color.Sienna);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.Mpeg2Sections), Color.SkyBlue);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.DTVCCData), Color.Teal);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.MSTVCaption), Color.Gold);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.DVDEncryptedPack), Color.IndianRed);
            PinLegend.AddPinType(typeof(object), typeof(PinDataTypes.MPEG1SystemStream), Color.Bisque);

            this.Disposed += new EventHandler(DSDaggerUIGraph_Disposed);
            this.NoodleAdded += new PinAfterConnectedHandler(DSDaggerUIGraph_NoodleAddedRemoved);
            this.NoodleRemoved += new PinAfterDisconnectedHandler(DSDaggerUIGraph_NoodleAddedRemoved);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DSDaggerUIGraph));
            this._pinImages = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // _pinImages
            // 
            this._pinImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_pinImages.ImageStream")));
            this._pinImages.TransparentColor = System.Drawing.Color.Transparent;
            this._pinImages.Images.SetKeyName(0, "pinc.png");
            this._pinImages.Images.SetKeyName(1, "pic.png");
            this._pinImages.Images.SetKeyName(2, "ponc.png");
            this._pinImages.Images.SetKeyName(3, "poc.png");
            this.ResumeLayout(false);
        }

        #endregion

        #region Events and Handlers

        /// <summary>
        /// Disposed event handler to release interfaces AFTER the UIGraph is disposed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DSDaggerUIGraph_Disposed(object sender, EventArgs e)
        {
            // clean up all interfaces and references
            _Graph = null;

            // release all the com interfaces in the Nodes
            foreach (DaggerUINode uinode in AllNodes)
            {
                (uinode.Node as DSFilterNode).CloseInterfaces();
            }
        }

        void DSDaggerUIGraph_NoodleAddedRemoved(DaggerOutputPin output, DaggerInputPin input)
        {
            RouteDVDControl();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if pin names are drawn on the graph
        /// </summary>
        public bool ShowPinNames
        {
            get
            {
                return _showPinNames;
            }
            set
            {
                _showPinNames = value;
                foreach (DaggerUINode node in AllNodes)
                {
                    (node as DaggerUINode).Invalidate(false);
                }
                Invalidate(false);
            }
        }

        /// <summary>
        /// Gets or sets if Property pages are shown as modal dialogs
        /// </summary>
        public bool ModalProperties
        {
            get
            {
                return _modalProperties;
            }
            set
            {
                if (value != _modalProperties)
                {
                    // create or release existing non-modal property pages
                    _modalProperties = value;

                    BeginCanvasUpdate();

                    // set the modal porperties state of all existing nodes
                    foreach (DSFilterNodeUI uinode in AllNodes)
                    {
                        uinode.SetModalProperties();
                    }

                    EndCanvasUpdate();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create and add a filter from a DSFilterTreeViewNode
        /// </summary>
        /// <param name="tn"></param>
        public IBaseFilter AddFilter(DSFilterTreeViewNode tn)
        {
            int hr = 0;
            IBaseFilter filter = null;

            if (tn.DSFilterType == FilterType.DMO)
            {
                // create a DMO wrapper and init it with the ClassGuid
                filter = (IBaseFilter)new DMOWrapperFilter();
                IDMOWrapperFilter wrapper = (IDMOWrapperFilter)filter;
                hr = wrapper.Init(tn.ClassGuid, tn.DMOCategory);
                if (hr != 0)
                {
                    MessageBox.Show(DsError.GetErrorText(hr), "Error wrapping DMO");
                    Marshal.ReleaseComObject(filter);
                    return null;
                }
                hr = _Graph.AddFilter(filter, tn.Text);
                if (hr != 0)
                {
                    MessageBox.Show(DsError.GetErrorText(hr), "Error Adding DMO");
                    Marshal.ReleaseComObject(filter);
                    return null;
                }
                SyncGraphs(filter);
                return filter;
            }
            else
            {
                // try adding it as a source filter first (this usually works for non-source filters anyway)
                hr = (_Graph as IFilterGraph2).AddSourceFilterForMoniker(tn.Moniker, null, tn.Text, out filter);

                // that didn't work.  Try AddFilterByDevicePath
                if (filter == null)
                {
                    DirectShowLib.Utils.FilterGraphTools.AddFilterByDevicePath(_Graph as IGraphBuilder, tn.DevicePath, tn.Name);
                }

                // force the DaggerGraph to reflect changes in the DS graph
                List<DSFilterNode> addedNodes = SyncGraphs(filter);

                // get the DSFilterNode that was added and give it the information
                if (addedNodes.Count == 1)
                {
                    addedNodes[0]._devicePath = tn.DevicePath;
                    addedNodes[0]._moniker = tn.Moniker;
                }

                return filter;
            }
        }

        /// <summary>
        /// Create or remove DSFilterNodes and Noodles based on state of FilterGraph
        /// </summary>
        public List<DSFilterNode> SyncGraphs(IBaseFilter manuallyAddedFilter)
        {
            List<DSFilterNode> nodesAdded = new List<DSFilterNode>();

            this.SuspendLayout();
            this.BeginNoodleUpdate();
            this.BeginCanvasUpdate();

            List<IBaseFilter> filters = GetFilters(_Graph);
            List<IDaggerUINode> nodes = AllNodes;

            // remove any nodes that no longer have corresponding IBaseFilters in the filtergraph
            foreach (DSFilterNodeUI node in nodes)
            {
                if(!filters.Contains((node.Node as DSFilterNode)._filter))
                {
                    this.Graph.DeleteNode(node.Node);
                }
            }

            // sync the pins in the nodes of the graph
            foreach (DaggerUINode node in AllNodes)
            {
                (node.Node as DSFilterNode).SyncPins();
            }

            // add nodes for any filters that aren't already in the graph
            for(int i = filters.Count - 1; i > -1; i--)
            {
                IBaseFilter filter = filters[i];

                // see if we haven't made a DaggerNode for this filter yet
                if (FindFilter(filter) == null)
                {
                    DSFilterNode node = new DSFilterNode(filter, manuallyAddedFilter == filter);
                    nodesAdded.Add(node);

                    // Setting _droppedNode to a value will make the node NOT visible when it's created.
                    // This way, we can reposition and resize the UINode before we display it.
                    _droppedNode = node;
                    Graph.AddNode(node);
                    _droppedNode = null;

                    // set the node's position and increment the drop location to the left
                    (node.UINode as DaggerUINode).Location = _dropLocation;
                    _dropLocation = new Point((node.UINode as DaggerUINode).Right + 24, _dropLocation.Y);
                    if (_dropLocation.X >= ActualCanvasSize.Width)
                    {
                        _dropLocation.X = 0;
                        _dropLocation.Y = _dropLocation.Y + (node.UINode as DaggerUINode).Height;
                    }

                    // mark the node visible
                    (node.UINode as DaggerUINode).Visible = true;
                }
                else
                {
                    // we already have a node for this one, release the ref enum.next added
                    if (filter.GetType().IsCOMObject)
                    {
                        int refc = Marshal.ReleaseComObject(filter);
                    }
                }
            }

            // purge disconnected noodles
            PurgeNoodles();

            // create new noodles
            BuildNoodles();

            // redraw the Canvas Image
            ResumeLayout();
            EndNoodleUpdate();
            EndCanvasUpdate();

            // route any dvd controls to thier video windows
            RouteDVDControl();

            // return the list of DSFilterNode that were added to the graph
            return nodesAdded;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Associate an IDvdControl2 with it's attached Video Windows
        /// </summary>
        private void RouteDVDControl()
        {
            // reset the IDvdControl2 of VideoInternalWindows
            foreach (DaggerNode node in Graph.BottomLevelNodes)
            {
                if ((node.UINode as DSFilterNodeUI)._videoWindow != null)
                {
                    (node.UINode as DSFilterNodeUI)._videoWindow.DVDControl = null;
                }
            }

            // pass IDvdControl2 to attached VideoInternalWindows
            foreach (DaggerNode node in Graph.TopLevelNodes)
            {
                IDvdControl2 dvd = (node as DSFilterNode)._filter as IDvdControl2;
                if (dvd != null)
                {
                    foreach (DaggerNode decnode in node._descendents)
                    {
                        if ((decnode.UINode as DSFilterNodeUI)._videoWindow != null)
                        {
                            (decnode.UINode as DSFilterNodeUI)._videoWindow.DVDControl = dvd;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get a List of all DirectShow IBasefilters in the FilterGraph
        /// </summary>
        /// <returns>List of filters</returns>
        internal List<IBaseFilter> GetFilters(IFilterGraph graph)
        {
            if (graph == null) return null;

            int hr = 0;
            IEnumFilters enumFilters;
            List<IBaseFilter> filtersArray = new List<IBaseFilter>();

            hr = graph.EnumFilters(out enumFilters);
            DsError.ThrowExceptionForHR(hr);

            IntPtr fetched = Marshal.AllocCoTaskMem(4);
            try
            {
                IBaseFilter[] filters = new IBaseFilter[1];                
                while (enumFilters.Next(filters.Length, filters, fetched) == 0)
                {
                    filtersArray.Add(filters[0]);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(enumFilters);
                Marshal.FreeCoTaskMem(fetched);
            }
            return filtersArray;
        }

        private void PurgeNoodles()
        {
            foreach (IDaggerNoodle noodle in AllNoodles)
            {
                IPin conto = null;
                ((noodle as DaggerNoodle).InputPin as DSInputPin)._pin.ConnectedTo(out conto);
                if(conto != ((noodle as DaggerNoodle).OutputPin as DSOutputPin)._pin)
                {
                    (noodle as DaggerNoodle).InputPin.Disconnect(true);
                }

                if (conto != null)
                {
                    Marshal.ReleaseComObject(conto);
                }
            }
        }

        /// <summary>
        /// Create noodle connections for the filter's pin connections
        /// </summary>
        private void BuildNoodles()
        {
            foreach (DSFilterNode node in Graph.AllNodes)
            {
                foreach (DSOutputPin outpin in node.OutputPins)
                {
                    if (!outpin.IsConnected)
                    {
                        IPin connectedTo;
                        outpin._pin.ConnectedTo(out connectedTo);

                        // get the DSInputPin this pin is connected to (if any)
                        DSInputPin foundPin = FindInputPin(connectedTo);
                        if (connectedTo != null)
                        {
                            Marshal.ReleaseComObject(connectedTo);
                        }

                        if (foundPin != null)
                        {
                            try
                            {
                                outpin.ConnectToInput(foundPin);
                            }
                            catch (InvalidOperationException ex)
                            {
#if DEBUG
                                MessageBox.Show("Pins already connected");
#endif
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find the DSInputPin in the Graph that holds the given DirectShow IPin
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        private DSInputPin FindInputPin(IPin pin)
        {
            if (pin == null) return null;

            foreach (DSFilterNode node in Graph.AllNodes)
            {
                foreach (DSInputPin inpin in node.InputPins)
                {
                    if (inpin._pin == pin)
                    {
                        return inpin;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the DSFilterNode that holds the given DirectShow IFilter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private DSFilterNode FindFilter(IBaseFilter filter)
        {
            foreach (DSFilterNode node in Graph.AllNodes)
            {
                if (node._filter == filter)
                {
                    return node;
                }
            }
            return null;
        }

        #endregion

        #region Overrides

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            //see if it is a TypeOf DSFilterTreeViewNode
            if (drgevent.Data.GetDataPresent(typeof(DSFilterTreeViewNode)))
            {
                drgevent.Effect = DragDropEffects.Move;
            }
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
           // bypass the default OnDragOver of DaggerUIGraph
        }

        /// <summary>
        /// Overide the default DaggerUIGraph OnDragDrop to handle TreeNodes from a DSFilterTreeView
        /// instead of TreeNodes from DaggerNodeTreeView
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragDrop(System.Windows.Forms.DragEventArgs drgevent)
        {
            DSFilterTreeViewNode tn = (DSFilterTreeViewNode)drgevent.Data.GetData(typeof(DSFilterTreeViewNode));
            if (tn != null)
            {
                // store the drop location so the node can be repositioned in SyncGraph
                _dropLocation = PointToClient(new Point(drgevent.X, drgevent.Y));
                AddFilter(tn);
            }
        }

        /// <summary>
        /// Overlay the pin names for the node that has focus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // let DaggerLib draw it's graph
            base.OnPaint(e);

            if (_showPinNames)
            {
                foreach (DSFilterNodeUI node in AllNodes)
                {
                    // see if we need to draw the pin names outside the node
                    if (node._expandPropertiesButton.State || node._videoWindow != null)
                    {
                        // measure and draw the names of it's pins
                        foreach (DSInputPin pin in node.Node.InputPins)
                        {
                            SizeF fs = e.Graphics.MeasureString(pin.Name, Font);
                            Point actualLocation = this.PointToClient(node.PointToScreen((pin.PinUIElements as PinUI).PinLocation));
                            RectangleF rect = new RectangleF(new PointF(actualLocation.X - fs.Width - 2, actualLocation.Y), fs);
                            DrawPinName(Font, e.Graphics, (pin.PinUIElements as PinUI).NoodleColor, pin.Name, rect);
                        }

                        foreach (DSOutputPin pin in node.Node.OutputPins)
                        {
                            SizeF fs = e.Graphics.MeasureString(pin.Name, Font);
                            Point actualLocation = this.PointToClient(node.PointToScreen((pin.PinUIElements as PinUI).PinLocation));
                            RectangleF rect = new RectangleF(new PointF(actualLocation.X + this.PinSize + 2, actualLocation.Y), fs);
                            DrawPinName(Font, e.Graphics, (pin.PinUIElements as PinUI).NoodleColor, pin.Name, rect);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw the name of the pin onto the canvas bound in a color-coded rectangle
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        /// <param name="name"></param>
        /// <param name="rect"></param>
        internal static void DrawPinName(Font f, Graphics g, Color c, string name, RectangleF rect)
        {
            Rectangle rr = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            using (Brush b = new SolidBrush(Color.FromArgb(150, ControlPaint.Dark(c))))
            {
                g.FillRectangle(b, rr);
            }
            using (Pen p = new Pen(ControlPaint.LightLight(c)))
            {
                g.DrawRectangle(p, rr);
            }
            g.DrawString(name, f, Brushes.White, rect.Location);
        }
        #endregion
    }
}
