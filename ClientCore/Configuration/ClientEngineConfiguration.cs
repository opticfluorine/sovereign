using Sovereign.EngineCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Configuration
{

    /// <summary>
    /// Client-side engine configuration.
    /// </summary>
    public class ClientEngineConfiguration : IEngineConfiguration
    {

        /* Events advance every 10 ms. */
        public ulong EventTickInterval => 10000;

    }

}
