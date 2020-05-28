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
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.EngineCore.Util
{

    /// <summary>
    /// Serialization surrogate for Vector3 structs.
    /// </summary>
    [ProtoContract]
    public struct Vector3Surrogate
    {

        [ProtoMember(1, IsRequired = true)] public float X;
        [ProtoMember(2, IsRequired = true)] public float Y;
        [ProtoMember(3, IsRequired = true)] public float Z;

        public static implicit operator Vector3(Vector3Surrogate surrogate)
        {
            return new Vector3(surrogate.X, surrogate.Y, surrogate.Z);
        }

        public static implicit operator Vector3Surrogate(Vector3 vector)
        {
            return new Vector3Surrogate()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
        }

    }

}
