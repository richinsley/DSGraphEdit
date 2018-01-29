using System;
using System.Collections.Generic;
using System.Text;

namespace DaggerLib.Interfaces
{
    public interface ISelector
    {
        List<IDaggerUINode> SelectedNodes
        {
            get;
        }

        //the noodles we have selected
        List<IDaggerNoodle> SelectedNoodles
        {
            get;
        }
    }
}
