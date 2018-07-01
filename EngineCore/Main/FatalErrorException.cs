using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Main
{

    /// <summary>
    /// Exception thrown when an error condition requires the engine to halt.
    /// </summary>
    /// 
    /// If this exception escapes the IoC container, it will not be reported as
    /// an unhandled exception.
    public class FatalErrorException : ApplicationException
    {
        public FatalErrorException()
        {
        }

        public FatalErrorException(string message) : base(message)
        {
        }

        public FatalErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FatalErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
