/*
 * Sovereign Engine
 * Copyright (c) 2026 opticfluorine
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
using System.IO;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database;

/// <summary>
///     Applies pending migration scripts to a database, each within its own transaction.
/// </summary>
public sealed class MigrationRunner
{
    private readonly IDbConnection connection;
    private readonly ILogger logger;
    private readonly MigrationScriptLocator locator;
    private readonly IMigrationQuery migrationQuery;

    /// <summary>
    ///     Creates a migration runner.
    /// </summary>
    /// <param name="connection">Open database connection to migrate.</param>
    /// <param name="migrationQuery">Query used to read the current migration level.</param>
    /// <param name="locator">Locator used to find pending migration scripts.</param>
    /// <param name="logger">Logger.</param>
    public MigrationRunner(IDbConnection connection, IMigrationQuery migrationQuery,
        MigrationScriptLocator locator, ILogger logger)
    {
        this.connection = connection;
        this.migrationQuery = migrationQuery;
        this.locator = locator;
        this.logger = logger;
    }

    /// <summary>
    ///     Applies all pending migration scripts up to and including the given level.
    /// </summary>
    /// <param name="latestMigration">Latest migration level expected by the application.</param>
    public void ApplyMigrations(int latestMigration)
    {
        var currentLevel = migrationQuery.GetLatestMigrationLevel();
        if (currentLevel >= latestMigration) return;

        foreach (var script in locator.GetPendingScripts(currentLevel))
        {
            // Pending scripts are sorted by level; stop once the application's latest level is passed.
            if (script.Level > latestMigration) break;

            ApplyScript(script);
        }
    }

    /// <summary>
    ///     Applies a single migration script within a transaction.
    /// </summary>
    /// <param name="script">Migration script to apply.</param>
    private void ApplyScript(MigrationScript script)
    {
        try
        {
            using var transaction = connection.BeginTransaction();

            var sql = File.ReadAllText(script.Filename);
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            command.ExecuteNonQuery();

            transaction.Commit();
            logger.LogInformation("Applied migration {Level} from {Filename}.", script.Level, script.Filename);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to apply migration {Level} from {Filename}.", script.Level,
                script.Filename);
            throw new FatalErrorException("Failed to apply database migration.", e);
        }
    }
}
