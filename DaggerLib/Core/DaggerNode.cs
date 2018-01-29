using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerNode : ISerializable
    {
        #region Fields

        //Graph this node belongs to
        private DaggerGraph _parentGraph;

        //DaggerUINode that contains this node
        private IDaggerUINode _uiNode;

        //the collections of pins
        private DaggerPinCollection<DaggerInputPin> _inputPins;
        private DaggerPinCollection<DaggerOutputPin> _outputPins;

        //the execution order of this node in relation to it's siblings
        private int _ordinal = -1;

        //discreet subgraph this node belongs to
        private int _subgraphAffiliation = -1;

        //the type of UINode that can edit this node
        private string _associatedUINode = "IDaggerUINode";

        //List containing all the nodes that this node effects in thier order of execution
        public List<DaggerNode> _descendents = new List<DaggerNode>();

        // help string for the node
        private string _helpString = string.Empty;

        //dictionaries containing the cached connection status of Pin Mutex Groups
        internal Dictionary<PinMutexGroups, bool> _inputMutexConnections = new Dictionary<PinMutexGroups, bool>();
        internal Dictionary<PinMutexGroups, bool> _outputMutexConnections = new Dictionary<PinMutexGroups, bool>();

        private Guid _instanceGuid = Guid.NewGuid();

        #endregion

        #region ctor

        public DaggerNode()
        {
            //create the collections to hold the pins and hook thier add/remove events
            _inputPins = new DaggerPinCollection<DaggerInputPin>(this);
            _outputPins = new DaggerPinCollection<DaggerOutputPin>(this);

            InputPins.PinAdded += new DaggerPinAdded(_PinAddedRemoved);
            InputPins.PinRemoved += new DaggerPinRemoved(_PinAddedRemoved);

            OutputPins.PinAdded += new DaggerPinAdded(_PinAddedRemoved);
            OutputPins.PinRemoved += new DaggerPinRemoved(_PinAddedRemoved);
        }

        protected DaggerNode(SerializationInfo info, StreamingContext ctxt)
        {
            //create the collections to hold the pins and hook thier add/remove events
            _inputPins = new DaggerPinCollection<DaggerInputPin>(this);
            _outputPins = new DaggerPinCollection<DaggerOutputPin>(this);

            _instanceGuid = (Guid)info.GetValue("InstanceGuid", typeof(Guid));

            InputPins.PinAdded += new DaggerPinAdded(_PinAddedRemoved);
            InputPins.PinRemoved += new DaggerPinRemoved(_PinAddedRemoved);

            OutputPins.PinAdded += new DaggerPinAdded(_PinAddedRemoved);
            OutputPins.PinRemoved += new DaggerPinRemoved(_PinAddedRemoved);
        }

        #endregion

        #region Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guid InstanceGuid
        {
            get
            {
                return _instanceGuid;
            }
            internal set
            {
                _instanceGuid = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DaggerGraph ParentGraph
        {
            get
            {
                return _parentGraph;
            }
            internal set
            {
                _parentGraph = value;
            }
        }

        /// <summary>
        /// Returns the Category the Node belongs to.  Override to provide information on where
        /// the node should be placed in a TreeView. 
        /// Return value should be of format "(Category Name)" or "(Category Name),(Node Name)"
        /// </summary>
        public static string NodeCategory()
        {
            return "";
        }

        public virtual string AssociatedUINode
        {
            get
            {
                return _associatedUINode;
            }
            set
            {
                _associatedUINode = value;
            }
        }

        public virtual string HelpString
        {
            get
            {
                return _helpString;
            }
            set
            {
                if (value == null)
                {
                    _helpString = "";
                }
                else
                {
                    _helpString = value;
                }
            }
        }

        public DaggerPinCollection<DaggerInputPin> InputPins
        {
            get
            {
                return _inputPins;
            }
        }

        public DaggerPinCollection<DaggerOutputPin> OutputPins
        {
            get
            {
                return _outputPins;
            }
        }

        /// <summary>
        /// The Execution order of this node in relation to it's subgraph siblings
        /// </summary>
        [Browsable(false)]
        public int Ordinal
        {
            get
            {
                return _ordinal;
            }
            internal set
            {
                _ordinal = value;
            }
        }

        /// <summary>
        /// The Discreet subgraph this node belongs to
        /// </summary>
        [Browsable(false)]
        public int SubgraphAffiliation
        {
            get
            {
                return _subgraphAffiliation;
            }
            internal set
            {
                _subgraphAffiliation = value;
            }
        }

        /// <summary>
        /// Returns true if this node has no connected input pins
        /// </summary>
        [Browsable(false)]
        public bool IsTopLevel
        {
            get
            {
                //are any input pins connected?
                DaggerInputPin inpin = null;

                foreach (DaggerInputPin pin in _inputPins)
                {
                    if (pin.IsConnected)
                    {
                        //ignore exported pins
                        if (pin._connectedTo._parentNode != null)
                        {
                            inpin = pin;
                            break;
                        }
                    }
                }

                if (inpin == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if this node has no connected output pins
        /// </summary>
        [Browsable(false)]
        public bool IsBottomLevel
        {
            get
            {
                //are any input pins connected?
                DaggerOutputPin outpin = null;

                foreach (DaggerOutputPin pin in _outputPins)
                {
                    if (pin.IsConnected)
                    {
                        //ignore exported pins
                        foreach (DaggerInputPin inpin in pin._connectedTo)
                        {
                            if (inpin._parentNode != null)
                            {
                                outpin = pin;
                                break;
                            }
                        }
                    }
                }

                if (outpin == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Browsable(false)]
        public IDaggerUINode UINode
        {
            get
            {
                return _uiNode;
            }
            set
            {
                _uiNode = value;
            }
        }

        /// <summary>
        /// Get all the Input Pin Mutex Groups that are fully connected
        /// </summary>
        [Browsable(false)]
        public PinMutexGroups InputMutexGroupsConnected
        {
            get
            {
                PinMutexGroups groups = PinMutexGroups.None;
                foreach (KeyValuePair<PinMutexGroups,bool> kvp in _inputMutexConnections)
                {
                    if (kvp.Value)
                    {
                        groups |= kvp.Key;
                    }
                }
                return groups;
            }
        }

        /// <summary>
        /// Get all the Output Pin Mutex Groups that are fully connected
        /// </summary>
        [Browsable(false)]
        public PinMutexGroups OutputMutexGroupsConnected
        {
            get
            {
                PinMutexGroups groups = PinMutexGroups.None;
                foreach (KeyValuePair<PinMutexGroups, bool> kvp in _outputMutexConnections)
                {
                    if (kvp.Value)
                    {
                        groups |= kvp.Key;
                    }
                }
                return groups;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets if all input pins in a mutex group(s) is(are) connected
        /// </summary>
        /// <param name="groups">Mutex group(s) to check for connections</param>
        /// <returns>true if all pins are connected</returns>
        internal bool IsInputMutexGroupsConnected(PinMutexGroups groups)
        {
            foreach (DaggerInputPin pin in InputPins)
            {
                if ((pin.MutexGroup & groups) != PinMutexGroups.None && !pin.IsConnected)
                {
                    //there is a pin in one of the mutex groups that is not connected
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets if all output pins in a mutex group(s) is(are) connected
        /// </summary>
        /// <param name="groups">Mutex group(s) to check for connections</param>
        /// <returns>true if all pins are connected</returns>
        internal bool IsOutputMutexGroupsConnected(PinMutexGroups groups)
        {
            foreach (DaggerOutputPin pin in OutputPins)
            {
                if ((pin.MutexGroup & groups) != PinMutexGroups.None && !pin.IsConnected)
                {
                    //there is a pin in one of the mutex groups that is not connected
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Raises the DaggerUINodeAttached event on this Node
        /// </summary>
        /// <param name="uinode"></param>
        public void InvokeUINodeAttached(IDaggerUINode uinode)
        {
            if (DaggerUINodeAttached != null)
            {
                DaggerUINodeAttached(uinode);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begin Processing subgraph starting at this node
        /// </summary>
        public void Process()
        {
            if (_parentGraph != null)
            {
                _parentGraph.GraphScheduler.ProcessGraph(this);
            }
        }

        /// <summary>
        /// Disconnect all pins connected to this node
        /// </summary>
        /// <returns>true if all pins diconnected</returns>
        public bool DisconnectAllPins()
        {
            // go in reverse order for pins to prevent autocloned pins from throwing off the collection iterator

            for (int i = OutputPins.Count; i > 0; i--)
            {
                if (!OutputPins[i - 1].Disconnect(false))
                {
                    //we failed to disconnect a pin
                    return false;
                }
            }
            
            for (int i = InputPins.Count; i > 0; i--)
            {
                if (!InputPins[i - 1].Disconnect(false))
                {
                    //we failed to disconnect a pin
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Events and Handlers

        /// <summary>
        /// Event that is raised when a DaggerUINode is attached to this node
        /// </summary>
        public event DaggerUINodeAttachedHandler DaggerUINodeAttached;

        /// <summary>
        /// Event that is rasied before a node is removed from it's container
        /// </summary>
        public event BeforeNodeRemoveHandler BeforeNodeRemoved;

        /// <summary>
        /// Event that is rasied after a node is removed from it's container
        /// </summary>
        public event AfterNodeRemoveHandler AfterNodeRemoved;

        // Handler for the Adding/Removing of pins from the collections
        void _PinAddedRemoved(object sender, DaggerBasePin pin)
        {
            if (_uiNode != null)
            {
                _uiNode.CalculateLayout();
            }
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Override to provide processing of the node from a Graph Scheduler
        /// </summary>
        public virtual void DoProcessing()
        {

        }

        /// <summary>
        /// Called before a node is removed from it's container
        /// </summary>
        /// <returns>true if node can be safely removed</returns>
        public virtual bool OnBeforeNodeRemoved()
        {
            if (BeforeNodeRemoved == null)
            {
                return true;
            }
            else
            {
                //make sure we actually have a parent and raise the event
                if (_parentGraph != null)
                {
                    return BeforeNodeRemoved(this);
                }
                else
                {
                    return true;
                }
            }
        }

        public virtual void OnAfterNodeRemoved()
        {
            //if there is a parent UIGraph, let it know so it can also be removed
            if (_uiNode != null && _uiNode.ParentUIGraph != null)
            {
                _uiNode.ParentUIGraph.OnAfterNodeRemoved(_uiNode);
            }

            if (AfterNodeRemoved != null)
            {
                //make sure we actually have a parent and raise the event
                if (_parentGraph != null)
                {
                    AfterNodeRemoved(this);
                }
            }
        }

        #endregion

        #region ISerializable

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("AssociatedUINode", AssociatedUINode);
            info.AddValue("InstanceGuid", _instanceGuid);

            //we don't want to serialize the PinCollections, just the list of Auto-Gened Inputs
            List<DaggerInputPin> inputpins = new List<DaggerInputPin>();

            foreach (DaggerInputPin pin in InputPins)
            {
                //serialize auto-gened pins
                //inputpins.Add(pin);
            }

            //info.AddValue("InputPins", inputpins);
        }

        #endregion
    }
}
