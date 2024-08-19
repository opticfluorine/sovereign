// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Security.Cryptography;

namespace Sovereign.UpdaterCore.Updater;

/// <summary>
///     Generates resource hashes for the updater.
/// </summary>
public class UpdaterHash
{
    /// <summary>
    ///     Hashes a file.
    /// </summary>
    /// <param name="fullPath">Full path to file.</param>
    /// <returns>Hash encoded as a hex string.</returns>
    public string Hash(string fullPath)
    {
        using var fs = File.Open(fullPath, FileMode.Open, FileAccess.Read);
        return Convert.ToHexString(SHA512.Create().ComputeHash(fs));
    }
}