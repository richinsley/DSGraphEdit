using System;
using System.IO;
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
    public partial class GraphForm : DockContent
    {
        private DSGraphEditPanel _panel;
        private string _path = "Untitled.grf";

        public GraphForm()
        {
            InitializeComponent();
            _panel = new DSGraphEditPanel();
            _panel.Dock = DockStyle.Fill;
            Controls.Add(_panel);
            Text = Path.GetFileName(_path);
        }

        public GraphForm(string path)
        {
            InitializeComponent();
            _panel = new DSGraphEditPanel(path);
            _panel.Dock = DockStyle.Fill;
            _path = path;
            Text = Path.GetFileName(_path);
            Controls.Add(_panel);
        }

        public GraphForm(DSGraphEditPanel panel)
        {
            InitializeComponent();
            _panel = panel;
            _panel.Dock = DockStyle.Fill;

            if (panel.IsRemoteGraph)
            {
                Text = panel.ROTEntryString;
            }
            else
            {
                Text = Path.GetFileName(_path);
            }

            Controls.Add(_panel);
        }

        public void SaveGraph(string path)
        {
            _panel.SaveFilterGraph(path);
            _path = path;
            Text = Path.GetFileName(_path);
        }

        public DSGraphEditPanel DSGraphEditPanel
        {
            get
            {
                return _panel;
            }
        }

        public string FilePath
        {
            get
            {
                return _path;
            }
        }
    }
}