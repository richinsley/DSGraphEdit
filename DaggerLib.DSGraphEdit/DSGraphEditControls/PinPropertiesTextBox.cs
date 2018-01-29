using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    [ToolboxItem(false)]
    public class PinPropertiesTextBox : TextBox
    {
        private IPin _pin;

        public PinPropertiesTextBox(IPin pin)
        {
            ReadOnly = true;
            Multiline = true;
            Dock = DockStyle.Fill;
            ScrollBars = ScrollBars.Both;
            WordWrap = false;
            BorderStyle = BorderStyle.FixedSingle;
            _pin = pin;
            RefreshProperties();
        }

        public void RefreshProperties()
        {
            this.Clear();

            int hr = 0;

            // if the pin is connected, get it's ConnectionMediaType
            bool added = false;
            AMMediaType contype = new AMMediaType();
            try
            {
                hr = _pin.ConnectionMediaType(contype);
                if (hr == 0)
                {
                    AppendText(ConnectionMediaTypeString(contype));
                    DsUtils.FreeAMMediaType(contype);
                    return;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.Message,"Error getting media connection type");
#endif
            }

            // the pin's not connected, so get each of the prefered media types for the pin
            AppendText("Prefered Media Types:\r\n");
            IEnumMediaTypes penum = null;
            hr = _pin.EnumMediaTypes(out penum);
            if (hr == 0 && penum != null)
            {
                AMMediaType[] mtypes = new AMMediaType[1];
                IntPtr fetched = Marshal.AllocCoTaskMem(4);
                try
                {
                    while (penum.Next(1, mtypes, fetched) == 0)
                    {
                        AppendText(ConnectionMediaTypeString(mtypes[0]));
                        DsUtils.FreeAMMediaType(mtypes[0]);
                        added = true;
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.Message, "Error getting pin prefered type");
#endif
                }
                finally
                {
                    Marshal.FreeCoTaskMem(fetched);
                }

                // if we added no prefered media types to the textbox, set it to "None"
                if (added == false)
                {
                    AppendText("None\r\n");
                }
                Marshal.ReleaseComObject(penum);
            }
        }

        private string ConnectionMediaTypeString(AMMediaType mtype)
        {
            string retval = 
                "Major type:\t" + DsToString.MediaTypeToString(mtype.majorType) + "\r\n" +
                "Subtype type:\t" + this.MediaSubTypeToString(mtype.subType) + "\r\n" +
                "Format type:\t" + DsToString.MediaFormatTypeToString(mtype.formatType) + "\r\n";
            if (mtype.formatPtr != IntPtr.Zero)
            {
                if (mtype.formatType == FormatType.WaveEx)
                {
                    retval += WaveFormatExString(mtype);
                }
                else if (mtype.formatType == FormatType.VideoInfo || mtype.formatType == FormatType.MpegVideo)
                {
                    retval += VideoInfoFormatString(mtype);
                }
                else if (mtype.formatType == FormatType.VideoInfo2 || mtype.formatType == FormatType.Mpeg2Video)
                {
                    retval += VideoInfo2FormatString(mtype);
                }
            }
            return retval + "---\r\n";
        }

        private string VideoInfoFormatString(AMMediaType mtype)
        {
            VideoInfoHeader vih = new VideoInfoHeader();
            Marshal.PtrToStructure(mtype.formatPtr, vih);
            return "\t\trcSrc " + vih.SrcRect.ToRectangle().ToString() + "\r\n" +
                    "\t\trcDest " + vih.TargetRect.ToRectangle().ToString() + "\r\n";
        }

        private string VideoInfo2FormatString(AMMediaType mtype)
        {
            VideoInfoHeader2 vih = new VideoInfoHeader2();
            Marshal.PtrToStructure(mtype.formatPtr, vih);
            return "\t\tAspect Ratio: " + vih.PictAspectRatioX.ToString() + "x" + vih.PictAspectRatioY.ToString() + "\r\n" +
                    "\t\tInterlace Format: " + vih.InterlaceFlags.ToString() + "\r\n" +
                    "\t\trcSrc " + vih.SrcRect.ToRectangle().ToString() + "\r\n" +
                    "\t\trcDest " + vih.TargetRect.ToRectangle().ToString() + "\r\n" +
                    "\t\tCtrlFlags " + vih.ControlFlags.ToString() + "\r\n";
        }

        private string WaveFormatExString(AMMediaType mtype)
        {
            WaveFormatEx wfex = new WaveFormatEx();
            Marshal.PtrToStructure(mtype.formatPtr, wfex);
            return "\t\t" + wfex.nSamplesPerSec.ToString() + " KHz \r\n" +
                    "\t\t" + wfex.wBitsPerSample.ToString() + " bit \r\n" +
                    "\t\t" + wfex.nChannels.ToString() + " channels \r\n";
        }

        /// <summary>
        /// The MediaSubTypeToString function in DirectShowLib fails to properly decode the FourCC of 
        /// guids that start with 0x0000.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private string MediaSubTypeToString(Guid guid)
        {
            string s = DsToString.MediaSubTypeToString(guid);
            if (s.Contains("\0"))
            {
                return guid.ToString();
            }
            else
            {
                return s;
            }
        }
    }
}
