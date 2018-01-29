using System;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    public enum PassPinDataAsClone
    {
        Never,
        Always,
        MultiConnect
    }

    [Flags]
    public enum PinMutexGroups
    {
        None = 0,
        Group1 = 1,
        Group2 = 2,
        Group3 = 4,
        Group4 = 8,
        Group5 = 16,
        Group6 = 32,
        Group7 = 64,
        Group8 = 128,
        Group9 = 256,
        Group10 = 512,
        All = 1023
    }

    // delegate for the OnPinDataSet event
    public delegate void DaggerPinDataSetHandler(DaggerBasePin sender,object data);

    //Delegates to Handle Pin Collection Add/Remove
    public delegate void DaggerPinAdded(object sender, DaggerBasePin pin);
    public delegate void DaggerPinRemoved(object sender, DaggerBasePin pin);

    // Delegate used as callback to signal completion of Graph Processing
    public delegate void DaggerGraphProcessingCompleteCallback(DaggerGraph graph);

    /// <summary>
    /// Handler for when a pin's name changes
    /// </summary>
    /// <param name="pin"></param>
    public delegate void DaggerPinNameChanged(DaggerBasePin pin);

    /// <summary>
    /// Handler for when a pin's Data Type has changed
    /// </summary>
    /// <param name="pin"></param>
    public delegate void DaggerPinDataTypeChanged(DaggerBasePin pin,Type type);

    /// <summary>
    /// Handler before a node is added/removed from a container panel
    /// </summary>
    /// <param name="node"></param>
    /// <param name="panel"></param>
    /// <returns>true if node can be removed</returns>
    public delegate bool BeforeNodeRemoveHandler(DaggerNode node);

    /// <summary>
    /// Handler called before a selection of Noodles and Nodes are deleted
    /// </summary>
    /// <param name="sender">UIGraph</param>
    /// <returns>true if selection can be deleted</returns>
    public delegate bool BeforeDeleteSelected(object sender);

    /// <summary>
    /// Handler called after a selection of Noodles and Nodes have been deleted
    /// </summary>
    /// <param name="sender"></param>
    public delegate void AfterDeleteSelected(object sender);

    /// <summary>
    /// Handler after a node is added/removed from a container panel
    /// </summary>
    /// <param name="node"></param>
    /// <param name="panel"></param>
    public delegate void AfterNodeRemoveHandler(DaggerNode node);

    /// <summary>
    /// Called before 2 pins are connected
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <returns>true if the pins can be connected</returns>
    public delegate bool PinBeforeConnectedHandler(DaggerOutputPin output, DaggerInputPin input);

    /// <summary>
    /// Called before 2 pins are disconnected
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <returns>returns true if pins can be disconnected</returns>
    public delegate bool PinBeforeDisconnectedHandler(DaggerOutputPin output, DaggerInputPin input);

    /// <summary>
    /// Called After 2 pins have been connected
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    public delegate void PinAfterConnectedHandler(DaggerOutputPin output, DaggerInputPin input);

    /// <summary>
    /// Called After 2 pins have been disconnected
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    public delegate void PinAfterDisconnectedHandler(DaggerOutputPin output, DaggerInputPin input);

    /// <summary>
    /// Called when a Dagger node is attached to a DaggerUINode
    /// </summary>
    /// <param name="node"></param>
    public delegate void DaggerNodeAttachedHandler(DaggerNode node);

    /// <summary>
    /// Called when a Dagger node is attached to a DaggerUINode
    /// </summary>
    /// <param name="node"></param>
    public delegate void DaggerUINodeAttachedHandler(IDaggerUINode node);

    /// <summary>
    /// Called before a base pin shows it's context menu
    /// </summary>
    /// <param name="pin"></param>
    public delegate void DaggerBasePinBeforeShowContextMenuHandler(DaggerBasePin pin);
}
