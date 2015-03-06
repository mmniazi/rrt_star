using System;
using System.Collections.Generic;
using System.Linq;

namespace SSL_HUB.Rrt
{
    public class Rrt
    {
        private readonly int _edgeLength;
        private readonly int _fieldHeight;
        private readonly int _fieldWidth;
        private readonly bool[][] _goalPoints;
        private readonly int _iterations;
        private readonly bool[][] _obstaclePoints;
        private readonly int _radius;
        private readonly int _tryGoalFactor;
        private List<Node> _currentPath;
        private Node _goalNode;
        private List<Node> _nearestNodes;
        private int _neighbourRadius;
        private Tree _tree;
        // TODO: breaking on moving out of boundary
        // TODO: Need further optimization to make it 90 ms.
        public Rrt(int radius, int fieldWidth, int fieldHeight)
        {
            _fieldWidth = fieldWidth;
            _fieldHeight = fieldHeight;
            _radius = radius;
            _goalPoints = new bool[fieldWidth + 1][];
            _obstaclePoints = new bool[fieldWidth + 1][];
            _tryGoalFactor = 5;
            _iterations = 1;
            _neighbourRadius = 1000;
            _nearestNodes = new List<Node>();
            _edgeLength = 50;
            _currentPath = new List<Node>();
        }

        public void SetConditions(Node startNode, Node goalNode, IEnumerable<Node> obstacles)
        {
            _tree = new Tree(startNode.X, startNode.Y);
            _goalNode = goalNode;
            for (var i = 0; i <= _fieldWidth; i++)
            {
                _goalPoints[i] = new bool[_fieldHeight + 1];
                _obstaclePoints[i] = new bool[_fieldHeight + 1];
            }

            foreach (var obstacle in obstacles)
            {
                for (var x = obstacle.X - _radius; x < obstacle.X + _radius; x++)
                {
                    for (var y = obstacle.Y - _radius; y < obstacle.Y + _radius; y++)
                    {
                        if ((x - obstacle.X)*(x - obstacle.X) + (y - obstacle.Y)*(y - obstacle.Y) <= _radius*_radius &&
                            x >= 0 && y >= 0 && x <= _fieldWidth && y <= _fieldHeight)
                        {
                            _obstaclePoints[x][y] = true;
                        }
                    }
                }
            }

            for (var x = goalNode.X - _radius; x <= goalNode.X + _radius; x++)
            {
                for (var y = goalNode.Y - _radius; y <= goalNode.Y + _radius; y++)
                {
                    if ((x - goalNode.X)*(x - goalNode.X) + (y - goalNode.Y)*(y - goalNode.Y) <= _radius*_radius &&
                        x >= 0 && y >= 0 && x <= _fieldWidth && y <= _fieldHeight)
                    {
                        _goalPoints[x][y] = true;
                    }
                }
            }
        }

        public List<Node> GetPath()
        {
            if (_currentPath.Count == 0 || _currentPath.Last().X != _goalNode.X || _currentPath.Last().Y != _goalNode.Y || !IsValidPath())
            {
                CalcPath();
                return _currentPath;
            }
            return null;
        }

        private bool IsValidPath()
        {
            for (var i = 0; i < _currentPath.Count - 1; i++)
            {
                if (!IsValidNode(_currentPath.ElementAt(i), _currentPath.ElementAt(i + 1)))
                {
                    return false;
                }
            }
            return true;
        }

        public void CalcPath()
        {
            var goalReached = false;
            for (var i = 0; i < _iterations || !goalReached; i++)
            {
                var randomNode = i%_tryGoalFactor == 0
                    ? _goalNode
                    : SelectRandomNode();
                _nearestNodes = GetNearestNodes(randomNode);

                if (_nearestNodes.Count > 200)
                {
                    _neighbourRadius = (int) DistanceBetween(randomNode, _nearestNodes.ElementAt(200));
                }
                else if (_nearestNodes.Count == 0)
                {
                    _neighbourRadius = 10000;
                }

                foreach (var node in _nearestNodes.Where(node => IsValidNode(randomNode, node)))
                {
                    var nearestNode = node;
                    var intermediateNode = randomNode;
                    while (DistanceBetween(nearestNode, randomNode) > 0)
                    {
                        intermediateNode = MoveTowardRandNode(nearestNode, randomNode, _edgeLength);
                        _tree.Add(nearestNode, intermediateNode);
                        nearestNode = intermediateNode;
                    }
                    if (_goalPoints[randomNode.X][randomNode.Y] && !goalReached)
                    {
                        _tree.Add(intermediateNode, _goalNode);
                        goalReached = true;
                    }
                    foreach (var thisNode in _nearestNodes)
                    {
                        var nearestNodePathLength = nearestNode.DistanceFromRoot;
                        var thisNodepathLength = thisNode.DistanceFromRoot;
                        var distance = DistanceBetween(nearestNode, thisNode);

                        if (nearestNodePathLength + distance < thisNodepathLength && IsValidNode(nearestNode, thisNode))
                        {
                            thisNode.Parent.RemoveChild(thisNode);
                            nearestNode.AddChild(thisNode);
                            thisNode.Parent = nearestNode;
                            thisNode.DistanceFromRoot = nearestNodePathLength + distance;
                        }
                    }
                    break;
                }
            }

            _currentPath = SmoothPath(FindPath(_goalNode));
        }

