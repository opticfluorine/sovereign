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

namespace SovereignClientMcp;

/// <summary>
///     Raised when a client debug operation fails and the failure should be
///     surfaced to the running Lua script.
/// </summary>
/// <param name="message">Error message.</param>
/// <param name="innerException">Optional inner exception.</param>
public sealed class DebugClientException(string message, Exception? innerException = null)
    : Exception(message, innerException);
