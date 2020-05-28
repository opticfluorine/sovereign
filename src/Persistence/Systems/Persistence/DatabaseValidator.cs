﻿/*
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

using Castle.Core.Logging;
using Sovereign.Persistence.Database;
using System;

namespace Sovereign.Persistence.Systems.Persistence
{

    /// <summary>
    /// Validates the compatibility of the connected database.
    /// </summary>
    public sealed class DatabaseValidator
    {

        public const int LatestMigration = 1;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Validates the database associated with the given IPersistenceProvider.
        /// </summary>
        /// <param name="provider">Persistence provider to validate.</param>
        /// <returns>true if valid, false otherwise.</returns>
        public bool ValidateDatabase(IPersistenceProvider provider)
        {
            try
            {
                var valid = provider.MigrationQuery.IsMigrationLevelApplied(LatestMigration);
                if (!valid)
                {
                    Logger.FatalFormat("Database is not at migration level {0}.", LatestMigration);
                }
                else
                {
                    Logger.InfoFormat("Database is at migration level {0}.", LatestMigration);
                }
                return valid;
            }
            catch (Exception e)
            {
                Logger.Fatal("Error checking database migration level.", e);
                return false;
            }
        }

    }

}
