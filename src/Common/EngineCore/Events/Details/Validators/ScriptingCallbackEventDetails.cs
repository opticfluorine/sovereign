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

using System;

namespace Sovereign.EngineCore.Events.Details.Validators;

/// <summary>
///     Event details for scripting callback events.
/// </summary>
public sealed class ScriptingCallbackEventDetails : IEventDetails
{
    /// <summary>
    ///     Lua state where callback function is hosted.
    /// </summary>
    public IntPtr LuaState { get; set; }

    /// <summary>
    ///     Lua reference index for the callback function.
    /// </summary>
    public int CallbackReference { get; set; }

    /// <summary>
    ///     Lua reference index for the argument.
    /// </summary>
    public int ArgumentReference { get; set; }
}