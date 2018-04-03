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
using System.Text;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Engine8.EngineCore.Logging;
using Engine8.EngineCore.Events;

namespace Engine8.EngineCore.Systems
{

    /// <summary>
    /// Manages the collection of System objects.
    /// </summary>
    public class SystemManager
    {

        /// <summary>
        /// Class-level logger.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SystemManager));

        /// <summary>
        /// Update time interval in microseconds.
        /// </summary>
        public ulong UpdateStep { get; private set; }

        /// <summary>
        /// Convenience property providing access to the set of all systems.
        /// </summary>
        public ISet<ISystem> AllSystems { get; private set; }

        /// <summary>
        /// Set of System objects which perform state updates.
        /// </summary>
        public ISet<ISystem> UpdatingSystems { get; private set; }

        /// <summary>
        /// Set of System objects which perform rendering steps.
        /// </summary>
        public ISet<ISystem> RenderingSystems { get; private set; }

        /// <summary>
        /// Dictionary holding dependencies to be injected to the systems.
        /// </summary>
        public IDictionary<Type, Object> Dependencies { get; private set; }

        /// <summary>
        /// Reference to the main event loop.
        /// </summary>
        public IEventLoop EventLoop { get; private set; }

        /// <summary>
        /// Creates a new SystemManager with the given update step.
        /// </summary>
        public SystemManager()
        {

        }

        /// <summary>
        /// Starts the system manager.
        /// </summary>
        public void Startup()
        {
            LoadSystems();
            InitializeSystems();
        }

        /// <summary>
        /// Stops the system manager.
        /// </summary>
        public void Stop()
        {
            CleanupSystems();
        }

        /// <summary>
        /// Performs a state update of all systems that support
        /// state updates.
        /// </summary>
        /// <param name="systemTime">System time in microseconds.</param>
        public void DoStateUpdate(ulong systemTime)
        {
            /* Iterate over systems that support state updates. */
            foreach (ISystem system in UpdatingSystems)
            {
                system.DoUpdate(systemTime);
            }

            /* Ensure that all events for immediate dispatch are sent. */
            
        }

        /// <summary>
        /// Invokes rendering on all systems that support rendering.
        /// </summary>
        /// <param name="timeSinceUpdate">
        /// Time in microseconds since the last state update.
        /// </param>
        public void DoRender(ulong timeSinceUpdate)
        {
            /* Iterate over systems that support rendering. */
            foreach (ISystem system in RenderingSystems)
            {
                system.DoRender(timeSinceUpdate);
            }
        }

        /// <summary>
        /// Registers the main event framework with the system manager.
        /// This may only be called once.
        /// </summary>
        /// <param name="loop">Main event loop.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the event framework has already been registered.
        /// </exception>
        public void RegisterEventFramework(IEventLoop loop)
        {
            /* State check. */
            if (EventLoop != null)
            {
                throw new InvalidOperationException("Event framework has already been registered.");
            }

            /* Register the event framework. */
            EventLoop = loop;
        }

        /// <summary>
        /// Loads the systems.
        /// </summary>
        private void LoadSystems()
        {
            // Any unhandled failure is a fatal error.
            try
            {
                // Load the service registry.
                var descriptions = SystemEnumerator.EnumerateSystems();

                // Instantiate the dual-role (update/render) systems.
                var dualSystems = from x in descriptions
                                  where x.DoesRender && x.DoesUpdate
                                  select x.Instantiate(Dependencies);

                // Instantiate the update-only systems.
                var updateSystems = from x in descriptions
                                    where x.DoesUpdate && !x.DoesRender
                                    select x.Instantiate(Dependencies);

                // Instantiate the render-only systems.
                var renderSystems = from x in descriptions
                                    where x.DoesRender && !x.DoesUpdate
                                    select x.Instantiate(Dependencies);

                // Load the systems into the sets.
                UpdatingSystems = new HashSet<ISystem>(updateSystems.Union(dualSystems));
                RenderingSystems = new HashSet<ISystem>(renderSystems.Union(dualSystems));
                AllSystems = new HashSet<ISystem>(UpdatingSystems.Union(RenderingSystems));
            }
            catch (Exception e)
            {
                // Fatal error.
                var sb = new StringBuilder("Error loading systems: ")
                    .Append(e.Message);
                LOG.Fatal(sb.ToString());
                ErrorHandler.Fatal(sb.ToString());
            }
        }

        /// <summary>
        /// Initializes all systems after they have been loaded.
        /// </summary>
        private void InitializeSystems()
        {
            foreach (ISystem system in AllSystems)
            {
                system.InitializeSystem(this);
            }
        }

        /// <summary>
        /// Cleans up the systems.
        /// </summary>
        private void CleanupSystems()
        {
            /* Iterate over the systems. */
            foreach (ISystem system in AllSystems)
            {
                system.CleanupSystem();
            }

            /* Clear the system sets. */
            RenderingSystems.Clear();
            UpdatingSystems.Clear();
        }

        /// <summary>
        /// Ensures that all events that need to be dispatched during
        /// an update step are dispatched.
        /// </summary>
        private void DispatchAllEvents()
        {
            /* Continuously pump the event loop until no events remain. */
            
        }

    }
}
