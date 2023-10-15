// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for controlling auto-ping behavior.
/// </summary>
public class AutoPingEventDetails : IEventDetails
{
    /// <summary>
    ///     Creates auto ping event details.
    /// </summary>
    /// <param name="enable">Whether to enable auto ping.</param>
    /// <param name="intervalMs">Interval in milliseconds between pings.</param>
    public AutoPingEventDetails(bool enable, uint intervalMs)
    {
        Enable = enable;
        IntervalMs = intervalMs;
    }

    /// <summary>
    ///     Whether to enable (true) or disable (false) the auto-ping.
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    ///     Time in milliseconds between each automatic ping.
    /// </summary>
    public uint IntervalMs { get; set; }
}