/* Initialize SDL. */

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SDL3;
using Serilog;
using Sovereign.ClientCore;
using Sovereign.EngineCore;
using Sovereign.NetworkCore;
using Sovereign.UpdaterCore;
using Sovereign.VeldridRenderer;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(dispose: true);

// Initialize SDL.
SDL.SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");
if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Audio | SDL.InitFlags.Events))
{
    Log.Logger.Fatal("Error initializing SDL: {Error}", SDL.GetError());
    Environment.Exit(1);
}

// Configure Sovereign Engine.
builder.Services
    .AddSovereignCore()
    .AddSovereignCoreHostedServices()
    .AddSovereignNetworkCore()
    .AddSovereignClient()
    .AddSovereignVeldridRenderer()
    .AddSovereignUpdaterCore();

// Bind appsettings.json (and other sources) to options classes.
builder.Services
    .AddSovereignCoreOptions(builder.Configuration)
    .AddSovereignClientOptions(builder.Configuration);

// Run application.
var host = builder.Build();
host.Run();

// Clean up SDL.
SDL.Quit();