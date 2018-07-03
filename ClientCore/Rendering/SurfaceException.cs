using System;
using System.Runtime.Serialization;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Exception type thrown when an error related to surfaces occurs.
    /// </summary>
    public class SurfaceException : ApplicationException
    {
        public SurfaceException()
        {
        }

        public SurfaceException(string message) : base(message)
        {
        }

        public SurfaceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SurfaceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
