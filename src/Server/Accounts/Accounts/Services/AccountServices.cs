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
using Castle.Core.Logging;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.Accounts.Accounts.Registration;
using Sovereign.EngineUtil.Monads;

namespace Sovereign.Accounts.Accounts.Services;

/// <summary>
///     Provides account-related services.
/// </summary>
public sealed class AccountServices
{
    private readonly AccountAuthenticator authenticator;
    private readonly AuthenticationAttemptLimiter limiter;
    private readonly LoginHandoffTracker loginHandoffTracker;
    private readonly AccountLoginTracker loginTracker;
    private readonly RegistrationController registrationController;
    private readonly RegistrationValidator registrationValidator;
    private readonly SharedSecretManager sharedSecretManager;

    public AccountServices(AccountAuthenticator authenticator,
        AuthenticationAttemptLimiter limiter,
        AccountLoginTracker loginTracker,
        RegistrationValidator registrationValidator,
        RegistrationController registrationController,
        SharedSecretManager sharedSecretManager,
        LoginHandoffTracker loginHandoffTracker)
    {
        this.authenticator = authenticator;
        this.limiter = limiter;
        this.loginTracker = loginTracker;
        this.registrationValidator = registrationValidator;
        this.registrationController = registrationController;
        this.sharedSecretManager = sharedSecretManager;
        this.loginHandoffTracker = loginHandoffTracker;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Attempts to authenticate the account with the given username and password.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="password">Password attempt.</param>
    /// <param name="guid">Account ID.</param>
    /// <param name="secret">Shared secret.</param>
    /// <returns>Authentication result.</returns>
    public AuthenticationResult Authenticate(string username, string password,
        out Guid guid, out string secret)
    {
        guid = Guid.Empty;
        secret = null;

        try
        {
            // Reject if too many authentication attempts have been made recently.
            if (limiter.IsAccountLoginDisabled(username))
            {
                // Too many login attempts.
                Logger.InfoFormat("Rejected login for {0}: too many failed attempts.", username);
                return AuthenticationResult.TooManyAttempts;
            }

            // Verify the username and password combination.
            var authValid = authenticator.Authenticate(username, password, out var id);
            if (!authValid)
            {
                // Bad password, log and reject.
                limiter.RegisterFailedAttempt(username);
                Logger.InfoFormat("Rejected login for {0}: authentication failure.", username);
                return AuthenticationResult.Failed;
            }

            // Verify that the account is not already logged in.
            if (loginTracker.GetLoginState(id) != AccountLoginState.NotLoggedIn)
            {
                // Already logged in, log and reject.
                Logger.InfoFormat("Rejected login for {0}: already logged in.", username);
                return AuthenticationResult.AlreadyLoggedIn;
            }

            // Login successful.
            loginTracker.Login(id);
            loginHandoffTracker.AddPendingHandoff(id);
            secret = sharedSecretManager.AddSecret(id);
            guid = id;

            Logger.InfoFormat("Login successful for {0}.", username);

            return AuthenticationResult.Successful;
        }
        catch (Exception e)
        {
            Logger.Error("Error while handling login request for " + username + "; rejecting.", e);
            return AuthenticationResult.Failed;
        }
    }

    /// <summary>
    ///     Registers a new account.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>Registration result.</returns>
    public RegistrationResult Register(string username, string password)
    {
        try
        {
            // Validate user input.
            if (!registrationValidator.ValidateRegistrationInput(username, password))
                return RegistrationResult.InvalidInput;

            // Attempt registration.
            if (!registrationController.Register(username, password))
                // Interpret failure as username being taken. It could also
                // be caused by other database failures, but in practice
                // violating the unique constraint is likely to be at
                // fault.
                return RegistrationResult.UsernameTaken;

            Logger.InfoFormat("New account registered for {0}.", username);

            return RegistrationResult.Successful;
        }
        catch (Exception e)
        {
            Logger.Error("Error handling registration request.", e);
            return RegistrationResult.UnknownFailure;
        }
    }

    /// <summary>
    ///     Gets the connection ID associated with a logged in player.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <returns>Connection ID if the player is logged in.</returns>
    public Maybe<int> GetConnectionIdForPlayer(ulong playerEntityId)
    {
        var result = new Maybe<int>();
        if (loginTracker.TryGetConnectionIdForPlayer(playerEntityId, out var connectionId))
        {
            result = new Maybe<int>(connectionId);
        }

        return result;
    }
}