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

using System.Collections.Generic;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Routes incoming chat messages to the appropriate chat processor.
/// </summary>
public class ChatRouter
{
    public ChatRouter(IList<IChatProcessor> processors)
    {
    }

    /// <summary>
    ///     Routes an incoming chat message to the appropriate chat processor.
    /// </summary>
    /// <param name="details"></param>
    public void RouteChatMessage(ChatEventDetails details)
    {
    }
}