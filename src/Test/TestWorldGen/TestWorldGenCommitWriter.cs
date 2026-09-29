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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.Persistence;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Database.Sqlite;
using Sovereign.Persistence.WorldGen;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.WorldGeneration;
using Sovereign.WorldGen.Output;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Tests of the commit writer against a real SQLite database: batch idempotency and
///     decoration entity rows.
/// </summary>
public class TestWorldGenCommitWriter : IDisposable
{
    /// <summary>
    ///     Temporary directory holding the test database and staging directories.
    /// </summary>
    private readonly string directory;

    public TestWorldGenCommitWriter()
    {
        directory = Path.Combine(Path.GetTempPath(), "worldgen-commitwriter-test",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
    }

    public void Dispose()
    {
        Directory.Delete(directory, true);
    }

    [Fact]
    public void WriteDecorations_CreatesEntityRows()
    {
        using var fixture = new WriterFixture(Path.Combine(directory, "decorations.db"));
        var staging = StagingDir("decorations", segmentCount: 0, decorationCount: 3);

        var stats = fixture.Writer.Execute(new WorldGenCommitRequest
        {
            StagingDirectory = staging,
            BatchSize = 2
        });

        Assert.Equal(3, stats.DecorationsCreated);
        Assert.Equal(0, stats.SegmentsWritten);

        var rows = fixture.QueryDecorations();
        Assert.Equal(3, rows.Count);
        for (var i = 0; i < 3; ++i)
        {
            var row = rows[i];
            Assert.Equal((ulong)(EntityConstants.FirstPersistedEntityId + (uint)i), row.Id);
            Assert.Equal(0x7FFE000000000011UL, row.TemplateId);
            Assert.Equal(100f + i, row.X);
            Assert.Equal(200f + i, row.Y);
            Assert.Equal(21f, row.Z);
            Assert.Equal(0, row.EntityType);
            Assert.True(row.ParentId is null, "Decorations must not have parents.");
        }

        // The in-memory entity assigner must never hand out an ID from the block.
        Assert.True(fixture.EntityAssigner.NextEntityId >= EntityConstants.FirstPersistedEntityId + 2);
    }

    [Fact]
    public void WriteSegments_WritesBlobsAtSegmentCoordinates()
    {
        using var fixture = new WriterFixture(Path.Combine(directory, "segments.db"));
        var staging = StagingDir("segments", segmentCount: 2, decorationCount: 0);

        var stats = fixture.Writer.Execute(new WorldGenCommitRequest
        {
            StagingDirectory = staging,
            BatchSize = 1
        });

        Assert.Equal(2, stats.SegmentsWritten);
        foreach (var index in new[] { (0, 0, -1), (0, 1, -1) })
        {
            var expected = ReadStagedBlob(staging, index);
            Assert.Equal(expected, fixture.ReadSegmentBlob(index));
        }
    }

    [Fact]
    public void Execute_InterruptedAfterBatchK_ReRunMatchesSingleRun()
    {
        var dbPath = Path.Combine(directory, "idempotent.db");

        // Simulate an interruption after the second batch: the first two batches remain
        // committed, the rest are lost.
        int interruptedAfter;
        using (var fixture = new WriterFixture(dbPath))
        {
            var staging = StagingDir("idempotent", segmentCount: 5, decorationCount: 4);
            var exception = Assert.Throws<InvalidOperationException>(() =>
                fixture.Writer.Execute(new WorldGenCommitRequest
                {
                    StagingDirectory = staging,
                    BatchSize = 2,
                    AfterBatch = batch =>
                    {
                        if (batch >= 2)
                        {
                            throw new InvalidOperationException("simulated interruption");
                        }
                    }
                }));
            Assert.Equal("simulated interruption", exception.Message);
            interruptedAfter = CountRows(fixture);
        }

        // Re-run the same staged plan to completion; the final database state must be
        // identical to a single uninterrupted run over the same plan.
        Dictionary<(int X, int Y, int Z), byte[]> rerunBlobs;
        var rerunDecorationCount = 0;
        using (var fixture = new WriterFixture(dbPath))
        {
            var staging = StagingDir("idempotent", segmentCount: 5, decorationCount: 4);
            var stats = fixture.Writer.Execute(new WorldGenCommitRequest
            {
                StagingDirectory = staging,
                BatchSize = 2
            });

            Assert.Equal(5, stats.SegmentsWritten);
            Assert.Equal(4, stats.DecorationsCreated);
            rerunBlobs = fixture.ReadAllSegmentBlobs();
            rerunDecorationCount = fixture.QueryDecorations().Count;
        }

        using (var reference = new WriterFixture(Path.Combine(directory, "reference.db")))
        {
            var staging = StagingDir("idempotent", segmentCount: 5, decorationCount: 4);
            reference.Writer.Execute(new WorldGenCommitRequest
            {
                StagingDirectory = staging,
                BatchSize = 2
            });

            var referenceBlobs = reference.ReadAllSegmentBlobs();
            Assert.Equal(referenceBlobs.Count, rerunBlobs.Count);
            foreach (var (index, blob) in referenceBlobs)
            {
                Assert.True(rerunBlobs.TryGetValue(index, out var rerunBlob),
                    $"Segment {index} missing after re-run.");
                Assert.Equal(blob, rerunBlob);
            }

            Assert.Equal(reference.QueryDecorations().Count, rerunDecorationCount);
        }
    }

    [Fact]
    public void Execute_ReplaceWorld_DeletesOldRowsFirst()
    {
        var dbPath = Path.Combine(directory, "replace.db");
        var oldEntry = SeedReplacedWorld(dbPath);

        using var fixture = new WriterFixture(dbPath);
        var staging = StagingDir("replace", segmentCount: 1, decorationCount: 1);

        var stats = fixture.Writer.Execute(new WorldGenCommitRequest
        {
            StagingDirectory = staging,
            BatchSize = 8,
            ReplaceWorld = oldEntry
        });

        Assert.Equal(1, stats.DecorationsDeleted);

        // The old world's segment rows are gone; the staged blob remains.
        Assert.False(fixture.HasSegmentBlob(1, 0, -1), "Old world segment should be deleted.");
        Assert.Equal(ReadStagedBlob(staging, (0, 0, -1)), fixture.ReadSegmentBlob((0, 0, -1)));

        // Only the new decoration remains.
        var rows = fixture.QueryDecorations();
        Assert.Single(rows);
    }

    /// <summary>
    ///     Seeds a database with a "previously committed" world: one segment blob and one
    ///     decoration inside the old footprint.
    /// </summary>
    /// <param name="path">Database path.</param>
    /// <returns>The registry entry of the old world.</returns>
    private WorldGenRegistryEntry SeedReplacedWorld(string path)
    {
        using var fixture = new WriterFixture(path);

        var data = new WorldSegmentBlockData
        {
            DefaultsPerPlane = new BlockData[32]
        };
        data.DefaultsPerPlane[0] = new BlockData
        {
            BlockType = BlockDataType.Template,
            TemplateIdOffset = 5
        };
        var blob = MessageConfig.SerializeMsgPack(data);
        fixture.Provider.SetWorldSegmentBlockDataQuery.SetWorldSegmentBlockData(
            new Sovereign.EngineCore.Components.Types.GridPosition { X = 1, Y = 0, Z = -1 },
            blob, (IDbTransaction)null!);
        fixture.Provider.BulkAddEntitiesQuery.AddEntities(new List<BulkEntityRow>
        {
            new()
            {
                EntityId = EntityConstants.FirstPersistedEntityId + 500,
                TemplateEntityId = 0x7FFE000000000012UL,
                X = 32.5f,
                Y = 0.5f,
                Z = 21f,
                EntityType = 0
            }
        }, (IDbTransaction)null!);

        return new WorldGenRegistryEntry
        {
            Seed = 99,
            Profile = "test",
            OriginX = 32,
            OriginY = 0,
            Width = 64,
            Height = 64,
            MinSegmentZ = -2,
            MaxSegmentZ = 0,
            CommittedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    ///     Creates a staging directory with synthetic segment blobs and decorations.
    /// </summary>
    /// <param name="name">Staging directory name.</param>
    /// <param name="segmentCount">Number of synthetic segments to stage.</param>
    /// <param name="decorationCount">Number of synthetic decorations to stage.</param>
    /// <returns>Staging directory path.</returns>
    private string StagingDir(string name, int segmentCount, int decorationCount)
    {
        var staging = Path.Combine(directory, "staging_" + name);
        var segments = Path.Combine(staging, "segments");
        Directory.CreateDirectory(segments);

        var indices = new List<(int X, int Y, int Z)>();
        for (var i = 0; i < segmentCount; ++i)
        {
            indices.Add((i / 2, i % 2, -1));
        }

        foreach (var (x, y, z) in indices)
        {
            var data = new WorldSegmentBlockData
            {
                DefaultsPerPlane = new BlockData[32]
            };
            data.DefaultsPerPlane[0] = new BlockData
            {
                BlockType = BlockDataType.Template,
                TemplateIdOffset = (ulong)(x + 1)
            };
            File.WriteAllBytes(Path.Combine(segments, $"{x}_{y}_{z}.bin"),
                MessageConfig.SerializeMsgPack(data));
        }

        var decorations = new List<StagedDecoration>();
        for (var i = 0; i < decorationCount; ++i)
        {
            decorations.Add(new StagedDecoration
            {
                TemplateEntityId = 0x7FFE000000000011,
                X = 100f + i,
                Y = 200f + i,
                Z = 21f
            });
        }

        File.WriteAllBytes(Path.Combine(staging, "decorations.bin"),
            MessageConfig.SerializeMsgPack(decorations));

        File.WriteAllText(Path.Combine(staging, "plan.json"),
            "{\"seed\":1,\"minSegmentZ\":-1,\"maxSegmentZ\":-1}");
        return staging;
    }

    /// <summary>
    ///     Reads one staged blob back.
    /// </summary>
    /// <param name="staging">Staging directory.</param>
    /// <param name="index">Segment index triple.</param>
    /// <returns>Serialized blob.</returns>
    private static byte[] ReadStagedBlob(string staging, (int X, int Y, int Z) index)
    {
        return File.ReadAllBytes(Path.Combine(staging, "segments",
            $"{index.Item1}_{index.Item2}_{index.Item3}.bin"));
    }

    /// <summary>
    ///     Counts decoration entity rows.
    /// </summary>
    /// <param name="fixture">Fixture.</param>
    /// <returns>Row count.</returns>
    private static int CountRows(WriterFixture fixture)
    {
        return fixture.QueryDecorations().Count;
    }

    /// <summary>
    ///     Fixture holding a SQLite provider, commit writer, and entity assigner.
    /// </summary>
    private sealed class WriterFixture : IDisposable
    {
        public WriterFixture(string path)
        {
            EntityAssigner = new EntityAssigner();
            var options = Options.Create(new DatabaseOptions
            {
                DatabaseType = DatabaseType.Sqlite,
                Host = path,
                CreateIfMissing = true
            });
            Provider = new SqlitePersistenceProvider(options.Value,
                NullLogger<SqlitePersistenceProvider>.Instance);
            Writer = new WorldGenCommitWriter(new PersistenceProviderManager(options,
                NullLogger<SqlitePersistenceProvider>.Instance), EntityAssigner);
        }

        /// <summary>
        ///     The persistence provider.
        /// </summary>
        public SqlitePersistenceProvider Provider { get; }

        /// <summary>
        ///     The commit writer under test.
        /// </summary>
        public WorldGenCommitWriter Writer { get; }

        /// <summary>
        ///     The entity assigner whose high-water mark the writer advances.
        /// </summary>
        public EntityAssigner EntityAssigner { get; }

        /// <summary>
        ///     Queries the decoration entity rows created by the writer.
        /// </summary>
        /// <returns>Decoration rows in ID order.</returns>
        public List<(ulong Id, ulong TemplateId, float X, float Y, float Z, int EntityType,
            ulong? ParentId)> QueryDecorations()
        {
            var rows = new List<(ulong, ulong, float, float, float, int, ulong?)>();
            using var cmd = ((SqliteConnection)Provider.Connection).CreateCommand();
            cmd.CommandText =
                @"SELECT id, template_id, pos_x, pos_y, pos_z, entity_type, parent_id
                  FROM Entity WHERE id >= @First ORDER BY id";
            var param = cmd.CreateParameter();
            param.ParameterName = "First";
            param.Value = (long)EntityConstants.FirstPersistedEntityId;
            cmd.Parameters.Add(param);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(((ulong)reader.GetInt64(0), (ulong)reader.GetInt64(1),
                    reader.GetFloat(2), reader.GetFloat(3), reader.GetFloat(4),
                    reader.GetInt32(5), reader.IsDBNull(6) ? null : (ulong)reader.GetInt64(6)));
            }

            return rows;
        }

        /// <summary>
        ///     Reads every stored segment blob.
        /// </summary>
        /// <returns>Blobs keyed by segment index triple.</returns>
        public Dictionary<(int X, int Y, int Z), byte[]> ReadAllSegmentBlobs()
        {
            var blobs = new Dictionary<(int, int, int), byte[]>();
            using var cmd = ((SqliteConnection)Provider.Connection).CreateCommand();
            cmd.CommandText = "SELECT x, y, z, data FROM WorldSegmentBlockData";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var buffer = new byte[reader.GetBytes(3, 0, null, 0, 0)];
                reader.GetBytes(3, 0, buffer, 0, buffer.Length);
                blobs[(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2))] = buffer;
            }

            return blobs;
        }

        /// <summary>
        ///     Reads one segment blob.
        /// </summary>
        /// <param name="index">Segment index triple.</param>
        /// <returns>Blob, or an empty array if absent.</returns>
        public byte[] ReadSegmentBlob((int X, int Y, int Z) index)
        {
            using var cmd = ((SqliteConnection)Provider.Connection).CreateCommand();
            cmd.CommandText =
                "SELECT data FROM WorldSegmentBlockData WHERE x = @X AND y = @Y AND z = @Z";
            AddParameter(cmd, "X", index.Item1);
            AddParameter(cmd, "Y", index.Item2);
            AddParameter(cmd, "Z", index.Item3);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return Array.Empty<byte>();

            var buffer = new byte[reader.GetBytes(0, 0, null, 0, 0)];
            reader.GetBytes(0, 0, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        ///     Checks whether a segment blob exists.
        /// </summary>
        /// <param name="x">Segment X.</param>
        /// <param name="y">Segment Y.</param>
        /// <param name="z">Segment Z.</param>
        /// <returns>true if a blob exists at the coordinates.</returns>
        public bool HasSegmentBlob(int x, int y, int z)
        {
            using var cmd = ((SqliteConnection)Provider.Connection).CreateCommand();
            cmd.CommandText =
                "SELECT COUNT(*) FROM WorldSegmentBlockData WHERE x = @X AND y = @Y AND z = @Z";
            AddParameter(cmd, "X", x);
            AddParameter(cmd, "Y", y);
            AddParameter(cmd, "Z", z);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        public void Dispose()
        {
            Provider.Dispose();
        }

        /// <summary>
        ///     Adds one integer parameter to a command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <param name="name">Parameter name.</param>
        /// <param name="value">Value.</param>
        private static void AddParameter(SqliteCommand cmd, string name, int value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            cmd.Parameters.Add(param);
        }
    }
}
