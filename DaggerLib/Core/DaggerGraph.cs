using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

using DaggerLib.UI;
using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerGraph :  ISerializable , ICloneable
    {
        #region Fields

        private List<DaggerNode> _nodes;

        //the user interface graph handling this graph
        private IDaggerUIGraph _parentUIGraph;

        //the number of subgraphs in this graph as computed by CalculateTopology
        private int _subGraphCount = 1;

        //list of guids of pin connections used to reconstruct after deserialization
        private List<PinConnection> _allConnections;

        internal DaggerPinCollection<DaggerOutputPin> _importedPins;
        internal DaggerPinCollection<DaggerInputPin> _exportedPins;

        private List<DaggerOutputPin> inlist;
        private List<DaggerInputPin> exlist;

        //flag to indicate if this graph has been fully deserialized
        private bool _isDeserialized = false;

        //flag to prevent pin AutoCloning during the deserialization process
        internal bool _isDeserializing = false;

        /// <summary>
        /// Which subgraph to serialize.  -1 for all subgraphs
        /// </summary>
        private int _subGraphToSerialize = -1;

        /// <summary>
        /// Selection of nodes and noodles to serialize
        /// </summary>
        private ISelector _selectionToSerialize = null;

        // SubNode this graph lies within
        internal DaggerSubNode _parentSubNode;

        /// <summary>
        /// Nodes that need assistance deserializing because they're not marked "Serializable"
        /// </summary>
        private List<DaggerNodeNonSerializationAssistant> _assistNodes;

        /// <summary>
        /// Graph scheduler that performs processing of the the graph
        /// </summary>
        private IGraphScheduler _scheduler;

        // stored layout of ui elements
        private List<GraphLayout> _layout;

        #endregion

        #region ctor

        public DaggerGraph()
        {
            _nodes = new List<DaggerNode>();

            _importedPins = new DaggerPinCollection<DaggerOutputPin>(this);
            _importedPins.PinAdded += new DaggerPinAdded(_importedPins_PinAdded);
            _exportedPins = new DaggerPinCollection<DaggerInputPin>(this);
            _exportedPins.PinAdded += new DaggerPinAdded(_exportedPins_PinAdded);

            _scheduler = new OrdinalExecutionScheduler();
            _scheduler.Graph = this;
        }

        /// <summary>
        /// Construct an empty DaggerGraph from a Pin Interface
        /// </summary>
        /// <param name="pinInterface"></param>
        public DaggerGraph(DaggerInterface pinInterface)
        {
            _nodes = new List<DaggerNode>();

            _importedPins = new DaggerPinCollection<DaggerOutputPin>(this);
            _importedPins.PinAdded += new DaggerPinAdded(_importedPins_PinAdded);
            _exportedPins = new DaggerPinCollection<DaggerInputPin>(this);
            _exportedPins.PinAdded += new DaggerPinAdded(_exportedPins_PinAdded);

            // Add the imported/exported pins
            AddImportPins(pinInterface.InputPins);
            AddExportPins(pinInterface.OutputPins);

            _scheduler = new OrdinalExecutionScheduler();
            _scheduler.Graph = this;
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        protected DaggerGraph(SerializationInfo info, StreamingContext ctxt)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            _importedPins = new DaggerPinCollection<DaggerOutputPin>(this);
            _importedPins.PinAdded += new DaggerPinAdded(_importedPins_PinAdded);
            _exportedPins = new DaggerPinCollection<DaggerInputPin>(this);
            _exportedPins.PinAdded += new DaggerPinAdded(_exportedPins_PinAdded);

            inlist = (List<DaggerOutputPin>)info.GetValue("ImportedPins", typeof(List<DaggerOutputPin>));
            exlist  = (List<DaggerInputPin>)info.GetValue("ExportedPins", typeof(List<DaggerInputPin>));

            // get serialized nodes
            _nodes = (List<DaggerNode>)info.GetValue("AllNodes", typeof(List<DaggerNode>));

            // get list of nodes that need assistance to deserialize
            _assistNodes = (List<DaggerNodeNonSerializationAssistant>)info.GetValue("AssistNodes", typeof(List<DaggerNodeNonSerializationAssistant>));
            
            // try to get the layout
            try
            {
                _layout = (List<GraphLayout>)info.GetValue("Layout",typeof(List<GraphLayout>));
            }
            catch (Exception ex)
            {
                //layout wasn't stored
            }

            _allConnections = (List<PinConnection>)info.GetValue("AllConnections", typeof(List<PinConnection>));            
        }

        #endregion

        #region Events

        /// <summary>
        /// Called whenever a node is added/removed or a pin is connected/disconnected
        /// </summary>
        public event EventHandler OnTopologyChanged;

        /// <summary>
        /// Called when a node has been added to the graph
        /// </summary>
        public event EventHandler NodeAdded;

        /// <summary>
        /// Called before a node is removed from the graph
        /// </summary>
        public event BeforeNodeRemoveHandler BeforeNodeRemoved;

        /// <summary>
        /// Called after a node is removed from the graph
        /// </summary>
        public event AfterNodeRemoveHandler AfterNodeRemoved;

        /// <summary>
        /// Called before two pins are connected
        /// </summary>
        public event PinBeforeConnectedHandler BeforePinsConnected;

        /// <summary>
        /// Called after two pins are connected
        /// </summary>
        public event PinAfterConnectedHandler AfterPinsConnected;

        /// <summary>
        /// Called before two pins are disconnected
        /// </summary>
        public event PinBeforeDisconnectedHandler BeforePinsDisconnected;

        /// <summary>
        /// Called after two pins are disconnected
        /// </summary>
        public event PinAfterDisconnectedHandler AfterPinsDisconnected;

        #endregion

        #region Properties

        [Browsable(false)]
        public IDaggerUIGraph ParentUIGraph
        {
            get
            {
                return _parentUIGraph;
            }
            set
            {
                _parentUIGraph = value;
            }
        }

        /// <summary>
        /// Get the stored layout of ui elements
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<GraphLayout> UILayout
        {
            get
            {
                return _layout;
            }
        }

        /// <summary>
        /// Gets the SubNode this graph lies within
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DaggerSubNode ParentSubNode
        {
            get
            {
                return _parentSubNode;
            }
        }

        /// <summary>
        /// Sets a selection of Nodes and Noodles to serialize
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISelector SelectionToSerialize
        {
            set
            {
                _selectionToSerialize = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGraphScheduler GraphScheduler
        {
            get
            {
                return _scheduler;
            }
            internal set
            {
                if (_scheduler != null)
                {
                    _scheduler.CancelProcessing();
                    _scheduler.Dispose();
                    _scheduler = null;
                }

                // we never want the Scheduler to be null
                if (value != null)
                {
                    _scheduler = value;
                    _scheduler.Graph = this;
                }
                else
                {
                    _scheduler = new OrdinalExecutionScheduler();
                    _scheduler.Graph = this;
                }

                _scheduler.OnTopologyChanged();
            }
        }

        public DaggerPinCollection<DaggerOutputPin> ImportedPins
        {
            get
            {
                return _importedPins;
            }
        }

        public DaggerPinCollection<DaggerInputPin> ExportedPins
        {
            get
            {
                return _exportedPins;
            }
        }

        public List<DaggerNode> AllNodes
        {
            get
            {
                //create a duplicate list and sort them by thier ordinals
                List<DaggerNode> allnodes = new List<DaggerNode>(_nodes);
                allnodes.Sort(new OrdinalComparer());
                return allnodes;
            }
        }

        /// <summary>
        /// Gets the number of subgraphs in this graph
        /// </summary>
        [Browsable(false)]
        public int SubGraphCount
        {
            get
            {
                return _subGraphCount;
            }
        }

        /// <summary>
        /// Gets the nodes in a subgraph of this graph
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Browsable(false)]
        public List<DaggerNode> this[int subgraph]
        {
            get
            {
                if (subgraph > (_subGraphCount - 1))
                {
                    throw new ArgumentOutOfRangeException();
                }

                List<DaggerNode> snodes = new List<DaggerNode>();
                foreach (DaggerNode node in _nodes)
                {
                    if (subgraph == node.SubgraphAffiliation)
                    {
                        snodes.Add(node);
                    }
                }

                //sort the new list by the ordinals
                snodes.Sort(new OrdinalComparer());
                return snodes;
            }
        }

        /// <summary>
        /// Gets all the nodes in an Ordinal of a SubGraph
        /// </summary>
        /// <param name="subgraph"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        [Browsable(false)]
        public List<DaggerNode> this[int subgraph, int ordinal]
        {
            get
            {
                List<DaggerNode> sgraph = this[subgraph];

                List<DaggerNode> nodes = new List<DaggerNode>();

                foreach (DaggerNode node in sgraph)
                {
                    if (node.Ordinal == ordinal)
                    {
                        nodes.Add(node);
                    }
                }

                return nodes;
            }
        }

        /// <summary>
        /// Get list of all nodes that have no connected input pins
        /// </summary>
        public List<DaggerNode> TopLevelNodes
        {
            get
            {
                List<DaggerNode> list = new List<DaggerNode>();

                foreach (DaggerNode node in _nodes)
                {
                    if ((node != null) && (node.IsTopLevel))
                    {
                        list.Add(node);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// Get list of all nodes that have no connected output pins
        /// </summary>
        public List<DaggerNode> BottomLevelNodes
        {
            get
            {
                List<DaggerNode> list = new List<DaggerNode>();

                foreach (DaggerNode node in _nodes)
                {
                    if ((node != null) && (node.IsBottomLevel))
                    {
                        list.Add(node);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// Get a list of all valid pin connections in a selection
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="includeExports"></param>
        /// <returns></returns>
        private List<PinConnection> this[ISelector selection]
        {
            get
            {
                List<PinConnection> con = new List<PinConnection>();

                foreach (IDaggerNoodle noodle in selection.SelectedNoodles)
                {                    
                    if (noodle.OutputPin.ParentNode == null)
                    {
                        // is it in imported noodle?
                        if (selection.SelectedNodes.Contains(noodle.InputPin.ParentNode.UINode))
                        {
                            PinConnection pc = new PinConnection();
                            pc.ConnectedFrom = noodle.OutputPin.InstanceGuid;
                            pc.ConnectedTo = noodle.InputPin.InstanceGuid;
                            con.Add(pc);
                        }
                    }
                    else if (noodle.InputPin.ParentNode == null)
                    {
                        // is it an exported noodle?
                        if (selection.SelectedNodes.Contains(noodle.OutputPin.ParentNode.UINode))
                        {
                            PinConnection pc = new PinConnection();
                            pc.ConnectedFrom = noodle.OutputPin.InstanceGuid;
                            pc.ConnectedTo = noodle.InputPin.InstanceGuid;
                            con.Add(pc);
                        }
                    }
                    else
                    {
                        // it's a regular noodle
                        if (selection.SelectedNodes.Contains(noodle.InputPin.ParentNode.UINode) && selection.SelectedNodes.Contains(noodle.OutputPin.ParentNode.UINode))
                        {
                            PinConnection pc = new PinConnection();
                            pc.ConnectedFrom = noodle.OutputPin.InstanceGuid;
                            pc.ConnectedTo = noodle.InputPin.InstanceGuid;
                            con.Add(pc);
                        }
                    }
                }
                return con;
            }
        }

        /// <summary>
        /// Get a list of all pin connections in a subgraph
        /// </summary>
        private List<PinConnection> this[int subgraph,bool includeExports]
        {
            get
            {
                List<PinConnection> con = new List<PinConnection>();

                if (subgraph < SubGraphCount)
                {
                    List<DaggerNode> allnodes = AllNodes;
                    foreach (DaggerNode node in allnodes)
                    {
                        if (node.SubgraphAffiliation == subgraph)
                        {
                            foreach (DaggerOutputPin outpin in node.OutputPins)
                            {
                                foreach (DaggerInputPin inpin in outpin._connectedTo)
                                {
                                    if (includeExports)
                                    {
                                        PinConnection pc = new PinConnection();
                                        pc.ConnectedFrom = outpin.InstanceGuid;
                                        pc.ConnectedTo = inpin.InstanceGuid;
                                        con.Add(pc);
                                    }
                                    else
                                    {
                                        //check to see if it is exported
                                        if (inpin.ParentNode != null)
                                        {
                                            PinConnection pc = new PinConnection();
                                            pc.ConnectedFrom = outpin.InstanceGuid;
                                            pc.ConnectedTo = inpin.InstanceGuid;
                                            con.Add(pc);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (includeExports)
                    {
                        foreach (DaggerOutputPin pin in ImportedPins)
                        {
                            foreach (DaggerInputPin inpin in pin.ConnectedTo)
                            {
                                if (inpin.ParentNode.SubgraphAffiliation == subgraph)
                                {
                                    PinConnection pc = new PinConnection();
                                    pc.ConnectedFrom = pin.InstanceGuid;
                                    pc.ConnectedTo = inpin.InstanceGuid;
                                    con.Add(pc);
                                }
                            }
                        }
                    }
                }

                return con;
            }
        }

        /// <summary>
        /// Get a list of all pin connections in this graph
        /// </summary>
        private List<PinConnection> AllGuidConnections
        {
            get
            {
                List<PinConnection> con = new List<PinConnection>();

                List<DaggerNode> allnodes = AllNodes;
                foreach (DaggerNode node in allnodes)
                {
                    foreach (DaggerOutputPin outpin in node.OutputPins)
                    {
                        foreach(Guid guid in outpin.ConnectedToGuids)
                        {
                            PinConnection pc = new PinConnection();
                            pc.ConnectedFrom = outpin.InstanceGuid;
                            pc.ConnectedTo = guid;
                            con.Add(pc);
                        }
                    }
                }

                foreach (DaggerOutputPin pin in ImportedPins)
                {
                    foreach (Guid guid in pin.ConnectedToGuids)
                    {
                        PinConnection pc = new PinConnection();
                        pc.ConnectedFrom = pin.InstanceGuid;
                        pc.ConnectedTo = guid;
                        con.Add(pc);
                    }
                }

                return con;
            }
        }

        private DaggerOutputPin FindOutputPinByGuid(Guid guid)
        {
            DaggerOutputPin pin = null;

            foreach (DaggerNode node in AllNodes)
            {
                pin = node.OutputPins[guid];
                if (pin != null)
                {
                    break;
                }
            }

            if (pin == null)
            {
                //look in the imported pins
                pin = ImportedPins[guid];
            }

            return pin;
        }

        private DaggerInputPin FindInputPinByGuid(Guid guid)
        {
            DaggerInputPin pin = null;

            foreach (DaggerNode node in AllNodes)
            {
                pin = node.InputPins[guid];
                if (pin != null)
                {
                    break;
                }
            }

            if (pin == null)
            {
                //look in the exported pins
                pin = ExportedPins[guid];
            }

            return pin;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Call the BeforeNodeRemoved Handler
        /// </summary>
        /// <param name="node"></param>
        /// <returns>true if node can be removed</returns>
        public virtual bool OnBeforeNodeRemoved(DaggerNode node)
        {
            if (BeforeNodeRemoved == null)
            {
                return true;
            }
            else
            {
                return BeforeNodeRemoved(node);
            }
        }

        /// <summary>
        /// Call before connect event to see if the two pins can be connected
        /// </summary>
        /// <param name="connectFrom"></param>
        /// <param name="connectTo"></param>
        /// <returns>true if pins can be connected</returns>
        public virtual bool OnBeforePinsConnected(DaggerOutputPin connectFrom, DaggerInputPin connectTo)
        {
            if (BeforePinsConnected == null)
            {
                return true;
            }
            else
            {
                return BeforePinsConnected(connectFrom, connectTo);
            }
        }

        /// <summary>
        /// Call before disconnect event to see if the two pins can be connected
        /// </summary>
        /// <param name="connectFrom"></param>
        /// <param name="connectTo"></param>
        /// <returns>true if pins can be disconnected</returns>
        public virtual bool OnBeforePinsDisconnected(DaggerOutputPin connectFrom, DaggerInputPin connectTo)
        {
            if (BeforePinsDisconnected == null)
            {
                return true;
            }
            else
            {
                return BeforePinsDisconnected(connectFrom, connectTo);
            }
        }

        void _exportedPins_PinAdded(object sender, DaggerBasePin pin)
        {
            OnExportPinAdded((DaggerInputPin)pin);
        }

        void _importedPins_PinAdded(object sender, DaggerBasePin pin)
        {
            OnImportPinAdded((DaggerOutputPin)pin);
        }

        public virtual void OnImportPinAdded(DaggerOutputPin pin)
        {
            if (_parentUIGraph != null)
            {
                _parentUIGraph.OnImportPinAdded(pin);
            }
        }

        public virtual void OnImportPinRemoved(DaggerOutputPin pin)
        {
            if (_parentUIGraph != null)
            {
                _parentUIGraph.OnImportPinRemoved(pin);
            }
        }

        public virtual void OnExportPinAdded(DaggerInputPin pin)
        {
            if (_parentUIGraph != null)
            {                
                _parentUIGraph.OnExportPinAdded(pin);
            }
        }

        public virtual void OnExportPinRemoved(DaggerInputPin pin)
        {
            if (_parentUIGraph != null)
            {
                _parentUIGraph.OnExportPinRemoved(pin);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Find and return an imported pin
        /// </summary>
        /// <param name="PinName">Name of Pin to find</param>
        /// <param name="PinDataType">Data Type of pin to find</param>
        /// <returns></returns>
        public DaggerOutputPin GetImportPin(string PinName, Type PinDataType)
        {
            DaggerOutputPin foundpin = null;

            foreach (DaggerOutputPin pin in _importedPins)
            {
                if (pin.Name == PinName && pin.DataType == PinDataType)
                {
                    foundpin = pin;
                    break;
                }
            }

            return foundpin;
        }

        /// <summary>
        /// Find and return an imported pin
        /// </summary>
        /// <param name="PinName">Name of Pin to find</param>
        /// <param name="PinDataType">Data Type of pin to find</param>
        /// <returns></returns>
        public DaggerInputPin GetExportPin(string PinName, Type PinDataType)
        {
            DaggerInputPin foundpin = null;

            foreach (DaggerInputPin pin in _exportedPins)
            {
                if (pin.Name == PinName && pin.DataType == PinDataType)
                {
                    foundpin = pin;
                    break;
                }
            }

            return foundpin;
        }

        /// <summary>
        /// Creates a Graph Scheduler of Type schedulertype for this Graph and all subnodes of the Graph
        /// </summary>
        /// <param name="schedulertype"></param>
        public void SetSchedulerType(Type schedulertype)
        {
            if (schedulertype.GetInterface("IGraphScheduler") == null)
            {
                throw new InvalidCastException("Type " + schedulertype + " does not implement interface IGraphScheduler");
            }

            GraphScheduler = (IGraphScheduler)Activator.CreateInstance(schedulertype);

            // propogate the scheduler type to subnodes
            foreach (DaggerNode node in AllNodes)
            {
                DaggerSubNode snode = node as DaggerSubNode;
                if (snode != null)
                {
                    snode.SubNodeGraph.SetSchedulerType(schedulertype);
                }
            }
        }

        private void AddImportPins(List<DaggerInterfacePin> importedPins)
        {
            foreach (DaggerInterfacePin pin in importedPins)
            {
                DaggerOutputPin outpin = new DaggerOutputPin();
                outpin.Name = pin.PinName;
                outpin.DataType = pin.PinDataType;
                outpin.AllowMultiConnect = true;
                outpin.IsInterfacePin = true;
                _importedPins.Add(outpin);
            }
        }

        private void AddExportPins(List<DaggerInterfacePin> exportedPins)
        {
            foreach (DaggerInterfacePin pin in exportedPins)
            {
                DaggerInputPin inpin = new DaggerInputPin();
                inpin.Name = pin.PinName;
                inpin.DataType = pin.PinDataType;
                inpin.IsInterfacePin = true;
                _exportedPins.Add(inpin);
            }
        }

        public DaggerOutputPin ExportPin(DaggerInputPin input)
        {
            DaggerOutputPin output = null;

            if (input.IsConnected)
            {
                return null;
            }

            //create an output pin to connect the input to
            output = new DaggerOutputPin();
            output.Name = input.Name;
            output.DataType = input.DataType;
            _importedPins.Add(output);
            output.ConnectToInput(input);

            // if this graph is inside a subnode, create an external pin
            if (_parentSubNode != null)
            {
                DaggerInputPin newinput = new DaggerInputPin();
                newinput.Name = input.Name;
                newinput.DataType = input.DataType;
                _parentSubNode.InputPins.Add(newinput);
                if (_parentSubNode.UINode != null)
                {
                    _parentSubNode.UINode.CalculateLayout();
                }
            }
            return output;
        }

        public DaggerInputPin ExportPin(DaggerOutputPin output)
        {
            DaggerInputPin input = null;

            if (!output.AllowMultiConnect && output.IsConnected)
            {
                return null;
            }

            //create an input pin to connect the output to
            input = new DaggerInputPin();
            input.Name = output.Name;
            input.DataType = output.DataType;
            _exportedPins.Add(input);
            output.ConnectToInput(input);

            // if this graph is inside a subnode, create an external pin
            if (_parentSubNode != null)
            {
                DaggerOutputPin newoutput = new DaggerOutputPin();
                newoutput.Name = output.Name;
                newoutput.DataType = output.DataType;
                newoutput.AllowMultiConnect = output.AllowMultiConnect;
                newoutput.PassByClone = output.PassByClone;
                _parentSubNode.OutputPins.Add(newoutput);
                if (_parentSubNode.UINode != null)
                {
                    _parentSubNode.UINode.CalculateLayout();
                }
            }
            return input;
        }

        /// <summary>
        /// Get the number of Ordinals in a subgraph
        /// </summary>
        /// <param name="subgraph"></param>
        /// <returns></returns>
        public int OrdinalCount(int subgraph)
        {
            if (subgraph > (_subGraphCount - 1))
            {
                throw new ArgumentOutOfRangeException();
            }

            List<DaggerNode> snode = this[subgraph];
            return snode[snode.Count - 1].Ordinal + 1;
        }

        /// <summary>
        /// Add a DaggerNode to this graph
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(DaggerNode node)
        {
            if (node.ParentGraph != null)
            {
                throw new InvalidOperationException("Node already belongs to a graph");
            }
            else
            {
                node.ParentGraph = this;
                _nodes.Add(node);

                CalculateTopology();
            }

            if (this.NodeAdded != null)
            {
                NodeAdded(node, new EventArgs());
            }
        }

        /// <summary>
        /// Serialize a subgraph in the DaggerGraph to a byte array
        /// </summary>
        /// <param name="subGraphToSerialize">subgraph index to serialize</param>
        /// <returns>An array of bytes containing a binary serialization of the subgraph</returns>
        public byte[] SerializeSubGraph(int subGraphToSerialize)
        {
            if (subGraphToSerialize < this.SubGraphCount)
            {
                _subGraphToSerialize = subGraphToSerialize;
            }

            MemoryStream ms = new MemoryStream();

            BinaryFormatter bformatter = new BinaryFormatter();

            bformatter.Serialize(ms, this);

            // set _subGraphToSerialize back to all
            _subGraphToSerialize = -1;

            if (ms.Length > 0)
            {
                ms.Close();
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Serialize the entire DaggerGraph to a byte array
        /// </summary>
        /// <returns>An array of bytes containing a binary serialization of the Graph</returns>
        public byte[] SerializeGraph()
        {
            // serialize the entire graph
            _subGraphToSerialize = -1;

            // create MemoryStream and Serialize via the Binaryformatter
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(ms, this);

            if (ms.Length > 0)
            {
                ms.Close();
                return ms.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Remove all nodes from a graph
        /// </summary>
        /// <returns></returns>
        public bool ClearGraph()
        {
            for (int i = AllNodes.Count - 1; i > -1; i--)
            {
                if(!DeleteNode(AllNodes[i]))
                {
                    return false;
                }
            }

            // clear out the import/export pins
            ExportedPins.Clear();
            ImportedPins.Clear();
            return true;
        }

        /// <summary>
        /// Removes a node from this graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns>true of succeeded</returns>
        public bool DeleteNode(DaggerNode node)
        {
            //try to disconnect all pins
            if (!node.DisconnectAllPins())
            {
                //we failed to disconnect a pin
                return false;
            }

            //see if we can remove this node from the graph
            if (!node.OnBeforeNodeRemoved())
            {
                //failed to remove node from graph
                return false;
            }
            if (!OnBeforeNodeRemoved(node))
            {
                //failed to remove node from graph
                return false;
            }

            //remove from the lists and the Controls
            _nodes.Remove(node);

            //let the node and the graph know it's toast so it can be removed from the ui also
            node.OnAfterNodeRemoved();

            if (AfterNodeRemoved != null)
            {
                AfterNodeRemoved(node);
            }

            CalculateTopology();

            return true;
        }

        /// <summary>
        /// Append DaggerGraph to this this Graph
        /// </summary>
        /// <param name="graph"></param>
        public void AppendGraph(DaggerGraph graph)
        {
            // keep track of node and noodles we've added so we can select them in UI mode
            List<IDaggerUINode> addedNodes = new List<IDaggerUINode>();
            List<IDaggerNoodle> addedNoodles = new List<IDaggerNoodle>();

            foreach (DaggerNode node in graph.AllNodes)
            {
                //disaffiliate the node from the old graph
                node.ParentGraph = null;
                node.UINode = null;

                AddNode(node);

                if (node.UINode != null)
                {
                    addedNodes.Add(node.UINode);
                }
            }

            foreach (DaggerOutputPin pin in graph.ImportedPins)
            {
                ImportedPins.Add(pin);

                //reflect imported pins to outter subnode
                if (_parentSubNode != null)
                {
                    DaggerInputPin newpin = new DaggerInputPin();
                    newpin.DataType = pin.DataType;
                    newpin.Name = pin.Name;
                    _parentSubNode.InputPins.Add(newpin);
                }

                //add noodles for imported pins
                if (_parentUIGraph != null)
                {
                    foreach (DaggerInputPin inpin in pin._connectedTo)
                    {
                        addedNoodles.Add(_parentUIGraph.AddNoodle(pin, inpin));
                    }
                }
            }

            foreach (DaggerInputPin pin in graph.ExportedPins)
            {
                ExportedPins.Add(pin);

                //reflect exported pins to outter subnode
                if (_parentSubNode != null)
                {
                    DaggerOutputPin newpin = new DaggerOutputPin();
                    newpin.DataType = pin.DataType;
                    newpin.Name = pin.Name;
                    _parentSubNode.OutputPins.Add(newpin);
                }
            }

            // update the UI of this DaggerGraph if one is attached
            if (_parentUIGraph != null)
            {
                _parentUIGraph.UpdateExportPins();
                _parentUIGraph.UpdateImportPins();

                foreach (DaggerNode node in graph.AllNodes)
                {
                    foreach (DaggerOutputPin outpin in node.OutputPins)
                    {
                        foreach (DaggerInputPin inpin in outpin.ConnectedTo)
                        {
                            addedNoodles.Add(_parentUIGraph.AddNoodle(outpin, inpin));
                        }
                    }
                }

                // if the appended graph had a stored layout apply it, offset by by location of the focus puck
                if (graph._layout != null && _parentUIGraph != null)
                {
                    foreach (GraphLayout gl in graph._layout)
                    {
                        DaggerNode node = null;

                        foreach (DaggerNode n in AllNodes)
                        {
                            if (n.InstanceGuid == gl.targetNodeGuid)
                            {
                                node = n;
                            }
                        }

                        if (node != null)
                        {
                            gl.Apply(node.UINode, _parentUIGraph.FocusLocationX,_parentUIGraph.FocusLocationY);
                        }
                    }
                }

                // if this graph is in subNode, update it's layout to include new pins
                if (_parentSubNode != null)
                {
                    _parentSubNode.UINode.CalculateLayout();
                }

                // create new selector for nodes and noodles we've added
                if (_parentUIGraph != null && addedNodes.Count != 0)
                {
                    _parentUIGraph.Select(addedNodes, addedNoodles);
                }
            }
        }

        #endregion

        #region Internal Methods
        
        /// <summary>
        /// Calculate the precedence of execution order and subgraph affiliation of each node
        /// </summary>
        internal void CalculateTopology()
        {
            //reset all presidence values and subgraph affiliations to -1
            foreach (DaggerNode node in _nodes)
            {
                node.Ordinal = -1;
                node.SubgraphAffiliation = -1;
                node._descendents.Clear();
            }

            // get the top level nodes
            List<DaggerNode> topLevelNodes = TopLevelNodes;

            // Create a List of Sets.  From the article "A Set Class" http://www.codeproject.com/csharp/Types_Set.asp
            // Each Set will hold a List of all Nodes that a top level Node touches on it's way to bottom level Nodes.
            // We'll in turn use these Sets to calculate subgraph affiliations.
            List<PIEBALD.Types.Set<DaggerNode>> touchedSetList = new List<PIEBALD.Types.Set<DaggerNode>>();

            // recursively walk each output pin marking the node's precedence and gathering a Set of all nodes it touches
            for (int i = 0; i < topLevelNodes.Count; i++)
            {
                DaggerNode node = topLevelNodes[i];

                // top level nodes always have an Ordinal of 0
                node.Ordinal = 0;

                // create a "Touched" Set
                PIEBALD.Types.Set<DaggerNode> touchedSet = new PIEBALD.Types.Set<DaggerNode>();

                foreach (DaggerOutputPin outpin in node.OutputPins)
                {
                    foreach (DaggerInputPin inpin in outpin._connectedTo)
                    {
                        // recurse through all it's connected pins
                        PIEBALD.Types.Set<DaggerNode> newset = _recurseCalculateTopology(1, inpin._parentNode, touchedSet);

                        // recreate our _descendents List from the newset
                        foreach (DaggerNode setnode in newset)
                        {
                            if (!node._descendents.Contains(setnode))
                            {
                                node._descendents.Add(setnode);
                            }
                        }
                        // sort the _descendents List by the Ordinal number
                        node._descendents.Sort(new OrdinalComparer());
                    }
                }

                // all top level nodes touch themselves <Let's keep it professional here>
                touchedSet.Add(node);

                // since we always have at least one subgraph, just add the first touched set to the list of sets
                if (i == 0)
                {
                    touchedSetList.Add(touchedSet);
                }
                else
                {
                    // if not the first Set, see if we can merge this Set with an existing touched Set
                    bool merged = false;
                    for (int u = 0; u < touchedSetList.Count; u++)
                    {
                        PIEBALD.Types.Set<DaggerNode> intersection = touchedSetList[u] & touchedSet;

                        // if the Cardinalty of the intersected Set is not 0, then these two Sets share the same subgraph
                        if (intersection.Cardinality > 0)
                        {
                            // merge the new Set with the stored one
                            touchedSetList[u] = touchedSetList[u] + touchedSet;
                            merged = true;
                            break;
                        }
                    }
                    if (!merged)
                    {
                        // we didn't find a Stored Set to merge with, so store this Set
                        touchedSetList.Add(touchedSet);
                    }
                }
            }

            // scrub through all the sets and mark the Subgraph tag of each node
            for (int i = 0; i < touchedSetList.Count; i++)
            {
                foreach (DaggerNode node in touchedSetList[i])
                {
                    node.SubgraphAffiliation = i;
                }
            }

            // let the graph know how many subgraphs it has
            _subGraphCount = touchedSetList.Count;

            // cache the connection status of the mutex pin groups for faster retrieval
            foreach (DaggerNode node in AllNodes)
            {
                PinMutexGroups allGroups = PinMutexGroups.None;

                // do the input pins
                foreach (DaggerInputPin pin in node.InputPins)
                {
                    //gather exising Mutex groups
                    allGroups |= pin.MutexGroup;
                }
                // clear the existing status dictionary
                node._inputMutexConnections.Clear();                
                if (allGroups != PinMutexGroups.None)
                {
                    int bitVal = 1;
                    for (int i = 0; i < 9; i++)
                    {
                        if (((int)allGroups & bitVal) != 0)
                        {
                            node._inputMutexConnections[(PinMutexGroups)bitVal] = node.IsInputMutexGroupsConnected((PinMutexGroups)bitVal);
                        }
                        bitVal = bitVal << 1;
                    }
                }

                allGroups = PinMutexGroups.None;
                // do the output pins
                foreach (DaggerOutputPin pin in node.OutputPins)
                {
                    // gather exising Mutex groups
                    allGroups |= pin.MutexGroup;
                }
                // clear the existing status dictionary
                node._outputMutexConnections.Clear();
                if (allGroups != PinMutexGroups.None)
                {
                    int bitVal = 1;
                    for (int i = 0; i < 9; i++)
                    {
                        if (((int)allGroups & bitVal) != 0)
                        {
                            node._outputMutexConnections[(PinMutexGroups)bitVal] = node.IsOutputMutexGroupsConnected((PinMutexGroups)bitVal);
                        }
                        bitVal = bitVal << 1;
                    }
                }
            }

            // let the Scheduler know the Topolgy has changed
            if (GraphScheduler != null)
            {
                GraphScheduler.OnTopologyChanged();
            }

            // call the OnTopologyChanged event
            if (OnTopologyChanged != null)
            {
                OnTopologyChanged(this, new EventArgs());
            }
        }

        private PIEBALD.Types.Set<DaggerNode> _recurseCalculateTopology(int level, DaggerNode node, PIEBALD.Types.Set<DaggerNode> touchedSet)
        {
            if (node == null)
            {
                // this was from an exported output pin, so return an empty set
                return new PIEBALD.Types.Set<DaggerNode>();
            }

            // set our precedence if level is larger than current value
            node.Ordinal = Math.Max(level, node.Ordinal);

            // create new set that will contain all my descendents
            PIEBALD.Types.Set<DaggerNode> newset = new PIEBALD.Types.Set<DaggerNode>();

            // recurse through all our output pins
            foreach (DaggerOutputPin outpin in node.OutputPins)
            {
                foreach (DaggerInputPin inpin in outpin._connectedTo)
                {
                    //recurse through all it's connected pins
                    newset.Add(_recurseCalculateTopology(level + 1, inpin._parentNode, touchedSet));
                }
            }

            // add this node to the touched Set (we don't want to be included in our own _descendents Set)
            touchedSet.Add(node);

            // recreate our _descendents List from the newset
            foreach (DaggerNode setnode in newset)
            {
                if (!node._descendents.Contains(setnode))
                {
                    node._descendents.Add(setnode);
                }
            }

            // sort the _descendents List by the Ordinal number
            node._descendents.Sort(new OrdinalComparer());

            // now add this node to the newset. 
            // We do this here because we don't want to include ourself in the _descendents List
            newset.Add(node);

            return newset;
        }

        internal void OnPinsConnected(DaggerOutputPin connectFrom, DaggerInputPin connectTo)
        {
            CalculateTopology();

            // call the after connect event
            if (AfterPinsConnected != null)
            {
                AfterPinsConnected(connectFrom, connectTo);
            }

            // if the pins belong to a mutex group update thier ui parent node's layout
            if (_parentUIGraph != null)
            {
                if (connectFrom.MutexGroup != PinMutexGroups.None)
                {
                    connectFrom.ParentNode.UINode.CalculateLayout();
                }

                if (connectTo.MutexGroup != PinMutexGroups.None)
                {
                    connectTo.ParentNode.UINode.CalculateLayout();
                }

                //update noodles to reflect the repositioning of available pins
                _parentUIGraph.UpdateNoodles(connectFrom.ParentNode);
                _parentUIGraph.UpdateNoodles(connectTo.ParentNode);
            }
        }

        internal void OnPinsDisonnected(DaggerOutputPin disconnectOutput, DaggerInputPin disconnectInput)
        {
            //if the pins are imported or exported, see if we need to remove them from the list
            if (_exportedPins.Contains(disconnectInput) && !disconnectInput.IsInterfacePin)
            {
                //before we remove the pin, propogate the disconnection of pins upward through parent nodes
                if (_parentSubNode != null)
                {
                    if (_parentSubNode.OutputPins[_exportedPins.IndexOf(disconnectInput)].Disconnect(false))
                    {
                        //delete outter pin
                        _parentSubNode.OutputPins.Remove(_parentSubNode.OutputPins[_exportedPins.IndexOf(disconnectInput)]);
                        //delete inner pin
                        _exportedPins.Remove(disconnectInput);
                        OnExportPinRemoved(disconnectInput);
                    }
                    else
                    {
                        //we couldn't disconnect an outter exported pin
                        return;
                    }
                }
                else
                {
                    _exportedPins.Remove(disconnectInput);
                    OnExportPinRemoved(disconnectInput);
                }
            }
            else if(_importedPins.Contains(disconnectOutput) && !disconnectOutput.IsInterfacePin)
            {
                // if no more connections remain on this imported pin, begin removal/disconnection propogation
                if (!disconnectOutput.IsConnected)
                {
                    if (_parentSubNode != null)
                    {
                        // propogate the disconnection upwards first
                        if (_parentSubNode.InputPins[_importedPins.IndexOf(disconnectOutput)].Disconnect(false))
                        {
                            // delete outter pin
                            _parentSubNode.InputPins.Remove(_parentSubNode.InputPins[_importedPins.IndexOf(disconnectOutput)]);
                            // delete inner pin
                            _importedPins.Remove(disconnectOutput);
                            OnImportPinRemoved(disconnectOutput);
                        }
                        else
                        {
                            // couldn't disconnect the outter imported pin
                            return;
                        }
                    }
                    else
                    {
                        // not embedded in a SubNode, so just delete the inner pin
                        _importedPins.Remove(disconnectOutput);
                        OnImportPinRemoved(disconnectOutput);
                    }
                }
            }

            CalculateTopology();

            // call the after disconnect event
            if (AfterPinsDisconnected != null)
            {
                AfterPinsDisconnected(disconnectOutput, disconnectInput);
            }

            // refresh the UISubNode to remove the outter pin regions
            if (_parentSubNode != null && _parentSubNode.UINode != null)
            {
                _parentSubNode.UINode.CalculateLayout();
            }

            // if the pins belong to a mutex group update thier ui parent node's layout
            if (_parentUIGraph != null)
            {
                if (disconnectOutput.MutexGroup != PinMutexGroups.None)
                {
                    disconnectOutput.ParentNode.UINode.CalculateLayout();
                }

                if (disconnectInput.MutexGroup != PinMutexGroups.None)
                {
                    disconnectInput.ParentNode.UINode.CalculateLayout();
                }

                // update noodles to reflect the repositioning of available pins
                _parentUIGraph.UpdateNoodles(disconnectOutput.ParentNode);
                _parentUIGraph.UpdateNoodles(disconnectInput.ParentNode);
            }
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            byte[] buffer = SerializeGraph();
            MemoryStream ms = new MemoryStream(buffer);
            BinaryFormatter bformatter = new BinaryFormatter();
            return bformatter.Deserialize(ms);
        }

        #endregion

        #region ISerializable

        /// <summary>
        /// Serialization GetObjectData.  DaggerGraph can serialize in one of 3 states: All, Subgraph, and Selected
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            List<DaggerNode> nodes = new List<DaggerNode>();
            List<DaggerNodeNonSerializationAssistant> assistNodes = new List<DaggerNodeNonSerializationAssistant>();

            // storage for the layout of ui nodes
            List<GraphLayout> layout = (_parentUIGraph != null) ? new List<GraphLayout>() : null;

            // are we serializing the entire graph, a subgraph or a selection?
            if (_selectionToSerialize != null)
            {
                // serialize a selection
                foreach (IDaggerUINode uinode in _selectionToSerialize.SelectedNodes)
                {
                    DaggerNode node = uinode.Node;

                    if (layout != null)
                    {
                        // store the uinode's layout
                        layout.Add(new GraphLayout(node.UINode));
                    }

                    if (node.GetType().IsSerializable)
                    {
                        nodes.Add(node);
                    }
                    else
                    {
                        assistNodes.Add(new DaggerNodeNonSerializationAssistant(node));
                    }
                }

                // add only the connections valid for the nodes that are selected
                info.AddValue("AllConnections", this[_selectionToSerialize]);
            }
            else if (_subGraphToSerialize == -1)
            {
                // serialize the entire graph
                foreach (DaggerNode node in AllNodes)
                {
                    if (layout != null)
                    {
                        // store the uinode's layout
                        layout.Add(new GraphLayout(node.UINode));
                    }

                    if (node.GetType().IsSerializable)
                    {
                        nodes.Add(node);
                    }
                    else
                    {
                        assistNodes.Add(new DaggerNodeNonSerializationAssistant(node));
                    }
                }

                info.AddValue("AllConnections", AllGuidConnections);
            }
            else
            {
                // serialize a subgraph
                foreach (DaggerNode node in this[_subGraphToSerialize])
                {
                    if (layout != null)
                    {
                        // store the uinode's layout
                        layout.Add(new GraphLayout(node.UINode));
                    }

                    if (node.GetType().IsSerializable)
                    {
                        nodes.Add(node);
                    }
                    else
                    {
                        assistNodes.Add(new DaggerNodeNonSerializationAssistant(node));
                    }
                }

                info.AddValue("AllConnections", this[_subGraphToSerialize, true]);
            }            

            // store nodes and assisted nodes
            info.AddValue("AllNodes", nodes);
            info.AddValue("AssistNodes", assistNodes);

            if (layout != null)
            {
                // store the layout
                info.AddValue("Layout", layout);
            }

            List<DaggerInputPin> exlist = new List<DaggerInputPin>();
            foreach (DaggerInputPin pin in ExportedPins)
            {
                if (pin.IsConnected)
                {
                    if (_selectionToSerialize != null)
                    {
                        // partial graph is being serialized.  Make sure pin is part of selection
                        foreach (PinConnection pc in this[_selectionToSerialize])
                        {
                            if (pc.ConnectedTo == pin.InstanceGuid)
                            {
                                exlist.Add(pin);
                                break;
                            }
                        }
                    }
                    else if (_subGraphToSerialize == -1)
                    {
                        // the entire graph is being serialized
                        exlist.Add(pin);
                    }
                    else
                    {
                        // Subgraph is being serialize.  Make sure it's part of the subgraph
                        if (pin.ConnectedToOutput.ParentNode.SubgraphAffiliation == _subGraphToSerialize)
                        {
                            exlist.Add(pin);
                        }
                    }
                }
            }

            List<DaggerOutputPin> inlist = new List<DaggerOutputPin>();
            foreach (DaggerOutputPin pin in ImportedPins)
            {
                if (_selectionToSerialize != null)
                {
                    // partial graph is being serialized.  Make sure pin is part of selection
                    foreach (PinConnection pc in this[_selectionToSerialize])
                    {
                        if (pc.ConnectedFrom == pin.InstanceGuid)
                        {
                            inlist.Add(pin);
                            break;
                        }
                    }
                }
                else if (_subGraphToSerialize == -1)
                {
                    // the entire graph is being serialized
                    inlist.Add(pin);
                }
                else
                {
                    // Subgraph is being serialize.  Make sure it's part of the subgraph
                    DaggerInputPin subgraphpin = null;
                    foreach (DaggerInputPin inpin in pin._connectedTo)
                    {
                        if (inpin.ParentNode.SubgraphAffiliation == _subGraphToSerialize)
                        {
                            subgraphpin = inpin;
                            break;
                        }
                    }
                    if (subgraphpin != null)
                    {
                        inlist.Add(pin);
                    }
                }
            }

            info.AddValue("ImportedPins", inlist);
            info.AddValue("ExportedPins", exlist);
        }

        /// <summary>
        /// Callback method before Deserialization begins
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing()]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            // not implemented yet
        }

        /// <summary>
        /// Callback method after Deserialization completes
        /// </summary>
        /// <param name="ctxt"></param>
        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext ctxt)
        {
            if (_isDeserialized)
            {
                //already been here
                return;
            }

            // Create default scheduler and give the scheduler this Graph
            _scheduler = new OrdinalExecutionScheduler();
            _scheduler.Graph = this;

            // mark this flag to prevent Pins from AutoCloning during the reconnection phase of deserialization
            _isDeserializing = true;

            // recreate nodes that need assistance
            if (_assistNodes.Count > 0)
            {
                foreach (DaggerNodeNonSerializationAssistant assist in _assistNodes)
                {
                    try
                    {
                        DaggerNode node = assist.CreateNode();
                        _nodes.Add(node);
                    }
                    catch (Exception ex)
                    {
                        throw new SerializationException("Error Assisting Deserialization of DaggerNode Type " + assist.NodeType.ToString(),
                            ex);
                    }
                }
            }

            // associate all the nodes
            foreach (DaggerNode node in AllNodes)
            {
                node.ParentGraph = this;
                if (node is DaggerSubNode)
                {
                    // we need to reverse the order of ondeserialization to ensure all nested
                    // subnodes are reconstructed from the bottom up
                    (node as DaggerSubNode)._subNodeGraph.OnDeserializedMethod(ctxt);
                    (node as DaggerSubNode).OnDeserializedMethod(ctxt);
                }
            }

            // add the imported and exported pins to thier collections
            foreach (DaggerOutputPin pin in inlist)
            {
                _importedPins.Add(pin);
            }
            foreach (DaggerInputPin pin in exlist)
            {
                _exportedPins.Add(pin);
            }

            // reconstruct the pin connections after deserialization is completed
            foreach (PinConnection con in _allConnections)
            {
                DaggerOutputPin outpin = FindOutputPinByGuid(con.ConnectedFrom);
                DaggerInputPin inpin = FindInputPinByGuid(con.ConnectedTo);

                if (outpin == null || inpin == null)
                {
                    throw new InvalidDataException("Pin not found.  DaggerNode not properly serialized.");
                }
                outpin.ConnectToInput(inpin);
            }

            // create new InstanceGuids for all the pins and nodes (prevents collisions when merging graphs)
            foreach (DaggerNode node in AllNodes)
            {
                foreach (DaggerInputPin pin in node.InputPins)
                {
                    pin._instanceGuid = Guid.NewGuid();
                }

                foreach (DaggerOutputPin pin in node.OutputPins)
                {
                    pin._instanceGuid = Guid.NewGuid();
                }

                Guid newNodeGuid = Guid.NewGuid();

                // if there is a layout stored, set the node's layout targetguid
                if (_layout != null)
                {
                    foreach(GraphLayout gl in _layout)
                    {
                        if (gl.targetNodeGuid == node.InstanceGuid)
                        {
                            gl.targetNodeGuid = newNodeGuid;
                            break;
                        }
                    }
                }

                node.InstanceGuid = newNodeGuid;
            }

            foreach (DaggerOutputPin pin in ImportedPins)
            {
                pin._instanceGuid = Guid.NewGuid();
            }

            foreach (DaggerInputPin pin in ExportedPins)
            {
                pin._instanceGuid = Guid.NewGuid();
            }

            _isDeserializing = false;
            _isDeserialized = true;
        }

        #endregion
    }

    #region PinConnection Class
    /// <summary>
    /// Class that represents the connection of two pins
    /// </summary>
    [Serializable]
    public class PinConnection
    {
        public Guid ConnectedFrom = Guid.Empty;
        public Guid ConnectedTo = Guid.Empty;

        public DaggerOutputPin OutputPin;
        public DaggerInputPin InputPin;

        public PinConnection()
        {

        }

        public PinConnection(DaggerOutputPin outpin, DaggerInputPin inpin)
        {
            OutputPin = outpin;
            InputPin = inpin;
        }
    }

    #endregion

    #region OrdinalComparer class

    /// <summary>
    /// Comparer class to sort DaggerNodes by thier Ordinal Number
    /// </summary>
    internal class OrdinalComparer : IComparer<DaggerNode>
    {
        public int Compare(DaggerNode x, DaggerNode y)
        {
            int result = 0;

            if (x == null && y == null)
            {
                result = 0;
            }
            else if (x == null)
            {
                result = -1;
            }
            else if (y == null)
            {
                result = 1;
            }
            else
            {
                if (x.Ordinal < y.Ordinal)
                {
                    result = -1;
                }
                else if (x.Ordinal == y.Ordinal)
                {
                    result = 0;
                }
                else result = 1;
            }

            return result;
        }
    }

    #endregion
}
