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