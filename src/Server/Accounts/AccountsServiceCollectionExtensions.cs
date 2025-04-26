// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.Accounts.Accounts.Registration;
using Sovereign.Accounts.Accounts.Services;
using Sovereign.Accounts.Systems.Accounts;
using Sovereign.EngineCore.Systems;

namespace Sovereign.Accounts;

/// <summary>
///     Manages service registrations for Sovereign.Accounts.
/// </summary>
public static class AccountsServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services for Sovereign.Accounts.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSovereignAccounts(this IServiceCollection services)
    {
        AddAuthentication(services);
        AddRegistration(services);
        AddAccountServices(services);
        AddAccountsSystem(services);

        return services;
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.TryAddSingleton<AccountAuthenticator>();
        services.TryAddSingleton<AccountLoginTracker>();
        services.TryAddSingleton<AuthenticationAttemptLimiter>();
        services.TryAddSingleton<SharedSecretManager>();
        services.TryAddSingleton<LoginHandoffTracker>();
    }

    private static void AddRegistration(IServiceCollection services)
    {
        services.TryAddSingleton<RegistrationController>();
        services.TryAddSingleton<RegistrationValidator>();
    }

    private static void AddAccountServices(IServiceCollection services)
    {
        services.TryAddSingleton<AccountServices>();
    }

    private static void AddAccountsSystem(IServiceCollection services)
    {
        services.TryAddSingleton<AccountsController>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISystem, AccountsSystem>());
    }
}