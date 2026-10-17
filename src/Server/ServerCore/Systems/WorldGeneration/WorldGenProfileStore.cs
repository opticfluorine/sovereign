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

using System.Linq;
using Sovereign.WorldGen;

namespace Sovereign.ServerCore.Systems.WorldGeneration;

/// <summary>
///     Validates and saves world generation profile files on behalf of administrative
///     tooling. All operations are synchronous; callers map rejections to their own error
///     surfaces.
/// </summary>
public sealed class WorldGenProfileStore
{
    private readonly ProfileLoader profileLoader;
    private readonly ProfileValidator profileValidator;

    public WorldGenProfileStore(ProfileLoader profileLoader, ProfileValidator profileValidator)
    {
        this.profileLoader = profileLoader;
        this.profileValidator = profileValidator;
    }

    /// <summary>
    ///     Validates a profile name and JSON body against the profile schema and semantic
    ///     profile rules, saving the profile as <c>&lt;name&gt;.json</c> in the profile
    ///     search directory when valid. An existing profile with the same name is
    ///     overwritten.
    /// </summary>
    /// <param name="name">Profile name from the request, without extension.</param>
    /// <param name="json">JSON text of the profile.</param>
    /// <param name="error">Human-readable rejection reason, or null on success.</param>
    /// <returns>true if the profile was saved, false if it was rejected.</returns>
    public bool TrySaveProfile(string name, string json, out string? error)
    {
        if (!ProfileLoader.IsValidProfileName(name))
        {
            error = $"Invalid profile name \"{name}\".";
            return false;
        }

        WorldGenProfile profile;
        try
        {
            profile = profileLoader.Parse(json);
        }
        catch (ProfileLoadException e)
        {
            error = e.Message;
            return false;
        }

        var issues = profileValidator.Validate(profile);
        var errors = issues.Where(issue => issue.Severity == ProfileValidationSeverity.Error)
            .ToList();
        if (errors.Count > 0)
        {
            error = "Invalid profile: "
                    + string.Join(" ", errors.Select(issue => issue.Message));
            return false;
        }

        profileLoader.Save(name, profile);
        error = null;
        return true;
    }
}
