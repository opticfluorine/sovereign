/*
 * Sovereign Engine
 * Copyright (c) 2026 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Sovereign.Persistence.Database;

/// <summary>
///     Locates incremental migration scripts on the filesystem.
/// </summary>
public sealed class MigrationScriptLocator
{
    /// <summary>
    ///     Default directory containing migration scripts, relative to the working directory.
    /// </summary>
    public const string DefaultMigrationsDirectory = "Migrations";

    /// <summary>
    ///     Pattern matching a migration directory name, e.g. "0002-SegmentFlags".
    /// </summary>
    private static readonly Regex DirectoryPattern = new(@"^(\d+)-", RegexOptions.Compiled);

    private readonly string migrationsDirectory;

    /// <summary>
    ///     Creates a locator rooted at the default migrations directory.
    /// </summary>
    public MigrationScriptLocator() : this(DefaultMigrationsDirectory)
    {
    }

    /// <summary>
    ///     Creates a locator rooted at the given migrations directory.
    /// </summary>
    /// <param name="migrationsDirectory">Directory containing migration subdirectories.</param>
    public MigrationScriptLocator(string migrationsDirectory)
    {
        this.migrationsDirectory = migrationsDirectory;
    }

    /// <summary>
    ///     Finds all migration scripts with a level greater than the given applied level, sorted by level.
    /// </summary>
    /// <param name="appliedLevel">Highest migration level already applied.</param>
    /// <returns>Pending migration scripts in ascending level order.</returns>
    public IReadOnlyList<MigrationScript> GetPendingScripts(int appliedLevel)
    {
        var scripts = new List<MigrationScript>();
        if (!Directory.Exists(migrationsDirectory)) return scripts;

        foreach (var directory in Directory.EnumerateDirectories(migrationsDirectory))
        {
            var name = Path.GetFileName(directory);
            var match = DirectoryPattern.Match(name);
            if (!match.Success) continue;

            var level = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            if (level <= appliedLevel) continue;

            var filename = Path.Combine(directory, $"{name}_sqlite.sql");
            if (!File.Exists(filename)) continue;

            scripts.Add(new MigrationScript(level, filename));
        }

        scripts.Sort((a, b) => a.Level.CompareTo(b.Level));
        return scripts;
    }
}

/// <summary>
///     A single migration script with its parsed level.
/// </summary>
/// <param name="Level">Migration level parsed from the directory name.</param>
/// <param name="Filename">Full path to the migration SQL script.</param>
public readonly record struct MigrationScript(int Level, string Filename);
