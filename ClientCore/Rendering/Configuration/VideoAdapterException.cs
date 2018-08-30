using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Configuration
{

    /// <summary>
    /// Exception thrown when there is an issue with a video adapter.
    /// </summary>
    [System.Serializable]
    public class VideoAdapterException : ApplicationException
    {
        public VideoAdapterException() { }
        public VideoAdapterException(string message) : base(message) { }
        public VideoAdapterException(string message, Exception inner) : base(message, inner) { }
        protected VideoAdapterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
