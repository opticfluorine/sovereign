﻿/*
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
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<AccountServices> logger;
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
        LoginHandoffTracker loginHandoffTracker,
        ILogger<AccountServices> logger)
    {
        this.authenticator = authenticator;
        this.limiter = limiter;
        this.loginTracker = loginTracker;
        this.registrationValidator = registrationValidator;
        this.registrationController = registrationController;
        this.sharedSecretManager = sharedSecretManager;
        this.loginHandoffTracker = loginHandoffTracker;
        this.logger = logger;
    }

    /// <summary>
    ///     Attempts to authenticate the account with the given username and password.
    /// </summary>
    /// <param name="username">Account username.</param>
    /// <param name="password">Password attempt.</param>
    /// <param name="guid">Account ID, or an empty GUID on failure.</param>
    /// <param name="secret">Shared secret, or an empty string on failure.</param>
    /// <returns>Authentication result.</returns>
    public AuthenticationResult Authenticate(string username, string password,
        out Guid guid, out string secret)
    {
        guid = Guid.Empty;
        secret = "";

        try
        {
            // Reject if too many authentication attempts have been made recently.
            if (limiter.IsAccountLoginDisabled(username))
            {
                // Too many login attempts.
                logger.LogInformation("Rejected login for {Username}: too many failed attempts.", username);
                return AuthenticationResult.TooManyAttempts;
            }

            // Verify the username and password combination.
            var authValid = authenticator.Authenticate(username, password, out var id);
            if (!authValid)
            {
                // Bad password, log and reject.
                limiter.RegisterFailedAttempt(username);
                logger.LogInformation("Rejected login for {Username}: authentication failure.", username);
                return AuthenticationResult.Failed;
            }

            // Verify that the account is not already logged in.
            if (loginTracker.GetLoginState(id) != AccountLoginState.NotLoggedIn)
            {
                // Already logged in, log and reject.
                logger.LogInformation("Rejected login for {Username}: already logged in.", username);
                return AuthenticationResult.AlreadyLoggedIn;
            }

            // Login successful.
            loginTracker.Login(id);
            loginHandoffTracker.AddPendingHandoff(id);
            secret = sharedSecretManager.AddSecret(id);
            guid = id;

            logger.LogInformation("Login successful for {Username}.", username);

            return AuthenticationResult.Successful;
        }
        catch (Exception e)
        {
            logger.LogError("Error while handling login request for " + username + "; rejecting.", e);
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

            logger.LogInformation("New account registered for {Username}.", username);

            return RegistrationResult.Successful;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling registration request.");
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
            result = new Maybe<int>(connectionId);

        return result;
    }

    /// <summary>
    ///     Gets the player (if any) being used by the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerEntityId">Player entity ID. Only valid if the method returns true.</param>
    /// <returns>true if a player was found, false otherwise.</returns>
    public bool TryGetPlayerForAccount(Guid accountId, out ulong playerEntityId)
    {
        return loginTracker.TryGetPlayerForAccountId(accountId, out playerEntityId);
    }

    /// <summary>
    ///     Gets the player entity ID associated with a network connection.
    /// </summary>
    /// <param name="connectionId">Connection ID.</param>
    /// <returns>Player entity ID if the connection is associated with a player.</returns>
    public Maybe<ulong> GetPlayerForConnectionId(int connectionId)
    {
        var result = new Maybe<ulong>();
        if (loginTracker.TryGetPlayerForConnectionId(connectionId, out var playerEntityId))
            result = new Maybe<ulong>(playerEntityId);
        return result;
    }

    /// <summary>
    ///     Checks whether a player is in cooldown and therefore cannot log in.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <returns>true if in cooldown, false otherwise.</returns>
    public bool IsPlayerInCooldown(ulong playerEntityId)
    {
        return loginTracker.IsPlayerInCooldown(playerEntityId);
    }
}