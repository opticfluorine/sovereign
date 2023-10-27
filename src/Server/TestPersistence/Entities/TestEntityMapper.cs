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

using Moq;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Entities;
using Xunit;

namespace TestPersistence.Entities;

/// <summary>
///     Unit tests for EntityMapper.
/// </summary>
public class TestEntityMapper
{
    private const ulong FirstPersistedId = EntityAssigner.FirstPersistedId;

    /// <summary>
    ///     Tests the GetPersistedId(ulong) method with new volatile IDs.
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
        var result = mapper.GetPersistedId(vol, out var needToCreate);

        /* Assert. */
        Assert.Equal(per, result);
        Assert.Equal(expNeedToCreate, needToCreate);
    }

    /// <summary>
    ///     Tests that the GetPersistedId(ulong) method remembers volatile
    ///     IDs that it has already seen.
    /// </summary>
    [Fact]
    public void TestGetPersistedId_Known()
    {
        /* Arrange. */
        var mapper = GetMapper();

        /* Act. */
        var firstPersisted = mapper.GetPersistedId(0, out var needToCreate);
        var secondPersisted = mapper.GetPersistedId(1, out var needToCreate2);
        var firstPersisted2 = mapper.GetPersistedId(0, out var needToCreate3);
        var secondPersisted2 = mapper.GetPersistedId(1, out var needToCreate4);

        /* Assert. */
        Assert.Equal(firstPersisted, firstPersisted2);
        Assert.Equal(secondPersisted, secondPersisted2);
        Assert.NotEqual(firstPersisted, secondPersisted);
    }

    /// <summary>
    ///     Tests that GetVolatileId() passes unknown values through.
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
    ///     Tests that GetVolatileId() correctly maps known values.
    /// </summary>
    [Fact]
    public void TestGetVolatileId_Known()
    {
        /* Arrange. */
        var mapper = GetMapper();
        var firstPersisted = mapper.GetPersistedId(0, out var needToCreate1);
        var secondPersisted = mapper.GetPersistedId(1, out var needToCreate2);

        /* Act. */
        var firstVolatile = mapper.GetVolatileId(firstPersisted);
        var secondVolatile = mapper.GetVolatileId(secondPersisted);

        /* Assert. */
        Assert.Equal((ulong)0, firstVolatile);
        Assert.Equal((ulong)1, secondVolatile);
    }

    /// <summary>
    ///     Tests unloading an entity ID from the mapper.
    /// </summary>
    [Fact]
    public void TestUnloadId()
    {
        /* Generate a mapping. */
        var mapper = GetMapper();
        ulong entityId = 1;
        var firstPersisted = mapper.GetPersistedId(entityId, out var needToCreate);

        /* Confirm that the reverse mapping exists. */
        var firstReversed = mapper.GetVolatileId(firstPersisted);
        Assert.Equal(entityId, firstReversed);

        /* Unload the entity. */
        mapper.UnloadId(entityId);

        /* Confirm that the reverse mapping is eliminated. */
        var secondReversed = mapper.GetVolatileId(firstPersisted);
        Assert.NotEqual(entityId, secondReversed);

        /* Confirm that the forward mapping is eliminated. */
        var secondPersisted = mapper.GetPersistedId(entityId, out var needToCreate2);
        Assert.NotEqual(firstPersisted, secondPersisted);
    }

    /// <summary>
    ///     Gets an EntityMapper object to test.
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