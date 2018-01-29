using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    public partial class ValueEditorDialog : Form
    {
        public ValueEditorDialog(DaggerBasePin pin)
        {
            InitializeComponent();
            Text = pin.Name;
            genericValueEditor1.EditedType = pin.DataType;
            genericValueEditor1.Value = pin.Data;
            genericValueEditor1.ValueChanged += new EventHandler(genericValueEditor1_ValueChanged);
        }

        void genericValueEditor1_ValueChanged(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public ValueEditorDialog(String caption, string value)
        {
            InitializeComponent();
            Text = caption;
            genericValueEditor1.EditedType = typeof(string);
            genericValueEditor1.Value = value;
        }

        public object Data
        {
            get
            {
                return genericValueEditor1.Value;
            }
        }

        private void ValueEditorDialog_Load(object sender, EventArgs e)
        {
            okButton.DialogResult = DialogResult.OK;
            cancelButton.DialogResult = DialogResult.Cancel;
        }
    }
}