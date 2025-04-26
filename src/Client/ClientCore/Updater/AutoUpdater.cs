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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.EngineCore.Resources;
using Sovereign.UpdaterCore.Updater;

namespace Sovereign.ClientCore.Updater;

/// <summary>
///     Possible states of the autoupdater.
/// </summary>
public enum AutoUpdaterState
{
    /// <summary>
    ///     Autoupdater has not yet started.
    /// </summary>
    NotStarted,

    /// <summary>
    ///     Autoupdater is waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    ///     Autoupdater is retrieving the release.json file.
    /// </summary>
    GetRelease,

    /// <summary>
    ///     Autoupdater is retrieving the index.json file.
    /// </summary>
    GetIndex,

    /// <summary>
    ///     Autoupdater is retrieving client-side resource files.
    /// </summary>
    GetFile,

    /// <summary>
    ///     Autoupdater has completed.
    /// </summary>
    Complete,

    /// <summary>
    ///     Autoupdater encountered an error.
    /// </summary>
    Error
}

/// <summary>
///     Automatically updates client-side game resources from a remote update server.
/// </summary>
public partial class AutoUpdater
{
    /// <summary>
    ///     Relative URI of the release.json file.
    /// </summary>
    private const string ReleaseJsonUri = "release.json";

    /// <summary>
    ///     Relative URI of the index.json file.
    /// </summary>
    private const string IndexJsonUri = "index.json";

    /// <summary>
    ///     List of allowed resource types.
    /// </summary>
    private readonly List<ResourceType> allowedResourceTypes = new()
    {
        ResourceType.Sprite,
        ResourceType.Spritesheet,
        ResourceType.World
    };

    private readonly AutoUpdaterOptions autoUpdaterOptions;

    /// <summary>
    ///     HTTP client used for interacting with the update server.
    /// </summary>
    private readonly HttpClient client;

    private readonly ILogger<AutoUpdater> logger;
    private readonly IResourcePathBuilder resourcePathBuilder;
    private readonly UpdaterHash updaterHash;

    /// <summary>
    ///     Current background task.
    /// </summary>
    private Task? backgroundTask;

