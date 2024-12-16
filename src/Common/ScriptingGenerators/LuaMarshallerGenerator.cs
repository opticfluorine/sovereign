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
///     Generates code for marshalling and unmarshalling objects between C# and Lua.
/// </summary>
[Generator]
public class LuaMarshallerGenerator : IIncrementalGenerator
{
    private const string ScriptableName = "Sovereign.EngineUtil.Attributes.Scriptable";
    private const string ScriptableOrderName = "ScriptableOrder";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName(ScriptableName,
                static (syntaxNode, _) =>
                    syntaxNode is ClassDeclarationSyntax || syntaxNode is StructDeclarationSyntax ||
                    syntaxNode is EnumDeclarationSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not INamedTypeSymbol sym)
                        throw new Exception("Bad symbol.");

                    var name = sym.ToString();
                    var isEnum = sym.EnumUnderlyingType != null;

                    // Identify all public fields and readable properties.
                    var memberFields = sym.GetMembers()
                        .Where(HasScriptableOrder)
                        .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                        .Where(m => m is IFieldSymbol f && !f.IsConst)
                        .Select(m => (IFieldSymbol)m);
                    var memberProperties = sym.GetMembers()
                        .Where(HasScriptableOrder)
                        .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                        .Where(m => m is IPropertySymbol p
                                    && p.GetMethod != null
                                    && p.GetMethod.DeclaredAccessibility == Accessibility.Public
                                    && p.SetMethod != null
                                    && p.SetMethod.DeclaredAccessibility == Accessibility.Public)
                        .Select(m => (IPropertySymbol)m);
                    var allToMarshall = memberFields.Select(FieldToDataModel)
                        .Union(memberProperties.Select(PropertyToDataModel))
                        .OrderBy(dm => dm.Index)
                        .ToList();

