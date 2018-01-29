using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    public class DSFilterTreeViewNode : TreeNode
    {
        private Guid _classGuid;
        private string _filterName;
        private string _devicePath;
        private IMoniker _moniker;
        private Guid _dmoCategory;
        private FilterData _filterData;
        private string _filePath = string.Empty;

        private FilterType _filterType = FilterType.DefaultFilter;

        public DSFilterTreeViewNode(DsDevice device, Guid dmoCategory)
        {
            _filterName = GetFriendlyName(device.Mon);
            Text = _filterName;
            _moniker = device.Mon;
            _devicePath = device.DevicePath;

            // get the type of the filter from the device path
            if (_devicePath.StartsWith("@device:sw"))
            {
                _filterType = FilterType.DefaultFilter;
            }
            else if (_devicePath.StartsWith("@device:dmo"))
            {
                _filterType = FilterType.DMO;
            }
            else if (_devicePath.StartsWith("@device:cm"))
            {
                _filterType = FilterType.CompressionManager;
            }
            else if (_devicePath.StartsWith("@device:pnp"))
            {
                _filterType = FilterType.PnP;
            }
            else
            {
                _filterType = FilterType.KSProxy;
            }

            _dmoCategory = dmoCategory;
            if (DSFilterType == FilterType.DMO)
            {
                _classGuid = FindDMOGuid(_filterName, dmoCategory);
            }
            else
            {
                _classGuid = GetMonikerGuid(device.Mon);
            }

            if (_classGuid != Guid.Empty)
            {
                // try to get the path to the clsid from the registry
                RegistryKey clsidKey = Registry.ClassesRoot.OpenSubKey("CLSID\\{" + _classGuid.ToString() + "}\\InprocServer32\\");
                if (clsidKey != null)
                {
                    try
                    {
                        _filePath = (string)clsidKey.GetValue("");

                        if (_filePath.ToLower().EndsWith("mscoree.dll"))
                        {
                            // YOU LIE!!! it's actually a .Net assembly masquerading as a COM clsid
                            _filePath = (string)clsidKey.GetValue("Assembly");
                        }
                    }
                    catch { }
                    finally
                    {
                        clsidKey.Close();
                    }
                }

                // color code according to the Filter Type
                switch (_filterType)
                {
                    case FilterType.DefaultFilter:
                        ForeColor = Color.Black;
                        break;
                    case FilterType.DMO:
                        ForeColor = Color.Green;
                        break;
                    case FilterType.KSProxy:
                        ForeColor = Color.Red;
                        break;
                    case FilterType.CompressionManager:
                        ForeColor = Color.Blue;
                        break;
                    case FilterType.PnP:
                        ForeColor = Color.Purple;
                        break;
                    default:
                        break;
                }

                // parse the filter data from the moniker
                _filterData = new FilterData(device.Mon);
            }
        }

        /// <summary>
        /// Gets the FilterData object for this filter
        /// </summary>
        public FilterData FilterInformation
        {
            get
            {
                return _filterData;
            }
        }

        public FilterType DSFilterType
        {
            get
            {
                return _filterType;
            }
        }

        /// <summary>
        /// Gets the DMO category if it is a DMO wrapped filter
        /// </summary>
        public Guid DMOCategory
        {
            get
            {
                return _dmoCategory;
            }
        }

        public IMoniker Moniker
        {
            get
            {
                return _moniker;
            }
        }

        public string DevicePath
        {
            get
            {
                return _devicePath;
            }
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }

        public Guid ClassGuid
        {
            get
            {
                return _classGuid;
            }
        }

        public override string ToString()
        {
            return _filterName;
        }

        private Guid FindDMOGuid(string gn, Guid cat)
        {
            int hr;

            IEnumDMO pEnum;
            Guid[] g2 = new Guid[1];
            string[] sn = new String[1];

            hr = DMOUtils.DMOEnum(cat, 0, 0, null, 0, null, out pEnum);
            DMOError.ThrowExceptionForHR(hr);

            IntPtr fetched = Marshal.AllocCoTaskMem(4);
            try
            {
                do
                {
                    hr = pEnum.Next(1, g2, sn, fetched);
                } while (hr == 0 && sn[0] != gn);

                // Handle any serious errors
                DMOError.ThrowExceptionForHR(hr);

                if (hr != 0)
                {
                    Console.WriteLine("Cannot find " + gn);
                    return Guid.Empty;
                }
            }
            finally
            {
                Marshal.ReleaseComObject(pEnum);
                Marshal.FreeCoTaskMem(fetched);
            }

            return g2[0];
        }

        /// <summary>
        /// Get the Guid for a moniker
        /// </summary>
        /// <returns>String or null on error</returns>
        public static Guid GetMonikerGuid(IMoniker m_Mon)
        {
            IPropertyBag bag = null;
            Guid ret = Guid.Empty;
            object bagObj = null;
            object val = null;

            try
            {
                Guid bagId = typeof(IPropertyBag).GUID;
                m_Mon.BindToStorage(null, null, ref bagId, out bagObj);
                bag = (IPropertyBag)bagObj;

                int hr = bag.Read("clsid", out val, null);
                DsError.ThrowExceptionForHR(hr);

                ret = new Guid(val as string);
            }
            catch
            {
                ret = Guid.Empty;
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

            return ret;
        }

        /// <summary>
        /// Get the FriendlyName for a moniker
        /// </summary>
        /// <returns>String or null on error</returns>
        public static string GetFriendlyName(IMoniker m_Mon)
        {
            IPropertyBag bag = null;
            string ret = null;
            object bagObj = null;
            object val = null;

            try
            {
                Guid bagId = typeof(IPropertyBag).GUID;
                m_Mon.BindToStorage(null, null, ref bagId, out bagObj);

                bag = (IPropertyBag)bagObj;

                int hr = bag.Read("FriendlyName", out val, null);
                DsError.ThrowExceptionForHR(hr);

                ret = val as string;
            }
            catch
            {
                ret = null;
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

            return ret;
        }
    }

    /// <summary>
    /// The type of DirectShow filter
    /// </summary>
    public enum FilterType
    {
        DefaultFilter,
        DMO,
        KSProxy,
        CompressionManager,
        PnP
    }
}
