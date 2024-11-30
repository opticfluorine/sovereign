/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineUtil.Validation;

namespace Sovereign.ClientCore.Rendering.Materials;

/// <summary>
///     Input validator for MaterialDefinitions.
/// </summary>
public sealed class MaterialDefinitionsValidator
{
    private readonly ILogger<MaterialDefinitionsValidator> logger;
    private readonly TileSpriteManager tileSpriteManager;

    public MaterialDefinitionsValidator(TileSpriteManager tileSpriteManager,
        ILogger<MaterialDefinitionsValidator> logger)
    {
        this.tileSpriteManager = tileSpriteManager;
        this.logger = logger;
    }

    /// <summary>
    ///     Validates the given material definitions.
    /// </summary>
    /// <param name="materialDefinitions">Material definitions to be validated.</param>
    /// <param name="errorMessages">Error messages produced, if any.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(MaterialDefinitions materialDefinitions, out string errorMessages)
    {
        var sb = new StringBuilder();

        var valid = CheckMaterialModifierUniqueness(materialDefinitions, sb)
                    && ValidateIds(materialDefinitions, sb)
                    && CheckTileSpriteIds(materialDefinitions.Materials, sb);

        errorMessages = sb.ToString().Trim();
        return valid;
    }

    /// <summary>
    ///     Checks that IDs are not duplicated and run from 1 to n.
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
            1, spriteCount + 1, out var duplicateIds, out var outOfRangeIds);

        var hasDuplicates = duplicateIds.Count > 0;
        var hasOutOfRanges = outOfRangeIds.Count > 0;
        var valid = !(hasDuplicates || hasOutOfRanges);

        if (!valid)
        {
            if (hasDuplicates)
            {
                sb.Append("The following material IDs are duplicated:\n\n");
                foreach (var id in duplicateIds) sb.Append("Material ").Append(id).Append("\n");
            }

            if (hasDuplicates && hasOutOfRanges) sb.Append("\n");

            if (hasOutOfRanges)
            {
                sb.Append("Material IDs must run consecutively from 1.\n")
                    .Append("The following IDs are out of range:\n\n");
                foreach (var id in outOfRangeIds) sb.Append("Material ").Append(id).Append("\n");
            }
        }

        return valid;
    }

    /// <summary>
    ///     Checks that material modifiers are not duplicated within any materials.
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
            satisfied = satisfied && CheckSingleMaterialModiferUniqueness(material, errorStringBuilder);
        return satisfied;
    }

    /// <summary>
    ///     Checks that material modifiers are not duplicated within a material.
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
            logger.LogError(error);
        }

        return satisfied;
    }

    /// <summary>
    ///     Gets the error message for duplicated material IDs.
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
        foreach (var duplicateId in duplicateIds) sb.Append("\nMaterial ID ").Append(duplicateId);

        return sb.ToString();
    }

    /// <summary>
    ///     Gets the error message for duplicated modifiers within a material.
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
            sb.Append("\nMaterial Modifier ").Append(duplicateModifier);

        return sb.ToString();
    }

    /// <summary>
    ///     Gets the error message for an invalid material ID.
    /// </summary>
    /// <param name="definitions">Definitions.</param>
    /// <returns>Error message.</returns>
    private string GetMaterialIdErrorMessage(MaterialDefinitions definitions)
    {
        /* Identify any invalid IDs. */
        var badIds = definitions.Materials
            .Where(material => material.Id < 1)
            .Select((material, idx) => material.Id);

        var sb = new StringBuilder();
        sb.Append("Material IDs must be greater than zero. Bad material IDs found:");
        foreach (var id in badIds) sb.Append("\nMaterial ").Append(id);

        return sb.ToString();
    }

    /// <summary>
    ///     Checks that all tile sprite references are valid.
    /// </summary>
    /// <param name="materials">Materials to check.</param>
    /// <param name="sb">StringBuilder for error reporting.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool CheckTileSpriteIds(List<Material> materials, StringBuilder sb)
    {
        var limit = tileSpriteManager.TileSprites.Count;
        var badIds = materials
            .Where(material => material.MaterialSubtypes.Any(subtype => subtype.TopFaceTileSpriteId >= limit
                                                                        || subtype.SideFaceTileSpriteId >= limit
                                                                        || subtype.ObscuredTopFaceTileSpriteId >=
                                                                        limit))
            .Select(material => material.Id)
            .ToList();
        var valid = badIds.Count == 0;

        if (!valid)
        {
            sb.Append("The following materials reference unknown Tile Sprites:\n\n");
            foreach (var badId in badIds) sb.Append("Material ").Append(badId).Append('\n');
        }

        return valid;
    }
}