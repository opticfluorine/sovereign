/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
