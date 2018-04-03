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
using System.Reflection;
using log4net;

namespace Engine8.EngineCore.Systems
{

    /// <summary>
    /// Describes a system.
    /// </summary>
    class SystemDescription
    {

        /// <summary>
        /// Class-level logger.
        /// </summary>
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SystemDescription));

        /// <summary>
        /// Fully qualified name of the system type.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type of the system class.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Names and types of dependencies to be injected.
        /// </summary>
        public ISet<Type> Dependencies { get; private set; }

        /// <summary>
        /// Whether the described system performs state updates.
        /// </summary>
        public bool DoesUpdate { get; private set; }

        /// <summary>
        /// Whether the described system performs render steps.
        /// </summary>
        public bool DoesRender { get; private set; }

        /// <summary>
        /// System constructor.
        /// </summary>
        private ConstructorInfo constructor;

        /// <summary>
        /// Creates a system description with no dependencies from the class name.
        /// </summary>
        /// <param name="name">System class name.</param>
        /// <param name="dependencies">Dependency types to be injected.</param>
        /// <param name="doesUpdate">Whether the system performs state updates.</param>
        /// <param name="doesRender">Whether the system performs rendering steps.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the type or constructor cannot be resolved.
        /// </exception>
        public SystemDescription(string name, ISet<Type> dependencies, 
            bool doesUpdate, bool doesRender)
        {
            // Set parameters
            Name = name;
            Dependencies = dependencies;
            DoesUpdate = doesUpdate;
            DoesRender = doesRender;

            // Resolve interface
            Type = ResolveType();
            constructor = LookupConstructor();
        }

        /// <summary>
        /// Instantiates a new copy of the described system.
        /// </summary>
        /// <param name="dependencies">Dependencies to be injected.</param>
        /// <returns>New system object.</returns>
        public ISystem Instantiate(IDictionary<Type, Object> dependencies)
        {
            // Prepare the dependencies for injection
            var pInfos = constructor.GetParameters();
            var query =
                from pInfo in pInfos
                select dependencies[pInfo.ParameterType];
            var constructorParams = query.ToArray();

            // Instantiate a new System object
            var sb = new StringBuilder("Instantiating ").Append(Name);
            LOG.Debug(sb.ToString());

            return (ISystem) constructor.Invoke(constructorParams);
        }

        /// <summary>
        /// Resolves the type of the described System.
        /// </summary>
        private Type ResolveType()
        {
            // Attempt to look up the type
            var sb = new StringBuilder("Loading system ").Append(Name);
            LOG.Debug(sb.ToString());

            var assembly = Assembly.GetExecutingAssembly();
            var candidateType = assembly.GetType(Name);
            if (candidateType == null)
            {
                sb.Clear().Append("System ").Append(Name).Append(" not found.");
                LOG.Error(sb.ToString());
                throw new ArgumentException(sb.ToString());
            }

            // Verify that the type is a System
            if (!candidateType.IsSubclassOf(typeof(ISystem)))
            {
                sb.Clear().Append("System ").Append(Name).Append(" is not an ISystem.");
                LOG.Error(sb.ToString());
                throw new ArgumentException(sb.ToString());
            }

            // All done
            return candidateType;
        }

        /// <summary>
        /// Attempts to determine the correct dependency injection constructor.
        /// </summary>
        private ConstructorInfo LookupConstructor()
        {
            // Iterate over the public constructors
            var constructorInfo = Type.GetConstructors();
            foreach (var currentInfo in constructorInfo)
            {
                // Determine the types of the constructor parameters
                ISet<Type> paramTypes = new HashSet<Type>(
                    from x in currentInfo.GetParameters()
                    select x.ParameterType
                );

                // Check for complete overlap with the dependency type set
                if (paramTypes.Count == currentInfo.GetParameters().Length &&
                    paramTypes.SetEquals(Dependencies))
                {
                    // Correct constructor located
                    return currentInfo;
                }
            }

            // If we get here, no constructor matches the dependency set
            throw new ArgumentException("No suitable constructor found.");
        }

    }

}
