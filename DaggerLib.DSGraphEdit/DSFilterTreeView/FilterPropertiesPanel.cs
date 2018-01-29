using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public partial class FilterPropertiesPanel : UserControl
    {
        public FilterPropertiesPanel(DSFilterTreeViewNode node)
        {
            InitializeComponent();
            _nameLabel.Text = node.Text;
            _meritLabel.Text = "0x" + node.FilterInformation.Merit.ToString("X");
            _monikerTextBox.Text = node.DevicePath;
            _filenameLabel.Text = node.FilePath;
            _filenameLabel.SelectAll();

            foreach (FilterDataPin pin in node.FilterInformation.Pins)
            {
                TreeNode tn = new TreeNode("pin " + pin.PinNumber.ToString() + ":");
                tn.Nodes.Add(new TreeNode("Many: " + pin.PinFlagMany.ToString()));
                tn.Nodes.Add(new TreeNode("Output: " + pin.PinFlagOutput.ToString()));
                tn.Nodes.Add(new TreeNode("Rendered: " + pin.PinFlagRenderer.ToString()));
                tn.Nodes.Add(new TreeNode("Zero: " + pin.PinFlagZero.ToString()));
                tn.Nodes.Add(new TreeNode("ClsPinCategory: " + pin.Category.ToString()));

                foreach (FilterDataPinType pt in pin.Mediums)
                {
                    TreeNode mtn = new TreeNode("Medium " + pt.TypeNumber.ToString());
                    mtn.Nodes.Add(new TreeNode("medium clsid: " + pt.MajorType.ToString()));
                    tn.Nodes.Add(mtn);
                }

                foreach (FilterDataPinType pt in pin.Types)
                {
                    TreeNode mtn = new TreeNode("type " + pt.TypeNumber.ToString());
                    mtn.Nodes.Add(new TreeNode("major type: " + DsToString.MediaTypeToString(pt.MajorType) + " {" + pt.MajorType.ToString() + "}"));
                    mtn.Nodes.Add(new TreeNode("subtype: " + DsToString.MediaSubTypeToString(pt.SubType) + " {" + pt.SubType.ToString() + "}"));
                    tn.Nodes.Add(mtn);
                }

                _pinsTreeView.Nodes.Add(tn);
            }
        }
    }

    [ToolboxItem(false)]
    public class AutoSizeTextBox : TextBox
    {
        public AutoSizeTextBox()
        {
            BorderStyle = BorderStyle.None;
            Multiline = false;
            ReadOnly = true;
            TextChanged += new EventHandler(AutoSizeTextbox_TextChanged);
        }

        void AutoSizeTextbox_TextChanged(object sender, EventArgs e)
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            SizeF s = g.MeasureString(Text, Font);
            g.Dispose();
            this.Width = (int)s.Width + 4;
        }
    }
}
