using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerInputPinCollection : CollectionBase , ISerializable
    {
        #region Fields

        //DaggerNode containing these pins
        internal DaggerNode _parentNode;

        //DaggerGraph containing these pins (_parentNode and _parentGraph are mutually exclusive)
        internal DaggerGraph _parentGraph;

        internal event DaggerPinAdded PinAdded;
        internal event DaggerPinRemoved PinRemoved;

        #endregion

        #region ctor

        public DaggerInputPinCollection(DaggerNode parentNode)
        {
            _parentNode = parentNode;
        }

        public DaggerInputPinCollection(DaggerGraph parentGraph)
        {
            _parentGraph = parentGraph;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a list of all pins that have a connection
        /// </summary>
        public List<DaggerInputPin> ConnectedPins
        {
            get
            {
                List<DaggerInputPin> pins = new List<DaggerInputPin>();

                foreach(DaggerInputPin pin in List)
                {
                    if (pin.IsConnected)
                    {
                        pins.Add(pin);
                    }
                }

                return pins;
            }
        }

        /// <summary>
        /// get a pin by it's index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public DaggerInputPin this[int Index]
        {
            get
            {
                return (DaggerInputPin)List[Index];
            }
        }

        /// <summary>
        /// Get a pin by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DaggerInputPin this[string name]
        {
            get
            {
                DaggerInputPin outpin = null;

                foreach (DaggerInputPin pin in List)
                {
                    if (name == pin.Name)
                    {
                        outpin = pin;
                        break;
                    }
                }

                return outpin;
            }
        }

        /// <summary>
        /// Get a pin by it's guid
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal DaggerInputPin this[Guid guid]
        {
            get
            {
                DaggerInputPin outpin = null;

                foreach (DaggerInputPin pin in List)
                {
                    if (guid == pin.InstanceGuid)
                    {
                        outpin = pin;
                        break;
                    }
                }

                return outpin;
            }
        }

        #endregion

        #region Methods

        public bool Contains(DaggerInputPin Pin)
        {
            return List.Contains(Pin);
        }

        public int Add(DaggerInputPin Pin)
        {
            int i;

            i = List.Add(Pin);
            Pin._parentNode = _parentNode;
            Pin._parentGraph = _parentGraph;

            if (PinAdded != null)
            {
                PinAdded(this, Pin);
            }
            return i;
        }

        public void Remove(DaggerInputPin Pin)
        {
            List.Remove(Pin);

            if (PinRemoved != null)
            {
                PinRemoved(this, Pin);
            }

            Pin._parentNode = null;
            Pin._parentGraph = null;
        }

        #endregion

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Inputpins", List);
        }
    }
}