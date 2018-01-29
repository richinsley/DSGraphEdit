using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using DaggerLib.DSGraphEdit;

namespace DSGraphEdit
{
    public partial class FiltersForm : DockContent
    {
        public FiltersForm()
        {
            InitializeComponent();
            HideOnClose = true;
        }

        public DSGraphEditPanel AssociatedGraphPanel
        {
            get
            {
                return _filtersPanel.AssociatedGraphPanel;
            }
            set
            {
                _filtersPanel.AssociatedGraphPanel = value;
            }
        }
    }
}