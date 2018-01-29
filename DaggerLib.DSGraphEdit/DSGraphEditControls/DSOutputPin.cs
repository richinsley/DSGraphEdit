using System;
using System.Collections.Generic;
using System.Text;

using DirectShowLib;
using DaggerLib.Core;
using DaggerLib.UI;

namespace DaggerLib.DSGraphEdit
{
    public class DSOutputPin : DaggerOutputPin
    {
        public IPin _pin;

        public DSOutputPin(IPin pin)
            : base()
        {
            // store the IPin
            _pin = pin;

            // we're only using the UI to work with a DirectShow graph
            // so we never pass data, and don't allow pin multi-connect
            AllowMultiConnect = false;
            PassByClone = PassPinDataAsClone.Never;

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