/*
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

using Sovereign.Persistence.Database.Sqlite.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Unit tests for SqliteMigrationQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteMigrationQuery
    {
        private readonly SqliteTestFixture testFixture;

        public TestSqliteMigrationQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests the function of the migration query.
        /// </summary>
        [Fact]
        public void TestMigrationQuery()
        {
            /* Set the migration level. */
            const int level = 32768;
            testFixture.AddMigrationLevel(level);

            /* Execute the query. */
            var query = new SqliteMigrationQuery(testFixture.Connection);
            Assert.True(query.IsMigrationLevelApplied(level));
            Assert.False(query.IsMigrationLevelApplied(level + 1));
        }

    }

}
