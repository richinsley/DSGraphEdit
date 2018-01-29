using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace DaggerLib.DSGraphEdit
{
    /// <summary>
    /// Summary description for GradientRange.
    /// </summary>
    [ToolboxItem(false)]
    public class TimeSliderControl : System.Windows.Forms.UserControl
    {
        private Pen blackPen = new Pen(Color.White, 1);
        private Pen TimePen = new Pen(Color.LightBlue);
        private Brush TextBrush = new SolidBrush(Color.Black);
        private Font myFont = null;
        private int TimeStampWidth = 0;

        private Color color1 = Color.Black;
        private Color color2 = Color.White;
        private Color color3 = Color.Black;
        private ColorSliderType type = ColorSliderType.Gradient;
        private bool doubleArrow = true;
        private Bitmap arrow;
        private int min = 0, max = 1000;
        private int width;
        private int height = 10;
        private int extent = 1000;
        private int trackMode = 0;
        private int position = 0;

        // values changed event
        public event EventHandler ValuesChanged;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public TimeSliderControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            // create arrow bitmap
            arrow = new Bitmap(9, 6);
            Graphics g = Graphics.FromImage(arrow);
            Pen p = new Pen(Color.Black);
            Brush b = new SolidBrush(Color.White);
            g.FillRectangle(b, 0, 0, 8, 5);
            g.DrawLine(p, 4, 1, 7, 4);
            g.DrawLine(p, 7, 4, 1, 4);
            g.DrawLine(p, 1, 4, 4, 1);

            myFont = new Font("MicroSoft Sans Serif", 7f);
            TimeStampWidth = (int)g.MeasureString("0:00:00;00", myFont).Width;

            g.Dispose();
            p.Dispose();
            b.Dispose();

            arrow.MakeTransparent(Color.FromArgb(255, 255, 255));

            width = this.Width - 6;
        }

        // Color1 property
        [DefaultValue(typeof(Color), "Black")]
        public Color Color1
        {
            get { return color1; }
            set
            {
                color1 = value;
                Invalidate();
            }
        }
        // Color2 property
        [DefaultValue(typeof(Color), "White")]
        public Color Color2
        {
            get { return color2; }
            set
            {
                color2 = value;
                Invalidate();
            }
        }
        // Color3 property
        [DefaultValue(typeof(Color), "Black")]
        public Color Color3
        {
            get { return color3; }
            set
            {
                color3 = value;
                if (TimePen != null) TimePen.Dispose();
                TimePen = new Pen(value);
                Invalidate();
            }
        }

        // Type property
        [DefaultValue(ColorSliderType.Gradient)]
        public ColorSliderType Type
        {
            get { return type; }
            set
            {
                type = value;
                if (type != ColorSliderType.Gradient)
                    DoubleArrow = true;
                Invalidate();
            }
        }
        // Min property
        [DefaultValue(0)]
        public int Min
        {
            get { return min; }
            set
            {
                min = value;
                if (position < min) Pos = min;
                Invalidate();
            }
        }

        // Min property
        [DefaultValue(1000)]
        public int Extent
        {
            get { return extent; }
            set
            {
                extent = value;
                Invalidate();
            }
        }

        // Max property
        [DefaultValue(1000)]
        public int Max
        {
            get { return max; }
            set
            {
                if (value > extent) value = extent;
                max = value;
                if (position > max) Pos = max;
                Invalidate();
            }
        }

        public int Pos
        {
            get { return position; }
            set
            {
                if (value > max) value = max;
                if (value < min) value = min;
                position = value;
                Invalidate();
            }
        }

        // DoubleArrow property
        [DefaultValue(true)]
        public bool DoubleArrow
        {
            get { return doubleArrow; }
            set
            {
                doubleArrow = value;
                if (!doubleArrow)
                    Type = ColorSliderType.Gradient;
                Invalidate();
            }
        }

        public bool Tracking
        {
            get
            {
                if (this.trackMode > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public TimeSliderTrackMode TrackMode
        {
            get
            {
                return (TimeSliderTrackMode)trackMode;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    blackPen.Dispose();
                    arrow.Dispose();
                    components.Dispose();
                    myFont.Dispose();
                    TextBrush.Dispose();
                    TimePen.Dispose();
                }
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
            // 
            // GradientRange
            // 
            this.Name = "GradientRange";
            this.Resize += new System.EventHandler(this.GradientRange_Resize);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GradientRange_MouseUp);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GradientRange_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GradientRange_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GradientRange_MouseDown);

        }
        #endregion

        private void GradientRange_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rc = this.ClientRectangle;
            //rc.Width =- 6;

            Brush brush;
            int x = 3;
            int y = 12;


            switch (type)
            {
                case ColorSliderType.Gradient:
                case ColorSliderType.InnerGradient:
                case ColorSliderType.OuterGradient:

                    // create gradient brush
                    brush = new LinearGradientBrush(new Point(x, 0), new Point(x + this.Width, 0), color1, color2);

                    g.FillRectangle(brush, x, y, width, height);
                    brush.Dispose();

                    // check type
                    if (type == ColorSliderType.InnerGradient)
                    {
                        // inner gradient
                        brush = new SolidBrush(color3);

                        if (min != 0)
                        {
                            g.FillRectangle(brush,
                                x,
                                y,
                                (min * ((float)width / extent)),
                                height);
                        }
                        if (max != extent)
                        {
                            g.FillRectangle(brush,
                                x + (max * ((float)width / extent)) + 1,
                                y,
                                (extent * ((float)width / extent)) - (max * ((float)width / extent)),
                                height);
                        }
                        brush.Dispose();
                    }
                    else if (type == ColorSliderType.OuterGradient)
                    {
                        // outer gradient
                        brush = new SolidBrush(color3);
                        // fill space between min & max with color 3
                        g.FillRectangle(brush,
                            x + (min * ((float)width / extent)),
                            y,
                            (max * ((float)width / extent)) - (min * ((float)width / extent)) + 1,
                            height);

                        brush.Dispose();
                    }
                    break;
                case ColorSliderType.Threshold:
                    // 1 - fill with color 1
                    brush = new SolidBrush(color1);
                    g.FillRectangle(brush, x, y, width, height);
                    brush.Dispose();
                    // 2 - fill space between min & max with color 2
                    brush = new SolidBrush(color2);
                    if (extent == 0) extent = 1;
                    g.FillRectangle(brush,
                        x + (min * ((float)width / extent)),
                        y,
                        (max * ((float)width / extent)) - (min * ((float)width / extent)) + 1,
                        height);

                    brush.Dispose();
                    break;
            }

            // draw rectangle around the control
            g.DrawRectangle(blackPen, x, y - 1, width, height + 1);

            //draw time marker
            g.DrawLine(TimePen, x + (position * ((float)width / extent)), y, x + (position * ((float)width / extent)), y + height);

            // draw arrows
            x -= 4;
            y += 1 + height;

            if (this.doubleArrow)
            {
                g.DrawImage(arrow, x + (min * ((float)width / extent)), y, 9, 6);
                g.DrawImage(arrow, x + (max * ((float)width / extent)), y, 9, 6);
            }
            else
            {
                g.DrawImage(arrow, x + (min * ((float)width / extent)), y, 9, 6);
            }

            //position time stamp
            PointF stampLoc = new PointF((position * ((float)width / extent)), 0.0f);
            if ((stampLoc.X + TimeStampWidth) > this.width) stampLoc.X = this.width - TimeStampWidth;
            g.DrawString(TimeCodeString(position), myFont, TextBrush, stampLoc);

            //min time stamp
            stampLoc.Y = 26;
            stampLoc.X = (min * ((float)width / extent)) - TimeStampWidth;
            g.DrawString(TimeCodeString(min), myFont, TextBrush, stampLoc);

            stampLoc.X = max * ((float)width / extent);
            g.DrawString(TimeCodeString(max), myFont, TextBrush, stampLoc);
        }

        private void GradientRange_Resize(object sender, System.EventArgs e)
        {
            width = this.Width - 6;
        }

        private void GradientRange_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int x = 3;
            int y = 12 + height;

            // check Y coordinate
            if ((e.Y >= y) && (e.Y < y + 6))
            {
                // check X coordinate
                if (this.doubleArrow && (e.X >= 2 - x + (max * ((float)width / extent))) && (e.X < x + (max * ((float)width / extent)) + 8))
                {
                    // right arrow
                    trackMode = 2;
                }
                else if ((e.X >= 2 - x + (min * ((float)width / extent))) && (e.X < x + (min * ((float)width / extent)) + 8))
                {
                    // left arrow
                    trackMode = 1;
                }
            }
            // check Y coordinate
            if (e.Y < y)
            {
                // check X coordinate
                //if(this.doubleArrow && (e.X >= 2 - x + ( position * ((float)width / extent))) && (e.X < x + ( position * ((float)width / extent)) + 8) )
                //{
                // right arrow
                trackMode = 3;
                //}
            }

            if (trackMode != 0)
                this.Capture = true;
        }

        private void GradientRange_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (trackMode != 0)
            {
                // release capture
                this.Capture = false;
                trackMode = 0;
            }
        }

        private void GradientRange_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (trackMode != 0)
            {
                if (trackMode == 1)
                {
                    // left arrow tracking
                    min = (int)((float)(e.X - 3) / ((float)width / (float)extent));
                    min = Math.Max(min, 0);
                    min = Math.Min(min, extent);
                    if (min > max) max = min;
                    if (position < min) position = min;

                }
                if (trackMode == 2)
                {
                    // right arrow tracking
                    max = (int)((float)(e.X - 3) / ((float)width / (float)extent));
                    max = Math.Max(max, 0);
                    max = Math.Min(max, extent);
                    if (max < min) min = max;
                    if (position > max) position = max;
                }
                if (trackMode == 3)
                {
                    //time marker tracking
                    position = (int)((float)(e.X - 3) / ((float)width / (float)extent));
                    position = Math.Max(position, 0);
                    position = Math.Min(position, extent);
                    if (position > max) position = max;
                    if (position < min) position = min;
                }

                // notify client
                if (ValuesChanged != null)
                    ValuesChanged(this, new EventArgs());

                // repaint control
                Invalidate();
            }
            else
            {
                //change the cursor if needed
                int x = 3;
                int y = 12 + height;
                Cursor = Cursors.Default;
                // check Y coordinate
                if ((e.Y >= y) && (e.Y < y + 6))
                {
                    // check X coordinate
                    if (this.doubleArrow && (e.X >= 2 - x + (max * ((float)width / extent))) && (e.X < x + (max * ((float)width / extent)) + 8))
                    {
                        Cursor = Cursors.SizeWE;
                    }
                    else if ((e.X >= 2 - x + (min * ((float)width / extent))) && (e.X < x + (min * ((float)width / extent)) + 8))
                    {
                        Cursor = Cursors.SizeWE;
                    }
                }
                // check Y coordinate
                if (e.Y < y)
                {
                    // check X coordinate
                    if (this.doubleArrow && (e.X >= 2 - x + (position * ((float)width / extent))) && (e.X < x + (position * ((float)width / extent)) + 8))
                    {
                        Cursor = Cursors.VSplit;
                    }
                }
            }
        }

        public static string TimeCodeString(int msecs)
        {
            //frames
            int timebase = msecs % 1000;
            string frames = ((int)((float)timebase / 33.3333333333f)).ToString();
            if (frames.Length == 1) frames = "0" + frames;
            msecs -= timebase;

            //seconds
            timebase = msecs % 60000;
            string secs = (timebase / 1000).ToString();
            if (secs.Length == 1) secs = "0" + secs;
            msecs -= timebase;

            //minutes
            timebase = msecs % 3600000;
            string mins = (timebase / 60000).ToString();
            if (mins.Length == 1) mins = "0" + mins;
            msecs -= timebase;

            return (msecs / 3600000).ToString() + ":" + mins + ":" + secs + ";" + frames;
        }
    }

    // ColorSliderType enumeration
    public enum ColorSliderType
    {
        Gradient,
        InnerGradient,
        OuterGradient,
        Threshold
    }

    public enum TimeSliderTrackMode
    {
        None,
        StartTime,
        StopTime,
        CurrentTime
    }
}