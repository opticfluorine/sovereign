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
using System.Text;

namespace Sovereign.Accounts.Configuration
{

    /// <summary>
    /// Configuration interface for account services.
    /// </summary>
    public interface IAccountsConfiguration
    {

        /// <summary>
        /// Maximum number of failed login attempts before access to
        /// an account is temporarily disabled.
        /// </summary>
        int MaxFailedLoginAttempts { get; }

        /// <summary>
        /// Length of the login denial period, in seconds. The failed login 
        /// attempt count is reset after this amount of time has elapsed
        /// since the last failed login attempt.
        /// </summary>
        int LoginDenialPeriodSeconds { get; }

        /// <summary>
        /// Minimum length of all new passwords.
        /// </summary>
        int MinimumPasswordLength { get; }

        /// <summary>
        /// Length of the connection handoff period following a successful
        /// login. The connection may only be established during the handoff,
        /// beyond which the handoff will be invalidated.
        /// </summary>
        int HandoffPeriodSeconds { get; }

    }

}
