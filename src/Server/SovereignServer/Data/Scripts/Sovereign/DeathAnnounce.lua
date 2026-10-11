-- DeathAnnounce.lua
-- Announces the death of players killed in combat.
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

local function GetEntityName(entityId)
    if entityId == 0 then return nil end
    return Components.Name.Get(entityId)
end

local function OnPlayerKilled(event)
    local victimName = GetEntityName(event.VictimEntityId) or "Someone"
    local killerName = GetEntityName(event.KillerEntityId)
    if killerName then
        Chat.SendToAll(Color.CHAT_GLOBAL,
                string.format("%s was slain by %s.", victimName, killerName))
    else
        Chat.SendToAll(Color.CHAT_GLOBAL,
                string.format("%s has died.", victimName))
    end
end

Scripting.AddEventCallback(Events.Server_Combat_PlayerKilled, OnPlayerKilled)
