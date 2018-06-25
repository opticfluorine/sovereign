﻿/*
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

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Engine8.EngineUtil.IoC;

namespace Engine8.EngineCore.Timing
{

    /// <summary>
    /// IoC installer for timing facilities.
    /// </summary>
    public class TimingInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* ISystemTimer. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<ISystemTimer>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton()
                );

            /* ITimedAction. */
            container.Register(EngineClasses.EngineAssemblies()
                .BasedOn<ITimedAction>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton()
                .AllowMultipleMatches()
                );

            /* TimeManager. */
            container.Register(Component.For<TimeManager>()
                .LifestyleSingleton()
                );
        }

    }

}