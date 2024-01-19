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

using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Movement;

public class MovementEventHandler
{
    private readonly MovementManager manager;

    public MovementEventHandler(MovementManager manager)
    {
        this.manager = manager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public void HandleEvent(Event ev)
    {
        switch (ev.EventId)
        {
            case EventId.Core_Movement_Move:
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received Move event with no details.");
                    break;
                }

                OnMove((MoveEventDetails)ev.EventDetails);
                break;

            case EventId.Core_Movement_RequestMove:
                if (ev.EventDetails == null)
                {
                    Logger.Error("Received RequestMove event with no details.");
                    break;
                }

                OnRequestMove((MoveEventDetails)ev.EventDetails);
                break;

            case EventId.Core_Tick:
                OnTick();
                break;
        }
    }

    private void OnRequestMove(MoveEventDetails eventDetails)
    {
    }

    private void OnMove(MoveEventDetails eventDetails)
    {
    }

    private void OnTick()
    {
    }
}