using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineUtil.Collections.Octree
{

    /// <summary>
    /// Enumeration of octants in an OctreeNode.
    /// </summary>
    [Flags]
    enum OctreeOctant
    {

        /// <summary>
        /// Indicates the octant is in the top half (greater z) of the space
        /// spanned by the OctreeNode.
        /// </summary>
        Top = 1 << 0,

        /// <summary>
        /// Indicates the octant is in the north half (greater y) of the space
        /// spanned by the OctreeNode.
        /// </summary>
        North = 1 << 1,

        /// <summary>
        /// Indicates the octant is in the eastern half (greater x) of the space
        /// spanned by the OctreeNode.
        /// </summary>
        East = 1 << 2,

    }

}
