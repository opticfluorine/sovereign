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

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Internal lookup key type for cached neighbor sets.
/// </summary>
public readonly struct TileContextKey : IEquatable<TileContextKey>
{
    public readonly int NorthId;
    public readonly int NortheastId;
    public readonly int EastId;
    public readonly int SoutheastId;
    public readonly int SouthId;
    public readonly int SouthwestId;
    public readonly int WestId;
    public readonly int NorthwestId;

    /// <summary>
    ///     Context key containing all wildcards.
    /// </summary>
    public static readonly TileContextKey AllWildcards = new(TileSprite.Wildcard, TileSprite.Wildcard,
        TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard, TileSprite.Wildcard,
        TileSprite.Wildcard);

    public bool Equals(TileContextKey other)
    {
        return NorthId == other.NorthId && NortheastId == other.NortheastId && EastId == other.EastId &&
               SoutheastId == other.SoutheastId && SouthId == other.SouthId && SouthwestId == other.SouthwestId &&
               WestId == other.WestId && NorthwestId == other.NorthwestId;
    }

    public override bool Equals(object? obj)
    {
        return obj is TileContextKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NorthId, NortheastId, EastId, SoutheastId, SouthId, SouthwestId, WestId,
            NorthwestId);
    }

    public static bool operator ==(TileContextKey left, TileContextKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TileContextKey left, TileContextKey right)
    {
        return !left.Equals(right);
    }

    public TileContextKey(int northId, int northeastId, int eastId, int southeastId, int southId, int southwestId,
        int westId, int northwestId)
    {
        NorthId = northId;
        NortheastId = northeastId;
        EastId = eastId;
        SoutheastId = southeastId;
        SouthId = southId;
        SouthwestId = southwestId;
        WestId = westId;
        NorthwestId = northwestId;
    }
}