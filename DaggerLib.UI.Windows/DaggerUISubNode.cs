using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using DaggerLib.Interfaces;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    public partial class DaggerUISubNode : DaggerUINode , IDaggerUISubNode
    {
        //the edit SubNode Caption Button
        internal SimpleImageButton _editSubNodeButton;

        //UIGraph for editing a subnode's graph
        internal DaggerUIGraph _subNodeUIGraph;

        private DaggerSubNode _node;

        public DaggerUISubNode()
        {
            InitializeComponent();
            this.DaggerNodeAttached += new DaggerNodeAttachedHandler(DaggerUISubNode_DaggerNodeAttached);
        }

        public DaggerPinLegend PinLegend
        {
            get
            {
                if (_subNodeUIGraph != null)
                {
                    return _subNodeUIGraph.PinLegend;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (_subNodeUIGraph != null)
                {
                    _subNodeUIGraph.PinLegend = value;
                }
            }
        }

        void DaggerUISubNode_DaggerNodeAttached(DaggerNode node)
        {
            // store the sub node so we can make the UIGraph for it later
            _node = (DaggerSubNode)node;
            _node.SubNodeGraph.ImportedPins.PinAdded += new DaggerPinAdded(ImportedPins_PinAdded);
            _node.SubNodeGraph.ExportedPins.PinAdded += new DaggerPinAdded(ExportedPins_PinAdded);
            _editSubNodeButton = new SimpleImageButton();
            _editSubNodeButton.ButtonImage = _subnodeEditButtonImageList.Images[0];
            _editSubNodeButton.ButtonImage2 = _subnodeEditButtonImageList.Images[1];
            _editSubNodeButton.MultiState = true;
            _editSubNodeButton.ToolTipText = "Edit SubNode";
            _editSubNodeButton.ToolTipEnabled = true;
            _editSubNodeButton.StateChanged += new SimpleImageButton.StateChangedEventHandler(_editSubNodeButton_StateChanged);
            CaptionButtons.Add(_editSubNodeButton);
        }

        void ExportedPins_PinAdded(object sender, DaggerBasePin pin)
        {
            if (pin.PinUIElements == null)
            {
                pin.PinUIElements = new PinUI(pin);
            }
        }

        void ImportedPins_PinAdded(object sender, DaggerBasePin pin)
        {
                        if (pin.PinUIElements == null)
            {
                pin.PinUIElements = new PinUI(pin);
            }
        }

        /// <summary>
        /// Event that is raised when the Edit SubNode Caption button is toggled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _editSubNodeButton_StateChanged(object sender, EventArgs e)
        {
            if (_editSubNodeButton.State)
            {
                if (_subNodeUIGraph == null)
                {
                    // create the ui graph for the SubNode
                    _subNodeUIGraph = new DaggerUIGraph(_node.SubNodeGraph, (ParentUIGraph as DaggerUIGraph).PinLegend);
                    _subNodeUIGraph.Visible = true;
                    _subNodeUIGraph.Dock = DockStyle.Fill;
                    _internalControl.Controls.Add(_subNodeUIGraph);

                    // copy the settings of the parent ui graph
                    (ParentUIGraph as DaggerUIGraph).CopySettings(_subNodeUIGraph);
                }

                //expand node for editing
                InternalControl.Visible = true;
                Size = _subNodeUIGraph.OccupiedRegion.Size;
                Resizable = true;
            }
            else
            {
                //collapse node
                InternalControl.Visible = false;
                Resizable = false;
                Size = NodeMinimumSize;
            }
        }

        private void _saveSubNodeToTreeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] subnode = (_node as DaggerSubNode).SubNodeGraph.SerializeSubGraph(-1);

            if (subnode != null)
            {
                if (_parentGraph.DaggerNodeTreeView != null)
                {
                    ValueEditorDialog vue = new ValueEditorDialog("Subnode Name", "Subnode");
                    if (vue.ShowDialog() == DialogResult.OK)
                    {
                        _parentGraph.DaggerNodeTreeView.AddSubNode("User functions", (string)vue.Data, false, subnode);
                    }
                }
                else
                {
                    ValueEditorDialog vue = new ValueEditorDialog("Subnode Name", "Subnode");
                    if (vue.ShowDialog() == DialogResult.OK)
                    {
                        _node.ParentGraph.AddNode(new DaggerSubNode((string)vue.Data, subnode));
                    }
                }
            }
        }
    }
}

