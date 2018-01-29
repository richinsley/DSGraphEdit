namespace DaggerLib.DSGraphEdit
{
    partial class DSGrapheditForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._filtersPanel = new DaggerLib.DSGraphEdit.DSFiltersPanel();
            this.dsGraphEditPanel1 = new DaggerLib.DSGraphEdit.DSGraphEditPanel();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._filtersPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dsGraphEditPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(768, 463);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 2;
            // 
            // _filtersPanel
            // 
            this._filtersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._filtersPanel.Location = new System.Drawing.Point(0, 0);
            this._filtersPanel.Name = "_filtersPanel";
            this._filtersPanel.Size = new System.Drawing.Size(232, 463);
            this._filtersPanel.TabIndex = 0;
            // 
            // dsGraphEditPanel1
            // 
            this.dsGraphEditPanel1.ConnectIntelligent = true;
            this.dsGraphEditPanel1.DropShadow = false;
            this.dsGraphEditPanel1.Location = new System.Drawing.Point(77, 94);
            this.dsGraphEditPanel1.ModalProperties = false;
            this.dsGraphEditPanel1.Name = "dsGraphEditPanel1";
            this.dsGraphEditPanel1.PinPlacement = DaggerLib.UI.Windows.DaggerNodePinPlacement.Outset;
            this.dsGraphEditPanel1.Size = new System.Drawing.Size(525, 354);
            this.dsGraphEditPanel1.TabIndex = 0;
            // 
            // DSGrapheditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 463);
            this.Controls.Add(this.splitContainer1);
            this.Name = "DSGrapheditForm";
            this.Text = "DSGrapheditForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private DaggerLib.DSGraphEdit.DSFiltersPanel _filtersPanel;
        private DSGraphEditPanel dsGraphEditPanel1;
    }
}