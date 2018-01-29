using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;
using DirectShowLib.Utils;
using DirectShowLib.Dvd;
using DaggerLib.Core;
using DaggerLib.UI.Windows;

namespace DaggerLib.DSGraphEdit
{
    public partial class DSGraphEditPanel : UserControl
    {
        #region Fields

        // style of pin placement for the nodes
        private DaggerNodePinPlacement _pinPlacement = DaggerNodePinPlacement.Outset;

        private const int WMGraphNotify = 0x0400 + 13;
        
        // FilterGraph for DirectShow Source
        private IFilterGraph _graph;

        // flag to indicate if filter graph was created here
        private bool _filterGraphCreated;

        // flag to indicate this graph is connected to an IFilterGraph on the ROT
        private bool _isRemoteGraph = false;

        // the string representing the ROT connected IFilterGraph
        private string _rotEntryString = string.Empty;

        // flag to indicate if we use Intelligent Connection
        private bool _connectIntelligent = true;

        // flag to indicate if the graph uses a reference clock if available
        private bool _useReferenceClock = true;

        //interfaces for controling DirectShow Sources
        private IGraphBuilder _graphBuilder;
        private IMediaControl _mediaControl;
        private IMediaSeeking _mediaSeeking;
        private IBasicAudio _basicAudio;
        private IBasicVideo2 _basicVideo;
        private IVideoWindow _videoWindow;
        private IMediaEventEx _mediaEventEx;
        private IVideoFrameStep _frameStep;

        //ROT to allow attaching in GraphEdit
        private DsROTEntry rot;

        private FilterState _mediaState = FilterState.Stopped;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DSGraphEditPanel()
        {
            InitializeComponent();
            int hr = 0;

            // create filter graph
            _graph = (IFilterGraph)new FilterGraph();

            _filterGraphCreated = true;

            // give the filter graph to the DaggerUIGraph
            dsDaggerUIGraph1._Graph = _graph;

            // mark it as having been created internally
            dsDaggerUIGraph1._filterGraphCreated = true;

#if DEBUG
            rot = new DsROTEntry(_graph);
#endif

            // Initialize items common to all constructors
            Init();

            // get the state
            _mediaControl.GetState(100, out _mediaState);

            // Have the graph signal event via window callbacks for performance
            _mediaEventEx = _graph as IMediaEventEx;
            if (_mediaEventEx != null)
            {
                hr = _mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        /// <summary>
        /// Connect to existing filtergraph
        /// </summary>
        public DSGraphEditPanel(IFilterGraph filterGraph)
        {
            InitializeComponent();

            // create filter graph
            _graph = filterGraph;

            // give the filter graph to the DaggerUIGraph
            dsDaggerUIGraph1._Graph = _graph;

            // Initialize items common to all constructors
            Init();

            // get the state
            _mediaControl.GetState(100, out _mediaState);

            // match our state to the existing state
            switch (_mediaState)
            {
                case FilterState.Paused:
                    Pause();
                    break;
                case FilterState.Running:
                    Play();
                    break;
                case FilterState.Stopped:
                    Stop();
                    break;
                default:
                    break;
            }

            dsDaggerUIGraph1.SyncGraphs(null);
            dsDaggerUIGraph1.ArrangeNodes(AutoArrangeStyle.All);

            // see if this graph has a reference clock
            IReferenceClock rc = null;
            (filterGraph as IMediaFilter).GetSyncSource(out rc);
            if (rc == null)
            {
                _useReferenceClock = false;
            }
        }

        /// <summary>
        /// Constructor from a Graph file
        /// </summary>
        /// <param name="graphFileName">Path of Graph File to load</param>
        public DSGraphEditPanel(string graphFileName)
        {
            InitializeComponent();
            int hr = 0;

            // create filter graph
            _graph = (IFilterGraph)new FilterGraph();
            _filterGraphCreated = true;

            // give the filter graph to the DaggerUIGraph
            dsDaggerUIGraph1._Graph = _graph;

            // mark it as having been created internally
            dsDaggerUIGraph1._filterGraphCreated = true;

#if DEBUG
            rot = new DsROTEntry(_graph);
#endif            
            // Initialize items common to all constructors
            Init();

            // try to load the graph from IStorage
            try
            {
                hr = FilterGraphTools.LoadGraphFile(_graphBuilder, graphFileName);
                Marshal.ThrowExceptionForHR(hr);
            }
            catch (Exception ex)
            {
                // release the filtergraph and rethrow the exception
                Marshal.ReleaseComObject(_graph);
                throw ex;
            }

            // get the state
            _mediaControl.GetState(100, out _mediaState);

            // Have the graph signal event via window callbacks for performance
            _mediaEventEx = (IMediaEventEx)_graph;
            hr = _mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);
            DsError.ThrowExceptionForHR(hr);

            // sync the loaded graph and arrange the nodes
            dsDaggerUIGraph1.SyncGraphs(null);
            dsDaggerUIGraph1.ArrangeNodes(AutoArrangeStyle.All);
        }

        // From Play-Wnd-2005 sample in DirectShowLib
        //
        // Some video renderers support stepping media frame by frame with the
        // IVideoFrameStep interface.  See the interface documentation for more
        // details on frame stepping.
        //
        private bool GetFrameStepInterface()
        {
            int hr = 0;

            IVideoFrameStep frameStepTest = null;

            // Get the frame step interface, if supported
            frameStepTest = (IVideoFrameStep)this._graph;

            // Check if this decoder can step
            hr = frameStepTest.CanStep(0, null);
            if (hr == 0)
            {
                _frameStep = frameStepTest;
                return true;
            }
            else
            {
                _frameStep = null;
                return false;
            }
        }

        /// <summary>
        /// Static method to connect to a IFilterGraph on the ROT and create a DSGraphEditPanel for it
        /// </summary>
        /// <returns></returns>
        public static DSGraphEditPanel ConnectToRemoteGraph()
        {
            DSGraphEditPanel ret = null;
            ROTEntriesDialog rd = new ROTEntriesDialog();
            if (rd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Connect to the FilterGraph selected in ROTEntriesDialog and create
                    // a new GraphEditPanel for it
                    IFilterGraph fg = rd.FilterGraph;
                    ret = new DSGraphEditPanel(fg);
                    ret._isRemoteGraph = true;
                    ret._rotEntryString = rd.SelectedROTEntry;
                }
                finally
                {
                    // always dispose of ROTEntriesDialog when you're done with it
                    rd.Dispose();
                    rd = null;
                }
            }
            return ret;
        }

