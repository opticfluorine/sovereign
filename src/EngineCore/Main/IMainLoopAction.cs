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

namespace Sovereign.EngineCore.Main
{

    /// <summary>
    /// Implemented by classes that provide actions to be executed
    /// periodically after a fixed number of main loop cycles.
    /// </summary>
    public interface IMainLoopAction
    {

        /// <summary>
        /// The number of main loop cycles after which the action is performed.
        /// </summary>
        ulong CycleInterval { get; }

        /// <summary>
        /// Executes the action. This will be called on the main thread.
        /// </summary>
        void Execute();

    }

}
