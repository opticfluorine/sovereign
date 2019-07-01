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

using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// Describes the structure of all events.
    /// </summary>
    public sealed class EventDescriptions
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Map from event ID to detail type.
        /// </summary>
        private readonly IDictionary<EventId, Type> detailTypes
            = new Dictionary<EventId, Type>();

        /// <summary>
        /// Gets the details type associated with the given event ID.
        /// </summary>
        /// <param name="eventId">Event ID.</param>
        /// <returns>Details type, or null if the event has no details.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the event ID has not been registered.
        /// </exception>
        public Type GetDetailType(EventId eventId)
        {
            if (detailTypes.ContainsKey(eventId))
            {
                return detailTypes[eventId];
            }
            else
            {
                throw new KeyNotFoundException("Event ID " + eventId + " not recognized.");
            }
        }

        /// <summary>
        /// Registers the given event with the given details type.
        /// </summary>
        /// <typeparam name="T">Event details type.</typeparam>
        /// <param name="eventId">Event ID.</param>
        public void RegisterEvent<T>(EventId eventId) where T : IEventDetails
        {
            RegisterEvent(eventId, typeof(T));
        }

        /// <summary>
        /// Registers the given event with no details.
        /// </summary>
        /// <param name="eventId"></param>
        public void RegisterNullEvent(EventId eventId)
        {
            RegisterEvent(eventId, null);
        }

        /// <summary>
        /// Logs debug info about the current mapping.
        /// </summary>
        public void LogDebugInfo()
        {
            Logger.Debug("Current event detail mapping:");
            var orderedKeys = detailTypes.Keys
                .OrderBy(id => id.ToString(), StringComparer.CurrentCulture);
            foreach (var key in orderedKeys)
            {
                var type = detailTypes[key];
                Logger.DebugFormat("{0} => {1}", key.ToString(), type != null ? type.Name : "null");
            }
        }

        private void RegisterEvent(EventId eventId, Type detailType)
        {
            lock (detailTypes)
            {
                detailTypes[eventId] = detailType;
            }
        }

    }

}
