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

using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Scripting;

/// <summary>
///     Interface for interacting with the Scripting system.
/// </summary>
public interface IScriptingController
{
    /// <summary>
    ///     Requests that the Scripting system reloads all scripts.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    void ReloadAllScripts(IEventSender eventSender);

    /// <summary>
    ///     Requests that a specific script be reloaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="scriptName">Script name.</param>
    void ReloadScript(IEventSender eventSender, string scriptName);

    /// <summary>
    ///     Requests that any new scripts be loaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    void LoadNewScripts(IEventSender eventSender);

    /// <summary>
    ///     Requests that an entity be soft-reloaded by the Scripting system.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    void ReloadEntity(IEventSender eventSender, ulong entityId);

    /// <summary>
    ///     Requests that all loaded instances of a template be soft-reloaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="templateId">Template entity ID.</param>
    void ReloadTemplate(IEventSender eventSender, ulong templateId);

    /// <summary>
    ///     Requests that the Scripting system run the test suite.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    void RunTests(IEventSender eventSender);

    /// <summary>
    ///     Asynchronously invokes a function from a loaded script.
    /// </summary>
    /// <param name="scriptName">Script name.</param>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    void CallFunctionAsync(string scriptName, string functionName, params ulong[] args);

    /// <summary>
    ///     Requests that the interaction callback of the target entity be invoked with the given tool entity.
    ///     If the target has an interaction callback, it is invoked with the using entity ID, tool entity ID,
    ///     and target entity ID as arguments, in that order.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="actorEntityId">Entity ID of the entity using the tool.</param>
    /// <param name="toolEntityId">Tool entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    void InvokeInteractCallback(IEventSender eventSender, ulong actorEntityId, ulong toolEntityId,
        ulong targetEntityId);
}
