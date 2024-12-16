/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Systems;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.Accounts.Systems.Accounts;

/// <summary>
/// </summary>
public sealed class AccountsSystem : ISystem
{
    /// <summary>
    ///     Number of ticks between subsequent limiter purges.
    /// </summary>
    private const int TicksBetweenPurges = 30;

    private readonly AuthenticationAttemptLimiter attemptLimiter;
    private readonly ILogger<AccountsSystem> logger;

    private readonly AccountLoginTracker loginTracker;

    private readonly SharedSecretManager sharedSecretManager;

    /// <summary>
    ///     Number of ticks since the last limiter purge.
    /// </summary>
    private int ticksSincePurge;

    public AccountsSystem(IEventLoop eventLoop,
        EventCommunicator eventCommunicator,
        AuthenticationAttemptLimiter attemptLimiter,
        SharedSecretManager sharedSecretManager,
        AccountLoginTracker loginTracker,
        ILogger<AccountsSystem> logger)
    {
        this.attemptLimiter = attemptLimiter;
        this.sharedSecretManager = sharedSecretManager;
        this.loginTracker = loginTracker;
        this.logger = logger;

        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Tick,
        EventId.Server_Network_ClientDisconnected,
        EventId.Server_Accounts_SelectPlayer,
        EventId.Server_Persistence_SynchronizeComplete,
        EventId.Server_Persistence_PlayerEnteredWorld,
        EventId.Core_Network_Logout
    };

    public int WorkloadEstimate => 10;

    public void Cleanup()
    {
    }

    public int ExecuteOnce()
    {
        var eventsProcessed = 0;
        while (EventCommunicator.GetIncomingEvent(out var ev))
        {
            eventsProcessed++;
            switch (ev.EventId)
            {
                case EventId.Core_Tick:
                    OnTick();
                    break;

                case EventId.Server_Network_ClientDisconnected:
                    if (ev.EventDetails == null)
                    {
                        logger.LogError("Received ClientDisconnected event with no details.");
                        break;
                    }

                    var details = (ConnectionIdEventDetails)ev.EventDetails;
                    OnClientDisconnect(details.ConnectionId);
                    break;

                case EventId.Server_Accounts_SelectPlayer:
                    if (ev.EventDetails == null)
                    {
                        logger.LogError("Received SelectPlayer event with no details.");
                        break;
                    }

                    var selectPlayerDetails = (SelectPlayerEventDetails)ev.EventDetails;
                    OnSelectPlayer(selectPlayerDetails);
                    break;

                case EventId.Server_Persistence_SynchronizeComplete:
                    OnSyncComplete();
                    break;

                case EventId.Server_Persistence_PlayerEnteredWorld:
                    if (ev.EventDetails is not EntityEventDetails playerDetails)
                    {
                        logger.LogError("Received PlayerEnteredWorld with no details.");
                        break;
                    }

                    OnPlayerEnteredWorld(playerDetails.EntityId);
                    break;

                case EventId.Core_Network_Logout:
                {
                    if (ev.EventDetails is not EntityEventDetails entityDetails)
                    {
                        logger.LogError("Received Logout event without details.");
                        break;
                    }

                    OnLogout(entityDetails.EntityId);
                }
                    break;
            }
        }

        return eventsProcessed;
    }

    public void Initialize()
    {
    }

    /// <summary>
    ///     Called when a player is selected during the login process.
    /// </summary>
    /// <param name="selectPlayerDetails">Player selection details.</param>
    private void OnSelectPlayer(SelectPlayerEventDetails selectPlayerDetails)
    {
        loginTracker.SelectPlayer(selectPlayerDetails.AccountId, selectPlayerDetails.PlayerCharacterEntityId);
    }

    /// <summary>
    ///     Called when a client disconnects from the event server.
    /// </summary>
    /// <param name="connectionId">Disconnected connection ID.</param>
    private void OnClientDisconnect(int connectionId)
    {
        loginTracker.HandleDisconnect(connectionId);
    }

    /// <summary>
    ///     Periodically purges expired login attempts.
    /// </summary>
    private void OnTick()
    {
        // TODO Switch to a time-based purge with a delayed event instead of tick polling.
        ticksSincePurge++;
        if (ticksSincePurge > TicksBetweenPurges)
        {
            attemptLimiter.PurgeExpiredRecords();
            sharedSecretManager.PurgeOldSecrets();
            ticksSincePurge = 0;
        }
    }

    /// <summary>
    ///     Called when a round of persistence synchronization has been completed.
    /// </summary>
    private void OnSyncComplete()
    {
        loginTracker.OnSyncComplete();
    }

    /// <summary>
    ///     Called when a player logs out to player selection.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    private void OnLogout(ulong playerEntityId)
    {
        loginTracker.Logout(playerEntityId);
    }

    /// <summary>
    ///     Called when a player has entered the world.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    private void OnPlayerEnteredWorld(ulong playerEntityId)
    {
        loginTracker.PlayerEnteredWorld(playerEntityId);
    }
}