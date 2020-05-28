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
