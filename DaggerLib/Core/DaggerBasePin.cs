using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    /// <summary>
    /// The Base class for Dagger Input and Output pins.
    /// </summary>
    [ToolboxItem(false)]
    [Serializable]
    public abstract class DaggerBasePin : ISerializable
    {
        #region Fields

        //data type the pin holds
        internal Type _dataType = typeof(object);

        // mutual exclusion group the pin belongs to
        private PinMutexGroups _mutexGroup = 0;

        //parent Node for this pin
        internal DaggerNode _parentNode = null;

        //parent Graph for this pin (_parentNode and _parentGraph are mutually exculsive)
        internal DaggerGraph _parentGraph = null;

        //data held by the pin
        protected object _data;

        //guid to help reconnection during deserialization
        internal Guid _instanceGuid;

        //used by DaggerNodeNonSerializationAssistant to replace pins in fields after deserialization
        internal List<FieldInfo> _reflectedTargets = new List<FieldInfo>();

        // indicates this pin is part of an imported/exported pin interface
        private bool _pinInterfacePin;

        #endregion

        #region CTOR

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected DaggerBasePin()
        {
            _instanceGuid = Guid.NewGuid();
        }

        protected DaggerBasePin(SerializationInfo info, StreamingContext ctxt)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            _instanceGuid = (Guid)info.GetValue("InstanceGuid", typeof(Guid));
            DataType = (Type)info.GetValue("DataType", typeof(Type));
            Name = (string)info.GetValue("Name",typeof(string));
            _mutexGroup = (PinMutexGroups)info.GetInt32("Mutex");
            _reflectedTargets = (List<FieldInfo>)info.GetValue("ReflectedTargets", typeof(List<FieldInfo>));
            _pinInterfacePin = info.GetBoolean("IsPinInterface");
        }

        #endregion

        #region UI Fields

        private object _uielements;
        public object PinUIElements
        {
            get
            {
                return _uielements;
            }
            set
            {
                _uielements = value;
            }
        }

        //the name of this pin
        private string _pinName;

        //flag to indicate if the name for the pin has been set yet
        private bool _nameSet;

        #endregion

        #region UI

        public DaggerGraph ParentGraph
        {
            get
            {
                return _parentGraph;
            }

        }

        public IDaggerUIGraph ParentUIGraph
        {
            get
            {
                if (_parentGraph != null)
                {
                    return _parentGraph.ParentUIGraph;
                }
                else
                {
                    if (_parentNode != null)
                    {
                        return _parentNode.ParentGraph.ParentUIGraph;
                    }
                }
                return null;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event that is raised when the Pin's data has been set
        /// </summary>
        public event DaggerPinDataSetHandler PinDataSet;

        public event DaggerPinNameChanged PinNameChanged;

        public event DaggerPinDataTypeChanged PinDataTypeChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Mutual Exclusion Group this pin belongs to
        /// </summary>
        public PinMutexGroups MutexGroup
        {
            get
            {
                return _mutexGroup;
            }
            set
            {
                _mutexGroup = value;
            }
        }

        /// <summary>
        /// Gets flag indicating the pin is part of an imported/exported pin interface
        /// </summary>
        public bool IsInterfacePin
        {
            get
            {
                return _pinInterfacePin;
            }
            internal set
            {
                _pinInterfacePin = value;
            }
        }

        /// <summary>
        /// Gets if the pin is available based on context of it's Mutex Group and connection status
        /// </summary>
        public bool MutexAvailable
        {
            get
            {
                // mutex group "none" and connected pins are always available
                if (_mutexGroup == PinMutexGroups.None || IsConnected) return true;

                // gather mutex groups of connected pins
                PinMutexGroups mutex = PinMutexGroups.All;
                if (this is DaggerInputPin)
                {
                    foreach (DaggerInputPin pin in _parentNode.InputPins)
                    {
                        if (pin.IsConnected)
                        {
                            mutex &= pin.MutexGroup;
                        }
                    }
                }
                else
                {
                    foreach (DaggerOutputPin pin in _parentNode.OutputPins)
                    {
                        if (pin.IsConnected)
                        {
                            mutex &= pin.MutexGroup;
                        }
                    }
                }

                if (mutex == PinMutexGroups.None)
                {
                    // no mutex group pins are connected
                    return true;
                }

                PinMutexGroups diff = _mutexGroup & mutex;
                if (diff == PinMutexGroups.None)
                {
                    return false;
                }

                return true;
            }
        }

        public Guid InstanceGuid
        {
            get
            {
                return _instanceGuid;
            }
        }

        public string Name
        {
            get
            {
                return ToString();
            }
            set
            {
                // if this pin already belongs to a collection, force the new name to be unique
                if (this is DaggerInputPin)
                {
                    if ((this as DaggerInputPin).ParentCollection != null)
                    {
                        value = (this as DaggerInputPin).ParentCollection.UniqueName(value);
                    }
                }
                else
                {
                    if ((this as DaggerOutputPin).ParentCollection != null)
                    {
                        value = (this as DaggerOutputPin).ParentCollection.UniqueName(value);
                    }
                }

                _pinName = value;

                //show we have set the name of this pin
                _nameSet = true;

                //raise the name changed event
                if (PinNameChanged != null)
                {
                    PinNameChanged(this);
                }
            }
        }

        public DaggerNode ParentNode
        {
            get
            {
                return _parentNode;
            }
        }

        public abstract bool IsConnected
        {
            get;
        }

        public abstract object Data
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Data Type this pin can receive or send
        /// </summary>
        public string DataTypeString
        {
            get
            {
                return _dataType.ToString();
            }
            set
            {
                DataType = Type.GetType(value, true, true);
            }
        }

        /// <summary>
        /// Gets or sets the Data Type this pin transmits/receives
        /// </summary>
        public Type DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;

                // Raise the PinDataTypeChanged event
                if(PinDataTypeChanged != null)
                {
                    PinDataTypeChanged(this,value);
                }

                // refresh the UINodes if they are attached
                if (_parentNode != null && _parentNode.UINode != null)
                {
                    _parentNode.UINode.CalculateLayout();
                }
            }
        }

        /// <summary>
        /// Gets the Data Type Code of this pin
        /// </summary>
        public TypeCode DataTypeCode
        {
            get
            {
                return Type.GetTypeCode(_dataType);
            }
        }

        #endregion

        #region Abstract

        public abstract bool CanConnectToPin(DaggerBasePin pin);

        public abstract bool Disconnect(bool forceDisconnect);

        #endregion

        #region Protected Methods

        protected void SetData(object data)
        {
            if (data == null)
            {
                _data = null;
            }
            try
            {
                _data = data;
            }
            catch(InvalidCastException ex)
            {
                //we need to convert the data
                TypeConverter tc = TypeDescriptor.GetConverter(data);
                if (tc is BooleanConverter)
                {
                    // BooleanConverters only go to/from text so change data to an Int holding 0 or 1
                    // then get a IntTypeConverter
                    data = (int)(((bool)data) ? 1 : 0);
                    tc = TypeDescriptor.GetConverter(data);
                }

                if (tc != null)
                {
                    if(tc.CanConvertTo(_dataType))
                    {
                        _data = tc.ConvertTo(data, _dataType);
                    }
                    else
                    {
                        //can't convert
                        throw new InvalidCastException();
                    }
                }
                else
                {
                    //no IConvertible available
                    throw new InvalidCastException();
                }
            }

            if (PinDataSet != null)
            {
                PinDataSet(this, _data);
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            //if the name has been set return that
            if (_nameSet)
            {
                return _pinName;
            }
            else
            {
                return base.ToString();
            }
        }

        #endregion

        #region Static

        public static bool IsCompatibleDataTypes(Type inputPinType, Type outputPinType)
        {
            // if the pin DataTypes are directly assignable they are compatible
            if (inputPinType.IsAssignableFrom(outputPinType))
            {
                return true;
            }

            // if the DataType of outpin is a subclass in inpin they are considered compatible
            if (outputPinType.IsSubclassOf(inputPinType))
            {
                return true;
            }

            // if both types are primitive they are compatible
            if (inputPinType.IsPrimitive && outputPinType.IsPrimitive)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsCompatibleDataTypes(DaggerInputPin inpin, DaggerOutputPin outpin)
        {
            return IsCompatibleDataTypes(inpin.DataType, outpin.DataType);
        }

        #endregion

        #region ISerializable

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("InstanceGuid", InstanceGuid);
            info.AddValue("DataType", DataType);
            info.AddValue("Name", Name);
            info.AddValue("Mutex", (int)_mutexGroup);
            info.AddValue("ReflectedTargets", _reflectedTargets);
            info.AddValue("IsPinInterface", _pinInterfacePin);
        }

        #endregion
    }
}