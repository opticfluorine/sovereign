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
    /// Unit tests for SqliteNextPersistedIdQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteNextPersistedIdQuery
    {
        private readonly SqliteTestFixture testFixture;

        public TestSqliteNextPersistedIdQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests that the query produces an unused ID.
        /// </summary>
        [Fact]
        public void TestNextIdQuery()
        {
            /* Add an entity with a large ID. */
            const ulong entityId = 0x7ffff00000000000;
            testFixture.AddEntity(entityId);

            /* Execute the query. */
            var query = new SqliteNextPersistedIdQuery(testFixture.Connection);
            var nextId = query.GetNextPersistedEntityId();
            Assert.Equal(entityId + 1, nextId);
        }

    }

}
