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
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sovereign.EngineCore.Components;

namespace Sovereign.EngineCore.Systems.Movement.Components
{

    /// <summary>
    /// Manages the positions of all positionable entities.
    /// </summary>
    public sealed class PositionComponentCollection : BaseComponentCollection<Vector3>
    {

        /// <summary>
        /// Initial number of allocated components.
        /// </summary>
        private const int BaseSize = 65536;

        public PositionComponentCollection(ComponentManager componentManager) 
            : base(componentManager, BaseSize, ComponentOperators.VectorOperators,
                  ComponentType.Position)
        {
        }

    }

}
