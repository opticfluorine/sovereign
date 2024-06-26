﻿/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */


namespace Sovereign.EngineUtil.Numerics;

/// <summary>
///     Provides multiplicative conversion constants between units.
/// </summary>
public static class UnitConversions
{
    /// <summary>
    ///     Multiplicative constant from microseconds to seconds.
    /// </summary>
    public const float UsToS = 1E-6f;

    /// <summary>
    ///     Multiplicative constant from seconds to microseconds.
    /// </summary>
    public const float SToUs = 1.0f / UsToS;

    /// <summary>
    ///     Multiplicative constant from microseconds to milliseconds.
    /// </summary>
    public const float UsToMs = 1e-3f;

    /// <summary>
    ///     Multiplicative constant from milliseconds to microseconds.
    /// </summary>
    public const float MsToUs = 1.0f / UsToMs;
}