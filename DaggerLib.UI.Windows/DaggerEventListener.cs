using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace DaggerLib.UI.Windows
{
    [ToolboxItem(false)]
    public class DaggerEventListener : UserControl
    {
        public DaggerEventListener()
        {
            this.BackColor = Color.Transparent;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // pass though the context menu
            if (e.Button == MouseButtons.Right)
            {
                (Parent as DaggerUIGraph).contextMenuStrip.Show(PointToScreen(e.Location));
            }
        }
    }
}
