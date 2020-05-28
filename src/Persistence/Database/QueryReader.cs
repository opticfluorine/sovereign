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

using System;
using System.Data;

namespace Sovereign.Persistence.Database
{

    /// <summary>
    /// Encapsulates a command/reader pair that can be disposed together.
    /// </summary>
    public sealed class QueryReader : IDisposable
    {
        private readonly IDbCommand command;

        /// <summary>
        /// Reader.
        /// </summary>
        public IDataReader Reader { get; }

        public QueryReader(IDbCommand command, IDataReader reader)
        {
            this.command = command;
            Reader = reader;
        }

        public QueryReader(IDbCommand command)
            : this(command, command.ExecuteReader())
        {

        }

        public void Dispose()
        {
            command.Dispose();
            Reader.Dispose();
        }

    }
    
}
