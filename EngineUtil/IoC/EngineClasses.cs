/*
 * Sovereign Engine
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

using Castle.MicroKernel.Registration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sovereign.EngineUtil.IoC
{

    /// <summary>
    /// Utility class for matching engine assemblies in Castle.Windsor installers.
    /// </summary>
    public static class EngineClasses
    {

        /// <summary>
        /// Selects all available assemblies belonging to the engine.
        /// </summary>
        /// <returns>Descriptor covering the assemblies belonging to the engine.</returns>
        public static FromAssemblyDescriptor EngineAssemblies()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(thisAssembly.Location);
            var prefix = thisAssembly.FullName.Substring(0, thisAssembly.FullName.IndexOf('.'));

            return Classes.FromAssemblyInDirectory(EngineAssemblyFilter());
        }

        /// <summary>
        /// Creates an AssemblyFilter that matches all available assemblies
        /// belonging to the engine.
        /// </summary>
        /// <returns>AssemblyFilter matching engine assemblies.</returns>
        public static AssemblyFilter EngineAssemblyFilter()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(thisAssembly.Location);
            var prefix = thisAssembly.FullName.Substring(0, thisAssembly.FullName.IndexOf('.'));

            return new AssemblyFilter(dir).FilterByName(name => name.FullName.StartsWith(prefix));
        }

    }

}
