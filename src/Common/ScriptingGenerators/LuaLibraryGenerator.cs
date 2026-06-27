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
    private const string ScriptableCallbackName = "ScriptableCallback";
    private const string ScriptableOutputBufferName = "ScriptableOutputBuffer";
    private const string DefaultMarshaller = "Sovereign.EngineCore.Lua.LuaMarshaller";

    private static readonly List<string> predefinedTypes =
    [
        "System.Int64", "System.UInt64", "System.Int32", "System.UInt32", "System.Int16", "System.UInt16",
        "System.Byte", "System.Single", "System.Boolean", "System.String", "Numerics.Vector3", "System.Guid"
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // This pipeline generates a series of input values, one per generated library,
        // each containing the library information and its functions.

        // Start by gathering all the [ScriptableLibrary] classes.
        var libraryPipeline = context.SyntaxProvider
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
                        LibraryNamespace = SyntaxUtil.GetFullNamespace(clsSymbol.ContainingNamespace)
                    };
                });

        var pipeline = libraryPipeline
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
                                .Zip(Enumerable.Range(0, function.Parameters.Length), (p, i) => (p, i))
                                .Select(GetParameterModel)
                                .OrderByDescending(p => p.IsOutputBuffer);

                            if (function.ReturnType is not INamedTypeSymbol returnType)
                                throw new Exception("return type has no name");

                            var paramModels = parameters.ToList();
                            var hasOutputBuffer = paramModels.Any(p => p.IsOutputBuffer);

                            var fnModel = new FunctionModel
                            {
                                FunctionName = GetFunctionName(function),
                                MethodName = function.Name,
                                LibraryClass = GetClassForFunction(function),
                                ParameterModels = new ValueEquatableList<ParameterModel>(paramModels),
                                ReturnTypeName =
                                    $"{SyntaxUtil.GetFullNamespace(function.ReturnType.ContainingNamespace)}.{function.ReturnType.Name}",
                                ReturnTypeMarshallerClass = GetMarshallerClass(returnType),
                                HasOutputBuffer = hasOutputBuffer,
                                OutputBufferName = hasOutputBuffer
                                    ? paramModels.First(p => p.IsOutputBuffer).Name
                                    : string.Empty
                            };

                            if (fnModel.HasOutputBuffer && fnModel.ReturnTypeName != "System.Void")
                                throw new Exception(
                                    $"Function {fnModel.FunctionName} has output buffer but also a non-void return type.");
                            if (paramModels.Count(p => p.IsOutputBuffer) > 1)
                                throw new Exception($"Function {fnModel.FunctionName} has multiple output buffers.");
                            if (paramModels.Any(p => p.IsOutputBuffer && !p.TypeName.StartsWith("System.Span")))
                                throw new Exception(
                                    $"Function {fnModel.FunctionName} has output buffer that is not a Span type.");

                            return fnModel;
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

        context.RegisterSourceOutput(libraryPipeline.Collect().Combine(context.CompilationProvider),
            static (context, details) => GenerateServiceCollectionSource(context, details.Left, details.Right));
    }

    /// <summary>
    ///     Extracts a model for the given parameter.
    /// </summary>
    /// <param name="pdata">Parameter data.</param>
    /// <returns>Parameter model.</returns>
    private static ParameterModel GetParameterModel((IParameterSymbol param, int index) pdata)
    {
        var (param, index) = pdata;
        if (param.Type is not INamedTypeSymbol typeSymbol) throw new Exception("parameter type not named");

        var outputBufferAttr = param.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass!.Name == ScriptableOutputBufferName);

        return new ParameterModel
        {
            Index = index,
            Name = param.Name,
            TypeName = $"{SyntaxUtil.GetFullNamespace(typeSymbol.ContainingNamespace)}.{typeSymbol.Name}",
            MarshallerClass = GetMarshallerClass(typeSymbol),
            IsRefCallback = param.GetAttributes().Any(a => a.AttributeClass!.Name == ScriptableCallbackName),
            IsLuaState = typeSymbol.Name == "IntPtr" && param.Name == "luaState",
            IsOutputBuffer = outputBufferAttr != null,
            OutputBufferSizeHintFn = outputBufferAttr != null
                ? (string)outputBufferAttr.ConstructorArguments.First().Value!
                : string.Empty,
            IsGeneric = typeSymbol.IsGenericType,
            GenericT = typeSymbol.TypeArguments
                .Select(t => $"{SyntaxUtil.GetFullNamespace(t.ContainingNamespace)}.{t.Name}").ToArray(),
            IsTUnmanaged = typeSymbol.TypeArguments.Select(t => t.IsUnmanagedType).ToArray()
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
            using Microsoft.Extensions.Logging;
            using Sovereign.Scripting.Lua;
            using Sovereign.EngineCore.Systems.Scripting;
            using static Sovereign.Scripting.Lua.LuaBindings;

            namespace {libraryModel.LibraryNamespace};

            /// <summary>
            ///     Autogenerated Lua binding.
            /// </summary>
            class {libraryModel.LibraryShortClass}LuaLibrary : ILuaLibrary
            {{
                private readonly {libraryModel.LibraryShortClass} _managedLibrary;
                private readonly ILogger logger;
                private readonly IScriptingServices scriptingServices;
                private const int StackLimit = 128;

                public {libraryModel.LibraryShortClass}LuaLibrary({libraryModel.LibraryShortClass} nativeLibrary, 
                    ILogger<{libraryModel.LibraryShortClass}LuaLibrary> logger,
                    IScriptingServices scriptingServices)
                {{
                    _managedLibrary = nativeLibrary;
                    this.logger = logger;
                    this.scriptingServices = scriptingServices;
                }}

                public void Install(LuaHost luaHost)
                {{
                    luaHost.BeginLibrary(""{libraryModel.LibraryName}"");");

        foreach (var function in functionModels)
            sb.Append($@"
                    luaHost.AddLibraryFunction(""{function.FunctionName}"", {function.MethodName}_Lua);");

        sb.Append(@"
                    luaHost.EndLibrary();
                }
        ");

        foreach (var function in functionModels)
        {
            var reqArgCount = function.ParameterModels.List.Count(p => !p.IsLuaState);
            sb.Append($@"
                public int {function.MethodName}_Lua(IntPtr luaState)
                {{
                    int returnCount = 0;
                    try {{
                        if (lua_gettop(luaState) != {reqArgCount})
                            throw new LuaException(""{libraryModel.LibraryName}.{function.MethodName} requires {reqArgCount} arguments."");");

            for (var i = function.ParameterModels.List.Count - 1; i >= 0; --i)
            {
                var param = function.ParameterModels.List[i];
                if (param.IsRefCallback)
                {
                    sb.Append($@"
                        if (!lua_isfunction(luaState, -1)) throw new LuaException(""callback must be a function"");
                        int {param.Name} = luaL_ref(luaState, LUA_REGISTRYINDEX);");
                }
                else if (param.IsOutputBuffer)
                {
                    sb.Append($@"
                        var {param.Name}_sz = _managedLibrary.{param.OutputBufferSizeHintFn};");
                    if (param.IsTUnmanaged[0])
                    {
                        // Unmanaged types can be stackalloc'd to avoid the heap.
                        sb.Append($@"
                        {param.TypeName}<{param.GenericT[0]}> {param.Name} = {param.Name}_sz > StackLimit ? new(new {param.GenericT[0]}[{param.Name}_sz]) : stackalloc {param.GenericT[0]}[{param.Name}_sz];
                        if ({param.Name}_sz <= StackLimit) {param.Name}.Clear();");
                    }
                    else
                    {
                        // Managed types have to go on the heap.
                        sb.Append($@"
                        {param.TypeName}<{param.GenericT[0]}> {param.Name} = new(new {param.GenericT[0]}[{param.Name}_sz]);");
                    }
                }
                else if (!param.IsLuaState)
                {
                    sb.Append($@"
                        {param.MarshallerClass}.Unmarshal(luaState, out {param.TypeName} {param.Name});");
                }
            }

            var capture = function.ReturnTypeName == "System.Void" ? "" : "var result = ";

            sb.Append($@"
                        {capture}_managedLibrary.{function.MethodName}(");

            var paramsCallOrder = function.ParameterModels.List.OrderBy(p => p.Index).ToList();
            for (var i = 0; i < paramsCallOrder.Count; ++i)
            {
                var param = paramsCallOrder[i];
                var delim = i < paramsCallOrder.Count - 1 ? ", " : "";
                sb.Append($"{param.Name}{delim}");
            }

            sb.Append(");");

            if (function.ReturnTypeName != "System.Void")
            {
                sb.Append($@"
                        // return type = {function.ReturnTypeName}
                        returnCount = {function.ReturnTypeMarshallerClass}.Marshal(luaState, result);");
            }
            else if (function.HasOutputBuffer)
            {
                // TODO handle return from output buffer
                sb.Append($@"
                        // return output buffer
                        returnCount = {DefaultMarshaller}.Marshal(luaState, {function.OutputBufferName});");
            }

            sb.Append(@"
                    }
                    catch (LuaException e)
                    {
                        var mainState = LuaUtil.GetMainThread(luaState);
                        scriptingServices.GetScriptLogger(mainState, logger).LogError(e.Message);
                        return 0;
                    }
                    catch (Exception e)
                    {
                        var mainState = LuaUtil.GetMainThread(luaState);
                        scriptingServices.GetScriptLogger(mainState, logger)
                            .LogError(e, ""Error when calling function from Lua."");
                        return 0;
                    }
                    return returnCount;
                }
            ");
        }

        sb.Append(@"
            }");

        context.AddSource($"{libraryModel.LibraryShortClass}LuaLibrary.g.cs", sb.ToString());
    }

    /// <summary>
    ///     Generates IServiceCollection extension method for registering all library bindings in this
    ///     assembly with a service collection.
    /// </summary>
    /// <param name="context">Source context.</param>
    /// <param name="models">All library models in assembly.</param>
    /// <param name="compilation">Compilation information.</param>
    private static void GenerateServiceCollectionSource(SourceProductionContext context,
        ImmutableArray<LibraryModel> models, Compilation compilation)
    {
        var assemblyName = compilation.AssemblyName ?? throw new Exception("AssemblyName is null");
        var baseName = assemblyName.Substring(assemblyName.IndexOf('.') + 1);
        var className = $"{baseName}LuaLibraryServiceCollectionExtensions";
        var methodName = $"AddSovereign{baseName}LuaLibraries";

        var sb = new StringBuilder();
        sb.Append($@"
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection.Extensions;
            using Sovereign.Scripting.Lua;
            namespace {assemblyName}.Lua;
            public static class {className}
            {{
                public static IServiceCollection {methodName}(this IServiceCollection services)
                {{");

        foreach (var model in models)
        {
            var fullName = $"{model.LibraryNamespace}.{model.LibraryShortClass}LuaLibrary";
            sb.Append($@"
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<ILuaLibrary, {fullName}>());");
        }

        sb.Append(@"
                    return services;
                }
            }
        ");

        context.AddSource($"{className}.g.cs", sb.ToString());
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
        public bool HasOutputBuffer { get; set; }
        public string OutputBufferName { get; set; }
        public ValueEquatableList<ParameterModel> ParameterModels { get; set; }
    }

    private record struct ParameterModel
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string MarshallerClass { get; set; }
        public bool IsRefCallback { get; set; }
        public bool IsLuaState { get; set; }
        public bool IsOutputBuffer { get; set; }
        public string OutputBufferSizeHintFn { get; set; }
        public bool IsGeneric { get; set; }
        public string[] GenericT { get; set; }
        public bool[] IsTUnmanaged { get; set; }
    }
}