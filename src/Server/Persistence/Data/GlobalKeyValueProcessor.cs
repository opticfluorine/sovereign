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

using Sovereign.EngineCore.Events;
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
    private readonly IDataController dataController;
    private readonly IEventSender eventSender;

    public GlobalKeyValueProcessor(IEventSender eventSender, IDataController dataController)
    {
        this.eventSender = eventSender;
        this.dataController = dataController;
    }

    /// <summary>
    ///     Processes global key-value pairs from the database.
    /// </summary>
    /// <param name="reader">
    ///     Data reader. First column must be keys (string), second column must be
    ///     values (string).
    /// </param>
    /// <returns>Number of key-value pairs loaded.</returns>
    public int Process(QueryReader reader)
    {
        var count = 0;

        while (reader.Reader.Read())
        {
            count++;

            var key = reader.Reader.GetString(IndexKey);
            var value = reader.Reader.GetString(IndexValue);

            dataController.SetGlobal(eventSender, key, value);
        }

        return count;
    }
}