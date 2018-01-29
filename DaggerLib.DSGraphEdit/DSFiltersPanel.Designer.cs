namespace DaggerLib.DSGraphEdit
{
    partial class DSFiltersPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DSFiltersPanel));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dsFilterTreeView1 = new DaggerLib.DSGraphEdit.DSFilterTreeView();
            this._filterPropertiesButton = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._refreshFiltersButton = new System.Windows.Forms.ToolStripButton();
            this._insertFilterButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._expandAllButton = new System.Windows.Forms.ToolStripButton();
            this._collapseAllButton = new System.Windows.Forms.ToolStripButton();
            this._findFilterTextBox = new System.Windows.Forms.ToolStripComboBox();
            this._findFilterButton = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dsFilterTreeView1);
            this.splitContainer1.Panel1.Controls.Add(this._filterPropertiesButton);
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(368, 467);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            // 
            // dsFilterTreeView1
            // 
            this.dsFilterTreeView1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.dsFilterTreeView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dsFilterTreeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dsFilterTreeView1.Location = new System.Drawing.Point(0, 25);
            this.dsFilterTreeView1.Name = "dsFilterTreeView1";
            this.dsFilterTreeView1.Size = new System.Drawing.Size(368, 197);
            this.dsFilterTreeView1.TabIndex = 3;
            this.dsFilterTreeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dsFilterTreeView1_AfterSelect);
            // 
            // _filterPropertiesButton
            // 
            this._filterPropertiesButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._filterPropertiesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._filterPropertiesButton.ImageIndex = 0;
            this._filterPropertiesButton.ImageList = this.imageList1;
            this._filterPropertiesButton.Location = new System.Drawing.Point(0, 222);
            this._filterPropertiesButton.Name = "_filterPropertiesButton";
            this._filterPropertiesButton.Size = new System.Drawing.Size(368, 10);
            this._filterPropertiesButton.TabIndex = 2;
            this._filterPropertiesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._filterPropertiesButton.UseVisualStyleBackColor = true;
            this._filterPropertiesButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "thumbdown.png");
            this.imageList1.Images.SetKeyName(1, "thumbup.png");
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._refreshFiltersButton,
            this._insertFilterButton,
            this.toolStripSeparator1,
            this._expandAllButton,
            this._collapseAllButton,
            this._findFilterTextBox,
            this._findFilterButton});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(368, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _refreshFiltersButton
            // 
            this._refreshFiltersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._refreshFiltersButton.Image = ((System.Drawing.Image)(resources.GetObject("_refreshFiltersButton.Image")));
            this._refreshFiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._refreshFiltersButton.Name = "_refreshFiltersButton";
            this._refreshFiltersButton.Size = new System.Drawing.Size(23, 22);
            this._refreshFiltersButton.Text = "toolStripButton1";
            this._refreshFiltersButton.ToolTipText = "Refresh Filters";
            this._refreshFiltersButton.Click += new System.EventHandler(this._refreshFiltersButton_Click);
            // 
            // _insertFilterButton
            // 
            this._insertFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._insertFilterButton.Enabled = false;
            this._insertFilterButton.Image = ((System.Drawing.Image)(resources.GetObject("_insertFilterButton.Image")));
            this._insertFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._insertFilterButton.Name = "_insertFilterButton";
            this._insertFilterButton.Size = new System.Drawing.Size(23, 22);
            this._insertFilterButton.Text = "toolStripButton1";
            this._insertFilterButton.ToolTipText = "Insert Filter";
            this._insertFilterButton.Visible = false;
            this._insertFilterButton.Click += new System.EventHandler(this._insertFilterButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _expandAllButton
            // 
            this._expandAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._expandAllButton.Image = ((System.Drawing.Image)(resources.GetObject("_expandAllButton.Image")));
            this._expandAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._expandAllButton.Name = "_expandAllButton";
            this._expandAllButton.Size = new System.Drawing.Size(23, 22);
            this._expandAllButton.Text = "toolStripButton1";
            this._expandAllButton.ToolTipText = "Expand All Nodes";
            this._expandAllButton.Click += new System.EventHandler(this._expandAllButton_Click);
            // 
            // _collapseAllButton
            // 
            this._collapseAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._collapseAllButton.Image = ((System.Drawing.Image)(resources.GetObject("_collapseAllButton.Image")));
            this._collapseAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._collapseAllButton.Name = "_collapseAllButton";
            this._collapseAllButton.Size = new System.Drawing.Size(23, 22);
            this._collapseAllButton.Text = "Collapse All Nodes";
            this._collapseAllButton.Click += new System.EventHandler(this._collapseAllButton_Click);
            // 
            // _findFilterTextBox
            // 
            this._findFilterTextBox.Name = "_findFilterTextBox";
            this._findFilterTextBox.Size = new System.Drawing.Size(100, 25);
            this._findFilterTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this._findFilterTextBox_KeyUp);
            this._findFilterTextBox.DropDown += new System.EventHandler(this._findFilterTextBox_DropDown);
            // 
            // _findFilterButton
            // 
            this._findFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._findFilterButton.Image = ((System.Drawing.Image)(resources.GetObject("_findFilterButton.Image")));
            this._findFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._findFilterButton.Name = "_findFilterButton";
            this._findFilterButton.Size = new System.Drawing.Size(23, 22);
            this._findFilterButton.Text = "Find Filter";
            this._findFilterButton.Click += new System.EventHandler(this._findFilterButton_Click);
            // 
            // DSFiltersPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DSFiltersPanel";
            this.Size = new System.Drawing.Size(368, 467);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button _filterPropertiesButton;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ImageList imageList1;
        private DSFilterTreeView dsFilterTreeView1;
        private System.Windows.Forms.ToolStripButton _refreshFiltersButton;
        private System.Windows.Forms.ToolStripButton _insertFilterButton;
        private System.Windows.Forms.ToolStripButton _expandAllButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _collapseAllButton;
        private System.Windows.Forms.ToolStripButton _findFilterButton;
        private System.Windows.Forms.ToolStripComboBox _findFilterTextBox;
    }
}
