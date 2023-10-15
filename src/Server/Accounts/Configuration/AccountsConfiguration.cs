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

using Sovereign.ServerCore.Configuration;

namespace Sovereign.Accounts.Configuration;

/// <summary>
///     Accounts configuration view.
/// </summary>
public sealed class AccountsConfiguration : IAccountsConfiguration
{
    private readonly ServerConfiguration configuration;

    public AccountsConfiguration(IServerConfigurationManager configurationManager)
    {
        configuration = configurationManager.ServerConfiguration;
    }

    public int MaxFailedLoginAttempts => configuration.Accounts.MaxFailedLoginAttempts;

    public int LoginDenialPeriodSeconds => configuration.Accounts.LoginDenialPeriodSeconds;

    public int MinimumPasswordLength => configuration.Accounts.MinimumPasswordLength;

    public int HandoffPeriodSeconds => configuration.Accounts.HandoffPeriodSeconds;
}