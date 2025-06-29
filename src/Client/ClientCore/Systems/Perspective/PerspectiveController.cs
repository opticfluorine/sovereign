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

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Controller for the Perspective system.
/// </summary>
public interface IPerspectiveController
{
    /// <summary>
    ///     Blocking call that prepares the Perspective system for the next frame.
    /// </summary>
    /// <param name="timeSinceTick">Time since the last tick in seconds.</param>
    void BeginFrameSync(float timeSinceTick);
}

/// <summary>
///     Implementation of the IPerspectiveController interface.
/// </summary>
internal class PerspectiveController(OverheadTransparency overheadTransparency) : IPerspectiveController
{
    public void BeginFrameSync(float timeSinceTick)
    {
        overheadTransparency.BeginFrame(timeSinceTick);
    }
}