using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DaggerLib.UI.Windows
{
    public partial class TypeConstantNodeUI : DaggerUINode
    {
        public TypeConstantNodeUI()
        {
            InitializeComponent();
            DaggerNodeAttached += new DaggerLib.Core.DaggerNodeAttachedHandler(TypeConstantNodeUI_DaggerNodeAttached);
        }

        void TypeConstantNodeUI_DaggerNodeAttached(DaggerLib.Core.DaggerNode node)
        {
            //set the data type for the GenericValueEditor
            genericValueEditor.EditedType = InputPins[0].DataType;

            //if there is data already present, set the textbox
            if (InputPins[0].Data != null)
            {
                genericValueEditor.Value = InputPins[0].Data;
            }

            //set the caption to the Output Data Type Code
            CaptionText = OutputPins[0].DataType.FullName;
        }

        private void genericValueEditor_ValueChanged(object sender, EventArgs e)
        {
            OutputPins[0].Data = genericValueEditor.Value;
            Node.Process();
        }
    }
}