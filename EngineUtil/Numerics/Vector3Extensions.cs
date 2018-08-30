using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.EngineUtil.Numerics
{

    /// <summary>
    /// Utility extensions to System.Numerics.Vector3.
    /// </summary>
    public static class Vector3Extensions
    {

        /// <summary>
        /// Checks whether all components in the vector are less than the
        /// corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are less; false otherwise.</returns>
        public static bool LessThanAll(this Vector3 left, Vector3 right)
        {
            return left.X < right.X && left.Y < right.Y && left.Z < right.Z;
        }

        /// <summary>
        /// Checks whether all components in the vector are less than or equal
        /// to the corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are less than or equal; false otherwise.</returns>
        public static bool LessThanOrEqualAll(this Vector3 left, Vector3 right)
        {
            return left.X <= right.X && left.Y <= right.Y && left.Z <= right.Z;
        }

        /// <summary>
        /// Checks whether all components in the vector are greater than or equal
        /// to the corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are greater than or equal; false otherwise.</returns>
        public static bool GreaterThanOrEqualAll(this Vector3 left, Vector3 right)
        {
            return left.X >= right.X && left.Y >= right.Y && left.Z >= right.Z;
        }

        /// <summary>
        /// Checks whether all components in the vector are greater than
        /// the corresponding components in another vector.
        /// </summary>
        /// <param name="left">This vector.</param>
        /// <param name="right">Other vector.</param>
        /// <returns>true if all components are greater; false otherwise.</returns>
        public static bool GreaterThanAll(this Vector3 left, Vector3 right)
        {
            return left.X > right.X && left.Y > right.Y && left.Z > right.Z;
        }

    }
}
