using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Engine8.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Describes a sprite sheet.
    /// </summary>
    public class SpriteSheetDefinition
    {

        /// <summary>
        /// Name of the spritesheet image file.
        /// </summary>
        public string SpriteSheetFilename { get; set; }

        /// <summary>
        /// Sprite width.
        /// </summary>
        public int SpriteWidth { get; set; }

        /// <summary>
        /// Sprite height.
        /// </summary>
        public int SpriteHeight { get; set; }

    }

}
