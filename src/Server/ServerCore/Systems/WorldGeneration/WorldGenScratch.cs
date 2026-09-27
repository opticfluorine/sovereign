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
}
