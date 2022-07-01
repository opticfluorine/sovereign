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
