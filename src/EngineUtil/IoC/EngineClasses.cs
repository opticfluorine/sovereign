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
