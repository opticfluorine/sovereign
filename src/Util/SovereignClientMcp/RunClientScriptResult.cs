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

using System.Text.Json.Serialization;

namespace SovereignClientMcp;

/// <summary>
///     Result payload returned by the run_client_script tool.
/// </summary>
public sealed record RunClientScriptResult
{
    /// <summary>
    ///     Messages that the script added via OutputMessage.
    /// </summary>
    [JsonPropertyName("messages")]
    public IList<string> Messages { get; init; } = [];

    /// <summary>
    ///     Absolute paths of the PNG screenshots captured by the script.
    /// </summary>
    [JsonPropertyName("screenshots")]
    public IList<string> Screenshots { get; init; } = [];

    /// <summary>
    ///     Error message if the script failed, or null if it completed successfully.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
