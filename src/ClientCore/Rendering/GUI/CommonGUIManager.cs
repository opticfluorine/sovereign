/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Numerics;
using System.Text;
using ImGuiNET;
using SDL2;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Timing;

namespace Sovereign.ClientCore.Rendering.Gui
{

    /// <summary>
    /// Responsible for configuring the renderer-independent GUI functions.
    /// </summary>
    /// <remarks>
    /// This class is largely based on the ImGui SDL2 C++ backend
    /// (imgui_impl_sdl.cpp).
    /// </remarks>
    public sealed class CommonGuiManager : IDisposable
    {
        private readonly MainDisplay mainDisplay;

        /// <summary>
        /// SDL cursor array.
        /// </summary>
        private readonly IntPtr[] mouseCursors =
            new IntPtr[(int) ImGuiMouseCursor.COUNT];

        /// <summary>
        /// Mouse press state array.
        /// </summary>
        private readonly bool[] mousePressed = new bool[3];

        private readonly FatalErrorHandler fatalErrorHandler;
        private readonly IErrorHandler errorHandler;
        private readonly ISystemTimer systemTimer;

        /// <summary>
        /// Last system time at which a frame was generated.
        /// </summary>
        private ulong lastSystemTime;

        /// <summary>
        /// ImGui context.
        /// </summary>
        private IntPtr context;

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
            {
                if (cursor != IntPtr.Zero)
                {
                    SDL.SDL_FreeCursor(cursor);
                }
            }
        }

        /// <summary>
        /// Initializes the renderer-independent GUI functions.
        /// Must be called after MainDisplay.Show().
        /// </summary>
        /// <remarks>
        /// Based on the ImGui_ImplSDL2_Init function in the C++ imgui library.
        /// </remarks>
        public void Initialize()
        {
            // Initialize ImGui.
            context = ImGui.CreateContext();
            ImGui.StyleColorsDark();

            // Configure input settings.
            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

            // Keyboard mappings.
            io.KeyMap[(int) ImGuiKey.Tab] = (int) SDL.SDL_Scancode.SDL_SCANCODE_TAB;
            io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) SDL.SDL_Scancode.SDL_SCANCODE_LEFT;
            io.KeyMap[(int) ImGuiKey.RightArrow] = (int) SDL.SDL_Scancode.SDL_SCANCODE_RIGHT;
            io.KeyMap[(int) ImGuiKey.UpArrow] = (int) SDL.SDL_Scancode.SDL_SCANCODE_UP;
            io.KeyMap[(int) ImGuiKey.DownArrow] = (int) SDL.SDL_Scancode.SDL_SCANCODE_DOWN;
            io.KeyMap[(int) ImGuiKey.PageUp] = (int) SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP;
            io.KeyMap[(int) ImGuiKey.PageDown] = (int) SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN;
            io.KeyMap[(int) ImGuiKey.Home] = (int) SDL.SDL_Scancode.SDL_SCANCODE_HOME;
            io.KeyMap[(int) ImGuiKey.End] = (int) SDL.SDL_Scancode.SDL_SCANCODE_END;
            io.KeyMap[(int) ImGuiKey.Insert] = (int) SDL.SDL_Scancode.SDL_SCANCODE_INSERT;
            io.KeyMap[(int) ImGuiKey.Delete] = (int) SDL.SDL_Scancode.SDL_SCANCODE_DELETE;
            io.KeyMap[(int) ImGuiKey.Backspace] = (int) SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE;
            io.KeyMap[(int) ImGuiKey.Space] = (int) SDL.SDL_Scancode.SDL_SCANCODE_SPACE;
            io.KeyMap[(int) ImGuiKey.Enter] = (int) SDL.SDL_Scancode.SDL_SCANCODE_RETURN;
            io.KeyMap[(int) ImGuiKey.Escape] = (int) SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE;
            io.KeyMap[(int) ImGuiKey.KeyPadEnter] = (int) SDL.SDL_Scancode.SDL_SCANCODE_RETURN2;
            io.KeyMap[(int) ImGuiKey.A] = (int) SDL.SDL_Scancode.SDL_SCANCODE_A;
            io.KeyMap[(int) ImGuiKey.C] = (int) SDL.SDL_Scancode.SDL_SCANCODE_C;
            io.KeyMap[(int) ImGuiKey.V] = (int) SDL.SDL_Scancode.SDL_SCANCODE_V;
            io.KeyMap[(int) ImGuiKey.X] = (int) SDL.SDL_Scancode.SDL_SCANCODE_X;
            io.KeyMap[(int) ImGuiKey.Y] = (int) SDL.SDL_Scancode.SDL_SCANCODE_Y;
            io.KeyMap[(int) ImGuiKey.Z] = (int) SDL.SDL_Scancode.SDL_SCANCODE_Z;

