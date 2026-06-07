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

local function OnPlayerEntered(event)
    local playerEntityId = event.EntityId
    Util.LogDebug(string.format("playerEntityId = %s (type %s)", playerEntityId, type(playerEntityId)))
    local playerName = Components.Name.Get(playerEntityId)
    Chat.SendToPlayer(playerEntityId, Color.MOTD,
            string.format("Welcome to Sovereign Engine, %s!", playerName))
    Chat.SendToAll(Color.CHAT_GLOBAL,
            string.format("%s has entered the world.", playerName))

    local isAdmin = Components.Admin.Get(playerEntityId)
    if (isAdmin) then
        Chat.SendToPlayer(playerEntityId, Color.ALERT, "You are an admin.")
    end
end

local function OnPlayerLogout(event)
    local playerEntityId = event.EntityId
    local playerName = Components.Name.Get(playerEntityId, true)
    Chat.SendToAll(Color.CHAT_GLOBAL,
            string.format("%s has left the world.", playerName))
end

Scripting.AddEventCallback(Events.Server_Persistence_PlayerEnteredWorld, OnPlayerEntered)
Scripting.AddEventCallback(Events.Core_Network_Logout, OnPlayerLogout)
