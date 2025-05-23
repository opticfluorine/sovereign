// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System.Diagnostics.CodeAnalysis;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Components.Validators;

/// <summary>
///     Validator for Shadow-valued components.
/// </summary>
public class ShadowComponentValidator
{
    /// <summary>
    ///     Tests the validity of a Shadow object.
    /// </summary>
    /// <param name="shadow">Shadow.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid([NotNullWhen(true)] Shadow? shadow)
    {
        return shadow.HasValue && shadow.Value.Radius >= 0.0f;
    }
}