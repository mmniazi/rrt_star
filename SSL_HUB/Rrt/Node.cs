using System.Collections.Generic;

namespace SSL_HUB.Rrt
{
    public class Node
    {
        public Node(int x, int y)
        {
            X = x;
            Y = y;
            DistanceFromGoal = -1;
            DistanceFromRoot = -1;
            Parent = null;
            Children = new List<Node>();
        }

        public Node(int x, int y, double distanceFromRoot, double distanceFromGoal, Node parent)
        {
            X = x;
            Y = y;
            DistanceFromGoal = distanceFromGoal;
            DistanceFromRoot = distanceFromRoot;
            Parent = parent;
            Children = new List<Node>();
        }

        public List<Node> Children { get; private set; }
        public double DistanceFromGoal { get; set; }
        public double DistanceFromRoot { get; set; }
        public Node Parent { get; set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public void SetCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void AddChild(Node child)
        {
            Children.Add(child);
        }

        public void RemoveChild(Node child)
        {
            Children.Remove(child);
        }
    }
}