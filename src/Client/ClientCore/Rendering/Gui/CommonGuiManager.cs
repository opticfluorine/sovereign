/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Hexa.NET.ImGui;
using SDL2;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Responsible for configuring the renderer-independent GUI functions.
/// </summary>
/// <remarks>
///     This class is largely based on the Dear ImGui SDL2 C++ backend
///     (imgui_impl_sdl2.cpp).
/// </remarks>
public sealed class CommonGuiManager : IDisposable
{
    private readonly IErrorHandler errorHandler;

    private readonly FatalErrorHandler fatalErrorHandler;
    private readonly MainDisplay mainDisplay;

    /// <summary>
    ///     SDL cursor array.
    /// </summary>
    private readonly IntPtr[] mouseCursors =
        new IntPtr[(int)ImGuiMouseCursor.Count];

    /// <summary>
    ///     Mouse press state array.
    /// </summary>
    private readonly bool[] mousePressed = new bool[3];

    private readonly ISystemTimer systemTimer;

    /// <summary>
    ///     ImGui context.
    /// </summary>
    private ImGuiContextPtr context;

    private bool initialized;

    /// <summary>
    ///     Last system time at which a frame was generated.
    /// </summary>
    private ulong lastSystemTime;

    public CommonGuiManager(MainDisplay mainDisplay, FatalErrorHandler fatalErrorHandler,
        IErrorHandler errorHandler, ISystemTimer systemTimer)
    {
        this.mainDisplay = mainDisplay;
        this.fatalErrorHandler = fatalErrorHandler;
        this.errorHandler = errorHandler;
        this.systemTimer = systemTimer;

        lastSystemTime = systemTimer.GetTime();
    }

    public void Dispose()
    {
        foreach (var cursor in mouseCursors)
            if (cursor != IntPtr.Zero)
                SDL.SDL_FreeCursor(cursor);
    }

