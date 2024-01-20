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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.Input;

/// <summary>
///     Internal controller for input system events.
/// </summary>
public class InputInternalController
{
    /// <summary>
    ///     Schedules a repeat movement event to the Input system.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="sequenceCount">Keypress sequence number.</param>
    /// <param name="nextTime">System time to handle next event.</param>
    public void ScheduleRepeatMove(IEventSender sender, uint sequenceCount, ulong nextTime)
    {
        var details = new SequenceEventDetails
        {
            SequenceCount = sequenceCount
        };
        var ev = new Event(EventId.Client_Input_RepeatMove, details, nextTime);
        sender.SendEvent(ev);
    }
}