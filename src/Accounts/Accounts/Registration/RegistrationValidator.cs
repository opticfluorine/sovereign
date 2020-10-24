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

using Sovereign.Accounts.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Accounts.Accounts.Registration
{

    /// <summary>
    /// Responsible for validating registration-related user input.
    /// </summary>
    public sealed class RegistrationValidator
    {
        private readonly IAccountsConfiguration configuration;

        public RegistrationValidator(IAccountsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Validates registration input.
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

}
