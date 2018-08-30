using System;
using System.Runtime.Serialization;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Exception type thrown when an error related to a texture atlas occurs.
    /// </summary>
    public class TextureAtlasException : ApplicationException
    {
        public TextureAtlasException()
        {
        }

        public TextureAtlasException(string message) : base(message)
        {
        }

        public TextureAtlasException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TextureAtlasException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
