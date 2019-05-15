using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PMQuadtree
{
    public class PMQuadtree<T> where T : HasPosition
    {
        private Node rootNode;
        public int size { get; private set; }

        public PMQuadtree(float minX, float minY, float maxX, float maxY)
        {
            Region mainRegion = new Region(minX, minY, maxX - minX, maxY - minY);
            rootNode = new GreyNode(mainRegion, true);
            size = 0;
        }

        public bool Remove(T element)
        {
            if (rootNode.Contains(element))
            {
                rootNode.Remove(element);
                size--;
                return true;
            }
            return false;
        }

        public bool IsEmpty()
        {
            return rootNode.IsEmpty();
        }

        public bool Contains(T element)
        {
            return rootNode.Contains(element);
        }

        public bool Insert(T element)
        {
            rootNode.Insert(element, null);
            size++;
            return true;
        }

        public T GetNearestNeighbor(T element)
        {
            return rootNode.GetNearestNeighbor(element);
        }

        public T GetNearestNeighbor(float x, float y)
        {
            return rootNode.GetNearestNeighbor(x, y);
        }

        private interface Node
        {
            bool IsEmpty();

            // region is the position and size of the current node
            // they are passed from the grey nodes to the black nodes
            // they are ignored when passed to grey nodes which have their own boundaries
            Node Insert(T newElement, Region region);

            Node Remove(T oldElement);

            bool Contains(T newElement);

            T GetNearestNeighbor(T element);
            T GetNearestNeighbor(float x, float y);
        }

        private class WhiteNode : Node
        {
            private static WhiteNode singleton = new WhiteNode();

            private WhiteNode()
            {

            }

            public bool IsEmpty()
            {
                return true;
            }

            public static WhiteNode GetSingleton()
            {
                return singleton;
            }

            public Node Insert(T newElement, Region region)
            {
                return new BlackNode(newElement);
            }

            public Node Remove(T oldElement)
            {
                return singleton;
            }

            public T GetNearestNeighbor(T element)
            {
                return default(T);
            }

            public T GetNearestNeighbor(float x, float y)
            {
                return default(T);
            }

            public bool Contains(T newElement)
            {
                return false;
            }
        }

        private class BlackNode : Node
        {
            T data;

            public BlackNode(T newElement)
            {
                this.data = newElement;
            }

            public bool IsEmpty()
            {
                return false;
            }

            public Node Insert(T newElement, Region region)
            {
                GreyNode grey = new GreyNode(region);
                grey.Insert(data, null);
                grey.Insert(newElement, null);
                return grey;
            }

            public Node Remove(T oldElement)
            {
                if (this.data.Equals(oldElement))
                {
                    return WhiteNode.GetSingleton();
                }
                return this;
            }

            public T GetNearestNeighbor(T element)
            {
                return data;
            }

            public T GetNearestNeighbor(float x, float y)
            {
                return data;
            }

            public bool Contains(T newElement)
            {
                return data != null && data.Equals(newElement);
            }
        }

        private class GreyNode : Node
        {
            Node[] nodes = new Node[4];
            Region[] childRegions;
            Region myRegion;
            bool isRootNode;

            public GreyNode(Region region, bool rootNode = false)
            {
                if (region.width < 0 || region.height < 0)
                {
                    throw new System.Exception("Invalid boundaries in GreyNode");
                }
                Node whiteSingleton = WhiteNode.GetSingleton();
                nodes = new Node[4] { whiteSingleton, whiteSingleton, whiteSingleton, whiteSingleton };
                this.isRootNode = rootNode;
                myRegion = region;
                childRegions = region.generateSubregions();
            }

            public T GetNearestNeighbor(T element)
            {
                float[] distances = new float[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    distances[i] = childRegions[i].distanceTo(element);
                }

                bool childrenRemaining = true;
                T nearestNeighbor = default(T);
                float distanceToNearestNeighbor = float.MaxValue;
                while (childrenRemaining)
                {
                    int nextChild = -1;
                    float minDistance = float.MaxValue;
                    for (int i = 0; i < distances.Length; i++)
                    {
                        if (distances[i] < minDistance && distances[i] >= 0)
                        {
                            nextChild = i;
                            minDistance = distances[i];
                        }
                    }
                    if (nextChild < 0)
                    {
                        childrenRemaining = false;
                    }
                    else if (minDistance < distanceToNearestNeighbor)
                    {
                        T potentialNeighbor = nodes[nextChild].GetNearestNeighbor(element);
                        float potentialNeighborDistance = float.MaxValue;
                        if (potentialNeighbor != null)
                        {
                            potentialNeighborDistance = potentialNeighbor.DistanceTo(element);
                        }
                        if (potentialNeighborDistance < distanceToNearestNeighbor)
                        {
                            nearestNeighbor = potentialNeighbor;
                            distanceToNearestNeighbor = potentialNeighborDistance;
                        }
                    }

                    if (nextChild >= 0)
                    {
                        // We are done searching nextChild so ignore it for the remaining iterations.
                        distances[nextChild] = -99;
                    }
                }
                return nearestNeighbor;
            }

            public T GetNearestNeighbor(float x, float y)
            {
                float[] distances = new float[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    distances[i] = childRegions[i].distanceTo(x, y);
                }

                bool childrenRemaining = true;
                T nearestNeighbor = default(T);
                float distanceToNearestNeighbor = float.MaxValue;
                while (childrenRemaining)
                {
                    int nextChild = -1;
                    float minDistance = float.MaxValue;
                    for (int i = 0; i < distances.Length; i++)
                    {
                        if (distances[i] < minDistance && distances[i] >= 0)
                        {
                            nextChild = i;
                            minDistance = distances[i];
                        }
                    }
                    if (nextChild < 0)
                    {
                        childrenRemaining = false;
                    }
                    else if (minDistance < distanceToNearestNeighbor)
                    {
                        T potentialNeighbor = nodes[nextChild].GetNearestNeighbor(x, y);
                        float potentialNeighborDistance = float.MaxValue;
                        if (potentialNeighbor != null)
                        {
                            potentialNeighborDistance = potentialNeighbor.DistanceTo(x, y);
                        }
                        if (potentialNeighborDistance < distanceToNearestNeighbor)
                        {
                            nearestNeighbor = potentialNeighbor;
                            distanceToNearestNeighbor = potentialNeighborDistance;
                        }
                    }

                    if (nextChild >= 0)
                    {
                        // We are done searching nextChild so ignore it for the remaining iterations.
                        distances[nextChild] = -99;
                    }
                }
                return nearestNeighbor;
            }

            public Node Insert(T newElement, Region region)
            {
                float positionX = newElement.GetPositionX();
                float positionY = newElement.GetPositionY();
                if (!myRegion.contains(newElement))
                {
                    throw new System.Exception("Element at (" + positionX + ", " + positionY + ") does not belong in this Grey Node which has region " + myRegion.ToString());
                }
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (childRegions[i].contains(newElement))
                    {
                        nodes[i] = nodes[i].Insert(newElement, childRegions[i]);
                    }
                }

                return this;
            }

            public bool IsEmpty()
            {
                foreach (Node node in nodes)
                {
                    if (!node.IsEmpty())
                    {
                        return false;
                    }
                }
                return true;
            }

            public Node Remove(T oldElement)
            {
                float positionX = oldElement.GetPositionX();
                float positionY = oldElement.GetPositionY();
                if (!myRegion.contains(oldElement))
                {
                    if (isRootNode)
                    {
                        return this;
                    }
                    throw new System.Exception("Element does not belong in this Grey Node");
                }

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (childRegions[i].contains(oldElement))
                    {
                        nodes[i] = nodes[i].Remove(oldElement);
                    }
                }

                if (isRootNode)
                {
                    return this;
                }

                int countBlackNodes = 0;
                int countGreyNodes = 0;
                Node lastBlackNode = null;
                foreach (Node node in nodes)
                {
                    if (node is GreyNode)
                    {
                        countGreyNodes++;
                        return this;
                    }
                    else if (node is BlackNode)
                    {
                        countBlackNodes++;
                        lastBlackNode = node;
                    }
                }

                if (countBlackNodes == 0 && countGreyNodes == 0 && !isRootNode)
                {
                    return WhiteNode.GetSingleton();
                }
                else if (countBlackNodes == 1 && countGreyNodes == 0 && lastBlackNode != null && !isRootNode)
                {
                    return lastBlackNode;
                }

                return this;
            }



            public bool Contains(T newElement)
            {
                float positionX = newElement.GetPositionX();
                float positionY = newElement.GetPositionY();
                if (!myRegion.contains(newElement))
                {
                    if (isRootNode)
                    {
                        return false;
                    }
                    throw new System.Exception("Element does not belong in this Grey Node");
                }

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (childRegions[i].contains(newElement))
                    {
                        return nodes[i].Contains(newElement);
                    }
                }

                return false;
            }
        }
    }

    public interface HasPosition
    {
        float GetPositionX();

        float GetPositionY();

        float DistanceTo(HasPosition otherElement);
        float DistanceTo(float x, float y);

    }

    /// <summary>
    /// General Point object which may hold a single referece to additional data
    /// </summary>
    public class Point2D<T> : HasPosition
    {
        public readonly float x;
        public readonly float y;
        public T data;

        public Point2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Point2D(float x, float y, T data)
        {
            this.x = x;
            this.y = y;
            this.data = data;
        }

        float HasPosition.GetPositionX()
        {
            return x;
        }

        float HasPosition.GetPositionY()
        {
            return y;
        }

        float HasPosition.DistanceTo(HasPosition otherElement)
        {
            float otherPositionX = otherElement.GetPositionX();
            float otherPositionY = otherElement.GetPositionY();
            return Mathf.Sqrt(Mathf.Pow(x - otherPositionX, 2f) + Mathf.Pow(y - otherPositionY, 2f));
        }

        float HasPosition.DistanceTo(float x, float y)
        {
            return Mathf.Sqrt(Mathf.Pow(this.x - x, 2f) + Mathf.Pow(this.y - y, 2f));
        }
    }

    /// <summary>
    /// An immutable class representing an axis-aligned rectangle.
    /// </summary>
    class Region
    {
        public readonly float x;
        public readonly float y;
        public readonly float width;
        public readonly float height;

        public Region(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Checks to see if the element is in this region.
        /// The region is closed on the minimum x and y edges but open on the max x and y edges.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool contains(HasPosition element)
        {
            float positionX = element.GetPositionX();
            float positionY = element.GetPositionY();
            return x <= positionX && positionX < x + width && y <= positionY && positionY < y + height;
        }

        /// <summary>
        /// Returns the minimum distance from the element to this region.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public float distanceTo(HasPosition element)
        {
            return distanceTo(element.GetPositionX(), element.GetPositionY());
        }

        public float distanceTo(float x, float y)
        {
            float clampedX = Mathf.Clamp(x, this.x, this.x + width);
            float clampedY = Mathf.Clamp(y, this.y, this.y + height);
            float distanceToClampedX = x - clampedX;
            float distanceToClampedY = y - clampedY;
            return Mathf.Sqrt(Mathf.Pow(distanceToClampedX, 2f) + Mathf.Pow(distanceToClampedY, 2f));
        }

        /// <summary>
        /// Returns an array with 4 equal sized subregions.
        /// ---------
        /// | 0 | 1 |
        /// ---------
        /// | 2 | 3 |
        /// ---------
        /// </summary>
        /// <returns></returns>
        public Region[] generateSubregions()
        {
            // Assume x,y is the top left corner
            Region topLeft = new Region(x, y, width / 2, height / 2);
            Region topRight = new Region(x + width / 2, y, width / 2, height / 2);
            Region bottomLeft = new Region(x, y + height / 2, width / 2, height / 2);
            Region bottomRight = new Region(x + width / 2, y + height / 2, width / 2, height / 2);
            return new Region[] { topLeft, topRight, bottomLeft, bottomRight };
        }

        override public string ToString()
        {
            return "Region: (" + x + ", " + y + ") to (" + (x + width) + ", " + (y + height) + ")";
        }
    }
}
