# unity-data-structures
A collection of data structures for use in Unity.  
Written in C#.

## PM Quadtree
-------------------------------------

### Usage
* Copy `PMQuadtree.cs` into your project.
* Use the `PMQuadtree` namespace in the scripts where you need it.
* Create a class implementing `PMQuadtree.HasPosition` or use the included `Point2D<T>` class for a simple map.
* Create a new PM Quadtree by calling `var tree = new PMQuadtree<Point2D<object>>(minX, minY, maxX, maxY)`
passing in your boundaries.
* Insert elements with `tree.Insert(new Point<object>(1, 1, null))`.

### API
* `bool Insert(T element)`
 * Currently always returns true.
* `bool Contains(T element)`
 * Returns true if the element is in the tree.
* `bool Remove(T element)`  
 * Removes the element if it is in the tree.
 * Does not throw an exception if the element is not in the tree.
* `bool IsEmpty()`
* `int size`
