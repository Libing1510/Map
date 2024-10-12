using System;
using System.Collections.Generic;

namespace YJ.Map.Dijkstra
{
    internal class NodeItem
    {
        public bool used { get; set; }
        public List<Node> roadNodes { get; set; }
        public float weight { get; set; }
        public int index { get; private set; }
        public Node node { get; private set; }

        public NodeItem(int index, Node node)
        {
            used = false;
            roadNodes = new List<Node>();
            weight = float.MaxValue;
            this.index = index;
            this.node = node;
        }
    }
}