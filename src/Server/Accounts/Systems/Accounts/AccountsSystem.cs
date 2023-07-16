/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using Sovereign.Accounts.Accounts.Authentication;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Systems;
using Sovereign.ServerCore.Events;

namespace Sovereign.Accounts.Systems.Accounts;

/// <summary>
/// </summary>
public sealed class AccountsSystem : ISystem
{
    /// <summary>
    ///     Number of ticks between subsequent limiter purges.
    /// </summary>
    private const int TICKS_BETWEEN_PURGES = 30;

    private readonly AuthenticationAttemptLimiter attemptLimiter;

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
        AccountLoginTracker loginTracker)
    {
        this.attemptLimiter = attemptLimiter;
        this.sharedSecretManager = sharedSecretManager;
        this.loginTracker = loginTracker;

        EventCommunicator = eventCommunicator;
        eventLoop.RegisterSystem(this);
    }

    public EventCommunicator EventCommunicator { get; }

    public ISet<EventId> EventIdsOfInterest { get; } = new HashSet<EventId>
    {
        EventId.Core_Tick,
        EventId.Server_Network_ClientDisconnected
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
                    var details = (ConnectionIdEventDetails)ev.EventDetails;
                    OnClientDisconnect(details.ConnectionId);
                    break;
            }
        }

        return eventsProcessed;
    }

    public void Initialize()
    {
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
        if (ticksSincePurge > TICKS_BETWEEN_PURGES)
        {
            attemptLimiter.PurgeExpiredRecords();
            sharedSecretManager.PurgeOldSecrets();
            ticksSincePurge = 0;
        }
    }
}