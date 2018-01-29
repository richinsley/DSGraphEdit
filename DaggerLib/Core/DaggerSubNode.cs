using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    /// <summary>
    /// Node that acts as a nexus into and out of a Subroutine Graph
    /// </summary>
    [Serializable]
    public class DaggerSubNode : DaggerNode, ISerializable
    {
        //the internal graph this subnode containes
        internal DaggerGraph _subNodeGraph;

        private string _subNodeName = "SubNode";

        List<DaggerInputPin> inputpins;
        List<DaggerOutputPin> outputpins;

        private bool _isDeserialized = false;

        public DaggerSubNode()
        {
            _subNodeGraph = new DaggerGraph();
            _subNodeGraph._parentSubNode = this;

            AssociatedUINode = "IDaggerUISubNode";
        }

        public DaggerSubNode(DaggerGraph graph) 
            : base()
        {
            _subNodeGraph = graph;
            _subNodeGraph._parentSubNode = this;

            AssociatedUINode = "IDaggerUISubNode";

            foreach (DaggerOutputPin pin in _subNodeGraph.ImportedPins)
            {
                DaggerInputPin inpin = new DaggerInputPin();
                inpin.Name = pin.Name;
                inpin.DataType = pin.DataType;
                InputPins.Add(inpin);
            }

            foreach (DaggerInputPin pin in _subNodeGraph.ExportedPins)
            {
                DaggerOutputPin outpin = new DaggerOutputPin();
                outpin.Name = pin.Name;
                outpin.DataType = pin.DataType;
                OutputPins.Add(outpin);
            }
        }

        /// <summary>
        /// Create a new subnode from a serialized graph
        /// </summary>
        /// <param name="buffer"></param>
        public DaggerSubNode(string name,byte[] buffer)
            : base()
        {
            MemoryStream ms = new MemoryStream(buffer);
            BinaryFormatter bformatter = new BinaryFormatter();

            _subNodeGraph = (DaggerGraph)bformatter.Deserialize(ms);
            _subNodeName = name;
            _subNodeGraph._parentSubNode = this;

            // reflect imported/exported pins to input/output pins
            foreach (DaggerOutputPin pin in _subNodeGraph.ImportedPins)
            {
                DaggerInputPin inpin = new DaggerInputPin();
                inpin.Name = pin.Name;
                inpin.DataType = pin.DataType;
                InputPins.Add(inpin);
            }
            foreach (DaggerInputPin pin in _subNodeGraph.ExportedPins)
            {
                DaggerOutputPin outpin = new DaggerOutputPin();
                outpin.Name = pin.Name;
                outpin.DataType = pin.DataType;
                OutputPins.Add(outpin);
            }

            AssociatedUINode = "IDaggerUISubNode";
        }

        protected DaggerSubNode(SerializationInfo info, StreamingContext ctxt) : base(info,ctxt)
        {
            _subNodeName = (string)info.GetValue("SubnodeName", typeof(string));
            _subNodeGraph = (DaggerGraph)info.GetValue("SubGraph",typeof(DaggerGraph));

            inputpins = (List<DaggerInputPin>)info.GetValue("InputPins", typeof(List<DaggerInputPin>));
            outputpins = (List<DaggerOutputPin>)info.GetValue("OutputPins",typeof(List<DaggerOutputPin>));

            AssociatedUINode = "IDaggerUISubNode";
        }

        public string SubNodeName
        {
            get
            {
                return _subNodeName;
            }
            set
            {
                _subNodeName = value;
            }
        }

        public DaggerGraph SubNodeGraph
        {
            get
            {
                return _subNodeGraph;
            }
        }

        public override string ToString()
        {
            return _subNodeName;
        }

        public override void DoProcessing()
        {
            for (int i = 0; i < InputPins.Count; i++)
            {
                _subNodeGraph.ImportedPins[i].Data = InputPins[i].Data;
            }

            _subNodeGraph.GraphScheduler.ProcessGraph();

            for (int i = 0; i < OutputPins.Count; i++)
            {
                OutputPins[i].Data = _subNodeGraph.ExportedPins[i].Data;
            }
        }

        /// <summary>
        /// Callback method after Deserialization completes
        /// </summary>
        /// <param name="ctxt"></param>
        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext ctxt)
        {
            if (_isDeserialized)
            {
                return;
            }

            foreach (DaggerInputPin pin in inputpins)
            {
                InputPins.Add(pin);
            }

            foreach (DaggerOutputPin pin in outputpins)
            {
                OutputPins.Add(pin);
            }

            // associate the subNodeGraph with this subNode
            _subNodeGraph._parentSubNode = this;

            _isDeserialized = true;
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("SubGraph", _subNodeGraph);

            info.AddValue("SubnodeName", _subNodeName);

            inputpins = new List<DaggerInputPin>();
            foreach (DaggerInputPin pin in InputPins)
            {
                inputpins.Add(pin);
            }

            outputpins = new List<DaggerOutputPin>();
            foreach (DaggerOutputPin pin in OutputPins)
            {
                outputpins.Add(pin);
            }

            info.AddValue("InputPins", inputpins);
            info.AddValue("OutputPins", outputpins);

            base.GetObjectData(info, ctxt);
        }
    }
}
