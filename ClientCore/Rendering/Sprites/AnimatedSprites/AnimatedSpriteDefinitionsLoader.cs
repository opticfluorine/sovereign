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
        private readonly Deserializer deserializer = new DeserializerBuilder()
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
