using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Core;
using DaggerLib.UI.Windows;
using DirectShowLib;
using DirectShowLib.Dvd;
using DirectShowLib.DMO;
using MediaFoundation;
using MediaFoundation.EVR;

namespace DaggerLib.DSGraphEdit
{
    /// <summary>
    /// The UI control to represent a DSFilterNode in the DSDaggerUIGraph
    /// </summary>
    public class DSFilterNodeUI : DaggerLib.UI.Windows.DaggerUINode
    {
        private DSFilterNode _dsfilternode;
        internal DaggerLib.UI.Windows.SimpleImageButton _expandPropertiesButton;
        private PropertyPagePanel _properties;
        internal DaggerLib.UI.Windows.SimpleImageButton _clockButton;
        internal VideoInternalWindow _videoWindow;
        private IMFVideoDisplayControl m_pVideoDisplay;
        private Size _defaultVideoWindowSize = new Size(130, 60);
        private DaggerLib.UI.Windows.SimpleImageButton _detachVideoWindowButton;
        private DaggerLib.UI.Windows.SimpleImageButton _closeButton;
        private Size _storedVideoWindowSize;
        private ImageList _propertiesImageList;
        private System.ComponentModel.IContainer components;
        private int _dmoPropertyCount = 0;
        internal SimpleImageButton dvdControlbutton;
        private ContextMenuStrip dvdControlContextMenuStrip;
        private ToolStripMenuItem titleMenuToolStripMenuItem;
        private ToolStripMenuItem resumeToolStripMenuItem;
        private ToolStripMenuItem rootMenuToolStripMenuItem;
        private ToolStripMenuItem chaptersToolStripMenuItem;
        internal IReferenceClock _referenceClock;

        public DSFilterNodeUI()
        {
            InitializeComponent();
            BackColor = Color.SeaGreen;
            InternalControl.BackColor = Color.Transparent;
            DaggerNodeAttached += new DaggerLib.Core.DaggerNodeAttachedHandler(DSFilterNodeUI_DaggerNodeAttached);
            BeforePinContextShown += new DaggerLib.Core.DaggerBasePinBeforeShowContextMenuHandler(DSFilterNodeUI_BeforePinContextShown);
        }

