/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Enumeration of all component types.
    /// </summary>
    public enum ComponentType
    {

        #region Common

        /// <summary>
        /// Position component.
        /// </summary>
        /// <seealso cref="PositionComponentCollection"/>
        Position = 0x0000,

        /// <summary>
        /// Velocity component.
        /// </summary>
        /// <seealso cref="VelocityComponentCollection"/>
        Velocity = 0x0001,

        /// <summary>
        /// Material component.
        /// </summary>
        /// <seealso cref="MaterialComponentCollection"/>
        Material = 0x0002,

        /// <summary>
        /// Material modifier component.
        /// </summary>
        /// <seealso cref="MaterialModifierComponentCollection"/>
        MaterialModifier = 0x0003,

        /// <summary>
        /// Above block component.
        /// </summary>
        /// <seealso cref="AboveBlockComponentCollection"/>
        AboveBlock = 0x0004,

        #endregion Common

        #region Client

        /// <summary>
        /// Animated sprite component.
        /// </summary>
        /// <seealso cref="AnimatedSpriteComponentCollection"/>
        AnimatedSprite = 0x1000,

        /// <summary>
        /// Drawable component.
        /// </summary>
        /// <seealso cref="DrawableComponentCollection"/>
        Drawable = 0x1001,

        #endregion Client

        #region Server

        #endregion Server

    }

}
