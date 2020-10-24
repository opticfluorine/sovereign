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

using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// Responsible for validating the list of loaded spritesheet definitions.
    /// </summary>
    public class SpriteSheetDefinitionValidator
    {

        /// <summary>
        /// Validates the list of loaded spritesheet definitions.
        /// </summary>
        /// <param name="definitions">Spritesheet definitions.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if an error is found in the spritesheet definitions.
        /// </exception>
        public void Validate(IList<SpriteSheetDefinition> definitions)
        {
            CheckIndividualConstraints(definitions);
        }

        /// <summary>
        /// Checks the definition constraints that apply to individual definitions.
        /// </summary>
        /// <param name="definitions">Spritesheet definitions.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if a constraint is violated.
        /// </exception>
        private void CheckIndividualConstraints(IList<SpriteSheetDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                CheckSpriteDimensions(definition);
            }
        }

        /// <summary>
        /// Individual constraint that ensures that the sprite dimensions are positive.
        /// </summary>
        /// <param name="definition">Definition.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if the constraint is violated.
        /// </exception>
        private void CheckSpriteDimensions(SpriteSheetDefinition definition)
        {
            if (definition.SpriteWidth <= 0 || definition.SpriteHeight <= 0)
            {
                ReportIndividualViolation("SpriteWidth and SpriteHeight must both be greater than zero.",
                    definition);
            }
        }

        /// <summary>
        /// Reports a constraint violation for an individual definition.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="violatingDefinition">Violating definition.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown with the given message and definition details.
        /// </exception>
        private void ReportIndividualViolation(string message, 
            SpriteSheetDefinition violatingDefinition)
        {
            /* Prepare detailed message. */
            var sb = new StringBuilder();
            sb.Append(message).Append("\n")
                .Append("Check the spritesheet definition file for ")
                .Append(violatingDefinition.Filename);

            /* Fire exception. */
            throw new SpriteSheetDefinitionException(sb.ToString());
        }

    }

}
