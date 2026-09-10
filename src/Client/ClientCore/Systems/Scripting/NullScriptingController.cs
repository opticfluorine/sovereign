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
    public void ReloadAllScripts(IEventSender eventSender)
    {
    }

    public void ReloadScript(IEventSender eventSender, string scriptName)
    {
    }

    public void LoadNewScripts(IEventSender eventSender)
    {
    }

    public void ReloadEntity(IEventSender eventSender, ulong entityId)
    {
    }

    public void ReloadTemplate(IEventSender eventSender, ulong templateId)
    {
    }

    public void RunTests(IEventSender eventSender)
    {
    }

    public void CallFunctionAsync(string scriptName, string functionName, params ulong[] args)
    {
    }

    public void InvokeInteractCallback(IEventSender eventSender, ulong usingEntityId, ulong toolEntityId,
        ulong targetEntityId)
    {
    }
}
