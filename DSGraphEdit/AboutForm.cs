using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DaggerLib.Core;
using DaggerLib.DSGraphEdit;
using DirectShowLib;
using MediaFoundation;
using WeifenLuo.WinFormsUI.Docking;

namespace DSGraphEdit
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();

            // get the versions for libs
            Type t = null;

            t = typeof(IGraphBuilder);
            label3.Text += t.Assembly.GetName().Version;

            t = typeof(IMFGetService);
            label1.Text += t.Assembly.GetName().Version;

            t = typeof(DSGraphEditPanel);
            label5.Text += t.Assembly.GetName().Version;

            t = typeof(DaggerGraph);
            label6.Text += t.Assembly.GetName().Version;

            t = typeof(DockPanel);
            label12.Text += t.Assembly.GetName().Version;

            button1.DialogResult = DialogResult.OK;
            button1.Focus();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://directshownet.sourceforge.net/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://mfnet.sourceforge.net/");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.famfamfam.com/lab/icons/silk/");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gnu.org/copyleft/lesser.txt");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:JohnnyLocust@gmail.com");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://creativecommons.org/licenses/by/3.0/us/");
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.opensource.org/licenses/mit-license.php");
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://sourceforge.net/projects/dockpanelsuite/");
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.codeproject.com/KB/audio-video/dsgraphedit.aspx");
        }
    }
}