    /// <summary>
    ///     Initializes the renderer-independent GUI functions.
    ///     Must be called after MainDisplay.Show().
    /// </summary>
    /// <remarks>
    ///     Based on the ImGui_ImplSDL2_Init function in the C++ imgui library.
    /// </remarks>
    public void Initialize()
    {
        // Initialize ImGui, configure global style.
        context = ImGui.CreateContext();
        ImGui.StyleColorsDark();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16.0f, 16.0f));
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, new Vector4(0.16f, 0.29f, 0.48f, 1.00f));

        // Configure input settings.
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

        // Mouse data.
        mouseCursors[(int)ImGuiMouseCursor.Arrow] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        mouseCursors[(int)ImGuiMouseCursor.TextInput] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
        mouseCursors[(int)ImGuiMouseCursor.ResizeAll] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
        mouseCursors[(int)ImGuiMouseCursor.ResizeNs] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS);
        mouseCursors[(int)ImGuiMouseCursor.ResizeEw] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
        mouseCursors[(int)ImGuiMouseCursor.ResizeNesw] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW);
        mouseCursors[(int)ImGuiMouseCursor.ResizeNwse] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE);
        mouseCursors[(int)ImGuiMouseCursor.Hand] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND);
        mouseCursors[(int)ImGuiMouseCursor.Wait] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT);
        mouseCursors[(int)ImGuiMouseCursor.Progress] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAITARROW);
        mouseCursors[(int)ImGuiMouseCursor.NotAllowed] =
            SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO);
        foreach (var cursor in mouseCursors)
            if (cursor == IntPtr.Zero)
            {
                // Fatal error.
                var sb = new StringBuilder();
                sb.Append("Error creating cursors: ")
                    .Append(SDL.SDL_GetError());
                errorHandler.Error(sb.ToString());
                fatalErrorHandler.FatalError();
            }

        // Screen size.
        io.DisplaySize = new Vector2(mainDisplay.DisplayMode!.Width,
            mainDisplay.DisplayMode!.Height);
        io.DisplayFramebufferScale = Vector2.One;

        initialized = true;
    }

    /// <summary>
    ///     Sets up a new ImGui frame.
    /// </summary>
    /// <remarks>
    ///     Based on the ImGui_ImplSDL2_NewFrame() function in the
    ///     C++ imgui library.
    /// </remarks>
    public void NewFrame()
    {
        if (!initialized) return;

        var io = ImGui.GetIO();
        var now = systemTimer.GetTime();
        io.DeltaTime = (now - lastSystemTime) * UnitConversions.UsToS;
        lastSystemTime = now;

        UpdateMousePosAndButtons();
        UpdateMouseCursor();

        ImGui.NewFrame();
    }

    /// <summary>
    ///     Renders the GUI into the Dear ImGui internal buffers.
    /// </summary>
    /// <returns>Draw data for rendering the GUI layer of the next frame.</returns>
    public ImDrawDataPtr Render()
    {
        ImGui.Render();
        return ImGui.GetDrawData();
    }

    /// <summary>
    ///     Processes an SDL event.
    /// </summary>
    /// <param name="ev">Event to process.</param>
    /// <param name="shouldDispatch">If true, the event should be dispatched to the application.</param>
    /// <remarks>
    ///     Based on the ImGui_ImplSDL2_ProcessEvent() function in the
    ///     C++ imgui library.
    /// </remarks>
    public void ProcessEvent(ref SDL.SDL_Event ev, out bool shouldDispatch)
    {
        shouldDispatch = true;
        if (!initialized) return;

        var io = ImGui.GetIO();
        switch (ev.type)
        {
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
                io.AddMouseSourceEvent(ev.motion.which == SDL.SDL_TOUCH_MOUSEID
                    ? ImGuiMouseSource.TouchScreen
                    : ImGuiMouseSource.Mouse);
                io.AddMousePosEvent(ev.motion.x, ev.motion.y);
                shouldDispatch = !io.WantCaptureMouse;
                break;

            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                io.AddMouseSourceEvent(ev.motion.which == SDL.SDL_TOUCH_MOUSEID
                    ? ImGuiMouseSource.TouchScreen
                    : ImGuiMouseSource.Mouse);
                io.AddMouseWheelEvent(ev.wheel.preciseX, ev.wheel.preciseY);
                shouldDispatch = !io.WantCaptureMouse;
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
            {
                var button = (uint)ev.button.button switch
                {
                    SDL.SDL_BUTTON_LEFT => 0,
                    SDL.SDL_BUTTON_RIGHT => 1,
                    SDL.SDL_BUTTON_MIDDLE => 2,
                    SDL.SDL_BUTTON_X1 => 3,
                    SDL.SDL_BUTTON_X2 => 4,
                    _ => -1
                };
                if (button != -1)
                {
                    io.AddMouseSourceEvent(ev.motion.which == SDL.SDL_TOUCH_MOUSEID
                        ? ImGuiMouseSource.TouchScreen
                        : ImGuiMouseSource.Mouse);
                    io.AddMouseButtonEvent(button, ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN);
                }
            }
                shouldDispatch = !io.WantCaptureMouse;
                break;

            case SDL.SDL_EventType.SDL_TEXTINPUT:
                HandleTextInput(ref ev, io);
                break;

            case SDL.SDL_EventType.SDL_KEYDOWN:
            case SDL.SDL_EventType.SDL_KEYUP:
            {
                var key = MapSdlKeyToImGui(ev.key.keysym.sym);
                var mod = ev.key.keysym.mod;
                io.AddKeyEvent(ImGuiKey.ModCtrl, mod.HasFlag(SDL.SDL_Keymod.KMOD_CTRL));
                io.AddKeyEvent(ImGuiKey.ModShift, mod.HasFlag(SDL.SDL_Keymod.KMOD_SHIFT));
                io.AddKeyEvent(ImGuiKey.ModAlt, mod.HasFlag(SDL.SDL_Keymod.KMOD_ALT));
                io.AddKeyEvent(ImGuiKey.ModSuper, mod.HasFlag(SDL.SDL_Keymod.KMOD_GUI));
                io.AddKeyEvent(key, ev.type == SDL.SDL_EventType.SDL_KEYDOWN);
                io.SetKeyEventNativeData(key, (int)ev.key.keysym.sym, (int)ev.key.keysym.scancode,
                    (int)ev.key.keysym.scancode);
            }
                shouldDispatch = !io.WantCaptureKeyboard;
                break;

            case SDL.SDL_EventType.SDL_WINDOWEVENT:
                if (ev.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
                    io.AddFocusEvent(true);
                else if (ev.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST)
                    io.AddFocusEvent(false);
                break;
        }
    }

    /// <summary>
    ///     Handles a text input event.
    /// </summary>
    /// <param name="ev">Event.</param>
    /// <param name="io">ImGui IO object.</param>
    private unsafe void HandleTextInput(ref SDL.SDL_Event ev, ImGuiIOPtr io)
    {
        fixed (byte* ptr = ev.text.text)
        {
            var str = Marshal.PtrToStringUTF8(new IntPtr(ptr));
            if (str != null) io.AddInputCharactersUTF8(str);
        }
    }

    /// <summary>
    ///     Updates the mouse cursor in use.
    /// </summary>
    /// <remarks>
    ///     Based on the ImGui_ImplSDL2_UpdateMouseCursor() function in the
    ///     C++ imgui library.
    /// </remarks>
    private void UpdateMouseCursor()
    {
        var io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0) return;

        var imguiCursor = ImGui.GetMouseCursor();
        if (io.MouseDrawCursor || imguiCursor == ImGuiMouseCursor.None)
        {
            SDL.SDL_ShowCursor(0);
        }
        else
        {
            SDL.SDL_SetCursor(mouseCursors[(int)imguiCursor]);
            SDL.SDL_ShowCursor(1);
        }
    }

    /// <summary>
    ///     Updates the mouse position and buttons.
    /// </summary>
    /// <remarks>
    ///     Based on the ImGui_ImplSDL2_UpdateMousePosAndButtons function
    ///     in the C++ imgui library.
    /// </remarks>
    private void UpdateMousePosAndButtons()
    {
        var io = ImGui.GetIO();

        // Initialize mouse position.
        if (io.WantSetMousePos)
            SDL.SDL_WarpMouseInWindow(mainDisplay.WindowHandle,
                (int)io.MousePos.X, (int)io.MousePos.Y);
        else
            io.MousePos = Vector2.One * float.MaxValue;

        // Check mouse buttons.
        var mouseButtons = SDL.SDL_GetMouseState(out var mx, out var my);
        io.MouseDown[0] = mousePressed[0] ||
                          (mouseButtons & SDL.SDL_BUTTON(SDL.SDL_BUTTON_LEFT)) != 0;
        io.MouseDown[1] = mousePressed[1] ||
                          (mouseButtons & SDL.SDL_BUTTON(SDL.SDL_BUTTON_RIGHT)) != 0;
        io.MouseDown[2] = mousePressed[2] ||
                          (mouseButtons & SDL.SDL_BUTTON(SDL.SDL_BUTTON_MIDDLE)) != 0;
        mousePressed[0] = false;
        mousePressed[1] = false;
        mousePressed[2] = false;

        // Update mouse position.
        if (mainDisplay.IsInputFocus) io.MousePos = new Vector2(mx, my);
    }

    /// <summary>
    ///     Maps an SDL keycode to the corresponding ImGui keycode.
    /// </summary>
    /// <param name="keycode">SDL keycode.</param>
    /// <returns>ImGui keycode.</returns>
    private static ImGuiKey MapSdlKeyToImGui(SDL.SDL_Keycode keycode)
    {
        switch (keycode)
        {
            case SDL.SDL_Keycode.SDLK_TAB: return ImGuiKey.Tab;
            case SDL.SDL_Keycode.SDLK_LEFT: return ImGuiKey.LeftArrow;
            case SDL.SDL_Keycode.SDLK_RIGHT: return ImGuiKey.RightArrow;
            case SDL.SDL_Keycode.SDLK_UP: return ImGuiKey.UpArrow;
            case SDL.SDL_Keycode.SDLK_DOWN: return ImGuiKey.DownArrow;
            case SDL.SDL_Keycode.SDLK_PAGEUP: return ImGuiKey.PageUp;
            case SDL.SDL_Keycode.SDLK_PAGEDOWN: return ImGuiKey.PageDown;
            case SDL.SDL_Keycode.SDLK_HOME: return ImGuiKey.Home;
            case SDL.SDL_Keycode.SDLK_END: return ImGuiKey.End;
            case SDL.SDL_Keycode.SDLK_INSERT: return ImGuiKey.Insert;
            case SDL.SDL_Keycode.SDLK_DELETE: return ImGuiKey.Delete;
            case SDL.SDL_Keycode.SDLK_BACKSPACE: return ImGuiKey.Backspace;
            case SDL.SDL_Keycode.SDLK_SPACE: return ImGuiKey.Space;
            case SDL.SDL_Keycode.SDLK_RETURN: return ImGuiKey.Enter;
            case SDL.SDL_Keycode.SDLK_ESCAPE: return ImGuiKey.Escape;
            case SDL.SDL_Keycode.SDLK_QUOTE: return ImGuiKey.Apostrophe;
            case SDL.SDL_Keycode.SDLK_COMMA: return ImGuiKey.Comma;
            case SDL.SDL_Keycode.SDLK_MINUS: return ImGuiKey.Minus;
            case SDL.SDL_Keycode.SDLK_PERIOD: return ImGuiKey.Period;
            case SDL.SDL_Keycode.SDLK_SLASH: return ImGuiKey.Slash;
            case SDL.SDL_Keycode.SDLK_SEMICOLON: return ImGuiKey.Semicolon;
            case SDL.SDL_Keycode.SDLK_EQUALS: return ImGuiKey.Equal;
            case SDL.SDL_Keycode.SDLK_LEFTBRACKET: return ImGuiKey.LeftBracket;
            case SDL.SDL_Keycode.SDLK_BACKSLASH: return ImGuiKey.Backslash;
            case SDL.SDL_Keycode.SDLK_RIGHTBRACKET: return ImGuiKey.RightBracket;
            case SDL.SDL_Keycode.SDLK_BACKQUOTE: return ImGuiKey.GraveAccent;
            case SDL.SDL_Keycode.SDLK_CAPSLOCK: return ImGuiKey.CapsLock;
            case SDL.SDL_Keycode.SDLK_SCROLLLOCK: return ImGuiKey.ScrollLock;
            case SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR: return ImGuiKey.NumLock;
            case SDL.SDL_Keycode.SDLK_PRINTSCREEN: return ImGuiKey.PrintScreen;
            case SDL.SDL_Keycode.SDLK_PAUSE: return ImGuiKey.Pause;
            case SDL.SDL_Keycode.SDLK_KP_0: return ImGuiKey.Keypad0;
            case SDL.SDL_Keycode.SDLK_KP_1: return ImGuiKey.Keypad1;
            case SDL.SDL_Keycode.SDLK_KP_2: return ImGuiKey.Keypad2;
            case SDL.SDL_Keycode.SDLK_KP_3: return ImGuiKey.Keypad3;
            case SDL.SDL_Keycode.SDLK_KP_4: return ImGuiKey.Keypad4;
            case SDL.SDL_Keycode.SDLK_KP_5: return ImGuiKey.Keypad5;
            case SDL.SDL_Keycode.SDLK_KP_6: return ImGuiKey.Keypad6;
            case SDL.SDL_Keycode.SDLK_KP_7: return ImGuiKey.Keypad7;
            case SDL.SDL_Keycode.SDLK_KP_8: return ImGuiKey.Keypad8;
            case SDL.SDL_Keycode.SDLK_KP_9: return ImGuiKey.Keypad9;
            case SDL.SDL_Keycode.SDLK_KP_PERIOD: return ImGuiKey.KeypadDecimal;
            case SDL.SDL_Keycode.SDLK_KP_DIVIDE: return ImGuiKey.KeypadDivide;
            case SDL.SDL_Keycode.SDLK_KP_MULTIPLY: return ImGuiKey.KeypadMultiply;
            case SDL.SDL_Keycode.SDLK_KP_MINUS: return ImGuiKey.KeypadSubtract;
            case SDL.SDL_Keycode.SDLK_KP_PLUS: return ImGuiKey.KeypadAdd;
            case SDL.SDL_Keycode.SDLK_KP_ENTER: return ImGuiKey.KeypadEnter;
            case SDL.SDL_Keycode.SDLK_KP_EQUALS: return ImGuiKey.KeypadEqual;
            case SDL.SDL_Keycode.SDLK_LCTRL: return ImGuiKey.LeftCtrl;
            case SDL.SDL_Keycode.SDLK_LSHIFT: return ImGuiKey.LeftShift;
            case SDL.SDL_Keycode.SDLK_LALT: return ImGuiKey.LeftAlt;
            case SDL.SDL_Keycode.SDLK_LGUI: return ImGuiKey.LeftSuper;
            case SDL.SDL_Keycode.SDLK_RCTRL: return ImGuiKey.RightCtrl;
            case SDL.SDL_Keycode.SDLK_RSHIFT: return ImGuiKey.RightShift;
            case SDL.SDL_Keycode.SDLK_RALT: return ImGuiKey.RightAlt;
            case SDL.SDL_Keycode.SDLK_RGUI: return ImGuiKey.RightSuper;
            case SDL.SDL_Keycode.SDLK_APPLICATION: return ImGuiKey.Menu;
            case SDL.SDL_Keycode.SDLK_0:
                return ImGuiKey.Key0;
            case SDL.SDL_Keycode.SDLK_1:
                return ImGuiKey.Key1;
            case SDL.SDL_Keycode.SDLK_2:
                return ImGuiKey.Key2;
            case SDL.SDL_Keycode.SDLK_3:
                return ImGuiKey.Key3;
            case SDL.SDL_Keycode.SDLK_4:
                return ImGuiKey.Key4;
            case SDL.SDL_Keycode.SDLK_5:
                return ImGuiKey.Key5;
            case SDL.SDL_Keycode.SDLK_6:
                return ImGuiKey.Key6;
            case SDL.SDL_Keycode.SDLK_7:
                return ImGuiKey.Key7;
            case SDL.SDL_Keycode.SDLK_8:
                return ImGuiKey.Key8;
            case SDL.SDL_Keycode.SDLK_9:
                return ImGuiKey.Key9;
            case SDL.SDL_Keycode.SDLK_a: return ImGuiKey.A;
            case SDL.SDL_Keycode.SDLK_b: return ImGuiKey.B;
            case SDL.SDL_Keycode.SDLK_c: return ImGuiKey.C;
            case SDL.SDL_Keycode.SDLK_d: return ImGuiKey.D;
            case SDL.SDL_Keycode.SDLK_e: return ImGuiKey.E;
            case SDL.SDL_Keycode.SDLK_f: return ImGuiKey.F;
            case SDL.SDL_Keycode.SDLK_g: return ImGuiKey.G;
            case SDL.SDL_Keycode.SDLK_h: return ImGuiKey.H;
            case SDL.SDL_Keycode.SDLK_i: return ImGuiKey.I;
            case SDL.SDL_Keycode.SDLK_j: return ImGuiKey.J;
            case SDL.SDL_Keycode.SDLK_k: return ImGuiKey.K;
            case SDL.SDL_Keycode.SDLK_l: return ImGuiKey.L;
            case SDL.SDL_Keycode.SDLK_m: return ImGuiKey.M;
            case SDL.SDL_Keycode.SDLK_n: return ImGuiKey.N;
            case SDL.SDL_Keycode.SDLK_o: return ImGuiKey.O;
            case SDL.SDL_Keycode.SDLK_p: return ImGuiKey.P;
            case SDL.SDL_Keycode.SDLK_q: return ImGuiKey.Q;
            case SDL.SDL_Keycode.SDLK_r: return ImGuiKey.R;
            case SDL.SDL_Keycode.SDLK_s: return ImGuiKey.S;
            case SDL.SDL_Keycode.SDLK_t: return ImGuiKey.T;
            case SDL.SDL_Keycode.SDLK_u: return ImGuiKey.U;
            case SDL.SDL_Keycode.SDLK_v: return ImGuiKey.V;
            case SDL.SDL_Keycode.SDLK_w: return ImGuiKey.W;
            case SDL.SDL_Keycode.SDLK_x: return ImGuiKey.X;
            case SDL.SDL_Keycode.SDLK_y: return ImGuiKey.Y;
            case SDL.SDL_Keycode.SDLK_z: return ImGuiKey.Z;
            case SDL.SDL_Keycode.SDLK_F1: return ImGuiKey.F1;
            case SDL.SDL_Keycode.SDLK_F2: return ImGuiKey.F2;
            case SDL.SDL_Keycode.SDLK_F3: return ImGuiKey.F3;
            case SDL.SDL_Keycode.SDLK_F4: return ImGuiKey.F4;
            case SDL.SDL_Keycode.SDLK_F5: return ImGuiKey.F5;
            case SDL.SDL_Keycode.SDLK_F6: return ImGuiKey.F6;
            case SDL.SDL_Keycode.SDLK_F7: return ImGuiKey.F7;
            case SDL.SDL_Keycode.SDLK_F8: return ImGuiKey.F8;
            case SDL.SDL_Keycode.SDLK_F9: return ImGuiKey.F9;
            case SDL.SDL_Keycode.SDLK_F10: return ImGuiKey.F10;
            case SDL.SDL_Keycode.SDLK_F11: return ImGuiKey.F11;
            case SDL.SDL_Keycode.SDLK_F12: return ImGuiKey.F12;
            case SDL.SDL_Keycode.SDLK_F13: return ImGuiKey.F13;
            case SDL.SDL_Keycode.SDLK_F14: return ImGuiKey.F14;
            case SDL.SDL_Keycode.SDLK_F15: return ImGuiKey.F15;
            case SDL.SDL_Keycode.SDLK_F16: return ImGuiKey.F16;
            case SDL.SDL_Keycode.SDLK_F17: return ImGuiKey.F17;
            case SDL.SDL_Keycode.SDLK_F18: return ImGuiKey.F18;
            case SDL.SDL_Keycode.SDLK_F19: return ImGuiKey.F19;
            case SDL.SDL_Keycode.SDLK_F20: return ImGuiKey.F20;
            case SDL.SDL_Keycode.SDLK_F21: return ImGuiKey.F21;
            case SDL.SDL_Keycode.SDLK_F22: return ImGuiKey.F22;
            case SDL.SDL_Keycode.SDLK_F23: return ImGuiKey.F23;
            case SDL.SDL_Keycode.SDLK_F24: return ImGuiKey.F24;
            case SDL.SDL_Keycode.SDLK_AC_BACK: return ImGuiKey.AppBack;
            case SDL.SDL_Keycode.SDLK_AC_FORWARD: return ImGuiKey.AppForward;
        }

        return ImGuiKey.None;
    }
}