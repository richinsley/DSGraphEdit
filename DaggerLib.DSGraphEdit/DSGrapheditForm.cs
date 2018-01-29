using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    public partial class DSGrapheditForm : Form
    {
        private DSGraphEditPanel _grapheditPanel;

        /// <summary>
        /// Contruct a DSGrapheditForm with an empty FilterGraph
        /// </summary>
        public DSGrapheditForm()
        {
            InitializeComponent();
            DSGraphEditPanel _grapheditPanel = new DSGraphEditPanel();
            _grapheditPanel.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(_grapheditPanel);
            this.Disposed += new EventHandler(DSGrapheditForm_Disposed);

            _filtersPanel.AssociatedGraphPanel = _grapheditPanel;
        }

        /// <summary>
        /// Contruct a DSGrapheditForm from an existing FilterGraph
        /// </summary>
        public DSGrapheditForm(IFilterGraph filtergraph)
        {
            InitializeComponent();
            DSGraphEditPanel _grapheditPanel = new DSGraphEditPanel(filtergraph);
            _grapheditPanel.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(_grapheditPanel);
            this.Disposed += new EventHandler(DSGrapheditForm_Disposed);

            _filtersPanel.AssociatedGraphPanel = _grapheditPanel;
        }

        void DSGrapheditForm_Disposed(object sender, EventArgs e)
        {
            // force the DSGraphEditPanel to release it's COM objects
            _grapheditPanel.Dispose();
            _grapheditPanel = null;
            GC.Collect();
        }
    }
}