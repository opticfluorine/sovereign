-- Motd.lua
-- Example script for performing actions when a player enters the world.
--
-- Sovereign Engine 
-- Copyright (c) 2024 opticfluorine
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
-- You should have received a copy of the GNU General Public License
-- along with this program.  If not, see <https://www.gnu.org/licenses/>.

function on_player_entered(event)
    local playerEntityId = event.EntityId
    local playerName = components.name.get(playerEntityId)
    chat.send_to_player(playerEntityId, color.MOTD,
            string.format("Welcome to Sovereign Engine, %s!", playerName))
    chat.send_to_all(color.CHAT_GLOBAL,
            string.format("%s has entered the world.", playerName))

    local isAdmin = components.admin.get(playerEntityId)
    if (isAdmin) then
        chat.send_to_player(playerEntityId, color.ALERT, "You are an admin.")
    end
end

function on_player_logout(event)
    local playerEntityId = event.EntityId
    local playerName = components.name.get(playerEntityId)
    chat.send_to_all(color.CHAT_GLOBAL,
            string.format("%s has left the world.", playerName))
end

scripting.add_event_callback(events.Server_Persistence_PlayerEnteredWorld, on_player_entered)
scripting.add_event_callback(events.Core_Network_Logout, on_player_logout)
