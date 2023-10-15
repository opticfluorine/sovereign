﻿/*
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

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     IoC installer for account authentication classes.
/// </summary>
public sealed class AccountsAuthenticationInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(Component.For<AccountAuthenticator>()
            .LifestyleSingleton());

        container.Register(Component.For<AccountLoginTracker>()
            .LifestyleSingleton());

        container.Register(Component.For<AuthenticationAttemptLimiter>()
            .LifestyleSingleton());

        container.Register(Component.For<SharedSecretManager>()
            .LifestyleSingleton());

        container.Register(Component.For<LoginHandoffTracker>()
            .LifestyleSingleton());
    }
}