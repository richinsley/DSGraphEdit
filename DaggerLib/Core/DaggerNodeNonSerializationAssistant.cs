using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace DaggerLib.Core
{
    [Serializable]
    internal class DaggerNodeNonSerializationAssistant : ISerializable
    {
        public List<DaggerInputPin> InputPins;
        public List<DaggerOutputPin> OutputPins;
        public Type NodeType;
        public Guid NodeInstanceGuid;

        public DaggerNodeNonSerializationAssistant(DaggerNode node)
        {
            NodeType = node.GetType();

            NodeInstanceGuid = node.InstanceGuid;

            InputPins = new List<DaggerInputPin>();
            foreach (DaggerInputPin pin in node.InputPins)
            {
                InputPins.Add(pin);
            }

            OutputPins = new List<DaggerOutputPin>();
            foreach (DaggerOutputPin pin in node.OutputPins)
            {
                OutputPins.Add(pin);
            }

            FieldInfo[] fi = NodeType.GetFields();

            for (int i = 0; i < fi.Length; i++)
            {
                if (fi[i].FieldType.IsSubclassOf(typeof(DaggerBasePin)))
                {
                    DaggerBasePin val = (DaggerBasePin)fi[i].GetValue(node);

                    if (val != null)
                    {
                        //if this pin is stored in the collection, serialize it's reflected FieldInfo
                        if (node.InputPins[val.InstanceGuid] != null || node.OutputPins[val.InstanceGuid] != null)
                        {
                            val._reflectedTargets.Add(fi[i]);
                        }
                    }
                }
            }
        }

        public DaggerNode CreateNode()
        {
            DaggerNode node = (DaggerNode)Activator.CreateInstance(NodeType);

            node.InputPins.Clear();
            node.OutputPins.Clear();
            
            //set it'a InstanceGuid
            node.InstanceGuid = NodeInstanceGuid;

            //give it the pins
            foreach (DaggerInputPin pin in InputPins)
            {
                node.InputPins.Add(pin);
            }

            foreach (DaggerOutputPin pin in OutputPins)
            {
                node.OutputPins.Add(pin);
            }

            // put pins into thier reflected fields
            foreach (DaggerInputPin pin in InputPins)
            {
                foreach (FieldInfo fi in pin._reflectedTargets)
                {
                    fi.SetValue(node, pin);
                }
            }
            foreach (DaggerOutputPin pin in OutputPins)
            {
                foreach (FieldInfo fi in pin._reflectedTargets)
                {
                    fi.SetValue(node, pin);
                }
            }

            return node;
        }

        public DaggerNodeNonSerializationAssistant(SerializationInfo info, StreamingContext ctxt)
        {
            NodeType = (Type)info.GetValue("NodeType", typeof(Type));
            InputPins = (List<DaggerInputPin>)info.GetValue("InputPins", typeof(List<DaggerInputPin>));
            OutputPins = (List<DaggerOutputPin>)info.GetValue("OutputPins", typeof(List<DaggerOutputPin>));
            NodeInstanceGuid = (Guid)info.GetValue("NodeGuid", typeof(Guid));
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("NodeType", NodeType);
            info.AddValue("NodeGuid", NodeInstanceGuid);
            info.AddValue("InputPins", InputPins);
            info.AddValue("OutputPins", OutputPins);
        }
    }
}
