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
    /// Loads sprite definitions from a YAML file.
    /// </summary>
    public sealed class SpriteDefinitionsLoader
    {

        /// <summary>
        /// Sprite definitions validator.
        /// </summary>
        private readonly SpriteDefinitionsValidator validator;

        /// <summary>
        /// YAML deserializer.
        /// </summary>
        private readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        public SpriteDefinitionsLoader(SpriteDefinitionsValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>
        /// Loads sprite definitions from the given file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Sprite definitions.</returns>
        /// <exception cref="SpriteDefinitionsException">
        /// Thrown if the definitions cannot be loaded or are invalid.
        /// </exception>
        public SpriteDefinitions LoadSpriteDefinitions(string filename)
        {
            /* Load definitions. */
            SpriteDefinitions definitions;
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    definitions = deserializer.Deserialize<SpriteDefinitions>(reader);
                }
            }
            catch (Exception e)
            {
                throw new SpriteDefinitionsException("Failed to read sprite definitions.", e);
            }

            validator.Validate(definitions);

            return definitions;
        }
        
    }

}
