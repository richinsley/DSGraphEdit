using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerInputPin : DaggerBasePin , ISerializable
    {
        #region Fields

        internal DaggerOutputPin _connectedTo;
        internal bool _autoClone = false;
        internal bool _wasCloned = false;

        /// <summary>
        /// Called after the pin is connected
        /// </summary>
        public event EventHandler PinConnected;

        /// <summary>
        /// Called after after the Pin is Disconnected
        /// </summary>
        public event EventHandler PinDisconnected;

        #endregion

        #region ctor

        public DaggerInputPin()
        {

        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        protected DaggerInputPin(SerializationInfo info, StreamingContext ctxt) : base(info,ctxt)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            SerializationInfoEnumerator e = info.GetEnumerator();

            _wasCloned = info.GetBoolean("WasCloned");
            _autoClone = info.GetBoolean("AutoClone");

            try
            {
                Data = info.GetValue("Data", DataType);
            }
            catch (Exception ex)
            {
                //Data wasn't stored
            }
        }

        #endregion

        #region Public Methods

        public override bool CanConnectToPin(DaggerBasePin pin)
        {
            if (pin is DaggerInputPin)
            {
                return false;
            }

            if (ParentNode == null)
            {
                // this is an exported pin, so it can connect to any DaggerOutputPin in the graph
                return this.IsCompatibleDataTypes(this, (DaggerOutputPin)pin);
            }

            if (!ParentNode._descendents.Contains(pin.ParentNode))
            {
                return this.IsCompatibleDataTypes(this, (DaggerOutputPin)pin);
            }
            else if (pin.ParentNode.Ordinal <= ParentNode.Ordinal)
            {
                return this.IsCompatibleDataTypes(this, (DaggerOutputPin)pin);
            }

            return false;
        }

        /// <summary>
        /// Disconnect link going to output
        /// </summary>
        /// <returns>true if disconnect succeded</returns>
        public override bool Disconnect(bool forceDisconnect)
        {
            if (_parentNode == null && _parentGraph == null)
            {
                throw new InvalidOperationException("Input pin is not associated with a DaggerGraph");
            }

            //is it even connected?
            if (IsConnected)
            {
                //call the OutputPin's Disconnect method
                return _connectedTo.Disconnect(this, forceDisconnect);
            }
            else
            {
                //since we were never connected to this pin, they are technically disconnected
                return true;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Becuase DaggerOuputPin actually performs all the work of connecting/disconnected, we need these
        /// internal methods to let DaggerOuputPin raise the connection events on the DaggerInputPin
        /// </summary>

        internal void InvokeAfterConnect()
        {
            if (PinConnected != null)
            {
                PinConnected(this, new EventArgs());
            }
        }

        internal void InvokeAfterDisconnect()
        {
            if (PinDisconnected != null)
            {
                PinDisconnected(this, new EventArgs());
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the pin collection this pin belongs to
        /// </summary>
        public DaggerPinCollection<DaggerInputPin> ParentCollection
        {
            get
            {
                if (_parentNode != null)
                {
                    return _parentNode.InputPins;
                }
                else if (_parentGraph != null)
                {
                    return _parentGraph.ExportedPins;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets if this pin creates a new InputPin when it is connected
        /// </summary>
        public bool AutoClone
        {
            get
            {
                return _autoClone;
            }
            set
            {
                _autoClone = value;
            }
        }

        /// <summary>
        /// Gets if this pin was created by autocloning
        /// </summary>
        public bool WasCloned
        {
            get
            {
                return _wasCloned;
            }
        }

        /// <summary>
        /// Gets the OutputPin this is connectected to.
        /// </summary>
        [Browsable(false)]
        public DaggerOutputPin ConnectedToOutput
        {
            get
            {
                return _connectedTo;
            }
        }

        /// <summary>
        /// Get the InstanceGuid of the pin this is connected to
        /// </summary>
        public Guid ConnectedToGuid
        {
            get
            {
                if (IsConnected)
                {
                    return _connectedTo.InstanceGuid;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the data of the output pin connected to this pin
        /// </summary>
        [Browsable(false)]
        public override object Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (value == null)
                {
                    SetData(null);
                }
                else
                {
                    //if the output pin is marked as PassByClone, clone the data
                    Type t = value.GetType();
                    if (value.GetType().IsPrimitive || !IsConnected)
                    {
                        SetData(value);
                    }
                    else if ((ConnectedToOutput.PassByClone == PassPinDataAsClone.MultiConnect && (ConnectedToOutput._connectedTo.Count > 1)) ||
                    (ConnectedToOutput.PassByClone == PassPinDataAsClone.Always))
                    {
                        // try to clone it
                        ICloneable clone = value as ICloneable;
                        if (clone != null)
                        {
                            SetData(clone.Clone());
                        }
                        // try to serialize it
                        else if (value.GetType().IsSerializable)
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                formatter.Serialize(memStream, value);
                                memStream.Seek(0, SeekOrigin.Begin);
                                SetData(formatter.Deserialize(memStream));
                            }
                        }
                        else
                        {
                            //can't clone it, can't serialize it, so try to unbox it
                            SetData(value);
                        }
                    }
                    else
                    {
                        SetData(value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets if this Input Pin is Connected to an Output Pin
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return (_connectedTo != null) ? true : false;
            }
        }

        #endregion

        #region ISerializable

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info, ctxt);

            //we only want to serialize InputPinData if it's not connected
            if (_data != null && !IsConnected)
            {
                info.AddValue("Data", _data);
            }

            info.AddValue("AutoClone", _autoClone);
            info.AddValue("WasCloned", _wasCloned);
        }

        #endregion
    }
}
