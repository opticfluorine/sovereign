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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sovereign.ScriptingGenerators;

/// <summary>
///     Generates Lua bindings for component collections.
/// </summary>
[Generator]
public class LuaComponentsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName("Sovereign.EngineUtil.Attributes.ScriptableComponents",
                static (context, _) => context is ClassDeclarationSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not
                        INamedTypeSymbol clsSymbol) throw new Exception("not a named type");

                    return new Model
                    {
                        Name = clsSymbol.Name,
                        FullNamespace = SyntaxUtil.GetFullNamespace(clsSymbol.ContainingNamespace),
                        BindingName = $"{clsSymbol.Name}LuaComponents"
                    };
                });

        var fullPipeline = pipeline
            .Collect()
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(pipeline, GenerateBinding);
        context.RegisterSourceOutput(fullPipeline,
            static (context, details) => GenerateServiceCollectionExtensions(context, details.Left, details.Right));
    }

    private static void GenerateBinding(SourceProductionContext context, Model model)
    {
    }

    private static void GenerateServiceCollectionExtensions(SourceProductionContext context,
        ImmutableArray<Model> models, Compilation compilation)
    {
    }

    private record struct Model
    {
        public string Name { get; set; }
        public string FullNamespace { get; set; }
        public string BindingName { get; set; }
    }
}