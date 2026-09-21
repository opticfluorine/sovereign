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

using System.Runtime.InteropServices;
using Sovereign.ClientCore.Systems.DebugInterface.Protocol;
using static Sovereign.Scripting.Lua.LuaBindings;

namespace SovereignClientMcp;

/// <summary>
///     Hosts the execution of one client debug Lua script in a fresh Lua state.
///     The script drives the client debug interface through a set of global functions
///     and records its results into the message and screenshot collections.
/// </summary>
public sealed class DebugScriptHost : IDisposable
{
    /// <summary>Registry key under which the native binding table is stored.</summary>
    private const string NativesRegistryKey = "sovereign_client_natives";

    /// <summary>Number of native binding functions registered.</summary>
    private const int NativeBindingCount = 9;

    /// <summary>
    ///     Lua bootstrap source that defines the global script functions. The native
    ///     bindings report failures by returning nil followed by an error message;
    ///     the wrappers convert those into real Lua errors, which unwind safely to the
    ///     enclosing lua_pcall rather than longjmping across a managed frame.
    /// </summary>
    private const string BootstrapSource = """
        local C = debug.getregistry()["sovereign_client_natives"]

        function OutputMessage(message)
            C.OutputMessage(message)
        end

        function Screenshot()
            local path, err = C.Screenshot()
            if err ~= nil then error(err, 2) end
            return path
        end

        function GetInputState()
            local state, err = C.GetInputState()
            if err ~= nil then error(err, 2) end
            return state
        end

        function SendKey(keycode, isDown, modifier)
            local _, err = C.SendKey(keycode, isDown, modifier)
            if err ~= nil then error(err, 2) end
        end

        function SendMouseMotion(x, y)
            local _, err = C.SendMouseMotion(x, y)
            if err ~= nil then error(err, 2) end
        end

        function SendMouseButton(button, isDown)
            local _, err = C.SendMouseButton(button, isDown)
            if err ~= nil then error(err, 2) end
        end

        function SendMouseWheel(dx, dy)
            local _, err = C.SendMouseWheel(dx, dy)
            if err ~= nil then error(err, 2) end
        end

        function ExitClient()
            local _, err = C.ExitClient()
            if err ~= nil then error(err, 2) end
        end

        function Sleep(seconds)
            local _, err = C.Sleep(seconds)
            if err ~= nil then error(err, 2) end
        end
        """;

    private readonly IntPtr luaState;
    private readonly IList<GCHandle> bindings = new List<GCHandle>();
    private int tracebackPosition;
    private readonly DebugConnection connection;
    private readonly IList<string> messages;
    private readonly IList<string> screenshots;
    private readonly string screenshotsDir;
    private bool disposed;

    /// <param name="connection">Connection to the client debug interface.</param>
    /// <param name="messages">Collection to which script messages are added.</param>
    /// <param name="screenshots">Collection to which screenshot paths are added.</param>
    /// <param name="screenshotsDir">Directory in which screenshots are saved.</param>
    public DebugScriptHost(DebugConnection connection, IList<string> messages,
        IList<string> screenshots, string screenshotsDir)
    {
        this.connection = connection;
        this.messages = messages;
        this.screenshots = screenshots;
        this.screenshotsDir = screenshotsDir;

        luaState = luaL_newstate();
        luaL_openlibs(luaState);
        InstallTracebackHandler();
        RegisterNativeBindings();
        RunBootstrap();
    }

    /// <summary>
    ///     Loads and executes the given script.
    /// </summary>
    /// <param name="script">Lua script source.</param>
    /// <exception cref="DebugScriptException">If the script fails to load or run.</exception>
    public void Run(string script)
    {
        var loadResult = luaL_loadstring(luaState, script);
        if (loadResult != LuaResult.Ok)
        {
            var message = lua_tostring(luaState, -1);
            lua_pop(luaState, 1);
            throw new DebugScriptException($"Failed to load the script: {message}");
        }

        var callResult = lua_pcall(luaState, 0, 0, tracebackPosition);
        if (callResult != LuaResult.Ok)
        {
            var message = lua_tostring(luaState, -1);
            lua_pop(luaState, 1);
            throw new DebugScriptException(message);
        }
    }

