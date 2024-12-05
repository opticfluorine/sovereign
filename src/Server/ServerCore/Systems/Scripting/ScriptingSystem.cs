// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;

namespace Sovereign.ServerCore.Systems.Scripting;

/// <summary>
///     System responsible for managing server-side scripts.
/// </summary>
public class ScriptingSystem : ISystem
{
    private readonly ScriptLoader scriptLoader;

    public ScriptingSystem(EventCommunicator eventCommunicator, IEventLoop eventLoop, ScriptLoader scriptLoader)
    {
        this.scriptLoader = scriptLoader;
        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }
    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>();
    public int WorkloadEstimate => 1000;

    public void Initialize()
    {
        scriptLoader.LoadAll();
    }

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        return 0;
    }
}