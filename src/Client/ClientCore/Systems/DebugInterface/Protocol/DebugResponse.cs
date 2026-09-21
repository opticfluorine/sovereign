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
using MessagePack;
using Sovereign.ClientCore.Rendering;

namespace Sovereign.ClientCore.Systems.DebugInterface.Protocol;

/// <summary>
///     Status of a debug response.
/// </summary>
public enum DebugResponseStatus
{
    /// <summary>
    ///     The request completed successfully.
    /// </summary>
    Ok = 0,

    /// <summary>
    ///     The request failed; ErrorMessage describes the failure.
    /// </summary>
    Error = 1
}

/// <summary>
///     Interface implemented by debug response detail classes.
/// </summary>
[Union(0, typeof(ScreenshotResponseDetails))]
[Union(1, typeof(InputStateResponseDetails))]
[Union(2, typeof(EmptyResponseDetails))]
public interface IDebugResponseDetails
{
}

/// <summary>
///     Details of a successful Screenshot response.
/// </summary>
[MessagePackObject]
public class ScreenshotResponseDetails : IDebugResponseDetails
{
    /// <summary>
    ///     Width of the captured frame in pixels.
    /// </summary>
    [Key(0)]
    public int Width { get; set; }

    /// <summary>
    ///     Height of the captured frame in pixels.
    /// </summary>
    [Key(1)]
    public int Height { get; set; }

    /// <summary>
    ///     Pixel format of the captured pixels.
    /// </summary>
    [Key(2)]
    public CapturedPixelFormat Format { get; set; }

    /// <summary>
    ///     Raw pixel data, ordered by row from the top of the frame, with no padding between rows.
    /// </summary>
    [Key(3)]
    public byte[] Pixels { get; set; } = Array.Empty<byte>();
}

/// <summary>
///     Details of a successful GetInputState response.
/// </summary>
[MessagePackObject]
public class InputStateResponseDetails : IDebugResponseDetails
{
    /// <summary>
    ///     SDL keycodes of the currently pressed keys.
    /// </summary>
    [Key(0)]
    public IList<int> PressedKeys { get; set; } = new List<int>();

    /// <summary>
    ///     Mouse x position in pixels relative to the window.
    /// </summary>
    [Key(1)]
    public float MouseX { get; set; }

    /// <summary>
    ///     Mouse y position in pixels relative to the window.
    /// </summary>
    [Key(2)]
    public float MouseY { get; set; }

    /// <summary>
    ///     true if the left mouse button is pressed down.
    /// </summary>
    [Key(3)]
    public bool LeftDown { get; set; }

    /// <summary>
    ///     true if the middle mouse button is pressed down.
    /// </summary>
    [Key(4)]
    public bool MiddleDown { get; set; }

    /// <summary>
    ///     true if the right mouse button is pressed down.
    /// </summary>
    [Key(5)]
    public bool RightDown { get; set; }

    /// <summary>
    ///     Cumulative mouse wheel scroll since engine startup (positive is scroll up).
    /// </summary>
    [Key(6)]
    public float TotalScrollAmount { get; set; }
}

/// <summary>
///     Details of an acknowledgement response with no payload.
/// </summary>
[MessagePackObject]
public class EmptyResponseDetails : IDebugResponseDetails
{
}

/// <summary>
///     Response sent from the client debug interface to a debug client.
/// </summary>
[MessagePackObject]
public class DebugResponse
{
    /// <summary>
    ///     Request identifier of the request being answered.
    /// </summary>
    [Key(0)]
    public uint RequestId { get; set; }

    /// <summary>
    ///     Response status.
    /// </summary>
    [Key(1)]
    public DebugResponseStatus Status { get; set; }

    /// <summary>
    ///     Human-readable error description if Status is Error, otherwise null.
    /// </summary>
    [Key(2)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Type-specific response details, or null if the response has no details.
    /// </summary>
    [Key(3)]
    public IDebugResponseDetails? Details { get; set; }
}
