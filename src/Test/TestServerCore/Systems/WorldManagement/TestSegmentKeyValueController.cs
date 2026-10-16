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

using System.Linq;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.ServerCore.TestSupport;
using Xunit;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Unit tests for SegmentKeyValueController, the sole write door for segment key-value data.
/// </summary>
public class TestSegmentKeyValueController
{
    private readonly FakeDataServices dataServices = new();
    private readonly SegmentKeyValueManager manager = new();
    private readonly PendingSegmentKeyValueUpdates pending = new();
    private readonly SegmentKeyValueController controller;

    public TestSegmentKeyValueController()
    {
        controller = new SegmentKeyValueController(manager, pending, dataServices);
    }

    [Fact]
    public void SetValue_WritesCacheImmediatelyAndEnqueuesDelta()
    {
        var segment = new GridPosition(1, 2, 3);
        controller.SetValue(segment, "Owner", DataValue.FromEntityId(0x42));

        // Read-your-write: the value is visible without a persistence round trip.
        Assert.True(controller.IsSegmentLoaded(segment));
        Assert.True(controller.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x42UL, value.AsEntityId());

        var update = Assert.Single(pending.TakeAll());
        Assert.Equal(segment, update.Segment);
        Assert.Equal("Owner", update.Key);
        Assert.True(update.Value.HasValue);
        Assert.Equal(0x42UL, update.Value!.Value.AsEntityId());
    }

    [Fact]
    public void RemoveKey_RemovesFromCacheAndEnqueuesRemoval()
    {
        var segment = new GridPosition(4, 5, 6);
        controller.SetValue(segment, "Owner", DataValue.FromEntityId(0x1));
        pending.TakeAll();

        controller.RemoveKey(segment, "Owner");

        Assert.False(controller.TryGetValue(segment, "Owner", out _));

        var update = Assert.Single(pending.TakeAll());
        Assert.Equal("Owner", update.Key);
        Assert.False(update.Value.HasValue);
    }

    [Fact]
    public void SetValue_ReadOnlyKey_RejectedWithoutCacheMutationOrDelta()
    {
        var segment = new GridPosition(7, 8, 9);

        Assert.False(controller.SetValue(segment, "__reserved", DataValue.FromInt(5)));

        Assert.False(manager.IsLoaded(segment));
        Assert.False(controller.TryGetValue(segment, "__reserved", out _));
        Assert.Empty(pending.TakeAll());
    }

    [Fact]
    public void RemoveKey_ReadOnlyKey_RejectedWithoutCacheMutationOrDelta()
    {
        var segment = new GridPosition(10, 11, 12);

        Assert.False(controller.RemoveKey(segment, "__reserved"));

        Assert.False(manager.IsLoaded(segment));
        Assert.Empty(pending.TakeAll());
    }

    [Fact]
    public void SetValue_MultipleSegments_QueuedIndependently()
    {
        var a = new GridPosition(1, 0, 0);
        var b = new GridPosition(2, 0, 0);

        controller.SetValue(a, "Owner", DataValue.FromEntityId(0xA));
        controller.SetValue(b, "Owner", DataValue.FromEntityId(0xB));

        var updates = pending.TakeAll();
        Assert.Equal(2, updates.Count);
        Assert.Contains(updates, u => u.Segment == a);
        Assert.Contains(updates, u => u.Segment == b);
    }
}
