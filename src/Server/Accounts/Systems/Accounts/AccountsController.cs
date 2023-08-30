// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using Sovereign.EngineCore.Events;
using Sovereign.ServerCore.Events;

namespace Sovereign.Accounts.Systems.Accounts;

/// <summary>
///     Controller API for the Accounts system.
/// </summary>
public class AccountsController
{
    /// <summary>
    ///     Selects a player character for use by an account that is logging in.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerCharacterEntityId">Player character entity ID.</param>
    public void SelectPlayer(IEventSender eventSender, Guid accountId, ulong playerCharacterEntityId)
    {
        var details = new SelectPlayerEventDetails
        {
            AccountId = accountId,
            PlayerCharacterEntityId = playerCharacterEntityId
        };
        var ev = new Event(EventId.Server_Accounts_SelectPlayer, details);
        eventSender.SendEvent(ev);
    }
}