    public AutoUpdater(UpdaterHash updaterHash, IOptions<AutoUpdaterOptions> autoUpdaterOptions,
        IResourcePathBuilder resourcePathBuilder, ILogger<AutoUpdater> logger)
    {
        this.updaterHash = updaterHash;
        this.resourcePathBuilder = resourcePathBuilder;
        this.logger = logger;
        this.autoUpdaterOptions = autoUpdaterOptions.Value;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            CheckCertificateRevocationList = true
        };
        client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Sovereign Engine");
    }

    /// <summary>
    ///     Current autoupdater state.
    /// </summary>
    public AutoUpdaterState State { get; private set; } = AutoUpdaterState.NotStarted;

    /// <summary>
    ///     Name of file currently being retrieved.
    /// </summary>
    public string CurrentFile { get; private set; } = "";

    /// <summary>
    ///     Current error message reported by the autoupdater.
    /// </summary>
    public string Error { get; private set; } = "";

    /// <summary>
    ///     Percent complete of overall update operation.
    /// </summary>
    public float PercentComplete { get; private set; }

    /// <summary>
    ///     Starts the update process in the background.
    /// </summary>
    public void UpdateInBackground()
    {
        State = AutoUpdaterState.Pending;
        backgroundTask = RunUpdaterAsync();
    }

    /// <summary>
    ///     Skips the update process entirely.
    /// </summary>
    public void SkipUpdates()
    {
        State = AutoUpdaterState.Complete;
    }

    /// <summary>
    ///     Asynchronously runs the updater.
    /// </summary>
    private async Task RunUpdaterAsync()
    {
        try
        {
            // Set up.
            var baseUri = new Uri(autoUpdaterOptions.UpdateServerUrl);

            // Get current release.
            State = AutoUpdaterState.GetRelease;
            CurrentFile = ReleaseJsonUri;
            PercentComplete = 0.0f;
            var releaseJsonUri = new Uri(baseUri, ReleaseJsonUri);
            var releaseJson = await client.GetFromJsonAsync<UpdaterRelease>(releaseJsonUri)
                              ?? throw new Exception("No update releases found.");
            var releaseBaseUri = new Uri(baseUri, $"{releaseJson.ReleaseId}/");

            // Update everything in the latest release.
            await UpdateReleaseAsync(releaseBaseUri);

            PercentComplete = 1.0f;
            State = AutoUpdaterState.Complete;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while updating {CurrentFile}.", CurrentFile);
            Error = e.Message;
            State = AutoUpdaterState.Error;
        }
    }

    /// <summary>
    ///     Asynchronously updates all content in the release located at the given URI.
    /// </summary>
    /// <param name="releaseBaseUri">Base URI of the release.</param>
    private async Task UpdateReleaseAsync(Uri releaseBaseUri)
    {
        // Retrieve and parse index.
        State = AutoUpdaterState.GetIndex;
        CurrentFile = IndexJsonUri;
        var indexUri = new Uri(releaseBaseUri, IndexJsonUri);
        var indexJson = await client.GetFromJsonAsync<UpdaterResourceSet>(indexUri)
                        ?? throw new Exception("No release index found.");
        logger.LogDebug("index.json contains {Count} resources.", indexJson.Resources.Count);

        // Silently exclude any disallowed file types.
        var allowedResources = indexJson.Resources
            .Where(res => allowedResourceTypes.Contains(res.ResourceType))
            .Where(res => AllowedFilenamesRegex().IsMatch($"{res.ResourceType}/{res.Filename}"))
            .ToList();
        foreach (var badResource in indexJson.Resources.Except(allowedResources))
        {
            logger.LogWarning("Skipping disallowed resource {ResourceType}/{Filename}.",
                badResource.ResourceType, badResource.Filename);
        }

        // Determine resources for update.
        var resourcesToUpdate = allowedResources
            .Where(res => NeedToUpdate(
                resourcePathBuilder.BuildPathToResource(res.ResourceType, res.Filename),
                res.Hash))
            .ToList();
        var progressStep = 1.0f / resourcesToUpdate.Count;

        // Update all resources in release.
        foreach (var resource in resourcesToUpdate)
        {
            await UpdateResourceAsync(releaseBaseUri, resource);
            PercentComplete += progressStep;
        }
    }

    /// <summary>
    ///     Asynchronously updates a single resource.
    /// </summary>
    /// <param name="releaseBaseUri">Base URI of release.</param>
    /// <param name="resource">Resource to update.</param>
    /// <exception cref="Exception">Thrown if an error occurs.</exception>
    private async Task UpdateResourceAsync(Uri releaseBaseUri, UpdaterResource resource)
    {
        // Start update.
        var filename = $"{resource.ResourceType}/{resource.Filename}";
        State = AutoUpdaterState.GetFile;
        CurrentFile = filename;

        var localPath = resourcePathBuilder.BuildPathToResource(resource.ResourceType, resource.Filename);
        var fileUri = new Uri(releaseBaseUri, filename);

        var tempFilePath = Path.GetTempFileName();
        await using (var fs = File.Open(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write))
        {
            var rs = await client.GetStreamAsync(fileUri);

            // Write the received file into the temporary file.
            const int bufSize = 1024;
            var buf = new byte[bufSize];
            var bytesRead = 0;
            do
            {
                bytesRead = await rs.ReadAsync(buf);
                await fs.WriteAsync(buf, 0, bytesRead);
            } while (bytesRead > 0);
        }

        // Verify the new file has the expected hash.
        var newHash = updaterHash.Hash(tempFilePath);
        if (!newHash.Equals(resource.Hash))
            throw new Exception("New file has incorrect hash.");

        // Update the local file now that the download is verified.
        File.Move(tempFilePath, localPath, true);
    }

    /// <summary>
    ///     Determines if a local file requires update.
    /// </summary>
    /// <param name="localPath">Local path.</param>
    /// <param name="hash">Expected hash for latest release.</param>
    /// <returns>true if an update is required, false otherwise.</returns>
    private bool NeedToUpdate(string localPath, string hash)
    {
        if (!File.Exists(localPath)) return true;

        // File exists, check its hash.
        var localHash = updaterHash.Hash(localPath);
        return !localHash.Equals(hash);
    }

    /// <summary>
    ///     Regex that matches allowed resource filenames.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[A-Za-z0-9_\-]+/[A-Za-z0-9_\-]+\.(?:png|json|yaml|txt)$")]
    private static partial Regex AllowedFilenamesRegex();
}