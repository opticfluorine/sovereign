// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sovereign.WorldGen;

/// <summary>
///     Loads world generation profiles from JSON files within a search directory.
/// </summary>
public sealed class ProfileLoader
{
    /// <summary>
    ///     Default profile search directory, relative to the current working directory.
    /// </summary>
    public static readonly string DefaultSearchDirectory = Path.Combine("Data", "Worldgen");

    /// <summary>
    ///     Serializer options for profile deserialization. Unknown properties are rejected
    ///     so that schema drift fails loudly instead of silently dropping fields.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    private readonly string searchDirectory;

    /// <summary>
    ///     Creates a loader that searches the default profile directory.
    /// </summary>
    public ProfileLoader() : this(DefaultSearchDirectory)
    {
    }

    /// <summary>
    ///     Creates a loader that searches the given directory for profile files.
    /// </summary>
    /// <param name="searchDirectory">Directory containing profile JSON files.</param>
    public ProfileLoader(string searchDirectory)
    {
        this.searchDirectory = searchDirectory;
    }

    /// <summary>
    ///     Loads and parses the named world generation profile.
    /// </summary>
    /// <param name="name">Profile name, without extension.</param>
    /// <returns>The parsed profile.</returns>
    /// <exception cref="ProfileLoadException">If the profile cannot be found, read, or parsed.</exception>
    public WorldGenProfile Load(string name)
    {
        var path = Path.Combine(searchDirectory, $"{name}.json");
        if (!File.Exists(path))
            throw new ProfileLoadException(
                $"World generation profile \"{name}\" was not found; searched in {searchDirectory}.");

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            throw new ProfileLoadException($"Failed to read world generation profile at {path}: {e.Message}", e);
        }

        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });
            return document.Deserialize<WorldGenProfile>(SerializerOptions)
                   ?? throw new ProfileLoadException($"World generation profile at {path} is empty.");
        }
        catch (JsonException e)
        {
            throw new ProfileLoadException(
                $"Failed to parse world generation profile {path}{FormatPosition(e)}: {e.Message}", e);
        }
    }

    /// <summary>
    ///     Formats the JSON line and column of the given exception, if known.
    /// </summary>
    /// <param name="exception">JSON exception.</param>
    /// <returns>Position suffix, or the empty string if the position is unknown.</returns>
    private static string FormatPosition(JsonException exception)
    {
        return exception.LineNumber is not null && exception.BytePositionInLine is not null
            ? $" at line {exception.LineNumber + 1}, column {exception.BytePositionInLine}"
            : "";
    }
}
