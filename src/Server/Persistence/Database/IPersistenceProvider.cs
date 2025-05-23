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
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database;

/// <summary>
///     Interface implemented by persistence providers.
/// </summary>
public interface IPersistenceProvider : IDisposable
{
    /// <summary>
    // Database connection.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    ///     IMigrationQuery for this persistence provider.
    /// </summary>
    IMigrationQuery MigrationQuery { get; }

    /// <summary>
    ///     INextPersistedIdQuery for this persistence provider.
    /// </summary>
    INextPersistedIdQuery NextPersistedIdQuery { get; }

    /// <summary>
    ///     IRetrieveAllTemplatesQuery for this persistence provider.
    /// </summary>
    IRetrieveAllTemplatesQuery RetrieveAllTemplatesQuery { get; }

    /// <summary>
    ///     IAddAccountQuery for this persistence provider.
    /// </summary>
    IAddAccountQuery AddAccountQuery { get; }

    /// <summary>
    ///     IRetrieveAccountQuery for this persistence provider.
    /// </summary>
    IRetrieveAccountQuery RetrieveAccountQuery { get; }

    /// <summary>
    ///     IRetrieveAccountWithAuthQUery for this persistence provider.
    /// </summary>
    IRetrieveAccountWithAuthQuery RetrieveAccountWithAuthQuery { get; }

    /// <summary>
    ///     IRetrieveEntityQuery for this persistence provider.
    /// </summary>
    IRetrieveEntityQuery RetrieveEntityQuery { get; }

    /// <summary>
    ///     IRetrieveRangeQuery for this persistence provider.
    /// </summary>
    IRetrieveRangeQuery RetrieveRangeQuery { get; }

    /// <summary>
    ///     IAddEntityQuery for this persistence provider.
    /// </summary>
    IAddEntityQuery AddEntityQuery { get; }

    /// <summary>
    ///     IRemoveEntityQuery for this persistence provider.
    /// </summary>
    IRemoveEntityQuery RemoveEntityQuery { get; }

    /// <summary>
    ///     ISetTemplateQuery for this persistence provider.
    /// </summary>
    ISetTemplateQuery SetTemplateQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Position component.
    /// </summary>
    IAddComponentQuery<Kinematics> AddPositionQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Position component.
    /// </summary>
    IModifyComponentQuery<Kinematics> ModifyPositionQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Position component.
    /// </summary>
    IRemoveComponentQuery RemovePositionQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Material component.
    /// </summary>
    IAddComponentQuery<int> AddMaterialQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Material component.
    /// </summary>
    IModifyComponentQuery<int> ModifyMaterialQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Material component.
    /// </summary>
    IRemoveComponentQuery RemoveMaterialQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the MaterialModifier component.
    /// </summary>
    IAddComponentQuery<int> AddMaterialModifierQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the MaterialModifier component.
    /// </summary>
    IModifyComponentQuery<int> ModifyMaterialModifierQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the MaterialModifier component.
    /// </summary>
    IRemoveComponentQuery RemoveMaterialModifierQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the PlayerCharacter tag.
    /// </summary>
    IAddComponentQuery<bool> AddPlayerCharacterQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the PlayerCharacter tag.
    /// </summary>
    IModifyComponentQuery<bool> ModifyPlayerCharacterQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the PlayerCharacter tag.
    /// </summary>
    IRemoveComponentQuery RemovePlayerCharacterQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Name component.
    /// </summary>
    IAddComponentQuery<string> AddNameQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Name component.
    /// </summary>
    IModifyComponentQuery<string> ModifyNameQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Name component.
    /// </summary>
    IRemoveComponentQuery RemoveNameQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Account component.
    /// </summary>
    IAddComponentQuery<Guid> AddAccountComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Account component.
    /// </summary>
    IModifyComponentQuery<Guid> ModifyAccountComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Account component.
    /// </summary>
    IRemoveComponentQuery RemoveAccountComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Parent component.
    /// </summary>
    IAddComponentQuery<ulong> AddParentComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Parent component.
    /// </summary>
    IModifyComponentQuery<ulong> ModifyParentComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Parent component.
    /// </summary>
    IRemoveComponentQuery RemoveParentComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Drawable tag.
    /// </summary>
    IAddComponentQuery<bool> AddDrawableComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Drawable tag.
    /// </summary>
    IModifyComponentQuery<bool> ModifyDrawableComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Drawable tag.
    /// </summary>
    IRemoveComponentQuery RemoveDrawableComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the AnimatedSprite component.
    /// </summary>
    IAddComponentQuery<int> AddAnimatedSpriteComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the AnimatedSprite component.
    /// </summary>
    IModifyComponentQuery<int> ModifyAnimatedSpriteComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the AnimatedSprite component.
    /// </summary>
    IRemoveComponentQuery RemoveAnimatedSpriteComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for Orientation component.
    /// </summary>
    IAddComponentQuery<Orientation> AddOrientationComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for Orientation component.
    /// </summary>
    IModifyComponentQuery<Orientation> ModifyOrientationComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for Orientation component.
    /// </summary>
    IRemoveComponentQuery RemoveOrientationComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for Admin tag.
    /// </summary>
    IAddComponentQuery<bool> AddAdminComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for Admin tag.
    /// </summary>
    IModifyComponentQuery<bool> ModifyAdminComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for Admin tag.
    /// </summary>
    IRemoveComponentQuery RemoveAdminComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for CastBlockShadows tag.
    /// </summary>
    IAddComponentQuery<bool> AddCastBlockShadowsComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for CastBlockShadows tag.
    /// </summary>
    IModifyComponentQuery<bool> ModifyCastBlockShadowsComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for CastBlockShadows tag.
    /// </summary>
    IRemoveComponentQuery RemoveCastBlockShadowsComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for PointLightSource component.
    /// </summary>
    IAddComponentQuery<PointLight> AddPointLightSourceComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for PointLightSource component.
    /// </summary>
    IModifyComponentQuery<PointLight> ModifyPointLightSourceComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for PointLightSource component.
    /// </summary>
    IRemoveComponentQuery RemovePointLightSourceComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the Physics tag.
    /// </summary>
    IAddComponentQuery<bool> AddPhysicsComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Physics tag.
    /// </summary>
    IModifyComponentQuery<bool> ModifyPhysicsComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the Physics tag.
    /// </summary>
    IRemoveComponentQuery RemovePhysicsComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for the BoundingBox component.
    /// </summary>
    IAddComponentQuery<BoundingBox> AddBoundingBoxComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the BoundingBox component.
    /// </summary>
    IModifyComponentQuery<BoundingBox> ModifyBoundingBoxComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for the BoundingBox component.
    /// </summary>
    IRemoveComponentQuery RemoveBoundingBoxComponentQuery { get; }

