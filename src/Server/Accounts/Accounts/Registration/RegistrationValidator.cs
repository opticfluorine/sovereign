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

using Sovereign.Accounts.Configuration;

namespace Sovereign.Accounts.Accounts.Registration;

/// <summary>
///     Responsible for validating registration-related user input.
/// </summary>
public sealed class RegistrationValidator
{
    private readonly IAccountsConfiguration configuration;

    public RegistrationValidator(IAccountsConfiguration configuration)
    {
        this.configuration = configuration;
    }

    /// <summary>
    ///     Validates registration input.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>true if input is valid, false otherwise.</returns>
    public bool ValidateRegistrationInput(string username, string password)
    {
        // Username must not be blank.
        if (username.Length < 1) return false;

        // Password must be at least the minimum length.
        if (password.Length < configuration.MinimumPasswordLength) return false;

        // All tests passed.
        return true;
    }
}