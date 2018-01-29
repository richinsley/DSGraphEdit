using System;
using System.Collections.Generic;
using System.Text;

namespace DaggerLib.UI.Windows
{
    public enum AutoArrangeStyle
    {
        None,
        Ordinal,
        All
    }

    public enum DaggerNodePinPlacement
    {
        Indent,
        Inset,
        Outset
    }

    public enum DaggerNodeAlterState
    {
        None,
        Context,
        Move,
        ConnectFromOutput,
        ConnectFromInput,
        CanConnectToOutput,
        CanConnectToInput,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }
}
