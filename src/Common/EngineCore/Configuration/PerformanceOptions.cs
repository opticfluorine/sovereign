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
///     User-configurable options for runtime behavior.
/// </summary>
public sealed class PerformanceOptions
{
    /// <summary>
    ///     Number of system executors.
    /// </summary>
    public int SystemExecutorCount { get; set; } = 1;

    /// <summary>
    ///     If true, yield the event loop thread after each iteration to limit CPU usage.
    /// </summary>
    public bool YieldEventLoop { get; set; } = false;

    /// <summary>
    ///     If true, yield the system executor threads after each iteration to limit CPU usage.
    /// </summary>
    public bool YieldSystemLoop { get; set; } = false;
}