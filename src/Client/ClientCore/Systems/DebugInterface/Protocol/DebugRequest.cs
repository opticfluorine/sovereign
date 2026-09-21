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

using MessagePack;

namespace Sovereign.ClientCore.Systems.DebugInterface.Protocol;

/// <summary>
///     Types of debug requests understood by the client debug interface.
/// </summary>
public enum DebugRequestType
{
    /// <summary>
    ///     Captures the next rendered frame and returns its pixels.
    /// </summary>
    Screenshot = 1,

    /// <summary>
    ///     Gets the current keyboard and mouse input state.
    /// </summary>
    GetInputState = 2,

    /// <summary>
    ///     Injects a keyboard event into the full input processing path.
    /// </summary>
    SendKeyEvent = 3,

    /// <summary>
    ///     Injects a mouse event into the full input processing path.
    /// </summary>
    SendMouseEvent = 4,

    /// <summary>
    ///     Quits the client.
    /// </summary>
    Exit = 5
}

/// <summary>
///     Interface implemented by debug request detail classes.
/// </summary>
[Union(0, typeof(KeyEventRequestDetails))]
[Union(1, typeof(MouseEventRequestDetails))]
public interface IDebugRequestDetails
{
}

/// <summary>
///     Details of a SendKeyEvent request.
/// </summary>
[MessagePackObject]
public class KeyEventRequestDetails : IDebugRequestDetails
{
    /// <summary>
    ///     SDL keycode of the key.
    /// </summary>
    [Key(0)]
    public int Keycode { get; set; }

    /// <summary>
    ///     SDL key modifier mask.
    /// </summary>
    [Key(1)]
    public ushort Modifier { get; set; }

    /// <summary>
    ///     true if the key is pressed down, false if released.
    /// </summary>
    [Key(2)]
    public bool IsDown { get; set; }
}

/// <summary>
///     Types of mouse events that can be injected by a SendMouseEvent request.
/// </summary>
public enum DebugMouseEventType
{
    /// <summary>
    ///     Mouse motion event.
    /// </summary>
    Motion = 1,

    /// <summary>
    ///     Mouse button event.
    /// </summary>
    Button = 2,

    /// <summary>
    ///     Mouse wheel event.
    /// </summary>
    Wheel = 3
}

/// <summary>
///     Details of a SendMouseEvent request. The relevant fields depend on EventType.
/// </summary>
[MessagePackObject]
public class MouseEventRequestDetails : IDebugRequestDetails
{
    /// <summary>
    ///     Type of mouse event to inject.
    /// </summary>
    [Key(0)]
    public DebugMouseEventType EventType { get; set; }

    /// <summary>
    ///     X position in pixels relative to the window. Used by Motion events.
    /// </summary>
    [Key(1)]
    public float X { get; set; }

    /// <summary>
    ///     Y position in pixels relative to the window. Used by Motion events.
    /// </summary>
    [Key(2)]
    public float Y { get; set; }

    /// <summary>
    ///     SDL mouse button number (1 = left, 2 = middle, 3 = right). Used by Button events.
    /// </summary>
    [Key(3)]
    public byte Button { get; set; }

    /// <summary>
    ///     true if the button is pressed down, false if released. Used by Button events.
    /// </summary>
    [Key(4)]
    public bool IsDown { get; set; }

    /// <summary>
    ///     Horizontal scroll delta. Used by Wheel events.
    /// </summary>
    [Key(5)]
    public float Dx { get; set; }

    /// <summary>
    ///     Vertical scroll delta. Used by Wheel events.
    /// </summary>
    [Key(6)]
    public float Dy { get; set; }
}

/// <summary>
///     Request sent from a debug client to the client debug interface.
/// </summary>
[MessagePackObject]
public class DebugRequest
{
    /// <summary>
    ///     Request type.
    /// </summary>
    [Key(0)]
    public DebugRequestType Type { get; set; }

    /// <summary>
    ///     Client-assigned request identifier echoed in the corresponding response.
    /// </summary>
    [Key(1)]
    public uint RequestId { get; set; }

    /// <summary>
    ///     Type-specific request details, or null if the request has no details.
    /// </summary>
    [Key(2)]
    public IDebugRequestDetails? Details { get; set; }
}
