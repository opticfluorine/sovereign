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

using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineUtil.IoC
{

    /// <summary>
    /// IoC-related utility methods.
    /// </summary>
    public static class IoCUtil
    {

        /// <summary>
        /// Creates an IoC container with default settings.
        /// </summary>
        /// <returns>IoC container.</returns>
        public static IWindsorContainer InitializeIoC()
        {
            /* Create the container. */
            var iocContainer = new WindsorContainer();

            /* Add additional resolvers. */
            iocContainer.Kernel.Resolver.AddSubResolver(
                new CollectionResolver(iocContainer.Kernel, true));

            /* Add facilities. */
            iocContainer.AddFacility<StartableFacility>(f => f.DeferredStart());
            iocContainer.AddFacility<TypedFactoryFacility>();

            /* Install components. */
            iocContainer.Install(FromAssembly.InDirectory(
                EngineClasses.EngineAssemblyFilter()));

            return iocContainer;
        }

    }

}
