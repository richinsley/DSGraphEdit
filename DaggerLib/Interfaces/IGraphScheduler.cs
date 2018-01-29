using System;
using System.Collections.Generic;
using System.Text;
using DaggerLib.Core;

namespace DaggerLib.Interfaces
{
    public interface IGraphScheduler : IDisposable
    {
        /// <summary>
        /// Gets or sets the graph this Scheduler will operate on
        /// </summary>
        DaggerGraph Graph
        {
            set;
            get;
        }

        /// <summary>
        /// Cancels the current processing of the graph
        /// </summary>
        void CancelProcessing();

        /// <summary>
        /// Called when the topology of the graph has changed
        /// </summary>
        void OnTopologyChanged();

        /// <summary>
        /// Process the entire graph and it's subgraphs
        /// </summary>
        void ProcessGraph();

        /// <summary>
        /// Process the graph from a given node
        /// </summary>
        /// <param name="node"></param>
        void ProcessGraph(DaggerNode node);
    }
}
