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

using Moq;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Entities;
using Xunit;

namespace TestPersistence.Entities
{

    /// <summary>
    /// Unit tests for EntityMapper.
    /// </summary>
    public class TestEntityMapper
    {

        private const ulong FirstPersistedId = EntityAssigner.FirstPersistedId;

        /// <summary>
        /// Tests the GetPersistedId(ulong) method with new volatile IDs.
        /// </summary>
        /// <param name="vol">Volatile entity ID.</param>
        /// <param name="per">Expected persisted entity ID.</param>
        /// <param name="expNeedToCreate">Whether the entity will need to be created.</param>
        [Theory]
        [InlineData(0, FirstPersistedId, true)]
        [InlineData(42, FirstPersistedId, true)]
        [InlineData(FirstPersistedId - 1, FirstPersistedId, true)]
        [InlineData(FirstPersistedId, FirstPersistedId, false)]
        [InlineData(FirstPersistedId + 1, FirstPersistedId + 1, false)]
        public void TestGetPersistedId_New(ulong vol, ulong per, bool expNeedToCreate)
        {
            /* Arrange. */
            var mapper = GetMapper();

            /* Act. */
            var result = mapper.GetPersistedId(vol, out bool needToCreate);

            /* Assert. */
            Assert.Equal(per, result);
            Assert.Equal(expNeedToCreate, needToCreate);
        }

        /// <summary>
        /// Tests that the GetPersistedId(ulong) method remembers volatile
        /// IDs that it has already seen.
        /// </summary>
        [Fact]
        public void TestGetPersistedId_Known()
        {
            /* Arrange. */
            var mapper = GetMapper();

            /* Act. */
            var firstPersisted = mapper.GetPersistedId(0, out bool needToCreate);
            var secondPersisted = mapper.GetPersistedId(1, out bool needToCreate2);
            var firstPersisted2 = mapper.GetPersistedId(0, out bool needToCreate3);
            var secondPersisted2 = mapper.GetPersistedId(1, out bool needToCreate4);

            /* Assert. */
            Assert.Equal(firstPersisted, firstPersisted2);
            Assert.Equal(secondPersisted, secondPersisted2);
            Assert.NotEqual(firstPersisted, secondPersisted);
        }

        /// <summary>
        /// Tests that GetVolatileId() passes unknown values through.
        /// </summary>
        [Fact]
        public void TestGetVolatileId_New()
        {
            /* Arrange. */
            var mapper = GetMapper();

            /* Act. */
            var result = mapper.GetVolatileId(FirstPersistedId);

            /* Assert. */
            Assert.Equal(FirstPersistedId, result);
        }

        /// <summary>
        /// Tests that GetVolatileId() correctly maps known values.
        /// </summary>
        [Fact]
        public void TestGetVolatileId_Known()
        {
            /* Arrange. */
            var mapper = GetMapper();
            var firstPersisted = mapper.GetPersistedId(0, out bool needToCreate1);
            var secondPersisted = mapper.GetPersistedId(1, out bool needToCreate2);

            /* Act. */
            var firstVolatile = mapper.GetVolatileId(firstPersisted);
            var secondVolatile = mapper.GetVolatileId(secondPersisted);

            /* Assert. */
            Assert.Equal((ulong)0, firstVolatile);
            Assert.Equal((ulong)1, secondVolatile);
        }

        /// <summary>
        /// Gets an EntityMapper object to test.
        /// </summary>
        /// <returns>EntityMapper object.</returns>
        private EntityMapper GetMapper()
        {
            var queryMock = new Mock<INextPersistedIdQuery>();
            queryMock.Setup(f => f.GetNextPersistedEntityId())
                .Returns(FirstPersistedId);

            var mapper = new EntityMapper();
            mapper.InitializeMapper(queryMock.Object);
            return mapper;
        }

    }

}
