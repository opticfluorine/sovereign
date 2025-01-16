// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Data;
using Microsoft.Data.Sqlite;
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Database.Sqlite.Queries;

public class SqliteModifyPositionComponentQuery : IModifyComponentQuery<Kinematics>
{
    private readonly Vector3SqliteModifyComponentQuery query;

    public SqliteModifyPositionComponentQuery(SqliteConnection dbConnection)
    {
        query = new Vector3SqliteModifyComponentQuery("pos_", dbConnection);
    }

    public void Modify(ulong entityId, Kinematics value, IDbTransaction transaction)
    {
        query.Modify(entityId, value.Position, transaction);
    }
}