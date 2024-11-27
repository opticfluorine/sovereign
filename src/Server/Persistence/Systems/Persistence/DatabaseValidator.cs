/*
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
using Microsoft.Extensions.Logging;
using Sovereign.Persistence.Database;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Validates the compatibility of the connected database.
/// </summary>
public sealed class DatabaseValidator
{
    public const int LatestMigration = 1;
    private readonly ILogger<DatabaseValidator> logger;

    public DatabaseValidator(ILogger<DatabaseValidator> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Validates the database associated with the given IPersistenceProvider.
    /// </summary>
    /// <param name="provider">Persistence provider to validate.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool ValidateDatabase(IPersistenceProvider provider)
    {
        try
        {
            var valid = provider.MigrationQuery.IsMigrationLevelApplied(LatestMigration);
            if (!valid)
                logger.LogCritical("Database is not at migration level {Level}.", LatestMigration);
            else
                logger.LogInformation("Database is at migration level {Level}.", LatestMigration);
            return valid;
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Error checking database migration level.");
            return false;
        }
    }
}