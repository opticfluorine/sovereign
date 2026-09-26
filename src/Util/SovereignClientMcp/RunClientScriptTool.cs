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

using System.ComponentModel;
using ModelContextProtocol.Server;

namespace SovereignClientMcp;

/// <summary>
///     MCP tool that runs a Lua script against the client debug interface.
/// </summary>
[McpServerToolType]
public static class RunClientScriptTool
{
    private const string ToolName = "run_client_script";

    private const string ToolDescription = """
        Run a Lua script that drives the Sovereign client debug interface, then return a
        JSON object with the keys "messages" (strings the script recorded via
        OutputMessage), "screenshots" (absolute paths of PNG files captured via
        Screenshot), and "error" (present only if the script failed; messages and
        screenshots collected before a failure are still returned).

        The script runs in a fresh Lua 5.1 (LuaJIT) state with the standard libraries and
        the following global functions:

        - OutputMessage(message): Record a message for the caller. Strings and numbers
          are accepted.
        - Screenshot(): Wait for the client to render the next frame and save it as a
          PNG file. Returns the absolute path of the saved file.
        - GetInputState(): Return the client's current input state as a table
          {PressedKeys={...}, MouseX, MouseY, LeftDown, MiddleDown, RightDown,
          TotalScrollAmount}. PressedKeys is an array of SDL keycodes.
        - SendKey(keycode, isDown, modifier): Inject a keyboard event. keycode and
          modifier are SDL keycode and keymod integers; modifier defaults to 0.
        - SendMouseMotion(x, y): Inject a mouse motion event in window coordinates.
        - SendMouseButton(button, isDown): Inject a mouse button event (1 = left,
          2 = middle, 3 = right).
        - SendMouseWheel(dx, dy): Inject a mouse wheel scroll event.
        - ExitClient(): Ask the client to exit.
        - Sleep(seconds): Suspend the script for the given number of seconds; useful
          for pacing input sequences and waiting for frames.

        All functions that communicate with the client (Screenshot, GetInputState,
        SendKey, SendMouse*, ExitClient) block until the client responds, up to a
        30 second timeout. If any call fails, the script aborts with an error that is
        reported in the "error" field, including a Lua traceback.
        """;

    /// <summary>
    ///     Runs a client debug Lua script.
    /// </summary>
    /// <param name="script">Lua script source to run.</param>
    /// <param name="service">Client debug script service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON string describing the script results.</returns>
    [McpServerTool(Name = ToolName), Description(ToolDescription)]
    public static async Task<string> RunClientScript(string script,
        DebugScriptService service, CancellationToken cancellationToken)
    {
        return await service.RunScriptAsync(script, cancellationToken);
    }
}
