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

using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Configuration;

namespace Sovereign.EngineCore.Systems.Time;

/// <summary>
///     Public read API for TimeSystem providing access to the in-game clock.
/// </summary>
public interface ITimeServices
{
    /// <summary>
    ///     Gets the absolute in-game time in seconds.
    /// </summary>
    uint AbsoluteTime { get; }

    /// <summary>
    ///     Current in-game year.
    /// </summary>
    uint Year { get; }

    /// <summary>
    ///     Current in-game season.
    /// </summary>
    Season Season { get; }

    /// <summary>
    ///     Current absolute in-game month.
    /// </summary>
    uint Month { get; }

    /// <summary>
    ///     Current in-game month of year (0..4*MonthsPerSeason).
    /// </summary>
    uint MonthOfYear { get; }

    /// <summary>
    ///     Current in-game week of month (0..WeeksPerMonth).
    /// </summary>
    uint WeekOfMonth { get; }

    /// <summary>
    ///     Current in-game day of month (0..DaysPerWeek*WeeksPerMonth).
    /// </summary>
    uint DayOfMonth { get; }

    /// <summary>
    ///     Current absolute in-game week.
    /// </summary>
    uint Week { get; }

    /// <summary>
    ///     Current in-game day of week (0..DaysPerWeek).
    /// </summary>
    uint DayOfWeek { get; }

    /// <summary>
    ///     Current absolute in-game day.
    /// </summary>
    uint Day { get; }

    /// <summary>
    ///     Gets the current in-game hour (0..23).
    /// </summary>
    uint HourOfDay { get; }

    /// <summary>
    ///     Gets the current in-game second of day (0..SecondsPerDay).
    /// </summary>
    uint SecondOfDay { get; }
}

/// <summary>
///     Implementation of ITimeServices.
/// </summary>
internal sealed class TimeServices(IGameClock clock, IOptions<TimeOptions> options) : ITimeServices
{
    public uint AbsoluteTime => clock.Time;
    public uint Year => Month / (options.Value.MonthsPerSeason * TimeConstants.SeasonsPerYear);
    public Season Season => (Season)(Month / options.Value.MonthsPerSeason % TimeConstants.SeasonsPerYear);
    public uint MonthOfYear => Month % (options.Value.MonthsPerSeason * TimeConstants.SeasonsPerYear);
    public uint Month => Week / options.Value.WeeksPerMonth;
    public uint WeekOfMonth => Week % options.Value.WeeksPerMonth;
    public uint DayOfMonth => Day % (options.Value.DaysPerWeek * options.Value.WeeksPerMonth);
    public uint Week => Day / options.Value.DaysPerWeek;
    public uint DayOfWeek => Day % options.Value.DaysPerWeek;
    public uint Day => clock.Time / (options.Value.SecondsPerHour * options.Value.HoursPerDay);
    public uint HourOfDay => SecondOfDay / options.Value.SecondsPerHour;
    public uint SecondOfDay => clock.Time % (options.Value.HoursPerDay * options.Value.SecondsPerHour);
}