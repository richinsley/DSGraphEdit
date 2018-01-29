using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using PropertyGridEx;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    public partial class DaggerGraphPropertyGrid : UserControl
    {
        private DaggerUIGraph _uiGraph;
        private DaggerInterface _pinInterface;

        /// <summary>
        /// Constructor for interacting with a DaggerUIGraph
        /// </summary>
        public DaggerGraphPropertyGrid()
        {
            InitializeComponent();
            propertyGridEx1.PropertyValueChanged += new PropertyValueChangedEventHandler(propertyGridEx1_PropertyValueChanged);
        }

        /// <summary>
        /// constructor that allows the setting data of Imported that are not implemented in a Given DaggerInterface
        /// </summary>
        /// <param name="Graph"></param>
        /// <param name="pinInterface"></param>
        public DaggerGraphPropertyGrid(DaggerGraph Graph, DaggerInterface pinInterface)
        {
            InitializeComponent();
            comboBox1.Visible = false;
            propertyGridEx1.PropertyValueChanged += new PropertyValueChangedEventHandler(propertyGridEx1_PropertyValueChangedNoProcess);
            propertyGridEx1.ShowCustomProperties = true;

            // populate the properties with the imported pins that are not part of the DaggerInterface
            foreach (DaggerOutputPin pin in Graph.ImportedPins)
            {
                if(!pinInterface.ContainsImportPin(new DaggerInterfacePin(pin.Name,pin.DataType)))
                {
                    AddPin("Import Pins","",pin);
                }
            }

            propertyGridEx1.Refresh();
        }

        public PropertyGridEx.PropertyGridEx PropertyGridEx
        {
            get
            {
                return propertyGridEx1;
            }
        }

        /// <summary>
        /// Called when a user sets a value.  Does not process the Graph
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        void propertyGridEx1_PropertyValueChangedNoProcess(object s, PropertyValueChangedEventArgs e)
        {
            PropertyGridEx.CustomProperty cp = (PropertyGridEx.CustomProperty)((PropertyGridEx.CustomProperty.CustomPropertyDescriptor)(e.ChangedItem.PropertyDescriptor)).CustomProperty;
            if (cp.Tag != null)
            {
                // set the pin's Data via it's property so the Data can be transmited through the 
                // proper channels
                DaggerBasePin pin = cp.Tag as DaggerBasePin;
                pin.Data = cp.Value;
            }
        }

        /// <summary>
        /// Called when a user sets a value
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        void propertyGridEx1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (comboBox1.SelectedIndex != 0)
            {
                PropertyGridEx.CustomProperty cp = (PropertyGridEx.CustomProperty)((PropertyGridEx.CustomProperty.CustomPropertyDescriptor)(e.ChangedItem.PropertyDescriptor)).CustomProperty;
                if (cp.Tag != null)
                {
                    // set the pin's Data via it's property so the Data can be transmited through the 
                    // proper channels
                    DaggerBasePin pin = cp.Tag as DaggerBasePin;
                    pin.Data = cp.Value;

                    // process the graph
                    if (pin.ParentNode != null)
                    {
                        pin.ParentNode.ParentGraph.GraphScheduler.ProcessGraph(pin.ParentNode);
                    }
                    else
                    {
                        // process the entire graph if it is an imported pin
                        if (pin is DaggerOutputPin)
                        {
                            pin.ParentGraph.GraphScheduler.ProcessGraph();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the DaggerUIGraph this PropertyGrid operates on
        /// </summary>
        public DaggerUIGraph DaggerUIGraph
        {
            get
            {
                return _uiGraph;
            }
            set
            {
                // if there was a previous graph, unhook our events
                if (_uiGraph != null)
                {
                    try
                    {
                        _uiGraph.Graph.NodeAdded -= new EventHandler(Graph_NodeAdded);
                        _uiGraph.Graph.AfterNodeRemoved -= new AfterNodeRemoveHandler(Graph_AfterNodeRemoved);
                        _uiGraph.Graph.ImportedPins.PinAdded -= new DaggerPinAdded(importexport_PinAdded);
                        _uiGraph.Graph.ImportedPins.PinRemoved -= new DaggerPinRemoved(importexport_PinRemoved);
                        _uiGraph.Graph.ExportedPins.PinAdded -= new DaggerPinAdded(importexport_PinAdded);
                        _uiGraph.Graph.ExportedPins.PinRemoved -= new DaggerPinRemoved(importexport_PinRemoved);

                        foreach (DaggerNode node in _uiGraph.Graph.AllNodes)
                        {
                            node.InputPins.PinAdded -= new DaggerPinAdded(node_PinAdded);
                            node.OutputPins.PinAdded -= new DaggerPinAdded(node_PinAdded);
                            node.InputPins.PinRemoved -= new DaggerPinRemoved(node_PinRemoved);
                            node.OutputPins.PinRemoved -= new DaggerPinRemoved(node_PinRemoved);
                        }
                    }
                    catch
                    {
                        // graph was disposed
                    }
                }

                _uiGraph = value;
                InitDropDownItems();
                if (_uiGraph != null)
                {
                    // hook the graphs events that we'll need
                    _uiGraph.Graph.NodeAdded += new EventHandler(Graph_NodeAdded);
                    _uiGraph.Graph.AfterNodeRemoved += new AfterNodeRemoveHandler(Graph_AfterNodeRemoved);
                    _uiGraph.Graph.ImportedPins.PinAdded += new DaggerPinAdded(importexport_PinAdded);
                    _uiGraph.Graph.ImportedPins.PinRemoved += new DaggerPinRemoved(importexport_PinRemoved);
                    _uiGraph.Graph.ExportedPins.PinAdded += new DaggerPinAdded(importexport_PinAdded);
                    _uiGraph.Graph.ExportedPins.PinRemoved += new DaggerPinRemoved(importexport_PinRemoved);
                }
            }
        }

        void Graph_NodeAdded(object sender, EventArgs e)
        {
            DaggerNode node = sender as DaggerNode;
            comboBox1.Items.Add(new DropDownItem(node.ToString() + " -" + node.GetType().ToString(), node));
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;

            // hook the pin add/remove events
            node.InputPins.PinAdded += new DaggerPinAdded(node_PinAdded);
            node.OutputPins.PinAdded += new DaggerPinAdded(node_PinAdded);
            node.InputPins.PinRemoved += new DaggerPinRemoved(node_PinRemoved);
            node.OutputPins.PinRemoved += new DaggerPinRemoved(node_PinRemoved);

            (node.UINode as DaggerUINode).Activated += new EventHandler(DaggerGraphPropertyGrid_Activated);
        }

        void DaggerGraphPropertyGrid_Activated(object sender, EventArgs e)
        {
            foreach (DropDownItem item in comboBox1.Items)
            {
                if ((item.Tag is DaggerNode) && item.Tag == (sender as DaggerUINode).Node)
                {
                    comboBox1.SelectedItem = item;
                    break;
                }
            }
        }

        void Graph_AfterNodeRemoved(DaggerNode node)
        {
            // if this node is currently selected, select the Graph Data item
            if (((DropDownItem)comboBox1.SelectedItem).Tag == node)
            {
                comboBox1.SelectedIndex = 1;
            }

            foreach (DropDownItem item in comboBox1.Items)
            {
                if (item.Tag == node)
                {
                    comboBox1.Items.Remove(item);
                    break;
                }
            }
        }

        #region Pin Add/Remove Handlers

        void importexport_PinRemoved(object sender, DaggerBasePin pin)
        {
            if (comboBox1.SelectedIndex == 1)
            {
                RemovePin(pin);
            }
        }

        void importexport_PinAdded(object sender, DaggerBasePin pin)
        {
            if (comboBox1.SelectedIndex == 1)
            {
                string cat = (pin is DaggerOutputPin) ? "Imported Pins" : "Exported Pins";
                AddPin(cat, "", pin);
                propertyGridEx1.Refresh();
            }
        }

        void node_PinRemoved(object sender, DaggerBasePin pin)
        {
            DropDownItem item = comboBox1.SelectedItem as DropDownItem;
            if (item.Tag == pin.ParentNode)
            {
                RemovePin(pin);
            }
        }

        void node_PinAdded(object sender, DaggerBasePin pin)
        {
            DropDownItem item = comboBox1.SelectedItem as DropDownItem;
            if (item.Tag == pin.ParentNode)
            {
                string cat = (pin is DaggerOutputPin) ? "Output Pins" : "Input Pins";
                AddPin(cat, "", pin);
                propertyGridEx1.Refresh();
            }
        }

        #endregion

        private void InitDropDownItems()
        {
            comboBox1.Items.Clear();
            if (_uiGraph != null)
            {
                comboBox1.Items.Add(new DropDownItem("Graph Properties", _uiGraph));
                comboBox1.Items.Add(new DropDownItem("Graph Data", _uiGraph.Graph));

                foreach (DaggerNode node in _uiGraph.Graph.AllNodes)
                {
                    comboBox1.Items.Add(new DropDownItem(node.ToString(), node));
                }

                // select the "Graph Data" item by default
                comboBox1.SelectedIndex = 1;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownItem item = (DropDownItem)comboBox1.SelectedItem;
            if (item.Tag is DaggerUIGraph)
            {
                propertyGridEx1.SelectedObject = item.Tag;
                propertyGridEx1.ShowCustomProperties = false;
            }
            else if (item.Tag is DaggerGraph)
            {
                propertyGridEx1.ShowCustomProperties = true;
                ClearProperties();
                foreach (DaggerOutputPin pin in _uiGraph.Graph.ImportedPins)
                {
                    AddPin("Imported Pins", "", pin);
                }
                foreach(DaggerInputPin pin in _uiGraph.Graph.ExportedPins)
                {
                    AddPin("Exported Pins", "", pin);
                }
                propertyGridEx1.Refresh();
            }
            else if (item.Tag is DaggerNode)
            {
                propertyGridEx1.ShowCustomProperties = true;
                ClearProperties();
                DaggerNode node = item.Tag as DaggerNode;
                foreach (DaggerInputPin pin in node.InputPins)
                {
                    AddPin("Input Pins", "", pin);
                }
                foreach (DaggerOutputPin pin in node.OutputPins)
                {
                    AddPin("Output Pins", "", pin);
                }
                propertyGridEx1.Refresh();
            }
            else
            {
                ClearProperties();
            }
        }

        /// <summary>
        /// Clear properties and unhook the pin data events
        /// </summary>
        private void ClearProperties()
        {
            // clear the existing pins
            for (int i = 0; i < propertyGridEx1.Item.Count; i++)
            {
                DaggerBasePin pin = propertyGridEx1.Item[i].Tag as DaggerBasePin;
                if (pin != null)
                {
                    // unhook existing events
                    pin.PinDataSet -= new DaggerPinDataSetHandler(pin_PinDataSet);
                    pin.PinDataTypeChanged -= new DaggerPinDataTypeChanged(pin_PinDataTypeChanged);
                    pin.PinNameChanged -= new DaggerPinNameChanged(pin_PinNameChanged);
                }
            }
            propertyGridEx1.Item.Clear();
        }

        private void AddPin(string category, string helpString, DaggerBasePin pin)
        {
            propertyGridEx1.Item.Add(pin.Name,pin.Data, false, category, helpString, true);
            propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].DefaultType = pin.DataType;
            propertyGridEx1.Item[propertyGridEx1.Item.Count - 1].Tag = pin;
            pin.PinDataSet += new DaggerPinDataSetHandler(pin_PinDataSet);
            pin.PinDataTypeChanged += new DaggerPinDataTypeChanged(pin_PinDataTypeChanged);
            pin.PinNameChanged += new DaggerPinNameChanged(pin_PinNameChanged);
        }

        private void RemovePin(DaggerBasePin pin)
        {
            if (comboBox1.SelectedIndex >= 1)
            {
                for (int i = 0; i < propertyGridEx1.Item.Count; i++)
                {
                    if (propertyGridEx1.Item[i].Tag == pin)
                    {
                        propertyGridEx1.Item.RemoveAt(i);
                        propertyGridEx1.Refresh();
                        break;
                    }
                }
            }
        }

        void pin_PinNameChanged(DaggerBasePin pin)
        {
            for (int i = 0; i < propertyGridEx1.Item.Count; i++)
            {
                if (propertyGridEx1.Item[i].Tag == pin)
                {
                    propertyGridEx1.Item[i].Name = pin.Name;
                    propertyGridEx1.Refresh();
                    break;
                }
            }
        }

        void pin_PinDataTypeChanged(DaggerBasePin pin, Type type)
        {
            for (int i = 0; i < propertyGridEx1.Item.Count; i++)
            {
                if (propertyGridEx1.Item[i].Tag == pin)
                {
                    propertyGridEx1.Item[i].DefaultType = pin.DataType;
                    propertyGridEx1.Item[i].Value = null;
                    propertyGridEx1.Refresh();
                    break;
                }
            }
        }

        void pin_PinDataSet(DaggerBasePin sender, object data)
        {
            if (Enabled)
            {
                for (int i = 0; i < propertyGridEx1.Item.Count; i++)
                {
                    if (propertyGridEx1.Item[i].Tag == sender)
                    {
                        propertyGridEx1.Item[i].Value = sender.Data;
                        propertyGridEx1.Refresh();
                        break;
                    }
                }
            }
        }
    }

    internal class DropDownItem
    {
        private string _name;
        private object _item;

        public DropDownItem(string name,object item)
        {
            _name = name;
            _item = item;
        }

        public override string ToString()
        {
            if (_item is DaggerNode)
            {
                if ((_item as DaggerNode).UINode != null)
                {
                    return (_item as DaggerNode).UINode.CaptionText;
                }
                else
                {
                    return _name;
                }
            }
            else
            {
                return _name;
            }
        }

        public object Tag
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
            }
        }
    }
}
