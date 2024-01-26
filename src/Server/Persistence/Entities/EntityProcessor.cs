﻿/*
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

using System;
using System.Data;
using System.Numerics;
using Castle.Core.Logging;
using Sovereign.EngineCore.Entities;

namespace Sovereign.Persistence.Entities;

/// <summary>
///     Processes entities as they are received from the database.
/// </summary>
public sealed class EntityProcessor
{
    private const int INDEX_ID = 0;
    private const int INDEX_POS_X = 1;
    private const int INDEX_POS_Y = 2;
    private const int INDEX_POS_Z = 3;
    private const int INDEX_MATERIAL = 4;
    private const int INDEX_MATERIAL_MODIFIER = 5;
    private const int INDEX_PLAYER_CHARACTER = 6;
    private const int INDEX_NAME = 7;
    private const int INDEX_ACCOUNT = 8;
    private const int INDEX_PARENT = 9;
    private const int INDEX_DRAWABLE = 10;
    private const int INDEX_ANIMATEDSPRITE = 11;
    private readonly IEntityFactory entityFactory;
    private readonly EntityMapper entityMapper;

    public EntityProcessor(EntityMapper entityMapper,
        IEntityFactory entityFactory)
    {
        this.entityMapper = entityMapper;
        this.entityFactory = entityFactory;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Processes all entities from the given reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <returns>Number of entities processed.</returns>
    public int ProcessFromReader(IDataReader reader)
    {
        var count = 0;
        while (reader.Read())
        {
            ProcessSingleEntity(reader);
            count++;
        }

        return count;
    }

    /// <summary>
    ///     Processes a single entity from the reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    private void ProcessSingleEntity(IDataReader reader)
    {
        /* Get the entity ID. */
        var entityId = (ulong)reader.GetInt64(INDEX_ID);

        /* Start loading the entity. */
        var builder = entityFactory.GetBuilder(entityId, true);

        /* Process components. */
        ProcessPosition(reader, builder);
        ProcessMaterial(reader, builder);
        ProcessPlayerCharacter(reader, builder);
        ProcessName(reader, builder);
        ProcessAccount(reader, builder, entityId);
        ProcessParent(reader, builder);
        ProcessDrawable(reader, builder);
        ProcessAnimatedSprite(reader, builder);

        /* Complete the entity. */
        builder.Build();
    }

    /// <summary>
    ///     Process the account component, if any, from the reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    /// <param name="entityId">Entity ID.</param>
    private void ProcessAccount(IDataReader reader, IEntityBuilder builder, ulong entityId)
    {
        /* Check for existence. */
        if (reader.IsDBNull(INDEX_ACCOUNT)) return;

        /* Extract GUID. */
        var accountIdBytes = new byte[16];
        var bytesRead = reader.GetBytes(INDEX_ACCOUNT, 0, accountIdBytes, 0, 16);
        if (bytesRead < 16)
        {
            Logger.ErrorFormat("Account GUID for entity {0} is too short; skipping component.", entityId);
            return;
        }

        /* Process. */
        builder.Account(new Guid(accountIdBytes));
    }

    /// <summary>
    ///     Process the name component, if any, from the reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessName(IDataReader reader, IEntityBuilder builder)
    {
        /* Check for existence. */
        if (reader.IsDBNull(INDEX_NAME)) return;

        /* Process. */
        builder.Name(reader.GetString(INDEX_NAME));
    }

    /// <summary>
    ///     Process the position component, if any, from the reader.
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
    ///     Processes the material and material modifier components, if any.
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
    ///     Processes the player character flag if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessPlayerCharacter(IDataReader reader, IEntityBuilder builder)
    {
        /* Check for existence. */
        if (reader.IsDBNull(INDEX_PLAYER_CHARACTER)) return;

        /* Process. */
        if (reader.GetBoolean(INDEX_PLAYER_CHARACTER)) builder.PlayerCharacter();
    }

    /// <summary>
    ///     Processes the parent entity mapping if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessParent(IDataReader reader, IEntityBuilder builder)
    {
        // Check for existence.
        if (reader.IsDBNull(INDEX_PARENT)) return;

        // Process.
        var parentEntityId = (ulong)reader.GetInt64(INDEX_PARENT);
        builder.Parent(parentEntityId);
    }

    /// <summary>
    ///     Processes the Drawable component if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessDrawable(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(INDEX_DRAWABLE)) return;
        if (reader.GetBoolean(INDEX_DRAWABLE)) builder.Drawable();
    }

    /// <summary>
    ///     Processes the AnimatedSprite component if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessAnimatedSprite(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(INDEX_ANIMATEDSPRITE)) return;
        builder.AnimatedSprite(reader.GetInt32(INDEX_ANIMATEDSPRITE));
    }

    /// <summary>
    ///     Extracts a Vector3 from the reader.
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