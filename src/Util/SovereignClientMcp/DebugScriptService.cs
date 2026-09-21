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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SovereignClientMcp;

/// <summary>
///     Runs client debug scripts one at a time and produces the JSON tool result.
///     The MCP SDK may invoke the tool on any thread pool thread, so scripts are
///     serialized through a gate.
/// </summary>
public sealed class DebugScriptService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly SemaphoreSlim scriptGate = new(1, 1);
    private readonly DebugConnection connection;
    private readonly ClientMcpOptions options;

    /// <param name="connection">Connection to the client debug interface.</param>
    /// <param name="options">MCP server runtime options.</param>
    public DebugScriptService(DebugConnection connection, ClientMcpOptions options)
    {
        this.connection = connection;
        this.options = options;
    }

    /// <summary>
    ///     Runs a Lua script against the client debug interface and returns the JSON
    ///     result describing its output.
    /// </summary>
    /// <param name="script">Lua script source.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON result with the script's messages, screenshots, and error, if any.</returns>
    public async Task<string> RunScriptAsync(string script, CancellationToken cancellationToken)
    {
        await scriptGate.WaitAsync(cancellationToken);
        try
        {
            var messages = new List<string>();
            var screenshots = new List<string>();
            string? error = null;
            try
            {
                using var host = new DebugScriptHost(connection, messages, screenshots,
                    options.ScreenshotsDir);
                host.Run(script);
            }
            catch (Exception e) when (e is DebugScriptException or DebugClientException)
            {
                error = e.Message;
            }

            return JsonSerializer.Serialize(new RunClientScriptResult
            {
                Messages = messages,
                Screenshots = screenshots,
                Error = error
            }, JsonOptions);
        }
        finally
        {
            scriptGate.Release();
        }
    }

    /// <summary>
    ///     Disposes the shared connection to the client debug interface.
    /// </summary>
    public void Dispose()
    {
        scriptGate.Dispose();
        connection.Dispose();
    }
}
