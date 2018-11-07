using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Resources.Buffers
{

    /// <summary>
    /// Vertex type with a three-component position and two-component texture coordinate.
    /// </summary>
    public struct Pos3Tex2Vertex
    {
        public float posX;
        public float posY;
        public float posZ;

        public float texX;
        public float texY;

        public Pos3Tex2Vertex(float posX, float posY, float posZ,
            float texX, float texY)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.texX = texX;
            this.texY = texY;
        }
    }

}
