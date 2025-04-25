// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.EngineCore.Configuration;

namespace Sovereign.EngineCore.Events;

public class EventLoggerService : BackgroundService
{
    private readonly EventLogger eventLogger;
    private readonly ILogger<EventLoggerService> logger;
    private readonly IOptions<DebugOptions> debugOptions;

    public EventLoggerService(EventLogger eventLogger, ILogger<EventLoggerService> logger,
        IOptions<DebugOptions> debugOptions)
    {
        this.eventLogger = eventLogger;
        this.logger = logger;
        this.debugOptions = debugOptions;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Run(stoppingToken));
    }

    /// <summary>
    ///     Runs the event logger service until signaled by the stopping token.
    /// </summary>
    /// <param name="stoppingToken">Token to signal when to stop.</param>
    private void Run(CancellationToken stoppingToken)
    {
        // Log files are lazy-initialized so they are only created once the
        // first event is sent to be logged.
        Lazy<FileStream> logFile = new(() =>
        {
            var filename = $"eventLog_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.json";
            var path = Path.Combine(debugOptions.Value.EventLogDirectory, filename);
            return File.Create(path);
        });
        Lazy<Utf8JsonWriter> jsonWriter = new(() =>
        {
            var writer = new Utf8JsonWriter(logFile.Value, new JsonWriterOptions
            {
                SkipValidation = true
            });
            writer.WriteStartArray();
            logger.LogInformation("Event logger started.");
            return writer;
        });

        var options = new JsonSerializerOptions
        {
            IncludeFields = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                eventLogger.WaitForEvents(stoppingToken);
                while (eventLogger.TryTakeEvent(out var ev))
                {
                    if (ShouldIgnoreEvent(ev.EventId)) continue;
                    JsonSerializer.Serialize(jsonWriter.Value, ev, options);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while logging events.");
            }
        }

        if (jsonWriter.IsValueCreated)
        {
            jsonWriter.Value.WriteEndArray();
            jsonWriter.Value.Dispose();
            logger.LogInformation("Event logger stopped.");
        }

        if (logFile.IsValueCreated)
        {
            logFile.Value.Close();
            logFile.Value.Dispose();
        }
    }

    /// <summary>
    ///     Filters events to exclude those that are not relevant for logging.
    /// </summary>
    /// <param name="eventId">Event ID.</param>
    /// <returns>true if the event should be ignored, false otherwise.</returns>
    private bool ShouldIgnoreEvent(EventId eventId)
    {
        return eventId switch
        {
            >= EventId.Client_Input_KeyDown and < EventId.Server_Persistence_RetrieveEntity => true,
            _ => false
        };
    }
}