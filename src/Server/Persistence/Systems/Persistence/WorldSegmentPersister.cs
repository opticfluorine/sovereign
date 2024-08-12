// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Systems.WorldManagement;
using Sovereign.Persistence.Database;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Responsible for persisting modified world segment block data.
/// </summary>
public class WorldSegmentPersister
{
    private readonly WorldManagementServices worldManagementServices;

    public WorldSegmentPersister(WorldManagementServices worldManagementServices)
    {
        this.worldManagementServices = worldManagementServices;
    }

    /// <summary>
    ///     Synchronizes modified world segment block data to the database.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="transaction">Transaction.</param>
    public void SynchronizeWorldSegments(IPersistenceProvider provider, IDbTransaction transaction)
    {
        var segmentIndices = worldManagementServices.GetAndClearSegmentsToPersist();
        var dataTasks = segmentIndices
            .Select(index => Tuple.Create(index, worldManagementServices.GetWorldSegmentBlockData(index) ??
                                                 throw new Exception($"Null data task for segment index {index}.")))
            .ToList();
        var tasks = dataTasks.Select(task => task.Item2).ToArray();

        Task.WaitAll(tasks);

        foreach (var dataTask in dataTasks)
        {
            var (segmentIndex, task) = dataTask;
            if (task.IsFaulted) throw task.Exception ?? new Exception("Data task faulted.");
            SyncSingleData(segmentIndex, task.Result.Item1, provider, transaction);
        }
    }

    /// <summary>
    ///     Synchronizes the block data for a single world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="data">Data.</param>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="transaction">Database transaction.</param>
    private void SyncSingleData(GridPosition segmentIndex, WorldSegmentBlockData data, IPersistenceProvider provider, 
        IDbTransaction transaction)
    {
        var serializedData = MessageConfig.SerializeMsgPack(data);
        provider.SetWorldSegmentBlockDataQuery.SetWorldSegmentBlockData(segmentIndex, serializedData, transaction);
    }
}