using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    public sealed class AnimatedSpriteDefinitionsException : ApplicationException
    {
        public AnimatedSpriteDefinitionsException()
        {
        }

        public AnimatedSpriteDefinitionsException(string message) : base(message)
        {
        }

        public AnimatedSpriteDefinitionsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
