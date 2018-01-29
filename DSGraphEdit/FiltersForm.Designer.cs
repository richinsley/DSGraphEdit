namespace DSGraphEdit
{
    partial class FiltersForm
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
            this._filtersPanel = new DaggerLib.DSGraphEdit.DSFiltersPanel();
            this.SuspendLayout();
            // 
            // _filtersPanel
            // 
            this._filtersPanel.AssociatedGraphPanel = null;
            this._filtersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._filtersPanel.Location = new System.Drawing.Point(0, 0);
            this._filtersPanel.Name = "_filtersPanel";
            this._filtersPanel.Size = new System.Drawing.Size(292, 517);
            this._filtersPanel.TabIndex = 1;
            // 
            // FiltersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 517);
            this.Controls.Add(this._filtersPanel);
            this.Name = "FiltersForm";
            this.TabText = "Filters";
            this.Text = "Filters";
            this.ResumeLayout(false);

        }

        #endregion

        private DaggerLib.DSGraphEdit.DSFiltersPanel _filtersPanel;
    }
}