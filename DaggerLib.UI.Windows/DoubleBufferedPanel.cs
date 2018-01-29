using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DaggerLib.UI.Windows
{
    [ToolboxItem(false)]
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}
