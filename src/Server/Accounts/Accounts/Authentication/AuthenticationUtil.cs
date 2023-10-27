// Sovereign Engine
// Copyright (c) 2023 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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