    /// <summary>
    ///     Closes the Lua state and releases the binding handles that keep the
    ///     registered delegates alive.
    /// </summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        lua_close(luaState);
        foreach (var binding in bindings)
        {
            if (binding.IsAllocated) binding.Free();
        }
        bindings.Clear();
    }

    /// <summary>
    ///     Leaves the debug.traceback error handler installed at a fixed stack position
    ///     so that it can be passed to lua_pcall as the error handler.
    /// </summary>
    private void InstallTracebackHandler()
    {
        lua_getglobal(luaState, "debug");
        lua_getfield(luaState, -1, "traceback");
        tracebackPosition = lua_gettop(luaState);
    }

    /// <summary>
    ///     Registers the native binding functions into a private table in the registry.
    /// </summary>
    private void RegisterNativeBindings()
    {
        lua_createtable(luaState, 0, NativeBindingCount);
        RegisterBinding("OutputMessage", Binding_OutputMessage);
        RegisterBinding("Screenshot", Binding_Screenshot);
        RegisterBinding("GetInputState", Binding_GetInputState);
        RegisterBinding("SendKey", Binding_SendKey);
        RegisterBinding("SendMouseMotion", Binding_SendMouseMotion);
        RegisterBinding("SendMouseButton", Binding_SendMouseButton);
        RegisterBinding("SendMouseWheel", Binding_SendMouseWheel);
        RegisterBinding("ExitClient", Binding_ExitClient);
        RegisterBinding("Sleep", Binding_Sleep);
        lua_setfield(luaState, LUA_REGISTRYINDEX, NativesRegistryKey);
    }

    /// <summary>
    ///     Registers a single native binding into the binding table on top of the stack.
    /// </summary>
    /// <param name="name">Binding name.</param>
    /// <param name="binding">Binding function.</param>
    private void RegisterBinding(string name, LuaCFunction binding)
    {
        bindings.Add(GCHandle.Alloc(binding));
        lua_pushcfunction(luaState, binding);
        lua_setfield(luaState, -2, name);
    }

    /// <summary>
    ///     Runs the bootstrap source that defines the global script functions.
    /// </summary>
    /// <exception cref="DebugClientException">If the bootstrap fails to load or run.</exception>
    private void RunBootstrap()
    {
        var loadResult = luaL_loadstring(luaState, BootstrapSource);
        if (loadResult != LuaResult.Ok)
        {
            var message = lua_tostring(luaState, -1);
            throw new DebugClientException(
                $"Failed to load the client debug Lua bootstrap: {message}");
        }

        var callResult = lua_pcall(luaState, 0, 0, tracebackPosition);
        if (callResult != LuaResult.Ok)
        {
            var message = lua_tostring(luaState, -1);
            throw new DebugClientException(
                $"Failed to run the client debug Lua bootstrap: {message}");
        }
    }

    /// <summary>
    ///     Invokes a binding body, converting any managed exception into a returned
    ///     error message. Managed exceptions must never escape into LuaJIT.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="binding">Binding body returning the number of result values.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Invoke(IntPtr state, Func<int> binding)
    {
        try
        {
            return binding();
        }
        catch (Exception e)
        {
            lua_pushnil(state);
            lua_pushstring(state, e.Message);
            return 2;
        }
    }

    /// <summary>
    ///     Sends a request to the client and waits for the matching response.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>Successful response.</returns>
    /// <exception cref="DebugClientException">
    ///     If the request failed or the client reported an error.
    /// </exception>
    private DebugResponse SendRequest(DebugRequest request)
    {
        var response = connection.SendRequestAsync(request).GetAwaiter().GetResult();
        if (response.Status != DebugResponseStatus.Ok)
            throw new DebugClientException(
                response.ErrorMessage ?? "The client debug interface reported an unspecified error.");
        return response;
    }

    /// <summary>
    ///     Binding for OutputMessage(message): records a message for the caller.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_OutputMessage(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var message = GetStringArgument(state, 1, "OutputMessage", "message");
            messages.Add(message);
            return 0;
        });
    }

    /// <summary>
    ///     Binding for Screenshot(): captures the next rendered frame, saves it as a PNG,
    ///     and returns the absolute path of the saved file.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_Screenshot(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var response = SendRequest(new DebugRequest
            {
                Type = DebugRequestType.Screenshot
            });
            if (response.Details is not ScreenshotResponseDetails details)
                throw new DebugClientException(
                    "The client returned an unexpected response to the screenshot request.");

            var path = ScreenshotWriter.SavePng(details, response.RequestId, screenshotsDir);
            screenshots.Add(path);
            lua_pushstring(state, path);
            return 1;
        });
    }

    /// <summary>
    ///     Binding for GetInputState(): returns the client input state as a table.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_GetInputState(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var response = SendRequest(new DebugRequest
            {
                Type = DebugRequestType.GetInputState
            });
            if (response.Details is not InputStateResponseDetails details)
                throw new DebugClientException(
                    "The client returned an unexpected response to the input state request.");

            PushInputState(state, details);
            return 1;
        });
    }

    /// <summary>
    ///     Binding for SendKey(keycode, isDown, modifier): injects a keyboard event.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_SendKey(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var keycode = GetIntegerArgument(state, 1, "SendKey", "keycode");
            var isDown = GetBooleanArgument(state, 2, "SendKey", "isDown");
            var modifier = GetOptionalIntegerArgument(state, 3, "SendKey", "modifier");

            SendRequest(new DebugRequest
            {
                Type = DebugRequestType.SendKeyEvent,
                Details = new KeyEventRequestDetails
                {
                    Keycode = (int)keycode,
                    IsDown = isDown,
                    Modifier = (ushort)modifier
                }
            });
            return 0;
        });
    }

    /// <summary>
    ///     Binding for SendMouseMotion(x, y): injects a mouse motion event.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_SendMouseMotion(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var x = GetNumberArgument(state, 1, "SendMouseMotion", "x");
            var y = GetNumberArgument(state, 2, "SendMouseMotion", "y");

            SendRequest(new DebugRequest
            {
                Type = DebugRequestType.SendMouseEvent,
                Details = new MouseEventRequestDetails
                {
                    EventType = DebugMouseEventType.Motion,
                    X = (float)x,
                    Y = (float)y
                }
            });
            return 0;
        });
    }

    /// <summary>
    ///     Binding for SendMouseButton(button, isDown): injects a mouse button event.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_SendMouseButton(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var button = GetIntegerArgument(state, 1, "SendMouseButton", "button");
            var isDown = GetBooleanArgument(state, 2, "SendMouseButton", "isDown");

            SendRequest(new DebugRequest
            {
                Type = DebugRequestType.SendMouseEvent,
                Details = new MouseEventRequestDetails
                {
                    EventType = DebugMouseEventType.Button,
                    Button = (byte)button,
                    IsDown = isDown
                }
            });
            return 0;
        });
    }

    /// <summary>
    ///     Binding for SendMouseWheel(dx, dy): injects a mouse wheel scroll event.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_SendMouseWheel(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var dx = GetNumberArgument(state, 1, "SendMouseWheel", "dx");
            var dy = GetNumberArgument(state, 2, "SendMouseWheel", "dy");

            SendRequest(new DebugRequest
            {
                Type = DebugRequestType.SendMouseEvent,
                Details = new MouseEventRequestDetails
                {
                    EventType = DebugMouseEventType.Wheel,
                    Dx = (float)dx,
                    Dy = (float)dy
                }
            });
            return 0;
        });
    }

    /// <summary>
    ///     Binding for ExitClient(): asks the client to exit.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_ExitClient(IntPtr state)
    {
        return Invoke(state, () =>
        {
            SendRequest(new DebugRequest
            {
                Type = DebugRequestType.Exit
            });
            return 0;
        });
    }

    /// <summary>
    ///     Binding for Sleep(seconds): suspends the script for the given time.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <returns>Number of result values on the stack.</returns>
    private int Binding_Sleep(IntPtr state)
    {
        return Invoke(state, () =>
        {
            var seconds = GetNumberArgument(state, 1, "Sleep", "seconds");
            if (seconds < 0.0)
                throw new DebugClientException(
                    "Sleep: the number of seconds must not be negative.");

            Thread.Sleep((int)Math.Min(seconds * 1000.0, int.MaxValue));
            return 0;
        });
    }

    /// <summary>
    ///     Pushes the client input state onto the stack as a table.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="details">Input state details.</param>
    private static void PushInputState(IntPtr state, InputStateResponseDetails details)
    {
        lua_createtable(state, 0, 7);

        lua_createtable(state, details.PressedKeys.Count, 0);
        for (var i = 0; i < details.PressedKeys.Count; ++i)
        {
            lua_pushinteger(state, details.PressedKeys[i]);
            lua_rawseti(state, -2, i + 1);
        }
        lua_setfield(state, -2, "PressedKeys");

        lua_pushnumber(state, details.MouseX);
        lua_setfield(state, -2, "MouseX");
        lua_pushnumber(state, details.MouseY);
        lua_setfield(state, -2, "MouseY");
        lua_pushboolean(state, details.LeftDown);
        lua_setfield(state, -2, "LeftDown");
        lua_pushboolean(state, details.MiddleDown);
        lua_setfield(state, -2, "MiddleDown");
        lua_pushboolean(state, details.RightDown);
        lua_setfield(state, -2, "RightDown");
        lua_pushnumber(state, details.TotalScrollAmount);
        lua_setfield(state, -2, "TotalScrollAmount");
    }

    /// <summary>
    ///     Gets a string argument, accepting strings and numbers like the Lua tostring
    ///     conversion.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="index">Argument index.</param>
    /// <param name="functionName">Calling function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <returns>Argument value.</returns>
    private static string GetStringArgument(IntPtr state, int index, string functionName,
        string argumentName)
    {
        var type = lua_type(state, index);
        if (type != LuaType.String && type != LuaType.Number)
            throw new DebugClientException(
                $"{functionName}: expected a string for argument #{index} ({argumentName}).");
        return lua_tostring(state, index);
    }

    /// <summary>
    ///     Gets a required numeric argument.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="index">Argument index.</param>
    /// <param name="functionName">Calling function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <returns>Argument value.</returns>
    private static double GetNumberArgument(IntPtr state, int index, string functionName,
        string argumentName)
    {
        if (!lua_isnumber(state, index))
            throw new DebugClientException(
                $"{functionName}: expected a number for argument #{index} ({argumentName}).");
        return lua_tonumber(state, index);
    }

    /// <summary>
    ///     Gets a required integer argument.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="index">Argument index.</param>
    /// <param name="functionName">Calling function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <returns>Argument value.</returns>
    private static long GetIntegerArgument(IntPtr state, int index, string functionName,
        string argumentName)
    {
        return (long)GetNumberArgument(state, index, functionName, argumentName);
    }

    /// <summary>
    ///     Gets an optional integer argument, defaulting to zero when absent or nil.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="index">Argument index.</param>
    /// <param name="functionName">Calling function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <returns>Argument value.</returns>
    private static long GetOptionalIntegerArgument(IntPtr state, int index,
        string functionName, string argumentName)
    {
        var type = lua_type(state, index);
        if (type == LuaType.None || type == LuaType.Nil) return 0;
        return GetIntegerArgument(state, index, functionName, argumentName);
    }

    /// <summary>
    ///     Gets a required boolean argument, interpreting any non-nil value as in Lua.
    /// </summary>
    /// <param name="state">Lua state.</param>
    /// <param name="index">Argument index.</param>
    /// <param name="functionName">Calling function name for error messages.</param>
    /// <param name="argumentName">Argument name for error messages.</param>
    /// <returns>Argument value.</returns>
    private static bool GetBooleanArgument(IntPtr state, int index, string functionName,
        string argumentName)
    {
        var type = lua_type(state, index);
        if (type == LuaType.None || type == LuaType.Nil)
            throw new DebugClientException(
                $"{functionName}: expected a value for argument #{index} ({argumentName}).");
        return lua_toboolean(state, index);
    }
}
