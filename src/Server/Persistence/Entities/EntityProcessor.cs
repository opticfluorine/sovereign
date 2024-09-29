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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Entities;

namespace Sovereign.Persistence.Entities;

/// <summary>
///     Processes entities as they are received from the database.
/// </summary>
public sealed class EntityProcessor
{
    private const int IndexId = 0;
    private const int IndexTemplateId = IndexId + 1;
    private const int IndexPosX = IndexTemplateId + 1;
    private const int IndexPosY = IndexPosX + 1;
    private const int IndexPosZ = IndexPosY + 1;
    private const int IndexMaterial = IndexPosZ + 1;
    private const int IndexMaterialModifier = IndexMaterial + 1;
    private const int IndexPlayerCharacter = IndexMaterialModifier + 1;
    private const int IndexName = IndexPlayerCharacter + 1;
    private const int IndexAccount = IndexName + 1;
    private const int IndexParent = IndexAccount + 1;
    private const int IndexDrawable = IndexParent + 1;
    private const int IndexAnimatedSprite = IndexDrawable + 1;
    private const int IndexOrientation = IndexAnimatedSprite + 1;
    private const int IndexAdmin = IndexOrientation + 1;
    private const int IndexCastBlockShadows = IndexAdmin + 1;
    private readonly IEntityFactory entityFactory;
    private readonly EntityMapper mapper;

    public EntityProcessor(IEntityFactory entityFactory, EntityMapper mapper)
    {
        this.entityFactory = entityFactory;
        this.mapper = mapper;
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
        var entityId = (ulong)reader.GetInt64(IndexId);

        /* Start loading the entity. */
        mapper.MarkEntityAsLoaded(entityId);
        var builder = entityFactory.GetBuilder(entityId, true);

        /* Process components. */
        ProcessTemplate(reader, builder);
        ProcessPosition(reader, builder);
        ProcessMaterial(reader, builder);
        ProcessPlayerCharacter(reader, builder);
        ProcessName(reader, builder);
        ProcessAccount(reader, builder, entityId);
        ProcessParent(reader, builder);
        ProcessDrawable(reader, builder);
        ProcessAnimatedSprite(reader, builder);
        ProcessOrientation(reader, builder);
        ProcessAdmin(reader, builder);
        ProcessCastBlockShadows(reader, builder);

        /* Complete the entity. */
        builder.Build();
    }

    /// <summary>
    ///     Process the entity template data, if any, from the reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Builder.</param>
    private void ProcessTemplate(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(IndexTemplateId)) return;

        builder.Template((ulong)reader.GetInt64(IndexTemplateId));
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
        if (reader.IsDBNull(IndexAccount)) return;

        /* Extract GUID. */
        var accountIdBytes = new byte[16];
        var bytesRead = reader.GetBytes(IndexAccount, 0, accountIdBytes, 0, 16);
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
        if (reader.IsDBNull(IndexName)) return;

        /* Process. */
        builder.Name(reader.GetString(IndexName));
    }

    /// <summary>
    ///     Process the position component, if any, from the reader.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessPosition(IDataReader reader, IEntityBuilder builder)
    {
        /* Check for existence. */
        if (reader.IsDBNull(IndexPosX)) return;

        /* Process. */
        builder.Positionable(GetVector3(reader,
            IndexPosX, IndexPosY, IndexPosZ));
    }

    /// <summary>
    ///     Processes the material and material modifier components, if any.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessMaterial(IDataReader reader, IEntityBuilder builder)
    {
        /* Check for existence. */
        if (reader.IsDBNull(IndexMaterial)) return;

        /* Process. */
        builder.Material(reader.GetInt32(IndexMaterial),
            reader.GetInt32(IndexMaterialModifier));
    }

    /// <summary>
    ///     Processes the player character flag if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessPlayerCharacter(IDataReader reader, IEntityBuilder builder)
    {
        /* Check for existence. */
        if (reader.IsDBNull(IndexPlayerCharacter)) return;

        /* Process. */
        if (reader.GetBoolean(IndexPlayerCharacter)) builder.PlayerCharacter();
    }

    /// <summary>
    ///     Processes the parent entity mapping if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessParent(IDataReader reader, IEntityBuilder builder)
    {
        // Check for existence.
        if (reader.IsDBNull(IndexParent)) return;

        // Process.
        var parentEntityId = (ulong)reader.GetInt64(IndexParent);
        builder.Parent(parentEntityId);
    }

    /// <summary>
    ///     Processes the Drawable component if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessDrawable(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(IndexDrawable)) return;
        if (reader.GetBoolean(IndexDrawable)) builder.Drawable();
    }

    /// <summary>
    ///     Processes the AnimatedSprite component if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessAnimatedSprite(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(IndexAnimatedSprite)) return;
        builder.AnimatedSprite(reader.GetInt32(IndexAnimatedSprite));
    }

    /// <summary>
    ///     Processes the Orientation component if present.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessOrientation(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(IndexOrientation)) return;
        builder.Orientation((Orientation)reader.GetByte(IndexOrientation));
    }

    /// <summary>
    ///     Processes the Admin component.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessAdmin(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(IndexAdmin)) return;
        if (reader.GetBoolean(IndexAdmin)) builder.Admin();
    }

    /// <summary>
    ///     Processes the CastBlockShadows component.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="builder">Entity builder.</param>
    private void ProcessCastBlockShadows(IDataReader reader, IEntityBuilder builder)
    {
        if (reader.IsDBNull(IndexCastBlockShadows)) return;
        if (reader.GetBoolean(IndexCastBlockShadows)) builder.CastBlockShadows();
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