using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace DaggerLib.Core
{
    public class DaggerInterface
    {
        private List<DaggerInterfacePin> _inputPins;
        private List<DaggerInterfacePin> _outputPins;

        public DaggerInterface()
        {
            _inputPins = new List<DaggerInterfacePin>();
            _outputPins = new List<DaggerInterfacePin>();
        }

        /// <summary>
        /// Contruct a DaggerInterface from an existing DaggerNode
        /// </summary>
        /// <param name="node"></param>
        public DaggerInterface(DaggerNode node)
        {
            _inputPins = new List<DaggerInterfacePin>();
            foreach (DaggerInputPin pin in node.InputPins)
            {
                DaggerInterfacePin newpin = AddInputPin(pin);
                if (newpin == null)
                {
                    throw new InvalidOperationException("Interface already contains an Input Pin named " + pin.Name);
                }
            }

            _outputPins = new List<DaggerInterfacePin>();
            foreach (DaggerOutputPin pin in node.OutputPins)
            {
                DaggerInterfacePin newpin = AddOutputPin(pin);
                if (newpin == null)
                {
                    throw new InvalidOperationException("Interface already contains an Output Pin named " + pin.Name);
                }
            }
        }

        /// <summary>
        /// Construct a new DaggerInterface from an exisiting one
        /// </summary>
        /// <param name="daggerInterface"></param>
        public DaggerInterface(DaggerInterface daggerInterface)
        {
            // we can create lists directly from the daggerInterface because the Pins properties are already deep copies
            _inputPins = new List<DaggerInterfacePin>(daggerInterface.InputPins);
            _outputPins = new List<DaggerInterfacePin>(daggerInterface.OutputPins);
        }

        #region Operator Overloads

        public static DaggerInterface operator +(DaggerInterface dx, DaggerInterface dy)
        {
            // create a new temp interface
            DaggerInterface newinterface = new DaggerInterface(dx);

            // merge the pins with the second
            foreach (DaggerInterfacePin pin in dy._inputPins)
            {
                newinterface.AddInputPin(pin);
            }
            foreach (DaggerInterfacePin pin in dy._outputPins)
            {
                newinterface.AddOutputPin(pin);
            }

            return newinterface;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if interface contains a given Imported DaggerInterfacePin
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool ContainsImportPin(DaggerInterfacePin pin)
        {
            foreach (DaggerInterfacePin inpin in this._inputPins)
            {
                if (inpin.PinDataType == pin.PinDataType && inpin.PinName == pin.PinName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if interface contains a given Exported DaggerInterfacePin
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool ContainsExportPin(DaggerInterfacePin pin)
        {
            foreach (DaggerInterfacePin outpin in this._outputPins)
            {
                if (outpin.PinDataType == pin.PinDataType && outpin.PinName == pin.PinName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if given DaggerGraph implements this DaggerInterface
        /// </summary>
        /// <param name="Graph"></param>
        /// <returns></returns>
        public bool GraphImplements(DaggerGraph Graph)
        {
            foreach (DaggerInterfacePin pin in _inputPins)
            {
                if (GetImportPin(pin, Graph) == null)
                {
                    return false;
                }
            }

            foreach (DaggerInterfacePin pin in _outputPins)
            {
                if (GetExportPin(pin, Graph) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private DaggerBasePin GetImportPin(DaggerInterfacePin pin, DaggerGraph Graph)
        {
            return (DaggerBasePin)Graph.GetImportPin(pin.PinName, pin.PinDataType);
        }

        private DaggerBasePin GetExportPin(DaggerInterfacePin pin, DaggerGraph Graph)
        {
            return (DaggerBasePin)Graph.GetExportPin(pin.PinName, pin.PinDataType);
        }

        /// <summary>
        /// Returns true if this implements given DaggerInterface
        /// </summary>
        /// <param name="daggerInterface"></param>
        /// <returns></returns>
        public bool Implements(DaggerInterface daggerInterface)
        {
            foreach(DaggerInterfacePin pin in daggerInterface._inputPins)
            {
                // find the input pin by name
                DaggerInterfacePin foundPin = GetInputPin(pin.PinName);
                if (foundPin == null) return false;

                // check type compatiblity
                if (!((foundPin.PinDataType.IsAssignableFrom(pin.PinDataType)) || pin.PinDataType.IsAssignableFrom(foundPin.PinDataType)))
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(foundPin.PinDataType);
                    if (tc != null)
                    {
                        if (!tc.CanConvertFrom(foundPin.PinDataType)) return false;
                    }
                    else { return false; }
                }
            }

            foreach (DaggerInterfacePin pin in daggerInterface._outputPins)
            {
                // find the output pin by name
                DaggerInterfacePin foundPin = GetOutputPin(pin.PinName);
                if (foundPin == null) return false;

                // check type compatibility
                if (!((foundPin.PinDataType.IsAssignableFrom(pin.PinDataType)) || pin.PinDataType.IsAssignableFrom(foundPin.PinDataType)))
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(foundPin.PinDataType);
                    if (tc != null)
                    {
                        return tc.CanConvertFrom(foundPin.PinDataType);
                    }
                    else { return false; }
                }
            }

            return true;
        }

        public DaggerInterfacePin GetInputPin(string name)
        {
            foreach (DaggerInterfacePin pin in _inputPins)
            {
                if (pin.PinName == name)
                {
                    return pin;
                }
            }
            return null;
        }

        public DaggerInterfacePin GetOutputPin(string name)
        {
            foreach (DaggerInterfacePin pin in _outputPins)
            {
                if (pin.PinName == name)
                {
                    return pin;
                }
            }
            return null;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a Deep Copy of the interface's InputPins
        /// </summary>
        public List<DaggerInterfacePin> InputPins
        {
            get
            {
                List<DaggerInterfacePin> pins = new List<DaggerInterfacePin>();
                foreach (DaggerInterfacePin pin in _inputPins)
                {
                    pins.Add(new DaggerInterfacePin(pin.PinName,pin.PinDataType));
                }
                return pins;
            }
        }

        /// <summary>
        /// Gets a Deep Copy of the interface's OutputPins
        /// </summary>
        public List<DaggerInterfacePin> OutputPins
        {
            get
            {
                List<DaggerInterfacePin> pins = new List<DaggerInterfacePin>();
                foreach (DaggerInterfacePin pin in _outputPins)
                {
                    pins.Add(new DaggerInterfacePin(pin.PinName, pin.PinDataType));
                }
                return pins;
            }
        }

        #endregion

        #region AddInputPin

        public DaggerInterfacePin AddInputPin(DaggerInterfacePin pin)
        {
            return AddInputPin(pin.PinName, pin.PinDataType);
        }

        public DaggerInterfacePin AddInputPin(DaggerInputPin pin)
        {
           return AddInputPin(pin.Name, pin.DataType);
        }

        public DaggerInterfacePin AddInputPin(string pinName, Type pinDataType)
        {
            bool found = false;
            foreach (DaggerInterfacePin pin in _inputPins)
            {
                if (pin.PinName == pinName)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                DaggerInterfacePin pin = new DaggerInterfacePin(pinName, pinDataType);
                _inputPins.Add(pin);
                return pin;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region AddOutputPin

        public DaggerInterfacePin AddOutputPin(DaggerInterfacePin pin)
        {
            return AddOutputPin(pin.PinName, pin.PinDataType);
        }

        public DaggerInterfacePin AddOutputPin(DaggerOutputPin pin)
        {
            return AddOutputPin(pin.Name, pin.DataType);
        }

        public DaggerInterfacePin AddOutputPin(string pinName, Type pinDataType)
        {
            bool found = false;
            foreach (DaggerInterfacePin pin in _outputPins)
            {
                if (pin.PinName == pinName)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                DaggerInterfacePin pin = new DaggerInterfacePin(pinName, pinDataType);
                _outputPins.Add(pin);
                return pin;
            }
            else
            {
                return null;
            }
        }
        #endregion

    }
}
