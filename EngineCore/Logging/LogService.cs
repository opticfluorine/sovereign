/*
 * D3D/SDL Base Framework
 * Copyright (c) 2017 opticfluorine
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

using System.IO;
using log4net;
using log4net.Config;

namespace Engine8.EngineCore.Logging
{

    /// <summary>
    /// Manages the debug logging services.
    /// </summary>
    public class LogService
    {
        /// <summary>
        /// Path to the log4net configuration file.
        /// </summary>
        public static readonly string CONFIG_FILE = "logging.xml";

        /// <summary>
        /// Class-level log.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(LogService));

        /// <summary>
        /// Sets up logging with log4net.
        /// </summary>
        public static void SetupLogging()
        {
            XmlConfigurator.Configure(new FileInfo(CONFIG_FILE));
            LOG.Info("Logging started.");
        }

    }

}
