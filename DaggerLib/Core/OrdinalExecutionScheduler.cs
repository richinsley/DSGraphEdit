using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    /// <summary>
    /// Default non-threaded Graph scheduler class
    /// </summary>
    public class OrdinalExecutionScheduler : IGraphScheduler
    {
        private DaggerGraph _graph;
        
        // list of subgraphs in ordinal sorted order
        private List<List<DaggerNode>> _subgraphs = new List<List<DaggerNode>>();

        public DaggerGraph Graph
        {
            get
            {
                return _graph;
            }
            set
            {
                CancelProcessing();
                _graph = value;
            }
        }

        public void CancelProcessing()
        {

        }

        public void OnTopologyChanged()
        {
            // recreate list of subgraphs in ordinal sorted order
            _subgraphs.Clear();
            for (int i = 0; i < _graph.SubGraphCount; i++)
            {
                _subgraphs.Add(_graph[i]);
            }
        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Process the entire graph and it's subgraphs
        /// </summary>
        public void ProcessGraph()
        {
            // execute the list of subgraphs
            foreach (List<DaggerNode> subgraph in _subgraphs)
            {
                foreach (DaggerNode node in subgraph)
                {
                    node.DoProcessing();

                    // if the node has a UINode attached, call it's DoUIProcessing method
                    if (node.UINode != null)
                    {
                        node.UINode.DoUIProcessing();
                    }
                }
            }
        }

        /// <summary>
        /// Process the graph from a given node
        /// </summary>
        /// <param name="node"></param>
        public void ProcessGraph(DaggerNode node)
        {
            node.DoProcessing();

            // if the node has a UINode attached, call it's DoUIProcessing method
            if (node.UINode != null)
            {
                node.UINode.DoUIProcessing();
            }

            foreach (DaggerNode n in node._descendents)
            {
                n.DoProcessing();

                // if the node has a UINode attached, call it's DoUIProcessing method
                if (n.UINode != null)
                {
                    n.UINode.DoUIProcessing();
                }
            }
        }
    }
}
