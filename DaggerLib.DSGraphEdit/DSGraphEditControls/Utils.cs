using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    public static class DaggerDSUtils
    {
        //A (modified) definition of OleCreatePropertyFrame found here: http://groups.google.no/group/microsoft.public.dotnet.languages.csharp/browse_thread/thread/db794e9779144a46/55dbed2bab4cd772?lnk=st&q=[DllImport(%22olepro32.dll%22)]&rnum=1&hl=no#55dbed2bab4cd772
        [DllImport("olepro32.dll")]
        public static extern int OleCreatePropertyFrame(
            IntPtr hwndOwner,
            int x,
            int y,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszCaption,
            int cObjects,
            [MarshalAs(UnmanagedType.Interface, ArraySubType = UnmanagedType.IUnknown)] 
			ref object ppUnk,
            int cPages,
            IntPtr lpPageClsID,
            int lcid,
            int dwReserved,
            IntPtr lpvReserved);

        [DllImport("ole32.dll", ExactSpelling = true)]
        internal static extern int GetRunningObjectTable(int r, out IRunningObjectTable pprot);

        [DllImport("ole32.dll")]
        internal static extern int CreateBindCtx(uint reserved, out IBindCtx pctx);

        /// <summary>
        /// Create a list of DSGrapheditROTEntry from IFilterGraphs on the ROT
        /// </summary>
        /// <returns></returns>
        public static List<DSGrapheditROTEntry> GetFilterGraphsFromROT()
        {
            IRunningObjectTable rtt = null;
            IEnumMoniker enumMon = null;
            List<DSGrapheditROTEntry> rots = new List<DSGrapheditROTEntry>();

            int hr = GetRunningObjectTable(0, out rtt);
            rtt.EnumRunning(out enumMon);
            enumMon.Reset();
            if (hr != 1)
            {
                try
                {
                    IMoniker[] mon = new IMoniker[1];
                    while ((enumMon.Next(1, mon, IntPtr.Zero) == 0))
                    {
                        try
                        {
                            IBindCtx pctx; string displayName;
                            CreateBindCtx(0, out pctx);
                            // Get the name of the file
                            mon[0].GetDisplayName(pctx, null, out displayName);
                            // Clean up
                            Marshal.ReleaseComObject(pctx);
                            if (displayName.StartsWith("!FilterGraph"))
                            {
                                rots.Add(new DSGrapheditROTEntry(displayName, mon[0]));
                            }
                            else
                            {
                                Marshal.ReleaseComObject(mon[0]);
                            }
                        }
                        catch
                        {
                            Marshal.ReleaseComObject(mon[0]);
                            //throw;
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(enumMon);
                }
            }

            return rots;
        }
    }

    public class DSGrapheditROTEntry : IDisposable
    {
        private string _rotDisplayName;
        private IntPtr _filterPtr = IntPtr.Zero;
        private int _pid = 0;
        private string _processName = string.Empty;
        private object _filterGraph;

        private IMoniker _mon;

        public DSGrapheditROTEntry(string displayName, IMoniker mon)
        {
            _rotDisplayName = displayName;
            _mon = mon;

            // parse the pointers
            string[] tokens = displayName.Split(' ');
            if (tokens.Length == 4)
            {
                _filterPtr = new IntPtr(Convert.ToInt64(tokens[1], 16));
                _pid = Convert.ToInt32(tokens[3], 16);
            }

            Process proc = Process.GetProcessById(_pid);
            if (proc != null)
            {
                _processName = proc.ProcessName;
            }
        }

        /// <summary>
        /// Get the IntPtr to the FilterGraph stored in the ROT
        /// </summary>
        public IntPtr FilterGraphPtr
        {
            get
            {
                return _filterPtr;
            }
        }

        /// <summary>
        /// Get the Process ID of the FilterGraph stored in the ROT
        /// </summary>
        public int PID
        {
            get
            {
                return _pid;
            }
        }

        public IFilterGraph ConnectToROTEntry()
        {
            IRunningObjectTable rtt = null;
            int hr = DaggerDSUtils.GetRunningObjectTable(0, out rtt);
            hr = rtt.GetObject(_mon, out _filterGraph);
            Marshal.ReleaseComObject(rtt);
            return _filterGraph as IFilterGraph;
        }

        public override string ToString()
        {
            return _processName + " PID (" + _pid.ToString("X") + ") IFilterGraph = " + _filterPtr.ToString("X");
        }

        public void Dispose()
        {
            Marshal.ReleaseComObject(_mon);
        }
    }
}
