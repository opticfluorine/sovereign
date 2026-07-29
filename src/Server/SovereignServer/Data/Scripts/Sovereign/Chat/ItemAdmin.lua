-- ItemAdmin.lua (Sovereign/Chat/ItemAdmin)
-- Provides chat commands for admin-level item and inventory manipulation
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
-- You should have received a copy of the GNU General Public License
-- along with this program.  If not, see <https://www.gnu.org/licenses/>.
--

local Entity = require('Sovereign.Entity')
local ErrorMessages = require('Sovereign.ErrorMessages')

--- /itemgive <player>, <relative template ID>, [quantity]
--- Admin command that inserts an item into a player's inventory.
--- @param command string
--- @param playerId integer
local function ItemGive(command, playerId)
    local player = Entity.Get(playerId)
    if not player.Components.Admin then
        player:SendSystemMessage(ErrorMessages.CommandRequiresAdmin)
        return
    end

    -- parse comma delimited args
    local args = {}
    for token in string.gmatch(command, "%s*([^%s,][^,]*[^%s,])%s*") do
        table.insert(args, token)
    end
end

Chat.AddCommand("itemgive", ItemGive)
