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
