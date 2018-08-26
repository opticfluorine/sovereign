using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine8.EngineUtil.IoC
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
