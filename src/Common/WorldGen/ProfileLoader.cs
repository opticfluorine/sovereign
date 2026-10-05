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
using System.Linq;
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
    ///     Maximum length of a profile name in characters.
    /// </summary>
    private const int MaxProfileNameLength = 64;

    /// <summary>
    ///     Characters that are rejected in profile names on every platform, so that saved
    ///     profiles remain portable and can never address a path outside the search
    ///     directory.
    /// </summary>
    private static readonly char[] InvalidNameCharacters = Path.GetInvalidFileNameChars()
        .Union(new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' })
        .ToArray();

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

    /// <summary>
    ///     Serializer options for profile serialization: the schema options with indentation
    ///     so that saved profiles remain hand-editable.
    /// </summary>
    private static readonly JsonSerializerOptions SaveSerializerOptions =
        new(SerializerOptions) { WriteIndented = true };

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
        var path = ResolveProfilePath(name);
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

        return Parse(json, path);
    }

    /// <summary>
    ///     Parses a world generation profile from JSON text using the profile schema rules.
    /// </summary>
    /// <param name="json">JSON text of the profile.</param>
    /// <returns>The parsed profile.</returns>
    /// <exception cref="ProfileLoadException">If the JSON is malformed or does not match the profile schema.</exception>
    public WorldGenProfile Parse(string json)
    {
        return Parse(json, null);
    }

    /// <summary>
    ///     Saves a world generation profile as indented JSON, overwriting any existing
    ///     profile with the same name.
    /// </summary>
    /// <param name="name">Profile name, without extension. Must pass
    ///     <see cref="IsValidProfileName" />.</param>
    /// <param name="profile">Profile to save.</param>
    /// <exception cref="ProfileLoadException">If the name is invalid or the profile cannot be written.</exception>
    public void Save(string name, WorldGenProfile profile)
    {
        var path = ResolveProfilePath(name);
        Directory.CreateDirectory(searchDirectory);
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(profile, SaveSerializerOptions) + "\n");
        }
        catch (Exception e)
        {
            throw new ProfileLoadException(
                $"Failed to write world generation profile at {path}: {e.Message}", e);
        }
    }

    /// <summary>
    ///     Parses the JSON of a world generation profile, attributing errors to the given
    ///     path when present.
    /// </summary>
    /// <param name="json">JSON text of the profile.</param>
    /// <param name="path">Path of the profile the JSON came from, or null.</param>
    /// <returns>The parsed profile.</returns>
    /// <exception cref="ProfileLoadException">If the JSON is malformed or does not match the profile schema.</exception>
    private WorldGenProfile Parse(string json, string? path)
    {
        var at = path is null ? "" : $" at {path}";
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });
            return document.Deserialize<WorldGenProfile>(SerializerOptions)
                   ?? throw new ProfileLoadException($"World generation profile{at} is empty.");
        }
        catch (JsonException e)
        {
            throw new ProfileLoadException(
                $"Failed to parse world generation profile {path}{FormatPosition(e)}: {e.Message}", e);
        }
    }

    /// <summary>
    ///     Indicates whether the given profile name is safe to resolve to a profile path.
    ///     Unsafe names include path separators, traversal segments, invalid filename
    ///     characters, control characters, and surrounding whitespace.
    /// </summary>
    /// <param name="name">Profile name to check, without extension.</param>
    /// <returns>true if the name is safe, false otherwise.</returns>
    public static bool IsValidProfileName(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > MaxProfileNameLength) return false;
        if (name != name.Trim()) return false;
        if (name is "." or "..") return false;
        if (name.IndexOfAny(InvalidNameCharacters) >= 0) return false;
        return name.All(c => !char.IsControl(c));
    }

    /// <summary>
    ///     Resolves the file path of the named profile, rejecting any name that does not
    ///     resolve to a file directly inside the search directory.
    /// </summary>
    /// <param name="name">Profile name, without extension.</param>
    /// <returns>Absolute path of the profile file.</returns>
    /// <exception cref="ProfileLoadException">If the name is invalid or escapes the search directory.</exception>
    private string ResolveProfilePath(string name)
    {
        if (!IsValidProfileName(name))
            throw new ProfileLoadException($"Invalid world generation profile name \"{name}\".");

        var directory = Path.GetFullPath(searchDirectory);
        var path = Path.GetFullPath(Path.Combine(directory, $"{name}.json"));
        if (!path.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ProfileLoadException($"Invalid world generation profile name \"{name}\".");

        return path;
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
