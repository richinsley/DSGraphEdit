using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DaggerLib.UI.Windows;
using DaggerLib.DSGraphEdit;
using WeifenLuo.WinFormsUI.Docking;

namespace DSGraphEdit
{
    public partial class GraphNavigatorForm : DockContent
    {
        private UIGraphNavigator _navigator;

        public GraphNavigatorForm()
        {
            InitializeComponent();
            _navigator = new UIGraphNavigator();
            _navigator.Dock = DockStyle.Fill;
            this.Controls.Add(_navigator);
            HideOnClose = true;
        }

        public DaggerUIGraph AssociatedUIGraph
        {
            get
            {
                return _navigator.AssociatedUIGraph;
            }
            set
            {
                if (_navigator.AssociatedUIGraph != value)
                {
                    _navigator.AssociatedUIGraph = value;
                    _navigator.Invalidate();
                }
            }
        }
    }
}