-- ItemSpawn.lua (Sovereign/Item/Spawn)
-- Generic behavior script for spawning items at the same position as the spawner.
--
-- Sovereign Engine 
-- Copyright (c) 2025 opticfluorine
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
-- ================
--   BEHAVIOR:
-- ================
--
-- The Sovereign/Item/ItemSpawn behavior provides a basic item spawner.
-- Each spawner will spawn a single item at the same position as the spawner.
-- Items are respawned at a fixed interval (default 60.0 seconds).
--
-- ================
--   HOW TO USE:
-- ================
--
-- 1. Create an NPC template entity to act as the spawner.
-- 2. Set the template's load callback to Sovereign/Item/ItemSpawn::OnLoad.
-- 3. Set the template's unload callback to Sovereign/Item/ItemSpawn::OnUnload.
-- 4. Set parameters as appropriate:
--    * Sovereign.ItemSpawn.TemplateId (required) - Absolute template ID for the spawned NPCs.
--    * Sovereign.ItemSpawn.Delay      (optional) - Delay between spawns in seconds.
-- 5. Place a spawner NPC using your new template at the desired spawn location.
--
-- ================
--

local EntityBehavior = require("EntityBehavior")
local Vectors = require("Vectors")

local ParamTemplateId = "Sovereign.ItemSpawn.TemplateId"
local ParamDelay = "Sovereign.ItemSpawn.Delay"

local KeyItemId = "Sovereign.ItemSpawn.ItemId"

local DefaultDelay = 60.0

EntityBehavior.Create(
function (behavior, spawnerEntityId)

    -- Load parameters for this spawner.
    local spawnData = data.GetEntityData(spawnerEntityId)

    local templateId = tonumber(spawnData[ParamTemplateId])
    if not templateId or not entities.IsTemplate(templateId) then
        util.LogError(string.format("Entity %X is missing required parameter %s.", 
            spawnerEntityId, ParamTemplateId))
        return
    end
    local templateType = components.entity_type.Get(templateId)
    if templateType ~= EntityType.Item then
        util.LogError(string.format("Entity %X has non-item spawn template.", spawnerEntityId))
        return
    end

    local delay = tonumber(spawnData[ParamDelay])
    if not delay then
        delay = DefaultDelay
    end

    -- Periodically check if we need to respawn.
    local itemId = tonumber(spawnData[KeyItemId])
    while true do
        -- Bail out if the spawner was destroyed.
        local spawnPosVel = components.kinematics.Get(spawnerEntityId)
        if not spawnPosVel then
            return
        end

        -- Check if a spawned item is in the spawn position.
        if itemId then
            local itemPosVel = components.kinematics.Get(itemId)
            if itemPosVel and Vectors.Equal(spawnPosVel.Position, itemPosVel.Position) then
                -- Spawned item is still in original location, no action needed.
                goto waiting
            end
        end

        -- Spawn new item.
        itemId = entities.Create({
            Template = templateId,
            Kinematics = {
                Position = spawnPosVel.Position,
                Velocity = Vectors.Vector3.Zero
            }
        })
        if not itemId then
            util.LogError(string.format("Spawner %X has failed; disabling until reload.", spawnerEntityId))
            return
        end

        -- Update the tracked item ID in case of restart/reload.
        spawnData[KeyItemId] = itemId

        -- Wait for the spawn delay to elapse before checking/spawning an item again.
        ::waiting::
        behavior:WaitAsync(spawnerEntityId, delay)
    end

end
):InstallGlobalHooks()

scripting.AddEntityParameterHint(
    EntityBehavior.DefaultLoadFunction,
    ParamTemplateId,
    "ItemTemplate",
    "Template to use for spawned NPCs.")

scripting.AddEntityParameterHint(
    EntityBehavior.DefaultLoadFunction,
    ParamDelay,
    "Float",
    string.format("Delay time in seconds between spawns (default: %.1f).", DefaultDelay))
