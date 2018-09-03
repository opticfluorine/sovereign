using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Logging
{

    /// <summary>
    /// Interface for reporting errors separately from the logging mechanism.
    /// </summary>
    public interface IErrorHandler
    {

        /// <summary>
        /// Reports the given error.
        /// </summary>
        /// <param name="message">Error message.</param>
        void Error(string message);

    }

}
