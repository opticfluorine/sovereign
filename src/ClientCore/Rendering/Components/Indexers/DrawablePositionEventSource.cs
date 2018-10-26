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
using Sovereign.EngineCore.Systems.Movement.Components;
using System;
using System.Numerics;

namespace Sovereign.ClientCore.Rendering.Components.Indexers
{

    /// <summary>
    /// Event source that filters position events to exclude non-drawable entities.
    /// </summary>
    public sealed class DrawablePositionEventSource : IComponentEventSource<Vector3>, IDisposable
    {

        private readonly PositionComponentCollection positionCollection;

        private readonly DrawableComponentCollection drawableCollection;

        public DrawablePositionEventSource(PositionComponentCollection positionCollection,
            DrawableComponentCollection drawableCollection)
        {
            this.positionCollection = positionCollection;
            this.drawableCollection = drawableCollection;

            positionCollection.OnStartUpdates += OnStartUpdates;
            positionCollection.OnEndUpdates += OnEndUpdates;
            positionCollection.OnComponentAdded += FilterComponentAdded;
            positionCollection.OnComponentModified += FilterComponentModified;
            positionCollection.OnComponentRemoved += FilterComponentRemoved;
        }

        public void Dispose()
        {
            positionCollection.OnComponentRemoved -= FilterComponentRemoved;
            positionCollection.OnComponentModified -= FilterComponentModified;
            positionCollection.OnComponentAdded -= FilterComponentAdded;
            positionCollection.OnEndUpdates -= OnEndUpdates;
            positionCollection.OnStartUpdates -= OnStartUpdates;
        }

        /// <summary>
        /// Filters add events to exclude non-drawable entities.
        /// </summary>
        /// <param name="id">Entity ID.</param>
        /// <param name="pos">Position.</param>
        private void FilterComponentAdded(ulong id, Vector3 pos)
        {
            if (drawableCollection.HasComponentForEntity(id))
                OnComponentAdded(id, pos);
        }

        /// <summary>
        /// Filters remove events to exclude non-drawable entities.
        /// </summary>
        /// <param name="id">Entity ID.</param>
        private void FilterComponentRemoved(ulong id)
        {
            if (drawableCollection.HasComponentForEntity(id))
                OnComponentRemoved(id);
        }

        /// <summary>
        /// Filters modify events to exclude non-drawable entities.
        /// </summary>
        /// <param name="id">Entity ID.</param>
        /// <param name="pos">Position.</param>
        private void FilterComponentModified(ulong id, Vector3 pos)
        {
            if (drawableCollection.HasComponentForEntity(id))
                OnComponentModified(id, pos);
        }

        public event EventHandler OnStartUpdates;
        public event ComponentEventDelegates<Vector3>.ComponentEventHandler OnComponentAdded;
        public event ComponentEventDelegates<Vector3>.ComponentRemovedEventHandler OnComponentRemoved;
        public event ComponentEventDelegates<Vector3>.ComponentEventHandler OnComponentModified;
        public event EventHandler OnEndUpdates;
    }

}
