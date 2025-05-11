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

namespace Sovereign.EngineCore.Configuration;

/// <summary>
///     User-configurable options for the in-game clock.
/// </summary>
public sealed class TimeOptions
{
    /// <summary>
    ///     Number of real-world seconds per in-game day.
    /// </summary>
    public uint SecondsPerHour { get; set; } = 1800;

    public uint HoursPerDay { get; set; } = 24;

    /// <summary>
    ///     Number of in-game days per in-game week.
    /// </summary>
    public uint DaysPerWeek { get; set; } = 7;

    /// <summary>
    ///     Number of in-game weeks per in-game month.
    /// </summary>
    public uint WeeksPerMonth { get; set; } = 4;

    /// <summary>
    ///     Number of in-game months per in-game season (quarter of in-game year).
    /// </summary>
    public uint MonthsPerSeason { get; set; } = 1;

    /// <summary>
    ///     Number of seconds between successive synchronization events.
    /// </summary>
    public uint SyncIntervalSeconds { get; set; } = 3;
}