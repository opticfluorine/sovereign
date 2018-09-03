using Sovereign.EngineCore.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Server.Logging
{

    /// <summary>
    /// Server-side error handler (no-op).
    /// </summary>
    public class ServerErrorHandler : IErrorHandler
    {

        public void Error(string message)
        {
        }

    }
}
