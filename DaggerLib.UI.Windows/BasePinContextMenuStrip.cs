using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    [ToolboxItem(false)]
    internal class BasePinContextMenuStrip : ContextMenuStrip
    {
        ToolStripMenuItem _disconnectMenuItem;
        ToolStripMenuItem _exportMenuItem;
        ToolStripMenuItem _setpropertyMenuItem;
        ToolStripMenuItem _attachToPinMenuItem;

        ToolStripSeparator _seperator;

        public BasePinContextMenuStrip()
        {
            _seperator = new ToolStripSeparator();

            _disconnectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            _disconnectMenuItem.Text = "Disconnect";
            _disconnectMenuItem.Click += new EventHandler(_disconnectMenuItem_Click);

            _exportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            _exportMenuItem.Text = "Export";
            _exportMenuItem.Click += new EventHandler(_exportMenuItem_Click);

            _setpropertyMenuItem = new ToolStripMenuItem();
            _setpropertyMenuItem.Text = "Set Value";
            _setpropertyMenuItem.Click += new EventHandler(_setpropertyMenuItem_Click);

            _attachToPinMenuItem = new ToolStripMenuItem();
            _attachToPinMenuItem.Text = "Connect to:";
            _attachToPinMenuItem.DropDown.Opacity = 0.5;
            _attachToPinMenuItem.DropDownOpened += new EventHandler(_attachToPinMenuItem_DropDownOpened);
            _attachToPinMenuItem.DropDownClosed += new EventHandler(_attachToPinMenuItem_DropDownClosed);
        }

        void _setpropertyMenuItem_Click(object sender, EventArgs e)
        {
            DaggerBasePin pin = Tag as DaggerBasePin;
            ValueEditorDialog ved = new ValueEditorDialog(pin);

            if (ved.ShowDialog() == DialogResult.OK)
            {
                pin.Data = ved.Data;
                pin.ParentNode.Process();
            }

            ved.Dispose();
        }

        void _exportMenuItem_Click(object sender, EventArgs e)
        {
            if (Tag is DaggerInputPin)
            {
                (Tag as DaggerInputPin).ParentNode.ParentGraph.ExportPin((DaggerInputPin)Tag);
            }
            else
            {
                (Tag as DaggerOutputPin).ParentNode.ParentGraph.ExportPin((DaggerOutputPin)Tag);
            }

            (Tag as DaggerBasePin).ParentUIGraph.RefreshGraph();
        }

        void _disconnectMenuItem_Click(object sender, EventArgs e)
        {
            DaggerBasePin pin = Tag as DaggerBasePin;
            pin.Disconnect(false);
            pin.ParentUIGraph.RefreshGraph();
        }

        protected override void OnOpening(System.ComponentModel.CancelEventArgs e)
        {
            base.OnOpening(e);
            
            //clear out the existing items and rebuild the menu to specs
            Items.Clear();

            bool itemsAdded = false;

            DaggerBasePin pin = Tag as DaggerBasePin;
            if (pin.IsConnected)
            {
                Items.Add(_disconnectMenuItem);
                itemsAdded = true;
            }
            else if (pin.ParentNode != null)
            {
                DaggerUIGraph parentui = pin.ParentNode.ParentGraph.ParentUIGraph as DaggerUIGraph;

                // can we set the value?
                if (parentui.AllowPinSetValue)
                {
                    Items.Add(_setpropertyMenuItem);
                    itemsAdded = true;
                }

                // can we export?
                if (parentui.AllowPinExport)
                {
                    Items.Add(_exportMenuItem);
                    itemsAdded = true;
                }
            }

            // build the "Connect to" list
            if ((pin is DaggerOutputPin && (pin as DaggerOutputPin).AllowMultiConnect) ||
                (pin is DaggerOutputPin && !(pin as DaggerOutputPin).AllowMultiConnect && !pin.IsConnected) ||
                (pin is DaggerInputPin && !pin.IsConnected))
            {
                _attachToPinMenuItem.DropDownItems.Clear();

                DaggerNode myNode = pin.ParentNode;

                foreach (DaggerNode node in pin.ParentNode.ParentGraph.AllNodes)
                {
                    if (node != myNode)
                    {
                        if (pin is DaggerInputPin)
                        {
                            if (node.Ordinal <= myNode.Ordinal || node.SubgraphAffiliation != myNode.SubgraphAffiliation)
                            {
                                foreach (DaggerOutputPin outpin in node.OutputPins.MutexAvailablePins)
                                {
                                    if ((!outpin.IsConnected || outpin.AllowMultiConnect) && outpin.IsCompatibleDataTypes((DaggerInputPin)pin, (DaggerOutputPin)outpin))
                                    {
                                        ToolStripMenuItem tmi = new ToolStripMenuItem();
                                        tmi.Text = node.UINode.CaptionText + ": " + outpin.Name;
                                        tmi.Tag = new PinConnection(outpin, (DaggerInputPin)pin);
                                        tmi.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                                        tmi.Image = (outpin.PinUIElements as PinUI).PinImageDisconnected;
                                        tmi.ImageTransparentColor = (outpin.PinUIElements as PinUI).PinImageDisconnectedTransparent;
                                        tmi.MouseEnter += new EventHandler(tmi_MouseEnter);
                                        tmi.MouseLeave += new EventHandler(tmi_MouseLeave);
                                        tmi.Click += new EventHandler(tmi_Click);
                                        _attachToPinMenuItem.DropDownItems.Add(tmi);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (node.Ordinal >= myNode.Ordinal || node.SubgraphAffiliation != myNode.SubgraphAffiliation)
                            {
                                foreach (DaggerInputPin inpin in node.InputPins.MutexAvailablePins)
                                {
                                    if (!inpin.IsConnected && inpin.IsCompatibleDataTypes((DaggerInputPin)inpin, (DaggerOutputPin)pin))
                                    {
                                        ToolStripMenuItem tmi = new ToolStripMenuItem();
                                        tmi.Text = node.UINode.CaptionText + ": " + inpin.Name;
                                        tmi.Tag = new PinConnection((DaggerOutputPin)pin, inpin);
                                        tmi.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                                        tmi.Image = (inpin.PinUIElements as PinUI).PinImageDisconnected;
                                        tmi.ImageTransparentColor = (inpin.PinUIElements as PinUI).PinImageDisconnectedTransparent;
                                        tmi.MouseEnter += new EventHandler(tmi_MouseEnter);
                                        tmi.MouseLeave += new EventHandler(tmi_MouseLeave);
                                        tmi.Click += new EventHandler(tmi_Click);
                                        _attachToPinMenuItem.DropDownItems.Add(tmi);
                                    }
                                }
                            }
                        }
                    }
                }

                if (_attachToPinMenuItem.DropDownItems.Count > 0)
                {
                    Items.Add(_attachToPinMenuItem);
                    itemsAdded = true;
                }
            }

            //merge the pin's user defined context menu
            if ((pin.PinUIElements as PinUI).ContextMenuStrip != null)
            {
                if (itemsAdded)
                {
                    Items.Add(_seperator);
                }

                // simulate a sync root on the MenuItemCollection
                ToolStripMenuItem[] tia = new ToolStripMenuItem[(pin.PinUIElements as PinUI).ContextMenuStrip.Items.Count];
                (pin.PinUIElements as PinUI).ContextMenuStrip.Items.CopyTo(tia, 0);
                for (int i = 0; i < tia.Length; i++)
                {
                    Items.Add(tia[i]);
                }
                itemsAdded = true;
            }

            //if we didn't add anything, don't show it
            e.Cancel = !itemsAdded;
        }

        #region Connect To Methods
        /// <summary>
        /// Set the transparence to half when showing the Connect To drop down.
        /// This way, we can preview noodles on the Graph before selecting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _attachToPinMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            Opacity = 0.5;
        }

        /// <summary>
        /// Set the tranparency to solid when not showing the Connect To drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _attachToPinMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            Opacity = 1;
            ((Tag as DaggerBasePin).ParentNode.ParentGraph.ParentUIGraph as DaggerUIGraph)._trackingConnectPin = null;
            (Tag as DaggerBasePin).ParentNode.ParentGraph.ParentUIGraph.RefreshGraph();
        }

        void tmi_Click(object sender, EventArgs e)
        {
            PinConnection con = (PinConnection)(sender as ToolStripMenuItem).Tag;
            con.OutputPin.ConnectToInput(con.InputPin);
        }

        void tmi_MouseLeave(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void tmi_MouseEnter(object sender, EventArgs e)
        {
            DaggerUIGraph uigraph = (Tag as DaggerBasePin).ParentNode.ParentGraph.ParentUIGraph as DaggerUIGraph;
            PinConnection con = con = (PinConnection)(sender as ToolStripMenuItem).Tag;
            uigraph._trackingConnectPin = new DraggingNoodle(con.InputPin, con.OutputPin);
            uigraph.Invalidate(false);
        }

        #endregion
    }
}
