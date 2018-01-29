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
    [ToolboxItem(false)]
    public partial class PropertiesDialog : Form
    {
        private TextBox _textBox;
        private PinPropertiesTextBox _pinTextBox;
        private PropertyPagePanel _properties;

        public PropertiesDialog(string caption, IBaseFilter filter)
        {
            InitializeComponent();

            // remove the default panels
            this.Controls.Remove(panel1);
            this.Controls.Remove(panel2);

            // create the PropertyPagePanel
            _properties = new PropertyPagePanel(true,filter);
            _properties.Dock = DockStyle.Fill;
            
            // make sure it's wide enough to show all the buttons
            int newwidth = Math.Max(350, _properties.PageSize.Width);
            int newheight = _properties.PageSize.Height;
            this.ClientSize = new Size(newwidth + 10, newheight + 23);

            this.Controls.Add(_properties);
            _properties.OkButton.DialogResult = DialogResult.OK;
            _properties.CloseButton.DialogResult = DialogResult.Cancel;
            this.CancelButton = _properties.CloseButton;
            this.AcceptButton = _properties.OkButton;
            Text = caption;
        }

        public PropertiesDialog(string caption,IPin pin)
        {
            InitializeComponent();
            _pinTextBox = new PinPropertiesTextBox(pin);
            panel2.Controls.Add(_pinTextBox);
            button1.DialogResult = DialogResult.OK;
            this.CancelButton = button1;
            Text = caption + " Properties";
        }

        public PropertiesDialog(string caption)
        {
            InitializeComponent();
            _textBox = new TextBox();
            _textBox.ReadOnly = true;
            _textBox.Multiline = true;
            _textBox.Dock = DockStyle.Fill;
            _textBox.ScrollBars = ScrollBars.Both;
            _textBox.WordWrap = false;
            _textBox.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(_textBox);
            button1.DialogResult = DialogResult.OK;
            this.CancelButton = button1;
            Text = caption;
        }

        public TextBox TextBox
        {
            get
            {
                return (TextBox)(_pinTextBox != null ? _pinTextBox : _textBox);
            }
        }

        public PropertyPagePanel PropertyPagePanel
        {
            get
            {
                return _properties;
            }
        }
    }
}