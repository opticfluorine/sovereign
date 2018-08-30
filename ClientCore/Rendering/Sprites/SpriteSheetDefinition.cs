using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Describes a sprite sheet.
    /// </summary>
    public class SpriteSheetDefinition
    {

        /// <summary>
        /// Name of the spritesheet image file.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// ID number of the spritesheet.
        /// </summary>
        public int SheetId { get; set; }

        /// <summary>
        /// Sprite width.
        /// </summary>
        public int SpriteWidth { get; set; }

        /// <summary>
        /// Sprite height.
        /// </summary>
        public int SpriteHeight { get; set; }

        /// <summary>
        /// Author of the spritesheet.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// License rights of the spritesheet.f
        /// </summary>
        public string License { get; set; }

    }

}
