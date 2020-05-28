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
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.D3D11Renderer.Rendering
{

    /// <summary>
    /// IoC installer for the D3D11 renderer.
    /// </summary>
    public class D3D11RenderingInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<D3D11Device>()
                .LifestyleSingleton());

            container.Register(Classes.FromThisAssembly()
                .BasedOn<IRenderStage>()
                .WithServiceDefaultInterfaces()
                .LifestyleSingleton()
                .AllowMultipleMatches());

            container.Register(Component.For<D3D11SceneConsumer>()
                .LifestyleSingleton());
        }
    }
}
