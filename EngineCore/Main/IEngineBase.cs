using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Main
{

    /// <summary>
    /// Interface implemented by the root component of the engine.
    /// </summary>
    public interface IEngineBase
    {

        /// <summary>
        /// Runs the engine.
        /// </summary>
        void Run();

    }

}
