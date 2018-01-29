namespace DaggerLib.DSGraphEdit
{
    partial class DSGraphEditPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DSGraphEditPanel));
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._playButton = new System.Windows.Forms.ToolStripButton();
            this._pauseButton = new System.Windows.Forms.ToolStripButton();
            this._stopButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._frameStepButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._refreshGraphButton = new System.Windows.Forms.ToolStripButton();
            this._arrangeNodesButton = new System.Windows.Forms.ToolStripButton();
            this._disconnectAllPinsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._noodleStyleButton = new System.Windows.Forms.ToolStripDropDownButton();
            this._optionsDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.dropShadowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pinPlacementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPinNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modalPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.useIntelligentConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._timeSliderVisibleMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._saveGraphFileButton = new System.Windows.Forms.ToolStripButton();
            this._renderMediaFileButton = new System.Windows.Forms.ToolStripButton();
            this._renderURLButton = new System.Windows.Forms.ToolStripButton();
            this.timeSliderTimer = new System.Windows.Forms.Timer(this.components);
            this._useClockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dsDaggerUIGraph1 = new DaggerLib.DSGraphEdit.DSDaggerUIGraph();
            this._timeSliderControl = new DaggerLib.DSGraphEdit.TimeSliderControl();
            this._toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _toolStrip
            // 
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._playButton,
            this._pauseButton,
            this._stopButton,
            this.toolStripSeparator3,
            this._frameStepButton,
            this.toolStripSeparator1,
            this._refreshGraphButton,
            this._arrangeNodesButton,
            this._disconnectAllPinsButton,
            this.toolStripSeparator2,
            this._noodleStyleButton,
            this._optionsDropDownButton,
            this.toolStripSeparator5,
            this._saveGraphFileButton,
            this._renderMediaFileButton,
            this._renderURLButton});
            this._toolStrip.Location = new System.Drawing.Point(0, 0);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Size = new System.Drawing.Size(525, 25);
            this._toolStrip.TabIndex = 0;
            this._toolStrip.Text = "toolStrip1";
            // 
            // _playButton
            // 
            this._playButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._playButton.Image = ((System.Drawing.Image)(resources.GetObject("_playButton.Image")));
            this._playButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._playButton.Name = "_playButton";
            this._playButton.Size = new System.Drawing.Size(23, 22);
            this._playButton.Text = "toolStripButton1";
            this._playButton.ToolTipText = "Play";
            this._playButton.Click += new System.EventHandler(this._playButton_Click);
            // 
            // _pauseButton
            // 
            this._pauseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._pauseButton.Image = ((System.Drawing.Image)(resources.GetObject("_pauseButton.Image")));
            this._pauseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._pauseButton.Name = "_pauseButton";
            this._pauseButton.Size = new System.Drawing.Size(23, 22);
            this._pauseButton.Text = "Pause";
            this._pauseButton.Click += new System.EventHandler(this._pauseButton_Click);
            // 
            // _stopButton
            // 
            this._stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._stopButton.Enabled = false;
            this._stopButton.Image = ((System.Drawing.Image)(resources.GetObject("_stopButton.Image")));
            this._stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._stopButton.Name = "_stopButton";
            this._stopButton.Size = new System.Drawing.Size(23, 22);
            this._stopButton.Text = "Stop";
            this._stopButton.Click += new System.EventHandler(this._stopButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // _frameStepButton
            // 
            this._frameStepButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._frameStepButton.Enabled = false;
            this._frameStepButton.Image = ((System.Drawing.Image)(resources.GetObject("_frameStepButton.Image")));
            this._frameStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._frameStepButton.Name = "_frameStepButton";
            this._frameStepButton.Size = new System.Drawing.Size(23, 22);
            this._frameStepButton.Text = "toolStripButton1";
            this._frameStepButton.ToolTipText = "Step One Frame";
            this._frameStepButton.Click += new System.EventHandler(this._frameStepButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _refreshGraphButton
            // 
            this._refreshGraphButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._refreshGraphButton.Image = ((System.Drawing.Image)(resources.GetObject("_refreshGraphButton.Image")));
            this._refreshGraphButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._refreshGraphButton.Name = "_refreshGraphButton";
            this._refreshGraphButton.Size = new System.Drawing.Size(23, 22);
            this._refreshGraphButton.Text = "Refresh Graph";
            this._refreshGraphButton.Click += new System.EventHandler(this._refreshGraphButton_Click);
            // 
            // _arrangeNodesButton
            // 
            this._arrangeNodesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._arrangeNodesButton.Image = ((System.Drawing.Image)(resources.GetObject("_arrangeNodesButton.Image")));
            this._arrangeNodesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._arrangeNodesButton.Name = "_arrangeNodesButton";
            this._arrangeNodesButton.Size = new System.Drawing.Size(23, 22);
            this._arrangeNodesButton.Text = "toolStripButton1";
            this._arrangeNodesButton.ToolTipText = "Auto-Arrange Nodes";
            this._arrangeNodesButton.Click += new System.EventHandler(this._arrangeNodesButton_Click);
            // 
            // _disconnectAllPinsButton
            // 
            this._disconnectAllPinsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._disconnectAllPinsButton.Image = ((System.Drawing.Image)(resources.GetObject("_disconnectAllPinsButton.Image")));
            this._disconnectAllPinsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._disconnectAllPinsButton.Name = "_disconnectAllPinsButton";
            this._disconnectAllPinsButton.Size = new System.Drawing.Size(23, 22);
            this._disconnectAllPinsButton.Text = "Disconnect All Pins";
            this._disconnectAllPinsButton.Click += new System.EventHandler(this._disconnectAllPinsButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // _noodleStyleButton
            // 
            this._noodleStyleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._noodleStyleButton.Image = ((System.Drawing.Image)(resources.GetObject("_noodleStyleButton.Image")));
            this._noodleStyleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._noodleStyleButton.Name = "_noodleStyleButton";
            this._noodleStyleButton.Size = new System.Drawing.Size(29, 22);
            this._noodleStyleButton.Text = "Noodle Style";
            this._noodleStyleButton.DropDownOpening += new System.EventHandler(this._noodleStyleButton_DropDownOpening);
            // 
            // _optionsDropDownButton
            // 
            this._optionsDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._optionsDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropShadowToolStripMenuItem,
            this.pinPlacementToolStripMenuItem,
            this.showPinNamesToolStripMenuItem,
            this._timeSliderVisibleMenuItem,
            this.modalPropertiesToolStripMenuItem,
            this.toolStripSeparator4,
            this.useIntelligentConnectToolStripMenuItem,
            this._useClockToolStripMenuItem});
            this._optionsDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("_optionsDropDownButton.Image")));
            this._optionsDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._optionsDropDownButton.Name = "_optionsDropDownButton";
            this._optionsDropDownButton.Size = new System.Drawing.Size(29, 22);
            this._optionsDropDownButton.Text = "Graph Options";
            this._optionsDropDownButton.DropDownOpening += new System.EventHandler(this._optionsDropDownButton_DropDownOpening);
            // 
            // dropShadowToolStripMenuItem
            // 
            this.dropShadowToolStripMenuItem.CheckOnClick = true;
            this.dropShadowToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.dropShadowToolStripMenuItem.Name = "dropShadowToolStripMenuItem";
            this.dropShadowToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.dropShadowToolStripMenuItem.Text = "Drop Shadow";
            this.dropShadowToolStripMenuItem.Click += new System.EventHandler(this.dropShadowToolStripMenuItem_Click);
            // 
            // pinPlacementToolStripMenuItem
            // 
            this.pinPlacementToolStripMenuItem.Name = "pinPlacementToolStripMenuItem";
            this.pinPlacementToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.pinPlacementToolStripMenuItem.Text = "Pin Placement";
            // 
            // showPinNamesToolStripMenuItem
            // 
            this.showPinNamesToolStripMenuItem.CheckOnClick = true;
            this.showPinNamesToolStripMenuItem.Name = "showPinNamesToolStripMenuItem";
            this.showPinNamesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.showPinNamesToolStripMenuItem.Text = "Show Pin Names";
            this.showPinNamesToolStripMenuItem.Click += new System.EventHandler(this.showPinNamesToolStripMenuItem_Click);
            // 
            // modalPropertiesToolStripMenuItem
            // 
            this.modalPropertiesToolStripMenuItem.CheckOnClick = true;
            this.modalPropertiesToolStripMenuItem.Name = "modalPropertiesToolStripMenuItem";
            this.modalPropertiesToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+M";
            this.modalPropertiesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.modalPropertiesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.modalPropertiesToolStripMenuItem.Text = "Modal Properties";
            this.modalPropertiesToolStripMenuItem.Click += new System.EventHandler(this.modalPropertiesToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(198, 6);
            // 
            // useIntelligentConnectToolStripMenuItem
            // 
            this.useIntelligentConnectToolStripMenuItem.CheckOnClick = true;
            this.useIntelligentConnectToolStripMenuItem.Name = "useIntelligentConnectToolStripMenuItem";
            this.useIntelligentConnectToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+I";
            this.useIntelligentConnectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.useIntelligentConnectToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.useIntelligentConnectToolStripMenuItem.Text = "Connect Intelligent";
            this.useIntelligentConnectToolStripMenuItem.Click += new System.EventHandler(this.useIntelligentConnectToolStripMenuItem_Click);
            // 
            // _timeSliderVisibleMenuItem
            // 
            this._timeSliderVisibleMenuItem.Name = "_timeSliderVisibleMenuItem";
            this._timeSliderVisibleMenuItem.ShortcutKeyDisplayString = "Ctrl+T";
            this._timeSliderVisibleMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this._timeSliderVisibleMenuItem.Size = new System.Drawing.Size(201, 22);
            this._timeSliderVisibleMenuItem.Text = "Hide Time Slider";
            this._timeSliderVisibleMenuItem.Click += new System.EventHandler(this._timeSliderVisibleMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // _saveGraphFileButton
            // 
            this._saveGraphFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveGraphFileButton.Image = ((System.Drawing.Image)(resources.GetObject("_saveGraphFileButton.Image")));
            this._saveGraphFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveGraphFileButton.Name = "_saveGraphFileButton";
            this._saveGraphFileButton.Size = new System.Drawing.Size(23, 22);
            this._saveGraphFileButton.Text = "toolStripButton1";
            this._saveGraphFileButton.ToolTipText = "Save Grf file";
            this._saveGraphFileButton.Click += new System.EventHandler(this._saveGraphFileButton_Click);
            // 
            // _renderMediaFileButton
            // 
            this._renderMediaFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._renderMediaFileButton.Image = ((System.Drawing.Image)(resources.GetObject("_renderMediaFileButton.Image")));
            this._renderMediaFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._renderMediaFileButton.Name = "_renderMediaFileButton";
            this._renderMediaFileButton.Size = new System.Drawing.Size(23, 22);
            this._renderMediaFileButton.Text = "toolStripButton1";
            this._renderMediaFileButton.ToolTipText = "Render Media File";
            this._renderMediaFileButton.Click += new System.EventHandler(this._renderMediaFileButton_Click);
            // 
            // _renderURLButton
            // 
            this._renderURLButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._renderURLButton.Image = ((System.Drawing.Image)(resources.GetObject("_renderURLButton.Image")));
            this._renderURLButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._renderURLButton.Name = "_renderURLButton";
            this._renderURLButton.Size = new System.Drawing.Size(23, 22);
            this._renderURLButton.Text = "toolStripButton1";
            this._renderURLButton.ToolTipText = "Render URL";
            this._renderURLButton.Click += new System.EventHandler(this._renderURLButton_Click);
            // 
            // timeSliderTimer
            // 
            this.timeSliderTimer.Interval = 33;
            this.timeSliderTimer.Tick += new System.EventHandler(this.timeSliderTimer_Tick);
            // 
            // _useClockToolStripMenuItem
            // 
            this._useClockToolStripMenuItem.CheckOnClick = true;
            this._useClockToolStripMenuItem.Name = "_useClockToolStripMenuItem";
            this._useClockToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+L";
            this._useClockToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this._useClockToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this._useClockToolStripMenuItem.Text = "Use Clock";
            this._useClockToolStripMenuItem.Click += new System.EventHandler(this._useClockToolStripMenuItem_Click);
            // 
            // dsDaggerUIGraph1
            // 
            this.dsDaggerUIGraph1.AllowAttachConstantEditor = false;
            this.dsDaggerUIGraph1.AllowDeleteRelink = true;
            this.dsDaggerUIGraph1.AllowDrop = true;
            this.dsDaggerUIGraph1.AllowNodeProcessing = false;
            this.dsDaggerUIGraph1.AllowNoodleBisecting = false;
            this.dsDaggerUIGraph1.AllowPinExport = false;
            this.dsDaggerUIGraph1.AllowPinSetValue = false;
            this.dsDaggerUIGraph1.AllowSubNodes = false;
            this.dsDaggerUIGraph1.AutoArrange = DaggerLib.UI.Windows.AutoArrangeStyle.None;
            this.dsDaggerUIGraph1.AutoArrangeHeightOffset = 25;
            this.dsDaggerUIGraph1.AutoArrangeWidthOffset = 45;
            this.dsDaggerUIGraph1.AutoScroll = true;
            this.dsDaggerUIGraph1.AutoScrollMinSize = new System.Drawing.Size(525, 292);
            this.dsDaggerUIGraph1.BackColor = System.Drawing.Color.Gray;
            this.dsDaggerUIGraph1.CanvasSize = new System.Drawing.Size(525, 292);
            this.dsDaggerUIGraph1.DaggerNodeTreeView = null;
            this.dsDaggerUIGraph1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dsDaggerUIGraph1.DropShadowAlpha = 0.33F;
            this.dsDaggerUIGraph1.DropShadowVisible = false;
            this.dsDaggerUIGraph1.DropShadowXOffset = 10;
            this.dsDaggerUIGraph1.DropShadowYOffset = 13;
            this.dsDaggerUIGraph1.Location = new System.Drawing.Point(0, 62);
            this.dsDaggerUIGraph1.ModalProperties = false;
            this.dsDaggerUIGraph1.Name = "dsDaggerUIGraph1";
            this.dsDaggerUIGraph1.NoodleStyle = DaggerLib.UI.Windows.NoodleStyle.Default;
            this.dsDaggerUIGraph1.PinSize = 11;
            this.dsDaggerUIGraph1.ShowOrdinal = true;
            this.dsDaggerUIGraph1.ShowPinNames = false;
            this.dsDaggerUIGraph1.Size = new System.Drawing.Size(525, 292);
            this.dsDaggerUIGraph1.TabIndex = 2;
            this.dsDaggerUIGraph1.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.dsDaggerUIGraph1_ControlAdded);
            // 
            // _timeSliderControl
            // 
            this._timeSliderControl.BackColor = System.Drawing.Color.Silver;
            this._timeSliderControl.Color2 = System.Drawing.Color.DarkGray;
            this._timeSliderControl.Dock = System.Windows.Forms.DockStyle.Top;
            this._timeSliderControl.Location = new System.Drawing.Point(0, 25);
            this._timeSliderControl.Name = "_timeSliderControl";
            this._timeSliderControl.Pos = 0;
            this._timeSliderControl.Size = new System.Drawing.Size(525, 37);
            this._timeSliderControl.TabIndex = 1;
            this._timeSliderControl.Type = DaggerLib.DSGraphEdit.ColorSliderType.Threshold;
            this._timeSliderControl.ValuesChanged += new System.EventHandler(this._timeSliderControl_ValuesChanged);
            // 
            // DSGraphEditPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dsDaggerUIGraph1);
            this.Controls.Add(this._timeSliderControl);
            this.Controls.Add(this._toolStrip);
            this.Name = "DSGraphEditPanel";
            this.Size = new System.Drawing.Size(525, 354);
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip _toolStrip;
        private System.Windows.Forms.ToolStripButton _playButton;
        private System.Windows.Forms.ToolStripButton _pauseButton;
        private System.Windows.Forms.ToolStripButton _stopButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _refreshGraphButton;
        private TimeSliderControl _timeSliderControl;
        private System.Windows.Forms.Timer timeSliderTimer;
        private System.Windows.Forms.ToolStripButton _arrangeNodesButton;
        private System.Windows.Forms.ToolStripDropDownButton _noodleStyleButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton _disconnectAllPinsButton;
        private System.Windows.Forms.ToolStripDropDownButton _optionsDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem dropShadowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pinPlacementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPinNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modalPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton _frameStepButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem useIntelligentConnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton _saveGraphFileButton;
        private System.Windows.Forms.ToolStripButton _renderMediaFileButton;
        private System.Windows.Forms.ToolStripButton _renderURLButton;
        private System.Windows.Forms.ToolStripMenuItem _timeSliderVisibleMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _useClockToolStripMenuItem;
        internal DSDaggerUIGraph dsDaggerUIGraph1;

    }
}
