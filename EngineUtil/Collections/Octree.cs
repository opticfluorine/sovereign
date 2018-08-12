using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Engine8.EngineUtil.Collections
{

    /// <summary>
    /// Octree implementation for managing mutable sets of
    /// distinct objects in three-dimensional space.
    /// </summary>
    public class Octree<T>
    {

        /// <summary>
        /// Creates an empty octree.
        /// </summary>
        public Octree()
        {

        }

        /// <summary>
        /// Creates an octree containing the given objects.
        /// </summary>
        /// <param name="initialData"></param>
        public Octree(IDictionary<T, Vector<float>> initialData)
        {
            /* Add all initial points to the octree. */
            foreach (var element in initialData.Keys)
            {
                var position = initialData[element];
                Add(position, element);
            }
        }
        
        /// <summary>
        /// Adds the given element to the octree at the given position.
        /// </summary>
        /// 
        /// If the element is already in the octree, this method has no effect.
        /// 
        /// <param name="position">Initial position of the element.</param>
        /// <param name="element">Element to be added.</param>
        public void Add(Vector<float> position, T element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the position of the given element.
        /// </summary>
        /// 
        /// <param name="element">Element to be updated.</param>
        /// <param name="position">New position of the element.</param>
        ///
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the element is not in the octree.
        /// </exception>
        public void UpdatePosition(T element, Vector<float> position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the given element from the octree.
        /// </summary>
        /// 
        /// <param name="element">Element to be removed.</param>
        /// 
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the element is not in the octree.
        /// </exception>
        public void Remove(T element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the elements in the given range.
        /// </summary>
        /// <param name="minPosition">Minimum position (inclusive).</param>
        /// <param name="maxPosition">Maximum position (exclusive).</param>
        /// <param name="buffer">Buffer to hold the results.</param>
        public void GetElementsInRange(Vector<float> minPosition,
            Vector<float> maxPosition, IList<Tuple<Vector<float>, T>> buffer)
        {
            throw new NotImplementedException();
        }

    }

}
