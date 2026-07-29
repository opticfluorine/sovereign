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

--- Minimum fuzyz match similarity for offering a "did you mean" suggestion.
local MinSimilarity = 0.7

--- Usage help text.
local ItemGiveUsage = "Usage: /itemgive <player>, <item>, [quantity]"

--- /itemgive <player>, <item>, [quantity]
--- Admin command that creates an item from a fuzzy-matched item template
--- and places it into a free slot of a fuzzy-matched online player's inventory.
--- @param args table 1-indexed table of trimmed string arguments.
--- @param playerId integer Entity ID of the player who issued the command.
local function ItemGive(args, playerId)
    local issuer = Entity.Get(playerId)
    if not issuer.Components.Admin then
        issuer:SendSystemMessage(ErrorMessages.CommandRequiresAdmin)
        return
    end

    -- Read comma-separated arguments.
    local targetName = args[1]
    local itemName = args[2]
    local qtyRaw = args[3]

    -- Validate required arguments.
    if targetName == nil or targetName == ""
        or itemName == nil or itemName == "" then
        issuer:SendSystemMessage(ItemGiveUsage)
        return
    end

    -- Parse the optional quantity argument, if present.
    local quantity = nil
    if qtyRaw ~= nil and qtyRaw ~= "" then
        quantity = tonumber(qtyRaw)
        if quantity == nil
            or math.tointeger(quantity) == nil
            or quantity < 1 then
            issuer:SendSystemMessage(ErrorMessages.InvalidQuantity)
            return
        end
        quantity = math.tointeger(quantity)
    end

    -- Resolve the target player by fuzzy name match (best match).
    local playerMatches = Players.FindByFuzzyName(targetName, 1)
    if #playerMatches == 0 then
        issuer:SendSystemMessage(ErrorMessages.PlayerNotFound)
        return
    end
    local playerMatch = playerMatches[1]
    if playerMatch.Score < MinSimilarity then
        issuer:SendSystemMessage(ErrorMessages.PlayerNotFound)
        return
    end
    if playerMatch.Score < 1.0 then
        issuer:SendSystemMessage(string.format(
            "No online player named \"%s\". Did you mean \"%s\"?",
            targetName, playerMatch.Name))
        return
    end
    local targetId = playerMatch.EntityId

    -- Resolve the item template by fuzzy name match (best match).
    local itemMatches = Items.FindByFuzzyName(itemName, 1)
    if #itemMatches == 0 then
        issuer:SendSystemMessage(ErrorMessages.ItemNotFound)
        return
    end
    local itemMatch = itemMatches[1]
    if itemMatch.Score < MinSimilarity then
        issuer:SendSystemMessage(ErrorMessages.ItemNotFound)
        return
    end
    if itemMatch.Score < 1.0 then
        issuer:SendSystemMessage(string.format(
            "No item template named \"%s\". Did you mean \"%s\"?",
            itemName, itemMatch.Name))
        return
    end
    local templateId = itemMatch.EntityId

    -- Create a new item entity from the resolved template.
    local itemId = 0
    if quantity ~= nil and Components.Stackable.Exists(templateId) then
        Entities.Create({ Template = templateId, Quantity = quantity })
    else
        Entities.Create({ Template = templateId })
    end

    if not itemId then
        Util.LogError(string.format(
            "ItemGive: failed to create item from template %X for issuer %X.",
            templateId, playerId))
        issuer:SendSystemMessage(ErrorMessages.ItemNotFound)
        return
    end

    -- Place the new item into the first free inventory slot of the target.
    if not Inventory.AddItem(targetId, itemId) then
        issuer:SendSystemMessage(ErrorMessages.NoFreeSlot)
        Entities.Remove(itemId)
        return
    end
end

Chat.AddCommand("itemgive", ItemGive, ChatCommandFlags.CommaSeparatedArgs)
