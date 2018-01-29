using System;
using System.Windows.Forms;

namespace DaggerLib.DSGraphEdit
{
    public interface IDSFilterCollection
    {
        string CategoryName
        {
            get;
        }

        void SyncTreeNodes(TreeNodeCollection collection);
    }
}
