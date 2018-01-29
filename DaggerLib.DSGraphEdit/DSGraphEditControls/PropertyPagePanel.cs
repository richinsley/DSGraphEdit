using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
using MediaFoundation;
using MediaFoundation.EVR;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public class PropertyPagePanel : Panel, IPropertyPageSite
    {
        #region Fields

        Panel _buttonsPanel;
        Button _applyButton;
        Button _interfacesButton;

        Button _okButton;
        Button _closeButton;

        TabControl _tabs;
        private Size _pageSize = new Size(200, 120);
        IBaseFilter _filter;

        List<IPropertyPage> _pages = new List<IPropertyPage>();
        List<IntPtr> _iunk = new List<IntPtr>();

        #endregion

        #region ctor

        public PropertyPagePanel(bool OkCloseButtons,IBaseFilter filter)
        {
            _filter = filter;

            base.BackColor = SystemColors.Control;

            _tabs = new TabControl();
            _tabs.Dock = DockStyle.Fill;
            this.Dock = DockStyle.Fill;
            _tabs.Multiline = false;
            Controls.Add(_tabs);

            _buttonsPanel = new Panel();
            _buttonsPanel.Dock = DockStyle.Bottom;

            _interfacesButton = new Button();
            _interfacesButton.Text = "Scan Interfaces";
            _interfacesButton.Width = 100;
            _interfacesButton.Dock = DockStyle.Left;
            _interfacesButton.BackColor = SystemColors.Control;
            _interfacesButton.Click += new EventHandler(_interfacesButton_Click);
            _buttonsPanel.Controls.Add(_interfacesButton);

            _applyButton = new Button();
            _applyButton.Text = "Apply";
            _applyButton.Dock = DockStyle.Left;
            _applyButton.BackColor = SystemColors.Control;
            _applyButton.Enabled = false;
            _applyButton.Click += new EventHandler(_applyButton_Click);
            _buttonsPanel.Controls.Add(_applyButton);

            if (OkCloseButtons)
            {
                _closeButton = new Button();
                _closeButton.Text = "Close";
                _closeButton.Dock = DockStyle.Left;
                _closeButton.BackColor = SystemColors.Control;
                _buttonsPanel.Controls.Add(_closeButton);

                _okButton = new Button();
                _okButton.Text = "Ok";
                _okButton.Dock = DockStyle.Left;
                _okButton.BackColor = SystemColors.Control;
                _okButton.Click += new EventHandler(_applyButton_Click);
                _buttonsPanel.Controls.Add(_okButton);
            }

            _buttonsPanel.Height = 23;
            Controls.Add(_buttonsPanel);
            int hr = 0;

            ISpecifyPropertyPages pProp = filter as ISpecifyPropertyPages;
            if (pProp != null)
            {
                // Get the propertypages from the property bag
                DsCAUUID caGUID;
                hr = pProp.GetPages(out caGUID);
                if (hr != 0 || caGUID.cElems == 0)
                {
                    // could not get property sheets or got 0 property sheets
                    return;
                }

                // convert caGUID to a managed array of Guids
                Guid[] propertyPages = caGUID.ToGuidArray();

                for (int i = 0; i < propertyPages.Length; i++)
                {
                    try
                    {
                        Type type = Type.GetTypeFromCLSID(propertyPages[i]);
                        object o = Activator.CreateInstance(type);

                        IPropertyPage pp = o as IPropertyPage;
                        PROPPAGEINFO pi = new PROPPAGEINFO();
                        pp.GetPageInfo(ref pi);

                        // get the page size, adjusting for button panel and padding
                        _pageSize.Width = Math.Max(_pageSize.Width, pi.size.Width + 10);
                        _pageSize.Height = Math.Max(_pageSize.Height, pi.size.Height + 33);

                        // we want to inc the refcount so the property page won't vanish on us
                        _iunk.Add(Marshal.GetIUnknownForObject(o));

                        object[] obs = { filter };
                        pp.SetObjects(1, obs);
                        pp.SetPageSite(this);
                        Rectangle rect = new Rectangle(0, 0, pi.size.Width, pi.size.Height);

                        TabPage tp = new TabPage(Marshal.PtrToStringAuto(pi.szTitle));
                        _tabs.Controls.Add(tp);                        
                        _pages.Add(pp);
                        pp.Activate(tp.Handle, ref rect, false);

                        // some PropertyPages aren't visible by default
                        IntPtr childwindow = GetWindow(tp.Handle, 5);
                        if (childwindow != IntPtr.Zero)
                        {
                            ShowWindow(childwindow, 5);
                        }
                    }
                    catch
                    {
                        // some property pages don't abide by the rules of COM
                    }
                }
            }
        }

        #endregion

        #region Properties

        public Button OkButton
        {
            get
            {
                return _okButton;
            }
        }

        public Button CloseButton
        {
            get
            {
                return _closeButton;
            }
        }

        public Size PageSize
        {
            get
            {
                return _pageSize;
            }
        }

        public TabControl TabControl
        {
            get
            {
                return _tabs;
            }
        }

        #endregion

        #region Private Methods

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Scans the interfaces for the filter and it's pins and displays them in a Dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _interfacesButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            PropertiesDialog pd = null;

            // are we in modal or non-modal mode
            if (this.Parent is PropertiesDialog)
            {
                pd = new PropertiesDialog((this.Parent as PropertiesDialog).Text + " Interfaces");
            }
            else
            {
                pd = new PropertiesDialog((this.Parent.Parent as DSFilterNodeUI).CaptionText + " Interfaces");
            }

            pd.TextBox.AppendText("Filter Interfaces:\r\n");

            // create the array of assemblie we want to scan interfaces against
            Assembly[] assemblies = new Assembly[2] { typeof(IBaseFilter).Assembly, typeof(IMFGetService).Assembly };

            // scan the filter's interfaces
            List<InterfacePair> pairs = InterfaceScanner.Scan(assemblies, _filter);
            foreach (InterfacePair p in pairs)
            {
                pd.TextBox.AppendText(p.InterfaceGuid.ToString() + "\t" + p.InterfaceName + "\r\n");
            }

            // scan the pin interfaces
            foreach (Control c in this.TabControl.Controls)
            {
                if (c.Tag is IPin)
                {
                    pd.TextBox.AppendText("\r\n" + c.Text + " Pin Interfaces:\r\n");

                    List<InterfacePair> pinpairs = InterfaceScanner.Scan(assemblies, c.Tag);
                    foreach (InterfacePair p in pinpairs)
                    {
                        pd.TextBox.AppendText(p.InterfaceGuid.ToString() + "\t" + p.InterfaceName + "\r\n");
                    }
                }
            }
            Cursor = Cursors.Default;
            pd.ShowDialog(this.TopLevelControl);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CloseInterfaces();
        }

        void _applyButton_Click(object sender, EventArgs e)
        {
            foreach (IPropertyPage pp in _pages)
            {
                pp.Apply();
            }
            _applyButton.Enabled = false;
        }

        #endregion

        #region Public Methods

        public void Apply()
        {
            foreach (IPropertyPage pp in _pages)
            {
                if (pp.IsPageDirty() != 0)
                {
                    pp.Apply();
                }
            }
        }

        public void CloseInterfaces()
        {
            if (_pages != null)
            {
                foreach (IPropertyPage pp in _pages)
                {
                    try
                    {
                        pp.Deactivate();
                        pp.SetObjects(0, null);
                        pp.SetPageSite(null);
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        int refc = Marshal.ReleaseComObject(pp);
                    }
                }

                foreach (IntPtr ptr in _iunk)
                {
                    // release our references to the property pages
                    int refc = Marshal.Release(ptr);
                }
                _iunk.Clear();
                _pages = null;
            }
        }

        #endregion

        #region IPropertyPageSite Implementation

        public void OnStatusChange(UInt32 dwFlags)
        {
            _applyButton.Enabled = true;
        }

        public void GetLocaleID(out UInt32 LocaleID)
        {
            LocaleID = 0;
        }

        public void GetPageContainer([MarshalAs(UnmanagedType.IUnknown)] out Object objs)
        {
            //this doesn't even do anything in windows yet
            objs = null;
        }

        [PreserveSig]
        public UInt32 TranslateAccelerator(ref Message msg)
        {
            return 0;
        }

        #endregion
    }

    #region IPropertyPage

    // IPropertyPage
    [ComImport]
    [Guid("B196B28D-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyPage
    {
        void SetPageSite(IPropertyPageSite site);
        void Activate(IntPtr wndParent, ref Rectangle rect, bool modal);
        void Deactivate();
        void GetPageInfo(ref PROPPAGEINFO info);
        void SetObjects(UInt32 count, [MarshalAs(UnmanagedType.LPArray,
          ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] Object[] objs);
        void Show(UInt32 cmdShow);
        void Move(ref Rectangle rect);
        [PreserveSig]
        UInt32 IsPageDirty();
        void Apply();
        void Help([MarshalAs(UnmanagedType.LPWStr)] ref String helpDir);
        [PreserveSig]
        UInt32 TranslateAccelerator(ref Message msg);
    };

    // IPropertyPageSite
    [ComImport]
    [Guid("B196B28C-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyPageSite
    {
        void OnStatusChange(UInt32 dwFlags);
        void GetLocaleID(out UInt32 LocaleID);
        void GetPageContainer([MarshalAs(UnmanagedType.IUnknown)] out Object objs);
        [PreserveSig]
        UInt32 TranslateAccelerator(ref Message msg);
    };


    [ComVisible(false)]
    public struct PROPPAGEINFO
    {
        public UInt32 cb;
        public IntPtr szTitle;
        public Size size;
        public IntPtr szDocString;
        public IntPtr szHelpFile;
        public UInt32 dwHelpContext;
    };

    #endregion 
}
