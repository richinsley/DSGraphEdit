using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public class DSFilterTreeView : TreeView
    {
        private ContextMenuStrip contextMenuStrip1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem _refreshFiltersContextMenuItem;
    
        public DSFilterTreeView()
        {
            InitializeComponent();
        }

        #region InitializeComponent

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._refreshFiltersContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._refreshFiltersContextMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(145, 26);
            // 
            // _refreshFiltersContextMenuItem
            // 
            this._refreshFiltersContextMenuItem.Name = "_refreshFiltersContextMenuItem";
            this._refreshFiltersContextMenuItem.Size = new System.Drawing.Size(144, 22);
            this._refreshFiltersContextMenuItem.Text = "Refresh Filters";
            this._refreshFiltersContextMenuItem.Click += new System.EventHandler(this._refreshFiltersContextMenuItem_Click);
            // 
            // DSFilterTreeView
            // 
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.LineColor = System.Drawing.Color.Black;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

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

        public void SyncFilters()
        {
            // get a list of all standard Direct Show Filter Categories
            List<DsDevice> categories = new List<DsDevice>(DirectShowLib.DsDevice.GetDevicesOfCat(FilterCategory.ActiveMovieCategories));

            // find tree nodes that haven't been created for a category yet
            foreach (DsDevice dev in categories)
            {
                if (GetParentNodeByName(dev.Name) == null)
                {
                    // create the IDSFilterCollection for this category
                    TreeNode tn = new TreeNode(dev.Name);
                    tn.Tag = new StandardFilterCategory(dev);
                    Nodes.Add(tn);
                }
            }

            this.BeginUpdate();
            foreach (TreeNode tn in Nodes)
            {
                IDSFilterCollection categoryCollection = (IDSFilterCollection)tn.Tag;
                // sync the filters in the filter collection with the tree view
                categoryCollection.SyncTreeNodes(tn.Nodes);
            }
            Sort();
            this.EndUpdate();
        }

        public void AddCategory(IDSFilterCollection categoryCollection)
        {
            // create a parent node for the filter collection
            TreeNode tn = new TreeNode(categoryCollection.CategoryName);
            tn.Tag = categoryCollection;
            Nodes.Add(tn);
        }

        public TreeNode GetParentNodeByName(string name)
        {
            foreach (TreeNode tn in Nodes)
            {
                if (tn.Text == name) return tn;
            }

            return null;
        }

        public static DSFilterTreeViewNode GetTreeNodeByDevicePath(string path,TreeNodeCollection collection)
        {
            foreach (DSFilterTreeViewNode tn in collection)
            {
                if (tn.DevicePath == path) return tn;
            }

            return null;
        }

        public static DSFilterTreeViewNode GetTreeNodeByGuid(Guid guid,TreeNodeCollection collection)
        {
            foreach (DSFilterTreeViewNode tn in collection)
            {
                if (tn.ClassGuid == guid) return tn;
            }

            return null;
        }

        private void _refreshFiltersContextMenuItem_Click(object sender, EventArgs e)
        {
            SyncFilters();
        }
    }
}
