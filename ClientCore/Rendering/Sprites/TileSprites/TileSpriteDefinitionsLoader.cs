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
        private readonly Deserializer deserializer = new DeserializerBuilder()
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
