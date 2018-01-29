namespace DaggerLib.DSGraphEdit
{
    partial class FilterPropertiesPanel
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
            this.panel1 = new System.Windows.Forms.Panel();
            this._filenameLabel = new DaggerLib.DSGraphEdit.AutoSizeTextBox();
            this._monikerTextBox = new System.Windows.Forms.TextBox();
            this._nameLabel = new System.Windows.Forms.Label();
            this._meritLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._pinsTreeView = new System.Windows.Forms.TreeView();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.panel1.Controls.Add(this._filenameLabel);
            this.panel1.Controls.Add(this._monikerTextBox);
            this.panel1.Controls.Add(this._nameLabel);
            this.panel1.Controls.Add(this._meritLabel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(226, 94);
            this.panel1.TabIndex = 0;
            // 
            // _filenameLabel
            // 
            this._filenameLabel.BackColor = System.Drawing.Color.MediumSeaGreen;
            this._filenameLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._filenameLabel.Location = new System.Drawing.Point(70, 19);
            this._filenameLabel.Name = "_filenameLabel";
            this._filenameLabel.ReadOnly = true;
            this._filenameLabel.Size = new System.Drawing.Size(17, 13);
            this._filenameLabel.TabIndex = 8;
            this._filenameLabel.Text = "...";
            // 
            // _monikerTextBox
            // 
            this._monikerTextBox.BackColor = System.Drawing.Color.MediumSeaGreen;
            this._monikerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._monikerTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._monikerTextBox.Location = new System.Drawing.Point(0, 54);
            this._monikerTextBox.Multiline = true;
            this._monikerTextBox.Name = "_monikerTextBox";
            this._monikerTextBox.ReadOnly = true;
            this._monikerTextBox.Size = new System.Drawing.Size(226, 40);
            this._monikerTextBox.TabIndex = 7;
            // 
            // _nameLabel
            // 
            this._nameLabel.AutoSize = true;
            this._nameLabel.Location = new System.Drawing.Point(67, 3);
            this._nameLabel.Name = "_nameLabel";
            this._nameLabel.Size = new System.Drawing.Size(16, 13);
            this._nameLabel.TabIndex = 5;
            this._nameLabel.Text = "...";
            // 
            // _meritLabel
            // 
            this._meritLabel.AutoSize = true;
            this._meritLabel.Location = new System.Drawing.Point(67, 38);
            this._meritLabel.Name = "_meritLabel";
            this._meritLabel.Size = new System.Drawing.Size(16, 13);
            this._meritLabel.TabIndex = 3;
            this._meritLabel.Text = "...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Filename: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Merit: ";
            // 
            // _pinsTreeView
            // 
            this._pinsTreeView.BackColor = System.Drawing.Color.DarkSeaGreen;
            this._pinsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._pinsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pinsTreeView.Location = new System.Drawing.Point(0, 94);
            this._pinsTreeView.Name = "_pinsTreeView";
            this._pinsTreeView.Size = new System.Drawing.Size(226, 83);
            this._pinsTreeView.TabIndex = 1;
            // 
            // FilterPropertiesPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._pinsTreeView);
            this.Controls.Add(this.panel1);
            this.Name = "FilterPropertiesPanel";
            this.Size = new System.Drawing.Size(226, 177);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _nameLabel;
        private System.Windows.Forms.Label _meritLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _monikerTextBox;
        private System.Windows.Forms.TreeView _pinsTreeView;
        private AutoSizeTextBox _filenameLabel;
    }
}
