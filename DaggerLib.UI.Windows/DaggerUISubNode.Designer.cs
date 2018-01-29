namespace DaggerLib.UI.Windows
{
    partial class DaggerUISubNode
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DaggerUISubNode));
            this._subnodeEditButtonImageList = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._saveSubNodeToTreeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _subnodeEditButtonImageList
            // 
            this._subnodeEditButtonImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_subnodeEditButtonImageList.ImageStream")));
            this._subnodeEditButtonImageList.TransparentColor = System.Drawing.Color.Transparent;
            this._subnodeEditButtonImageList.Images.SetKeyName(0, "bullet_arrow_down.png");
            this._subnodeEditButtonImageList.Images.SetKeyName(1, "bullet_arrow_up.png");
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._saveSubNodeToTreeViewToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(207, 76);
            // 
            // _saveSubNodeToTreeViewToolStripMenuItem
            // 
            this._saveSubNodeToTreeViewToolStripMenuItem.Name = "_saveSubNodeToTreeViewToolStripMenuItem";
            this._saveSubNodeToTreeViewToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this._saveSubNodeToTreeViewToolStripMenuItem.Text = "Save SubNode to TreeView";
            this._saveSubNodeToTreeViewToolStripMenuItem.Click += new System.EventHandler(this._saveSubNodeToTreeViewToolStripMenuItem_Click);
            // 
            // DaggerUISubNode
            // 
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Name = "DaggerUISubNode";
            this.Size = new System.Drawing.Size(111, 76);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList _subnodeEditButtonImageList;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem _saveSubNodeToTreeViewToolStripMenuItem;
    }
}
