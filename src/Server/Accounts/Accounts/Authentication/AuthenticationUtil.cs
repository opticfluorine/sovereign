// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Provides utility methods for authentication processing.
/// </summary>
public class AuthenticationUtil
{
    /// <summary>
    ///     Securely compares two hashes in a way that is resistant to timing
    ///     attacks.
    /// </summary>
    /// <param name="left">First hash.</param>
    /// <param name="right">Second hash.</param>
    /// <returns>true if equal, false otherwise.</returns>
    /// <remarks>
    ///     Optimizations are disabled for this method to prevent the compiler
    ///     and JITter from re-introducing vulnerability through loop
    ///     optimization.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static bool CompareHashes(byte[] left, byte[] right)
    {
        // PRE: Hash lengths are equal.
        if (left.Length != right.Length)
            throw new ArgumentException("Array length mismatch.", nameof(right));

        // Loop over the entire array, performing a constant time check.
        var check = 0;
        for (var i = 0; i < left.Length; ++i) check += left[i] ^ right[i];
        return check == 0;
    }

    /// <summary>
    ///     Securely compares two UTF-8 encoded strings for equality in a way that is resistant
    ///     to side channel timing attacks.
    /// </summary>
    /// <param name="left">First string.</param>
    /// <param name="right">Second string.</param>
    /// <returns>true if equal, false otherwise.</returns>
    public static bool CompareUtf8Strings(string left, string right)
    {
        // PRE: String lengths are equal.
        // Check this after the transform.

        // Get byte arrays for strings and compare as hashes.
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return CompareHashes(leftBytes, rightBytes);
    }
}