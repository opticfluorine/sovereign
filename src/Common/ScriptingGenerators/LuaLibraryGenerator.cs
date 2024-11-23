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
    private const string DefaultMarshaller = "Sovereign.EngineCore.Lua.LuaMarshaller";

    private static readonly List<string> predefinedTypes =
    [
        "System.Int64", "System.UInt64", "System.Int32", "System.UInt32", "System.Int16", "System.UInt16",
        "System.Byte", "System.Single", "System.Boolean", "System.String", "System.Numerics.Vector3", "System.Guid"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // This pipeline generates a series of input values, one per generated library,
        // each containing the library information and its functions.

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
                        LibraryShortClass = clsSymbol.Name,
                        LibraryNamespace = GetFullNamespace(clsSymbol.ContainingNamespace)
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

                            if (function.ReturnType is not INamedTypeSymbol returnType)
                                throw new Exception("return type has no name");

                            return new FunctionModel
                            {
                                FunctionName = GetFunctionName(function),
                                MethodName = function.Name,
                                LibraryClass = GetClassForFunction(function),
                                ParameterModels = new ValueEquatableList<ParameterModel>(parameters.ToList()),
                                ReturnTypeName =
                                    $"{GetFullNamespace(function.ReturnType.ContainingNamespace)}.{function.ReturnType.Name}",
                                ReturnTypeMarshallerClass = GetMarshallerClass(returnType)
                            };
                        })
                    .Collect()
            )
            .Select(static (details, _) =>
            {
                // Filter down the set of functions to only those that are part of the current library.
                var matches = details.Right
                    .Where(f => f.LibraryClass == details.Left.LibraryClass);
                return (details.Left, matches.ToImmutableArray());
            });

        context.RegisterSourceOutput(pipeline,
            static (context, details) => GenerateSource(context, details.Left, details.Item2));
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
            TypeName = $"{GetFullNamespace(typeSymbol.ContainingNamespace)}.{typeSymbol.Name}",
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

        var fullTypeName = $"{typeSymbol.ContainingNamespace.Name}.{typeSymbol.Name}";
        if (predefinedTypes.Contains(fullTypeName)) return DefaultMarshaller;

        return $"{typeSymbol.ContainingAssembly.Name}.Lua.LuaMarshaller";
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
    private static void GenerateSource(SourceProductionContext context, LibraryModel libraryModel,
        ImmutableArray<FunctionModel> functionModels)
    {
        var sb = new StringBuilder();

        sb.Append($@"
            using System;
            using Sovereign.Scripting.Lua;

            namespace {libraryModel.LibraryNamespace};

            class {libraryModel.LibraryShortClass}LuaLibrary
            {{
                private {libraryModel.LibraryShortClass} _nativeLibrary;

                public {libraryModel.LibraryShortClass}LuaLibrary({libraryModel.LibraryShortClass} nativeLibrary)
                {{
                    _nativeLibrary = nativeLibrary;
                }}

                public void Install(LuaHost luaHost)
                {{
                    luaHost.StartLibrary(""{libraryModel.LibraryName}"");");

        foreach (var function in functionModels)
            sb.Append($@"
                    luaHost.AddLibraryFunction(""{function.FunctionName}"", {function.MethodName}_Lua);");

        sb.Append(@"
                    luaHost.EndLibrary();
                }
        ");

        foreach (var function in functionModels)
        {
            sb.Append($@"
                public int {function.MethodName}_Lua(IntPtr luaState)
                {{");

            foreach (var param in function.ParameterModels.List)
                sb.Append($@"
                    {param.MarshallerClass}.Unmarshal(luaState, out {param.TypeName} {param.Name});");

            var capture = function.ReturnTypeName == "System.Void" ? "" : "var result = ";

            sb.Append($@"
                    {capture}_nativeLibrary.{function.MethodName}(");

            for (var i = 0; i < function.ParameterModels.List.Count; ++i)
            {
                var param = function.ParameterModels.List[i];
                var delim = i < function.ParameterModels.List.Count - 1 ? ", " : "";
                sb.Append($"{param.Name}{delim}");
            }

            sb.Append(");");

            if (function.ReturnTypeName != "System.Void")
                sb.Append($@"
                    // return type = {function.ReturnTypeName}
                    {function.ReturnTypeMarshallerClass}.Marshal(luaState, result);");

            sb.Append(@"
                    return 0;
                }
            ");
        }

        sb.Append(@"
            }");

        context.AddSource($"{libraryModel.LibraryShortClass}LuaLibrary.g.cs", sb.ToString());
    }

    /// <summary>
    ///     Gets the fully qualified name of the given namespace.
    /// </summary>
    /// <param name="namespaceSymbol">Namespace symbol.</param>
    /// <returns>Fully qualified name of namespace.</returns>
    private static string GetFullNamespace(INamespaceSymbol namespaceSymbol)
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

    /// <summary>
    ///     Model capturing the semantics of a Lua library to be generated.
    /// </summary>
    private record struct LibraryModel
    {
        public string LibraryName { get; set; }
        public string LibraryClass { get; set; }
        public string LibraryShortClass { get; set; }
        public string LibraryNamespace { get; set; }
    }

    /// <summary>
    ///     Model capturing the semantics of a function within a library.
    /// </summary>
    private record struct FunctionModel
    {
        public string FunctionName { get; set; }
        public string MethodName { get; set; }
        public string LibraryClass { get; set; }
        public string ReturnTypeName { get; set; }
        public string ReturnTypeMarshallerClass { get; set; }
        public ValueEquatableList<ParameterModel> ParameterModels { get; set; }
    }

    private record struct ParameterModel
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string MarshallerClass { get; set; }
    }
}