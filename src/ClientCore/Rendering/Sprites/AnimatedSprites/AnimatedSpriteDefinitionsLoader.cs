/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites
{

    /// <summary>
    /// Loads the animated sprite definitions file.
    /// </summary>
    public sealed class AnimatedSpriteDefinitionsLoader
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
        private readonly AnimatedSpriteDefinitionsValidator validator;

        public AnimatedSpriteDefinitionsLoader(AnimatedSpriteDefinitionsValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>
        /// Loads and validates the animated sprite definitions.
        /// </summary>
        /// <param name="filename">Animated sprite definitions file.</param>
        /// <returns>Animated sprite definitions.</returns>
        /// <exception cref="AnimatedSpriteDefinitionsException">
        /// Thrown if the definitions cannot be loaded or are invalid.
        /// </exception>
        public AnimatedSpriteDefinitions LoadDefinitions(string filename)
        {
            /* Load definitions. */
            AnimatedSpriteDefinitions definitions;
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    definitions = deserializer.Deserialize<AnimatedSpriteDefinitions>(reader);
                }
            }
            catch (Exception e)
            {
                throw new AnimatedSpriteDefinitionsException(
                    "Failed to load animated sprite definitions.", e);
            }

            /* Validate. */
            validator.Validate(definitions);

            return definitions;
        }

    }

}
