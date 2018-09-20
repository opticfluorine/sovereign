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
        private readonly Deserializer deserializer = new DeserializerBuilder()
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
