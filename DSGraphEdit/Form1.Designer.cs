namespace DSGraphEdit
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveGraphAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.renderMediaFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.connectToRemoteGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.saveCanvasImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphNavigatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filtersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.defaultGraphOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(592, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newGraphToolStripMenuItem,
            this.openGraphToolStripMenuItem,
            this.saveGraphToolStripMenuItem,
            this.saveGraphAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.renderMediaFileToolStripMenuItem,
            this.renderURLToolStripMenuItem,
            this.toolStripSeparator2,
            this.connectToRemoteGraphToolStripMenuItem,
            this.toolStripSeparator3,
            this.saveCanvasImageToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            // 
            // newGraphToolStripMenuItem
            // 
            this.newGraphToolStripMenuItem.Name = "newGraphToolStripMenuItem";
            this.newGraphToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newGraphToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.newGraphToolStripMenuItem.Text = "New Graph";
            this.newGraphToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openGraphToolStripMenuItem
            // 
            this.openGraphToolStripMenuItem.Name = "openGraphToolStripMenuItem";
            this.openGraphToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openGraphToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.openGraphToolStripMenuItem.Text = "Open Graph";
            this.openGraphToolStripMenuItem.Click += new System.EventHandler(this.openGraphToolStripMenuItem_Click);
            // 
            // saveGraphToolStripMenuItem
            // 
            this.saveGraphToolStripMenuItem.Name = "saveGraphToolStripMenuItem";
            this.saveGraphToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveGraphToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.saveGraphToolStripMenuItem.Text = "Save Graph";
            this.saveGraphToolStripMenuItem.Click += new System.EventHandler(this.saveGraphToolStripMenuItem_Click);
            // 
            // saveGraphAsToolStripMenuItem
            // 
            this.saveGraphAsToolStripMenuItem.Name = "saveGraphAsToolStripMenuItem";
            this.saveGraphAsToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.saveGraphAsToolStripMenuItem.Text = "Save Graph As";
            this.saveGraphAsToolStripMenuItem.Click += new System.EventHandler(this.saveGraphAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(254, 6);
            // 
            // renderMediaFileToolStripMenuItem
            // 
            this.renderMediaFileToolStripMenuItem.Name = "renderMediaFileToolStripMenuItem";
            this.renderMediaFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.renderMediaFileToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.renderMediaFileToolStripMenuItem.Text = "Render Media File";
            this.renderMediaFileToolStripMenuItem.Click += new System.EventHandler(this.renderMediaFileToolStripMenuItem_Click);
            // 
            // renderURLToolStripMenuItem
            // 
            this.renderURLToolStripMenuItem.Name = "renderURLToolStripMenuItem";
            this.renderURLToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.renderURLToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.renderURLToolStripMenuItem.Text = "Render URL";
            this.renderURLToolStripMenuItem.Click += new System.EventHandler(this.renderURLToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(254, 6);
            // 
            // connectToRemoteGraphToolStripMenuItem
            // 
            this.connectToRemoteGraphToolStripMenuItem.Name = "connectToRemoteGraphToolStripMenuItem";
            this.connectToRemoteGraphToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.connectToRemoteGraphToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.connectToRemoteGraphToolStripMenuItem.Text = "Connect To Remote Graph";
            this.connectToRemoteGraphToolStripMenuItem.Click += new System.EventHandler(this.connectToRemoteGraphToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(254, 6);
            // 
            // saveCanvasImageToolStripMenuItem
            // 
            this.saveCanvasImageToolStripMenuItem.Name = "saveCanvasImageToolStripMenuItem";
            this.saveCanvasImageToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.saveCanvasImageToolStripMenuItem.Text = "Save Canvas Image";
            this.saveCanvasImageToolStripMenuItem.Click += new System.EventHandler(this.saveCanvasImageToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.graphNavigatorToolStripMenuItem,
            this.filtersToolStripMenuItem,
            this.defaultGraphOptionsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            this.viewToolStripMenuItem.DropDownOpening += new System.EventHandler(this.viewToolStripMenuItem_DropDownOpening);
            // 
            // graphNavigatorToolStripMenuItem
            // 
            this.graphNavigatorToolStripMenuItem.Name = "graphNavigatorToolStripMenuItem";
            this.graphNavigatorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.graphNavigatorToolStripMenuItem.Text = "Graph Navigator";
            this.graphNavigatorToolStripMenuItem.Click += new System.EventHandler(this.graphNavigatorToolStripMenuItem_Click);
            // 
            // filtersToolStripMenuItem
            // 
            this.filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            this.filtersToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.filtersToolStripMenuItem.Text = "Filters";
            this.filtersToolStripMenuItem.Click += new System.EventHandler(this.filtersToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutDSGraphEditToolStripMenuItem_Click);
            // 
            // _dockPanel
            // 
            this._dockPanel.ActiveAutoHideContent = null;
            this._dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dockPanel.Location = new System.Drawing.Point(0, 24);
            this._dockPanel.Name = "_dockPanel";
            this._dockPanel.Size = new System.Drawing.Size(592, 331);
            this._dockPanel.TabIndex = 2;
            // 
            // defaultGraphOptionsToolStripMenuItem
            // 
            this.defaultGraphOptionsToolStripMenuItem.Name = "defaultGraphOptionsToolStripMenuItem";
            this.defaultGraphOptionsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.defaultGraphOptionsToolStripMenuItem.Text = "Default Graph Options";
            this.defaultGraphOptionsToolStripMenuItem.Click += new System.EventHandler(this.defaultGraphOptionsToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 355);
            this.Controls.Add(this._dockPanel);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "DSGraphEdit";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private WeifenLuo.WinFormsUI.Docking.DockPanel _dockPanel;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveGraphAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem renderMediaFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem connectToRemoteGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem graphNavigatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filtersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveCanvasImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem defaultGraphOptionsToolStripMenuItem;


    }
}

