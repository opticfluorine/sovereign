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
using Microsoft.Extensions.Hosting;
using Serilog;
using Sovereign.EngineCore;
using Sovereign.NetworkCore;
using Sovereign.ServerCore;
using Sovereign.ServerNetwork;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging.
_ = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Services.AddLogging(
    loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

// Configure Sovereign Engine.
builder.Services
    .AddSovereignCore()
    .AddSovereignCoreHostedServices()
    .AddSovereignNetworkCore()
    .AddSovereignServer()
    .AddSovereignServerNetwork();

// Run application.
var host = builder.Build();
host.Run();