                    return new Model
                    {
                        Name = name ?? throw new Exception("null name"),
                        IsEnum = isEnum,
                        DataModels = new ValueEquatableList<DataModel>(allToMarshall)
                    };
                })
            .Collect()
            .Combine(context.CompilationProvider);

        // Register an incremental source generator fed by this pipeline.
        context.RegisterSourceOutput(pipeline,
            static (context, data) => GenerateSource(context, data.Left, data.Right));
    }

    /// <summary>
    ///     Generates source for the LuaMarshaller class from models extracted from the [Scriptable] classes and structs.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <param name="models">Models.</param>
    /// <param name="compilation">Compilation.</param>
    private static void GenerateSource(SourceProductionContext context, ImmutableArray<Model> models,
        Compilation compilation)
    {
        var sb = new StringBuilder();

        // Convenient constants to reuse.
        const string throwTypeError = @"throw new LuaException(""Top of stack is wrong type."")";

        // Preamble, class definition, marshallers for base types.
        sb.Append($@"
            using System;
            using System.Numerics;
            using Sovereign.Scripting.Lua;
            using static Sovereign.Scripting.Lua.LuaBindings;

            namespace {compilation.AssemblyName}.Lua;

            /// <summary>
            ///     Marshals C# types to and from the Lua stack.
            /// </summary>
            /// <remarks>
            ///     This is a generated class. See LuaMarshallerGenerator for details.
            /// </remarks>
            public static class LuaMarshaller
            {{
                public static int Marshal(IntPtr luaState, long value)
                {{
                    luaL_checkstack(luaState, 1, null);
                    lua_pushinteger(luaState, value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out long value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, ulong value)
                {{
                    luaL_checkstack(luaState, 1, null);
                    lua_pushinteger(luaState, (long)value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out ulong value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (ulong)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, int value)
                {{
                    Marshal(luaState, (long)value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out int value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (int)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, uint value) 
                {{
                    Marshal(luaState, (ulong)value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out uint value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (uint)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, short value) 
                {{
                    Marshal(luaState, (long)value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out short value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (short)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, ushort value)
                {{
                    Marshal(luaState, (ulong)value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out ushort value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (ushort)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, byte value)
                {{
                    Marshal(luaState, (long)value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out byte value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (byte)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, float value)
                {{
                    luaL_checkstack(luaState, 1, null);
                    lua_pushnumber(luaState, value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out float value)
                {{
                    if (!lua_isnumber(luaState, -1)) {throwTypeError};
                    value = lua_tonumber(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, bool value)
                {{
                    luaL_checkstack(luaState, 1, null);
                    lua_pushboolean(luaState, value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out bool value)
                {{
                    if (!lua_isboolean(luaState, -1)) {throwTypeError};
                    value = lua_toboolean(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, string value) 
                {{
                    luaL_checkstack(luaState, 1, null);
                    lua_pushstring(luaState, value);
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out string value)
                {{
                    if (!lua_isstring(luaState, -1)) {throwTypeError};
                    value = lua_tostring(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static int Marshal(IntPtr luaState, Vector3 value)
                {{
                    luaL_checkstack(luaState, 2, null);
                    lua_createtable(luaState, 0, 3);
                    Marshal(luaState, value.X);
                    lua_setfield(luaState, -2, ""x"");
                    Marshal(luaState, value.Y);
                    lua_setfield(luaState, -2, ""y"");
                    Marshal(luaState, value.Z);
                    lua_setfield(luaState, -2, ""z"");
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out Vector3 value)
                {{
                    value = new Vector3();
                    if (!lua_istable(luaState, -1)) {throwTypeError};
                    luaL_checkstack(luaState, 1, null);
                    if (lua_getfield(luaState, -1, ""x"") != LuaType.Number) {throwTypeError};
                    Unmarshal(luaState, out value.X);
                    if (lua_getfield(luaState, -1, ""y"") != LuaType.Number) {throwTypeError};
                    Unmarshal(luaState, out value.Y);
                    if (lua_getfield(luaState, -1, ""z"") != LuaType.Number) {throwTypeError};
                    Unmarshal(luaState, out value.Z);
                }}

                public static int Marshal(IntPtr luaState, Guid value)
                {{
                    luaL_checkstack(luaState, 1, null);
                    Marshal(luaState, value.ToString());
                    return 1;
                }}

                public static void Unmarshal(IntPtr luaState, out Guid value)
                {{
                    Unmarshal(luaState, out string guidString);
                    value = new Guid(guidString);
                }}

            ");

        // Classes and structs.
        foreach (var record in models)
        {
            sb.Append($@"
                public static int Marshal(IntPtr luaState, {record.Name} value)
                {{
                    int pushCount = 0;");

            if (record.IsEnum)
            {
                sb.Append(@"
                    luaL_checkstack(luaState, 1, null);
                    pushCount += Marshal(luaState, (long)value);");
            }
            else
            {
                sb.Append(@"
                    luaL_checkstack(luaState, 2, null);
                    lua_createtable(luaState, 0, 0);
                    pushCount = 1;");
                foreach (var dataModel in record.DataModels.List)
                    sb.Append($@"
                    Marshal(luaState, value.{dataModel.Name});
                    lua_setfield(luaState, -2, ""{dataModel.Name}"");");
            }

            sb.Append($@"
                    return pushCount;
                }}

                public static void Unmarshal(IntPtr luaState, out {record.Name} value)
                {{");
            if (record.IsEnum)
            {
                sb.Append($@"
                    long tmp;
                    Unmarshal(luaState, out tmp);
                    value = ({record.Name})tmp;
                }}
                ");
            }
            else
            {
                sb.Append($@"
                    var tmp = new {record.Name}();
                    luaL_checkstack(luaState, 1, null);
                    if (!lua_istable(luaState, -1)) {throwTypeError};");

                for (var i = record.DataModels.List.Count - 1; i >= 0; --i)
                {
                    var dataModel = record.DataModels.List[i];
                    sb.Append($@"
                    {{
                        lua_getfield(luaState, -1, ""{dataModel.Name}"");
                        Unmarshal(luaState, out {dataModel.NativeType} tval);
                        tmp.{dataModel.Name} = tval;
                    }}
                ");
                }

                sb.Append(@"
                    value = tmp;
                }
                ");
            }
        }

        // End.
        sb.Append(@"
            }
        ");

        // Output generated source.
        context.AddSource("LuaMarshaller.g.cs", sb.ToString());
    }

    /// <summary>
    ///     Checks whether the given symbol has the ScriptableOrder attribute.
    /// </summary>
    /// <param name="symbol">Symbol.</param>
    /// <returns>true if the ScriptableOrder attribute is present, false otherwise.</returns>
    private static bool HasScriptableOrder(ISymbol symbol)
    {
        return symbol
            .GetAttributes()
            .Any(attr => attr.AttributeClass != null &&
                         attr.AttributeClass.Name.Equals(ScriptableOrderName));
    }

    /// <summary>
    ///     Gets the index parameter from the ScriptableOrder attribute decorating the symbol.
    /// </summary>
    /// <param name="symbol">Field or property symbol.</param>
    /// <returns>Index parameter.</returns>
    /// <exception cref="Exception">Thrown if something is malformed (should be prevented by compile errors).</exception>
    private static uint GetScriptableOrder(ISymbol symbol)
    {
        return symbol
            .GetAttributes()
            .Where(attr => attr.AttributeClass != null &&
                           attr.AttributeClass.Name.Equals(ScriptableOrderName))
            .Select(attr =>
                (uint)(attr.ConstructorArguments.First().Value ?? throw new Exception("Unexpected null index.")))
            .First();
    }

    /// <summary>
    ///     Extracts a DataModel from a public field.
    /// </summary>
    /// <param name="symbol">Field symbol.</param>
    /// <returns>DataModel record.</returns>
    private static DataModel FieldToDataModel(IFieldSymbol symbol)
    {
        return new DataModel
        {
            Name = symbol.Name,
            NativeType = symbol.Type.Name,
            Index = GetScriptableOrder(symbol)
        };
    }

    /// <summary>
    ///     Extracts a DataModel from a public property.
    /// </summary>
    /// <param name="symbol">Property symbol.</param>
    /// <returns>DataModel record.</returns>
    private static DataModel PropertyToDataModel(IPropertySymbol symbol)
    {
        return new DataModel
        {
            Name = symbol.Name,
            NativeType = symbol.Type.Name,
            Index = GetScriptableOrder(symbol)
        };
    }

    /// <summary>
    ///     Top-level model which captures the semantics of a single [Scriptable] class or struct.
    /// </summary>
    private record struct Model
    {
        public string Name { get; set; }
        public bool IsEnum { get; set; }
        public ValueEquatableList<DataModel> DataModels { get; set; }
    }

    /// <summary>
    ///     Field/property-level model which captures the semantics of a single [ScriptableOrder] field or property.
    /// </summary>
    private record struct DataModel
    {
        public string Name { get; set; }
        public string NativeType { get; set; }
        public uint Index { get; set; }
    }
}