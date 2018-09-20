using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Exception type thrown for errors related to sprite definitions.
    /// </summary>
    public sealed class SpriteDefinitionsException : ApplicationException
    {
        public SpriteDefinitionsException()
        {
        }

        public SpriteDefinitionsException(string message) : base(message)
        {
        }

        public SpriteDefinitionsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
