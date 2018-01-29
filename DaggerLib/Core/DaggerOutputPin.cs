using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerOutputPin : DaggerBasePin , ISerializable
    {
        #region Fields

        // list of pins this output pin is connected to.
        internal List<DaggerInputPin> _connectedTo;

        // how to pass the data to ann input pin
        private PassPinDataAsClone _byClone = PassPinDataAsClone.Always;
        
        // flag to indicate if this output pin can connect to multiple input pins
        private bool _allowmulti = true;

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

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DaggerOutputPin()
        {
            _connectedTo = new List<DaggerInputPin>();
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        protected DaggerOutputPin(SerializationInfo info, StreamingContext ctxt) : base(info,ctxt)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            _connectedTo = new List<DaggerInputPin>();

            _allowmulti = info.GetBoolean("MultiConnect");
            PassByClone = (PassPinDataAsClone)info.GetValue("PassByClone", typeof(PassPinDataAsClone));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if this output pin can connect to a given input pin based on relationship in the Graph
        /// </summary>
        /// <param name="pin">input pin to test</param>
        /// <returns>true if pins can connect</returns>
        public override bool CanConnectToPin(DaggerBasePin pin)
        {
            if (pin is DaggerOutputPin)
            {
                return false;
            }

            if (!pin.ParentNode._descendents.Contains(ParentNode))
            {
                return this.IsCompatibleDataTypes((DaggerInputPin)pin, this);
            }
            else if (pin.ParentNode.Ordinal >= ParentNode.Ordinal)
            {
                return this.IsCompatibleDataTypes((DaggerInputPin)pin, this);
            }

            return false;
        }

        /// <summary>
        /// Connect this pin to Input Pin
        /// </summary>
        /// <param name="input">pin to connect to</param>
        /// <returns>true if succeded</returns>
        public bool ConnectToInput(DaggerInputPin input)
        {
            DaggerGraph outputpincontainer = (_parentNode != null) ? _parentNode.ParentGraph : _parentGraph;
            DaggerGraph inputpincontainer = (input._parentNode != null) ? input._parentNode.ParentGraph : input._parentGraph;

            if (outputpincontainer == null)
            {
                throw new InvalidOperationException("Output pin is not associated with a DaggerGraph");
            }

            if (inputpincontainer == null)
            {
                throw new InvalidOperationException("Input pin is not associated with a DaggerGraph");
            }

            if (inputpincontainer != outputpincontainer)
            {
                throw new InvalidOperationException("Input pin and Output pin are not associated with the same DaggerGraph");
            }

            if (input.IsConnected)
            {
                // disconnect the input pin from it's previous connection
                if (!input.Disconnect(false))
                {
                    return false;
                }
            }

            //call the before connect event to see if we can connect them
            if (outputpincontainer.OnBeforePinsConnected(this, input))
            {
                //connect the two pins
                _connectedTo.Add(input);
                input._connectedTo = this;

                //if we have data, give it to the input
                if (_data != null)
                {
                    input.Data = _data;
                }

                // let the graph know they are connected            
                outputpincontainer.OnPinsConnected(this, input);

                // if the input pin is marked as autoclone, create a duplicate pin
                if (input.AutoClone)
                {
                    // Don't AutoClone during the deserialization process
                    if (!input._parentNode.ParentGraph._isDeserializing)
                    {
                        DaggerInputPin newpin = new DaggerInputPin();
                        newpin.Name = input.Name;
                        newpin.DataType = input.DataType;
                        newpin.AutoClone = true;
                        newpin._wasCloned = true;
                        input.ParentNode.InputPins.Add(newpin);
                    }
                }

                // refresh the UINodes if they are attached
                if (_parentNode != null && _parentNode.UINode != null)
                {
                    _parentNode.UINode.CalculateLayout();
                }
                if (input._parentNode != null && input._parentNode.UINode != null)
                {
                    input._parentNode.UINode.CalculateLayout();
                }

                // if we autocloned, refresh the UIGraph to re-align the noodles
                if (input._autoClone && input._parentNode.UINode != null)
                {
                    input._parentNode.UINode.ParentUIGraph.UpdateNoodles(input._parentNode);
                }

                // call the AfterPinConnected event
                if (PinConnected != null)
                {
                    PinConnected(this, new EventArgs());
                }
                input.InvokeAfterConnect();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnect link going to input
        /// </summary>
        /// <param name="input">Input pin to disconnect from</param>
        /// <param name="forceDisconnect">if true, ignore pre-disconnect testing</param>
        /// <returns>true if disconnect succeded</returns>
        public bool Disconnect(DaggerInputPin input, bool forceDisconnect)
        {
            //get the parent graph of this pin
            DaggerGraph parentGraph = (_parentGraph == null) ? _parentNode.ParentGraph : _parentGraph;

            if ( parentGraph == null)
            {
                throw new InvalidOperationException("Output pin is not associated with a DaggerGraph");
            }

            //do we have this input pin?
            if (_connectedTo.Contains(input))
            {
                // call the before connect event to see if we can disconnect them
                if (parentGraph.OnBeforePinsDisconnected(this, input) || forceDisconnect)
                {
                    _connectedTo.Remove(input);
                    input._connectedTo = null;

                    //let the container know they are disonnected            
                    parentGraph.OnPinsDisonnected(this, input);

                    //if the input was autocloned or is marked AutoClone, remove it from the node
                    DaggerNode inputParentNode = input._parentNode;
                    if (input._wasCloned || input._autoClone)
                    {                        
                        inputParentNode.InputPins.Remove(input);

                        //refresh the ui
                        if (inputParentNode.UINode != null)
                        {
                            inputParentNode.UINode.CalculateLayout();
                            inputParentNode.UINode.ParentUIGraph.UpdateNoodles(inputParentNode);
                        }
                    }

                    //if the input was marked AutoClone, pass the torch to the next compatible pin
                    if (input._autoClone)
                    {
                        foreach (DaggerInputPin pin in inputParentNode.InputPins)
                        {
                            if (pin.DataType == input.DataType)
                            {
                                pin._autoClone = true;
                                break;
                            }
                        }
                    }

                    // raise the AfterPinDisconnected event
                    if (PinDisconnected != null)
                    {
                        PinDisconnected(this, new EventArgs());
                    }
                    input.InvokeAfterDisconnect();

                    return true;
                }
                else
                {
                    //we failed to disconnect them
                    return false;
                }
            }
            else
            {
                // raise the AfterPinDisconnected event
                if (PinDisconnected != null)
                {
                    PinDisconnected(this, new EventArgs());
                }
                input.InvokeAfterDisconnect();

                //since we were never connected to this pin, they are technically disconnected
                return true;
            }
        }

        /// <summary>
        /// Disconnect ALL pins to this output
        /// </summary>
        /// <returns>true if succeded</returns>
        public override bool Disconnect(bool forceDisconnect)
        {
            for (int i = _connectedTo.Count - 1; i > -1; i--)
            {
                if (!_connectedTo[i].Disconnect(forceDisconnect))
                {
                    //we failed to disconnect a pin
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Progerties

        /// <summary>
        /// Get the pin collection this pin belongs to
        /// </summary>
        public DaggerPinCollection<DaggerOutputPin> ParentCollection
        {
            get
            {
                if (_parentNode != null)
                {
                    return _parentNode.OutputPins;
                }
                else if (_parentGraph != null)
                {
                    return _parentGraph.ImportedPins;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or Sets if this pin can connect to multiple Input Pins
        /// </summary>
        public bool AllowMultiConnect
        {
            get
            {
                return _allowmulti;
            }
            set
            {
                _allowmulti = value;
            }
        }

        /// <summary>
        /// Gets if this pin is connected to any Input Pins
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return (this._connectedTo.Count > 0) ? true : false;
            }
        }

        /// <summary>
        /// Gets or sets if this Pin passes data by cloning it
        /// </summary>
        [Browsable(true)]
        public PassPinDataAsClone PassByClone
        {
            get
            {
                return _byClone;
            }
            set
            {
                _byClone = value;
            }
        }

        /// <summary>
        /// Gets or sets the data this pin sends to Input Pins
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
                SetData(value);

                //pass the data to all the connected pins
                foreach (DaggerInputPin pin in _connectedTo)
                {
                    pin.Data = _data;
                }
            }
        }

        /// <summary>
        /// Get the list of Input pins this is connected to
        /// </summary>
        public List<DaggerInputPin> ConnectedTo
        {
            get
            {
                return _connectedTo;
            }
        }

        /// <summary>
        /// Get a list of the Input Pin's guids this is connected to
        /// </summary>
        public List<Guid> ConnectedToGuids
        {
            get
            {
                List<Guid> guids = new List<Guid>();
                foreach (DaggerInputPin pin in _connectedTo)
                {
                    guids.Add(pin.InstanceGuid);
                }
                return guids;
            }
        }

        #endregion

        #region ISerializable

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info, ctxt);
            info.AddValue("PassByClone", PassByClone);
            info.AddValue("MultiConnect", _allowmulti);
        }
        #endregion
    }
}
