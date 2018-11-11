using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Resources.Buffers
{

    /// <summary>
    /// Vertex type with a three-component position and two-component texture coordinate.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Pos3Tex2Vertex
    {
        [FieldOffset(0)] public float posX;
        [FieldOffset(sizeof(float))] public float posY;
        [FieldOffset(2*sizeof(float))] public float posZ;

        [FieldOffset(3*sizeof(float))] public float texX;
        [FieldOffset(4*sizeof(float))] public float texY;

    }

}
