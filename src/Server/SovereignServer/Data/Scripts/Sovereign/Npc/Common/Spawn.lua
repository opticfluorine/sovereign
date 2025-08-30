-- Spawn.lua (Sovereign/Npc/Common/Spawn)
-- Generic behavior script for spawning NPCs in the area around a spawner.
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
-- The Sovereign/Npc/Common/Spawn behavior provides a basic NPC spawner.
-- Each spawner will spawn up to a specific number of NPCs (default 1) somewhere within
-- a fixed radius (default 8.0 world units) of the spawner location. NPCs are spawned one
-- at a time every Sovereign.Spawn.Delay seconds (default 30.0).
--
-- ================
--   HOW TO USE:
-- ================
--
-- 1. Create an NPC template entity to act as the spawner.
-- 2. Set the template's load callback to Sovereign/Npc/Common/Spawn::OnLoad.
-- 3. Set the template's unload callback to Sovereign/Npc/Common/Spawn::OnUnload.
-- 4. Set parameters as appropriate:
--    * Sovereign.Spawn.TemplateId (required) - Relative template ID for the spawned NPCs.
--    * Sovereign.Spawn.Delay      (optional) - Delay between spawns in seconds.
--    * Sovereign.Spawn.Radius     (optional) - Spawn radius in world units.
--    * Sovereign.Spawn.Count      (optional) - Maximum number of spawned NPCs at one time.
-- 5. Place a spawner NPC using your new template at the desired spawn location.
--
-- ================
--

local EntityBehavior = require("EntityBehavior")

local ParamTemplateId = "Sovereign.Spawn.TemplateId"
local ParamDelay = "Sovereign.Spawn.Delay"
local ParamRadius = "Sovereign.Spawn.Radius"
local ParamCount = "Sovereign.Spawn.Count"

local DefaultDelay = 30.0
local DefaultRadius = 8.0
local DefaultCount = 1

EntityBehavior.Create(
function (behavior, spawnerEntityId)

    -- Load parameters for this spawner.
    local spawnData = data.GetEntityData(spawnerEntityId)

    local templateId = tonumber(spawnData[ParamTemplateId])
    if not templateId then
        util.LogError(string.format("Entity %X is missing required parameter %s.", 
            spawnerEntityId, ParamTemplateId))
        return
    end
    templateId = templateId + entities.FirstTemplateEntityId

    local delay = tonumber(spawnData[ParamDelay])
    if not delay then
        delay = DefaultDelay
    end

    local radius = tonumber(spawnData[ParamRadius])
    if not radius then
        radius = DefaultRadius
    end

    local maxSpawns = tonumber(spawnData[ParamCount])
    if not maxSpawns then
        maxSpawns = DefaultCount
    end

    -- Periodically check if we need to respawn.
    local spawnedIds = {}
    while true do
        -- Bail out if the spawner was destroyed.
        local spawnPosVel = components.kinematics.Get(spawnerEntityId)
        if not spawnPosVel then
            return
        end

        -- Remove any entities that were destroyed since the last check.
        local removedIndices = {}
        for index, entityId in ipairs(spawnedIds) do
            if not components.kinematics.Get(entityId) then
                table.insert(removedIndices, index)
            end
        end
        for i = #removedIndices, 1, -1 do
            table.remove(spawnedIds, removedIndices[i])
        end

        -- Spawn a new entity if we haven't reached the spawn count yet.
        if #spawnedIds < maxSpawns then
            -- Select random position within the radius.
            local theta = 2.0 * math.pi * math.random()
            local r = radius * math.random()
            local x = r * math.cos(theta)
            local y = r * math.sin(theta)

            -- Spawn new entity.
            local newId = entities.Create({
                Template = templateId,
                Kinematics = {
                    Position = {
                        X = x + spawnPosVel.Position.X,
                        Y = y + spawnPosVel.Position.Y,
                        Z = spawnPosVel.Position.Z
                    },
                    Velocity = {
                        X = 0.0,
                        Y = 0.0,
                        Z = 0.0
                    }
                },
                NonPersistent = true
            })

            if not newId then
                util.LogError(string.format("Spawner %X has failed; disabling until reload.", spawnerEntityId))
                return
            end

            table.insert(spawnedIds, newId)
        end

        behavior:WaitAsync(spawnerEntityId, delay)
    end

end
):InstallGlobalHooks()

scripting.AddEntityParameterHint(EntityBehavior.DefaultLoadFunction, ParamTemplateId)
scripting.AddEntityParameterHint(EntityBehavior.DefaultLoadFunction, ParamDelay)
scripting.AddEntityParameterHint(EntityBehavior.DefaultLoadFunction, ParamRadius)
scripting.AddEntityParameterHint(EntityBehavior.DefaultLoadFunction, ParamCount)
