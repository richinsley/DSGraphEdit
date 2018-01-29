using System;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Core;

namespace DaggerLib.UI.Windows
{
    public class DaggerNodeTreeView : TreeView
    {
        private ContextMenuStrip _subNodeContextMenuStrip;
        private ToolStripMenuItem _exportSubNodeMenuItem;
        private ContextMenuStrip _categoryContextMenuStrip;
        private ToolStripMenuItem _importSubNodeMenuItem;
        private System.ComponentModel.IContainer components;
    
        public DaggerNodeTreeView()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        /// Get a list of the Category names in the TreeView
        /// </summary>
        public List<string> Categories
        {
            get
            {
                List<string> s = new List<string>();

                foreach (TreeNode tn in Nodes)
                {
                    s.Add(tn.ToString());
                }

                return s;
            }
        }

        #endregion

        #region Public Methods

        public TreeNode AddNodeType(string Category, string name, bool readOnly, Type type)
        {
            TreeNode catNode = Nodes[Category];

            if (catNode == null)
            {
                //create that category
                catNode = Nodes.Add(Category, Category);
                catNode.ContextMenuStrip = _categoryContextMenuStrip;
            }

            TreeNode newNode = catNode.Nodes.Add(name, name);
            newNode.Tag = new DaggerNodeTreeViewSubnodeItem(Category, name, type);
            newNode.ToolTipText = (newNode.Tag as DaggerNodeTreeViewSubnodeItem).ToolTipText;
            return newNode;
        }

        public TreeNode AddSubNode(string Category, string name, bool readOnly, byte[] buffer)
        {
            TreeNode catNode = Nodes[Category];

            if (catNode == null)
            {
                //create that category
                catNode = Nodes.Add(Category, Category);
                catNode.ContextMenuStrip = _categoryContextMenuStrip;
            }

            TreeNode newNode = catNode.Nodes.Add(name, name);
            newNode.Tag = new DaggerNodeTreeViewSubnodeItem(Category, name, readOnly, buffer);
            newNode.ContextMenuStrip = _subNodeContextMenuStrip;
            return newNode;
        }

