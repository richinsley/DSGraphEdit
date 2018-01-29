using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    internal class DaggerNoodleBisector
    {
        public DaggerNoodle Noodle;
        public Point BisectLocation;

        public DaggerNoodleBisector(DaggerNoodle noodle, Point bisectLocation)
        {
            Noodle = noodle;
            BisectLocation = bisectLocation;
        }
    }
}
