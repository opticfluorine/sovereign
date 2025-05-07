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
using Sovereign.Scripting.Lua;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Lua library that provides access to DataSystem data.
/// </summary>
public class DataLuaLibrary : ILuaLibrary
{
    private readonly IDataController dataController;

    public DataLuaLibrary(IDataController dataController, IEventSender eventSender)
    {
        this.dataController = dataController;
    }

    public void Install(LuaHost luaHost)
    {
        try
        {
            luaHost.BeginLibrary("data");
        }
        finally
        {
            luaHost.EndLibrary();
        }
    }
}