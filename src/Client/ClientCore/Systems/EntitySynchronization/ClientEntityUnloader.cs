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

using System.Linq;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.WorldManagement.Components.Indexers;

namespace Sovereign.ClientCore.Systems.EntitySynchronization;

/// <summary>
///     Responsible for unloading entities from memory for the client.
/// </summary>
public class ClientEntityUnloader
{
    private readonly EntityManager entityManager;
    private readonly EntityHierarchyIndexer hierarchyIndexer;
    private readonly WorldSegmentIndexer segmentIndexer;

    /// <summary>
    ///     Current player entity ID.
    /// </summary>
    private ulong playerEntityId;

    public ClientEntityUnloader(WorldSegmentIndexer segmentIndexer, EntityHierarchyIndexer hierarchyIndexer,
        EntityManager entityManager)
    {
        this.segmentIndexer = segmentIndexer;
        this.hierarchyIndexer = hierarchyIndexer;
        this.entityManager = entityManager;
    }

    /// <summary>
    ///     Unloads all entities from the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    public void UnloadWorldSegment(GridPosition segmentIndex)
    {
        // Get all positioned entities in the segment, excluding the player (which can occur rarely if the
        // client is far behind the server when the unsubscribe event is received).
        // Then join them with any descendants.
        var entitiesToUnload = segmentIndexer.GetEntitiesInWorldSegment(segmentIndex)
            .Where(entityId => entityId != playerEntityId)
            .SelectMany(entityId =>
            {
                var descendants = hierarchyIndexer.GetAllDescendants(entityId);
                descendants.Add(entityId);
                return descendants;
            });
        foreach (var entityId in entitiesToUnload) entityManager.UnloadEntity(entityId);
    }

    /// <summary>
    ///     Sets the current player entity ID to prevent accidental unloading in high-latency situations.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    public void SetPlayer(ulong playerEntityId)
    {
        this.playerEntityId = playerEntityId;
    }
}