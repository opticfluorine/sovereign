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
        private readonly Deserializer deserializer = new DeserializerBuilder()
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
