using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;
using DirectShowLib.Dvd;
using MediaFoundation.EVR;
using MediaFoundation.Misc;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public partial class VideoInternalWindow : UserControl
    {
        private IVideoWindow _vw;
        private bool _isInit;
        private IMFVideoDisplayControl _evr;
        private DetachedVideoWindow _detachedWindow;
        private bool _isFullScreen = false;
        private string _filterName;
        private IDvdControl2 _dvdControl;
        private bool _handleLost = false;

        // when loading a graph, the EVR can't be initialized to a window until the VideoInternalWindow's handle
        // is fully created
        private bool _delayedInit;

        public VideoInternalWindow(string caption, IBaseFilter filter)
        {
            InitializeComponent();            
            _vw = filter as IVideoWindow;
            _isInit = false;
            _filterName = caption;

            this.Resize += new EventHandler(VideoInternalWindow_Resize);
            this.Paint += new PaintEventHandler(VideoInternalWindow_Paint);
            this.MouseMove += new MouseEventHandler(VideoInternalWindow_MouseMove);
            this.MouseDown += new MouseEventHandler(VideoInternalWindow_MouseDown);
        }

        public VideoInternalWindow(string caption, IMFVideoDisplayControl evr)
        {
            InitializeComponent();
            _evr = evr;
            _isInit = false;
            _filterName = caption;

            this.Resize += new EventHandler(VideoInternalWindow_Resize);
            this.Paint += new PaintEventHandler(VideoInternalWindow_Paint);
            this.MouseMove += new MouseEventHandler(VideoInternalWindow_MouseMove);
            this.MouseDown += new MouseEventHandler(VideoInternalWindow_MouseDown);
        }

        /// <summary>
        /// Gets or sets the IDvdControl2 associated with this Video Window
        /// </summary>
        public IDvdControl2 DVDControl
        {
            get
            {
                return _dvdControl;
            }
            set
            {
                _dvdControl = value;

                // show/hide the dvd control button
                (this.Parent.Parent as DSFilterNodeUI).dvdControlbutton.Visible = (value == null) ? false : true;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_delayedInit)
            {
                // we had to delay the init of the EVR's Video Window until it was made visible
                _delayedInit = false;
                _isInit = false;
                InitVideoWindow();
            }
        }
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

            if (_vw != null)
            {
                int hr = _vw.put_Owner(IntPtr.Zero);
                _vw = null;
            }

            if (_evr != null)
            {
                // not needed as EVR does not accept a non-valid (even null) handle
                /*
                try
                {
                    _evr.SetVideoWindow(IntPtr.Zero);
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.Message, "Failed to reset EVR Window");
#endif
                }
                finally
                {
                    _evr = null;
                }
                */
            }

            if (_detachedWindow != null)
            {
                _detachedWindow.Close();
            }

            base.Dispose(disposing);
        }

        public void InitVideoWindow()
        {
            InitVideoWindow(this.Handle);
        }

        private void InitVideoWindow(IntPtr handle)
        {
            int hr = 0;
            if (!_isInit)
            {
                _isInit = true;

                if (_vw != null)
                {
                    hr = _vw.put_Owner(handle);
                    if (hr != 0)
                    {
                        return;
                    }

                    hr = _vw.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
                    _vw.put_MessageDrain(handle);
                    if (hr != 0)
                    {
                        return;
                    }
                }
                else if (_evr != null)
                {
                    if (!_delayedInit)
                    {
                        try
                        {
                            _evr.SetVideoWindow(handle);
                        }
                        catch (Exception ex)
                        {                            
                            _isInit = false;
                            _delayedInit = true;
                        }
                    }
                }                
            }
            OnResize(new EventArgs());
        }

        public Form DetachVideoWindow(bool fullscreen)
        {
            if (!_isInit) return null;

            //if we're already deteched, close the form
            if (_detachedWindow != null)
            {
                _detachedWindow.Close();
                return null;
            }

            _detachedWindow = new DetachedVideoWindow(this);
            _detachedWindow.Size = new Size(320, 240);
            _detachedWindow.BackColor = Color.Black;
            _detachedWindow.Text = _filterName;
            _detachedWindow.FormClosing += new FormClosingEventHandler(_detachedWindow_FormClosing);
            _detachedWindow.Resize += new EventHandler(VideoInternalWindow_Resize);
            _detachedWindow.Paint += new PaintEventHandler(VideoInternalWindow_Paint);
            _detachedWindow.MouseMove += new MouseEventHandler(VideoInternalWindow_MouseMove);
            _detachedWindow.MouseDown += new MouseEventHandler(VideoInternalWindow_MouseDown);
            _detachedWindow.Icon = (this.TopLevelControl as Form).Icon;

            // if we're going full screen, we're going to fake the funk with a fullscreen window
            if (fullscreen)
            {
                _detachedWindow.FullScreen = true;
            }
            _detachedWindow.Show(this.TopLevelControl);
            
            // reset the video initialization
            _isInit = false;
            InitVideoWindow(_detachedWindow.Handle);

            // if it's a vmr7 or vmr9, sned it's message drain through the detached window
            if (_vw != null)
            {
                int hr = _vw.put_MessageDrain(_detachedWindow.Handle);
            }

            return _detachedWindow;
        }

        #region Events

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 2) // Catch WM_Destroy to reassign the VideoWindow owner
            {
                if (_detachedWindow == null && _vw != null)
                {
                    // The EVR easily handles this, but VMR7 and VMR9 parent windows have to be reset
                    // when they lose the Handle.
                    _vw.put_Visible(OABool.False);
                    _vw.put_Owner(IntPtr.Zero);                    
                }
                _handleLost = true;
            }

            base.WndProc(ref m);

            if (m.Msg == 1)
            {
                if (_handleLost)
                {
                    if (_detachedWindow == null)
                    {
                        // the window handle was lost (probably due to docking).  Reinit with the new handle                
                        _isInit = false;
                        InitVideoWindow(this.Handle);
                        if (_vw != null)
                        {
                            _vw.put_Visible(OABool.True);
                        }
                    }
                    _handleLost = false;
                }
            }

            if (m.Msg == 0x203) //WM_LBUTTONDBLCLK
            {
                this.VideoInternalWindow_DoubleClick(this, new EventArgs());
            }
        }

        void VideoInternalWindow_Paint(object sender, PaintEventArgs e)
        {
            if (_evr != null && _isInit)
            {
                try
                {
                    _evr.RepaintVideo();
                }
                catch
                {
                    // evr is not connected
                }
            }
        }

        void VideoInternalWindow_Resize(object sender, EventArgs e)
        {
            int top = 0;
            int left = 0;
            int width = this.Width;
            int height = this.Height;

            if (_detachedWindow != null)
            {
                top = _detachedWindow.ClientRectangle.Top;
                left = _detachedWindow.ClientRectangle.Left;
                width = _detachedWindow.ClientRectangle.Width;
                height = _detachedWindow.ClientRectangle.Height;
            }

            if (_isInit)
            {
                if (_vw != null)
                {
                    _vw.SetWindowPosition(top, left, width, height);
                }
                else if (_evr != null & !_delayedInit)
                {
                    try
                    {
                        MFRect rcDest = new MFRect();
                        MFVideoNormalizedRect nRect = new MFVideoNormalizedRect();

                        nRect.left = 0;
                        nRect.right = 1;
                        nRect.top = 0;
                        nRect.bottom = 1;
                        rcDest.left = top;
                        rcDest.top = left;
                        rcDest.right = width;
                        rcDest.bottom = height;

                        _evr.SetVideoPosition(nRect, rcDest);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        MessageBox.Show(ex.Message, "Failed to set EVR Window Position");
#endif
                    }
                }
            }
        }

        void _detachedWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_evr != null || _vw != null)
            {
                // put the video window back to this control
                _isInit = false;
                InitVideoWindow(this.Handle);
            }

            _detachedWindow.Dispose();
            _detachedWindow = null;

            // force resize to repaint the frame of video in the Node
            VideoInternalWindow_Resize(null, null);
        }

        void VideoInternalWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (_dvdControl == null || (sender == this && _detachedWindow != null))
            {
                // ignore the internal window if there is a detached window
                return;
            }
            _dvdControl.ActivateAtPosition(e.Location);
        }

        void VideoInternalWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dvdControl == null || (sender == this && _detachedWindow != null))
            {
                // ignore the internal window if there is a detached window
                return;
            }
            _dvdControl.SelectAtPosition(e.Location);
        }

        public void ToggleFullScreen()
        {
            if (_isFullScreen)
            {
                _detachedWindow.FullScreen = false;
                _isFullScreen = false;
            }
            else
            {
                if (_detachedWindow != null)
                {
                    _detachedWindow.FullScreen = true;
                    _isFullScreen = true;
                }
                else
                {
                    DetachVideoWindow(true);
                    _isFullScreen = true;
                }
            }
        }

        void VideoInternalWindow_DoubleClick(object sender, EventArgs e)
        {
            ToggleFullScreen();
        }

        #endregion
    }

    internal class DetachedVideoWindow : Form
    {
        private VideoInternalWindow _parent;
        private bool _isFullScreen = false;

        public DetachedVideoWindow(VideoInternalWindow parent)
        {
            _parent = parent;
        }

        public bool FullScreen
        {
            get
            {
                return _isFullScreen;
            }
            set
            {
                if (value)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    this.TopMost = true;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.TopMost = false;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                }
                _isFullScreen = value;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == (char)0x1b && _isFullScreen)
            {
                _parent.ToggleFullScreen();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x203) //WM_LBUTTONDBLCLK
            {
                _parent.ToggleFullScreen();
            }
        }
    }
}
