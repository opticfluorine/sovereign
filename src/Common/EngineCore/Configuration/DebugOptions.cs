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
///     User-configurable debug settings.
/// </summary>
public sealed class DebugOptions
{
    /// <summary>
    ///     Flag indicating whether event logging is enabled.
    /// </summary>
    public bool EnableEventLogging { get; set; } = false;

    /// <summary>
    ///     If event logging is enabled, specifies the directory where
    ///     the event log files are stored.
    /// </summary>
    public string EventLogDirectory { get; set; } = "Logs";
}