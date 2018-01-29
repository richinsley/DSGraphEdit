namespace DaggerLib.UI.Windows
{
    partial class TypeConstantNodeUI
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
            System.ComponentModel.StringConverter stringConverter1 = new System.ComponentModel.StringConverter();
            this.genericValueEditor = new GenericValueEditor();
            this.InternalControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // InternalControl
            // 
            this.InternalControl.Controls.Add(this.genericValueEditor);
            this.InternalControl.Size = new System.Drawing.Size(134, 20);
            // 
            // genericValueEditor
            // 
            this.genericValueEditor.Converter = stringConverter1;
            this.genericValueEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.genericValueEditor.Location = new System.Drawing.Point(0, 0);
            this.genericValueEditor.Name = "genericValueEditor";
            this.genericValueEditor.Size = new System.Drawing.Size(134, 20);
            this.genericValueEditor.TabIndex = 0;
            this.genericValueEditor.ValueChanged += new System.EventHandler(this.genericValueEditor_ValueChanged);
            // 
            // TypeConstantNodeUI
            // 
            this.Name = "TypeConstantNodeUI";
            this.Size = new System.Drawing.Size(166, 44);
            this.InternalControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public GenericValueEditor genericValueEditor;

    }
}
