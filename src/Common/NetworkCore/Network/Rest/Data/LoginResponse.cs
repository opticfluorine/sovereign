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

namespace Sovereign.NetworkCore.Network.Rest.Data
{

    /// <summary>
    /// Login response data record.
    /// </summary>
    public class LoginResponse
    {

        /// <summary>
        /// Human-readable string describing the result of the login attempt.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// User ID. Used for connection handoff.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Shared secret between the client and server.
        /// </summary>
        public string SharedSecret { get; set; }

        /// <summary>
        /// Hostname of the server to connect to.
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        /// Port of the server to connect to.
        /// </summary>
        public ushort ServerPort { get; set; }

    }

}