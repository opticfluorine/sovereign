// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SovereignClientMcp;

var options = ClientMcpOptions.Default;

for (var i = 0; i < args.Length; ++i)
{
    switch (args[i])
    {
        case "--host":
            options = options with { Host = NextValue(args, ref i, "--host") };
            break;

        case "--port":
        {
            var value = NextValue(args, ref i, "--port");
            if (!ushort.TryParse(value, out var port))
            {
                Console.Error.WriteLine($"Invalid value for --port: {value}");
                return 1;
            }
            options = options with { Port = port };
            break;
        }

        case "--screenshots-dir":
        {
            var value = NextValue(args, ref i, "--screenshots-dir");
            var dir = Path.IsPathRooted(value)
                ? value
                : Path.Combine(Environment.CurrentDirectory, value);
            options = options with { ScreenshotsDir = Path.GetFullPath(dir) };
            break;
        }

        case "--help":
        case "-h":
            PrintUsage();
            return 0;

        default:
            Console.Error.WriteLine($"Unknown option: {args[i]}");
            PrintUsage();
            return 1;
    }
}

if (!IPAddress.TryParse(options.Host, out _))
{
    Console.Error.WriteLine($"Invalid value for --host: {options.Host}");
    return 1;
}

AppDomain.CurrentDomain.ProcessExit += (_, _) => ScreenshotWriter.Shutdown();

var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP protocol, so all logging must go to stderr.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton(options);
builder.Services.AddSingleton<DebugConnection>();
builder.Services.AddSingleton<DebugScriptService>();
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
return 0;

void PrintUsage()
{
    Console.Error.WriteLine(
        "Usage: SovereignClientMcp [--host <address>] [--port <port>] [--screenshots-dir <dir>]");
}

string NextValue(string[] values, ref int index, string optionName)
{
    if (index + 1 >= values.Length)
    {
        Console.Error.WriteLine($"Missing value for {optionName}.");
        Environment.Exit(1);
    }
    return values[++index];
}
