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

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sovereign.Scripting.Attributes;
using Sovereign.Scripting.Lua;
using Exception = System.Exception;

namespace Sovereign.Scripting.Generators;

/// <summary>
///     Generates code for marshalling and unmarshalling objects between C# and Lua.
/// </summary>
[Generator]
public class LuaMarshallerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // First define an incremental pipeline that transforms every [Scriptable] class into a model
        // with an ordered list of its [ScriptableOrder] fields and properties.
        var pipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName(typeof(Scriptable).FullName!,
                static (syntaxNode, cToken) =>
                    syntaxNode is ClassDeclarationSyntax || syntaxNode is StructDeclarationSyntax,
                static (context, cToken) =>
                {
                    if (context.SemanticModel.GetDeclaredSymbol(context.TargetNode, cToken) is not INamedTypeSymbol sym)
                        throw new Exception("Bad symbol.");

                    var name = sym.ToString();

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
                        DataModels = new ValueEquatableList<DataModel>(allToMarshall)
                    };
                })
            .Collect();

        // Register an incremental source generator fed by this pipeline.
        context.RegisterSourceOutput(pipeline, GenerateSource);
    }

    /// <summary>
    ///     Generates source for the LuaMarshaller class from models extracted from the [Scriptable] classes and structs.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <param name="models">Models.</param>
    private static void GenerateSource(SourceProductionContext context, ImmutableArray<Model> models)
    {
        var sb = new StringBuilder();

        // Convenient constants to reuse.
        const string throwTypeError = @"throw new LuaException(""Top of stack is wrong type."")";

        // Preamble, class definition, marshallers for base types.
        sb.Append($@"
            using static {typeof(LuaBindings).FullName ?? throw new Exception()};

            namespace Sovereign.Scripting.Lua;

            /// <summary>
            ///     Marshals C# types to and from the Lua stack.
            /// </summary>
            /// <remarks>
            ///     This is a generated class. See LuaMarshallerGenerator for details.
            /// </remarks>
            public static class LuaMarshaller
            {{
                public static void Marshal(IntPtr luaState, long value, bool checkStack = true)
                {{
                    if (checkStack) luaL_checkstack(luaState, 1, null);
                    lua_pushinteger(luaState, value);
                }}

                public static void Unmarshal(IntPtr luaState, out long value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, ulong value, bool checkStack = true)
                {{
                    if (checkStack) luaL_checkstack(luaState, 1, null);
                    lua_pushinteger(luaState, (int)value);
                }}

                public static void Unmarshal(IntPtr luaState, out ulong value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (ulong)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, int value, bool checkStack = true)
                {{
                    Marshal(luaState, (long)value, checkStack);
                }}

                public static void Unmarshal(IntPtr luaState, out int value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (int)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, uint value, bool checkStack = true)
                {{
                    Marshal(luaState, (ulong)value, checkStack);
                }}

                public static void Unmarshal(IntPtr luaState, out uint value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (uint)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, short value, bool checkStack = true)
                {{
                    Marshal(luaState, (long)value, checkStack);
                }}

                public static void Unmarshal(IntPtr luaState, out short value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (short)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, ushort value, bool checkStack = true)
                {{
                    Marshal(luaState, (ulong)value, checkStack);
                }}

                public static void Unmarshal(IntPtr luaState, out ushort value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (ushort)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, byte value, bool checkStack = true)
                {{
                    Marshal(luaState, (long)value, checkStack);
                }}

                public static void Unmarshal(IntPtr luaState, out byte value)
                {{
                    if (!lua_isinteger(luaState, -1)) {throwTypeError};
                    value = (byte)lua_tointeger(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, float value, bool checkStack = true)
                {{
                    if (checkStack) luaL_checkstack(luaState, 1, null);
                    lua_pushnumber(luaState, value);
                }}

                public static void Unmarshal(IntPtr luaState, out float value)
                {{
                    if (!lua_isnumber(luaState, -1)) {throwTypeError};
                    value = lua_tonumber(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, bool value, bool checkStack = true)
                {{
                    if (checkStack) luaL_checkstack(luaState, 1, null);
                    lua_pushboolean(luaState, value);
                }}

                public static void Unmarshal(IntPtr luaState, out bool value)
                {{
                    if (!lua_isboolean(luaState, -1)) {throwTypeError};
                    value = lua_toboolean(luaState, -1);
                    lua_pop(luaState, 1);
                }}

                public static void Marshal(IntPtr luaState, string value, bool checkStack = true)
                {{
                    if (checkStack) luaL_checkstack(luaState, 1, null);
                    lua_pushstring(luaState, value);
                }}

                public static void Unmarshal(IntPtr luaState, out string value)
                {{
                    if (!lua_isstring(luaState, -1)) {throwTypeError};
                    value = lua_tostring(luaState, -1);
                    lua_pop(luaState, 1);
                }}

            ");

        // Classes and structs.

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
                         attr.AttributeClass.Name.Equals(typeof(ScriptableOrder).FullName));
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
                           attr.AttributeClass.Name.Equals(typeof(ScriptableOrder).FullName))
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
    private readonly record struct Model
    {
        public required string Name { get; init; }
        public required ValueEquatableList<DataModel> DataModels { get; init; }
    }

    /// <summary>
    ///     Field/property-level model which captures the semantics of a single [ScriptableOrder] field or property.
    /// </summary>
    private readonly record struct DataModel
    {
        public required string Name { get; init; }
        public required string NativeType { get; init; }
        public required uint Index { get; init; }
    }

    /// <summary>
    ///     Value-equatable wrapper around a generic List.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    private readonly struct ValueEquatableList<T> : IEquatable<ValueEquatableList<T>>
    {
        public List<T> List { get; }

        public ValueEquatableList(List<T> list)
        {
            List = list;
        }

        public bool Equals(ValueEquatableList<T> other)
        {
            return List.SequenceEqual(other.List);
        }

        public override bool Equals(object? obj)
        {
            return obj is ValueEquatableList<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return List.GetHashCode();
        }

        public static bool operator ==(ValueEquatableList<T> left, ValueEquatableList<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ValueEquatableList<T> left, ValueEquatableList<T> right)
        {
            return !left.Equals(right);
        }
    }
}