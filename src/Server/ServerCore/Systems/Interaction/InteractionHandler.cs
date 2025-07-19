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

using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.ServerCore.Systems.Scripting;

namespace Sovereign.ServerCore.Systems.Interaction;

/// <summary>
///     Responsible for server-side handling of interactions between entities.
/// </summary>
internal sealed class InteractionHandler(
    InteractionValidator validator,
    IDataServices dataServices,
    ScriptingController scriptingController)
{
    /// <summary>
    ///     Handles a requested interaction.
    /// </summary>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    /// <param name="toolEntityId">Tool entity ID (set to sourceEntityId to use self).</param>
    public void HandleInteraction(ulong sourceEntityId, ulong targetEntityId, ulong toolEntityId)
    {
        if (!validator.IsInteractionAllowed(sourceEntityId, targetEntityId, toolEntityId)) return;

        // Call any interaction script attached to the target entity.
        if (!dataServices.TryGetEntityKeyValue(targetEntityId, EntityConstants.InteractScriptKey, out var scriptName) ||
            !dataServices.TryGetEntityKeyValue(targetEntityId, EntityConstants.InteractFunctionKey,
                out var funcName)) return;

        scriptingController.CallFunctionAsync(scriptName, funcName, sourceEntityId, targetEntityId, toolEntityId);
    }
}