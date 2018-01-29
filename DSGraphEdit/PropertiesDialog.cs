using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DaggerLib.UI.Windows;
using DaggerLib.DSGraphEdit;

namespace DSGraphEdit
{
    public partial class PropertiesDialog : Form
    {
        public PropertiesDialog(DSGraphEditPanelProperties properties)
        {
            InitializeComponent();
            okButton.DialogResult = DialogResult.OK;
            cancelButton.DialogResult = DialogResult.Cancel;

            // populate the ui elements
            for (int i = 0; i < 7; i++)
            {
                noodleStyleComboBox.Items.Add(((NoodleStyle)i).ToString());
            }
            noodleStyleComboBox.SelectedIndex = (int)properties.NoodleStyle;

            for (int i = 0; i < 3; i++)
            {
                pinPlacementCombobox.Items.Add(((DaggerNodePinPlacement)i).ToString());
            }
            pinPlacementCombobox.SelectedIndex = (int)properties.PinPlacement;

            canvasColorButton.BackColor = properties.CanvasBackColor;
            dropShadowVisibleCheckBox.Checked = properties.DropShadowVisible;
            modalPropertiesCheckbox.Checked = properties.ModalProperties;
            showTimeSliderCheckbox.Checked = properties.ShowTimeSlider;
            showPinNamesCheckbox.Checked = properties.ShowPinNames;
        }

        public DSGraphEditPanelProperties DSGraphEditPanelProperties
        {
            get
            {
                DSGraphEditPanelProperties properties = new DSGraphEditPanelProperties();
                properties.CanvasBackColor = canvasColorButton.BackColor;
                properties.DropShadowVisible = dropShadowVisibleCheckBox.Checked;
                properties.ModalProperties = modalPropertiesCheckbox.Checked;
                properties.ShowTimeSlider = showTimeSliderCheckbox.Checked;
                properties.ShowPinNames = showPinNamesCheckbox.Checked;
                properties.NoodleStyle = (NoodleStyle)noodleStyleComboBox.SelectedIndex;
                properties.PinPlacement = (DaggerNodePinPlacement)pinPlacementCombobox.SelectedIndex;
                return properties;
            }
        }

        private void canvasColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = canvasColorButton.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                canvasColorButton.BackColor = cd.Color;
            }
            cd.Dispose();
            cd = null;
        }
    }
}