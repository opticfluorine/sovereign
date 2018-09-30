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
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for loading spritesheet definition files.
    /// </summary>
    public class SpriteSheetDefinitionLoader
    {

        /// <summary>
        /// YAML deserializer for the spritesheet definition file.
        /// </summary>
        private readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        /// <summary>
        /// Loads a spritesheet definition from the given file.
        /// </summary>
        /// <param name="filename">Definition file to load.</param>
        /// <returns>Spritesheet definition.</returns>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if an error occurs while loading the definition.
        /// </exception>
        public SpriteSheetDefinition LoadDefinition(string filename)
        {
            /* Deserialize the definition file. */
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    return deserializer.Deserialize<SpriteSheetDefinition>(reader);
                }
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.Append("Failed to load spritesheet definition file '")
                    .Append(filename).Append("'.");
                throw new SpriteSheetDefinitionException(sb.ToString(), e);
            }
        }

    }
}
