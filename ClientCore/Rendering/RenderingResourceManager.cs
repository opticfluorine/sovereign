using Engine8.ClientCore.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.ClientCore.Rendering
{

    /// <summary>
    /// Responsible for managing resources used by rendering.
    /// </summary>
    public class RenderingResourceManager
    {

        /// <summary>
        /// Spritesheet manager.
        /// </summary>
        private SpriteSheetManager spriteSheetManager;

        public RenderingResourceManager(SpriteSheetManager spriteSheetManager)
        {
            this.spriteSheetManager = spriteSheetManager;
        }

        /// <summary>
        /// Loads and initializes the resources used by rendering.
        /// </summary>
        public void InitializeResources()
        {
            spriteSheetManager.InitializeSpriteSheets();
        }

        /// <summary>
        /// Cleans up all resources used by rendering.
        /// </summary>
        public void CleanupResources()
        {
            spriteSheetManager.ReleaseSpriteSheets();
        }

    }

}
