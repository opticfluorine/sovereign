﻿/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.Data.Sqlite;
using Sovereign.Persistence.Database.Queries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.Database.Sqlite.Queries
{

    /// <summary>
    /// SQLite implementation of IRetrieveRangeQuery.
    /// </summary>
    public sealed class SqliteRetrieveRangeQuery : IRetrieveRangeQuery
    {

        private readonly SqliteConnection dbConnection;

        /// <summary>
        /// SQL query.
        /// </summary>
        private const string Query =
            @"SELECT * FROM EntityWithComponents
              WHERE x >= @X1 AND x < @X2
              AND y >= @Y1 AND y < @Y2
              AND z >= @Z1 AND z < @Z2";

        public SqliteRetrieveRangeQuery(IDbConnection dbConnection)
        {
            this.dbConnection = (SqliteConnection)dbConnection;
        }

        public QueryReader RetrieveEntitiesInRange(Vector3 minPos, Vector3 maxPos)
        {
            var cmd = PrepareCommand(minPos, maxPos);
            return new QueryReader(cmd);
        }

        /// <summary>
        /// Prepares the SQL command.
        /// </summary>
        /// <param name="minPos">Minimum position.</param>
        /// <param name="maxPos">Maximum position.</param>
        /// <returns>SQL command.</returns>
        private SqliteCommand PrepareCommand(Vector3 minPos, Vector3 maxPos)
        {
            var cmd = new SqliteCommand(Query, dbConnection);

            cmd.Parameters.Add(MakeParameter("X1", minPos.X));
            cmd.Parameters.Add(MakeParameter("X2", maxPos.X));
            cmd.Parameters.Add(MakeParameter("Y1", minPos.Y));
            cmd.Parameters.Add(MakeParameter("Y2", maxPos.Y));
            cmd.Parameters.Add(MakeParameter("Z1", minPos.Z));
            cmd.Parameters.Add(MakeParameter("Z2", maxPos.Z));

            return cmd;
        }

        /// <summary>
        /// Creates a query parameter.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        /// <returns>Parameter.</returns>
        private SqliteParameter MakeParameter(string name, float value)
        {
            var param = new SqliteParameter(name, value);
            param.SqliteType = SqliteType.Real;
            return param;
        }

    }

}
