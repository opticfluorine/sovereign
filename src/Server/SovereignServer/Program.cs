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

using System;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Sovereign.Accounts;
using Sovereign.EngineCore;
using Sovereign.EngineCore.Lua;
using Sovereign.NetworkCore;
using Sovereign.Persistence;
using Sovereign.Server;
using Sovereign.ServerCore;
using Sovereign.ServerCore.Lua;
using Sovereign.ServerNetwork;
using Sovereign.ServerNetwork.Network.Rest;

var builder = WebApplication.CreateBuilder(args);

// Load server runtime options.
var runtimeOptions = builder.Configuration
    .GetSection($"Sovereign:{nameof(RuntimeOptions)}")
    .Get<RuntimeOptions>() ?? throw new ApplicationException("bad config");

// Pin working directory to the executable location.
// This is needed for running as a Windows Service.
if (runtimeOptions.PinWorkingDirectory) Environment.CurrentDirectory = Path.GetFullPath(AppContext.BaseDirectory);

// Configure logging.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(dispose: true);

// Configure Sovereign Engine.
builder.Services
    .AddSovereignCore()
    .AddSovereignCoreHostedServices()
    .AddSovereignNetworkCore()
    .AddSovereignServer()
    .AddSovereignServerNetwork()
    .AddSovereignPersistence()
    .AddSovereignAccounts()
    .AddSovereignEngineCoreLuaLibraries()
    .AddSovereignServerCoreLuaLibraries()
    .AddSovereignEngineCoreLuaComponents()
    .AddSovereignEngineCoreLuaEnums();

// Bind appsettings.json (and other sources) to options classes.
builder.Services
    .AddSovereignCoreOptions(builder.Configuration)
    .AddSovereignServerOptions(builder.Configuration);

// Authentication and authorization for REST APIs.
builder.Services
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, RestAuthenticationHandler>(RestAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorizationBuilder()
    .AddSovereignPolicies();

// JSON configuration for REST server.
builder.Services
    .ConfigureHttpJsonOptions(options => { options.SerializerOptions.IncludeFields = true; });

// Configure runtime modes.
builder.Services.AddSystemd();
builder.Services.AddWindowsService(options => { options.ServiceName = runtimeOptions.WindowsServiceName; });

// Run application.
var app = builder.Build();

var restProvider = app.Services.GetService<RestServiceProvider>() ?? throw new Exception("No REST provider resolved.");
restProvider.AddEndpoints(app);

app.Run();