        /// <summary>
        /// Find all the types of DaggerNode in the assembly that is loaded into the current AppDomain
        /// </summary>
        /// <param name="assembly">Name of assembly to generate TreeNodes from</param>
        public void AddAssembly(string assembly)
        {
            // get the list of assemblies loaded into the AppDomain
            Assembly[] asms;
            asms = AppDomain.CurrentDomain.GetAssemblies();

            // find the given assembly name in the list
            Assembly foundas = null;
            for (int i = 0; i < asms.Length; i++)
            {
                if (asms[i].GetName().Name == assembly)
                {
                    foundas = asms[i];
                    break;
                }
            }

            if (foundas != null)
            {
                // find all the Types derived from DaggerNode
                Type[] types = foundas.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    if(types[i].IsSubclassOf(typeof(DaggerNode)))
                    {
                        // get the static method CategoryName
                        MethodInfo mi = types[i].GetMethod("NodeCategory", BindingFlags.Static | BindingFlags.Public);
                        if (mi != null)
                        {
                            object cat = mi.Invoke(null, new object[] { });
                                                        if (cat != null)
                            {
                                string[] s = (cat as string).Split(',');
                                if (s.Length == 1)
                                {
                                    this.AddNodeType(s[0], types[i].Name, true, types[i]);
                                }
                                else
                                {
                                    this.AddNodeType(s[0], s[s.Length - 1], true, types[i]);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                throw(new InvalidOperationException("Assembly " + assembly + " is not loaded into the current AppDomain"));
            }
        }

        #endregion

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            //only allow dragging of child nodes
            if ((e.Item as TreeNode).Level > 0)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
            base.OnItemDrag(e);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._subNodeContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._exportSubNodeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._categoryContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._importSubNodeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._subNodeContextMenuStrip.SuspendLayout();
            this._categoryContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _subNodeContextMenuStrip
            // 
            this._subNodeContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._exportSubNodeMenuItem});
            this._subNodeContextMenuStrip.Name = "_subNodeContextMenuStrip";
            this._subNodeContextMenuStrip.Size = new System.Drawing.Size(153, 26);
            // 
            // _exportSubNodeMenuItem
            // 
            this._exportSubNodeMenuItem.Name = "_exportSubNodeMenuItem";
            this._exportSubNodeMenuItem.Size = new System.Drawing.Size(152, 22);
            this._exportSubNodeMenuItem.Text = "Export SubNode";
            this._exportSubNodeMenuItem.Click += new System.EventHandler(this._exportSubNodeMenuItem_Click);
            // 
            // _categoryContextMenuStrip
            // 
            this._categoryContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._importSubNodeMenuItem});
            this._categoryContextMenuStrip.Name = "_categoryContextMenuStrip";
            this._categoryContextMenuStrip.Size = new System.Drawing.Size(153, 26);
            this._categoryContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this._categoryContextMenuStrip_Opening);
            // 
            // _importSubNodeMenuItem
            // 
            this._importSubNodeMenuItem.Name = "_importSubNodeMenuItem";
            this._importSubNodeMenuItem.Size = new System.Drawing.Size(152, 22);
            this._importSubNodeMenuItem.Text = "Import SubNode";
            this._importSubNodeMenuItem.Click += new System.EventHandler(this._importSubNodeMenuItem_Click);
            this._subNodeContextMenuStrip.ResumeLayout(false);
            this._categoryContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void _exportSubNodeMenuItem_Click(object sender, EventArgs e)
        {
            DaggerNodeTreeViewSubnodeItem tn = (DaggerNodeTreeViewSubnodeItem)this.SelectedNode.Tag;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.Filter = "Dagger Graph files (*.dgrf)|*.txt|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.FileName = this.SelectedNode.Text;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Stream bufStream = null;
                    if ((bufStream = sfd.OpenFile()) != null)
                    {
                        bufStream.Write(tn.DaggerNodeSerializedBuffer, 0, (int)tn.DaggerNodeSerializedBuffer.Length);
                        // Code to write the stream goes here.
                        bufStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception " + ex.Message + " writing graph","Error");
                }
            }
            sfd.Dispose();
        }

        private void _importSubNodeMenuItem_Click(object sender, EventArgs e)
        {
            string cat = (string)_categoryContextMenuStrip.Tag;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dagger Graph files (*.dgrf)|*.txt|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Stream bufStream = null;
                if ((bufStream = ofd.OpenFile()) != null)
                {
                    byte[] buff = new byte[bufStream.Length];
                    bufStream.Read(buff, 0, (int)bufStream.Length);
                    bufStream.Close();
                    this.AddSubNode(cat, Path.GetFileNameWithoutExtension(ofd.FileName), false, buff);
                }
            }
            ofd.Dispose();
        }

        private void _categoryContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _categoryContextMenuStrip.Tag = this.SelectedNode.Text;
        }
    }

    internal class DaggerNodeTreeViewSubnodeItem
    {
        public string Category;
        public string SubnodeName;
        public bool ReadOnly;
        public byte[] DaggerNodeSerializedBuffer;
        public Type DaggerNodeType;
        public string ToolTipText = string.Empty;

        public List<Type> InputpinTypes;
        public List<Type> OutputPinTypes;

        public DaggerNodeTreeViewSubnodeItem(string category, string name, bool readOnly, byte[] buffer)
        {
            Category = category;
            SubnodeName = name;
            ReadOnly = readOnly;
            DaggerNodeSerializedBuffer = buffer;
        }

        public DaggerNodeTreeViewSubnodeItem(string category, string name, Type nodeType)
        {
            Category = category;
            SubnodeName = name;
            ReadOnly = true;
            DaggerNodeType = nodeType;

            // Fabricate a temp node to extract it's pin's data types.
            // This will be used in computing noodle bisecting and auto drop connections
            DaggerNode node = null;
            try
            {
                node = (DaggerNode)Activator.CreateInstance(nodeType);
            }
            catch (MissingMemberException ex)
            {
                // The node type has no parameterless constructor.
                // The most likely cause is that there is a deserialization constructor, but
                // no public parameterless constructor defined.

                MessageBox.Show("Node type " + name + " has no parameterless constructor defined","Error adding node to DaggerNodeTreeView");
            }

            if (node != null)
            {
                OutputPinTypes = new List<Type>();
                InputpinTypes = new List<Type>();

                foreach (DaggerInputPin pin in node.InputPins)
                {
                    InputpinTypes.Add(pin.DataType);
                }

                foreach (DaggerOutputPin pin in node.OutputPins)
                {
                    OutputPinTypes.Add(pin.DataType);
                }

                // set the ToolTip to the help string
                ToolTipText = node.HelpString;
            }
        }         
    }
}
