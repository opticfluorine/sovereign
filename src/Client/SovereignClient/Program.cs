/* Initialize SDL. */

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SDL2;
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
SDL.SDL_SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");
var err = SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
if (err < 0)
{
    Log.Logger.Fatal("Error initializing SDL: {Error}", SDL.SDL_GetError());
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
SDL_image.IMG_Quit();
SDL.SDL_Quit();