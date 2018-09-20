using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Exception type thrown when errors related to tilesprite definitions occur.
    /// </summary>
    public sealed class TileSpriteDefinitionsException : ApplicationException
    {
        public TileSpriteDefinitionsException()
        {
        }

        public TileSpriteDefinitionsException(string message) : base(message)
        {
        }

        public TileSpriteDefinitionsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
