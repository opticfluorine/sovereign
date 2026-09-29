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

using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Sqlite;
using Sovereign.Persistence.WorldGen;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests of the worldgen world registry: footprint classification, JSON round-trip,
///     and the global key-value store against a real SQLite database.
/// </summary>
public class TestWorldGenWorldRegistry : IDisposable
{
    /// <summary>
    ///     Temporary directory holding the test database.
    /// </summary>
    private readonly string directory;

    public TestWorldGenWorldRegistry()
    {
        directory = Path.Combine(Path.GetTempPath(), "worldgen-registry-test",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
    }

    public void Dispose()
    {
        Directory.Delete(directory, true);
    }

    [Fact]
    public void Classify_DisjointFootprints_Accept()
    {
        var world = Entry(0, 0);
        Assert.Equal(WorldGenFootprintRelation.Disjoint,
            WorldGenWorldRegistry.Classify(world, 64, 0, 32, 32));
    }

    [Fact]
    public void Classify_TouchingEdges_Accept()
    {
        var world = Entry(0, 0, width: 32, height: 32);
        Assert.Equal(WorldGenFootprintRelation.Disjoint,
            WorldGenWorldRegistry.Classify(world, 32, 0, 32, 32));
        Assert.Equal(WorldGenFootprintRelation.Disjoint,
            WorldGenWorldRegistry.Classify(world, 0, 32, 32, 32));
    }

    [Fact]
    public void Classify_ContainedWorld_RequiresReplace()
    {
        var world = Entry(16, 16, width: 32, height: 32);
        Assert.Equal(WorldGenFootprintRelation.Contains,
            WorldGenWorldRegistry.Classify(world, 0, 0, 128, 128));
    }

    [Fact]
    public void Classify_IdenticalFootprint_RequiresReplace()
    {
        var world = Entry(0, 0, width: 128, height: 128);
        Assert.Equal(WorldGenFootprintRelation.Contains,
            WorldGenWorldRegistry.Classify(world, 0, 0, 128, 128));
    }

    [Fact]
    public void Classify_StraddlingFootprints_Refuse()
    {
        var world = Entry(0, 0, width: 64, height: 64);
        Assert.Equal(WorldGenFootprintRelation.Straddles,
            WorldGenWorldRegistry.Classify(world, 32, 0, 64, 64));
        Assert.Equal(WorldGenFootprintRelation.Straddles,
            WorldGenWorldRegistry.Classify(world, -32, -32, 64, 64));
    }

    [Fact]
    public void Classify_NewInsideOld_Refuses()
    {
        var world = Entry(0, 0, width: 128, height: 128);
        Assert.Equal(WorldGenFootprintRelation.Straddles,
            WorldGenWorldRegistry.Classify(world, 16, 16, 32, 32));
    }

    [Fact]
    public void Entry_JsonRoundTrips()
    {
        var entry = new WorldGenRegistryEntry
        {
            Seed = 123456789,
            Profile = "default",
            OriginX = -64,
            OriginY = 128,
            Width = 2048,
            Height = 1024,
            MinSegmentZ = -2,
            MaxSegmentZ = 0,
            CommittedAtUtc = new DateTime(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc)
        };

        var decoded = WorldGenRegistryEntry.FromJson(entry.ToJson());

        Assert.NotNull(decoded);
        Assert.Equal(entry.Seed, decoded!.Seed);
        Assert.Equal(entry.Profile, decoded.Profile);
        Assert.Equal(entry.OriginX, decoded.OriginX);
        Assert.Equal(entry.OriginY, decoded.OriginY);
        Assert.Equal(entry.Width, decoded.Width);
        Assert.Equal(entry.Height, decoded.Height);
        Assert.Equal(entry.MinSegmentZ, decoded.MinSegmentZ);
        Assert.Equal(entry.MaxSegmentZ, decoded.MaxSegmentZ);
        Assert.Equal(entry.CommittedAtUtc, decoded.CommittedAtUtc);
    }

    [Fact]
    public void Entry_FromJson_Malformed_ReturnsNull()
    {
        Assert.Null(WorldGenRegistryEntry.FromJson("not json at all"));
    }

    [Fact]
    public void Store_AppendAndLoad_RoundTrips()
    {
        using var store = new RegistryStoreFixture(Path.Combine(directory, "roundtrip.db"));

        store.Store.AppendWorld(Entry(0, 0));
        store.Store.AppendWorld(Entry(1024, -512));

        var worlds = store.Store.LoadWorlds();

        Assert.Equal(2, worlds.Count);
        Assert.Equal(0, worlds[0].OriginX);
        Assert.Equal(1024, worlds[1].OriginX);
        Assert.Equal(-512, worlds[1].OriginY);
    }

    [Fact]
    public void Store_Load_SkipsMalformedEntries()
    {
        var path = Path.Combine(directory, "malformed.db");
        using (var fixture = new RegistryStoreFixture(path))
        {
            using var transaction = fixture.Provider.Connection.BeginTransaction();
            fixture.Provider.UpdateGlobalKeyValuePairQuery.UpdateGlobalKeyValuePair(
                WorldGenWorldRegistry.KeyPrefix + "99", "{ this is not json", transaction);
            transaction.Commit();
        }

        using var fixture2 = new RegistryStoreFixture(path);
        Assert.Empty(fixture2.Store.LoadWorlds());
    }

    /// <summary>
    ///     Creates a registry entry for classification tests.
    /// </summary>
    /// <param name="originX">Origin X.</param>
    /// <param name="originY">Origin Y.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <returns>Entry.</returns>
    private static WorldGenRegistryEntry Entry(int originX, int originY, int width = 64,
        int height = 64)
    {
        return new WorldGenRegistryEntry
        {
            Seed = 42,
            Profile = "default",
            OriginX = originX,
            OriginY = originY,
            Width = width,
            Height = height,
            MinSegmentZ = -2,
            MaxSegmentZ = 0,
            CommittedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Fixture holding a SQLite-backed provider and registry store.
    /// </summary>
    private sealed class RegistryStoreFixture : IDisposable
    {
        public RegistryStoreFixture(string path)
        {
            var options = Options.Create(new DatabaseOptions
            {
                DatabaseType = DatabaseType.Sqlite,
                Host = path,
                CreateIfMissing = true
            });
            Provider = new SqlitePersistenceProvider(options.Value,
                NullLogger<SqlitePersistenceProvider>.Instance);
            Store = new WorldGenWorldRegistryStore(new PersistenceProviderManager(options,
                NullLogger<SqlitePersistenceProvider>.Instance));
        }

        /// <summary>
        ///     The persistence provider.
        /// </summary>
        public SqlitePersistenceProvider Provider { get; }

        /// <summary>
        ///     The registry store under test.
        /// </summary>
        public WorldGenWorldRegistryStore Store { get; }

        public void Dispose()
        {
            Provider.Dispose();
        }
    }
}
