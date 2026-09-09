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
using Sovereign.EngineCore.Systems.Scripting;

namespace Sovereign.ClientCore.Systems.Scripting;

/// <summary>
///     No-op implementation of IScriptingController for the client.
/// </summary>
public sealed class NullScriptingController : IScriptingController
{
    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void ReloadAllScripts(IEventSender eventSender)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="scriptName">Script name.</param>
    public void ReloadScript(IEventSender eventSender, string scriptName)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void LoadNewScripts(IEventSender eventSender)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    public void ReloadEntity(IEventSender eventSender, ulong entityId)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="templateId">Template entity ID.</param>
    public void ReloadTemplate(IEventSender eventSender, ulong templateId)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void RunTests(IEventSender eventSender)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="scriptName">Script name.</param>
    /// <param name="functionName">Function name.</param>
    /// <param name="args">Arguments.</param>
    public void CallFunctionAsync(string scriptName, string functionName, params ulong[] args)
    {
    }

    /// <summary>
    ///     Does nothing.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="toolEntityId">Tool entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    public void InvokeInteractCallback(IEventSender eventSender, ulong toolEntityId, ulong targetEntityId)
    {
    }
}
