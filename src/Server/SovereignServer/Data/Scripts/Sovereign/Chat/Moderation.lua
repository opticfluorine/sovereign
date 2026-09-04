-- Moderation.lua
-- Default script for announcing chat mute changes.
--
-- Sovereign Engine
-- Copyright (c) 2026 opticfluorine
--
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU General Public License as published by
-- the Free Software Foundation, either version 3 of the License, or
-- (at your option) any later version.
--
-- This program is distributed in the hope that it will be useful,
-- but WITHOUT ANY WARRANTY; without even the implied warranty of
-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
-- GNU General Public License for more details.
--
-- You should have received a copy of the GNU General Public License
-- along with this program.  If not, see <https://www.gnu.org/licenses/>.

local function DescribeScope(scope)
    if (scope == ChatMuteScope.All) then
        return "all chat"
    end
    return "global chat"
end

local function OnMuteAdded(event)
    local playerName = Components.Name.Get(event.EntityId, true)
    Chat.SendToAll(Color.CHAT_GLOBAL,
            string.format("%s has been muted from %s.", playerName, DescribeScope(event.Scope)))
end

local function OnMuteRemoved(event)
    local playerName = Components.Name.Get(event.EntityId, true)
    if (event.ExpirySystemTime > 0) then
        Chat.SendToAll(Color.CHAT_GLOBAL,
                string.format("%s is no longer muted (mute expired).", playerName))
    else
        Chat.SendToAll(Color.CHAT_GLOBAL,
                string.format("%s is no longer muted.", playerName))
    end
end

Scripting.AddEventCallback(Events.Server_Chat_MuteAdded, OnMuteAdded)
Scripting.AddEventCallback(Events.Server_Chat_MuteRemoved, OnMuteRemoved)
