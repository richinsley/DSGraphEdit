using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;

using DirectShowLib;
using DirectShowLib.DMO;
    
namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public class DMONumericalParam : UserControl
    {
        IMediaParams _param;
        int _paramNum;
        ParamInfo _pInfo;

        private bool _trackbar = true;
        private Button button1;
        private NumericUpDown numericUpDown1;
        private ColorTrackBar colorTrackBar1;

        bool _initialzed = false;

        public DMONumericalParam(IMediaParams param, int paramNum, ParamInfo pInfo)
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            _param = param;
            _paramNum = paramNum;
            _pInfo = pInfo;

            MPData val;
            param.GetParam(_paramNum, out val);

            if (pInfo.mpType == MPType.FLOAT)
            {
                numericUpDown1.DecimalPlaces = 3;
                numericUpDown1.Increment = 0.001M;
            }

            if (pInfo.mpType == MPType.INT && pInfo.mopCaps == MPCaps.Jump)
            {
                // a regular int control
                numericUpDown1.Minimum = _pInfo.mpdMinValue.vInt;
                numericUpDown1.Maximum = _pInfo.mpdMaxValue.vInt;
                numericUpDown1.Value = val.vInt;
            }
            else
            {
                // float
                numericUpDown1.Minimum = (decimal)_pInfo.mpdMinValue.vFloat;
                numericUpDown1.Maximum = (decimal)_pInfo.mpdMaxValue.vFloat;
                numericUpDown1.Value = (decimal)val.vFloat;
            }

            numericUpDown1.ValueChanged += new EventHandler(numericUpDown1_ValueChanged);
            colorTrackBar1.ValueChanged += new ColorTrackBar.ValueChangedEventHandler(colorTrackBar1_ValueChanged);

            // force the colorTrackbar to update it's value and caption
            numericUpDown1_ValueChanged(null, null);
            _initialzed = true;
        }

        void colorTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            float val = (((float)colorTrackBar1.Value / 1000F) * ((float)numericUpDown1.Maximum - (float)numericUpDown1.Minimum)) + (float)numericUpDown1.Minimum;
            if (_pInfo.mpType == MPType.INT)
            {
                colorTrackBar1.Caption = ((int)val).ToString() + " " +_pInfo.szUnitText;
            }
            else
            {
                colorTrackBar1.Caption = val.ToString() + " " + _pInfo.szUnitText;
            }

            if (colorTrackBar1.Tracking)
            {
                numericUpDown1.Value = (decimal)val;
            }
        }

        void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!colorTrackBar1.Tracking)
            {
                colorTrackBar1.Value = (int)((((0 - numericUpDown1.Minimum) + numericUpDown1.Value) / ((0 - numericUpDown1.Minimum) + numericUpDown1.Maximum)) * 1000);
            }

            if (_initialzed)
            {
                // only set the value if the control is fully inititalized
                MPData val = new MPData();
                if (_pInfo.mopCaps == MPCaps.Jump)
                {
                    val.vInt = (int)numericUpDown1.Value;
                }
                else
                {
                    val.vFloat = (float)numericUpDown1.Value;
                }
                _param.SetParam(_paramNum, val);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            button1.Size = new Size(Height, Height);
            button1.Location = new Point(Width - Height, 0);
            colorTrackBar1.Size = new Size(Width - Height, 16);
            colorTrackBar1.Location = new Point(0, (Height - 16) / 2);
            numericUpDown1.Size = new Size(Width - Height, Height);
            numericUpDown1.Location = new Point(0, 0);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DMONumericalParam));
            this.button1 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.colorTrackBar1 = new ColorTrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(126, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 23);
            this.button1.TabIndex = 0;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(3, 3);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 2;
            this.numericUpDown1.Visible = false;
            // 
            // colorTrackBar1
            // 
            this.colorTrackBar1.BarBorderColor = System.Drawing.Color.DarkGreen;
            this.colorTrackBar1.BarColor = System.Drawing.Color.Green;
            this.colorTrackBar1.BarOrientation = Orientations.Horizontal;
            this.colorTrackBar1.Caption = "caption";
            this.colorTrackBar1.CaptionAlpha = ((byte)(255));
            this.colorTrackBar1.CaptionColor = System.Drawing.Color.White;
            this.colorTrackBar1.ControlCornerStyle = CornerStyles.Rounded;
            this.colorTrackBar1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.colorTrackBar1.Location = new System.Drawing.Point(0, 6);
            this.colorTrackBar1.Maximum = 1000;
            this.colorTrackBar1.MaximumValueSide = Poles.Right;
            this.colorTrackBar1.Minimum = 0;
            this.colorTrackBar1.Name = "colorTrackBar1";
            this.colorTrackBar1.Size = new System.Drawing.Size(126, 16);
            this.colorTrackBar1.TabIndex = 1;
            this.colorTrackBar1.Text = "colorTrackBar1";
            this.colorTrackBar1.TrackerBorderColor = System.Drawing.Color.ForestGreen;
            this.colorTrackBar1.TrackerColor = System.Drawing.Color.LightGreen;
            this.colorTrackBar1.TrackerSize = 16;
            this.colorTrackBar1.Value = 25;
            // 
            // DMONumericalParam
            // 
            this.Controls.Add(this.colorTrackBar1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericUpDown1);
            this.Name = "DMONumericalParam";
            this.Size = new System.Drawing.Size(150, 25);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _trackbar = !_trackbar;
            if (_trackbar)
            {
                colorTrackBar1.Visible = true;
                numericUpDown1.Visible = false;
            }
            else
            {
                colorTrackBar1.Visible = false;
                numericUpDown1.Visible = true;
            }
        }
    }
}
