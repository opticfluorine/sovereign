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
using System.Reflection;
using log4net;
using System.Text;
using System.Xml.Linq;

namespace Engine8.EngineCore.Systems
{

    /// <summary>
    /// Enumerates the avaiable Systems from the Systems.xml file.
    /// </summary>
    class SystemEnumerator
    {

        /// <summary>
        /// Class-level logger.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SystemEnumerator));

        /// <summary>
        /// Name of the XML file containing the system registry.
        /// </summary>
        private static readonly string SYSTEMS_FILE = @"Config\Systems.xml";

        /// <summary>
        /// XML tag encapsulating individual systems.
        /// </summary>
        private static readonly string TAG_SYSTEM = "System";

        /// <summary>
        /// XML tag encapsulating the system name.
        /// </summary>
        private static readonly string TAG_NAME = "Name";

        /// <summary>
        /// XML tag encapsulating a dependency type for a system.
        /// </summary>
        private static readonly string TAG_DEPENDENCY = "Dependency";

        /// <summary>
        /// XML tag encapsulating a state update flag.
        /// </summary>
        private static readonly string TAG_DOESUPDATE = "DoesUpdate";

        /// <summary>
        /// XML tag encapsulating a render flag.
        /// </summary>
        private static readonly string TAG_DOESRENDER = "DoesRender";

        /// <summary>
        /// Enumerates the System classes to be managed.
        /// </summary>
        /// <returns>Set of System class types.</returns>
        public static ISet<SystemDescription> EnumerateSystems()
        {
            return new HashSet<SystemDescription>(EnumerateSystemDescriptions());
        }

        /// <summary>
        /// Parses the system registry file to obtain a list of system descriptions.
        /// </summary>
        /// <returns>Collection of system descriptions.</returns>
        private static IEnumerable<SystemDescription> EnumerateSystemDescriptions()
        {
            // Load the file
            var root = XElement.Load(SYSTEMS_FILE);

            // Parse the file
            return
                from systemElement in root.Descendants(TAG_SYSTEM)
                select ParseSystemDescripton(systemElement);
        }

        /// <summary>
        /// Parses a System element in the registry to obtain a SystemDescription.
        /// </summary>
        /// <param name="systemElement">System element from the registry.</param>
        /// <returns></returns>
        private static SystemDescription ParseSystemDescripton(XElement systemElement)
        {
            // Parse the system name
            string name;
            try
            {
                name = systemElement.Descendants(TAG_NAME).First().Value;
            }
            catch (Exception e)
            {
                var sb = new StringBuilder("Could not parse system name: ")
                    .Append(e.Message);
                LOG.Error(sb.ToString());
                throw new Exception(sb.ToString());
            }

            // Parse the dependencies
            var assembly = Assembly.GetExecutingAssembly();
            ISet<Type> dependencies = new HashSet<Type>(
                from depElement in systemElement.Descendants(TAG_DEPENDENCY)
                select ResolveDependency(depElement.Value)
            );

            // Parse the update/render flags
            bool doesUpdate = false, doesRender = false;
            try
            {
                doesUpdate = bool.Parse(systemElement.Descendants(TAG_DOESUPDATE).First().Value);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder("Could not parse ").Append(TAG_DOESUPDATE)
                    .Append("for system ").Append(name).Append(": ").Append(e.Message);
                LOG.Error(sb.ToString());
            }
            try
            {
                doesRender = bool.Parse(systemElement.Descendants(TAG_DOESRENDER).First().Value);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder("Could not parse ").Append(TAG_DOESRENDER)
                    .Append(" for system ").Append(name).Append(": ").Append(e.Message);
                LOG.Error(sb.ToString());
            }

            // Create the system description
            return new SystemDescription(name, dependencies, doesUpdate, doesRender);
        }

        /// <summary>
        /// Attempts to resolve a dependency type.
        /// </summary>
        /// <param name="name">Fully qualified type name.</param>
        /// <returns>Resolved type.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the type cannot be resolved from the given name.
        /// </exception>
        private static Type ResolveDependency(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var t = assembly.GetType(name);
            if (t == null)
            {
                var sb = new StringBuilder("Type ").Append(name)
                    .Append(" cannot be resolved.");
                LOG.Error(sb.ToString());
                throw new ArgumentException(sb.ToString());
            }
            return t;
        }

    }

}
