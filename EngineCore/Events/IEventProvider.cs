/*
 * Engine8 Dynamic World MMORPG Engine
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

using System;
using System.Collections.Generic;

namespace Engine8.EngineCore.Events
{

    /// <summary>
    /// Interface implemented by event provider classes.  These classes are
    /// dynamically located and loaded at construction of an EventFactory.
    /// </summary>
    public interface IEventProvider
    {

        /// <summary>
        /// Map from event ID to a list of IEventDetails classes that must be
        /// instantiated for each event.
        /// </summary>
        IDictionary<int, IList<Type>> IdToRequiredDetailsMap { get; }

        /// <summary>
        /// Gets a default list of event details for the given event ID.
        ///
        /// Implementers must return a valid list for each ID included in the
        /// IDToRequiredDetailsMap.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <returns>List of default event details.</returns>
        IList<IEventDetails> GetDefaultDetails(int id);

        /// <summary>
        /// Validates the given event.
        ///
        /// Implementers must validate each event type included in the
        /// IdToRequiredDetailsMap.
        /// </summary>
        /// <param name="ev">Event to be validated.</param>
        /// <returns>true if the event is valid, false otherwise.</returns>
        bool IsEventValid(Event ev);

    }

}
