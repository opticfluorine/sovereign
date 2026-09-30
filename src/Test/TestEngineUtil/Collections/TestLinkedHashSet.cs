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

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Sovereign.EngineUtil.Collections;

/// <summary>
///     Unit tests for the LinkedHashSet class.
/// </summary>
public class TestLinkedHashSet
{
    [Fact]
    public void Add_EnumeratesInInsertionOrder()
    {
        var set = new LinkedHashSet<string>();
        set.Add("third");
        set.Add("first");
        set.Add("second");

        Assert.Equal(new[] { "third", "first", "second" }, set.ToArray());
    }

    [Fact]
    public void Add_Duplicate_IsIgnored()
    {
        var set = new LinkedHashSet<string>();
        Assert.True(set.Add("value"));
        Assert.False(set.Add("value"));

        Assert.Equal(1, set.Count);
        Assert.Equal(new[] { "value" }, set.ToArray());
    }

    [Fact]
    public void Add_Duplicate_DoesNotChangeOrder()
    {
        var set = new LinkedHashSet<string>();
        set.Add("first");
        set.Add("second");
        set.Add("first");

        Assert.Equal(2, set.Count);
        Assert.Equal(new[] { "first", "second" }, set.ToArray());
    }

    [Fact]
    public void CollectionInitializer_EnumeratesInInsertionOrder()
    {
        var set = new LinkedHashSet<int> { 3, 1, 2 };

        Assert.Equal(3, set.Count);
        Assert.Equal(new[] { 3, 1, 2 }, set.ToArray());
    }

    [Fact]
    public void CustomComparer_DeduplicatesEquivalentElements()
    {
        var set = new LinkedHashSet<string>(StringComparer.OrdinalIgnoreCase);
        set.Add("Value");
        set.Add("value");

        Assert.Equal(1, set.Count);
        Assert.True(set.Contains("VALUE"));
    }

    [Fact]
    public void Contains_ReportsMembership()
    {
        var set = new LinkedHashSet<string> { "present" };

        Assert.True(set.Contains("present"));
        Assert.False(set.Contains("absent"));
    }

    [Fact]
    public void EmptySet_EnumeratesNothing()
    {
        var set = new LinkedHashSet<int>();

        Assert.Equal(0, set.Count);
        Assert.Empty(set);
    }
}
