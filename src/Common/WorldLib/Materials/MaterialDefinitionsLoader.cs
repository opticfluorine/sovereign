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
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sovereign.WorldLib.Materials
{

    /// <summary>
    /// Loads materials definitions from a YAML file.
    /// </summary>
    public sealed class MaterialDefinitionsLoader
    {

        /// <summary>
        /// YAML deserializer.
        /// </summary>
        private readonly IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
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
