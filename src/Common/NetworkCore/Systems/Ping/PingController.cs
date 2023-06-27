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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.NetworkCore.Systems.Ping;

/// <summary>
///     Provides public API to the ping system.
/// </summary>
public class PingController
{
    /// <summary>
    ///     Commands the ping system to initiate a new ping.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void StartPing(IEventSender eventSender)
    {
        var ev = new Event(EventId.Core_Ping_Start);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Configures auto-ping behavior.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="enabled">If true, enable auto-ping; if false, disable.</param>
    /// <param name="intervalTicks">Ticks between pings. Ignored if auto-ping is disabled.</param>
    public void SetAutoPing(IEventSender eventSender, bool enabled, uint intervalTicks)
    {
        var details = new AutoPingEventDetails(enabled, intervalTicks);
        var ev = new Event(EventId.Core_Ping_SetAuto, details);
        eventSender.SendEvent(ev);
    }
}