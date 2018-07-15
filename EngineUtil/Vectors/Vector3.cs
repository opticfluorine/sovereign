using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.EngineUtil.Vectors
{

    /// <summary>
    /// Generic three-dimensional vector.
    /// </summary>
    /// <typeparam name="T">Vector element type.</typeparam>
    public struct Vector3<T>
    {

        /// <summary>
        /// X coordinate.
        /// </summary>
        public T x;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public T y;

        /// <summary>
        /// Z coordinate.
        /// </summary>
        public T z;

    }
}
