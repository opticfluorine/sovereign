//
// Sovereign Engine
// Copyright (c) 2023 opticfluorine
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network.Rest;
using Sovereign.ServerCore.Configuration;
using Sovereign.ServerCore.Systems.Debug;
using WatsonWebserver;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Sovereign.ServerNetwork.Network.Rest.Debug;

public class DebugRestService : IRestService
{
    private readonly ServerConfiguration config;
    private readonly IEventSender eventSender;
    private readonly DebugController debugController;

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public string Path => RestEndpoints.Debug;
    
    public RestPathType PathType => RestPathType.Static;
    
    public HttpMethod RequestType => HttpMethod.POST;

    /// <summary>
    /// Whether debug mode is enabled.
    /// </summary>
    private bool Enabled => config.Debug.EnableDebugMode;

    public DebugRestService(IServerConfigurationManager configManager, DebugController debugController,
        IEventSender eventSender)
    {
        config = configManager.ServerConfiguration;
        this.debugController = debugController;
        this.eventSender = eventSender;
    }
    
    public async Task OnRequest(HttpContext ctx)
    {
        try
        {
            if (!Enabled)
            {
                // Short-circuit with a 404 when debug mode is disabled.
                Logger.WarnFormat("Debug service request received from {0} when debug mode is disabled.",
                    ctx.Request.Source.IpAddress);
                ctx.Response.StatusCode = 404;
                await ctx.Response.Send();
            }
            else
            {
                try
                {
                    var data = ctx.Request.DataAsBytes;
                    var command = JsonSerializer.Deserialize<DebugCommand>(data);
                    if (!command.IsValid)
                    {
                        throw new ArgumentException("Bad debug command.");
                    }

                    debugController.SendDebugCommand(eventSender, command);

                    ctx.Response.StatusCode = 200;
                    await ctx.Response.Send();
                }
                catch (JsonException)
                {
                    Logger.WarnFormat("Bad debug service request from {0}.", ctx.Request.Source.IpAddress);

                    ctx.Response.StatusCode = 400;
                    await ctx.Response.Send();
                }
                catch (Exception e)
                {
                    Logger.WarnFormat(e, "Unhandled exception in debug service for request from {0}.",
                        ctx.Request.Source.IpAddress);

                    ctx.Response.StatusCode = 500;
                    await ctx.Response.Send();
                }
            }
        }
        catch (Exception e)
        {
            Logger.ErrorFormat(e, "Unhandled exception in debug service for request from {0}.",
                ctx.Request.Source.IpAddress);

            ctx.Response.StatusCode = 500;
            await ctx.Response.Send();
        }
    }
    
}