/* Initialize SDL. */

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDL2;
using Serilog;
using Sovereign.ClientCore;
using Sovereign.EngineCore;
using Sovereign.NetworkCore;
using Sovereign.VeldridRenderer;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging.
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Services.AddLogging(
    loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

// Initialize SDL.
SDL.SDL_SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");
var err = SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
if (err < 0)
{
    logger.Fatal("Error initializing SDL: {Error}", SDL.SDL_GetError());
    Environment.Exit(1);
}

// Configure Sovereign Engine.
builder.Services
    .AddSovereignCore()
    .AddSovereignCoreHostedServices()
    .AddSovereignNetworkCore()
    .AddSovereignClient()
    .AddSovereignVeldridRenderer();

// Run application.
var host = builder.Build();
host.Run();

// Clean up SDL.
SDL_image.IMG_Quit();
SDL.SDL_Quit();