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

using System.Numerics;
using MessagePack;
using MessagePack.Formatters;

namespace Sovereign.EngineCore.Network.MsgPack;

/// <summary>
///     MsgPack formatter for Vector2.
/// </summary>
public class Vector2Formatter : IMessagePackFormatter<Vector2>
{
    public static readonly Vector2Formatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
    {
        writer.Write(value.X);
        writer.Write(value.Y);
    }

    public Vector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        options.Security.DepthStep(ref reader);
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        reader.Depth--;
        return new Vector2(x, y);
    }
}