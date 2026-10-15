// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Persistence.Database;

namespace Sovereign.Persistence.Data;

/// <summary>
///     Processes global key-value pairs from the database.
/// </summary>
public class GlobalKeyValueProcessor
{
    private const int IndexKey = 0;
    private const int IndexValue = 1;
    private const int IndexType = 2;
    private readonly IDataController dataController;

    public GlobalKeyValueProcessor(IDataController dataController)
    {
        this.dataController = dataController;
    }

    /// <summary>
    ///     Processes global key-value pairs from the database.
    /// </summary>
    /// <param name="reader">
    ///     Data reader. First column must be keys (string), second column values (string),
    ///     and third column type tags (integer).
    /// </param>
    /// <returns>Number of key-value pairs loaded.</returns>
    /// <exception cref="FatalErrorException">Thrown if a stored value cannot be decoded.</exception>
    public int Process(QueryReader reader)
    {
        var count = 0;

        while (reader.Reader.Read())
        {
            count++;

            var key = reader.Reader.GetString(IndexKey);
            var value = reader.Reader.GetString(IndexValue);

            DataValue dataValue;
            try
            {
                var type = reader.Reader.GetInt32(IndexType);
                dataValue = DataValueCodec.Decode((DataValueType)type, value);
            }
            catch (Exception e)
            {
                throw new FatalErrorException($"Failed to decode global value for key '{key}'.", e);
            }

            dataController.SetGlobalSync(key, dataValue);
        }

        return count;
    }
}
