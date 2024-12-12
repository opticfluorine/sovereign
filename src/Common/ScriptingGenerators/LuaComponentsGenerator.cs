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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                static (context, _) => context is ClassDeclarationSyntax cSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not
                        INamedTypeSymbol clsSymbol) throw new Exception("not a named type");

                    var luaName = clsSymbol.GetAttributes()
                        .Where(a => a.AttributeClass!.Name == "ScriptableComponents")
                        .Select(a => a.ConstructorArguments[0].Value)
                        .OfType<string>()
                        .First();

                    var valueTypeSym = clsSymbol.BaseType!.TypeArguments[0];
                    var valueTypeName = valueTypeSym.Name;
                    var isSystemType = valueTypeSym.ContainingNamespace == null || // intrinsics
                                       valueTypeSym.ContainingNamespace.Name == "System" || // most base types
                                       valueTypeSym.ContainingNamespace.Name == "Numerics"; // vectors/matrices
                    var valueTypeFullNs = SyntaxUtil.GetFullNamespace(valueTypeSym.ContainingNamespace!);
                    var marshallerAssemblyName = isSystemType
                        ? "Sovereign.EngineCore"
                        : Regex.Match(valueTypeFullNs, @"^(Sovereign\..*?)\.").Groups[1].Value;

                    return new Model
                    {
                        Name = clsSymbol.Name,
                        FullNamespace = SyntaxUtil.GetFullNamespace(clsSymbol.ContainingNamespace),
                        BindingName = $"{clsSymbol.Name}LuaComponents",
                        LuaName = luaName,
                        ValueType = valueTypeName,
                        ValueTypeFullNamespace = valueTypeFullNs,
                        MarshallerAssemblyName = marshallerAssemblyName
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
        var sb = new StringBuilder();
        sb.Append($@"
            using System;
            using Microsoft.Extensions.Logging;
            using Sovereign.EngineCore.Components;
            using Sovereign.Scripting.Lua;
            using static Sovereign.Scripting.Lua.LuaBindings;

            namespace {model.FullNamespace};

            /// <summary>
            ///     Lua component collection bindings for {model.Name}.
            /// </summary>
            public class {model.BindingName} : ILuaComponents
            {{
                private readonly {model.Name} components;
                private readonly ILogger logger;

                public {model.BindingName}({model.Name} components, ILogger<{model.BindingName}> logger)
                {{
                    this.components = components;
                    this.logger = logger;
                }}

                public void Install(LuaHost luaHost)
                {{
                    luaL_checkstack(luaHost.LuaState, 2, null);

                    lua_createtable(luaHost.LuaState, 0, 0);

                    lua_pushcfunction(luaHost.LuaState, HasComponentForEntity);
                    lua_setfield(luaHost.LuaState, -2, ""exists"");

                    lua_pushcfunction(luaHost.LuaState, GetComponent);
                    lua_setfield(luaHost.LuaState, -2, ""get"");

                    lua_pushcfunction(luaHost.LuaState, RemoveComponent);
                    lua_setfield(luaHost.LuaState, -2, ""remove"");

                    lua_pushcfunction(luaHost.LuaState, SetComponent);
                    lua_setfield(luaHost.LuaState, -2, ""set"");

                    lua_pushcfunction(luaHost.LuaState, AddComponent);
                    lua_setfield(luaHost.LuaState, -2, ""add"");

                    lua_pushcfunction(luaHost.LuaState, MultiplyComponent);
                    lua_setfield(luaHost.LuaState, -2, ""multiply"");

                    lua_pushcfunction(luaHost.LuaState, DivideComponent);
                    lua_setfield(luaHost.LuaState, -2, ""divide"");

                    lua_pushcfunction(luaHost.LuaState, SetVelocityComponent);
                    lua_setfield(luaHost.LuaState, -2, ""set_velocity"");

                    lua_pushcfunction(luaHost.LuaState, AddPositionComponent);
                    lua_setfield(luaHost.LuaState, -2, ""add_position"");

                    lua_setfield(luaHost.LuaState, -2, ""{model.LuaName}"");
                }}

                private int HasComponentForEntity(IntPtr luaState)
                {{
                    var result = false;

                    try 
                    {{
                        var argCount = lua_gettop(luaState);
                        if (argCount != 1) throw new LuaException(""Must be called with one argument."");
                        if (!lua_isinteger(luaState, 1)) throw new LuaException(""First argument must be integer."");

                        var entityId = (ulong)lua_tointeger(luaState, 1);

                        result = components.HasComponentForEntity(entityId);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.exists()."");
                    }}

                    luaL_checkstack(luaState, 1, null);
                    lua_pushboolean(luaState, result);
                    return 1;
                }}

                private int GetComponent(IntPtr luaState)
                {{
                    var resultCount = 1;

                    try
                    {{
                        luaL_checkstack(luaState, 1, null);

                        var argCount = lua_gettop(luaState);
                        if (argCount != 1) throw new LuaException(""Must be called with one argument."");
                        if (!lua_isinteger(luaState, 1)) throw new LuaException(""First argument must be integer."");

                        var entityId = (ulong)lua_tointeger(luaState, -1);
                        var value = components[entityId];

                        resultCount = {model.MarshallerAssemblyName}.Lua.LuaMarshaller.Marshal(luaState, value);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.get()."");
                        lua_pushnil(luaState);
                    }}

                    return resultCount;
                }}

                private int RemoveComponent(IntPtr luaState)
                {{
                    try
                    {{
                        var argCount = lua_gettop(luaState);
                        if (argCount != 1) throw new LuaException(""Must be called with one argument."");
                        if (!lua_isinteger(luaState, 1)) throw new LuaException(""First argument must be integer."");

                        var entityId = (ulong)lua_tointeger(luaState, 1);
                        components.RemoveComponent(entityId);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.remove()."");
                    }}
                    return 0;
                }}

                private int SetComponent(IntPtr luaState)
                {{
                    try
                    {{
                        var argCount = lua_gettop(luaState);
                        if (argCount < 2) throw new LuaException(""Too few arguments."");
                        if (!lua_isinteger(luaState, 1)) throw new LuaException(""First argument must be integer."");

                        var entityId = (ulong)lua_tointeger(luaState, 1);
                        {model.ValueTypeFullNamespace}.{model.ValueType} value;
                        {model.MarshallerAssemblyName}.Lua.LuaMarshaller.Unmarshal(luaState, out value);

                        components.AddOrUpdateComponent(entityId, value);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.set()."");
                    }}
                    return 0;
                }}

                private int AddComponent(IntPtr luaState)
                {{
                    try
                    {{
                        ModifyComponent(luaState, ComponentOperation.Add);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.add()."");
                    }}
                    return 0;
                }}

                private int MultiplyComponent(IntPtr luaState)
                {{
                    try
                    {{
                        ModifyComponent(luaState, ComponentOperation.Multiply);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.multiply()."");
                    }}
                    return 0;
                }}

                private int DivideComponent(IntPtr luaState)
                {{
                    try
                    {{
                        ModifyComponent(luaState, ComponentOperation.Divide);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.divide()."");
                    }}
                    return 0;
                }}

                private int SetVelocityComponent(IntPtr luaState)
                {{
                    try
                    {{
                        ModifyComponent(luaState, ComponentOperation.SetVelocity);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.set_velocity()."");
                    }}
                    return 0;
                }}

                private int AddPositionComponent(IntPtr luaState)
                {{
                    try
                    {{
                        ModifyComponent(luaState, ComponentOperation.AddPosition);
                    }}
                    catch (Exception e)
                    {{
                        logger.LogError(e, ""Error in components.{model.LuaName}.add_position()."");
                    }}
                    return 0;
                }}

                private void ModifyComponent(IntPtr luaState, ComponentOperation op)
                {{
                    var argCount = lua_gettop(luaState);
                    if (argCount < 2) throw new LuaException(""Too few arguments."");
                    if (!lua_isinteger(luaState, 1)) throw new LuaException(""First argument must be integer."");

                    var entityId = (ulong)lua_tointeger(luaState, 1);
                    {model.ValueTypeFullNamespace}.{model.ValueType} value;
                    {model.MarshallerAssemblyName}.Lua.LuaMarshaller.Unmarshal(luaState, out value);

                    components.ModifyComponent(entityId, op, value);
                }}
            }}");

        context.AddSource($"{model.BindingName}.g.cs", sb.ToString());
    }

    private static void GenerateServiceCollectionExtensions(SourceProductionContext context,
        ImmutableArray<Model> models, Compilation compilation)
    {
        if (models.Length == 0) return;

        var assemblyName = compilation.AssemblyName!;
        var assemblyShortName = assemblyName.Substring(assemblyName.IndexOf('.') + 1);
        var className = $"{assemblyShortName}LuaComponentsServiceCollectionExtensions";

        var sb = new StringBuilder();
        sb.Append($@"
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection.Extensions;
            using Sovereign.Scripting.Lua;

            namespace {assemblyName}.Lua;

            public static class {className}
            {{

                /// <summary>
                ///     Adds the ILuaComponents services from {assemblyName}.
                /// </summary>
                /// <param name=""services"">Service collection.</param>
                /// <returns>Service collection.</returns>
                public static IServiceCollection AddSovereign{assemblyShortName}LuaComponents(this IServiceCollection services)
                {{");

        foreach (var model in models)
            sb.Append($@"
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<ILuaComponents, {model.FullNamespace}.{model.BindingName}>());");

        sb.Append(@"
                    return services;
                }
            }");

        context.AddSource($"{className}.g.cs", sb.ToString());
    }

    private record struct Model
    {
        public string Name { get; set; }
        public string FullNamespace { get; set; }
        public string BindingName { get; set; }
        public string LuaName { get; set; }
        public string ValueType { get; set; }
        public string ValueTypeFullNamespace { get; set; }
        public string MarshallerAssemblyName { get; set; }
    }
}