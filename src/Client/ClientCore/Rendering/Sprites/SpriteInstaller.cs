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

namespace Sovereign.ClientCore.Rendering.Sprites
{

    /// <summary>
    /// IoC installer for sprite-related services.
    /// </summary>
    public class SpriteInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            /* SurfaceLoader. */
            container.Register(Component.For<SurfaceLoader>()
                .LifestyleSingleton());

            /* SpriteSheetFactory. */
            container.Register(Component.For<SpriteSheetFactory>()
                .LifestyleSingleton());

            /* SpriteSheetManager. */
            container.Register(Component.For<SpriteSheetManager>()
                .LifestyleSingleton());

            /* SpriteSheetDefinitionLoader. */
            container.Register(Component.For<SpriteSheetDefinitionLoader>()
                .LifestyleSingleton());

            /* SpriteSheetDefinitionValidator. */
            container.Register(Component.For<SpriteSheetDefinitionValidator>()
                .LifestyleSingleton());

            /* SpriteManager. */
            container.Register(Component.For<SpriteManager>()
                .LifestyleSingleton());

            /* SpriteDefinitionsLoader. */
            container.Register(Component.For<SpriteDefinitionsLoader>()
                .LifestyleSingleton());

            /* SpriteDefinitionsValidator. */
            container.Register(Component.For<SpriteDefinitionsValidator>()
                .LifestyleSingleton());
        }

    }

}
