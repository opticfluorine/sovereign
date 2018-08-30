using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sovereign.WorldLib.Material
{

    /// <summary>
    /// Exception type thrown when an error occurs with material definitions.
    /// </summary>
    public sealed class MaterialDefinitionsException : ApplicationException
    {
        public MaterialDefinitionsException()
        {
        }

        public MaterialDefinitionsException(string message) : base(message)
        {
        }

        public MaterialDefinitionsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
