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

using System.Threading;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems.Combat;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.ServerCore.Systems.Combat;

/// <summary>
///     Provides the "Combat" Lua module for performing attacks.
/// </summary>
[ScriptableLibrary("Combat")]
public class CombatScripting(CombatController combatController, IEventSender eventSender)
{
    private readonly Lock eventLock = new();

    /// <summary>
    ///     Scripting API for requesting an attack by the given actor.
    /// </summary>
    /// <param name="actorEntityId">Entity ID of the attacking actor.</param>
    [ScriptableFunction("Attack")]
    public void Attack(ulong actorEntityId)
    {
        lock (eventLock)
        {
            combatController.Attack(eventSender, actorEntityId);
        }
    }
}
