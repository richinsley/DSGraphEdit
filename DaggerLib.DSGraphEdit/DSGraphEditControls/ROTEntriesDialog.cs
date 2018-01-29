using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;
using DaggerLib.DSGraphEdit;

namespace DaggerLib.DSGraphEdit
{
    public partial class ROTEntriesDialog : Form
    {
        private IFilterGraph _filterGraph;

        public ROTEntriesDialog()
        {
            InitializeComponent();
            _okButton.DialogResult = DialogResult.OK;
            _cancelButton.DialogResult = DialogResult.Cancel;
            RefreshEntries();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        public string SelectedROTEntry
        {
            get
            {
                if (listBox1.SelectedIndex != -1)
                {
                    return listBox1.SelectedItem.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public IFilterGraph FilterGraph
        {
            get
            {
                if (_filterGraph != null)
                {
                    return _filterGraph;
                }
                else
                {
                    if (listBox1.SelectedIndex != -1)
                    {
                        _filterGraph = (listBox1.SelectedItem as DSGrapheditROTEntry).ConnectToROTEntry();
                        return _filterGraph;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private void PurgeEntries()
        {
            foreach (DSGrapheditROTEntry rote in listBox1.Items)
            {
                rote.Dispose();
            }
            listBox1.Items.Clear();
        }

        private void RefreshEntries()
        {
            PurgeEntries();
            listBox1.Items.AddRange(DaggerDSUtils.GetFilterGraphsFromROT().ToArray());
        }
    }
}