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

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Thrown when one or more template names referenced by a world generation profile
///     cannot be resolved against the live template entity set.
/// </summary>
public sealed class WorldGenTemplateResolutionException : Exception
{
    /// <summary>
    ///     Creates a resolution exception.
    /// </summary>
    /// <param name="message">Consolidated error message naming every missing template.</param>
    public WorldGenTemplateResolutionException(string message) : base(message)
    {
    }
}
