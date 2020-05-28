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

using Sovereign.EngineCore.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.Entities
{

    /// <summary>
    /// Processes entities as they are received from the database.
    /// </summary>
    public sealed class EntityProcessor
    {
        private readonly EntityMapper entityMapper;
        private readonly IEntityFactory entityFactory;
        private const int INDEX_ID = 0;
        private const int INDEX_POS_X = 1;
        private const int INDEX_POS_Y = 2;
        private const int INDEX_POS_Z = 3;
        private const int INDEX_MATERIAL = 4;
        private const int INDEX_MATERIAL_MODIFIER = 5;

        public EntityProcessor(EntityMapper entityMapper,
            IEntityFactory entityFactory)
        {
            this.entityMapper = entityMapper;
            this.entityFactory = entityFactory;
        }

        /// <summary>
        /// Processes all entities from the given reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public void ProcessFromReader(IDataReader reader)
        {
            while (reader.Read())
            {
                ProcessSingleEntity(reader);
            }
        }

        /// <summary>
        /// Processes a single entity from the reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        private void ProcessSingleEntity(IDataReader reader)
        {
            /* Get the entity ID. */
            ulong entityId = (ulong)reader.GetInt64(0);

            /* Start creating the entity. */
            var builder = entityFactory.GetBuilder(entityId);

            /* Process components. */
            ProcessPosition(reader, builder);
            ProcessMaterial(reader, builder);

            /* Complete the entity. */
            builder.Build();
        }

        /// <summary>
        /// Process the position component, if any, from the reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        /// <param name="builder">Entity builder.</param>
        private void ProcessPosition(IDataReader reader, IEntityBuilder builder)
        {
            /* Check for existence. */
            if (reader.IsDBNull(INDEX_POS_X)) return;

            /* Process. */
            builder.Positionable(GetVector3(reader, 
                INDEX_POS_X, INDEX_POS_Y, INDEX_POS_Z));
        }

        /// <summary>
        /// Processes the material and material modifier components, if any.
        /// </summary>
        /// <param name="reader">Reader.</param>
        /// <param name="builder">Entity builder.</param>
        private void ProcessMaterial(IDataReader reader, IEntityBuilder builder)
        {
            /* Check for existence. */
            if (reader.IsDBNull(INDEX_MATERIAL)) return;

            /* Process. */
            builder.Material(reader.GetInt32(INDEX_MATERIAL),
                reader.GetInt32(INDEX_MATERIAL_MODIFIER));
        }

        /// <summary>
        /// Extracts a Vector3 from the reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        /// <param name="indexX">Index of the x component.</param>
        /// <param name="indexY">Index of the y component.</param>
        /// <param name="indexZ">Index of the z component.</param>
        /// <returns>Vector3.</returns>
        private Vector3 GetVector3(IDataReader reader, int indexX, int indexY, int indexZ)
        {
            var x = reader.GetFloat(indexX);
            var y = reader.GetFloat(indexY);
            var z = reader.GetFloat(indexZ);

            return new Vector3(x, y, z);
        }

    }

}
