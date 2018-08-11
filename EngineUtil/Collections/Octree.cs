using Engine8.EngineUtil.Vectors;
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

        }

    }

}
