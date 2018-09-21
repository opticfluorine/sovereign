﻿/*
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
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Sovereign.EngineCore.Systems;
using Sovereign.EngineUtil.IoC;

namespace Sovereign.EngineCore.Events
{

    /// <summary>
    /// IoC installer for the event handling infrastructure.
    /// </summary>
    public class EventInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* Event loop. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IEventLoop>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton());

            /* Event communicators. */
            container.Register(Component.For<EventCommunicator>()
                .LifestyleTransient());

            /* Event senders. */
            container.Register(Component.For<EventSender>()
                .LifestyleTransient());

            /* Event adapters. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<IEventAdapter>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton());
        }

    }

}
