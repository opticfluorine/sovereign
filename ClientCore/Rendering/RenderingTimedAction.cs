using Castle.Core.Logging;
using Engine8.EngineCore.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Timed action that attempts to execute the renderer on the main thread at 60 FPS.
    /// </summary>
    public class RenderingTimedAction : ITimedAction
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        // Target 60 FPS.
        public ulong Interval { get; } = 16667;

        public void Invoke(ulong triggerTime)
        {
            Logger.DebugFormat("Begin rendering tick {0}.", triggerTime / Interval);
        }
    }

}
