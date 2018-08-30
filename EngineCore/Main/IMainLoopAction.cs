using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.Main
{

    /// <summary>
    /// Implemented by classes that provide actions to be executed
    /// periodically after a fixed number of main loop cycles.
    /// </summary>
    public interface IMainLoopAction
    {

        /// <summary>
        /// The number of main loop cycles after which the action is performed.
        /// </summary>
        ulong CycleInterval { get; }

        /// <summary>
        /// Executes the action. This will be called on the main thread.
        /// </summary>
        void Execute();

    }

}
