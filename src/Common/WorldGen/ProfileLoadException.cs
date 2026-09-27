// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

namespace Sovereign.WorldGen;

/// <summary>
///     Exception thrown when a world generation profile cannot be found, read, or parsed.
/// </summary>
public class ProfileLoadException : Exception
{
    /// <summary>
    ///     Creates a profile load exception with the given message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public ProfileLoadException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Creates a profile load exception with the given message and inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    public ProfileLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
