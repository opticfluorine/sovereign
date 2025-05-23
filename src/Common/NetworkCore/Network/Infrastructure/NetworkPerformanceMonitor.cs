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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Performance;

namespace Sovereign.NetworkCore.Network.Infrastructure;

/// <summary>
///     Performance monitor which tracks network statistics.
/// </summary>
public sealed class NetworkPerformanceMonitor(ILogger<NetworkPerformanceMonitor> logger, INetworkManager networkManager) : IPerformanceMonitor
{
    public void OnPerformanceEvent(IEventDetails eventDetails)
    {
    }

    public void ReportPerformance()
    {
        logger.LogDebug("Bytes Sent: {BytesSent}", networkManager.NetStatistics.BytesSent);
        logger.LogDebug("Bytes Received: {BytesReceived}", networkManager.NetStatistics.BytesReceived);
        logger.LogDebug("Packets Sent: {PacketsSent}", networkManager.NetStatistics.PacketsSent);
        logger.LogDebug("Packets Received: {PacketsReceived}", networkManager.NetStatistics.PacketsReceived);
        logger.LogDebug("Packet Loss: {PacketLoss} ({LossPercent}%)", networkManager.NetStatistics.PacketLoss,
            networkManager.NetStatistics.PacketLossPercent);
    }
}