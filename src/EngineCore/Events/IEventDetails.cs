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

using ProtoBuf;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Interface implemented by event detail classes.
    /// </summary>
    /// <remarks>
    /// Ensure that derived types are linked with a ProtoInclude here.
    /// </remarks>
    [ProtoContract]
    [ProtoInclude(1, "SetVelocityEventDetails")]
    [ProtoInclude(2, "MoveOnceEventDetails")]
    [ProtoInclude(3, "EntityEventDetails")]
    [ProtoInclude(4, "EntityVectorEventDetails")]
    public interface IEventDetails
    {
    }

}
