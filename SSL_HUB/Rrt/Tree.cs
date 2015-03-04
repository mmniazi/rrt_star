using System;
using System.Collections.Generic;

namespace SSL_HUB.Rrt
{
    internal class Tree
    {
        public List<Node> NodeList { get; private set; }
        public Node Root { get; private set; }

        public Tree(int x, int y)
        {
            Root = new Node(x, y, 0, Double.MaxValue, null);
            NodeList = new List<Node> {Root};
        }

        public void Add(Node parent, Node child)
        {
            parent.AddChild(child);
            NodeList.Add(child);
            child.Parent = parent;
        }

        public void Remove(Node node)
        {
            node.Parent.RemoveChild(node);
            NodeList.Remove(node);
        }

        public bool Contains(Node nodeReq)
        {
            return NodeList.Contains(nodeReq);
        }

        public Node NearestNode(int x, int y)
        {
            var minDistance = Distance(Root.X, Root.Y, x, y);
            var nearestNode = Root;
            foreach (var node in NodeList)
            {
                var currentDistance = Distance(node.X, node.Y, x, y);
                if (!(currentDistance < minDistance)) continue;
                minDistance = currentDistance;
                nearestNode = node;
            }
            return nearestNode;
        }

        public double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));
        }
    }
}