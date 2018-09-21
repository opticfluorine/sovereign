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
            CheckCollectiveConstraints(definitions);
        }

        /// <summary>
        /// Checks the definition constraints that apply to the list of all definitions.
        /// </summary>
        /// <param name="definitions">Spritesheet definitions.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if a constraint is violated.
        /// </exception>
        private void CheckCollectiveConstraints(IList<SpriteSheetDefinition> definitions)
        {
            CheckSheetIdNotDuplicated(definitions);
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
                CheckSheetIdNotNegative(definition);
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
        /// Individual constraint that ensures that the sheet ID is not negative.
        /// </summary>
        /// <param name="definition">Definition.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if the constraint is violated.
        /// </exception>
        private void CheckSheetIdNotNegative(SpriteSheetDefinition definition)
        {
            if (definition.SheetId < 0)
            {
                ReportIndividualViolation("SheetId must be greater than or equal to zero.", 
                    definition);
            }
        }

        /// <summary>
        /// Collective constraint that ensures that sheet IDs are not duplicated.
        /// </summary>
        /// <param name="definitions">List of all definitions.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown if the constraint is violated.
        /// </exception>
        private void CheckSheetIdNotDuplicated(IList<SpriteSheetDefinition> definitions)
        {
            /* Identify the bad definitions. */
            var badDefinitions = definitions.GroupBy(def => def.SheetId)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .OrderBy(def => def.SheetId)
                .ThenBy(def => def.Filename);

            /* Fire an error if needed. */
            if (badDefinitions.Count() > 0)
            {
                ReportCollectiveViolation("Sheet IDs must be unique.", badDefinitions);
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
                .Append(violatingDefinition.Filename)
                .Append(" (Sheet ID = ").Append(violatingDefinition.SheetId)
                .Append(").");

            /* Fire exception. */
            throw new SpriteSheetDefinitionException(sb.ToString());
        }

        /// <summary>
        /// Reports a constraint violation by a collection of definitions.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="violatingDefinitions">Violating definitions.</param>
        /// <exception cref="SpriteSheetDefinitionException">
        /// Thrown with the given message and definition details.
        /// </exception>
        private void ReportCollectiveViolation(string message, 
            IEnumerable<SpriteSheetDefinition> violatingDefinitions)
        {
            /* Prepare detailed message. */
            var sb = new StringBuilder();
            sb.Append(message).Append("\n")
                .Append("Check the spritesheet definition files for the following spritesheets:\n");
            foreach (var definition in violatingDefinitions.OrderBy(def => def.SheetId))
            {
                sb.Append("\n").Append(definition.Filename)
                    .Append(" (Sheet ID = ").Append(definition.SheetId)
                    .Append(")");
            }

            /* Fire exception. */
            throw new SpriteSheetDefinitionException(sb.ToString());
        }

    }

}
