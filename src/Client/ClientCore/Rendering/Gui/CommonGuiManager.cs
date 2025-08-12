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
using SDL3;
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
                SDL.Free(cursor);
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
            SDL.CreateSystemCursor(SDL.SystemCursor.Default);
        mouseCursors[(int)ImGuiMouseCursor.TextInput] =
            SDL.CreateSystemCursor(SDL.SystemCursor.Text);
        mouseCursors[(int)ImGuiMouseCursor.ResizeAll] =
            SDL.CreateSystemCursor(SDL.SystemCursor.Crosshair);
        mouseCursors[(int)ImGuiMouseCursor.ResizeNs] =
            SDL.CreateSystemCursor(SDL.SystemCursor.NSResize);
        mouseCursors[(int)ImGuiMouseCursor.ResizeEw] =
            SDL.CreateSystemCursor(SDL.SystemCursor.EWResize);
        mouseCursors[(int)ImGuiMouseCursor.ResizeNesw] =
            SDL.CreateSystemCursor(SDL.SystemCursor.NESWResize);
        mouseCursors[(int)ImGuiMouseCursor.ResizeNwse] =
            SDL.CreateSystemCursor(SDL.SystemCursor.NWSEResize);
        mouseCursors[(int)ImGuiMouseCursor.Hand] =
            SDL.CreateSystemCursor(SDL.SystemCursor.Pointer);
        mouseCursors[(int)ImGuiMouseCursor.Wait] =
            SDL.CreateSystemCursor(SDL.SystemCursor.Wait);
        mouseCursors[(int)ImGuiMouseCursor.Progress] =
            SDL.CreateSystemCursor(SDL.SystemCursor.Progress);
        mouseCursors[(int)ImGuiMouseCursor.NotAllowed] =
            SDL.CreateSystemCursor(SDL.SystemCursor.NotAllowed);
        foreach (var cursor in mouseCursors)
            if (cursor == IntPtr.Zero)
            {
                // Fatal error.
                var sb = new StringBuilder();
                sb.Append("Error creating cursors: ")
                    .Append(SDL.GetError());
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
    public void ProcessEvent(ref SDL.Event ev, out bool shouldDispatch)
    {
        shouldDispatch = true;
        if (!initialized) return;

        var io = ImGui.GetIO();
        switch ((SDL.EventType)ev.Type)
        {
            case SDL.EventType.MouseMotion:
                io.AddMouseSourceEvent(ev.Motion.Which == SDL.TouchMouseID
                    ? ImGuiMouseSource.TouchScreen
                    : ImGuiMouseSource.Mouse);
                io.AddMousePosEvent(ev.Motion.X, ev.Motion.Y);
                shouldDispatch = !io.WantCaptureMouse;
                break;

            case SDL.EventType.MouseWheel:
                io.AddMouseSourceEvent(ev.Motion.Which == SDL.TouchMouseID
                    ? ImGuiMouseSource.TouchScreen
                    : ImGuiMouseSource.Mouse);
                io.AddMouseWheelEvent(ev.Wheel.X, ev.Wheel.Y);
                shouldDispatch = !io.WantCaptureMouse;
                break;

            case SDL.EventType.MouseButtonDown:
            case SDL.EventType.MouseButtonUp:
            {
                var button = ev.Button.Button switch
                {
                    SDL.ButtonLeft => 0,
                    SDL.ButtonRight => 1,
                    SDL.ButtonMiddle => 2,
                    SDL.ButtonX1 => 3,
                    SDL.ButtonX2 => 4,
                    _ => -1
                };
                if (button != -1)
                {
                    io.AddMouseSourceEvent(ev.Motion.Which == SDL.TouchMouseID
                        ? ImGuiMouseSource.TouchScreen
                        : ImGuiMouseSource.Mouse);
                    io.AddMouseButtonEvent(button, ev.Type == (uint)SDL.EventType.MouseButtonDown);
                }
            }
                shouldDispatch = !io.WantCaptureMouse;
                break;

            case SDL.EventType.TextInput:
                HandleTextInput(ref ev, io);
                break;

            case SDL.EventType.KeyDown:
            case SDL.EventType.KeyUp:
            {
                var key = MapSdlKeyToImGui(ev.Key.Key);
                var mod = ev.Key.Mod;
                io.AddKeyEvent(ImGuiKey.ModCtrl, mod.HasFlag(SDL.Keymod.Ctrl));
                io.AddKeyEvent(ImGuiKey.ModShift, mod.HasFlag(SDL.Keymod.Shift));
                io.AddKeyEvent(ImGuiKey.ModAlt, mod.HasFlag(SDL.Keymod.Alt));
                io.AddKeyEvent(ImGuiKey.ModSuper, mod.HasFlag(SDL.Keymod.GUI));
                io.AddKeyEvent(key, ev.Type == (uint)SDL.EventType.KeyDown);
                io.SetKeyEventNativeData(key, (int)ev.Key.Key, (int)ev.Key.Scancode,
                    (int)ev.Key.Scancode);
            }
                shouldDispatch = !io.WantCaptureKeyboard;
                break;

            case SDL.EventType.WindowFocusGained:
                io.AddFocusEvent(true);
                break;

            case SDL.EventType.WindowFocusLost:
                io.AddFocusEvent(false);
                break;
        }
    }

    /// <summary>
    ///     Handles a text input event.
    /// </summary>
    /// <param name="ev">Event.</param>
    /// <param name="io">ImGui IO object.</param>
    private unsafe void HandleTextInput(ref SDL.Event ev, ImGuiIOPtr io)
    {
        var ptr = (byte*)ev.Text.Text;
        var str = Marshal.PtrToStringUTF8(new IntPtr(ptr));
        if (str != null) io.AddInputCharactersUTF8(str);
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
            SDL.HideCursor();
        }
        else
        {
            SDL.SetCursor(mouseCursors[(int)imguiCursor]);
            SDL.ShowCursor();
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
            SDL.WarpMouseInWindow(mainDisplay.WindowHandle,
                (int)io.MousePos.X, (int)io.MousePos.Y);
        else
            io.MousePos = Vector2.One * float.MaxValue;

        // Check mouse buttons.
        var mouseButtons = SDL.GetMouseState(out var mx, out var my);
        io.MouseDown[0] = mousePressed[0] ||
                          (mouseButtons & SDL.MouseButtonFlags.Left) != 0;
        io.MouseDown[1] = mousePressed[1] ||
                          (mouseButtons & SDL.MouseButtonFlags.Right) != 0;
        io.MouseDown[2] = mousePressed[2] ||
                          (mouseButtons & SDL.MouseButtonFlags.Middle) != 0;
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
    private static ImGuiKey MapSdlKeyToImGui(SDL.Keycode keycode)
    {
        switch (keycode)
        {
            case SDL.Keycode.Tab: return ImGuiKey.Tab;
            case SDL.Keycode.Left: return ImGuiKey.LeftArrow;
            case SDL.Keycode.Right: return ImGuiKey.RightArrow;
            case SDL.Keycode.Up: return ImGuiKey.UpArrow;
            case SDL.Keycode.Down: return ImGuiKey.DownArrow;
            case SDL.Keycode.Pageup: return ImGuiKey.PageUp;
            case SDL.Keycode.Pagedown: return ImGuiKey.PageDown;
            case SDL.Keycode.Home: return ImGuiKey.Home;
            case SDL.Keycode.End: return ImGuiKey.End;
            case SDL.Keycode.Insert: return ImGuiKey.Insert;
            case SDL.Keycode.Delete: return ImGuiKey.Delete;
            case SDL.Keycode.Backspace: return ImGuiKey.Backspace;
            case SDL.Keycode.Space: return ImGuiKey.Space;
            case SDL.Keycode.Return: return ImGuiKey.Enter;
            case SDL.Keycode.Escape: return ImGuiKey.Escape;
            case SDL.Keycode.Apostrophe: return ImGuiKey.Apostrophe;
            case SDL.Keycode.Comma: return ImGuiKey.Comma;
            case SDL.Keycode.Minus: return ImGuiKey.Minus;
            case SDL.Keycode.Period: return ImGuiKey.Period;
            case SDL.Keycode.Slash: return ImGuiKey.Slash;
            case SDL.Keycode.Semicolon: return ImGuiKey.Semicolon;
            case SDL.Keycode.Equals: return ImGuiKey.Equal;
            case SDL.Keycode.LeftBracket: return ImGuiKey.LeftBracket;
            case SDL.Keycode.Backslash: return ImGuiKey.Backslash;
            case SDL.Keycode.RightBracket: return ImGuiKey.RightBracket;
            case SDL.Keycode.Grave: return ImGuiKey.GraveAccent;
            case SDL.Keycode.Capslock: return ImGuiKey.CapsLock;
            case SDL.Keycode.ScrollLock: return ImGuiKey.ScrollLock;
            case SDL.Keycode.NumLockClear: return ImGuiKey.NumLock;
            case SDL.Keycode.PrintScreen: return ImGuiKey.PrintScreen;
            case SDL.Keycode.Pause: return ImGuiKey.Pause;
            case SDL.Keycode.Kp0: return ImGuiKey.Keypad0;
            case SDL.Keycode.Kp1: return ImGuiKey.Keypad1;
            case SDL.Keycode.Kp2: return ImGuiKey.Keypad2;
            case SDL.Keycode.Kp3: return ImGuiKey.Keypad3;
            case SDL.Keycode.Kp4: return ImGuiKey.Keypad4;
            case SDL.Keycode.Kp5: return ImGuiKey.Keypad5;
            case SDL.Keycode.Kp6: return ImGuiKey.Keypad6;
            case SDL.Keycode.Kp7: return ImGuiKey.Keypad7;
            case SDL.Keycode.Kp8: return ImGuiKey.Keypad8;
            case SDL.Keycode.Kp9: return ImGuiKey.Keypad9;
            case SDL.Keycode.KpPeriod: return ImGuiKey.KeypadDecimal;
            case SDL.Keycode.KpDivide: return ImGuiKey.KeypadDivide;
            case SDL.Keycode.KpMultiply: return ImGuiKey.KeypadMultiply;
            case SDL.Keycode.KpMinus: return ImGuiKey.KeypadSubtract;
            case SDL.Keycode.KpPlus: return ImGuiKey.KeypadAdd;
            case SDL.Keycode.KpEnter: return ImGuiKey.KeypadEnter;
            case SDL.Keycode.KpEquals: return ImGuiKey.KeypadEqual;
            case SDL.Keycode.LCtrl: return ImGuiKey.LeftCtrl;
            case SDL.Keycode.LShift: return ImGuiKey.LeftShift;
            case SDL.Keycode.LAlt: return ImGuiKey.LeftAlt;
            case SDL.Keycode.LGui: return ImGuiKey.LeftSuper;
            case SDL.Keycode.RCtrl: return ImGuiKey.RightCtrl;
            case SDL.Keycode.RShift: return ImGuiKey.RightShift;
            case SDL.Keycode.RAlt: return ImGuiKey.RightAlt;
            case SDL.Keycode.RGUI: return ImGuiKey.RightSuper;
            case SDL.Keycode.Application: return ImGuiKey.Menu;
            case SDL.Keycode.Alpha0:
                return ImGuiKey.Key0;
            case SDL.Keycode.Alpha1:
                return ImGuiKey.Key1;
            case SDL.Keycode.Alpha2:
                return ImGuiKey.Key2;
            case SDL.Keycode.Alpha3:
                return ImGuiKey.Key3;
            case SDL.Keycode.Alpha4:
                return ImGuiKey.Key4;
            case SDL.Keycode.Alpha5:
                return ImGuiKey.Key5;
            case SDL.Keycode.Alpha6:
                return ImGuiKey.Key6;
            case SDL.Keycode.Alpha7:
                return ImGuiKey.Key7;
            case SDL.Keycode.Alpha8:
                return ImGuiKey.Key8;
            case SDL.Keycode.Alpha9:
                return ImGuiKey.Key9;
            case SDL.Keycode.A: return ImGuiKey.A;
            case SDL.Keycode.B: return ImGuiKey.B;
            case SDL.Keycode.C: return ImGuiKey.C;
            case SDL.Keycode.D: return ImGuiKey.D;
            case SDL.Keycode.E: return ImGuiKey.E;
            case SDL.Keycode.F: return ImGuiKey.F;
            case SDL.Keycode.G: return ImGuiKey.G;
            case SDL.Keycode.H: return ImGuiKey.H;
            case SDL.Keycode.I: return ImGuiKey.I;
            case SDL.Keycode.J: return ImGuiKey.J;
            case SDL.Keycode.K: return ImGuiKey.K;
            case SDL.Keycode.L: return ImGuiKey.L;
            case SDL.Keycode.M: return ImGuiKey.M;
            case SDL.Keycode.N: return ImGuiKey.N;
            case SDL.Keycode.O: return ImGuiKey.O;
            case SDL.Keycode.P: return ImGuiKey.P;
            case SDL.Keycode.Q: return ImGuiKey.Q;
            case SDL.Keycode.R: return ImGuiKey.R;
            case SDL.Keycode.S: return ImGuiKey.S;
            case SDL.Keycode.T: return ImGuiKey.T;
            case SDL.Keycode.U: return ImGuiKey.U;
            case SDL.Keycode.V: return ImGuiKey.V;
            case SDL.Keycode.W: return ImGuiKey.W;
            case SDL.Keycode.X: return ImGuiKey.X;
            case SDL.Keycode.Y: return ImGuiKey.Y;
            case SDL.Keycode.Z: return ImGuiKey.Z;
            case SDL.Keycode.F1: return ImGuiKey.F1;
            case SDL.Keycode.F2: return ImGuiKey.F2;
            case SDL.Keycode.F3: return ImGuiKey.F3;
            case SDL.Keycode.F4: return ImGuiKey.F4;
            case SDL.Keycode.F5: return ImGuiKey.F5;
            case SDL.Keycode.F6: return ImGuiKey.F6;
            case SDL.Keycode.F7: return ImGuiKey.F7;
            case SDL.Keycode.F8: return ImGuiKey.F8;
            case SDL.Keycode.F9: return ImGuiKey.F9;
            case SDL.Keycode.F10: return ImGuiKey.F10;
            case SDL.Keycode.F11: return ImGuiKey.F11;
            case SDL.Keycode.F12: return ImGuiKey.F12;
            case SDL.Keycode.F13: return ImGuiKey.F13;
            case SDL.Keycode.F14: return ImGuiKey.F14;
            case SDL.Keycode.F15: return ImGuiKey.F15;
            case SDL.Keycode.F16: return ImGuiKey.F16;
            case SDL.Keycode.F17: return ImGuiKey.F17;
            case SDL.Keycode.F18: return ImGuiKey.F18;
            case SDL.Keycode.F19: return ImGuiKey.F19;
            case SDL.Keycode.F20: return ImGuiKey.F20;
            case SDL.Keycode.F21: return ImGuiKey.F21;
            case SDL.Keycode.F22: return ImGuiKey.F22;
            case SDL.Keycode.F23: return ImGuiKey.F23;
            case SDL.Keycode.F24: return ImGuiKey.F24;
            case SDL.Keycode.AcBack: return ImGuiKey.AppBack;
            case SDL.Keycode.AcForward: return ImGuiKey.AppForward;
        }

        return ImGuiKey.None;
    }
}