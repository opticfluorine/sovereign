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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Systems.Data;
using Xunit;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Unit tests for SegmentKeyValueManager.
/// </summary>
public class TestSegmentKeyValueManager
{
    private readonly SegmentKeyValueManager manager = new();

    [Fact]
    public void IsLoaded_UnknownSegment_IsFalse()
    {
        Assert.False(manager.IsLoaded(new GridPosition(1, 2, 3)));
    }

    [Fact]
    public void SetValue_CreatesEntryLazilyAndReadsBack()
    {
        var segment = new GridPosition(4, 5, 6);
        manager.SetValue(segment, "Owner", DataValue.FromEntityId(0x10));

        Assert.True(manager.IsLoaded(segment));
        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x10UL, value.AsEntityId());
    }

    [Fact]
    public void SetValue_OverwritesExistingValue()
    {
        var segment = new GridPosition(7, 8, 9);
        manager.SetValue(segment, "Owner", DataValue.FromEntityId(0x1));
        manager.SetValue(segment, "Owner", DataValue.FromEntityId(0x2));

        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x2UL, value.AsEntityId());
    }

    [Fact]
    public void RemoveKey_RemovesValueButKeepsSegmentLoaded()
    {
        var segment = new GridPosition(10, 11, 12);
        manager.SetValue(segment, "Owner", DataValue.FromEntityId(0x1));
        manager.RemoveKey(segment, "Owner");

        Assert.False(manager.TryGetValue(segment, "Owner", out _));
        Assert.True(manager.IsLoaded(segment));
    }

    [Fact]
    public void LoadSegment_ReplacesCachedPairs()
    {
        var segment = new GridPosition(13, 14, 15);
        manager.SetValue(segment, "Stale", DataValue.FromString("stale"));

        manager.LoadSegment(segment, new Dictionary<string, DataValue>
        {
            ["Owner"] = DataValue.FromEntityId(0x99)
        });

        Assert.False(manager.TryGetValue(segment, "Stale", out _));
        Assert.True(manager.TryGetValue(segment, "Owner", out var value));
        Assert.Equal(0x99UL, value.AsEntityId());
        Assert.True(manager.IsLoaded(segment));
    }

    [Fact]
    public void LoadSegment_EmptyPairs_MarksLoadedWithNoValues()
    {
        var segment = new GridPosition(16, 17, 18);
        manager.LoadSegment(segment, new Dictionary<string, DataValue>());

        Assert.True(manager.IsLoaded(segment));
        Assert.False(manager.TryGetValue(segment, "Owner", out _));
    }

    [Fact]
    public void DropSegment_ClearsValuesAndLoadedState()
    {
        var segment = new GridPosition(19, 20, 21);
        manager.SetValue(segment, "Owner", DataValue.FromEntityId(0x1));
        manager.DropSegment(segment);

        Assert.False(manager.IsLoaded(segment));
        Assert.False(manager.TryGetValue(segment, "Owner", out _));
    }

    [Fact]
    public void GetAllKeyValuePairs_CopiesEntries()
    {
        var segment = new GridPosition(22, 23, 24);
        manager.SetValue(segment, "A", DataValue.FromInt(1));
        manager.SetValue(segment, "B", DataValue.FromInt(2));

        var pairs = new Dictionary<string, DataValue>();
        manager.GetAllKeyValuePairs(segment, pairs);

        Assert.Equal(2, pairs.Count);
        Assert.Equal(1, pairs["A"].AsInt());
        Assert.Equal(2, pairs["B"].AsInt());
    }
}
