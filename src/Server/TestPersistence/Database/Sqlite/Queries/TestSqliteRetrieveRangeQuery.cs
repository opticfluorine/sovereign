/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System.Numerics;
using Sovereign.Persistence.Database.Sqlite.Queries;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries;

/// <summary>
///     Unit tests for SqliteRetrieveRangeQuery.
/// </summary>
[Collection("Sqlite")]
public sealed class TestSqliteRetrieveRangeQuery
{
    private const ulong baseEntityId = 0x7fff000000020000;
    private readonly SqliteTestFixture testFixture;

    public TestSqliteRetrieveRangeQuery(SqliteTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    /// <summary>
    ///     Tests that the query retrieves only the entities that are
    ///     positioned in a given range.
    /// </summary>
    [Fact]
    public void TestRetrieveRange()
    {
        /* Define the range. */
        const float minPos = -1.0f;
        const float maxPos = 1.0f;
        const ulong lowerBoundId = baseEntityId;
        const ulong inRangeId = baseEntityId + 1;
        const ulong upperBoundId = baseEntityId + 2;
        const ulong outRangeId = baseEntityId + 3;
        const ulong unpositionedId = baseEntityId + 4;

        /* Add an entity that is positioned on the lower bound. */
        testFixture.AddEntity(lowerBoundId);
        var lowerBoundPos = new Vector3(minPos);
        testFixture.PositionEntity(lowerBoundId, lowerBoundPos);

        /* Add an entity that is positioned in the range. */
        testFixture.AddEntity(inRangeId);
        var inRangePos = new Vector3((minPos + maxPos) * 0.5f);
        testFixture.PositionEntity(inRangeId, inRangePos);

        /* Add an entity that is positioned on the upper bound. */
        testFixture.AddEntity(upperBoundId);
        var upperBoundPos = new Vector3(maxPos);
        testFixture.PositionEntity(upperBoundId, upperBoundPos);

        /* Add an entity that is positioned out of range. */
        testFixture.AddEntity(outRangeId);
        var outRangePos = new Vector3(maxPos + 1.0f);
        testFixture.PositionEntity(outRangeId, outRangePos);

        /* Add an entity that is not positioned. */
        testFixture.AddEntity(unpositionedId);

        /* Query all entities in range. */
        var query = new SqliteRetrieveRangeQuery(testFixture.Connection);
        using (var reader = query.RetrieveEntitiesInRange(
                   lowerBoundPos, upperBoundPos))
        {
            var count = 0;
            while (reader.Reader.Read())
            {
                var id = (ulong)reader.Reader.GetInt64(0);

                // Ignore prepopulated "default" entities supplied by the test fixture.
                if (id < baseEntityId) continue;

                Assert.True(id == lowerBoundId || id == inRangeId);
                count++;
            }

            Assert.Equal(2, count);
        }
    }
}