        private List<Node> SmoothPath(IReadOnlyCollection<Node> path)
        {
            var currentNode = path.Last();
            var smoothPath = new List<Node> {currentNode};
            for (var i = path.Count - 2; i >= 0; i--)
            {
                if (!IsValidNode(path.ElementAt(i), currentNode))
                {
                    smoothPath.Add(path.ElementAt(i + 1));
                    currentNode = path.ElementAt(i + 1);
                }
            }
            smoothPath.Add(path.First());
            return smoothPath;
        }

        private static Node MoveTowardRandNode(Node nearestNode, Node randomNode, int edgeLength)
        {
            var x1 = nearestNode.X;
            var y1 = nearestNode.Y;
            var x2 = randomNode.X;
            var y2 = randomNode.Y;

            var newNode = new Node(x2, y2);

            var d = DistanceBetween(nearestNode, randomNode);

            if (d <= edgeLength)
            {
                newNode.DistanceFromRoot = nearestNode.DistanceFromRoot + d;
                return newNode;
            }

            newNode.DistanceFromRoot = nearestNode.DistanceFromRoot + edgeLength;

            double distFromLine;
            double distFromNearestNode;
            int num;
            double den;

            if (x1 > x2 && y1 > y2)
            {
                for (var x = x2; x <= x1; x++)
                {
                    for (var y = y2; y <= y1; y++)
                    {
                        num = Math.Abs((x - x1)*(y1 - y2) - (y - y1)*(x1 - x2));
                        den = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

                        distFromLine = num/den;
                        distFromNearestNode = Math.Sqrt((x - x1)*(x - x1) + (y - y1)*(y - y1));

                        if (distFromLine <= 2 && distFromNearestNode <= edgeLength &&
                            distFromNearestNode > edgeLength - 2)
                        {
                            newNode.SetCoordinate(x, y);
                            return newNode;
                        }
                    }
                }
            }
            else if (x1 > x2 && y1 < y2)
            {
                for (var x = x2; x <= x1; x++)
                {
                    for (var y = y2; y >= y1; y--)
                    {
                        num = Math.Abs((x - x1)*(y1 - y2) - (y - y1)*(x1 - x2));
                        den = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

                        distFromLine = num/den;
                        distFromNearestNode = Math.Sqrt((x - x1)*(x - x1) + (y - y1)*(y - y1));

                        if (distFromLine <= 2 && distFromNearestNode <= edgeLength &&
                            distFromNearestNode > edgeLength - 2)
                        {
                            newNode.SetCoordinate(x, y);
                            return newNode;
                        }
                    }
                }
            }
            else if (x1 < x2 && y1 > y2)
            {
                for (var x = x2; x >= x1; x--)
                {
                    for (var y = y2; y <= y1; y++)
                    {
                        num = Math.Abs((x - x1)*(y1 - y2) - (y - y1)*(x1 - x2));
                        den = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

                        distFromLine = num/den;
                        distFromNearestNode = Math.Sqrt((x - x1)*(x - x1) + (y - y1)*(y - y1));

                        if (distFromLine <= 2 && distFromNearestNode <= edgeLength &&
                            distFromNearestNode > edgeLength - 2)
                        {
                            newNode.SetCoordinate(x, y);
                            return newNode;
                        }
                    }
                }
            }
            else if (x1 < x2 && y1 < y2)
            {
                for (var x = x2; x >= x1; x--)
                {
                    for (var y = y2; y >= y1; y--)
                    {
                        num = Math.Abs((x - x1)*(y1 - y2) - (y - y1)*(x1 - x2));
                        den = Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));

                        distFromLine = num/den;
                        distFromNearestNode = Math.Sqrt((x - x1)*(x - x1) + (y - y1)*(y - y1));

                        if (distFromLine <= 2 && distFromNearestNode <= edgeLength &&
                            distFromNearestNode > edgeLength - 2)
                        {
                            newNode.SetCoordinate(x, y);
                            return newNode;
                        }
                    }
                }
            }
            else if (x1 == x2 && y1 > y2)
            {
                for (var y = y2; y <= y1; y++)
                {
                    distFromNearestNode = Math.Sqrt((y - y1)*(y - y1));

                    if (distFromNearestNode <= edgeLength && distFromNearestNode > edgeLength - 2)
                    {
                        newNode.SetCoordinate(x1, y);
                        return newNode;
                    }
                }
            }
            else if (x1 == x2 && y1 < y2)
            {
                for (var y = y2; y >= y1; y--)
                {
                    distFromNearestNode = Math.Sqrt((y - y1)*(y - y1));

                    if (distFromNearestNode <= edgeLength && distFromNearestNode > edgeLength - 2)
                    {
                        newNode.SetCoordinate(x1, y);
                        return newNode;
                    }
                }
            }
            else if (x1 > x2 && y1 == y2)
            {
                for (var x = x2; x <= x1; x++)
                {
                    distFromNearestNode = Math.Sqrt((x - x1)*(x - x1));

                    if (distFromNearestNode <= edgeLength && distFromNearestNode > edgeLength - 2)
                    {
                        newNode.SetCoordinate(x, y1);
                        return newNode;
                    }
                }
            }
            else if (x1 < x2 && y1 == y2)
            {
                for (var x = x2; x >= x1; x--)
                {
                    distFromNearestNode = Math.Sqrt((x - x1)*(x - x1));

                    if (distFromNearestNode <= edgeLength && distFromNearestNode > edgeLength - 2)
                    {
                        newNode.SetCoordinate(x, y1);
                        return newNode;
                    }
                }
            }
            return null;
        }

        private bool IsValidNode(Node randomNode, Node nearestNode)
        {
            var x1 = nearestNode.X;
            var y1 = nearestNode.Y;
            var x2 = randomNode.X;
            var y2 = randomNode.Y;

            if (x1 == x2 && y1 == y2)
            {
                return false;
            }
            if (x1 > x2 && y1 > y2)
            {
                for (var x = x2; x <= x1; x++)
                {
                    for (var y = y2; y <= y1; y++)
                    {
                        if (((x - x1)/(x1 - x2)) == ((y - y1)/(y1 - y2)) && _obstaclePoints[x][y])
                        {
                            return false;
                        }
                    }
                }
            }
            else if (x1 > x2 && y1 < y2)
            {
                for (var x = x2; x <= x1; x++)
                {
                    for (var y = y2; y >= y1; y--)
                    {
                        if (((x - x1)/(x1 - x2)) == ((y - y1)/(y1 - y2)) && _obstaclePoints[x][y])
                        {
                            return false;
                        }
                    }
                }
            }
            else if (x1 < x2 && y1 > y2)
            {
                for (var x = x2; x >= x1; x--)
                {
                    for (var y = y2; y <= y1; y++)
                    {
                        if (((x - x1)/(x1 - x2)) == ((y - y1)/(y1 - y2)) && _obstaclePoints[x][y])
                        {
                            return false;
                        }
                    }
                }
            }
            else if (x1 < x2 && y1 < y2)
            {
                for (var x = x2; x >= x1; x--)
                {
                    for (var y = y2; y >= y1; y--)
                    {
                        if (((x - x1)/(x1 - x2)) == ((y - y1)/(y1 - y2)) && _obstaclePoints[x][y])
                        {
                            return false;
                        }
                    }
                }
            }
            else if (x1 == x2 && y1 > y2)
            {
                for (var y = y2; y <= y1; y++)
                {
                    if (_obstaclePoints[x1][y])
                    {
                        return false;
                    }
                }
            }
            else if (x1 == x2 && y1 < y2)
            {
                for (var y = y2; y >= y1; y--)
                {
                    if (_obstaclePoints[x1][y])
                    {
                        return false;
                    }
                }
            }
            else if (x1 > x2 && y1 == y2)
            {
                for (var x = x2; x <= x1; x++)
                {
                    if (_obstaclePoints[x][y1])
                    {
                        return false;
                    }
                }
            }
            else if (x1 < x2 && y1 == y2)
            {
                for (var x = x2; x >= x1; x--)
                {
                    if (_obstaclePoints[x][y1])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Node SelectRandomNode()
        {
            var random = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
            var x = (int) (random.NextDouble()*_fieldWidth);
            var y = (int) (random.NextDouble()*_fieldHeight);

            while (_obstaclePoints[x][y])
            {
                x = (int) (random.NextDouble()*_fieldWidth);
                y = (int) (random.NextDouble()*_fieldHeight);
            }

            return new Node(x, y);
        }

        public List<Node> GetNearestNodes(Node node)
        {
            var nearestNodes = new Dictionary<Node, Double>();

            foreach (var treeNode in _tree.NodeList)
            {
                var distance = DistanceBetween(treeNode, node);
                var pathLength = treeNode.DistanceFromRoot;
                if (distance < _neighbourRadius)
                    nearestNodes.Add(treeNode, pathLength + distance);
            }
            var sortedList = new List<Node>();
            nearestNodes.OrderBy(pair => pair.Value).ToList().ForEach(sortedPair => sortedList.Add(sortedPair.Key));
            return sortedList;
        }

        public static double DistanceBetween(Node n1, Node n2)
        {
            var x1 = n1.X;
            var y1 = n1.Y;
            var x2 = n2.X;
            var y2 = n2.Y;
            return Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));
        }

        public List<Node> FindPath(Node end)
        {
            var path = new List<Node> {end};
            var node = end;
            while (true)
            {
                node = node.Parent;
                path.Add(node);
                if (node.Parent == _tree.Root)
                {
                    return path;
                }
            }
        }
    }
}