        /// <summary>
        /// Initialize items common to all constructors
        /// </summary>
        private void Init()
        {
            // get interfaces
            _mediaControl = _graph as IMediaControl;
            _basicAudio = _graph as IBasicAudio;
            _basicVideo = _graph as IBasicVideo2;
            _graphBuilder = _graph as IGraphBuilder;            

            // hook the DaggerGraph events
            dsDaggerUIGraph1.Graph.BeforeNodeRemoved += new BeforeNodeRemoveHandler(Graph_BeforeNodeRemoved);
            dsDaggerUIGraph1.Graph.BeforePinsConnected += new PinBeforeConnectedHandler(Graph_BeforePinsConnected);
            dsDaggerUIGraph1.Graph.BeforePinsDisconnected += new PinBeforeDisconnectedHandler(Graph_BeforePinsDisconnected);
            dsDaggerUIGraph1.BeforeSelectionDeleted += new BeforeDeleteSelected(dsDaggerUIGraph1_BeforeSelectionDeleted);
            dsDaggerUIGraph1.Graph.OnTopologyChanged +=new EventHandler(Graph_OnTopologyChanged);
            this.Disposed += new EventHandler(DSGraphEditPanel_Disposed);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get's or sets the display style of filter connections
        /// </summary>
        [Category("GraphStyles")]
        [Description("Get's or sets the display style of filter connections")]
        public NoodleStyle NoodleStyle
        {
            get
            {
                return dsDaggerUIGraph1.NoodleStyle;
            }
            set
            {
                dsDaggerUIGraph1.NoodleStyle = value;
            }
        }

        /// <summary>
        /// Get's or sets if the pin names are drawn onto the canvas
        /// </summary>
        [Category("GraphStyles")]
        [Description("Get's or sets if the pin names are drawn onto the canvas")]
        public bool ShowPinNames
        {
            get
            {
                return this.dsDaggerUIGraph1.ShowPinNames;
            }
            set
            {
                this.dsDaggerUIGraph1.ShowPinNames = value;
            }
        }

        /// <summary>
        /// Get's or sets the pin placement in the nodes
        /// </summary>
        [Category("GraphStyles")]
        [Description("Gets or sets the visual style of the filters in the graph")]
        public DaggerNodePinPlacement PinPlacement
        {
            get
            {
                return _pinPlacement;
            }
            set
            {
                _pinPlacement = value;
                dsDaggerUIGraph1.BeginCanvasUpdate();
                foreach (DaggerUINode uinode in dsDaggerUIGraph1.AllNodes)
                {
                    if (uinode.PinPlacement != value)
                    {
                        uinode.PinPlacement = value;
                    }
                }
                dsDaggerUIGraph1.EndCanvasUpdate();
            }
        }

        /// <summary>
        /// Sets visual properties of the DSGraphEditPanel
        /// </summary>
        [Browsable(false)]
        public DSGraphEditPanelProperties DSGraphEditPanelProperties
        {
            set
            {
                int r = dsDaggerUIGraph1.BeginCanvasUpdate();
                DropShadow = value.DropShadowVisible;
                PinPlacement = value.PinPlacement;
                ModalProperties = value.ModalProperties;
                BackColor = value.CanvasBackColor;
                ShowTimeSlider = value.ShowTimeSlider;
                ShowPinNames = value.ShowPinNames;
                NoodleStyle = value.NoodleStyle;
                r = dsDaggerUIGraph1.EndCanvasUpdate();
            }
        }

        /// <summary>
        /// Gets or sets if FilterGraph uses a reference clock if available
        /// </summary>
        [Browsable(false)]
        public bool UseReferenceClock
        {
            get
            {
                return _useReferenceClock;
            }
            set
            {
                int hr = 0;
                if (value != _useReferenceClock)
                {
                    // make sure the graph is stopped
                    Stop();

                    _useReferenceClock = value;
                    DSFilterNodeUI refnode = GetReferenceClock();
                    try
                    {
                        if (value)
                        {
                            if (refnode == null)
                            {
                                dsDaggerUIGraph1._Graph.SetDefaultSyncSource();
                            }
                            else
                            {
                                (dsDaggerUIGraph1._Graph as IMediaFilter).SetSyncSource(refnode._referenceClock);
                            }
                        }
                        else
                        {
                            hr = (dsDaggerUIGraph1._Graph as IMediaFilter).SetSyncSource(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error setting reference clock");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or Sets if Intelligent Connect is used in IPin connections
        /// </summary>
        [Category("GraphStyles")]
        [Description("Gets or sets if pin connecting uses Intelligent Connect")]
        public bool ConnectIntelligent
        {
            get
            {
                return _connectIntelligent;
            }
            set
            {
                _connectIntelligent = value;
            }
        }

        /// <summary>
        /// Gets or sets if the drop shadow is visible
        /// </summary>
        [Description("Gets or sets if the graph shows a dropshadow")]
        [Category("GraphStyles")]
        public bool DropShadow
        {
            get
            {
                return dsDaggerUIGraph1.DropShadowVisible;
            }
            set
            {
                dsDaggerUIGraph1.DropShadowVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets if Property pages are shown as modal dialogs
        /// </summary>
        [Description("Gets or sets if Property pages are shown as modal dialogs")]
        [Category("GraphStyles")]
        public bool ModalProperties
        {
            get
            {
                return dsDaggerUIGraph1.ModalProperties;
            }
            set
            {
                dsDaggerUIGraph1.ModalProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the back color of the Canvas
        /// </summary>
        [Description("Gets or sets the backcolor of the graph canvas")]
        [Category("GraphStyles")]
        public override Color BackColor
        {
            get
            {
                return dsDaggerUIGraph1.BackColor;
            }
            set
            {
                dsDaggerUIGraph1.BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets if the Time Slider control is visible
        /// </summary>
        [Description("Gets or sets if the Time Slider control is visible")]
        [Category("GraphStyles")]
        public bool ShowTimeSlider
        {
            get
            {
                return _timeSliderControl.Visible;
            }
            set
            {
                _timeSliderControl.Visible = value;
            }
        }

        /// <summary>
        /// Returns true if this is connected to a remote IFilterGraph on the ROT
        /// </summary>
        [Browsable(false)]
        public bool IsRemoteGraph
        {
            get
            {
                return _isRemoteGraph;
            }
        }

        /// <summary>
        /// Returns a string representing the remote IFilterGraph
        /// </summary>
        [Browsable(false)]
        public string ROTEntryString
        {
            get
            {
                return _rotEntryString;
            }
        }

        /// <summary>
        /// Returns the internal UI Graph
        /// </summary>
        [Browsable(false)]
        public DSDaggerUIGraph DSDaggerUIGraph
        {
            get
            {
                return dsDaggerUIGraph1;
            }
        }

        /// <summary>
        /// Gets a copy of the DaggerUIGraph Canvas Image
        /// </summary>
        [Browsable(false)]
        public Bitmap CanvasImage
        {
            get
            {
                return dsDaggerUIGraph1.CanvasImage;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Render a Media File
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public int RenderMediaFile(string filename)
        {
            int hr = _graphBuilder.RenderFile(filename, string.Empty);
            dsDaggerUIGraph1.SyncGraphs(null);
            dsDaggerUIGraph1.ArrangeNodes(AutoArrangeStyle.All);
            return hr;
        }

        /// <summary>
        /// Render a Media URL
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public int RenderURL(string URL)
        {
            // guess what?  It's the same thing as RenderMediaFile :)
            return RenderMediaFile(URL);
        }

        /// <summary>
        /// Get the DSFilterNodeUI that is marked as the reference clock for the graph
        /// </summary>
        /// <returns></returns>
        internal DSFilterNodeUI GetReferenceClock()
        {
            foreach (DSFilterNodeUI node in dsDaggerUIGraph1.AllNodes)
            {
                if (node._clockButton.Tag != null && (bool)node._clockButton.Tag == true)
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Save the current IFilterGraph to a grf file
        /// </summary>
        /// <param name="filename"></param>
        public void SaveFilterGraph(string filename)
        {
            FilterGraphTools.SaveGraphFile(_graphBuilder, filename);
        }

        /// <summary>
        /// Force the Disconnection of all IPins in the filterGraph
        /// </summary>
        public void DisconnectAllPins()
        {
            dsDaggerUIGraph1.BeginCanvasUpdate();

            foreach (DaggerNode node in dsDaggerUIGraph1.Graph.AllNodes)
            {
                foreach (DaggerOutputPin pin in node.OutputPins)
                {
                    // force disconnect even if it says it can't
                    pin.Disconnect(true);
                }
            }

            dsDaggerUIGraph1.EndCanvasUpdate();
        }

        /// <summary>
        /// Create and add a filter from a DSFilterTreeViewNode
        /// </summary>
        /// <param name="tn"></param>
        public IBaseFilter AddFilter(DSFilterTreeViewNode tn)
        {
            return dsDaggerUIGraph1.AddFilter(tn);
        }

        #endregion

        #region Media Control

        public void Play()
        {
            if (_mediaState != FilterState.Running)
            {
                _mediaControl.Run();
                _mediaState = FilterState.Running;
            }
            _playButton.Enabled = false;
            _pauseButton.Enabled = true;
            _stopButton.Enabled = true;
            timeSliderTimer.Enabled = true;
        }

        public void Pause()
        {
            if (_mediaState != FilterState.Paused)
            {
                _mediaControl.Pause();
                _mediaState = FilterState.Paused;
            }
            _playButton.Enabled = true;
            _pauseButton.Enabled = false;
            _stopButton.Enabled = true;
            timeSliderTimer.Enabled = false;
        }

        public void Stop()
        {
            try
            {
                if (_mediaState != FilterState.Stopped)
                {
                    _mediaControl.Stop();
                    _mediaState = FilterState.Stopped;
                }
            }
            catch(Exception ex)
            {
#if DEBUG 
                MessageBox.Show(ex.Message);
#endif
            }
            finally
            {
                _playButton.Enabled = true;
                _pauseButton.Enabled = true;
                _stopButton.Enabled = false;
                timeSliderTimer.Enabled = false;
            }
        }

        public void SyncGraphs()
        {
            dsDaggerUIGraph1.SyncGraphs(null);

            // reset the canvas size to the actual size
            dsDaggerUIGraph1.CanvasSize = dsDaggerUIGraph1.ActualCanvasSize;
        }

        public void ArrangeNodes()
        {
            dsDaggerUIGraph1.ArrangeNodes(AutoArrangeStyle.All);
        }

        public int StepOneFrame()
        {
            int hr = 0;

            // If the Frame Stepping interface exists, use it to step one frame
            if (_frameStep != null)
            {
                // The graph must be paused for frame stepping to work
                Pause();

                // Step the requested number of frames, if supported
                hr = _frameStep.Step(1, null);
            }

            // update the TimeSlider position
            timeSliderTimer_Tick(null, null);

            return hr;
        }

        #endregion

        #region DaggerUIGraph Events

        /////////////////////////////////////////////////////////////////////////////////////////////////
        // These events translate the user interface of DaggerLib into DirectShow filter connection
        // methods.
        /////////////////////////////////////////////////////////////////////////////////////////////////

        bool dsDaggerUIGraph1_BeforeSelectionDeleted(object sender)
        {
            if (_mediaState != FilterState.Stopped)
            {
                MessageBox.Show("Graph must be stopped before deleting", "Error");
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Called when a uinode is added it the graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dsDaggerUIGraph1_ControlAdded(object sender, ControlEventArgs e)
        {
            // get the last uinode added to the graph
            DSFilterNodeUI node = (sender as DSDaggerUIGraph).Controls[(sender as DSDaggerUIGraph).Controls.Count - 1] as DSFilterNodeUI;
            if (node != null)
            {
                // set it's pin placement
                if (node.PinPlacement != _pinPlacement)
                {
                    node.PinPlacement = _pinPlacement;
                }

                // set the Properties Button styles based on ModalProperties setting
                node.SetModalProperties();

                // update it's pins in case they changed during the filter insertion process (EVR especially)
                (node.Node as DSFilterNode).SyncPins();
            }
        }

        /// <summary>
        /// Called when a node or a connection is added/removed in the DaggerLib graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Graph_OnTopologyChanged(object sender, EventArgs e)
        {
            // the topology of the graph has changed
            // see if seeking is available and set the extent of the TimeSliderControl
            _mediaSeeking = _graph as IMediaSeeking;
            if (_mediaSeeking != null)
            {
                long duration = 0;
                _mediaSeeking.GetDuration(out duration);

                // only set the extents if something has changed
                if (_timeSliderControl.Extent != (int)(duration / 10000))
                {
                    _timeSliderControl.Extent = (int)(duration / 10000);
                    _timeSliderControl.Min = 0;
                    _timeSliderControl.Max = _timeSliderControl.Extent;
                }
            }

            // see if frame step is available
            _frameStepButton.Enabled = GetFrameStepInterface();

            // see if IVideoWindow is available
            _videoWindow = _graph as IVideoWindow;
        }

        /// <summary>
        /// Called before a node is removed from a DaggerLib Graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns>false if node cannot be removed</returns>
        bool Graph_BeforeNodeRemoved(DaggerNode node)
        {
            // make sure the graph is stopped
            Stop();

            // make sure there if in fact a IBaseFilter in this node
            if ((node as DSFilterNode)._filter != null)
            {
                int hr = _graph.RemoveFilter((node as DSFilterNode)._filter);
                if (hr != 0)
                {
                    // the filtergraph won't let go of the filter
                    MessageBox.Show(DsError.GetErrorText(hr));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if we are allowed to disconnect two pins
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <returns>false if pins failed to disconnect</returns>
        bool Graph_BeforePinsDisconnected(DaggerOutputPin output, DaggerInputPin input)
        {
            int hr = 0;

            // make sure the graph is stopped
            Stop();

            // do these two pins still belong to the ds filters?
            if ((output.ParentNode as DSFilterNode).GetPins().Contains((output as DSOutputPin)._pin) &&
                (input.ParentNode as DSFilterNode).GetPins().Contains((input as DSInputPin)._pin))
            {
                // are these two pins actually connected?  This can be caused by Syncing a graph that has pins re-routed elsewhere.
                IPin conto = null;
                (output as DSOutputPin)._pin.ConnectedTo(out conto);
                if ((input as DSInputPin)._pin != conto)
                {
                    if (conto != null)
                    {
                        Marshal.ReleaseComObject(conto);
                    }
                    return true;
                }
                if (conto != null)
                {
                    Marshal.ReleaseComObject(conto);
                }

                // If we fail to disconnect the IPins return false.
                // This will tell DaggerLib not to remove the Noodle in it's own internal graph.
                hr = (output as DSOutputPin)._pin.Disconnect();
                if (hr != 0 && hr != 1) /* 1 = not connected*/
                {
                    MessageBox.Show(DsError.GetErrorText(hr));
                    return false;
                }

                // Call Disconnect on the input also to reset it's allowed media types (who knew?)
                (input as DSInputPin)._pin.Disconnect();

                // sync the pins on the nodes in case pins have been added or deleted
                if ((output.ParentNode as DSFilterNode)._filter != null)
                {
                    (output.ParentNode as DSFilterNode).SyncPins();
                }
                if ((input.ParentNode as DSFilterNode)._filter != null)
                {
                    (input.ParentNode as DSFilterNode).SyncPins();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Try to connect 2 pins in the FilterGraph.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <returns>false if pins cannot be connected</returns>
        bool Graph_BeforePinsConnected(DaggerOutputPin output, DaggerInputPin input)
        {
            IPin connectedTo;
            (output as DSOutputPin)._pin.ConnectedTo(out connectedTo);
            if (connectedTo == (input as DSInputPin)._pin)
            {
                // these two are already connected in DSGraph so allow it
                Marshal.ReleaseComObject(connectedTo);
                return true;
            }

            if (connectedTo != null)
            {
                Marshal.ReleaseComObject(connectedTo);
            }

            // make sure the graph is stopped
            Stop();

            int hr = 0;

            if (!_connectIntelligent)
            {
                hr = _graph.ConnectDirect((output as DSOutputPin)._pin, (input as DSInputPin)._pin, null);
                if (hr == 0)
                {
                    // sync the pins on the nodes
                    (output.ParentNode as DSFilterNode).SyncPins();
                    (input.ParentNode as DSFilterNode).SyncPins();

                    return true;
                }
            }
            else
            {
                // try intelligent connection with the GraphBuilder
                hr = _graphBuilder.Connect((output as DSOutputPin)._pin, (input as DSInputPin)._pin);
                if (hr == 0 || hr == DsResults.S_PartialRender)
                {
                    // sync the FilterGraph and the DaggerGraph
                    dsDaggerUIGraph1.SyncGraphs(null);

                    // sync the pins on the nodes
                    (output.ParentNode as DSFilterNode).SyncPins();
                    (input.ParentNode as DSFilterNode).SyncPins();

                    // because SyncGraph already creates needed connections, return false to cancel this connection
                    return false;
                }
            }

            // cancel pin connection operations
            dsDaggerUIGraph1.StopPinConnect();

            // if we get here, we simply couldn't connect the pins
            MessageBox.Show(DsError.GetErrorText(hr));

            // sync the pins on the nodes just in case an attempt to connect created new pins
            (output.ParentNode as DSFilterNode).SyncPins();
            (input.ParentNode as DSFilterNode).SyncPins();

            return false;
        }

        #endregion

        #region ToolStrip Button Events

        private void _playButton_Click(object sender, EventArgs e)
        {
            Play();
        }

        private void _pauseButton_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void _stopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void _refreshGraphButton_Click(object sender, EventArgs e)
        {
            SyncGraphs();
        }

        private void _arrangeNodesButton_Click(object sender, EventArgs e)
        {
            ArrangeNodes();
        }

        private void _disconnectAllPinsButton_Click(object sender, EventArgs e)
        {
            DisconnectAllPins();
        }

        private void _frameStepButton_Click(object sender, EventArgs e)
        {
            StepOneFrame();
        }

        #endregion

        #region Overrides

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WMGraphNotify:
                    {
                        HandleGraphEvent();
                        break;
                    }
            }

            // Pass this message to the video window for notification of system changes
            try
            {
                if (_videoWindow != null)
                    _videoWindow.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);
            }
            catch
            {
                _videoWindow = null;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Event Handlers

        void DSGraphEditPanel_Disposed(object sender, EventArgs e)
        {
            // only stop the graph if we created it
            if (_filterGraphCreated)
            {
                Stop();
            }

            timeSliderTimer.Enabled = false;

            // remove from rot table if we added it
            if (rot != null)
            {
                rot.Dispose();
                rot = null;
            }

            try
            {
                // nix the media sink
                if (_mediaEventEx != null)
                {
                    _mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                }

                _graphBuilder = null;
                _mediaControl = null;
                _mediaSeeking = null;
                _basicAudio = null;
                _basicVideo = null;
                _videoWindow = null;
                _mediaEventEx = null;

                // only release the filtergraph if WE created it.  If the user passed one into a constructor,
                // it's up to them to release it.
                if (_filterGraphCreated)
                {
                    int refc = Marshal.ReleaseComObject(_graph);
                }

                // if it is a connected graph, renounce all claim on the proxie's RCW
                if (_isRemoteGraph)
                {
                    Marshal.FinalReleaseComObject(_graph);
                }
            }
            catch (InvalidComObjectException ex)
            {
                // the RCW became disconnected,  This is most likely caused by connecting to a remote graph
                // in the same AppDomain.  Just ignore it.
            }
        }

        /// <summary>
        /// Callback that handles events sent from the IFilterGraph
        /// </summary>
        private void HandleGraphEvent()
        {
            int hr = 0;
            EventCode evCode;
            IntPtr evParam1, evParam2;

            // Make sure that we don't access the media event interface
            // after it has already been released.
            if (_mediaEventEx == null)
                return;

            // Process all queued events
            while (_mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
            {
                // Free memory associated with callback, since we're not using it
                hr = _mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

                // If this is the end of the clip, reset to beginning
                if (evCode == EventCode.Complete)
                {
                    Stop();
                    // Rewind to first frame of movie
                    hr = _mediaSeeking.SetPositions((long)_timeSliderControl.Min * 10000, AMSeekingSeekingFlags.AbsolutePositioning,
                      null, AMSeekingSeekingFlags.NoPositioning);
                    _timeSliderControl.Pos = _timeSliderControl.Min;
                }
            }
        }

        /// <summary>
        /// Timer Tick Event that updates time slider positions during playback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeSliderTimer_Tick(object sender, EventArgs e)
        {
            if (_mediaSeeking != null)
            {
                try
                {
                    long pos = 0;
                    _mediaSeeking.GetCurrentPosition(out pos);
                    _timeSliderControl.Pos = (int)(pos / 10000);
                }
                catch
                {
                    timeSliderTimer.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Update MediaSeeking based on events from the TimesliderControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timeSliderControl_ValuesChanged(object sender, EventArgs e)
        {
            switch (_timeSliderControl.TrackMode)
            {
                case TimeSliderTrackMode.None:
                    break;
                case TimeSliderTrackMode.StartTime:
                    if (_mediaSeeking != null)
                    {
                        long cur, stop;
                        _mediaSeeking.GetPositions(out cur, out stop);

                        // if the start position is moved past current position, update time
                        if (_timeSliderControl.Min > (int)(cur / 10000))
                        {
                            _mediaSeeking.SetPositions((long)_timeSliderControl.Min * 10000, AMSeekingSeekingFlags.AbsolutePositioning, 0, AMSeekingSeekingFlags.NoPositioning);
                        }
                    }
                    break;
                case TimeSliderTrackMode.StopTime:
                    if (_mediaSeeking != null)
                    {
                        long cur, stop;
                        _mediaSeeking.GetPositions(out cur, out stop);

                        // if the end position is moved before current position, update time
                        if (_timeSliderControl.Max < (int)(cur / 10000))
                        {
                            // rewind to start and set new end time
                            _mediaSeeking.SetPositions((long)_timeSliderControl.Min * 10000, AMSeekingSeekingFlags.AbsolutePositioning, (long)_timeSliderControl.Max * 10000, AMSeekingSeekingFlags.AbsolutePositioning);
                        }
                        else
                        {
                            // just set the end time
                            _mediaSeeking.SetPositions(0, AMSeekingSeekingFlags.NoPositioning, (long)_timeSliderControl.Max * 10000, AMSeekingSeekingFlags.AbsolutePositioning);
                        }
                    }
                    break;
                case TimeSliderTrackMode.CurrentTime:
                    if (_mediaSeeking != null)
                    {
                        _mediaSeeking.SetPositions((long)_timeSliderControl.Pos * 10000, AMSeekingSeekingFlags.AbsolutePositioning, 0, AMSeekingSeekingFlags.NoPositioning);
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Menus and Menu Handlers

        private void _optionsDropDownButton_DropDownOpening(object sender, EventArgs e)
        {
            // build the Pin Placement menu items
            pinPlacementToolStripMenuItem.DropDown.Items.Clear();
            for (int i = 0; i < 3; i++)
            {
                ToolStripMenuItem pinPlacementMenuItem = new ToolStripMenuItem(((DaggerNodePinPlacement)i).ToString());
                pinPlacementMenuItem.Tag = i;
                pinPlacementMenuItem.Click += new EventHandler(pinPlacementMenuItem_Click);
                if (i == (int)_pinPlacement) pinPlacementMenuItem.Checked = true;
                pinPlacementToolStripMenuItem.DropDown.Items.Add(pinPlacementMenuItem);
            }

            // set the check state of toggle items
            dropShadowToolStripMenuItem.Checked = this.dsDaggerUIGraph1.DropShadowVisible;
            showPinNamesToolStripMenuItem.Checked = this.dsDaggerUIGraph1.ShowPinNames;
            modalPropertiesToolStripMenuItem.Checked = this.dsDaggerUIGraph1.ModalProperties;
            useIntelligentConnectToolStripMenuItem.Checked = _connectIntelligent;
            _useClockToolStripMenuItem.Checked = this._useReferenceClock;
            _timeSliderVisibleMenuItem.Text = _timeSliderControl.Visible ? "Hide Time Slider" : "Show Time Slider";
        }

        private void _noodleStyleButton_DropDownOpening(object sender, EventArgs e)
        {
            // build the noodle-style menu items
            _noodleStyleButton.DropDown.Items.Clear();
            for (int i = 0; i < 6; i++)
            {
                ToolStripMenuItem nsMenuItem = new ToolStripMenuItem(((NoodleStyle)i).ToString());
                nsMenuItem.Tag = i;
                nsMenuItem.Click += new EventHandler(nsMenuItem_Click);
                if (i == (int)dsDaggerUIGraph1.NoodleStyle) nsMenuItem.Checked = true;
                _noodleStyleButton.DropDown.Items.Add(nsMenuItem);
            }
        }

        /// <summary>
        /// the user toggled the Time Slider Visible menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timeSliderVisibleMenuItem_Click(object sender, EventArgs e)
        {
            _timeSliderControl.Visible = !_timeSliderControl.Visible;
        }

        // the user selected a new pin placement style
        void pinPlacementMenuItem_Click(object sender, EventArgs e)
        {
            PinPlacement = (DaggerNodePinPlacement)(sender as ToolStripMenuItem).Tag;
        }

        // the user selected a new noodle style
        void nsMenuItem_Click(object sender, EventArgs e)
        {
            dsDaggerUIGraph1.NoodleStyle = (NoodleStyle)(sender as ToolStripMenuItem).Tag;
        }

        /// <summary>
        /// Toggle Drop Shadow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dropShadowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dsDaggerUIGraph1.DropShadowVisible = dropShadowToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Toggle Show Pin Names
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showPinNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dsDaggerUIGraph1.ShowPinNames = showPinNamesToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Toggle Modal Properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modalPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dsDaggerUIGraph1.ModalProperties = modalPropertiesToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Toggle Connect Intelligent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void useIntelligentConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectIntelligent = useIntelligentConnectToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Toggle Use Clock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _useClockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UseReferenceClock = _useClockToolStripMenuItem.Checked;
        }

        private void _saveGraphFileButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "Untitled.grf";
            sfd.DefaultExt = "grf";
            sfd.Filter = "Graph Files (*.grf)|*.grf";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveFilterGraph(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving FilterGraph");
                }
            }
        }

        private void _renderMediaFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // we don't want to create a new DSGraphEdit panel, just render
                // the media file onto the existing graph
                try
                {
                    // show we're busy rendering and building up the ui for the graph
                    Cursor = Cursors.WaitCursor;

                    int hr = RenderMediaFile(ofd.FileName);
                    if (hr != 0)
                    {
                        MessageBox.Show(DsError.GetErrorText(hr));
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            ofd.Dispose();
            ofd = null;
        }

        private void _renderURLButton_Click(object sender, EventArgs e)
        {
            URLDialog ud = new URLDialog();
            if (ud.ShowDialog() == DialogResult.OK)
            {
                // we don't want to create a new DSGraphEdit panel, just render
                // the URL into the existing graph
                try
                {
                    // show we're busy rendering and building up the ui for the graph
                    Cursor = Cursors.WaitCursor;

                    int hr = RenderMediaFile(ud.URL);
                    if (hr != 0)
                    {
                        MessageBox.Show(DsError.GetErrorText(hr));
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            ud.Dispose();
            ud = null;
        }

        #endregion
    }
}
