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
    private readonly AdminStateTracker adminStateTracker;
    private readonly BoundingBoxStateTracker boundingBoxStateTracker;
    private readonly CastBlockShadowsStateTracker castBlockShadowsStateTracker;
    private readonly CastShadowsStateTracker castShadowsStateTracker;
    private readonly EntityTypeStateTracker entityTypeStateTracker;
    private readonly PhysicsStateTracker physicsStateTracker;
    private readonly PointLightSourceStateTracker pointLightSourceStateTracker;
    private readonly TemplateStateTracker templateStateTracker;

    public TrackerManager(KinematicsStateTracker kinematicsStateTracker,
        MaterialStateTracker materialStateTracker,
        MaterialModifierStateTracker materialModifierStateTracker,
        PlayerCharacterStateTracker playerCharacterStateTracker,
        NameStateTracker nameStateTracker,
        AccountStateTracker accountStateTracker,
        ParentStateTracker parentStateTracker,
        DrawableStateTracker drawableStateTracker,
        AnimatedSpriteStateTracker animatedSpriteStateTracker,
        OrientationStateTracker orientationStateTracker,
        AdminStateTracker adminStateTracker,
        TemplateStateTracker templateStateTracker,
        CastBlockShadowsStateTracker castBlockShadowsStateTracker,
        PointLightSourceStateTracker pointLightSourceStateTracker,
        CastShadowsStateTracker castShadowsStateTracker,
        PhysicsStateTracker physicsStateTracker,
        BoundingBoxStateTracker boundingBoxStateTracker,
        EntityTypeStateTracker entityTypeStateTracker)
    {
        this.adminStateTracker = adminStateTracker;
        this.templateStateTracker = templateStateTracker;
        this.castBlockShadowsStateTracker = castBlockShadowsStateTracker;
        this.pointLightSourceStateTracker = pointLightSourceStateTracker;
        this.castShadowsStateTracker = castShadowsStateTracker;
        this.physicsStateTracker = physicsStateTracker;
        this.boundingBoxStateTracker = boundingBoxStateTracker;
        this.entityTypeStateTracker = entityTypeStateTracker;
        OrientationStateTracker = orientationStateTracker;
        KinematicsStateTracker = kinematicsStateTracker;
        MaterialStateTracker = materialStateTracker;
        MaterialModifierStateTracker = materialModifierStateTracker;
        PlayerCharacterStateTracker = playerCharacterStateTracker;
        NameStateTracker = nameStateTracker;
        AccountStateTracker = accountStateTracker;
        ParentStateTracker = parentStateTracker;
        DrawableStateTracker = drawableStateTracker;
        AnimatedSpriteStateTracker = animatedSpriteStateTracker;
    }

    /// <summary>
    ///     Orientation state tracker.
    /// </summary>
    public OrientationStateTracker OrientationStateTracker { get; }

    /// <summary>
    ///     Position state tracker.
    /// </summary>
    public KinematicsStateTracker KinematicsStateTracker { get; }

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

    /// <summary>
    ///     Drawable tag state tracker.
    /// </summary>
    public DrawableStateTracker DrawableStateTracker { get; }

    /// <summary>
    ///     AnimatedSprite component state tracker.
    /// </summary>
    public AnimatedSpriteStateTracker AnimatedSpriteStateTracker { get; }
}