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
using System.Threading;
using Microsoft.Extensions.Options;
using Sovereign.ServerCore.Configuration;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Resolves the world generation scratch directory used for output such as preview images.
///     The directory is taken from configuration when set, defaulting to a fixed subdirectory
///     of the system temporary path; the world data directory is never used.
/// </summary>
public sealed class WorldGenScratch
{
    /// <summary>
    ///     Default scratch directory name below the system temporary path.
    /// </summary>
    private const string DefaultScratchDirectoryName = "sovereign-worldgen";

    /// <summary>
    ///     Prefix of per-plan staging directories.
    /// </summary>
    public const string SessionDirectoryPrefix = "staging_";

    /// <summary>
    ///     Age at which a staging directory or preview image becomes stale and is removed
    ///     by startup cleanup. Staged plans are session-scoped, so anything older than one
    ///     day can only be debris of a lost server session.
    /// </summary>
    private static readonly TimeSpan StagingTtl = TimeSpan.FromHours(24);

    private readonly IOptions<WorldGenOptions> options;

    /// <summary>
    ///     Number of preview paths resolved in this server session.
    /// </summary>
    private int previewCount;

    public WorldGenScratch(IOptions<WorldGenOptions> options)
    {
        this.options = options;
    }

    /// <summary>
    ///     Resolves the scratch directory, creating it on demand.
    /// </summary>
    /// <returns>Absolute path of the scratch directory.</returns>
    public string ResolveDirectory()
    {
        var configured = options.Value.ScratchDirectory;
        var directory = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(Path.GetTempPath(), DefaultScratchDirectoryName)
            : configured;
        Directory.CreateDirectory(directory);
        return Path.GetFullPath(directory);
    }

    /// <summary>
    ///     Resolves the path for the next preview image, creating the scratch directory on
    ///     demand. The file name is suffixed with a per-session counter so that repeated plans
    ///     do not overwrite one another.
    /// </summary>
    /// <param name="seed">World generation seed.</param>
    /// <returns>Absolute path for the next preview image.</returns>
    public string ResolvePreviewPath(ulong seed)
    {
        var n = Interlocked.Increment(ref previewCount);
        return Path.Combine(ResolveDirectory(), $"preview_{seed}_{n}.png");
    }

    /// <summary>
    ///     Resolves and creates a fresh staging directory for one staged plan.
    /// </summary>
    /// <param name="seed">World generation seed of the plan.</param>
    /// <returns>Absolute path of the staging directory.</returns>
    public string CreateSessionDirectory(ulong seed)
    {
        var directory = Path.Combine(ResolveDirectory(),
            $"{SessionDirectoryPrefix}{seed}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    ///     Deletes a staged plan's staging directory and all of its contents.
    /// </summary>
    /// <param name="path">Absolute path of the staging directory.</param>
    public void DeleteSessionDirectory(string path)
    {
        if (Directory.Exists(Path.Combine(path, "segments"))) Directory.Delete(path, true);
    }

    /// <summary>
    ///     Removes staging directories and preview images older than the staging TTL.
    ///     Staged plans are session-scoped, so stale directories can only be debris of a
    ///     lost server session; in particular, no job can be in progress when this runs.
    /// </summary>
    /// <returns>Number of entries removed.</returns>
    public int CleanupStaleDirectories()
    {
        var directory = ResolveDirectory();
        var removed = 0;
        foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
        {
            var name = Path.GetFileName(entry);
            var isStaging = name.StartsWith(SessionDirectoryPrefix, StringComparison.Ordinal);
            var isPreview = name.StartsWith("preview_", StringComparison.Ordinal)
                            || name.StartsWith("caves_", StringComparison.Ordinal);
            if (!isStaging && !isPreview) continue;

            DateTime lastWrite;
            try
            {
                lastWrite = Directory.GetLastWriteTimeUtc(entry);
                if (DateTime.UtcNow - lastWrite < StagingTtl) continue;

                if (Directory.Exists(entry)) Directory.Delete(entry, true);
                else File.Delete(entry);
                ++removed;
            }
            catch (Exception)
            {
                // A missing or locked entry is skipped; the next startup pass retries.
            }
        }

        return removed;
    }
}
