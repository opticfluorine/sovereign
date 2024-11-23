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
using System.Collections.Generic;
using System.Linq;

namespace Sovereign.ScriptingGenerators;

/// <summary>
///     Value-equatable wrapper around a generic List.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
internal readonly struct ValueEquatableList<T> : IEquatable<ValueEquatableList<T>>
{
    public List<T> List { get; }

    public ValueEquatableList(List<T> list)
    {
        List = list;
    }

    public bool Equals(ValueEquatableList<T> other)
    {
        return List.SequenceEqual(other.List);
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueEquatableList<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return List.GetHashCode();
    }

    public static bool operator ==(ValueEquatableList<T> left, ValueEquatableList<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueEquatableList<T> left, ValueEquatableList<T> right)
    {
        return !left.Equals(right);
    }
}