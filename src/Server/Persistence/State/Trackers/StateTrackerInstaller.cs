/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Sovereign.Persistence.State.Trackers;

public sealed class StateTrackerInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(Component.For<AccountStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<TrackerManager>()
            .LifestyleSingleton());

        container.Register(Component.For<PositionStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<MaterialStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<MaterialModifierStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<PlayerCharacterStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<NameStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<ParentStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<DrawableStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<AnimatedSpriteStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<OrientationStateTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<AdminStateTracker>().LifestyleSingleton());
        container.Register(Component.For<TemplateStateTracker>().LifestyleSingleton());
        container.Register(Component.For<CastBlockShadowsStateTracker>().LifestyleSingleton());
        container.Register(Component.For<PointLightSourceStateTracker>().LifestyleSingleton());
    }
}