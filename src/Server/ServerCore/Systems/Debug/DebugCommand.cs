// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Text.Json.Serialization;

namespace Sovereign.ServerCore.Systems.Debug;

public enum DebugCommandType
{
    /// <summary>
    /// Unknown command type.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Debug command to generate test world data.
    /// </summary>
    GenerateWorldData,
}

/// <summary>
/// JSON-serializable debug command structure.
/// </summary>
public sealed class DebugCommand
{
    
    /// <summary>
    /// Debug command type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DebugCommandType Type { get; set; }

    /// <summary>
    /// Checks whether the record is valid.
    /// </summary>
    public bool IsValid => Type != DebugCommandType.Unknown;
}
