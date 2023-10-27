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

using System.Text.RegularExpressions;

namespace Sovereign.EngineCore.Components.Validators;

/// <summary>
///     Provides validation of name components.
/// </summary>
public class NameComponentValidator
{
    /// <summary>
    ///     Maximum length of a name.
    /// </summary>
    public const int MaxLength = 64;

    /// <summary>
    ///     Regular expression for name validation.
    /// </summary>
    private readonly Regex validNameRegex = new(@"^[\w- ]+$");

    /// <summary>
    ///     Checks the validity of a given name.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(string name)
    {
        return name is { Length: > 0 and <= MaxLength } && validNameRegex.IsMatch(name);
    }
}