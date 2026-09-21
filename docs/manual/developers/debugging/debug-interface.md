# Client Debug Interface

The client debug interface is a small MessagePack-over-UDP IPC server built into the
Sovereign client. It allows external tools (test harnesses, bots, automated smoke tests)
to interact with a running client: capturing screenshots, reading input state, injecting
keyboard and mouse events, and shutting the client down cleanly.

The interface is **disabled by default** and is intended for development and testing
only. When enabled, it binds to the loopback interface by default so that only local
processes can connect.

## Enabling the Debug Interface

The interface is controlled by the `Sovereign:DebugInterfaceOptions` configuration
section. See [Client Configuration Options](../../creators/config/index.md) for the full
list of options. To enable it via a command line option, start the client with:

```
--Sovereign:DebugInterfaceOptions:Enabled=true
```

Once enabled, the server listens for LiteNetLib UDP connections on `Host:Port`
(default `127.0.0.1:12821`). All connection requests are accepted; the loopback binding
is the security boundary for the interface.

## Wire Protocol

Messages are MessagePack-encoded and exchanged over LiteNetLib, one message per
request or response, using the reliable ordered delivery method. Serialization follows
the same hardened (untrusted-data) MessagePack options used by the rest of the engine.

### Requests

Each `DebugRequest` carries a request type, a client-assigned `RequestId` (a `uint`
echoed in the response), and optional type-specific details:

| Type | Value | Details | Effect |
| --- | --- | --- | --- |
| `Screenshot` | 1 | none | Captures the next rendered frame and returns its pixels. |
| `GetInputState` | 2 | none | Returns the current keyboard and mouse input state. |
| `SendKeyEvent` | 3 | `KeyEventRequestDetails` | Injects a keyboard event. |
| `SendMouseEvent` | 4 | `MouseEventRequestDetails` | Injects a mouse event. |
| `Exit` | 5 | none | Quits the client cleanly (equivalent to closing the window). |

`KeyEventRequestDetails` carries an SDL keycode (`Keycode`), an SDL key modifier mask
(`Modifier`), and an `IsDown` flag.

`MouseEventRequestDetails` carries an `EventType` discriminator (`Motion`, `Button`, or
`Wheel`) along with the fields relevant to that event type:

- `Motion`: mouse position `X` and `Y` in pixels relative to the window.
- `Button`: SDL mouse button number `Button` (1 = left, 2 = middle, 3 = right) and an
  `IsDown` flag.
- `Wheel`: scroll deltas `Dx` and `Dy`.

### Responses

Each `DebugResponse` echoes the request's `RequestId` and carries a `Status`
(`Ok` or `Error`), an optional `ErrorMessage`, and optional type-specific details:

| Details type | Returned by | Payload |
| --- | --- | --- |
| `ScreenshotResponseDetails` | `Screenshot` | `Width`, `Height`, `Format` (`Bgra8`, `Rgba8`, `Bgra8Srgb`, or `Rgba8Srgb`), and raw `Pixels` with no padding between rows. |
| `InputStateResponseDetails` | `GetInputState` | `PressedKeys` (SDL keycodes), `MouseX`/`MouseY`, per-button down flags, and `TotalScrollAmount`. |
| `EmptyResponseDetails` | `SendKeyEvent`, `SendMouseEvent`, `Exit` | No payload (acknowledgement). |

:::{note}
The pixel format of a screenshot is reported in the response because it depends on
the renderer configuration; convert accordingly (e.g. `Bgra8Srgb` when the client runs
with sRGB enabled).
:::

## Event Injection Semantics

Injected key and mouse events pass through the client's full input processing path:
they are first offered to Dear ImGui, and any event that the GUI consumes never reaches
the engine's input system. This mirrors the behavior of real OS input events. Keep this
in mind when writing harnesses that interact with GUI-focused screens: clicks on widgets
that Dear ImGui handles will not also reach gameplay systems.

## Timing Notes

- A `Screenshot` request is fulfilled with the first frame rendered *after* the request
  is processed. Harnesses should wait for the response with a generous timeout,
  especially if the client may be starting up.
- Large screenshots (e.g. 1080p BGRA is about 8 MB) are delivered using LiteNetLib's
  automatic fragmentation of reliable ordered messages; clients must be prepared to
  reassemble them.
