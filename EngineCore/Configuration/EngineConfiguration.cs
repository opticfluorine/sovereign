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

namespace Engine8.EngineCore.Configuration
{

    /// <summary>
    /// Runtime configuration to be supplied during startup.
    /// </summary>
    public class EngineConfiguration
    {

        /// <summary>
        /// Engine configuration filename.
        /// </summary>
        public static readonly string CONFIG_FILENAME = "Engine.xml";

        /// <summary>
        /// System timer.
        /// </summary>
        public ISystemTimer SystemTimer { get; private set; }

        /// <summary>
        /// Creates an immutable configuration object and populates it from the XML file.
        /// </summary>
        public EngineConfiguration()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Loads the configuration from the XML file.
        /// </summary>
        private void LoadConfiguration()
        {
            /* Open the XML file for parsing. */
            var configRoot = XElement.Load(CONFIG_FILENAME);
        }

        /// <summary>
        /// Parses the CoreServices section of the configuration.
        /// </summary>
        /// <param name="configRoot">Root configuration element.</param>
        private void ProcessCoreServices(XElement configRoot)
        {
            /* Process the section. */
            var processor = new CoreServicesProcessor(configRoot);

            /* Propagate the loaded services. */
            SystemTimer = processor.SystemTimer;
        }

    }

}