    /// <summary>
    ///     IAddComponentQuery for CastShadows component.
    /// </summary>
    IAddComponentQuery<Shadow> AddCastShadowsComponentQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for CastShadows component.
    /// </summary>
    IModifyComponentQuery<Shadow> ModifyCastShadowsComponentQuery { get; }

    /// <summary>
    ///     IRemoveComponentQuery for CastShadows component.
    /// </summary>
    IRemoveComponentQuery RemoveCastShadowsComponentQuery { get; }

    /// <summary>
    ///     IPlayerExistsQuery for this persistence provider.
    /// </summary>
    IPlayerExistsQuery PlayerExistsQuery { get; }

    /// <summary>
    ///     IGetAccountForPlayerQuery for this persistence provider.
    /// </summary>
    IGetAccountForPlayerQuery GetAccountForPlayerQuery { get; }

    /// <summary>
    ///     IListPlayersQuery for this persistence provider.
    /// </summary>
    IListPlayersQuery ListPlayersQuery { get; }

    /// <summary>
    ///     IDeletePlayerQuery for this persistence provider.
    /// </summary>
    IDeletePlayerQuery DeletePlayerQuery { get; }

    /// <summary>
    ///     IAddAdminRoleQuery for this persistence provider.
    /// </summary>
    IAddAdminRoleQuery AddAdminRoleQuery { get; }

    /// <summary>
    ///     IRemoveAdminRoleQuery for this persistence provider.
    /// </summary>
    IRemoveAdminRoleQuery RemoveAdminRoleQuery { get; }

    /// <summary>
    ///     IGetWorldSegmentBlockDataQuery for this persistence provider.
    /// </summary>
    IGetWorldSegmentBlockDataQuery GetWorldSegmentBlockDataQuery { get; }

    /// <summary>
    ///     ISetWorldSegmentBlockDataQuery for this persistence provider.
    /// </summary>
    ISetWorldSegmentBlockDataQuery SetWorldSegmentBlockDataQuery { get; }

    /// <summary>
    ///     IGetGlobalKeyValuePairsQuery for this persistence provider.
    /// </summary>
    IGetGlobalKeyValuePairsQuery GetGlobalKeyValuePairsQuery { get; }

    /// <summary>
    ///     IUpdateGlobalKeyValuePairQuery for this persistence provider.
    /// </summary>
    IUpdateGlobalKeyValuePairQuery UpdateGlobalKeyValuePairQuery { get; }

    /// <summary>
    ///     IRemoveGlobalKeyValuePairQuery for this persistence provider.
    /// </summary>
    IRemoveGlobalKeyValuePairQuery RemoveGlobalKeyValuePairQuery { get; }
}