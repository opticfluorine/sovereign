using Castle.Core.Logging;
using Engine8.ClientCore.Rendering.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for loading SDL_Surfaces and converting them to
    /// the display format.
    /// </summary>
    public class SurfaceLoader
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Main display.
        /// </summary>
        private readonly MainDisplay mainDisplay;

        public SurfaceLoader(MainDisplay mainDisplay)
        {
            this.mainDisplay = mainDisplay;
        }

    }

}
