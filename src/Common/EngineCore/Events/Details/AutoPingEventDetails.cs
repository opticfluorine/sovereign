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

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Event details for controlling auto-ping behavior.
/// </summary>
public class AutoPingEventDetails : IEventDetails
{
    /// <summary>
    ///     Creates auto ping event details.
    /// </summary>
    /// <param name="enable">Whether to enable auto ping.</param>
    /// <param name="intervalMs">Interval in milliseconds between pings.</param>
    public AutoPingEventDetails(bool enable, uint intervalMs)
    {
        Enable = enable;
        IntervalMs = intervalMs;
    }

    /// <summary>
    ///     Whether to enable (true) or disable (false) the auto-ping.
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    ///     Time in milliseconds between each automatic ping.
    /// </summary>
    public uint IntervalMs { get; set; }
}