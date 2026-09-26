// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using Sovereign.ClientCore.Configuration;

namespace SovereignClientMcp;

/// <summary>
///     Runtime options for the client debug MCP server.
/// </summary>
/// <param name="Host">IPv4 address of the client debug interface.</param>
/// <param name="Port">UDP port of the client debug interface.</param>
/// <param name="ScreenshotsDir">Directory in which captured screenshots are written.</param>
public sealed record ClientMcpOptions(string Host, ushort Port, string ScreenshotsDir)
{
    /// <summary>
    ///     Default options derived from the client debug interface defaults.
    /// </summary>
    public static ClientMcpOptions Default { get; } = new(
        DebugInterfaceOptions.DefaultHost,
        DebugInterfaceOptions.DefaultPort,
        Path.Combine(Environment.CurrentDirectory, "screenshots"));
}
