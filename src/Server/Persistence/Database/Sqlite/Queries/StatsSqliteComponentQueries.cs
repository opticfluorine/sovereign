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
///     Reusable component queries for Stats-valued components.
/// </summary>
public class StatsSqliteComponentQueries(string columnPrefix, SqliteConnection connection)
    : IAddComponentQuery<Stats>, IModifyComponentQuery<Stats>, IRemoveComponentQuery
{
    private readonly string addModifySql
        = $@"UPDATE Entity SET
                {columnPrefix}strength = @Strength,
                {columnPrefix}defense = @Defense,
                {columnPrefix}agility = @Agility,
                {columnPrefix}intelligence = @Intelligence,
                {columnPrefix}wisdom = @Wisdom,
                {columnPrefix}charisma = @Charisma,
                {columnPrefix}luck = @Luck
             WHERE id = @Id";

    private readonly string removeSql
        = $@"UPDATE Entity SET
                {columnPrefix}strength = NULL,
                {columnPrefix}defense = NULL,
                {columnPrefix}agility = NULL,
                {columnPrefix}intelligence = NULL,
                {columnPrefix}wisdom = NULL,
                {columnPrefix}charisma = NULL,
                {columnPrefix}luck = NULL
             WHERE id = @Id";

    public void Add(ulong entityId, Stats value, IDbTransaction transaction)
    {
        DoUpdate(entityId, value, transaction);
    }

    public void Modify(ulong entityId, Stats value, IDbTransaction transaction)
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
    ///     Updates the Stats columns for the given entity in the database.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">New stats value.</param>
    /// <param name="transaction">Database transaction.</param>
    private void DoUpdate(ulong entityId, Stats value, IDbTransaction transaction)
    {
        using var cmd = new SqliteCommand(addModifySql, connection, (SqliteTransaction)transaction);

        var pId = new SqliteParameter
        {
            ParameterName = "Id",
            SqliteType = SqliteType.Integer,
            Value = entityId
        };
        cmd.Parameters.Add(pId);

        var pStrength = new SqliteParameter
        {
            ParameterName = "Strength",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Strength
        };
        cmd.Parameters.Add(pStrength);

        var pDefense = new SqliteParameter
        {
            ParameterName = "Defense",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Defense
        };
        cmd.Parameters.Add(pDefense);

        var pAgility = new SqliteParameter
        {
            ParameterName = "Agility",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Agility
        };
        cmd.Parameters.Add(pAgility);

        var pIntelligence = new SqliteParameter
        {
            ParameterName = "Intelligence",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Intelligence
        };
        cmd.Parameters.Add(pIntelligence);

        var pWisdom = new SqliteParameter
        {
            ParameterName = "Wisdom",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Wisdom
        };
        cmd.Parameters.Add(pWisdom);

        var pCharisma = new SqliteParameter
        {
            ParameterName = "Charisma",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Charisma
        };
        cmd.Parameters.Add(pCharisma);

        var pLuck = new SqliteParameter
        {
            ParameterName = "Luck",
            SqliteType = SqliteType.Integer,
            Value = (long)value.Luck
        };
        cmd.Parameters.Add(pLuck);

        cmd.ExecuteNonQuery();
    }
}
