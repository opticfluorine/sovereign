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

using Sovereign.EngineCore.Systems.Time;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.ServerCore.Systems.Time;

/// <summary>
///     Provides the "time" Lua module for reading the game clock.
/// </summary>
[ScriptableLibrary("time")]
public class TimeScripting(ITimeServices timeServices)
{
    [ScriptableFunction("GetAbsoluteTime")]
    public uint GetAbsoluteTime()
    {
        return timeServices.AbsoluteTime;
    }

    [ScriptableFunction("GetYear")]
    public uint GetYear()
    {
        return timeServices.Year;
    }

    [ScriptableFunction("GetSeason")]
    public Season GetSeason()
    {
        return timeServices.Season;
    }

    [ScriptableFunction("GetMonth")]
    public uint GetMonth()
    {
        return timeServices.Month;
    }

    [ScriptableFunction("GetMonthOfYear")]
    public uint GetMonthOfYear()
    {
        return timeServices.MonthOfYear;
    }

    [ScriptableFunction("GetWeekOfMonth")]
    public uint GetWeekOfMonth()
    {
        return timeServices.WeekOfMonth;
    }

    [ScriptableFunction("GetDayOfMonth")]
    public uint GetDayOfMonth()
    {
        return timeServices.DayOfMonth;
    }

    [ScriptableFunction("GetWeek")]
    public uint GetWeek()
    {
        return timeServices.Week;
    }

    [ScriptableFunction("GetDayOfWeek")]
    public uint GetDayOfWeek()
    {
        return timeServices.DayOfWeek;
    }

    [ScriptableFunction("GetDay")]
    public uint GetDay()
    {
        return timeServices.Day;
    }

    [ScriptableFunction("GetHourOfDay")]
    public uint GetHourOfDay()
    {
        return timeServices.HourOfDay;
    }

    [ScriptableFunction("GetSecondOfDay")]
    public uint GetSecondOfDay()
    {
        return timeServices.SecondOfDay;
    }
}