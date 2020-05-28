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
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering
{

    /// <summary>
    /// Single stage of the rendering process.
    /// </summary>
    public interface IRenderStage
    {

        /// <summary>
        /// Stage priorty. Stages with smaller priorities are executed earlier.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Initializes the render stage.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Executes the render stage.
        /// </summary>
        void Render();

        /// <summary>
        /// Cleans up the render stage.
        /// </summary>
        void Cleanup();

    }

}
