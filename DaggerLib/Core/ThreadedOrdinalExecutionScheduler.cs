using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DaggerLib.Interfaces;

namespace DaggerLib.Core
{
    /// <summary>
    /// Threaded Graph scheduler class
    /// </summary>
    public class ThreadedOrdinalExecutionScheduler : IGraphScheduler
    {
        private DaggerGraph _graph;
        private List<DaggerNode> ordinals;
        private List<Thread> _threads = new List<Thread>();
        private ManualResetEvent mre = new ManualResetEvent(false);
        private int _nodesCompleted = 0;

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

        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Process the entire graph and it's subgraphs
        /// </summary>
        public void ProcessGraph()
        {
            if (_graph != null)
            {
                //get the subgraphs
                for (int subg = 0; subg < _graph.SubGraphCount; subg++)
                {
                    // if this subgraph has only one top level node, process graph from there
                    if (_graph[subg, 0].Count == 1)
                    {
                        ProcessGraph(_graph[subg, 0][0]);
                    }
                    else
                    {
                        // merge the descendents of all the top level nodes and process ordinal slices seperately
                        List<DaggerNode> nodes = new List<DaggerNode>();
                        foreach (DaggerNode tlnode in _graph[subg, 0])
                        {
                            // add the top level node
                            nodes.Add(tlnode);

                            // add it's decendents if it's not already on the list
                            foreach (DaggerNode node in tlnode._descendents)
                            {
                                if (!nodes.Contains(node))
                                {
                                    nodes.Add(node);
                                }
                            }
                        }

                        // sort the list of merged decendents by the ordinals
                        nodes.Sort(new OrdinalComparer());

                        // process each node in the merged list
                        foreach (DaggerNode node in nodes)
                        {
                            node.DoProcessing();
                        }
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

            foreach (DaggerNode n in node._descendents)
            {
                n.DoProcessing();
            }
        }

        /*
        private void _process()
        {
            _nodesCompleted = 0;

            foreach (DaggerNode node in ordinals)
            {
                if (node.UINode != null)
                {
                    node.UINode.IsProcessing = true;
                }

                Thread t = new Thread(_processThread);
                t.IsBackground = true;
                t.Start(node);
            }

            WaitHandle.WaitAll(new WaitHandle[] { mre });
        }

        private void _processThread(object data)
        {
            DaggerNode node = data as DaggerNode;
            node.InvokeProcess();
            _processComplete(node);
        }

        private void _processComplete(DaggerNode node)
        {
            lock (this)
                _nodesCompleted++;

            if (node.UINode != null)
            {
                node.UINode.IsProcessing = false;
            }

            if (_nodesCompleted == ordinals.Count)
            {
                //signal that all nodes in the ordinal have been processed
                mre.Set();
            }
        }
        */
    }
}
