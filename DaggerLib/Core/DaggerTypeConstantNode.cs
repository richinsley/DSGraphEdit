using System;
using System.Collections.Generic;
using System.Text;
using DaggerLib.UI;

using System.Runtime.Serialization;

namespace DaggerLib.Core
{
    [Serializable]
    public class DaggerTypeConstantNode : DaggerNode, ISerializable
    {
        public DaggerInputPin inpin;
        public DaggerOutputPin outpin;
        public Type DataType = typeof(object);

        public DaggerTypeConstantNode(Type constantType)
        {
            inpin = new DaggerInputPin();
            inpin.DataType = constantType;
            inpin.Name = "Constant Input";
            InputPins.Add(inpin);

            outpin = new DaggerOutputPin();
            outpin.DataType = constantType;
            OutputPins.Add(outpin);

            AssociatedUINode = typeof(TypeConstantNodeUI);
            DoProcessing += new ProcessHandler(DaggerTypeConstantNode_DoProcessing);
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        protected DaggerTypeConstantNode(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            DataType = (Type)info.GetValue("DataType", typeof(Type));

            inpin = (DaggerInputPin)info.GetValue("InPin", typeof(DaggerInputPin));
            InputPins.Add(inpin);

            outpin = (DaggerOutputPin)info.GetValue("OutPin", typeof(DaggerOutputPin));
            OutputPins.Add(outpin);            

            AssociatedUINode = typeof(TypeConstantNodeUI);
            DoProcessing += new ProcessHandler(DaggerTypeConstantNode_DoProcessing);
        }

        void DaggerTypeConstantNode_DoProcessing(object sender)
        {
            if (inpin.Data != null)
            {
                outpin.Data = inpin.Data;

                //if we have a UI node assigned, set the GenericValueEditor to the incomming value
                if (UINode != null)
                {
                    (UINode as TypeConstantNodeUI).genericValueEditor.Value = inpin.Data;
                }
            }
        }

        #region ISerializable

        //Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("DataType", DataType);

            //Serialize our two pins
            info.AddValue("InPin", inpin);
            info.AddValue("OutPin", outpin);

            //Do the base's Serialization
            base.GetObjectData(info, ctxt);
        }

        #endregion
    }
}
