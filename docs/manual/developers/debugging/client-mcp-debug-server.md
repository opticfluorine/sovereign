(client-mcp-debug-server)=
# Client MCP Debug Server

The `SovereignClientMcp` utility exposes a [Model Context Protocol](https://modelcontextprotocol.io)
(MCP) server over stdio that lets an AI coding agent drive a running Sovereign client for
debugging and UI automation. The agent writes a Lua script; the MCP server runs the script
and lets it interact with the client through the
[client debug interface](debug-interface.md), returning the script's messages, the paths of
any captured screenshots, and any error as a JSON object.

The MCP server is a development and testing tool. It connects to the client debug interface,
which is disabled by default; see the debug interface documentation for how to enable it.

## Running the Server

The server is launched by the coding agent as an MCP stdio server:

```bash
src/Util/SovereignClientMcp/bin/Debug/net10.0/SovereignClientMcp [--host <address>] [--port <port>] [--screenshots-dir <dir>]
```

| Option | Default | Description |
| --- | --- | --- |
| `--host` | `127.0.0.1` | IPv4 address of the client debug interface. |
| `--port` | `12821` | UDP port of the client debug interface. |
| `--screenshots-dir` | `screenshots` under the working directory | Directory in which captured screenshots are written. |

All logging is written to stderr; stdout carries only the MCP protocol.

## Example MCP Client Configuration

Most coding agents read an MCP client configuration, for example:

```json
{
  "mcpServers": {
    "sovereign-client": {
      "command": "/path/to/SovereignClientMcp",
      "args": ["--screenshots-dir", "/tmp/shots"]
    }
  }
}
```

## The run_client_script Tool

The server exposes a single tool, `run_client_script`, which takes a Lua script and returns
a JSON object:

```json
{
  "messages": ["..."],
  "screenshots": ["/abs/path/screenshot-1-120355.png"],
  "error": "only present if the script failed"
}
```

The script runs in a fresh Lua state with the standard libraries and the following global
functions:

| Function | Description |
| --- | --- |
| `OutputMessage(message)` | Records a message for the caller. |
| `Screenshot()` | Waits for the next rendered frame, saves it as a PNG, and returns its path. |
| `GetInputState()` | Returns `{PressedKeys={...}, MouseX, MouseY, LeftDown, MiddleDown, RightDown, TotalScrollAmount}`. |
| `SendKey(keycode, isDown, modifier)` | Injects a keyboard event (SDL keycode and keymod integers). |
| `SendMouseMotion(x, y)` | Injects a mouse motion event in window coordinates. |
| `SendMouseButton(button, isDown)` | Injects a mouse button event (1 = left, 2 = middle, 3 = right). |
| `SendMouseWheel(dx, dy)` | Injects a mouse wheel scroll event. |
| `ExitClient()` | Asks the client to exit. |
| `Sleep(seconds)` | Suspends the script; useful for pacing input sequences. |

The client-facing functions block until the client responds, up to a 30 second timeout. If
a call fails, the script aborts with an error and the failure is reported in the `error`
field of the result, including a Lua traceback.

## Example Agent Usage

Start the client with the debug interface enabled, then instruct the agent to run a script
such as:

```lua
Sleep(2.0)
local path = Screenshot()
local state = GetInputState()
SendMouseMotion(640, 360)
SendMouseButton(1, true)
Sleep(0.1)
SendMouseButton(1, false)
```

The agent inspects the saved PNG and the returned input state to reason about the state of
the client, and repeats as needed to debug an issue or exercise a UI flow.
