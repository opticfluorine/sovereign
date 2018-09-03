using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Logging
{

    /// <summary>
    /// No-op error handler.
    /// </summary>
    public sealed class NullErrorHandler : IErrorHandler
    {

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly NullErrorHandler Instance = new NullErrorHandler();

        public void Error(string message)
        {
        }

    }
}
