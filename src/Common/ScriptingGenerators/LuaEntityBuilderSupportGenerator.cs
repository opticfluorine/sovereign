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

                    var methodName = methodSym.Name;
                    var unsetMethodName = Regex.Replace(methodName, @"^With", "Without");

                    if (methodSym.Parameters.Length > 1)
                        throw new Exception($"{methodSym.Name}: must be zero or one parameters");
                    if (methodSym.Parameters.Length == 1)
                    {
                        var paramTypeSym = methodSym.Parameters[0].Type;
                        var paramFullNs = SyntaxUtil.GetFullNamespace(paramTypeSym.ContainingNamespace);
                        var isExternalType = !paramFullNs.StartsWith("Sovereign");
                        var paramMarshallerAssembly = isExternalType
                            ? "Sovereign.EngineCore"
                            : SyntaxUtil.GetSovereignAssembly(paramFullNs);

                        return new Model
                        {
                            Key = key,
                            MethodName = methodName,
                            UnsetMethodName = unsetMethodName,
                            IsTag = false,
                            ParamTypeName = paramTypeSym.Name,
                            ParamTypeFullNs = paramFullNs,
                            ParamMarshallerAssembly = paramMarshallerAssembly
                        };
                    }

                    return new Model
                    {
                        Key = key,
                        MethodName = methodName,
                        UnsetMethodName = unsetMethodName,
                        IsTag = true
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
            using Microsoft.Extensions.Logging;
            using Sovereign.EngineCore.Entities;

            namespace {compilation.AssemblyName!}.Lua;

            /// <summary>
            ///     Generated class that supports the Lua bindings for IEntityBuilder.
            /// </summary>
            public static class LuaEntityBuilderSupport
            {{
                /// <summary>
                ///     Processes a key-value pair from an entity builder specification.
                /// </summary>
                /// <param name=""luaState"">Lua state.</param>
                /// <param name=""builder"">Entity builder.</param>
                /// <param name=""localLogger"">Logger associated with the executing script.</param>
                /// <param name=""key"">Current key in entity specification.</param>
                /// <return>true if successful, false if error.</return>
                /// <remarks>
                ///     The value associated with the key should be at the top of the Lua stack.
                ///     This method pops the value from the top of the Lua stack.
                /// </remarks>
                public static bool HandleKeyValuePair(IntPtr luaState, IEntityBuilder builder, ILogger localLogger, string key)
                {{
                    try
                    {{
                        switch (key)
                        {{
                ");

        foreach (var model in models)
            if (model.IsTag)
                sb.Append($@"
                        case ""{model.Key}"":
                            {{
                                Sovereign.EngineCore.Lua.LuaMarshaller.Unmarshal(luaState, out bool isTagSet);
                                if (isTagSet) builder.{model.MethodName}();
                                else builder.{model.UnsetMethodName}();
                                return true;
                            }}
                ");
            else
                sb.Append($@"
                        case ""{model.Key}"":
                            {{
                                {model.ParamMarshallerAssembly}.Lua.LuaMarshaller.Unmarshal(luaState, out {model.ParamTypeFullNs}.{model.ParamTypeName} value);
                                builder.{model.MethodName}(value);
                                return true;
                            }}
                ");

        sb.Append(@"
                        default:
                            localLogger.LogError(""entities.Build: Unrecognized entity specification key '{Key}'."", key);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        localLogger.LogError(e, ""entities.Build: Error reading key '{Key} from entity specification."", key);
                    }

                    return false;
                }
            }");

        context.AddSource("LuaEntityBuilderSupport.g.cs", sb.ToString());
    }

    private record struct Model
    {
        public string Key { get; set; }
        public string MethodName { get; set; }
        public string UnsetMethodName { get; set; }
        public bool IsTag { get; set; }
        public string ParamTypeName { get; set; }
        public string ParamTypeFullNs { get; set; }
        public string ParamMarshallerAssembly { get; set; }
    }
}