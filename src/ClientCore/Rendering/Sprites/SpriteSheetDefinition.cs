/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

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
