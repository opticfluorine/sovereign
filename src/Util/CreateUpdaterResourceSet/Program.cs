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

using System.Text.Json;
using CreateUpdaterResourceSet;
using Sovereign.EngineCore.Network;
using Sovereign.UpdaterCore.Updater;

const string indexFilename = "index.json";
const string releaseFilename = "release.json";

try
{
    // Parse args, basic error checking
    if (args.Length != 1)
    {
        Console.Error.WriteLine("Usage: CreateUpdaterResourceSet resourcePath");
        return 1;
    }

    var basePath = args[0];
    if (!Directory.Exists(basePath))
    {
        Console.Error.WriteLine($"Resource directory {basePath} does not exist.");
        return 1;
    }

    // Set up resource discovery
    var pathBuilder = new UtilResourcePathBuilder(basePath);
    var updaterHash = new UpdaterHash();
    var builder = new UpdaterResourceSetBuilder(updaterHash);

    // Generate index.json
    var resourceSet = builder.Build(pathBuilder);
    var indexPath = Path.Combine(basePath, indexFilename);
    using var indexFs = File.Open(indexPath, FileMode.OpenOrCreate, FileAccess.Write);
    JsonSerializer.Serialize(indexFs, resourceSet, MessageConfig.JsonOptions);

    // Generate release.json
    var release = new UpdaterRelease { ReleaseId = resourceSet.ReleaseId };
    using var releaseFs = File.Open(releaseFilename, FileMode.OpenOrCreate, FileAccess.Write);
    JsonSerializer.Serialize(releaseFs, release, MessageConfig.JsonOptions);

    return 0;
}
catch (Exception e)
{
    Console.Error.WriteLine("An error occurred (unhandled exception).");
    Console.Error.WriteLine(e);
    return 1;
}