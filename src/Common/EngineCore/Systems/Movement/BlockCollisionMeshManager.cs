// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Collections.Generic;
using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Responsible for managing the set of block entity collision meshes.
/// </summary>
public class BlockCollisionMeshManager
{
}

/// <summary>
///     Represents an immutable set of block entity collision meshes for a single world segment.
/// </summary>
internal class MeshSet
{
    /// <summary>
    ///     Flag indicating whether this mesh set has been simplified.
    /// </summary>
    private readonly bool isSimplified;

    private readonly WorldSegmentResolver resolver;

    private readonly GridPosition segmentIndex;

    /// <summary>
    ///     Array indexed by segment local Z offset of per-layer list of block entity collision slabs.
    /// </summary>
    private readonly List<BoundingBox>[] slabsByZ;

    /// <summary>
    ///     Creates a new MeshSet containing the given blocks.
    /// </summary>
    /// <param name="blocks">Blocks.</param>
    public MeshSet(List<GridPosition> blocks, GridPosition segmentIndex, WorldSegmentResolver resolver)
    {
        slabsByZ = MakeEmptyLayers();
        isSimplified = false;
        this.segmentIndex = segmentIndex;
        this.resolver = resolver;

        var baseZ = segmentIndex.Z * WorldManagementConfiguration.SegmentLength;
        var blockOffset = new Vector3(1.0f, -1.0f, -1.0f);

        foreach (var block in blocks)
        {
            var z = block.Z - baseZ;
            if (z < 0 || z >= WorldManagementConfiguration.SegmentLength)
                throw new ArgumentOutOfRangeException("blocks",
                    $"Block index {block.Z} out of range (baseZ = {baseZ}.");

            var basePos = (Vector3)block;
            slabsByZ[z].Add(new BoundingBox
            {
                Position = basePos,
                Size = basePos + blockOffset
            });
        }
    }

    /// <summary>
    ///     Creates a new MeshSet containing the given collision slabs.
    /// </summary>
    /// <param name="slabsByZ"></param>
    private MeshSet(List<BoundingBox>[] slabsByZ, GridPosition segmentIndex, WorldSegmentResolver resolver)
    {
        this.slabsByZ = slabsByZ;
        isSimplified = true;
        this.segmentIndex = segmentIndex;
        this.resolver = resolver;
    }

    /// <summary>
    ///     Simplifies this MeshSet by greedily merging blocks in each Z layer repeatedly to form the
    ///     largest possible collision slabs in each layer, producing a new MeshSet if any changes are made.
    /// </summary>
    public MeshSet Simplify()
    {
        if (isSimplified) return this;

        var newLayers = MakeEmptyLayers();
        for (var i = 0; i < WorldManagementConfiguration.SegmentLength; ++i) SimplifyLayer(slabsByZ[i], newLayers[i]);

        return new MeshSet(newLayers, segmentIndex, resolver);
    }

    /// <summary>
    ///     Simplifies the input layer to produce an output layer with largest collision slabs.
    /// </summary>
    /// <param name="input">Input layer consisting of unit blocks.</param>
    /// <param name="output">Output layer to be populated with largest collision slabs.</param>
    private void SimplifyLayer(List<BoundingBox> input, List<BoundingBox> output)
    {
        // Since not already simplified, the inputs are a list of individual blocks.
        // General approach to simplification per layer:
        //   1. Partition to connected components (union-find).
        //   2. Represent each component as histogram (y-binning).
        //   3. Stack-based reduction of histogram to largest rectangles.
        // As these are blocks in a z-layer, only the top surface needs to be considered (2D problem).

        // Two special cases exist where we don't need to apply the above algorithm to simplify the layer:
        //   1. Single block - nothing else to merge with, so already simplified.
        //   2. Maximum blocks - only one way to merge (into a full slab covering the world segment).
        // We can skip the algorithm for these special cases as a shortcut.
        if (input.Count == 1)
        {
            // Single block only, no simplification possible for this layer.
            output.Add(input[0]);
            return;
        }

        if (input.Count == WorldManagementConfiguration.SegmentLength * WorldManagementConfiguration.SegmentLength)
        {
            // Maximum packing, replace with full slab.
            var (segmentStart, segmentEnd) = resolver.GetRangeForWorldSegment(segmentIndex);

            output.Add(new BoundingBox
            {
                Position = new Vector3(segmentStart.X, segmentStart.Y, 0),
                Size = new Vector3(WorldManagementConfiguration.SegmentLength,
                    -WorldManagementConfiguration.SegmentLength, -1.0f)
            });
        }
    }

    /// <summary>
    ///     Creates an empty array of layers.
    /// </summary>
    /// <returns>Empty array of layers.</returns>
    private static List<BoundingBox>[] MakeEmptyLayers()
    {
        var layers = new List<BoundingBox>[WorldManagementConfiguration.SegmentLength];
        for (var i = 0; i < WorldManagementConfiguration.SegmentLength; ++i) layers[i] = new List<BoundingBox>();

        return layers;
    }
}