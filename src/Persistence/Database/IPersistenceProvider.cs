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

using Sovereign.Persistence.Database.Queries;
using System;
using System.Data;
using System.Numerics;

namespace Sovereign.Persistence.Database
{

    /// <summary>
    /// Interface implemented by persistence providers.
    /// </summary>
    public interface IPersistenceProvider
    {

        /// <summary>
        // Database connection.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// IMigrationQuery for this persistence provider.
        /// </summary>
        IMigrationQuery MigrationQuery { get; }

        /// <summary>
        /// INextPersistedIdQuery for this persistence provider.
        /// </summary>
        INextPersistedIdQuery NextPersistedIdQuery { get; }

        /// <summary>
        /// IAddAccountQuery for this persistence provider.
        /// </summary>
        IAddAccountQuery AddAccountQuery { get; }

        /// <summary>
        /// IRetrieveAccountQuery for this persistence provider.
        /// </summary>
        IRetrieveAccountQuery RetrieveAccountQuery { get; }

        /// <summary>
        /// IRetrieveAccountWithAuthQUery for this persistence provider.
        /// </summary>
        IRetrieveAccountWithAuthQuery RetrieveAccountWithAuthQuery { get; }

        /// <summary>
        /// IRetrieveEntityQuery for this persistence provider.
        /// </summary>
        IRetrieveEntityQuery RetrieveEntityQuery { get; }

        /// <summary>
        /// IRetrieveRangeQuery for this persistence provider.
        /// </summary>
        IRetrieveRangeQuery RetrieveRangeQuery { get; }

        /// <summary>
        /// IAddEntityQuery for this persistence provider.
        /// </summary>
        IAddEntityQuery AddEntityQuery { get; }

        /// <summary>
        /// IRemoveEntityQuery for this persistence provider.
        /// </summary>
        IRemoveEntityQuery RemoveEntityQuery { get; }

        /// <summary>
        /// IAddComponentQuery for the Position component.
        /// </summary>
        IAddComponentQuery<Vector3> AddPositionQuery { get; }

        /// <summary>
        /// IModifyComponentQuery for the Position component.
        /// </summary>
        IModifyComponentQuery<Vector3> ModifyPositionQuery { get; }

        /// <summary>
        /// IRemoveComponentQuery for the Position component.
        /// </summary>
        IRemoveComponentQuery RemovePositionQuery { get; }

        /// <summary>
        /// IAddComponentQuery for the Material component.
        /// </summary>
        IAddComponentQuery<int> AddMaterialQuery { get; }

        /// <summary>
        /// IModifyComponentQuery for the Material component.
        /// </summary>
        IModifyComponentQuery<int> ModifyMaterialQuery { get; }

        /// <summary>
        /// IRemoveComponentQuery for the Material component.
        /// </summary>
        IRemoveComponentQuery RemoveMaterialQuery { get; }

        /// <summary>
        /// IAddComponentQuery for the MaterialModifier component.
        /// </summary>
        IAddComponentQuery<int> AddMaterialModifierQuery { get; }

        /// <summary>
        /// IModifyComponentQuery for the MaterialModifier component.
        /// </summary>
        IModifyComponentQuery<int> ModifyMaterialModifierQuery { get; }

        /// <summary>
        /// IRemoveComponentQuery for the MaterialModifier component.
        /// </summary>
        IRemoveComponentQuery RemoveMaterialModifierQuery { get; }

        /// <summary>
        /// Initializes the persistence provider.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Cleans up the persistence provider.
        /// </summary>
        void Cleanup();

    }

}
