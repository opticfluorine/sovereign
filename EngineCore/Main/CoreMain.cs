using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Main
{

    /// <summary>
    /// Common main class for the engine.
    /// </summary>
    public class CoreMain
    {

        public void RunEngine()
        {
            /* Start up the IoC container */
            var iocContainer = InitializeIoC();

            /* Shut down the IoC container */
            ShutdownIoC(iocContainer);
        }

        private IWindsorContainer InitializeIoC()
        {
            /* Create the container. */
            var iocContainer = new WindsorContainer()
                .Install(FromAssembly.InThisApplication());

            return iocContainer;
        }

        private void ShutdownIoC(IWindsorContainer IocContainer)
        {
            IocContainer.Dispose();
        }

    }

}
