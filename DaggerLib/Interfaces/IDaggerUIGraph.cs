using System;
using System.Collections.Generic;
using System.Text;

using DaggerLib.Core;
using DaggerLib.UI;

namespace DaggerLib.Interfaces
{
    public interface IDaggerUIGraph
    {
        DaggerGraph Graph
        {
            get;
        }

        string DefaultAssociatedDaggerUINode
        {
            get;
        }

        int FocusLocationX
        {
            get;
        }

        int FocusLocationY
        {
            get;
        }

        int PinSize
        {
            get;
            set;
        }

        List<IDaggerUINode> AllNodes
        {
            get;
        }

        void AddNode(DaggerNode node);
        IDaggerNoodle AddNoodle(DaggerOutputPin output, DaggerInputPin input);
        void UpdateNoodles(DaggerNode bn);
        void UpdateImportPins();
        void UpdateExportPins();
        void Select(List<IDaggerUINode> nodes,List<IDaggerNoodle> noodles);

        void OnAfterNodeRemoved(IDaggerUINode uinode);
        void OnImportPinAdded(DaggerOutputPin pin);
        void OnImportPinRemoved(DaggerOutputPin pin);
        void OnExportPinAdded(DaggerInputPin pin);
        void OnExportPinRemoved(DaggerInputPin pin);
        void RefreshGraph();
    }
}
