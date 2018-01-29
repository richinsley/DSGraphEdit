using System;
using System.Collections.Generic;
using System.Text;

namespace DaggerLib.Core
{
    /// <summary>
    /// Class that represents a pin name/datatype pair for a DaggerInterface
    /// </summary>
    public class DaggerInterfacePin
    {
        private string _pinName;
        private Type _pinDataType;

        public DaggerInterfacePin(string pinName, Type pinDataType)
        {
            _pinName = pinName;
            _pinDataType = pinDataType;
        }

        public string PinName
        {
            get
            {
                return _pinName;
            }
        }

        public Type PinDataType
        {
            get
            {
                return _pinDataType;
            }
        }
    }
}
