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

using Microsoft.Extensions.Configuration;
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

var builder = Host.CreateApplicationBuilder(args);

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

// Load server runtime options.
var runtimeOptions = builder.Configuration
    .GetSection($"Sovereign:{nameof(RuntimeOptions)}")
    .Get<RuntimeOptions>() ?? new RuntimeOptions();

// Configure runtime modes.
builder.Services.AddSystemd();
builder.Services.AddWindowsService(options => { options.ServiceName = runtimeOptions.WindowsServiceName; });

// Run application.
var host = builder.Build();
host.Run();