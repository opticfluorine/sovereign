/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Sovereign.ClientCore.Rendering.Components;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.Systems.Player.Components;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Entity factory for the client.
/// </summary>
public sealed class ClientEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly ComponentManager componentManager;
    private readonly DrawableComponentCollection drawables;
    private readonly EntityManager entityManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;
    private EntityAssigner assigner;

    public ClientEntityFactory(EntityManager entityManager,
        ComponentManager componentManager,
        PositionComponentCollection positions,
        VelocityComponentCollection velocities,
        DrawableComponentCollection drawables,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        AnimatedSpriteComponentCollection animatedSprites,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names)
    {
        this.entityManager = entityManager;
        this.componentManager = componentManager;
        this.positions = positions;
        this.velocities = velocities;
        this.drawables = drawables;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.animatedSprites = animatedSprites;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
    }

    public IEntityBuilder GetBuilder()
    {
        if (assigner == null)
            assigner = entityManager.GetNewAssigner();

        return GetBuilder(assigner.GetNextId());
    }

    public IEntityBuilder GetBuilder(ulong entityId)
    {
        return new ClientEntityBuilder(entityId,
            componentManager, positions, velocities, drawables, materials,
            materialModifiers, aboveBlocks, animatedSprites, playerCharacterTags, names);
    }
}