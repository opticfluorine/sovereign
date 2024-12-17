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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sovereign.ScriptingGenerators;

/// <summary>
///     Source generator for generating support classes for the Lua entity builder bindings.
/// </summary>
[Generator]
public class LuaEntityBuilderSupportGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName("Sovereign.EngineUtil.Attributes.ScriptableEntityBuilderAction",
                static (context, _) => context is MethodDeclarationSyntax,
                static (context, cToken) =>
                {
                    var sym = context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken);
                    if (sym is not IMethodSymbol methodSym) throw new Exception("not a method symbol");

                    var key = methodSym.GetAttributes()
                        .Where(a => a.AttributeClass!.Name == "ScriptableEntityBuilderAction")
                        .Select(a => a.ConstructorArguments[0].Value)
                        .OfType<string>()
                        .First();

                    if (methodSym.Parameters.Length != 1) throw new Exception("must be one parameter");
                    var paramTypeSym = methodSym.Parameters[0].Type;
                    var paramFullNs = SyntaxUtil.GetFullNamespace(paramTypeSym.ContainingNamespace);
                    var isExternalType = paramFullNs.StartsWith("Sovereign");
                    var paramMarshallerAssembly = isExternalType
                        ? "Sovereign.EngineCore"
                        : SyntaxUtil.GetSovereignAssembly(paramFullNs);

                    return new Model
                    {
                        Key = key,
                        ParamTypeName = paramTypeSym.Name,
                        ParamTypeFullNs = paramFullNs,
                        ParamMarshallerAssembly = paramMarshallerAssembly
                    };
                })
            .Collect()
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(pipeline,
            static (context, details) => GenerateSource(context, details.Left, details.Right));
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<Model> models,
        Compilation compilation)
    {
        if (models.Length == 0) return;

        var sb = new StringBuilder();

        sb.Append($@"
            using System;

            namespace {compilation.AssemblyName!}.Lua;

            public enum LuaEntitySpecKey
            {{");

        foreach (var model in models)
            sb.Append($@"
                {model.Key},");

        sb.Append(@"
            }

            public static class LuaEntityBuilderSupport
            {

                public static void HandleKeyValuePair(IntPtr luaState, LuaEntitySpecKey key)
                PP
                
                }
            }
        ");

        context.AddSource("LuaEntityBuilderSupport.g.cs", sb.ToString());
    }

    private record struct Model
    {
        public string Key { get; set; }
        public string ParamTypeName { get; set; }
        public string ParamTypeFullNs { get; set; }
        public string ParamMarshallerAssembly { get; set; }
    }
}