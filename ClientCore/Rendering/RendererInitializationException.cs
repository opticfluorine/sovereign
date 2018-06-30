using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Exception type thrown when an error occurs while initializing a renderer.
    /// </summary>
    [Serializable]
    public class RendererInitializationException : ApplicationException
    {
        public RendererInitializationException()
        {
        }

        public RendererInitializationException(string message) : base(message)
        {
        }

        public RendererInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RendererInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
