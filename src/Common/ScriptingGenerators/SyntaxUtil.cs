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

using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Sovereign.ScriptingGenerators;

/// <summary>
///     Utility functions for getting information from syntax.
/// </summary>
public static class SyntaxUtil
{
    /// <summary>
    ///     Gets the fully qualified name of the given namespace.
    /// </summary>
    /// <param name="namespaceSymbol">Namespace symbol.</param>
    /// <returns>Fully qualified name of namespace.</returns>
    public static string GetFullNamespace(INamespaceSymbol namespaceSymbol)
    {
        var namespaceStack = new Stack<string>();
        var current = namespaceSymbol;
        while (current != null)
        {
            namespaceStack.Push(current.Name);
            current = current.ContainingNamespace;
        }

        var sb = new StringBuilder();
        while (namespaceStack.Count > 0)
        {
            if (sb.Length > 0) sb.Append(".");
            sb.Append(namespaceStack.Pop());
        }

        return sb.ToString();
    }
}