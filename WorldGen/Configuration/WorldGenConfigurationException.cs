using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sovereign.WorldGen.Configuration
{

    /// <summary>
    /// Exception type thrown for errors related to WorldGen configuration.
    /// </summary>
    public sealed class WorldGenConfigurationException : ApplicationException
    {
        public WorldGenConfigurationException()
        {
        }

        public WorldGenConfigurationException(string message) : base(message)
        {
        }

        public WorldGenConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
