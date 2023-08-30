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
using Castle.Core.Logging;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.Accounts.Accounts.Registration;

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
}