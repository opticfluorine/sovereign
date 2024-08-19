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
using System.Net;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MessagePack;
using Sovereign.ClientCore.Entities;
using Sovereign.ClientCore.Network.Rest;
using Sovereign.ClientCore.Systems.ClientNetwork;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineCore.Network.Rest;

namespace Sovereign.ClientCore.Network.Infrastructure;

/// <summary>
///     Client-side template entity loader.
/// </summary>
public class TemplateEntityDataClient
{
    /// <summary>
    ///     Maximum length of response from server (512 KB).
    /// </summary>
    private const int MaxContentLength = 524288;

    private readonly RestClient client;
    private readonly IEventSender eventSender;
    private readonly TemplateEntityDataLoader loader;
    private readonly ClientNetworkController networkController;

    public TemplateEntityDataClient(RestClient client, ClientNetworkController networkController,
        IEventSender eventSender, TemplateEntityDataLoader loader)
    {
        this.client = client;
        this.networkController = networkController;
        this.eventSender = eventSender;
        this.loader = loader;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Asynchronously loads the template entities from the server.
    /// </summary>
    /// <remarks>
    ///     This should only be called once as the player enters the world. Future updates to the
    ///     templates while the player is in game will be delivered by a separate entity sync mechanism.
    /// </remarks>
    public void LoadTemplateEntities()
    {
        Task.Run(RetrieveAndLoadTemplateEntities);
    }

    /// <summary>
    ///     Retrieves and loads the latest template entity data from the server.
    /// </summary>
    private async void RetrieveAndLoadTemplateEntities()
    {
        try
        {
            var response = await client.Get(RestEndpoints.TemplateEntities);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Logger.ErrorFormat("Failed to load template entities from server (status {0}).", response.StatusCode);
                networkController.DeclareConnectionLost(eventSender);
                return;
            }

            if (response.Content.Headers.ContentLength > MaxContentLength)
            {
                Logger.ErrorFormat("Template entity data content length {0} is too large (max {1}).",
                    response.Content.Headers.ContentLength, MaxContentLength);
                networkController.DeclareConnectionLost(eventSender);
                return;
            }

            var data = MessagePackSerializer.Deserialize<TemplateEntityData>(
                await response.Content.ReadAsByteArrayAsync(), MessageConfig.CompressedUntrustedMessagePackOptions);
            loader.Load(data);
        }
        catch (Exception e)
        {
            Logger.Error("Error while loading template entities from server; connection lost.", e);
            networkController.DeclareConnectionLost(eventSender);
        }
    }
}