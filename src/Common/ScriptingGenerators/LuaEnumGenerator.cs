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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sovereign.ScriptingGenerators;

/// <summary>
///     Generates Lua bindings for enums with the [ScriptableEnum] attribute.
/// </summary>
[Generator]
public class LuaEnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName("Sovereign.EngineUtil.Attributes.ScriptableEnum",
                static (ctx, _) => ctx is EnumDeclarationSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken)
                        is not INamedTypeSymbol sym) throw new Exception("Not named symbol.");

                    var name = sym.Name;
                    var constants = new List<ConstantModel>();

                    foreach (var cSym in sym.GetMembers())
                    {
                        if (cSym is not IFieldSymbol fieldSym) continue;

                        constants.Add(new ConstantModel
                        {
                            Name = fieldSym.Name,
                            Value = (int)fieldSym.ConstantValue!
                        });
                    }

                    return new Model
                    {
                        Name = name,
                        Constants = new ValueEquatableList<ConstantModel>(constants)
                    };
                })
            .Collect()
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(pipeline, (ctx, details) => Generate(ctx, details.Left, details.Right));
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<Model> models, Compilation compilation)
    {
        var bindingSrc = GenerateBindings(models, compilation);
        var installerSrc = GenerateServiceCollectionExtensions(compilation);

        context.AddSource("EnumsLuaLibrary.g.cs", bindingSrc);
        context.AddSource("LuaEnumsServiceCollectionExtensions.g.cs", installerSrc);
    }

    private static string GenerateBindings(ImmutableArray<Model> models, Compilation compilation)
    {
        var sb = new StringBuilder();

        sb.Append($@"
            using System;
            using Sovereign.Scripting.Lua;

            namespace {compilation.AssemblyName}.Lua;

            /// <summary>
            ///     Generated Lua bindings for enums in {compilation.AssemblyName}.
            /// </summary>
            public sealed class EnumsLuaLibrary : ILuaLibrary
            {{
                public void Install(LuaHost luaHost)
                {{");

        foreach (var model in models)
        {
            sb.Append($@"
                    luaHost.BeginLibrary(""{model.Name}"");
                    try
                    {{");

            foreach (var constant in model.Constants.List)
                sb.Append($@"
                        luaHost.AddLibraryConstant(""{constant.Name}"", {constant.Value});");

            sb.Append(@"
                    }
                    finally
                    {
                        luaHost.EndLibrary();
                    }
            ");
        }

        sb.Append(@"
                }
            }");

        return sb.ToString();
    }

    private static string GenerateServiceCollectionExtensions(Compilation compilation)
    {
        var sb = new StringBuilder();

        var assemblyName = compilation.AssemblyName;
        var shortName = assemblyName!.Remove(assemblyName.IndexOf('.'), 1);

        sb.Append($@"
            using System;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection.Extensions;
            using Sovereign.Scripting.Lua;

            namespace {assemblyName}.Lua;

            public static class LuaEnumServiceCollectionExtensions
            {{
                /// <summary>
                ///     Adds enum bindings for Lua from the {assemblyName} assembly.
                /// </summary>
                public static IServiceCollection Add{shortName}LuaEnums(this IServiceCollection services)
                {{
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<ILuaLibrary, EnumsLuaLibrary>());
                    return services;
                }}
            }}");

        return sb.ToString();
    }

    private record struct Model
    {
        public string Name { get; set; }
        public ValueEquatableList<ConstantModel> Constants { get; set; }
    }

    private record struct ConstantModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}