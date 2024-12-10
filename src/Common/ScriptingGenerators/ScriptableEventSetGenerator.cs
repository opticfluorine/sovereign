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
///     Generates a static class that enumerates the events which a script may react to.
/// </summary>
[Generator]
public class ScriptableEventSetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName("Sovereign.EngineUtil.Attributes.ScriptableEvent",
                static (node, _) => node is EnumMemberDeclarationSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not IFieldSymbol sym)
                        throw new Exception("Not FieldSymbol");

                    return new EventModel
                    {
                        Name = sym.Name,
                        Details = sym.GetAttributes()
                            .Where(a => a.AttributeClass != null && a.AttributeClass.Name == "ScriptableEvent")
                            .Select(a =>
                                a.ConstructorArguments[0].IsNull ? "" : a.ConstructorArguments[0].Value!.ToString())
                            .First()
                    };
                })
            .Collect()
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(pipeline,
            static (context, details) => GenerateSource(context, details.Left, details.Right));
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<EventModel> events,
        Compilation compilation)
    {
        if (events.Length == 0) return;

        var sb = new StringBuilder();

        sb.Append($@"
            using System.Collections.Generic;
            using Sovereign.EngineCore.Events;
            using Sovereign.EngineCore.Events.Details;
            using Sovereign.Scripting.Lua;

            namespace {compilation.AssemblyName}.Lua;

            /// <summary>
            ///     Provides the set of events to which a script may react.
            /// </summary>
            public static class ScriptableEventSet
            {{
                /// <summary>
                ///     Set of events to which a script may react.
                /// </summary>
                public static HashSet<EventId> Events = new HashSet<EventId>()
                {{");

        foreach (var ev in events)
            sb.Append($@"
                    EventId.{ev.Name},");

        sb.Append(@"
                };

                /// <summary>
                ///     Marshals details for the given event.
                /// </summary>
                /// <param name=""luaHost"">Lua host.</param>
                /// <param name=""eventId"">Event ID.</param>
                /// <param name=""details"">Details, or null if none.</param>
                /// <returns>Number of Lua arguments pushed to the stack.</returns>
                public static int MarshalEventDetails(LuaHost luaHost, EventId eventId, IEventDetails details)
                {
                    switch (eventId)
                    {");

        foreach (var ev in events)
            if (ev.Details != "")
                sb.Append($@"
                        case EventId.{ev.Name}:
                            if (details is null) throw new LuaException(""Details are null."");
                            return LuaMarshaller.Marshal(luaHost.LuaState, ({ev.Details})details);");

        sb.Append(@"
                        default:
                            return 0;
                    }
                }
            }
        ");

        context.AddSource("ScriptableEventSet.g.cs", sb.ToString());
    }

    private record struct EventModel
    {
        public string Name { get; set; }
        public string Details { get; set; }
    }
}