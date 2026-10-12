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

using System;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SDL2;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Systems.Input;
using Sovereign.EngineCore.Events;
using TestClientCore.TestSupport;
using Xunit;

namespace TestClientCore.Systems.Input;

/// <summary>
///     Unit tests for the attack key binding and its input edge trigger.
/// </summary>
public class TestAttackKeybinding : IDisposable
{
    private readonly ClientTestHarness harness = new();
    private readonly InGameInputHandler handler;
    private readonly SDL.SDL_Keycode attackKey;

    public TestAttackKeybinding()
    {
        handler = harness.Get<InGameInputHandler>();
        attackKey = harness.Get<Keybindings>().Attack;
    }

    public void Dispose()
    {
        harness.Dispose();
    }

    [Fact]
    public void DefaultOptions_BindsAttackToF()
    {
        var keybindings = new Keybindings(Options.Create(new KeybindingsOptions()),
            NullLogger<Keybindings>.Instance);

        Assert.Equal(SDL.SDL_Keycode.SDLK_f, keybindings.Attack);
    }

    [Fact]
    public void UnknownKeyName_DisablesAttackBinding()
    {
        var options = new KeybindingsOptions();
        options.InGameActions.Attack = "NotARealKey";

        var keybindings = new Keybindings(Options.Create(options), NullLogger<Keybindings>.Instance);

        Assert.Equal(SDL.SDL_Keycode.SDLK_UNKNOWN, keybindings.Attack);
    }

    [Fact]
    public void FreshKeyDown_SendsOneAttack()
    {
        handler.HandleKeyboardEvent(new KeyEventDetails { Key = attackKey }, false, false);

        Assert.Equal(1, AttackCount());
    }

    [Fact]
    public void AutoRepeat_SendsNothing()
    {
        // OS key auto-repeat arrives as a down event with the key already down.
        handler.HandleKeyboardEvent(new KeyEventDetails { Key = attackKey }, false, true);

        Assert.Equal(0, AttackCount());
    }

    [Fact]
    public void ReleaseThenPress_SendsAgain()
    {
        handler.HandleKeyboardEvent(new KeyEventDetails { Key = attackKey }, false, false);
        handler.HandleKeyboardEvent(new KeyEventDetails { Key = attackKey }, true, true);
        handler.HandleKeyboardEvent(new KeyEventDetails { Key = attackKey }, false, false);

        Assert.Equal(2, AttackCount());
    }

    /// <summary>
    ///     Counts the Client_Player_Attack events sent through the harness event sender.
    /// </summary>
    /// <returns>Number of attack events sent.</returns>
    private int AttackCount()
    {
        return harness.EventSender.SentEvents.Count(ev => ev.EventId == EventId.Client_Player_Attack);
    }
}
