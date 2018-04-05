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

using Engine8.EngineCore.Timing;
using System.Xml.Linq;
using System;

namespace Engine8.EngineCore.Configuration
{

    /// <summary>
    /// Parses the CoreServices section of the engine configuration.
    /// </summary>
    class CoreServicesProcessor
    {

        /// <summary>
        /// Core services XML tag.
        /// </summary>
        private static readonly string TAG_CORE_SERVICES = "CoreServices";

        /// <summary>
        /// Tag name for the SystemTimer service.
        /// </summary>
        private const string TAG_SYSTEMTIMER = "ISystemTimer";

        /// <summary>
        /// System timer.
        /// </summary>
        public ISystemTimer SystemTimer { get; private set; }

        /// <summary>
        /// Parses the CoreServices section.
        /// </summary>
        /// <param name="configRoot">Root element of the engine configuration.</param>
        public CoreServicesProcessor(XElement configRoot)
        {
            /* Retrieve the CoreServices section. */
            var serviceRoot = configRoot.Descendants(TAG_CORE_SERVICES);
            foreach (var serviceElement in serviceRoot) {
                switch (serviceElement.Name.ToString())
                {
                    case TAG_SYSTEMTIMER:
                        ParseSystemTimer(serviceElement);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the SystemTimer element.
        /// </summary>
        /// <param name="systemTimerElement">SystemTimer element.</param>
        private void ParseSystemTimer(XElement systemTimerElement)
        {
            string className = systemTimerElement.Value;
            SystemTimer = (ISystemTimer)CreateObjectByReflection(className);
        }

        /// <summary>
        /// Instantiates a core service by reflection given a fully qualified class name.
        /// </summary>
        /// <param name="className">Fully qualified class name.</param>
        /// <returns>Instantiated service object.</returns>
        private Object CreateObjectByReflection(string className)
        {
            var objectType = Type.GetType(className, true);
            return Activator.CreateInstance(objectType);
        }

    }

}
