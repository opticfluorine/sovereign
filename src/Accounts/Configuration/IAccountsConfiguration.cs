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
