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

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites
{

    /// <summary>
    /// Loads tile sprite definitions from a YAML file.
    /// </summary>
    public sealed class TileSpriteDefinitionsLoader
    {

        /// <summary>
        /// YAML deserializer.
        /// </summary>
        private readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        /// <summary>
        /// Definitions validator.
        /// </summary>
        private readonly TileSpriteDefinitionsValidator validator;

        public TileSpriteDefinitionsLoader(TileSpriteDefinitionsValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>
        /// Loads tile sprite definitions from the given file.
        /// </summary>
        /// <param name="filename">Definitions filename.</param>
        /// <returns>Tile sprite definitions.</returns>
        /// <exception cref="TileSpriteDefinitionsException">
        /// Thrown if the definitions cannot be loaded or are invalid.
        /// </exception>
        public TileSpriteDefinitions LoadDefinitions(string filename)
        {
            /* Attempt to load the definitions. */
            TileSpriteDefinitions definitions;
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    definitions = deserializer.Deserialize<TileSpriteDefinitions>(reader);
                }
            }
            catch (Exception e)
            {
                /* Wrap the exception. */
                throw new TileSpriteDefinitionsException("Failed to load tile sprite definitions.", e);
            }

            /* Validate the definitions. */
            validator.Validate(definitions);

            /* Postprocess the definitions. */

            /* Done. */
            return definitions;
        }

    }

}
