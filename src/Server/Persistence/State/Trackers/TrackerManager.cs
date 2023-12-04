/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

namespace Sovereign.Persistence.State.Trackers;

/// <summary>
///     Manages the trackers, allowing them to be integrated
///     through a single dependency.
/// </summary>
public sealed class TrackerManager
{
    public TrackerManager(PositionStateTracker positionStateTracker,
        MaterialStateTracker materialStateTracker,
        MaterialModifierStateTracker materialModifierStateTracker,
        PlayerCharacterStateTracker playerCharacterStateTracker,
        NameStateTracker nameStateTracker,
        AccountStateTracker accountStateTracker,
        ParentStateTracker parentStateTracker)
    {
        PositionStateTracker = positionStateTracker;
        MaterialStateTracker = materialStateTracker;
        MaterialModifierStateTracker = materialModifierStateTracker;
        PlayerCharacterStateTracker = playerCharacterStateTracker;
        NameStateTracker = nameStateTracker;
        AccountStateTracker = accountStateTracker;
        ParentStateTracker = parentStateTracker;
    }

    /// <summary>
    ///     Position state tracker.
    /// </summary>
    public PositionStateTracker PositionStateTracker { get; }

    /// <summary>
    ///     Material state tracker.
    /// </summary>
    public MaterialStateTracker MaterialStateTracker { get; }

    /// <summary>
    ///     Material modifier state tracker.
    /// </summary>
    public MaterialModifierStateTracker MaterialModifierStateTracker { get; }

    /// <summary>
    ///     Player character state tracker.
    /// </summary>
    public PlayerCharacterStateTracker PlayerCharacterStateTracker { get; }

    /// <summary>
    ///     Name state tracker.
    /// </summary>
    public NameStateTracker NameStateTracker { get; }

    /// <summary>
    ///     Account linkage state tracker.
    /// </summary>
    public AccountStateTracker AccountStateTracker { get; }

    /// <summary>
    ///     Parent entity linkage state tracker.
    /// </summary>
    public ParentStateTracker ParentStateTracker { get; }
}