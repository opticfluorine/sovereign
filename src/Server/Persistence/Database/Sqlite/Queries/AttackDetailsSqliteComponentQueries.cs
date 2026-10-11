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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

/// <summary>
///     Reusable component queries for AttackDetails-valued components.
/// </summary>
public class AttackDetailsSqliteComponentQueries(string columnPrefix, SqliteConnection connection)
    : IAddComponentQuery<AttackDetails>, IModifyComponentQuery<AttackDetails>, IRemoveComponentQuery
{
    private readonly string addModifySql
        = $@"UPDATE Entity SET
                {columnPrefix}range = @AttackRange,
                {columnPrefix}delay_us = @AttackDelayUs
             WHERE id = @Id";

    private readonly string removeSql
        = $@"UPDATE Entity SET
                {columnPrefix}range = NULL,
                {columnPrefix}delay_us = NULL
             WHERE id = @Id";

    public void Add(ulong entityId, AttackDetails value, IDbTransaction transaction)
    {
        DoUpdate(entityId, value, transaction);
    }

    public void Modify(ulong entityId, AttackDetails value, IDbTransaction transaction)
    {
        DoUpdate(entityId, value, transaction);
    }

    public void Remove(ulong entityId, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(removeSql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            SqliteType = SqliteType.Integer,
            Value = entityId
        };
        cmd.Parameters.Add(pId);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    ///     Updates the AttackDetails columns for the given entity in the database.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">New attack details value.</param>
    /// <param name="transaction">Database transaction.</param>
    private void DoUpdate(ulong entityId, AttackDetails value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(addModifySql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            SqliteType = SqliteType.Integer,
            Value = entityId
        };
        cmd.Parameters.Add(pId);

        var pAttackRange = new SqliteParameter
        {
            ParameterName = "AttackRange",
            SqliteType = SqliteType.Real,
            Value = value.AttackRange
        };
        cmd.Parameters.Add(pAttackRange);

        var pAttackDelayUs = new SqliteParameter
        {
            ParameterName = "AttackDelayUs",
            SqliteType = SqliteType.Integer,
            Value = (long)value.AttackDelayUs
        };
        cmd.Parameters.Add(pAttackDelayUs);

        cmd.ExecuteNonQuery();
    }
}
