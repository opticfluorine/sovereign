/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Systems.Movement.Components;
using System;
using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Components.Indexers
{

    /// <summary>
    /// Event source that filters position events to exclude non-drawable entities.
    /// </summary>
    public sealed class DrawablePositionEventFilter : BaseComponentEventFilter<Vector3>
    {
        private readonly DrawableComponentCollection drawableCollection;

        public DrawablePositionEventFilter(PositionComponentCollection positionCollection,
            DrawableComponentCollection drawableCollection)
            : base(positionCollection, positionCollection)
        {
            this.drawableCollection = drawableCollection;
        }

        protected override bool ShouldAccept(ulong entityId)
        {
            return drawableCollection.HasComponentForEntity(entityId);
        }

    }

}
