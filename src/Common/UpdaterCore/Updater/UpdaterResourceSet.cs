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

using Sovereign.EngineCore.Resources;

namespace Sovereign.UpdaterCore.Updater;

/// <summary>
///     Describes a set of game resources that can be automatically updated.
/// </summary>
public class UpdaterResourceSet
{
    /// <summary>
    ///     Unique release ID. Used to avoid server-side cache conflicts between releases.
    /// </summary>
    public Guid ReleaseId { get; set; }

    /// <summary>
    ///     List of updateable resources.
    /// </summary>
    public List<UpdaterResource> Resources { get; set; } = new();
}

/// <summary>
///     Describes a single game resource that can be automatically updated.
/// </summary>
public class UpdaterResource
{
    /// <summary>
    ///     Resource type.
    /// </summary>
    public ResourceType ResourceType { get; set; }

    /// <summary>
    ///     Resource filename.
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    ///     SHA512 hash of the file.
    /// </summary>
    public string Hash { get; set; }
}