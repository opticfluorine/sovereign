/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;
using Sovereign.EngineCore.Systems.Player.Components;
using Sovereign.ServerCore.Components;

namespace Sovereign.ServerCore.Entities;

/// <summary>
///     Server-side entity factory.
/// </summary>
public sealed class ServerEntityFactory : IEntityFactory
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AccountComponentCollection accounts;
    private readonly ComponentManager componentManager;
    private readonly EntityManager entityManager;
    private readonly MaterialModifierComponentCollection materialModifiers;
    private readonly MaterialComponentCollection materials;
    private readonly NameComponentCollection names;
    private readonly PlayerCharacterTagCollection playerCharacterTags;
    private readonly PositionComponentCollection positions;
    private readonly VelocityComponentCollection velocities;

    private EntityAssigner entityAssigner;

    public ServerEntityFactory(
        EntityManager entityManager,
        ComponentManager componentManager,
        PositionComponentCollection positions,
        VelocityComponentCollection velocities,
        MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AboveBlockComponentCollection aboveBlocks,
        PlayerCharacterTagCollection playerCharacterTags,
        NameComponentCollection names,
        AccountComponentCollection accounts)
    {
        this.entityManager = entityManager;
        this.componentManager = componentManager;
        this.positions = positions;
        this.velocities = velocities;
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.aboveBlocks = aboveBlocks;
        this.playerCharacterTags = playerCharacterTags;
        this.names = names;
        this.accounts = accounts;
    }

    public IEntityBuilder GetBuilder()
    {
        if (entityAssigner == null)
            entityAssigner = entityManager.GetNewAssigner();

        return GetBuilder(entityAssigner.GetNextId());
    }

    public IEntityBuilder GetBuilder(ulong entityId)
    {
        return new ServerEntityBuilder(
            entityId,
            componentManager,
            positions,
            velocities,
            materials,
            materialModifiers,
            aboveBlocks,
            playerCharacterTags,
            names,
            accounts);
    }
}