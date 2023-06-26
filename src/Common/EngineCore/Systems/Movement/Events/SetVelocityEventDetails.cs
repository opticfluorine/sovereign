/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using MessagePack;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Movement.Events;

/// <summary>
///     Sets the velocity of an entity. The entity will move under this
///     velocity until the velocity is changed, movement is ended, or the
///     entity cannot proceed to move.
/// </summary>
[MessagePackObject]
public class SetVelocityEventDetails : IEventDetails
{
    /// <summary>
    ///     Entity identifier.
    /// </summary>
    [Key(0)]
    public ulong EntityId { get; set; }

    /// <summary>
    ///     Relative rate of movement along X as a ratio of the entity's base speed.
    /// </summary>
    [Key(1)]
    public float RateX { get; set; }

    /// <summary>
    ///     Relative rate of movement along Y as a ratio of the entity's base speed.
    /// </summary>
    [Key(2)]
    public float RateY { get; set; }
}