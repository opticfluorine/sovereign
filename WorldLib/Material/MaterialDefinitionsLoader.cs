﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.WorldLib.Material
{

    /// <summary>
    /// Loads materials definitions from a YAML file.
    /// </summary>
    public sealed class MaterialDefinitionsLoader
    {

        /// <summary>
        /// YAML deserializer.
        /// </summary>
        private readonly Deserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(new PascalCaseNamingConvention())
            .Build();

        /// <summary>
        /// Validator.
        /// </summary>
        private readonly MaterialDefinitionsValidator validator;

        public MaterialDefinitionsLoader(MaterialDefinitionsValidator validator)
        {
            this.validator = validator;
        }

        /// <summary>
        /// Loads material definitions from the given file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Material definitions.</returns>
        /// <exception cref="MaterialDefinitionsException">
        /// Thrown if the definitions file is invalid.
        /// </exception>
        public MaterialDefinitions LoadDefinitions(string filename)
        {
            /* Deserialize. */
            MaterialDefinitions materialDefinitions;
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    /* Read the YAML file. */
                    materialDefinitions = deserializer.Deserialize<MaterialDefinitions>(reader);
                }
            }
            catch (Exception e)
            {
                /* Wrap in a MaterialDefinitionsException. */
                var sb = new StringBuilder();
                sb.Append("Failed to load materials definition file '")
                    .Append(filename).Append("'.");
                throw new MaterialDefinitionsException(sb.ToString(), e);
            }
 
            /* Perform additional validation. */
            var isValid = validator.IsValid(materialDefinitions, out string errorMessages);
            if (!isValid)
            {
                var sb = new StringBuilder();
                sb.Append("Errors in materials definition file '")
                    .Append(filename).Append("':\n\n").Append(errorMessages);
                throw new MaterialDefinitionsException(sb.ToString());
            }

            return materialDefinitions;
        }

    }

}
