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

using System.Linq;
using Sovereign.ClientCore.Systems.Input;
using TestClientCore.TestSupport;
using static SDL2.SDL;
using Xunit;

namespace TestClientCore.Systems.Input;

/// <summary>
///     Unit tests for <see cref="KeyboardState"/> and the <see cref="InputServices"/>
///     pressed-keys facade.
/// </summary>
public class TestKeyboardState
{
    [Fact]
    public void GetPressedKeys_EmptyWhenNoKeysPressed()
    {
        var state = new KeyboardState();

        Assert.Empty(state.GetPressedKeys());
    }

    [Fact]
    public void GetPressedKeys_ReturnsPressedKeys()
    {
        var state = new KeyboardState();
        state.KeyDown(SDL_Keycode.SDLK_w);
        state.KeyDown(SDL_Keycode.SDLK_a);

        var pressed = state.GetPressedKeys();

        Assert.Equal(2, pressed.Count);
        Assert.Contains(SDL_Keycode.SDLK_w, pressed);
        Assert.Contains(SDL_Keycode.SDLK_a, pressed);
    }

    [Fact]
    public void GetPressedKeys_ExcludesReleasedKeys()
    {
        var state = new KeyboardState();
        state.KeyDown(SDL_Keycode.SDLK_w);
        state.KeyDown(SDL_Keycode.SDLK_a);
        state.KeyUp(SDL_Keycode.SDLK_w);

        var pressed = state.GetPressedKeys();

        Assert.Single(pressed);
        Assert.Contains(SDL_Keycode.SDLK_a, pressed);
    }

    [Fact]
    public void GetPressedKeys_SnapshotIsIndependentOfLaterChanges()
    {
        var state = new KeyboardState();
        state.KeyDown(SDL_Keycode.SDLK_w);
        var snapshot = state.GetPressedKeys();

        state.KeyDown(SDL_Keycode.SDLK_a);
        state.KeyUp(SDL_Keycode.SDLK_w);

        var current = state.GetPressedKeys();
        Assert.Single(current);
        Assert.Equal(SDL_Keycode.SDLK_a, current.Single());
        Assert.Single(snapshot);
        Assert.Equal(SDL_Keycode.SDLK_w, snapshot.Single());
    }

    [Fact]
    public void InputServices_GetPressedKeys_DelegatesToKeyboardState()
    {
        var keyboardState = new KeyboardState();
        var mouseState = CreateMouseState();
        var services = new InputServices(mouseState, keyboardState);

        keyboardState.KeyDown(SDL_Keycode.SDLK_q);
        keyboardState.KeyDown(SDL_Keycode.SDLK_e);
        keyboardState.KeyUp(SDL_Keycode.SDLK_e);

        var pressed = services.GetPressedKeys();

        Assert.Single(pressed);
        Assert.Equal(SDL_Keycode.SDLK_q, pressed.Single());
    }

    /// <summary>
    ///     Creates a MouseState suitable for tests.
    /// </summary>
    /// <returns>MouseState.</returns>
    private static MouseState CreateMouseState()
    {
        return new MouseState(new InputInternalController(), new TestEventSender(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MouseState>.Instance);
    }
}
