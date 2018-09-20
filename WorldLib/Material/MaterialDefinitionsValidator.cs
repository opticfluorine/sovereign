﻿using Castle.Core.Logging;
using Sovereign.EngineUtil.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sovereign.WorldLib.Material
{

    /// <summary>
    /// Input validator for MaterialDefinitions.
    /// </summary>
    public sealed class MaterialDefinitionsValidator
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Validates the given material definitions.
        /// </summary>
        /// <param name="materialDefinitions">Material definitions to be validated.</param>
        /// <param name="errorMessages">Error messages produced, if any.</param>
        /// <returns>true if valid, false otherwise.</returns>
        public bool IsValid(MaterialDefinitions materialDefinitions, out string errorMessages)
        {
            var sb = new StringBuilder();

            var valid = CheckMaterialIdUniqueness(materialDefinitions, sb)
                && CheckMaterialModifierUniqueness(materialDefinitions, sb);

            errorMessages = sb.ToString().Trim();
            return valid;
        }

        /// <summary>
        /// Checks that IDs are not duplicated and run from 0 to n - 1.
        /// </summary>
        /// <param name="definitions">Definitions.</param>
        /// <param name="sb">StringBuilder for error reporting.</param>
        /// <returns>true if valid, false otherwise.</returns>
        private bool ValidateIds(MaterialDefinitions definitions,
            StringBuilder sb)
        {
            var spriteCount = definitions.Materials.Count;
            var validator = new ConsecutiveRangeValidation();
            validator.IsRangeConsecutive(definitions.Materials.Select(material => material.Id),
                0, spriteCount, out var duplicateIds, out var outOfRangeIds);

            var hasDuplicates = duplicateIds.Count > 0;
            var hasOutOfRanges = outOfRangeIds.Count > 0;
            var valid = hasDuplicates || hasOutOfRanges;

            if (!valid)
            {
                if (hasDuplicates)
                {
                    sb.Append("The following material IDs are duplicated:\n\n");
                    foreach (var id in duplicateIds)
                    {
                        sb.Append("Material ").Append(id).Append("\n");
                    }
                }

                if (hasDuplicates && hasOutOfRanges) sb.Append("\n");

                if (hasOutOfRanges)
                {
                    sb.Append("Material IDs must run consecutively from 0.\n")
                        .Append("The following IDs are out of range:\n\n");
                    foreach (var id in outOfRangeIds)
                    {
                        sb.Append("Material ").Append(id).Append("\n");
                    }
                }
            }

            return valid;
        }

        /// <summary>
        /// Checks that Material IDs are not duplicated in the definitions.
        /// </summary>
        /// <param name="materialDefinitions">Material definitions.</param>
        /// <param name="errorStringBuilder">String builder for error messages.</param>
        /// <returns>true if there are no duplicates, false otherwise.</returns>
        private bool CheckMaterialIdUniqueness(MaterialDefinitions materialDefinitions,
            StringBuilder errorStringBuilder)
        {
            /* Check whether any IDs are duplicated. */
            var uniqueIdCount = materialDefinitions.Materials
                .Select(material => material.Id)
                .Distinct().Count();
            var satisfied = uniqueIdCount == materialDefinitions.Materials.Count;

            /* Output an error message if there are any violations. */
            if (!satisfied)
            {
                var error = GetMaterialIdUniquenessErrorMessage(materialDefinitions);
                errorStringBuilder.Append(error).Append("\n\n");
                Logger.Error(() => GetMaterialIdUniquenessErrorMessage(materialDefinitions));
            }

            return satisfied;
        }

        /// <summary>
        /// Checks that material modifiers are not duplicated within any materials.
        /// </summary>
        /// <param name="materialDefinitions">Material definitions.</param>
        /// <param name="errorStringBuilder">String builder for error messages.</param>
        /// <returns>true if there are no duplicates, false otherwise.</returns>
        private bool CheckMaterialModifierUniqueness(MaterialDefinitions materialDefinitions,
            StringBuilder errorStringBuilder)
        {
            /* Check each material individually. */
            var satisfied = true;
            foreach (var material in materialDefinitions.Materials)
            {
                satisfied = satisfied && CheckSingleMaterialModiferUniqueness(material, errorStringBuilder);
            }
            return satisfied;
        }

        /// <summary>
        /// Checks that material modifiers are not duplicated within a material.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <param name="errorStringBuilder">String builder for error messages.</param>
        /// <returns>true if there are no duplicates, false otherwise.</returns>
        private bool CheckSingleMaterialModiferUniqueness(Material material,
            StringBuilder errorStringBuilder)
        {
            /* Check for duplicated IDs. */
            var uniqueIdCount = material.MaterialSubtypes
                .Select(materialSubtype => materialSubtype.MaterialModifier)
                .Distinct().Count();
            var satisfied = uniqueIdCount == material.MaterialSubtypes.Count;

            /* Output error message if needed. */
            if (!satisfied)
            {
                var error = GetMaterialModifierUniquenessErrorMessage(material);
                errorStringBuilder.Append(error).Append("\n\n");
                Logger.Error(error);
            }

            return satisfied;
        }

        /// <summary>
        /// Gets the error message for duplicated material IDs.
        /// </summary>
        /// <param name="materialDefinitions">Material definitions.</param>
        /// <returns>Error message.</returns>
        private string GetMaterialIdUniquenessErrorMessage(MaterialDefinitions materialDefinitions)
        {
            /* Identify the duplicated IDs. */
            var duplicateIds = materialDefinitions.Materials
                .GroupBy(material => material.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(materialId => materialId);

            /* Produce error message. */
            var sb = new StringBuilder();
            sb.Append("Material IDs must be unique within the material definitions.\n")
                .Append("The following Material IDs are duplicated:");
            foreach (var duplicateId in duplicateIds)
            {
                sb.Append("\nMaterial ID ").Append(duplicateId);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the error message for duplicated modifiers within a material.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>Error messages.</returns>
        private string GetMaterialModifierUniquenessErrorMessage(Material material)
        {
            /* Identify the duplicated modifiers. */
            var duplicateModifiers = material.MaterialSubtypes
                .GroupBy(materialSubtype => materialSubtype.MaterialModifier)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(modifier => modifier);

            /* Produce error message. */
            var sb = new StringBuilder();
            sb.Append("Material modifiers must be unique within a single Material.\n")
                .Append("For Material ").Append(material.Id)
                .Append(" (\"").Append(material.MaterialName).Append("\"), the following "
                + "modifiers are duplicated:");
            foreach (var duplicateModifier in duplicateModifiers)
            {
                sb.Append("\nMaterial Modifier ").Append(duplicateModifier);
            }

            return sb.ToString();
        }

    }

}
