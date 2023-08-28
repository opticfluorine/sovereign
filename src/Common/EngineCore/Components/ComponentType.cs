/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using Sovereign.EngineCore.Systems.Player.Components;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Enumeration of all component types.
/// </summary>
public enum ComponentType
{
    #region Common

    /// <summary>
    ///     Position component.
    /// </summary>
    /// <seealso cref="Sovereign.EngineCore.Systems.Movement.Components.PositionComponentCollection" />
    Position = 0x0000,

    /// <summary>
    ///     Velocity component.
    /// </summary>
    /// <seealso cref="Sovereign.EngineCore.Systems.Movement.Components.VelocityComponentCollection" />
    Velocity = 0x0001,

    /// <summary>
    ///     Material component.
    /// </summary>
    /// <seealso cref="Sovereign.EngineCore.Systems.Block.Components.MaterialComponentCollection" />
    Material = 0x0002,

    /// <summary>
    ///     Material modifier component.
    /// </summary>
    /// <seealso cref="Sovereign.EngineCore.Systems.Block.Components.MaterialModifierComponentCollection" />
    MaterialModifier = 0x0003,

    /// <summary>
    ///     Above block component.
    /// </summary>
    /// <seealso cref="Sovereign.EngineCore.Systems.Block.Components.AboveBlockComponentCollection" />
    AboveBlock = 0x0004,

    /// <summary>
    ///     Tag indicating that an entity is a player character.
    /// </summary>
    /// <seealso cref="PlayerCharacterTagCollection" />
    PlayerCharacter = 0x0005,

    /// <summary>
    ///     Component giving an entity its name.
    /// </summary>
    /// <seealso cref="Sovereign.EngineCore.Components.NameComponentCollection" />
    Name = 0x0006,

    #endregion Common

    #region Client

    /// <summary>
    ///     Animated sprite component.
    /// </summary>
    /// <seealso cref="Sovereign.ClientCore.Rendering.Components.AnimatedSpriteComponentCollection" />
    AnimatedSprite = 0x1000,

    /// <summary>
    ///     Drawable component.
    /// </summary>
    /// <seealso cref="Sovereign.ClientCore.Rendering.Components.DrawableComponentCollection" />
    Drawable = 0x1001,

    #endregion Client

    #region Server

    /// <summary>
    ///     Account component.
    /// </summary>
    /// <seealso cref="Sovereign.ServerCore.Components.AccountComponentCollection" />
    Account = 0x2000,

    #endregion Server
}