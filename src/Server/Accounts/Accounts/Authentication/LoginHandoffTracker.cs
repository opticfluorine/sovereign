/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Tracks connection handoffs after successful login.
/// </summary>
public sealed class LoginHandoffTracker
{
    /// <summary>
    ///     Account IDs with pending handoffs.
    /// </summary>
    private readonly HashSet<Guid> pendingHandoffs = new();

    /// <summary>
    ///     Adds an account to the pending handoff list.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <remarks>
    ///     If the account is already in the pending handoff list, this
    ///     method has no effect.
    /// </remarks>
    public void AddPendingHandoff(Guid accountId)
    {
        pendingHandoffs.Add(accountId);
    }

    /// <summary>
    ///     Removes an account from the pending handoff list if it is
    ///     present.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>true if the account was in the list; false otherwise.</returns>
    public bool TakePendingHandoff(Guid accountId)
    {
        return pendingHandoffs.Remove(accountId);
    }
}