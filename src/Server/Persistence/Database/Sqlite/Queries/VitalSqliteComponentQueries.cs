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
///     Reusable component queries for Vital-valued components.
/// </summary>
public class VitalSqliteComponentQueries(string columnPrefix, SqliteConnection connection)
    : IAddComponentQuery<Vital>, IModifyComponentQuery<Vital>, IRemoveComponentQuery
{
    private readonly string addModifySql
        = $@"UPDATE Entity SET
                {columnPrefix}value = @Value,
                {columnPrefix}max_value = @MaxValue,
                {columnPrefix}change_rate = @ChangeRate,
                {columnPrefix}change_interval = @ChangeInterval
             WHERE id = @Id";

    private readonly string removeSql
        = $@"UPDATE Entity SET
                {columnPrefix}value = NULL,
                {columnPrefix}max_value = NULL,
                {columnPrefix}change_rate = NULL,
                {columnPrefix}change_interval = NULL
             WHERE id = @Id";

    public void Add(ulong entityId, Vital value, IDbTransaction transaction)
    {
        DoUpdate(entityId, value, transaction);
    }

    public void Modify(ulong entityId, Vital value, IDbTransaction transaction)
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
    ///     Updates the Vital columns for the given entity in the database.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">New vital value.</param>
    /// <param name="transaction">Database transaction.</param>
    private void DoUpdate(ulong entityId, Vital value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(addModifySql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            SqliteType = SqliteType.Integer,
            Value = entityId
        };
        cmd.Parameters.Add(pId);

        var pValue = new SqliteParameter
        {
            ParameterName = "Value",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Value
        };
        cmd.Parameters.Add(pValue);

        var pMaxValue = new SqliteParameter
        {
            ParameterName = "MaxValue",
            SqliteType = SqliteType.Integer,
            Value = (long)value.MaxValue
        };
        cmd.Parameters.Add(pMaxValue);

        var pChangeRate = new SqliteParameter
        {
            ParameterName = "ChangeRate",
            SqliteType = SqliteType.Integer,
            Value = (long)value.ChangeRate
        };
        cmd.Parameters.Add(pChangeRate);

        var pChangeInterval = new SqliteParameter
        {
            ParameterName = "ChangeInterval",
            SqliteType = SqliteType.Integer,
            Value = (long)value.ChangeInterval
        };
        cmd.Parameters.Add(pChangeInterval);

        cmd.ExecuteNonQuery();
    }
}