        #region InitializeComponent

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DSFilterNodeUI));
            this._expandPropertiesButton = new DaggerLib.UI.Windows.SimpleImageButton();
            this._clockButton = new DaggerLib.UI.Windows.SimpleImageButton();
            this._detachVideoWindowButton = new DaggerLib.UI.Windows.SimpleImageButton();
            this._closeButton = new DaggerLib.UI.Windows.SimpleImageButton();
            this._propertiesImageList = new System.Windows.Forms.ImageList(this.components);
            this.dvdControlbutton = new DaggerLib.UI.Windows.SimpleImageButton();
            this.dvdControlContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.titleMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rootMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chaptersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dvdControlContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _internalControl
            // 
            this._internalControl.Location = new System.Drawing.Point(16, 21);
            this._internalControl.Size = new System.Drawing.Size(173, 14);
            // 
            // _expandPropertiesButton
            // 
            this._expandPropertiesButton.ButtonImage = null;
            this._expandPropertiesButton.ButtonImage2 = null;
            this._expandPropertiesButton.MouseInsideTint = System.Drawing.Color.White;
            this._expandPropertiesButton.MouseOutsideTint = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._expandPropertiesButton.MultiState = true;
            this._expandPropertiesButton.Name = "_expandPropertiesButton";
            this._expandPropertiesButton.State = false;
            this._expandPropertiesButton.TabIndex = 1;
            this._expandPropertiesButton.ToolTipEnabled = true;
            this._expandPropertiesButton.ToolTipText = "Show/Hide Properties";
            this._expandPropertiesButton.StateChanged += new DaggerLib.UI.Windows.SimpleImageButton.StateChangedEventHandler(this._expandPropertiesButton_StateChanged);
            this._expandPropertiesButton.Click += new System.EventHandler(this._expandPropertiesButton_Click);
            // 
            // _clockButton
            // 
            this._clockButton.ButtonImage = ((System.Drawing.Image)(resources.GetObject("_clockButton.ButtonImage")));
            this._clockButton.ButtonImage2 = null;
            this._clockButton.MouseInsideTint = System.Drawing.Color.White;
            this._clockButton.MouseOutsideTint = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this._clockButton.MultiState = false;
            this._clockButton.Name = "_clockButton";
            this._clockButton.State = false;
            this._clockButton.TabIndex = 2;
            this._clockButton.ToolTipEnabled = true;
            this._clockButton.ToolTipText = "Set as Reference Clock";
            this._clockButton.Click += new System.EventHandler(this._clockButton_Click);
            // 
            // _detachVideoWindowButton
            // 
            this._detachVideoWindowButton.ButtonImage = ((System.Drawing.Image)(resources.GetObject("_detachVideoWindowButton.ButtonImage")));
            this._detachVideoWindowButton.ButtonImage2 = null;
            this._detachVideoWindowButton.MouseInsideTint = System.Drawing.Color.White;
            this._detachVideoWindowButton.MouseOutsideTint = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))));
            this._detachVideoWindowButton.MultiState = false;
            this._detachVideoWindowButton.Name = "_detachVideoWindowButton";
            this._detachVideoWindowButton.State = false;
            this._detachVideoWindowButton.TabIndex = 3;
            this._detachVideoWindowButton.ToolTipEnabled = true;
            this._detachVideoWindowButton.ToolTipText = "Detach Video Window";
            this._detachVideoWindowButton.Click += new System.EventHandler(this._detachVideoWindowButton_Click);
            // 
            // _closeButton
            // 
            this._closeButton.ButtonImage = ((System.Drawing.Image)(resources.GetObject("_closeButton.ButtonImage")));
            this._closeButton.ButtonImage2 = null;
            this._closeButton.MouseInsideTint = System.Drawing.Color.White;
            this._closeButton.MouseOutsideTint = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this._closeButton.MultiState = false;
            this._closeButton.Name = "_closeButton";
            this._closeButton.State = false;
            this._closeButton.TabIndex = 4;
            this._closeButton.ToolTipEnabled = true;
            this._closeButton.ToolTipText = "Remove Filter";
            this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
            // 
            // _propertiesImageList
            // 
            this._propertiesImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_propertiesImageList.ImageStream")));
            this._propertiesImageList.TransparentColor = System.Drawing.Color.Transparent;
            this._propertiesImageList.Images.SetKeyName(0, "down.png");
            this._propertiesImageList.Images.SetKeyName(1, "up.png");
            this._propertiesImageList.Images.SetKeyName(2, "cog.png");
            // 
            // dvdControlbutton
            // 
            this.dvdControlbutton.ButtonImage = ((System.Drawing.Image)(resources.GetObject("dvdControlbutton.ButtonImage")));
            this.dvdControlbutton.ButtonImage2 = null;
            this.dvdControlbutton.MouseInsideTint = System.Drawing.Color.White;
            this.dvdControlbutton.MouseOutsideTint = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))));
            this.dvdControlbutton.MultiState = false;
            this.dvdControlbutton.Name = "dvdControlbutton";
            this.dvdControlbutton.State = false;
            this.dvdControlbutton.TabIndex = 5;
            this.dvdControlbutton.ToolTipEnabled = false;
            this.dvdControlbutton.ToolTipText = "DVD Control";
            this.dvdControlbutton.Visible = false;
            this.dvdControlbutton.Click += new System.EventHandler(this.dvdControlbutton_Click);
            // 
            // dvdControlContextMenuStrip
            // 
            this.dvdControlContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.titleMenuToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.rootMenuToolStripMenuItem,
            this.chaptersToolStripMenuItem});
            this.dvdControlContextMenuStrip.Name = "dvdControlContextMenuStrip";
            this.dvdControlContextMenuStrip.Size = new System.Drawing.Size(134, 92);
            this.dvdControlContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.dvdControlContextMenuStrip_Opening);
            // 
            // titleMenuToolStripMenuItem
            // 
            this.titleMenuToolStripMenuItem.Name = "titleMenuToolStripMenuItem";
            this.titleMenuToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.titleMenuToolStripMenuItem.Text = "Title Menu";
            this.titleMenuToolStripMenuItem.Click += new System.EventHandler(this.titleMenuToolStripMenuItem_Click);
            // 
            // resumeToolStripMenuItem
            // 
            this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
            this.resumeToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.resumeToolStripMenuItem.Text = "Resume";
            this.resumeToolStripMenuItem.Click += new System.EventHandler(this.resumeToolStripMenuItem_Click);
            // 
            // rootMenuToolStripMenuItem
            // 
            this.rootMenuToolStripMenuItem.Name = "rootMenuToolStripMenuItem";
            this.rootMenuToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.rootMenuToolStripMenuItem.Text = "Root Menu";
            this.rootMenuToolStripMenuItem.Click += new System.EventHandler(this.rootMenuToolStripMenuItem_Click);
            // 
            // chaptersToolStripMenuItem
            // 
            this.chaptersToolStripMenuItem.Name = "chaptersToolStripMenuItem";
            this.chaptersToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.chaptersToolStripMenuItem.Text = "Chapters";
            // 
            // DSFilterNodeUI
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CaptionButtons.Add(this.dvdControlbutton);
            this.CaptionButtons.Add(this._detachVideoWindowButton);
            this.CaptionButtons.Add(this._clockButton);
            this.CaptionButtons.Add(this._expandPropertiesButton);
            this.CaptionButtons.Add(this._closeButton);
            this.CaptionColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.CaptionColorUnfocused = System.Drawing.Color.Blue;
            this.CaptionSize = 16;
            this.Controls.Add(this.dvdControlbutton);
            this.Controls.Add(this._detachVideoWindowButton);
            this.Controls.Add(this._clockButton);
            this.Controls.Add(this._expandPropertiesButton);
            this.Controls.Add(this._closeButton);
            this.Name = "DSFilterNodeUI";
            this.Size = new System.Drawing.Size(205, 40);
            this.Controls.SetChildIndex(this._closeButton, 0);
            this.Controls.SetChildIndex(this._expandPropertiesButton, 0);
            this.Controls.SetChildIndex(this._clockButton, 0);
            this.Controls.SetChildIndex(this._detachVideoWindowButton, 0);
            this.Controls.SetChildIndex(this.dvdControlbutton, 0);
            this.Controls.SetChildIndex(this._internalControl, 0);
            this.dvdControlContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Modify a pin's context menu before showing it
        /// </summary>
        /// <param name="pin"></param>
        void DSFilterNodeUI_BeforePinContextShown(DaggerLib.Core.DaggerBasePin pin)
        {
            (pin.PinUIElements as PinUI).ContextMenuStrip = new ContextMenuStrip();

            // if it is an output pin and is not connected, offer the Render Pin menu item
            if (pin is DSOutputPin && !pin.IsConnected)
            {
                ToolStripMenuItem tmi = new ToolStripMenuItem("Render Pin");
                tmi.Tag = pin;
                tmi.Click += new EventHandler(renderPinMenuItem_Click);
                (pin.PinUIElements as PinUI).ContextMenuStrip.Items.Add(tmi);
            }

            ToolStripMenuItem ppmi = new ToolStripMenuItem("Pin Properties");
            ppmi.Tag = pin;
            ppmi.Click += new EventHandler(tmi_Click);
            (pin.PinUIElements as PinUI).ContextMenuStrip.Items.Add(ppmi);
        }

        /// <summary>
        /// User has clicked on a pin properties menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tmi_Click(object sender, EventArgs e)
        {
            // if this pin implements ISpecifyPropertyPages, show that instead            
            PropertiesDialog pd;
            IPin pin = null;
            string pinname;

            // get the DaggerPin
            DSOutputPin opin = (sender as ToolStripMenuItem).Tag as DSOutputPin;
            if (opin != null)
            {
                // it's an output pin
                pin = opin._pin;
                pinname = opin.Name;
            }
            else
            {
                // it's an input pin
                pin = ((sender as ToolStripMenuItem).Tag as DSInputPin)._pin;
                pinname = ((sender as ToolStripMenuItem).Tag as DSInputPin).Name;
            }

            ISpecifyPropertyPages proppage = pin as ISpecifyPropertyPages;
            bool displayed = false;
            if (proppage != null)
            {
                // display the pin's property pages
                displayed = DirectShowLib.Utils.FilterGraphTools.ShowPinFilterPropertyPage(pin, this.TopLevelControl.Handle, pinname);
            }

            // if ShowPinFilterPropertyPage failed, or there's no ISpecifyPropertyPages, show the default pin info
            if(!displayed)
            {
                pd = new PropertiesDialog(pinname, pin);
                pd.ShowDialog();
                pd.Dispose();
                pd = null;
            }
        }

        /// <summary>
        /// User has clicked on a render pin menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void renderPinMenuItem_Click(object sender, EventArgs e)
        {
            int hr = 0;

            // get the DaggerPin
            DSOutputPin pin = (sender as ToolStripMenuItem).Tag as DSOutputPin;

            // get the Parent UIGraph
            DSDaggerUIGraph parentui = this.Parent as DSDaggerUIGraph;

            // get the FilterGraph
            IGraphBuilder graph = parentui._Graph as IGraphBuilder;

            // atempt to render the pin
            try
            {
                hr = graph.Render(pin._pin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error rendering pin");
            }

            Point dropstart = parentui.PointToClient(this.PointToScreen((pin.PinUIElements as PinUI).PinLocation));
            parentui._dropLocation = new Point(dropstart.X + 25, dropstart.Y);

            // Sync the graphs
            parentui.SyncGraphs(null);

            if (hr != 0)
            {
                MessageBox.Show(DsError.GetErrorText(hr));
            }
        }

        /// <summary>
        /// Event that is raised after a DaggerNode has been created and associated to the UI element
        /// </summary>
        /// <param name="node"></param>
        void DSFilterNodeUI_DaggerNodeAttached(DaggerLib.Core.DaggerNode node)
        {
            _dsfilternode = (DSFilterNode)node;
            CaptionText = node.ToString();

            // hook the AfterNodeRemoved event to dispose of any directshow interfaces
            node.AfterNodeRemoved += new DaggerLib.Core.AfterNodeRemoveHandler(node_AfterNodeRemoved);

            // get the IBaseFilter from the DSFilterNode
            IBaseFilter filter = _dsfilternode._filter;

            // only grab the video window or EVR if it was manually added to the graph via the UI
            if (_dsfilternode._manualAdded || (_dsfilternode.ParentGraph.ParentUIGraph as DSDaggerUIGraph)._filterGraphCreated)
            {
                // if it supports IVideoWindow create a VideoInternalWindow for it
                IVideoWindow vw = filter as IVideoWindow;
                if (vw != null)
                {
                    try
                    {
                        _videoWindow = new VideoInternalWindow(CaptionText, filter);
                        _videoWindow.Dock = DockStyle.Fill;
                        _videoWindow.Visible = true;
                        InternalControl.Controls.Add(_videoWindow);

                        // only nodes with video windows are resizeable
                        Resizable = true;

                        // hook the connection events to init/deinit the video window
                        node.ParentGraph.AfterPinsConnected += new DaggerLib.Core.PinAfterConnectedHandler(ParentGraph_AfterPinsConnected);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        MessageBox.Show(ex.Message);
#endif
                        _videoWindow = null;
                    }
                }

                // if it's an Enhaced Video Renderer create a VideoInternalWindow for it
                // (see docs for Windows Media Foundation)
                IMFGetService mfgs = filter as IMFGetService;
                if (mfgs != null)
                {
                    // this is a video horse of a different color
                    // create a video clipping window for the Media Foundation Enhanced Video Renderer
                    try
                    {

                        // get the IMFVideoDisplayControl for the EVR filter
                        object o = null;
                        mfgs.GetService(MediaFoundation.MFServices.MR_VIDEO_RENDER_SERVICE,
                            typeof(IMFVideoDisplayControl).GUID,
                            out o
                            );
                        m_pVideoDisplay = o as IMFVideoDisplayControl;

                        // if the Video Size is 0,0 the EVR hasn't been initialized/connected yet
                        MediaFoundation.Misc.MFSize videoSize = new MediaFoundation.Misc.MFSize();
                        MediaFoundation.Misc.MFSize ar = new MediaFoundation.Misc.MFSize();
                        m_pVideoDisplay.GetNativeVideoSize(videoSize, ar);

                        if (videoSize.cx == 0 && videoSize.cy == 0)
                        {
                            // You only get one chance to set the number of pins in an EVR filter.
                            PinsComboBoxForm pcf = new PinsComboBoxForm();
                            if (pcf.ShowDialog() == DialogResult.OK)
                            {
                                (filter as IEVRFilterConfig).SetNumberOfStreams(pcf.Value);
                            }
                            pcf.Dispose();
                        }

                        _videoWindow = new VideoInternalWindow(CaptionText, m_pVideoDisplay);
                        _videoWindow.Dock = DockStyle.Fill;
                        _videoWindow.Visible = true;
                        InternalControl.Controls.Add(_videoWindow);

                        // only nodes with video windows are resizeable
                        Resizable = true;

                        // hook the connection events to init/deinit the video window
                        node.ParentGraph.AfterPinsConnected += new DaggerLib.Core.PinAfterConnectedHandler(ParentGraph_AfterPinsConnected);
                    }
                    catch (InvalidCastException)
                    {
                        m_pVideoDisplay = null;
                    }
                }
            }

            // if it's a DMO, create the DMO properties page for it
            if ((filter as IDMOWrapperFilter) != null)
            {
                // set the caption to show it's a DMO
                CaptionText = "DMO - " + CaptionText;
                CaptionColor = Color.Green;
                CaptionColorUnfocused = Color.LightGreen;
            }

            // remove clock button if it doesn't support IReferenceClock
            _referenceClock = filter as IReferenceClock;
            if (_referenceClock == null)
            {
                CaptionButtons.RemoveAt(CaptionButtons.AllButtons.IndexOf(_clockButton));
            }
            else
            {
                // see if this filter is the reference clock for the graph
                IReferenceClock graphClock = null;
                filter.GetSyncSource(out graphClock);
                _clockButton.Tag = false;
                _clockButton.MouseOutsideTint = Color.DarkGray;
                if (graphClock != null)
                {
                    if (graphClock == _referenceClock)
                    {
                        _clockButton.MouseOutsideTint = Color.Yellow;
                        _clockButton.Tag = true;
                    }
                    Marshal.ReleaseComObject(graphClock);
                }
            }

            // remove video window button if it's not a video window
            if (_videoWindow == null)
            {
                CaptionButtons.RemoveAt(CaptionButtons.AllButtons.IndexOf(_detachVideoWindowButton));
            }

            // Sync the pins to the Pin Property Pages
            SyncPinPropertyPages(null);

            // set it to the smallest possible size.  DaggerLib uses InternalControlMinimumSize
            // to prevent the UI node from being smaller than designated
            this.Size = new Size(1, 1);
        }

        void ParentGraph_AfterPinsConnected(DaggerLib.Core.DaggerOutputPin output, DaggerLib.Core.DaggerInputPin input)
        {            
            if (_videoWindow != null)
            {
                _videoWindow.InitVideoWindow();
            }
        }

        void node_AfterNodeRemoved(DaggerLib.Core.DaggerNode node)
        {
            //get rid of the property pages etc
            if (_properties != null)
            {
                _properties.Apply();
                _properties.Parent = null;
                _properties.CloseInterfaces();
                _properties = null;
            }

            // deinitialize the video window
            if (_videoWindow != null)
            {
                _videoWindow.Dispose();
                _videoWindow = null;
            }
        }

        /// <summary>
        /// Add, Remove, and Update Pin property pages in the _properties control
        /// </summary>
        internal void SyncPinPropertyPages(PropertyPagePanel properties)
        {
            if (properties == null) properties = _properties;

            if (properties != null)
            {
                // remove any pin property pages that are no longer valid
                List<IPin> pins = (Node as DSFilterNode).GetPins();
                for (int i = properties.TabControl.Controls.Count - 1; i > -1; i--)
                {
                    try
                    {
                        if (properties.TabControl.Controls[i].Tag is IPin)
                        {
                            if (!pins.Contains(_properties.TabControl.Controls[i].Tag as IPin))
                            {
                                properties.TabControl.Controls.RemoveAt(i);
                            }
                        }
                    }
                    catch
                    {
                        // the IPin was removed from the filter before we could sync it
                        // go ahead and remove the property page
                        properties.TabControl.Controls.RemoveAt(i);
                    }
                }

                // find or create a new tabpage for each remaining pin
                foreach (IPin pin in pins)
                {
                    TabPage tp = GetPinPropertyPage(pin);
                    PinPropertiesTextBox tbox = null;
                    if (tp == null)
                    {
                        // we don't have this one yet so create it
                        PinInfo pi;
                        pin.QueryPinInfo(out pi);
                        tp = new TabPage(pi.name);
                        DsUtils.FreePinInfo(pi);
                        tp.Tag = pin;
                        tbox = new PinPropertiesTextBox(pin);
                        tp.Controls.Add(tbox);
                        properties.TabControl.Controls.Add(tp);
                    }
                    else
                    {
                        // we already have this property page, refresh it's text box
                        tbox = tp.Controls[0] as PinPropertiesTextBox;
                        tbox.RefreshProperties();
                    }
                }
            }
        }

        /// <summary>
        /// Toggle Modal Property pages
        /// </summary>
        internal void SetModalProperties()
        {
            if ((this.Parent as DSDaggerUIGraph).ModalProperties && ((_dsfilternode._filter as IDMOWrapperFilter) == null))
            {
                if (_properties != null)
                {
                    // Kill the internal PropertyPagePanel.  Technically our internal PropertyPages
                    // and PropertyPages created with OleCreatePropertyFrame can coexist, but
                    // it's unpredicatable at best.
                    InternalControl.Controls.Remove(_properties);
                    _properties.Dispose();
                    _properties = null;
                    GC.Collect();
                }

                this._expandPropertiesButton.State = false;
                this._expandPropertiesButton.ToolTipText = "Properties";
                this._expandPropertiesButton.MultiState = false;
                this._expandPropertiesButton.ButtonImage = _propertiesImageList.Images[2];

                // if it doesn't have a video window embedded in it, there's not reason
                // it should be resizable
                if (_videoWindow == null)
                {
                    this.Resizable = false;
                }
            }
            else
            {
                if (_properties == null)
                {
                    // Create the PropertyPagePanel for the filter
                    _properties = new PropertyPagePanel(false,_dsfilternode._filter);
                    InternalControl.Controls.Add(_properties);
                    InternalControl.AutoScroll = true;                
                    _properties.Visible = false;

                    // if it's a DMO create the dmo properties for it
                    // DMOs always have non-modal properties because OleCreatePropertyFrame doesn't
                    // work on them
                    if ((_dsfilternode._filter as IDMOWrapperFilter) != null)
                    {
                        TabPage dmopage = SetDMOParams();
                        _properties.TabControl.TabPages.Add(dmopage);
                    }

                    SyncPinPropertyPages(null);
                }

                this._expandPropertiesButton.ToolTipText = "Show/Hide Properties";
                this._expandPropertiesButton.MultiState = true;
                this._expandPropertiesButton.ButtonImage = _propertiesImageList.Images[0];
                this._expandPropertiesButton.ButtonImage2 = _propertiesImageList.Images[1];

                // make sure the button is visible
                this._expandPropertiesButton.Visible = true;

                // we want it to be resizable
                this.Resizable = true;
            }
        }

        private TabPage GetPinPropertyPage(IPin pin)
        {
            if (_properties == null)
            {
                return null;
            }

            foreach (Control c in _properties.TabControl.Controls)
            {
                if (c.Tag == pin)
                {
                    return c as TabPage;
                }
            }
            return null;
        }

        public override Size AutoArrangeSize
        {
            get
            {
                if((Parent as DSDaggerUIGraph).ShowPinNames && (_videoWindow != null || _expandPropertiesButton.State))
                {
                    // adjust for the pin names being displayed outside the node
                    return new Size((int)WidestInputPinName() + (int)WidestOutputPinName() + this.Width, this.Height);
                }
                else
                {
                    return this.Size;
                }
            }
        }

        public override Point AutoArrangeOffset
        {
            get
            {
                if ((Parent as DSDaggerUIGraph).ShowPinNames && (_videoWindow != null || _expandPropertiesButton.State))
                {

                    return new Point((int)WidestInputPinName(), 0);
                }
                else
                {
                    return new Point(0, 0);
                }
            }
        }

        // Get the widest input pin
        internal float WidestInputPinName()
        {
            float maxpin = 0;
            Graphics g = Graphics.FromHwnd(this.Handle);
            foreach (DSInputPin pin in Node.InputPins)
            {
                SizeF fs = g.MeasureString(pin.Name, Font);
                maxpin = Math.Max(fs.Width, maxpin);
            }
            g.Dispose();
            return maxpin;
        }

        // Get the widest output pin
        internal float WidestOutputPinName()
        {
            float maxpin = 0;
            Graphics g = Graphics.FromHwnd(this.Handle);
            foreach (DSOutputPin pin in Node.OutputPins)
            {
                SizeF fs = g.MeasureString(pin.Name, Font);
                maxpin = Math.Max(fs.Width, maxpin);
            }
            g.Dispose();
            return maxpin;
        }

        /// <summary>
        /// Gets the minimum size DaggerLib should allow the internal control to be
        /// </summary>
        public override Size InternalControlMinimumSize
        {
            get
            {
                if (_expandPropertiesButton.State)
                {
                    if ((this._dsfilternode._filter as IDMOWrapperFilter) != null)
                    {
                        return new Size(200, (_dmoPropertyCount + 1) * 23);
                    }
                    else if (_properties != null)
                    {
                        return _properties.PageSize;
                    }
                    else
                    {
                        return new Size(1, 16);
                    }
                }
                else
                {
                    if (_videoWindow != null)
                    {
                        return _defaultVideoWindowSize;
                    }
                    else
                    {
                        // make it so the pin names will fit inside the node
                        int maxPinHeight = Node.InputPins.Count * (Parent as DSDaggerUIGraph).PinLegend.PinSize;
                        maxPinHeight = Math.Max(maxPinHeight, Node.OutputPins.Count * (Parent as DSDaggerUIGraph).PinLegend.PinSize);
                        return new Size((int)WidestInputPinName() + (int)WidestOutputPinName() + 3, maxPinHeight);
                    }
                }
            }
        }

        /// <summary>
        /// User has toggled the Properties button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _expandPropertiesButton_StateChanged(object sender, EventArgs e)
        {
            (ParentUIGraph as DaggerUIGraph).BeginCanvasUpdate();

            if (_properties != null)
            {
                if (_expandPropertiesButton.State)
                {
                    if (_videoWindow != null)
                    {
                        // store the size of the video window and toggle to properties pages
                        _storedVideoWindowSize = this.Size;
                        _videoWindow.Visible = false;
                    }
                    _properties.Visible = true;
                }
                else
                {
                    _properties.Visible = false;
                    if (_videoWindow != null)
                    {
                        // make the video window visible
                        _videoWindow.Visible = true;

                        // if we stored a previous size, set it now
                        if (_storedVideoWindowSize != null)
                        {
                            this.Size = _storedVideoWindowSize;
                        }
                    }
                    _properties.Apply();
                }
            }

            // set it to the smallest possible size if not showing a video window
            if (_videoWindow == null || _videoWindow.Visible == false)
            {
                this.Size = new Size(1, 1);
            }

            (ParentUIGraph as DaggerUIGraph).EndCanvasUpdate();
        }

        /// <summary>
        /// User has toggled the Detach Video Window button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _detachVideoWindowButton_Click(object sender, EventArgs e)
        {
            _videoWindow.DetachVideoWindow(false);
        }

        /// <summary>
        /// User has clicked the close filter button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _closeButton_Click(object sender, EventArgs e)
        {
            DSDaggerUIGraph parentGraph = Parent as DSDaggerUIGraph;
            parentGraph.Graph.DeleteNode(this._dsfilternode);
            parentGraph.RefreshGraph();
        }

        private void _expandPropertiesButton_Click(object sender, EventArgs e)
        {
            if ((this.Parent as DSDaggerUIGraph).ModalProperties && ((_dsfilternode._filter as IDMOWrapperFilter) == null))
            {
                try
                {
                    PropertiesDialog pd = new PropertiesDialog(CaptionText, _dsfilternode._filter);
                    SyncPinPropertyPages(pd.PropertyPagePanel);
                    pd.ShowDialog(this.TopLevelControl);
                    pd.Dispose();
                    _dsfilternode.SyncPins();
                    pd = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error creating Proprty Page");
                }
            }
        }

        /// <summary>
        /// Toggle SetSyncClock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _clockButton_Click(object sender, EventArgs e)
        {
            if ((bool)_clockButton.Tag)
            {
                // unset as reference clock
                _clockButton.MouseOutsideTint = Color.DarkGray;
                _clockButton.ToolTipText = "Set as Reference Clock";
                _clockButton.Tag = false;
            }
            else
            {
                DSFilterNodeUI refnode = (Parent.Parent as DSGraphEditPanel).GetReferenceClock();
                if (refnode != null)
                {
                    // unset the other reference clock
                    refnode._clockButton.MouseOutsideTint = Color.DarkGray;
                    refnode._clockButton.ToolTipText = "Set as Reference Clock";
                    refnode._clockButton.Tag = false;
                }

                // set as reference clock
                _clockButton.MouseOutsideTint = Color.Yellow;
                _clockButton.ToolTipText = "Unset Reference Clock";
                _clockButton.Tag = true;
            }

            // if the graph is set up to use a reference clock, set the new one
            if ((Parent.Parent as DSGraphEditPanel).UseReferenceClock)
            {
                (Parent.Parent as DSGraphEditPanel).Stop();

                DSFilterNodeUI refnode = (Parent.Parent as DSGraphEditPanel).GetReferenceClock();
                IMediaFilter mf = (Parent as DSDaggerUIGraph)._Graph as IMediaFilter;
                try
                {
                    if (refnode != null)
                    {
                        mf.SetSyncSource(refnode._referenceClock);
                    }
                    else
                    {
                        (Parent as DSDaggerUIGraph)._Graph.SetDefaultSyncSource();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error setting reference clock");
                }
            }
        }

        #region DMO Param Helpers

        private TabPage SetDMOParams()
        {
            int hr;

            // create the tab page and the layout grid
            TabPage tp = new TabPage("DMO Parameters");
            TableLayoutPanel tl = new TableLayoutPanel();
            tl.Dock = DockStyle.Top;
            tl.ColumnCount = 2;
            tl.AutoSizeMode = AutoSizeMode.GrowOnly;
            tl.AutoSize = true;
            tp.Controls.Add(tl);

            IMediaParamInfo paramInfo = this._dsfilternode._filter as IMediaParamInfo;
            if (paramInfo == null)
            {
                Resizable = false;
                InternalControl.Visible = false;
                return tp;
            }

            IMediaParams m_param = this._dsfilternode._filter as IMediaParams;

            hr = paramInfo.GetParamCount(out _dmoPropertyCount);
            DMOError.ThrowExceptionForHR(hr);

            tl.RowCount = _dmoPropertyCount;

            // Walk all the parameters
            for (int pCur = 0; pCur < _dmoPropertyCount; pCur++)
            {
                ParamInfo pInfo;
                IntPtr ip;

                hr = paramInfo.GetParamInfo(pCur, out pInfo);
                DMOError.ThrowExceptionForHR(hr);

                hr = paramInfo.GetParamText(pCur, out ip);
                DMOError.ThrowExceptionForHR(hr);

                string sName, sUnits;
                string[] sEnum;

                try
                {
                    ParseParamText(ip, out sName, out sUnits, out sEnum);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ip);
                }

                Label l = new Label();
                l.Text = pInfo.szLabel;
                tl.Controls.Add(l);

                switch (pInfo.mpType)
                {
                    case MPType.BOOL:
                        {
                            tl.Controls.Add(new DMOBoolParam(m_param, pCur, pInfo));
                        }
                        break;
                    case MPType.ENUM:
                        {
                            tl.Controls.Add(new DMOEnumParam(sEnum, m_param, pCur, pInfo));
                        }
                        break;
                    case MPType.FLOAT:
                        {
                            tl.Controls.Add(new DMONumericalParam(m_param, pCur, pInfo));
                        }
                        break;
                    case MPType.INT:
                        {
                            tl.Controls.Add(new DMONumericalParam(m_param, pCur, pInfo));
                        }
                        break;
                    case MPType.MAX:
                        {
                            tl.Controls.Add(new Label());
                        }
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i < tl.RowCount; i++)
            {
                tl.RowStyles.Add(new RowStyle(SizeType.Absolute, 23f));
            }
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, .5f));

            return tp;
        }

        // Break up the pointer to ParamText into usable fields
        private void ParseParamText(IntPtr ip, out string ParamName, out string ParamUnits, out string[] ParamEnum)
        {
            int iCount = 0;
            string s;

            string str = Marshal.PtrToStringAuto(ip);
            // Up to the first null is the display name
            ParamName = Marshal.PtrToStringUni(ip);
            ip = (IntPtr)(ip.ToInt32() + ((ParamName.Length + 1) * 2));

            // Next is the units
            ParamUnits = Marshal.PtrToStringUni(ip);
            ip = (IntPtr)(ip.ToInt32() + ((ParamUnits.Length + 1) * 2));

            // Following, there may b zero or more enum strings.  First we count them.
            IntPtr ip2 = ip;
            while (Marshal.ReadInt16(ip2) != 0) // Terminate on a zero length string
            {
                s = Marshal.PtrToStringUni(ip2);
                ip2 = (IntPtr)(ip2.ToInt32() + ((s.Length + 1) * 2));
                iCount++;
            }

            // Now we allocate the array, and copy the values in.
            ParamEnum = new string[iCount];
            for (int x = 0; x < iCount; x++)
            {
                ParamEnum[x] = Marshal.PtrToStringUni(ip);
                ip = (IntPtr)(ip.ToInt32() + ((ParamEnum[x].Length + 1) * 2));
            }
        }

        #endregion

        #region DVD controls

        private void dvdControlbutton_Click(object sender, EventArgs e)
        {
            // show the DVD control context menu
            dvdControlContextMenuStrip.Show(dvdControlbutton, new Point(0,CaptionSize));
        }

        private void titleMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_videoWindow != null && _videoWindow.DVDControl != null)
            {
                IDvdCmd icmd;
                int hr = _videoWindow.DVDControl.ShowMenu(DvdMenuId.Title, DvdCmdFlags.Block | DvdCmdFlags.Flush, out icmd);
            }
        }

        private void rootMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_videoWindow != null && _videoWindow.DVDControl != null)
            {
                IDvdCmd icmd;
                int hr = _videoWindow.DVDControl.ShowMenu(DvdMenuId.Root, DvdCmdFlags.Block | DvdCmdFlags.Flush, out icmd);
            }
        }

        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_videoWindow != null && _videoWindow.DVDControl != null)
            {
                IDvdCmd icmd;
                int hr = _videoWindow.DVDControl.Resume(DvdCmdFlags.Block | DvdCmdFlags.Flush, out icmd);
            }
        }

        private void dvdControlContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // build the list of Title/Chapter menu items
            if (_videoWindow != null && _videoWindow.DVDControl != null)
            {
                chaptersToolStripMenuItem.DropDown.Items.Clear();
                IDvdInfo2 dvdinfo = _videoWindow.DVDControl as IDvdInfo2;
                if (dvdinfo != null)
                {
                    int numVol;
                    DvdDiscSide side = new DvdDiscSide();
                    int vol;
                    int numTitles;

                    int hr = dvdinfo.GetDVDVolumeInfo(out numVol, out vol, out side, out numTitles);
                    if (hr == 0)
                    {
                        for (int i = 0; i < numTitles; i++ )
                        {
                            ToolStripMenuItem titlemi = new ToolStripMenuItem("Title " + (i + 1).ToString());
                            chaptersToolStripMenuItem.DropDown.Items.Add(titlemi);
                            int numChapters;
                            hr = dvdinfo.GetNumberOfChapters(i + 1, out numChapters);
                            if (hr == 0)
                            {
                                for (int x = 0; x < numChapters; x++)
                                {
                                    ToolStripMenuItem chapmi = new ToolStripMenuItem("Chapter " + (x + 1).ToString());
                                    chapmi.Tag = new DVDTitleChapterPair(i + 1, x + 1);
                                    chapmi.Click += new EventHandler(chaptermenuitem_Click);
                                    titlemi.DropDown.Items.Add(chapmi);
                                }
                            }
                        }
                    }
                }
            }
        }

        void chaptermenuitem_Click(object sender, EventArgs e)
        {
            // play the selected Title/Chapter
            if (_videoWindow != null && _videoWindow.DVDControl != null)
            {
                DVDTitleChapterPair pair = (sender as ToolStripMenuItem).Tag as DVDTitleChapterPair;
                if (pair != null)
                {
                    IDvdCmd icmd;
                    int hr = _videoWindow.DVDControl.PlayChapterInTitle(pair.Title, pair.Chapter, DvdCmdFlags.Block | DvdCmdFlags.Flush, out icmd);
                }
            }
        }

        #endregion

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (Brush br = new LinearGradientBrush(ClientRectangle,
                        IsFocused ?  CaptionColor : this.CaptionColorUnfocused,
                        BackColor,
                        LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(br, ClientRectangle);
            }

            if ((Parent as DSDaggerUIGraph).ShowPinNames && !_expandPropertiesButton.State)
            {
                // measure and draw the names of it's pins
                foreach (DSInputPin pin in Node.InputPins)
                {
                    SizeF fs = e.Graphics.MeasureString(pin.Name, Font);                    
                    Point actualLocation = (pin.PinUIElements as PinUI).PinLocation;
                    actualLocation.X = InternalControl.Left;
                    RectangleF rect = new RectangleF(new PointF(actualLocation.X, actualLocation.Y), fs);
                    DSDaggerUIGraph.DrawPinName(Font, e.Graphics, (pin.PinUIElements as PinUI).NoodleColor, pin.Name, rect);
                }

                foreach (DSOutputPin pin in Node.OutputPins)
                {
                    SizeF fs = e.Graphics.MeasureString(pin.Name, Font);
                    Point actualLocation = (pin.PinUIElements as PinUI).PinLocation;
                    actualLocation.X = InternalControl.Right - (int)fs.Width;
                    RectangleF rect = new RectangleF(new PointF(actualLocation.X, actualLocation.Y), fs);
                    DSDaggerUIGraph.DrawPinName(Font, e.Graphics, (pin.PinUIElements as PinUI).NoodleColor, pin.Name, rect);
                }
            }
        }
    }

    internal class DVDTitleChapterPair
    {
        public int Title;
        public int Chapter;

        public DVDTitleChapterPair(int title, int chapter)
        {
            Title = title;
            Chapter = chapter;
        }
    }
}
