using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

using DaggerLib.Core;
using DaggerLib.UI.Windows;
using DaggerLib.DSGraphEdit.PinTypes;

using DirectShowLib;

namespace DaggerLib.DSGraphEdit
{
    public class DSFilterNode : DaggerNode
    {
        public IBaseFilter _filter;
        internal IMoniker _moniker;
        internal string _devicePath;
        private List<IPin> _pins;
        internal bool _manualAdded;

        public DSFilterNode(IBaseFilter filter,bool manualAdded)
        {
            _filter = filter;
            _manualAdded = manualAdded;

            AssociatedUINode = typeof(DSFilterNodeUI).AssemblyQualifiedName;

            // if it's a filesource filter, get the filename for it
            IFileSourceFilter fs = filter as IFileSourceFilter;
            if (fs != null)
            {
                IAMOpenProgress op = filter as IAMOpenProgress;
                if (op != null)
                {
                    // it wants a URL (thought you were being sneaky huh?)
                    string url = string.Empty;
                    AMMediaType mtype = new AMMediaType();
                    fs.GetCurFile(out url, mtype);
                    if (url == null)
                    {
                        URLDialog ud = new URLDialog();
                        if (ud.ShowDialog() == DialogResult.OK)
                        {
                            fs.Load(ud.URL, null);
                        }
                        ud.Dispose();
                        ud = null;
                    }
                    fs = null;
                }
                else
                {
                    // it wants a filename
                    string filename = string.Empty;
                    AMMediaType mtype = new AMMediaType();
                    fs.GetCurFile(out filename, mtype);
                    if (filename == null)
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            fs.Load(ofd.FileName, null);
                        }
                        ofd.Dispose();
                        ofd = null;
                    }
                    fs = null;
                }
            }

