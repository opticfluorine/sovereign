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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sovereign.ScriptingGenerators;

/// <summary>
///     Source generator that produces bindings of C# classes as Lua libraries.
/// </summary>
[Generator]
public class LuaLibraryGenerator : IIncrementalGenerator
{
    private const string ScriptableLibraryFullName = "Sovereign.EngineUtil.Attributes.ScriptableLibrary";
    private const string ScriptableLibraryName = "ScriptableLibrary";
    private const string ScriptableFunctionFullName = "Sovereign.EngineUtil.Attributes.ScriptableFunction";
    private const string ScriptableFunctionName = "ScriptableFunction";
    private const string DefaultMarshaller = "Sovereign.EngineUtil.LuaMarshaller";

    private static readonly List<string> predefinedTypes =
    [
        "long", "ulong", "int", "uint", "short", "ushort", "byte", "float", "bool", "string",
        "System.Numerics.Vector3", "System.Guid"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // This pipeline generates a series of input values, one per generated library,
        // each containing the library information, its functions, and the compilation information.

        // Start by gathering all the [ScriptableLibrary] classes.
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName(ScriptableLibraryFullName,
                static (context, _) => context is ClassDeclarationSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not
                        INamedTypeSymbol clsSymbol) throw new Exception("not a named type");

                    return new LibraryModel
                    {
                        LibraryName = GetLibraryName(clsSymbol),
                        LibraryClass = GetClassForLibrary(clsSymbol),
                        LibraryShortClass = clsSymbol.Name
                    };
                })
            .Combine(
                // Pair each library with an array of *all* functions. (Will be narrowed down later.)
                context.SyntaxProvider
                    .ForAttributeWithMetadataName(ScriptableFunctionFullName,
                        static (context, _) => context is MethodDeclarationSyntax,
                        static (context, cToken) =>
                        {
                            if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not
                                IMethodSymbol function) throw new Exception("not a method");

                            var parameters = function.Parameters
                                .Select(GetParameterModel);

                            return new FunctionModel
                            {
                                FunctionName = GetFunctionName(function),
                                LibraryClass = GetClassForFunction(function),
                                ParameterModels = new ValueEquatableList<ParameterModel>(parameters.ToList())
                            };
                        })
                    .Collect()
            )
            .Select(static (details, cToken) =>
            {
                // Filter down the set of functions to only those that are part of the current library.
                var matches = details.Right
                    .Where(f => f.LibraryClass == details.Left.LibraryClass);
                return (details.Left, matches.ToImmutableArray());
            })
            // Finally combine with compilation details.
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(pipeline,
            static (context, details) => GenerateSource(context, details.Left.Left, details.Left.Item2, details.Right));
    }

    /// <summary>
    ///     Extracts a model for the given parameter.
    /// </summary>
    /// <param name="param">Parameter.</param>
    /// <returns>Parameter model.</returns>
    private static ParameterModel GetParameterModel(IParameterSymbol param)
    {
        if (param.Type is not INamedTypeSymbol typeSymbol) throw new Exception("parameter type not named");

        return new ParameterModel
        {
            Name = param.Name,
            MarshallerClass = GetMarshallerClass(typeSymbol)
        };
    }

    /// <summary>
    ///     Gets the full name of the marshaller class for the given type.
    /// </summary>
    /// <param name="typeSymbol">Type symbol.</param>
    /// <returns>Full name of corresponding marshaller class.</returns>
    private static string GetMarshallerClass(INamedTypeSymbol typeSymbol)
    {
        // Screen out fundamental types with no namespaces.
        if (typeSymbol.ContainingNamespace == null) return DefaultMarshaller;

        var fullTypeName = $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
        if (predefinedTypes.Contains(fullTypeName)) return DefaultMarshaller;

        return $"{typeSymbol.ContainingAssembly}.LuaMarshaller";
    }

    /// <summary>
    ///     Gets the library name argument from the library class symbol.
    /// </summary>
    /// <param name="library">Library class symbol.</param>
    /// <returns>Library name.</returns>
    private static string GetLibraryName(INamedTypeSymbol library)
    {
        return library
            .GetAttributes()
            .Where(a => a.AttributeClass != null && a.AttributeClass.Name.Equals(ScriptableLibraryName))
            .Where(a => a.ConstructorArguments.Length > 0)
            .Select(a => a.ConstructorArguments.First().Value)
            .OfType<string>()
            .Append(library.Name)
            .First();
    }

    /// <summary>
    ///     Gets the Lua function name argument from the function method symbol.
    /// </summary>
    /// <param name="function">Function method symbol.</param>
    /// <returns>Lua function name.</returns>
    private static string GetFunctionName(IMethodSymbol function)
    {
        return function
            .GetAttributes()
            .Where(a => a.AttributeClass != null && a.AttributeClass.Name == ScriptableFunctionName)
            .Where(a => a.ConstructorArguments.Length > 0)
            .Select(a => a.ConstructorArguments.First().Value)
            .OfType<string>()
            .Append(function.Name)
            .First();
    }

    /// <summary>
    ///     Gets the full name of the class associated with the given library.
    /// </summary>
    /// <param name="library">Library.</param>
    /// <returns>Full name of the library class.</returns>
    private static string GetClassForLibrary(INamedTypeSymbol library)
    {
        return $"{library.ContainingNamespace.Name}.{library.Name}";
    }

    /// <summary>
    ///     Gets the full name of the library class associated with the given function.
    /// </summary>
    /// <param name="function">Function.</param>
    /// <returns>Full name of the library class.</returns>
    private static string GetClassForFunction(IMethodSymbol function)
    {
        return GetClassForLibrary(function.ContainingType);
    }

    /// <summary>
    ///     Generates source code for a single [ScriptableLibrary] binding.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <param name="libraryModel">Library model.</param>
    /// <param name="functionModels">Function models belonging to the library.</param>
    /// <param name="compilation">Compilation details.</param>
    private static void GenerateSource(SourceProductionContext context, LibraryModel libraryModel,
        ImmutableArray<FunctionModel> functionModels, Compilation compilation)
    {
        var sb = new StringBuilder();

        context.AddSource($"{libraryModel.LibraryShortClass}LuaLibrary.g.cs", sb.ToString());
    }

    /// <summary>
    ///     Model capturing the semantics of a Lua library to be generated.
    /// </summary>
    private record struct LibraryModel
    {
        public string LibraryName { get; set; }
        public string LibraryClass { get; set; }
        public string LibraryShortClass { get; set; }
    }

    /// <summary>
    ///     Model capturing the semantics of a function within a library.
    /// </summary>
    private record struct FunctionModel
    {
        public string FunctionName { get; set; }
        public string LibraryClass { get; set; }
        public string ReturnTypeName { get; set; }
        public string ReturnTypeMarshallerClass { get; set; }
        public ValueEquatableList<ParameterModel> ParameterModels { get; set; }
    }

    private record struct ParameterModel
    {
        public string Name { get; set; }
        public string MarshallerClass { get; set; }
    }
}