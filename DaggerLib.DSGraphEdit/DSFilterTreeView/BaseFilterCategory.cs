using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

using DirectShowLib;
using DirectShowLib.DMO;

namespace DaggerLib.DSGraphEdit
{
    /// <summary>
    /// Abstract class that uses GetDevicesOfCat to build a collection of Filters by category
    /// </summary>
    public abstract class BaseFilterCategory : IDSFilterCollection
    {
        /// <summary>
        /// Get the name of the category
        /// </summary>
        public abstract string CategoryName
        {
            get;
        }

        /// <summary>
        /// Get the DMOCategory that DMO filters in the category belong to
        /// </summary>
        public abstract Guid DMOCategory
        {
            get;
        }

        /// <summary>
        /// Get the Guid for this Category
        /// </summary>
        protected abstract Guid Category
        {
            get;
        }

        /// <summary>
        /// Adds or removes DSFilterTreeViewNodes to/from a TreeNodeCollection based on Filters
        /// that are available on the system.
        /// </summary>
        /// <param name="collection"></param>
        public virtual void SyncTreeNodes(TreeNodeCollection collection)
        {
            // get a list of all standard Direct Show Filters
            List<DsDevice> devices = new List<DsDevice>(DirectShowLib.DsDevice.GetDevicesOfCat(Category));

            // GC will release the monikers when the devices list goes out of scope

            // go in reverse order and remove any nodes that no longer have registered filters
            for (int i = collection.Count - 1; i > -1; i--)
            {
                DSFilterTreeViewNode tn = collection[i] as DSFilterTreeViewNode;
                if (tn != null)
                {
                    if(GetDeviceFromMoniker(devices,tn.Moniker) == null)
                    {
                        collection.Remove(tn);
                    }
                }
            }

            // add new TreeNodes for filters that are not yet in the collection 
            foreach (DsDevice device in devices)
            {
                if (DSFilterTreeView.GetTreeNodeByDevicePath(device.DevicePath, collection) == null)
                {
                    // we don't have this one yet
                    try
                    {
                        DSFilterTreeViewNode tn = new DSFilterTreeViewNode(device, this.DMOCategory);
                        if (tn.Text != "")
                        {
                            collection.Add(tn);
                        }
                    }
                    catch
                    {
                        // some filters just don't play nice
                    }
                }
            }
        }

        /// <summary>
        /// Search for a Moniker in a list of DsDevices
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="mon"></param>
        /// <returns></returns>
        internal DsDevice GetDeviceFromMoniker(List<DsDevice> devices, IMoniker mon)
        {
            DsDevice dev = null;
            foreach (DsDevice dd in devices)
            {
                if (dd.Mon == mon)
                {
                    dev = dd;
                    break;
                }
            }
            return dev;
        }
    }
}
