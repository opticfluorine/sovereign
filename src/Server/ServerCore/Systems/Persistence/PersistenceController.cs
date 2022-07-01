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

using Sovereign.EngineCore.Events;
using System.Numerics;

namespace Sovereign.ServerCore.Systems.Persistence
{

    /// <summary>
    /// Event controller for the persistence system.
    /// </summary>
    public sealed class PersistenceController
    {

        /// <summary>
        /// Requests that the persistence engine load the given entity.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        /// <param name="persistedEntityId">Persisted entity ID.</param>
        public void RetrieveEntity(IEventSender eventSender, ulong persistedEntityId)
        {
            var ev = new Event(EventId.Server_Persistence_RetrieveEntity);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Requests that the persistence engine load all entities positioned
        /// within the given range.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        /// <param name="rangeMin">Minimum extent of range.</param>
        /// <param name="rangeMax">Maximum extent of range.</param>
        public void RetrieveEntitiesInRange(IEventSender eventSender, Vector3 rangeMin,
            Vector3 rangeMax)
        {
            var ev = new Event(EventId.Server_Persistence_RetrieveEntitiesInRange);
            eventSender.SendEvent(ev);
        }

        /// <summary>
        /// Requests that the persistence engine synchronizes with the database.
        /// </summary>
        /// <param name="eventSender">Event sender.</param>
        public void Synchronize(IEventSender eventSender)
        {
            var ev = new Event(EventId.Server_Persistence_Synchronize);
            eventSender.SendEvent(ev);
        }

    }

}
