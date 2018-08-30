using System;
using System.Runtime.Serialization;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Exception type thrown when an error occurs while loading a spritesheet definition.
    /// </summary>
    public class SpriteSheetDefinitionException : ApplicationException
    {
        public SpriteSheetDefinitionException()
        {
        }

        public SpriteSheetDefinitionException(string message) : base(message)
        {
        }

        public SpriteSheetDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SpriteSheetDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
