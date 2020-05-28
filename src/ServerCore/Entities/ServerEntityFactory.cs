/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Systems.Block.Components;
using Sovereign.EngineCore.Systems.Movement.Components;

namespace Sovereign.ServerCore.Entities
{

    /// <summary>
    /// Server-side entity factory.
    /// </summary>
    public sealed class ServerEntityFactory : IEntityFactory
    {
        private readonly EntityManager entityManager;
        private readonly ComponentManager componentManager;
        private readonly PositionComponentCollection positions;
        private readonly VelocityComponentCollection velocities;
        private readonly MaterialComponentCollection materials;
        private readonly MaterialModifierComponentCollection materialModifiers;
        private readonly AboveBlockComponentCollection aboveBlocks;

        private EntityAssigner entityAssigner;

        public ServerEntityFactory(
            EntityManager entityManager,
            ComponentManager componentManager,
            PositionComponentCollection positions,
            VelocityComponentCollection velocities,
            MaterialComponentCollection materials,
            MaterialModifierComponentCollection materialModifiers,
            AboveBlockComponentCollection aboveBlocks)
        {
            this.entityManager = entityManager;
            this.componentManager = componentManager;
            this.positions = positions;
            this.velocities = velocities;
            this.materials = materials;
            this.materialModifiers = materialModifiers;
            this.aboveBlocks = aboveBlocks;
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
                aboveBlocks);
        }

    }

}
