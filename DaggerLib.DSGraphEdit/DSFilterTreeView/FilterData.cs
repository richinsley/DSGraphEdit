using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    /// <summary>
    /// A class to hold information about a DirectShow filter registered on the system
    /// </summary>
    public class FilterData
    {
        public int Version;
        public int Merit;
        public int NumPins;
        public List<FilterDataPin> Pins = new List<FilterDataPin>();

        /// <summary>
        /// Constructor from an IMoniker
        /// </summary>
        /// <param name="m_Mon"></param>
        public FilterData(IMoniker m_Mon)
        {
            IPropertyBag bag = null;
            byte[] filterData = null;
            object bagObj = null;
            object val = null;

            try
            {
                Guid bagId = typeof(IPropertyBag).GUID;
                m_Mon.BindToStorage(null, null, ref bagId, out bagObj);
                bag = (IPropertyBag)bagObj;

                bag.Read("FilterData", out val, null);
                filterData = (byte[])val;
            }
            catch
            {
                filterData = null;
            }
            finally
            {
                bag = null;
                if (bagObj != null)
                {
                    Marshal.ReleaseComObject(bagObj);
                    bagObj = null;
                }
            }

            if (filterData != null)
            {
                // get the FilterData version and Merit
                Version = IntFromBytes(filterData, 0);
                Merit = IntFromBytes(filterData, 4);

                // Parse the pins
                NumPins = IntFromBytes(filterData, 8);
                int iPos = 16;
                for (int z = 0; z < NumPins; z++)
                {
                    int iMT = IntFromBytes(filterData, iPos + 12);
                    int iMedium = IntFromBytes(filterData, iPos + 16);
                    int flag = IntFromBytes(filterData, iPos + 4);
                    int inst = IntFromBytes(filterData, iPos + 8);
                    Guid cat = GuidFromOffset(filterData, iPos + 20);

                    FilterDataPin fdp = new FilterDataPin(z,flag,inst,cat);
                    iPos += 24;

                    // Parse the media types for the pin
                    for (int x = 0; x < iMT; x++)
                    {
                        fdp.Types.Add(new FilterDataPinType(x, GuidFromOffset(filterData, iPos + 8), GuidFromOffset(filterData, iPos + 12)));
                        iPos += 16;
                    }

                    // Parse the mediums for the pin
                    for (int y = 0; y < iMedium; y++)
                    {
                        fdp.Mediums.Add(new FilterDataPinType(y, GuidFromOffset(filterData, iPos), Guid.Empty));
                        iPos += 4;
                    }

                    Pins.Add(fdp);
                }
            }
        }

        static int IntFromBytes(byte[] b, int iOffset)
        {
            return (
            b[iOffset] +
            b[iOffset + 1] * 256 +
            b[iOffset + 2] * 256 * 256 +
            b[iOffset + 3] * 256 * 256 * 256
            );
        }

        static Guid GuidFromOffset(byte[] b, int iOffset)
        {
            Guid g;

            int iGuidOffset = IntFromBytes(b, iOffset);

            if (iGuidOffset > 0)
                g = GuidFromBytes(b, iGuidOffset);
            else
                g = new Guid();

            return g;
        }

        static Guid GuidFromBytes(byte[] b, int iOffset)
        {
            byte[] b2 = new byte[16];
            Array.Copy(b, iOffset, b2, 0, 16);

            Guid g = new Guid(b2);

            return g;
        }
    }

    /// <summary>
    /// Class to hold information about pins found in FilterData
    /// </summary>
    public class FilterDataPin
    {
        private int _flag;

        public int PinNumber;
        public int Inst;
        public Guid Category;
        public List<FilterDataPinType> Types = new List<FilterDataPinType>();
        public List<FilterDataPinType> Mediums = new List<FilterDataPinType>();

        public FilterDataPin(int pinNumber, int flag, int inst, Guid category)
        {
            PinNumber = pinNumber;
            _flag = flag;
            Inst = inst;
            Category = category;
        }

        /// <summary>
        /// Gets if the filter can have zero instances of this pin.
        /// </summary>
        public bool PinFlagZero
        {
            get
            {
                return (_flag & 0x1) == 0x1 ? true : false;
            }
        }

        /// <summary>
        /// Gets if the filter renders the input from this pin.
        /// </summary>
        public bool PinFlagRenderer
        {
            get
            {
                return (_flag & 0x2) == 0x2 ? true : false;
            }
        }

        /// <summary>
        /// Gets if the filter can create more than one instance of this pin.
        /// </summary>
        public bool PinFlagMany
        {
            get
            {
                return (_flag & 0x4) == 0x4 ? true : false;
            }
        }

        /// <summary>
        /// Gets if this pin is an output pin.
        /// </summary>
        public bool PinFlagOutput
        {
            get
            {
                return (_flag & 0x8) == 0x8 ? true : false;
            }
        }
    }

    public class FilterDataPinType
    {
        public int TypeNumber;
        public Guid MajorType;
        public Guid SubType;

        public FilterDataPinType(int number, Guid major, Guid minor)
        {
            TypeNumber = number;
            MajorType = major;
            SubType = minor;
        }
    }
}
