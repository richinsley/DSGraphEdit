using System;
using System.Collections.Generic;
using System.Text;

using DirectShowLib;
using DaggerLib.Core;
using DaggerLib.UI;

namespace DaggerLib.DSGraphEdit
{
    public class DSInputPin : DaggerInputPin
    {
        public IPin _pin;

        public DSInputPin(IPin pin) : base()
        {
            _pin = pin;

            // get the name of the IPin
            PinInfo pi;
            _pin.QueryPinInfo(out pi);
            Name = pi.name;
            DsUtils.FreePinInfo(pi);
        }

        public override bool IsCompatibleDataTypes(DaggerInputPin inpin, DaggerOutputPin outpin)
        {
            // all pins in DSGraphEdit are compatible becuase they hold no data
            return true;
        }
    }
}
