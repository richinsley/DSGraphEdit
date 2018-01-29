using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;

namespace DaggerLib.DSGraphEdit
{
    public sealed class InterfaceScanner
    {
        static InterfaceScanner instance = null;
        private List<InterfacePair> _pairs = new List<InterfacePair>();

        private InterfaceScanner()
        {
            RegistryKey MyKey = Registry.ClassesRoot.OpenSubKey(@"Interface\");
            string[] s = MyKey.GetSubKeyNames();
            for (int i = 0; i < s.Length; i++)
            {
                RegistryKey tk = MyKey.OpenSubKey(s[i]);
                string name = (string)tk.GetValue("");
                tk.Close();

                if (s[i].StartsWith("{"))
                {
                    InterfacePair ip = new InterfacePair();
                    ip.InterfaceGuid = new Guid(s[i]);
                    ip.InterfaceName = name;
                    _pairs.Add(ip);
                }
            }
            MyKey.Close();
        }

        /// <summary>
        /// Performs a brute force scan of all interfaces the COM object implements
        /// </summary>
        /// <param name="assembly">optional Assembly to scan against</param>
        /// <param name="o">Object to scan</param>
        /// <returns>List of Pairs of Interface Name/Interface GUID</returns>
        public static List<InterfacePair> Scan(Assembly[] assemblies, object o)
        {
            if (instance == null)
            {
                instance = new InterfaceScanner();
            }
   
            Dictionary<string, Guid> hash = new Dictionary<string, Guid>();
            List<InterfacePair> pairs = new List<InterfacePair>();

            IntPtr ukn = Marshal.GetIUnknownForObject(o);
            foreach (InterfacePair ip in instance._pairs)
            {
                IntPtr iptr = IntPtr.Zero;
                Marshal.QueryInterface(ukn, ref ip.InterfaceGuid, out iptr);
                if (iptr != IntPtr.Zero)
                {
                    hash.Add(ip.InterfaceName, ip.InterfaceGuid);
                    Marshal.Release(iptr);
                }
            }

            // scan against the array assembly if one was given
            if (assemblies != null)
            {
                for (int x = 0; x < assemblies.Length; x++)
                {
                    Type[] asstypes = assemblies[x].GetTypes();
                    for (int i = 0; i < asstypes.Length; i++)
                    {
                        if (asstypes[i].IsInterface)
                        {
                            IntPtr iptr = IntPtr.Zero;
                            Guid g = asstypes[i].GUID;
                            Marshal.QueryInterface(ukn, ref g, out iptr);
                            if (iptr != IntPtr.Zero)
                            {
                                if (!hash.ContainsKey(asstypes[i].Name))
                                {
                                    hash.Add(asstypes[i].Name, asstypes[i].GUID);
                                }
                                Marshal.Release(iptr);
                            }
                        }
                    }
                }
            }

            Marshal.Release(ukn);

            // if it's a managed type, get all of it's interfaces also
            if (!o.GetType().IsCOMObject)
            {
                Type[] t = o.GetType().GetInterfaces();
                for (int i = 0; i < t.Length; i++)
                {
                    if (!hash.ContainsKey(t[i].Name))
                    {
                        hash.Add(t[i].Name, t[i].GUID);
                    }
                }
            }

            // flatten the dictionary into a list
            foreach (KeyValuePair<string, Guid> kvp in hash)
            {
                InterfacePair ip = new InterfacePair();
                ip.InterfaceName = kvp.Key;
                ip.InterfaceGuid = kvp.Value;
                pairs.Add(ip);
            }

            return pairs;
        }
    }

    public class InterfacePair
    {
        public Guid InterfaceGuid;
        public string InterfaceName;
    }
}