            // if it's a filewriter, get the filename for it
            IFileSinkFilter fw = filter as IFileSinkFilter;
            if (fw != null)
            {
                string filename = string.Empty;
                AMMediaType mtype = new AMMediaType();
                fw.GetCurFile(out filename, mtype);
                if (filename == null)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        fw.SetFileName(sfd.FileName, null);
                    }
                }
                fw = null;
            }

            // create and add all DaggerPins for this filter
            SyncPins(); 
        }

        /// <summary>
        /// Add or remove DaggerBasePins depending on the IPins in the filter
        /// </summary>
        public void SyncPins()
        {
            bool altered = false;

            // get the pins and convert to DaggerBasePins if they dont exisit yet
            _pins = GetPins();
            foreach (IPin pin in _pins)
            {
                PinDirection pd;
                pin.QueryDirection(out pd);
                if (pd == PinDirection.Input)
                {
                    if (GetDaggerPin(pin) == null)
                    {
                        DSInputPin inpin = new DSInputPin(pin);
                        InputPins.Add(inpin);
                        altered = true;
                    }
                }
                else
                {
                    if (GetDaggerPin(pin) == null)
                    {
                        DSOutputPin outpin = new DSOutputPin(pin);
                        OutputPins.Add(outpin);
                        altered = true;
                    }
                }
            }

            // remove any DaggerDSPins that may have vanished
            for(int i = InputPins.Count - 1;i > -1; i--)
            {
                if (!_pins.Contains((InputPins[i] as DSInputPin)._pin))
                {
                    // force the disconnect
                    InputPins[i].Disconnect(true);
                    Marshal.ReleaseComObject((InputPins[i] as DSInputPin)._pin);
                    (InputPins[i] as DSInputPin)._pin = null;
                    InputPins.Remove(InputPins[i]);
                    altered = true;
                }

                // check the major media format of the pin and set it's data type
                Type majorType = typeof(PinDataTypes.Unknown);
                bool whoops = false;
                try
                {
                    majorType = GetPinMajorMediaType((InputPins[i] as DSInputPin)._pin);
                }
                catch
                {
                    // the pin was removed by directshow, ignore changing the data type
                    whoops = true;
                }
                finally
                {
                    if (!whoops)
                    {
                        if (InputPins[i].DataType != majorType)
                        {
                            InputPins[i].DataType = majorType;
                            altered = true;
                        }
                    }
                }
            }

            for (int i = OutputPins.Count - 1; i > -1; i--)
            {
                if (!_pins.Contains((OutputPins[i] as DSOutputPin)._pin))
                {
                    // force the disconnect
                    OutputPins[i].Disconnect(true);
                    Marshal.ReleaseComObject((OutputPins[i] as DSOutputPin)._pin);
                    (OutputPins[i] as DSOutputPin)._pin = null;
                    OutputPins.Remove(OutputPins[i]);
                    altered = true;
                }

                // check the major media format of the pin and set it's data type
                Type majorType = typeof(PinDataTypes.Unknown);
                bool whoops = false;
                try
                {
                    majorType = GetPinMajorMediaType((OutputPins[i] as DSOutputPin)._pin);
                }
                catch
                {
                    // the pin was removed by directshow, ignore changing the data type
                    whoops = true;
                }
                finally
                {
                    if (!whoops)
                    {
                        if (OutputPins[i].DataType != majorType)
                        {
                            OutputPins[i].DataType = majorType;
                            altered = true;
                        }
                    }
                }
            }

            // if we altered anything, update the filter's ui elements and redraw the graph
            if (altered)
            {
                if (this.UINode != null)
                {
                    UINode.CalculateLayout();
                    if ((UINode as DaggerUINode).Parent != null)
                    {
                        // update the graph ui if the uinode is still part of a uigraph
                        (UINode as DaggerUINode).Parent.Invalidate();
                    }
                }
            }

            // tell the ui node to update any pin property pages
            if (UINode != null && UINode is DSFilterNodeUI)
            {
                (UINode as DSFilterNodeUI).SyncPinPropertyPages(null);
            }
        }

        /// <summary>
        /// Get the IPinIPType for the pin's prefered or connected media type
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        private Type GetPinMajorMediaType(IPin pin)
        {
            Type t = null;
            AMMediaType contype = new AMMediaType();
            int hr = pin.ConnectionMediaType(contype);
            if (hr == 0)
            {
                t = PinDataTypes.GetMajorPinType(contype.majorType);
                DsUtils.FreeAMMediaType(contype);
                return t;
            }
            else
            {
                // wasn't connected, enumerate the prefered media types and get the major type of the first one
                IEnumMediaTypes penum = null;
                hr = pin.EnumMediaTypes(out penum);
                if (hr == 0 && penum != null)
                {
                    AMMediaType[] mtypes = new AMMediaType[1];
                    IntPtr fetched = Marshal.AllocCoTaskMem(4);
                    try
                    {
                        if (penum.Next(1, mtypes, fetched) == 0)
                        {
                            t = PinDataTypes.GetMajorPinType(mtypes[0].majorType);
                            DsUtils.FreeAMMediaType(mtypes[0]);
                            Marshal.ReleaseComObject(penum);
                            return t;
                        }
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(fetched);
                    }
                }
            }

            // couldn't get the pin's major media type
            return typeof(PinDataTypes.Unknown);
        }

        /// <summary>
        /// Get the DaggerBasePin that holds the DS IPin
        /// </summary>
        /// <param name="dspin"></param>
        /// <returns></returns>
        public DaggerBasePin GetDaggerPin(IPin dspin)
        {
            foreach (DSInputPin pin in InputPins)
            {
                if (pin._pin == dspin)
                {
                    return pin;
                }
            }

            foreach (DSOutputPin pin in OutputPins)
            {
                if (pin._pin == dspin)
                {
                    return pin;
                }
            }

            return null;
        }

        /// <summary>
        /// Enumerate the ds IPins in the filter
        /// </summary>
        /// <returns></returns>
        public List<IPin> GetPins()
        {
            int hr = 0;

            IEnumPins enumPins;
            List<IPin> pinsArray = new List<IPin>();

            if (_filter == null)
            {
                return pinsArray;
            }

            hr = _filter.EnumPins(out enumPins);
            DsError.ThrowExceptionForHR(hr);

            IntPtr fetched = Marshal.AllocCoTaskMem(4);

            try
            {
                IPin[] pins = new IPin[1]; 
                while (enumPins.Next(pins.Length, pins, fetched) == 0)
                {
                    pinsArray.Add(pins[0]);

                    // GetPins can be called many times for the life of a node,
                    // but we only want to keep one COM reference to it.  If we've already enumerated this pin,
                    // remove the reference enumPins.Next just added to it.
                    bool found = false;
                    foreach (DSInputPin pin in InputPins)
                    {
                        if (pin._pin == pins[0])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        foreach (DSOutputPin pin in OutputPins)
                        {
                            if (pin._pin == pins[0])
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        // there's already a reference to this so decrement it
                        int refc = Marshal.ReleaseComObject(pins[0]);
                    }
                }
            }
            finally
            {
                try
                {
                    Marshal.ReleaseComObject(enumPins);
                }
                catch { } // this is due to enumpins being a managed object, not a com object
                Marshal.FreeCoTaskMem(fetched);
            }

            return pinsArray;
        }

        public override void OnAfterNodeRemoved()
        {
            base.OnAfterNodeRemoved();
            CloseInterfaces();            
        }

        /// <summary>
        /// Retrieve the name of the IBaseFilter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            FilterInfo fi;
            _filter.QueryFilterInfo(out fi);

            // we're only after the name.  QueryFilterInfo incs the ref counter for
            // the FilterGraph (not the IBaseFilter) so release it here
            int count = Marshal.ReleaseComObject(fi.pGraph);

            // if it's a path, just get the filename
            string shortPath = Path.GetFileName(fi.achName);

            if (shortPath != fi.achName)
            {
                return shortPath;
            }
         
            return fi.achName;
        }

        public void CloseInterfaces()
        {
            if (_filter != null)
            {
                // release our refs to the pins 
                foreach (DSInputPin pin in InputPins)
                {
                    if (pin._pin != null)
                    {
                        try
                        {
                            int refc = Marshal.ReleaseComObject(pin._pin);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            MessageBox.Show(ex.Message, "Failed to release pin");
#endif
                        }
                        finally
                        {
                            pin._pin = null;
                        }
                    }
                }
                foreach (DSOutputPin pin in OutputPins)
                {
                    if (pin._pin != null)
                    {
                        try
                        {
                            int refc = Marshal.ReleaseComObject(pin._pin);
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            MessageBox.Show(ex.Message, "Failed to release pin");
#endif
                        }
                        finally
                        {
                            pin._pin = null;
                        }
                    }
                }

                // SyncGraph keeps one reference to a filter regardless of where it came from
                try
                {
                    int frefc = Marshal.ReleaseComObject(_filter);
                }
                catch (Exception ex)
                {
#if DEBUG 
                    MessageBox.Show(ex.Message,"Failed to release IBaseFilter");
#endif
                }
                finally
                {
                    _filter = null;
                }
            }
        }
    }
}
