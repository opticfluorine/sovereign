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

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Manages periodic notifications of movement from server to client.
/// </summary>
public interface IMovementNotifier
{
    /// <summary>
    ///     Schedules the given entity for a motion update in the next batch.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="immediate">If true, send as soon as possible.</param>
    void ScheduleEntity(ulong entityId, bool immediate = false);

    /// <summary>
    ///     Sends any notifications that are due to be sent.
    /// </summary>
    void SendScheduled();
}