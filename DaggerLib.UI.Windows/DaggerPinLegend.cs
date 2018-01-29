using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DaggerLib.UI.Windows
{
    /// <summary>
    /// Class containing a HashTable of Pin images and thier associated data types
    /// </summary>
    public class DaggerPinLegend
    {
        #region Fields

        private Dictionary<Type, DaggerPinLegendElement> _pins;
        private int _pinSize;
        private DaggerPinLegendElement _defaultPin;

        #endregion

        #region ctor

        public DaggerPinLegend(int pinSize)
        {
            _pins = new Dictionary<Type, DaggerPinLegendElement>();
            _pinSize = pinSize;
            _defaultPin = new DaggerPinLegendElement(pinSize, Color.LightGray);
        }

        #endregion

        #region Properties

        public int PinSize
        {
            get
            {
                return _pinSize;
            }
        }

        public DaggerPinLegendElement this[Type t]
        {
            get
            {
                if (_pins.ContainsKey(t))
                {
                    return _pins[t];
                }
                else
                {
                    return _defaultPin;
                }
            }
            set
            {
                if (t == typeof(object))
                {
                    _defaultPin = value;
                }
                else
                {
                    _pins.Add(t, value);
                }
            }
        }

        #endregion

        #region Methods

        public void AddPinType(Type t, Color color)
        {

            this[t] = new DaggerPinLegendElement(_pinSize, color);
        }

        public void AddPinType(Type t, Bitmap InputNotConnected, Bitmap InputConnected, Bitmap OutputNotConnected, Bitmap OutputConnected, Color pinColor,Color transparencyKey)
        {
            this[t] = new DaggerPinLegendElement(_pinSize, InputNotConnected, InputConnected, OutputNotConnected, OutputConnected, pinColor, transparencyKey);
        }

        public void AddPinType(Type copyType, Type newType, Color pinColor)
        {
            this[newType] = new DaggerPinLegendElement(this[copyType], pinColor);
        }

        #endregion
    }

    public class DaggerPinLegendElement
    {
        #region Fields

        private Bitmap _inputPinImageConnected;
        private Region _inputPinRegionConnected;

        private Bitmap _inputPinImageDisconnected;
        private Region _inputPinRegionDisconnected;

        private Color _pinInputConnectedTrans = Color.Red;
        private Color _pinInputDisconnectedTrans = Color.Red;

        private Bitmap _outputPinImageConnected;
        private Region _outputPinRegionConnected;

        private Bitmap _outputPinImageDisconnected;
        private Region _outputPinRegionDisconnected;

        private Color _pinOutputConnectedTrans = Color.Red;
        private Color _pinOutputDisconnectedTrans = Color.Red;

        private Color _noodleColor;

        private int _pinSize;

        #endregion

        #region ctor

        /// <summary>
        /// Constructor to create default pin images
        /// </summary>
        /// <param name="pinSize"></param>
        /// <param name="color"></param>
        internal DaggerPinLegendElement(int pinSize, Color color)
        {
            _noodleColor = color;
            _pinSize = pinSize;
            
            //create default pin images and calculate the regions
            Brush b = new SolidBrush(color);

            _inputPinImageConnected = new Bitmap(pinSize, pinSize);
            Graphics g = Graphics.FromImage(_inputPinImageConnected);
            g.Clear(Color.Red);
            g.FillEllipse(b, 0, 0, pinSize - 1, pinSize - 1);
            g.DrawEllipse(Pens.Black, 0, 0, pinSize - 1, pinSize - 1);
            g.Dispose();

            _inputPinImageDisconnected = new Bitmap(pinSize, pinSize);
            g = Graphics.FromImage(_inputPinImageDisconnected);
            g.Clear(Color.Red);
            g.FillEllipse(b, 0, 0, pinSize - 1, pinSize - 1);
            g.DrawEllipse(Pens.Black, 0, 0, pinSize - 1, pinSize - 1);
            int center = pinSize / 2;
            g.DrawRectangle(Pens.Black, center - 1, center - 1, 2, 2);
            g.Dispose();

            b.Dispose();

            //copy them to the output pin images
            _outputPinImageConnected = (Bitmap)_inputPinImageConnected.Clone();
            _outputPinImageDisconnected = (Bitmap)_inputPinImageDisconnected.Clone();

            //create the regions
            DaggerPinLegendElement.CalculatePinRegion(_outputPinImageDisconnected, _pinOutputDisconnectedTrans, ref _outputPinRegionDisconnected);
            DaggerPinLegendElement.CalculatePinRegion(_outputPinImageConnected, _pinOutputConnectedTrans, ref _outputPinRegionConnected);
            DaggerPinLegendElement.CalculatePinRegion(_inputPinImageDisconnected, _pinInputDisconnectedTrans, ref _inputPinRegionDisconnected);
            DaggerPinLegendElement.CalculatePinRegion(_inputPinImageConnected, _pinInputConnectedTrans, ref _inputPinRegionConnected);
        }

        internal DaggerPinLegendElement(int pinsize, Bitmap InputNotConnected, Bitmap InputConnected, Bitmap OutputNotConnected, Bitmap OutputConnected, Color pinColor,Color transparenceKey)
        {
            _noodleColor = pinColor;
            _pinSize = pinsize;

            _inputPinImageConnected = CopyAndRecolorBitmap(InputConnected, transparenceKey, pinsize, pinColor);
            _inputPinImageDisconnected = CopyAndRecolorBitmap(InputNotConnected, transparenceKey, pinsize, pinColor);
            _outputPinImageConnected = CopyAndRecolorBitmap(OutputConnected, transparenceKey, pinsize, pinColor);
            _outputPinImageDisconnected = CopyAndRecolorBitmap(OutputNotConnected, transparenceKey, pinsize, pinColor);

            //create the regions
            DaggerPinLegendElement.CalculatePinRegion(_outputPinImageDisconnected, _pinOutputDisconnectedTrans, ref _outputPinRegionDisconnected);
            DaggerPinLegendElement.CalculatePinRegion(_outputPinImageConnected, _pinOutputConnectedTrans, ref _outputPinRegionConnected);
            DaggerPinLegendElement.CalculatePinRegion(_inputPinImageDisconnected, _pinInputDisconnectedTrans, ref _inputPinRegionDisconnected);
            DaggerPinLegendElement.CalculatePinRegion(_inputPinImageConnected, _pinInputConnectedTrans, ref _inputPinRegionConnected);
        }

        internal DaggerPinLegendElement(DaggerPinLegendElement copyFrom, Color pinColor)
        {
            _noodleColor = pinColor;
            _pinSize = copyFrom._pinSize;

            _inputPinImageConnected = CopyAndRecolorBitmap(copyFrom._inputPinImageConnected, Color.Red, copyFrom._pinSize, pinColor);
            _inputPinImageDisconnected = CopyAndRecolorBitmap(copyFrom._inputPinImageDisconnected, Color.Red, copyFrom._pinSize, pinColor);
            _outputPinImageConnected = CopyAndRecolorBitmap(copyFrom._outputPinImageConnected, Color.Red, copyFrom._pinSize, pinColor);
            _outputPinImageDisconnected = CopyAndRecolorBitmap(copyFrom._outputPinImageDisconnected, Color.Red, copyFrom._pinSize, pinColor);

            //create the regions
            DaggerPinLegendElement.CalculatePinRegion(_outputPinImageDisconnected, _pinOutputDisconnectedTrans, ref _outputPinRegionDisconnected);
            DaggerPinLegendElement.CalculatePinRegion(_outputPinImageConnected, _pinOutputConnectedTrans, ref _outputPinRegionConnected);
            DaggerPinLegendElement.CalculatePinRegion(_inputPinImageDisconnected, _pinInputDisconnectedTrans, ref _inputPinRegionDisconnected);
            DaggerPinLegendElement.CalculatePinRegion(_inputPinImageConnected, _pinInputConnectedTrans, ref _inputPinRegionConnected);

        }

        #endregion

        #region Properties

        public Bitmap InputPinImageConnected
        {
            get
            {
                return _inputPinImageConnected;
            }
        }

        public Bitmap InputPinImageDisconnected
        {
            get
            {
                return _inputPinImageDisconnected;
            }
        }

        public Bitmap OutputPinImageConnected
        {
            get
            {
                return _outputPinImageConnected;
            }
        }

        public Bitmap OutputPinImageDisconnected
        {
            get
            {
                return _outputPinImageDisconnected;
            }
        }

        public Region InputPinRegionConnected
        {
            get
            {
                return _inputPinRegionConnected;
            }
        }

        public Region InputPinRegionDisconnected
        {
            get
            {
                return _inputPinRegionDisconnected;
            }
        }

        public Region OutputPinRegionConnected
        {
            get
            {
                return _outputPinRegionConnected;
            }
        }

        public Region OutputPinRegionDisconnected
        {
            get
            {
                return _outputPinRegionDisconnected;
            }
        }

        public Color InputPinImageConnectedTransparent
        {
            get
            {
                return _pinInputConnectedTrans;
            }
        }

        public Color InputPinImageDisconnectedTransparent
        {
            get
            {
                return _pinInputDisconnectedTrans;
            }
        }

        public Color OutputPinImageConnectedTransparent
        {
            get
            {
                return _pinOutputConnectedTrans;
            }
        }

        public Color OutputPinImageDisconnectedTransparent
        {
            get
            {
                return _pinOutputDisconnectedTrans;
            }
        }

        public Color NoodleColor
        {
            get
            {
                return _noodleColor;
            }
        }

        #endregion

        #region Static

        private static Bitmap CopyAndRecolorBitmap(Bitmap bitmap, Color transparencyKey, int pinSize, Color recolor)
        {
            Bitmap b1 = new Bitmap(pinSize, pinSize);

            //resize and copy with transparency key into b1
            Graphics g = Graphics.FromImage(b1);
            ImageAttributes iat = new ImageAttributes();
            iat.SetColorKey(transparencyKey, transparencyKey);
            // ColorMatrix elements
            float[][] ptsArray =
                    {
                    new float[] {recolor.R / 255f, 0, 0, 0, 0},
                    new float[] {0, recolor.G / 255f, 0, 0, 0},
                    new float[] {0, 0, recolor.B / 255f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                    };
            iat.SetColorMatrix(new ColorMatrix(ptsArray));
            g.DrawImage(bitmap, new Rectangle(0, 0, pinSize, pinSize), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, iat);
            g.Dispose();
            return b1;
        }

        private static void CalculatePinRegion(Bitmap bitmap, Color transparencyKey, ref Region pinRegion)
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = bitmap.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.Left, (int)boundsF.Top,
                               (int)boundsF.Width, (int)boundsF.Height);

            int key = ((transparencyKey.A << 24) |
                              (transparencyKey.R << 16) |
                              (transparencyKey.G << 8) |
                              (transparencyKey.B << 0));

            BitmapData bmpData = bitmap.LockBits(bounds, ImageLockMode.ReadOnly,
                                 PixelFormat.Format32bppArgb);

            // Get the address of the first line.
            IntPtr dataPtr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            // This code is specific to a bitmap with 32 bits per pixels.

            int yMax = (int)boundsF.Height;
            int xMax = (int)boundsF.Width;
            int stride = bmpData.Stride / 4;
            int words = xMax * yMax;
            int[] rgbValues = new int[words];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(dataPtr, rgbValues, 0, words);

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            GraphicsPath path = new GraphicsPath();

            for (int j = 0; j < yMax; j++)
                for (int i = 0; i < xMax; i++)
                {
                    if ((rgbValues[(stride * j) + i] == key) || ((rgbValues[(stride * j) + i] & 0xff000000) == 0))
                        continue;
                    int x0 = i;
                    while ((i < xMax) && (rgbValues[(stride * j) + i] != key) && ((rgbValues[(stride * j) + i] & 0xff000000) != 0))
                        i++;
                    path.AddRectangle(new Rectangle(x0, j, i - x0, 1));
                }

            if(pinRegion != null)
            {
                pinRegion.Dispose();
            }

            pinRegion = new Region(path);
        }
        #endregion
    }
}