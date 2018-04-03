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
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using log4net;

namespace Engine8.EngineCore.Events
{

    /// <summary>
    /// Builds events with their detail classes.
    /// </summary>
    public class EventFactory
    {

        private static readonly ILog classLog
            = LogManager.GetLogger(typeof(EventFactory));

        /// <summary>
        /// Map from event IDs to IEventDetails classes.
        /// </summary>
        private readonly IDictionary<int, IList<Type>> idDetailsMap;

        /// <summary>
        /// Map from event IDs to IEventProvider objects.
        /// </summary>
        private readonly IDictionary<int, IEventProvider> idProviderMap;

        /// <summary>
        /// Creates a new EventFactory object.
        /// </summary>
        /// <param name="SearchDirectory">
        /// Search directoryy for assemblies.
        /// If null, only the EventCore assembly will be searched.
        /// </param>
        public EventFactory(string SearchDirectory)
        {
            // create data structures
            idDetailsMap = new Dictionary<int, IList<Type>>();
            idProviderMap = new Dictionary<int, IEventProvider>();

            // load all available event providers
            LocateEventProviders(SearchDirectory);
        }

        /// <summary>
        /// Creates a new event with the given ID and with default details.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <returns>New event with the given ID.</returns>
        public Event CreateEvent(int id)
        {
            IList<IEventDetails> details;
            if (idDetailsMap.ContainsKey(id))
            {
                // get default event details from the event provider
                var provider = idProviderMap[id];
                details = provider.GetDefaultDetails(id);
            }
            else
            {
                // No EventProvider known for this event type
                var sb = new StringBuilder("No EventProvider known for event ID ");
                sb.Append(id);
                sb.Append(", proceeding with no details.");
                classLog.Warn(sb.ToString());

                details = new List<IEventDetails>();
            }

            return CreateEvent(id, details, false);
        }

        /// <summary>
        /// Creates a new event of the given type and with the given details.
        /// The details will be validated by the corresponding EventProvider.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <param name="details">Details.</param>
        /// <param name="validate">If true, validate the details.</param>
        /// <returns>New event.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if details is null.
        /// </exception>
        public Event CreateEvent(int id, IList<IEventDetails> details, bool validate=true)
        {
            // input check
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            // create the event
            var ev = new Event(id, details);

            // validate if necessary
            if (validate)
            {
                var provider = idProviderMap[id];
                if (!provider.IsEventValid(ev))
                {
                    // invalid event
                    var sb = new StringBuilder("Validation failed for event with ID ");
                    sb.Append(id);
                    classLog.Error(sb.ToString());
                    throw new ArgumentException(sb.ToString(), nameof(details));
                }
            }

            // all done
            return ev;
        }

        /// <summary>
        /// Dynamically locates event providers by checking all of the
        /// assemblies in a directory along with the EventCore assembly.
        /// </summary>
        /// <param name="searchDirectory">Path to the directory to search.</param>
        private void LocateEventProviders(string searchDirectory)
        {
            // iterate over the class libraries in the search directory
            if (searchDirectory != null)
            {
                foreach (string assemblyPath in Directory.GetFiles(searchDirectory, "*.dll"))
                {
                    LoadEventProviders(assemblyPath);
                }
            }

            // load whatever is in the EventCore assembly
            var ecAssembly = GetType().Assembly;
            LoadEventProvidersFromAssembly(ecAssembly);
        }

        /// <summary>
        /// Dynamically loads any event providers found in the given directory.
        /// </summary>
        /// <param name="assemblyPath"></param>
        private void LoadEventProviders(string assemblyPath)
        {
            // try to load the assembly from the file
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception e)
            {
                // Can't load this assembly, move along
                classLog.Debug("Failed to load assembly " + assemblyPath, e);
                return;
            }

            // search the assembly for event providers
            LoadEventProvidersFromAssembly(assembly);
        }

        /// <summary>
        /// Loads event providers from the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        private void LoadEventProvidersFromAssembly(Assembly assembly)
        {
            // iterate over the event providers in the assembly
            var types = assembly.GetTypes();
            var query = from t in types
                        where t.IsSubclassOf(typeof(IEventProvider))
                        select t;
            foreach (Type provider in query)
            {
                // instantiate the provider
                IEventProvider eventProvider;
                try
                {
                    eventProvider = (IEventProvider) Activator.CreateInstance(provider);
                }
                catch (Exception e)
                {
                    // something is wrong with this provider, skip it
                    classLog.Debug("Failed to instantiate event provider " + provider.Name, e);
                    continue;
                }

                // load the provider
                LoadEventProvider(eventProvider);
            }
        }

        /// <summary>
        /// Loads the given event provider.
        /// </summary>
        /// <param name="provider">Event provider.</param>
        private void LoadEventProvider(IEventProvider provider)
        {
            foreach (int id in provider.IdToRequiredDetailsMap.Keys)
            {
                // check for duplicates
                if (idDetailsMap.ContainsKey(id))
                {
                    // duplicate id located, log and skip
                    var sb = new StringBuilder();
                    sb.Append("Duplicate event ID ");
                    sb.Append(id);
                    sb.Append(" encountered in provider ");
                    sb.Append(provider.GetType().Name);

                    classLog.Debug(sb.ToString());
                    continue;
                }

                // register the event provider
                idDetailsMap[id] = provider.IdToRequiredDetailsMap[id];
            }
        }

    }

}
