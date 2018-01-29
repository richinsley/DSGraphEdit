using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerOutputPinCollection : CollectionBase, ISerializable
    {
        #region Fields

        internal DaggerNode _parentNode;

        //DaggerGraph containing these pins (_parentNode and _parentGraph are mutually exclusive)
        internal DaggerGraph _parentGraph;

        internal event DaggerPinAdded PinAdded;
        internal event DaggerPinRemoved PinRemoved;

        #endregion

        #region ctor

        public DaggerOutputPinCollection(DaggerNode parentNode)
        {
            _parentNode = parentNode;
        }

        public DaggerOutputPinCollection(DaggerGraph parentGraph)
        {
            _parentGraph = parentGraph;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a list of all pins that have a connection
        /// </summary>
        public List<DaggerOutputPin> ConnectedPins
        {
            get
            {
                List<DaggerOutputPin> pins = new List<DaggerOutputPin>();

                foreach (DaggerOutputPin pin in List)
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
        /// Get a pin by it's index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public DaggerOutputPin this[int Index]
        {
            get
            {
                return (DaggerOutputPin)List[Index];
            }
        }

        /// <summary>
        /// Get a pin by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DaggerOutputPin this[string name]
        {
            get
            {
                DaggerOutputPin outpin = null;

                foreach (DaggerOutputPin pin in List)
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
        internal DaggerOutputPin this[Guid guid]
        {
            get
            {
                DaggerOutputPin outpin = null;

                foreach (DaggerOutputPin pin in List)
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

        public bool Contains(DaggerOutputPin Pin)
        {
            return List.Contains(Pin);
        }

        public int Add(DaggerOutputPin Pin)
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

        public void Remove(DaggerOutputPin Pin)
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
            info.AddValue("OutputPins", List);
        }
    }
}