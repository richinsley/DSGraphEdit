using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DaggerLib.DSGraphEdit
{
    public partial class PinsComboBoxForm : Form
    {
        public PinsComboBoxForm()
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
        }

        public int Value
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
        }
    }
}