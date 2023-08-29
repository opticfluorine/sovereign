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

using System;
using System.Data;
using System.Numerics;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database;

/// <summary>
///     Interface implemented by persistence providers.
/// </summary>
public interface IPersistenceProvider
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
    ///     IAddComponentQuery for the Position component.
    /// </summary>
    IAddComponentQuery<Vector3> AddPositionQuery { get; }

    /// <summary>
    ///     IModifyComponentQuery for the Position component.
    /// </summary>
    IModifyComponentQuery<Vector3> ModifyPositionQuery { get; }

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
    ///     IPlayerExistsQuery for this persistence provider.
    /// </summary>
    IPlayerExistsQuery PlayerExistsQuery { get; }

    /// <summary>
    ///     Initializes the persistence provider.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Cleans up the persistence provider.
    /// </summary>
    void Cleanup();
}