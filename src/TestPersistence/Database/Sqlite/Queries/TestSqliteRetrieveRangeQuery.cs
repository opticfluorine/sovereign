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

using Sovereign.Persistence.Database.Sqlite.Queries;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

namespace TestPersistence.Database.Sqlite.Queries
{

    /// <summary>
    /// Unit tests for SqliteRetrieveRangeQuery.
    /// </summary>
    [Collection("Sqlite")]
    public sealed class TestSqliteRetrieveRangeQuery
    {
        private readonly SqliteTestFixture testFixture;

        private const ulong baseEntityId = 0x7fff000000020000;

        public TestSqliteRetrieveRangeQuery(SqliteTestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /// <summary>
        /// Tests that the query retrieves only the entities that are
        /// positioned in a given range.
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
            Vector3 lowerBoundPos = new Vector3(minPos);
            testFixture.PositionEntity(lowerBoundId, lowerBoundPos);
            
            /* Add an entity that is positioned in the range. */
            testFixture.AddEntity(inRangeId);
            Vector3 inRangePos = new Vector3((minPos + maxPos) * 0.5f);
            testFixture.PositionEntity(inRangeId, inRangePos);

            /* Add an entity that is positioned on the upper bound. */
            testFixture.AddEntity(upperBoundId);
            Vector3 upperBoundPos = new Vector3(maxPos);
            testFixture.PositionEntity(upperBoundId, upperBoundPos);

            /* Add an entity that is positioned out of range. */
            testFixture.AddEntity(outRangeId);
            Vector3 outRangePos = new Vector3(maxPos + 1.0f);
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
                    count++;
                    var id = (ulong)reader.Reader.GetInt64(0);
                    Assert.True(id == lowerBoundId || id == inRangeId);
                }
                Assert.Equal(2, count);
            }
        }

    }

}
