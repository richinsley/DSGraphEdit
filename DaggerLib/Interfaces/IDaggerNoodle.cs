using System;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Core;

namespace DaggerLib.Interfaces
{
    public interface IDaggerNoodle
    {
        DaggerOutputPin OutputPin
        {
            get;
        }

        DaggerInputPin InputPin
        {
            get;
        }

        bool Disconnect();
    }
}