            // Mouse data.
            mouseCursors[(int) ImGuiMouseCursor.Arrow] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
            mouseCursors[(int) ImGuiMouseCursor.TextInput] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
            mouseCursors[(int) ImGuiMouseCursor.ResizeAll] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
            mouseCursors[(int) ImGuiMouseCursor.ResizeNS] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS);
            mouseCursors[(int) ImGuiMouseCursor.ResizeEW] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
            mouseCursors[(int) ImGuiMouseCursor.ResizeNESW] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW);
            mouseCursors[(int) ImGuiMouseCursor.ResizeNWSE] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE);
            mouseCursors[(int) ImGuiMouseCursor.Hand] =
                SDL.SDL_CreateSystemCursor(SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND);
            foreach (var cursor in mouseCursors)
            {
                if (cursor == IntPtr.Zero)
                {
                    // Fatal error.
                    var sb = new StringBuilder();
                    sb.Append("Error creating cursors: ")
                        .Append(SDL.SDL_GetError());
                    errorHandler.Error(sb.ToString());
                    fatalErrorHandler.FatalError();
                }
            }

            // Window data.
            io.ImeWindowHandle = mainDisplay.WindowHwnd;

            // Screen size.
            io.DisplaySize = new Vector2(mainDisplay.DisplayMode.Width,
                mainDisplay.DisplayMode.Height);
            io.DisplayFramebufferScale = Vector2.One;
        }

        /// <summary>
        /// Sets up a new ImGui frame.
        /// </summary>
        /// <remarks>
        /// Based on the ImGui_ImplSDL2_NewFrame() function in the
        /// C++ imgui library.
        /// </remarks>
        public void NewFrame()
        {
            var io = ImGui.GetIO();
            var now = systemTimer.GetTime();
            io.DeltaTime = (now - lastSystemTime) * 1E-3f;
            lastSystemTime = now;

            UpdateMousePosAndButtons();
            UpdateMouseCursor();
        }

        /// <summary>
        /// Processes an SDL event.
        /// </summary>
        /// <param name="ev">Event to process.</param>
        /// <param name="shouldDispatch">If true, the event should be dispatched to the application.</param>
        /// <remarks>
        /// Based on the ImGui_ImplSDL2_ProcessEvent() function in the
        /// C++ imgui library.
        /// </remarks>
        public void ProcessEvent(ref SDL.SDL_Event ev, out bool shouldDispatch)
        {
            var io = ImGui.GetIO();
            shouldDispatch = true;
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    if (ev.wheel.x > 0) io.MouseWheelH += 1;
                    else if (ev.wheel.x < 0) io.MouseWheelH -= 1;
                    if (ev.wheel.y > 0) io.MouseWheel += 1;
                    else if (ev.wheel.y < 0) io.MouseWheel -= 1;
                    shouldDispatch = !io.WantCaptureMouse;
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT)
                        mousePressed[0] = true;
                    if (ev.button.button == SDL.SDL_BUTTON_RIGHT)
                        mousePressed[1] = true;
                    if (ev.button.button == SDL.SDL_BUTTON_MIDDLE)
                        mousePressed[2] = true;
                    shouldDispatch = !io.WantCaptureMouse;
                    break;

                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    io.AddInputCharactersUTF8(ev.text.ToString());
                    break;

                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    shouldDispatch = !io.WantCaptureKeyboard;
                    var key = (int) ev.key.keysym.scancode;
                    if (key < 0 || key >= io.KeysDown.Count) break;
                    io.KeysDown[key] = (ev.type == SDL.SDL_EventType.SDL_KEYDOWN);
                    var kmod = SDL.SDL_GetModState();
                    io.KeyShift = ((kmod & SDL.SDL_Keymod.KMOD_SHIFT) != 0);
                    io.KeyCtrl = ((kmod & SDL.SDL_Keymod.KMOD_CTRL) != 0);
                    io.KeyAlt = ((kmod & SDL.SDL_Keymod.KMOD_ALT) != 0);
                    io.KeySuper = ((kmod & SDL.SDL_Keymod.KMOD_GUI) != 0);
                    break;
            }
        }

        /// <summary>
        /// Updates the mouse cursor in use.
        /// </summary>
        /// <remarks>
        /// Based on the ImGui_ImplSDL2_UpdateMouseCursor() function in the
        /// C++ imgui library.
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
        /// Updates the mouse position and buttons.
        /// </summary>
        /// <remarks>
        /// Based on the ImGui_ImplSDL2_UpdateMousePosAndButtons function
        /// in the C++ imgui library.
        /// </remarks>
        private void UpdateMousePosAndButtons()
        {
            var io = ImGui.GetIO();

            // Initialize mouse position.
            if (io.WantSetMousePos)
            {
                SDL.SDL_WarpMouseInWindow(mainDisplay.WindowHandle,
                    (int) io.MousePos.X, (int) io.MousePos.Y);
            }
            else
            {
                io.MousePos = Vector2.One * float.MaxValue;
            }

            // Check mouse buttons.
            uint mouseButtons = SDL.SDL_GetMouseState(out var mx, out var my);
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
            if (mainDisplay.IsInputFocus)
            {
                io.MousePos = new Vector2(mx, my);
            }
        }
    }

}
