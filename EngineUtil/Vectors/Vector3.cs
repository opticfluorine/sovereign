using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.EngineUtil.Vectors
{

    /// <summary>
    /// Generic three-dimensional vector.
    /// </summary>
    public struct Vector3
    {

        /// <summary>
        /// X coordinate.
        /// </summary>
        public float x;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public float y;

        /// <summary>
        /// Z coordinate.
        /// </summary>
        public float z;

        /// <summary>
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>Length of the vector.</returns>
        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Sum of the two vectors.</returns>
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3()
            {
                x = a.x + b.x,
                y = a.y + b.y,
                z = a.z + b.z
            };
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector</param>
        /// <returns>Difference of the first and second vectors.</returns>
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3()
            {
                x = a.x - b.x,
                y = a.y - b.y,
                z = a.z - b.z
            };
        }

        /// <summary>
        /// Computes the product of a scalar and a vector.
        /// </summary>
        /// <param name="c">Scalar.</param>
        /// <param name="a">Vector.</param>
        /// <returns>Product of the scalar and vector.</returns>
        public static Vector3 operator *(float c, Vector3 a)
        {
            return new Vector3()
            {
                x = c * a.x,
                y = c * a.y,
                z = c * a.z
            };
        }

        /// <summary>
        /// Computes the product of a vector and a scalar.
        /// </summary>
        /// <param name="a">Vector.</param>
        /// <param name="c">Scalar.</param>
        /// <returns>Product of the vector and scalar.</returns>
        public static Vector3 operator *(Vector3 a, float c)
        {
            return c * a;
        }

        /// <summary>
        /// Computes the inner (dot) product of two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Inner product of the two vectors.</returns>
        public static float operator *(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

    }
}
