// Sovereign Engine
// Copyright (c) 2026 opticfluorine
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     One entity row to be written by <see cref="IBulkAddEntitiesQuery" />.
/// </summary>
public sealed class BulkEntityRow
{
    /// <summary>
    ///     Entity ID.
    /// </summary>
    public required ulong EntityId { get; init; }

    /// <summary>
    ///     Template entity ID of the entity.
    /// </summary>
    public required ulong TemplateEntityId { get; init; }

    /// <summary>
    ///     World X coordinate of the entity.
    /// </summary>
    public required float X { get; init; }

    /// <summary>
    ///     World Y coordinate of the entity.
    /// </summary>
    public required float Y { get; init; }

    /// <summary>
    ///     World Z coordinate of the entity.
    /// </summary>
    public required float Z { get; init; }

    /// <summary>
    ///     Entity type of the entity.
    /// </summary>
    public required int EntityType { get; init; }
}

/// <summary>
///     Query for bulk-adding positioned, templated entity rows to the database.
/// </summary>
public interface IBulkAddEntitiesQuery
{
    /// <summary>
    ///     Adds the given entity rows. All rows are written within the caller's transaction.
    /// </summary>
    /// <param name="rows">Rows to add.</param>
    /// <param name="transaction">Transaction.</param>
    void AddEntities(IReadOnlyList<BulkEntityRow> rows, System.Data.IDbTransaction transaction);
}
