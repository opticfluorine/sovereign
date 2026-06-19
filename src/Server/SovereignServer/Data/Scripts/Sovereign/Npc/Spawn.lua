-- Spawn.lua (Sovereign/Npc/Spawn)
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
-- The Sovereign/Npc/Spawn behavior provides a basic NPC spawner.
-- Each spawner will spawn up to a specific number of NPCs (default 1) somewhere within
-- a fixed radius (default 8.0 world units) of the spawner location. NPCs are spawned one
-- at a time every Sovereign.Spawn.Delay seconds (default 30.0).
--
-- ================
--   HOW TO USE:
-- ================
--
-- 1. Create an NPC template entity to act as the spawner.
-- 2. Set the template's load callback to Sovereign/Npc/Spawn::OnLoad.
-- 3. Set the template's unload callback to Sovereign/Npc/Spawn::OnUnload.
-- 4. Set parameters as appropriate:
--    * Sovereign.Spawn.TemplateId (required) - Absolute template ID for the spawned NPCs.
--    * Sovereign.Spawn.Delay      (optional) - Delay between spawns in seconds.
--    * Sovereign.Spawn.Radius     (optional) - Spawn radius in world units.
--    * Sovereign.Spawn.Count      (optional) - Maximum number of spawned NPCs at one time.
-- 5. Place a spawner NPC using your new template at the desired spawn location.
--
-- ================
--

local EntityBehavior = require("Sovereign.EntityBehavior")
local WaitType = EntityBehavior.WaitType

local ParamTemplateId = "Sovereign.Spawn.TemplateId"
local ParamDelay = "Sovereign.Spawn.Delay"
local ParamRadius = "Sovereign.Spawn.Radius"
local ParamCount = "Sovereign.Spawn.Count"

local DefaultDelay = 30.0
local DefaultRadius = 8.0
local DefaultCount = 1

local spawn = EntityBehavior.Create(
---@param behavior EntityBehavior
---@param spawnerEntity Entity
function (behavior, spawnerEntity)

    -- Load parameters for this spawner.
    local templateId = tonumber(spawnerEntity.Data[ParamTemplateId])
    if not templateId or not Entities.IsTemplate(templateId) then
        Util.LogError(string.format("Entity %X is missing required parameter %s.",
            spawnerEntity.EntityId, ParamTemplateId))
        return
    end
    local templateType = Components.EntityType.Get(templateId)
    if templateType ~= EntityType.Npc then
        Util.LogError(string.format("Entity %X has non-NPC spawn template.", spawnerEntity.EntityId))
        return
    end

    local delay = tonumber(spawnerEntity.Data[ParamDelay])
    if not delay then
        delay = DefaultDelay
    end

    local radius = tonumber(spawnerEntity.Data[ParamRadius])
    if not radius then
        radius = DefaultRadius
    end

    local maxSpawns = tonumber(spawnerEntity.Data[ParamCount])
    if not maxSpawns then
        maxSpawns = DefaultCount
    end

    -- Periodically check if we need to respawn.
    local spawnedIds = {}
    while true do
        -- Bail out if the spawner was destroyed.
        local spawnPosVel = spawnerEntity.Components.Kinematics
        if not spawnPosVel then
            return
        end

        -- Check if a spawned entity has been destroyed so that we can open a slot.
        -- Don't need to remove all of the destroyed entities, one is sufficient to
        -- allow a respawn to proceed.
        for index, entityId in ipairs(spawnedIds) do
            if not Components.Kinematics.Get(entityId) then
                table.remove(spawnedIds, index)
                break
            end
        end

        -- Spawn a new entity if we haven't reached the spawn count yet.
        if #spawnedIds < maxSpawns then
            -- Select random position within the radius.
            local theta = 2.0 * math.pi * math.random()
            local r = radius * math.random()
            local x = r * math.cos(theta)
            local y = r * math.sin(theta)

            -- Spawn new entity.
            local newId = Entities.Create({
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
                Util.LogError(string.format("Spawner %X has failed; disabling until reload.", spawnerEntity.EntityId))
                return
            end

            table.insert(spawnedIds, newId)
        end

        behavior:Wait(spawnerEntity.EntityId, WaitType.Time, delay)
    end

end
)

------------------------------

------------------------------------------
-- Export Public API to NPC Editor      --
------------------------------------------

Scripting.AddEntityParameterHint(
    spawn.LoadHookName,
    ParamTemplateId,
    "NpcTemplate",
    "Template to use for spawned NPCs.")

Scripting.AddEntityParameterHint(
    spawn.LoadHookName,
    ParamDelay,
    "Float",
    string.format("Delay time in seconds between spawns (default: %.1f).", DefaultDelay))

Scripting.AddEntityParameterHint(
    spawn.LoadHookName,
    ParamRadius,
    "Float",
    string.format("Spawn radius (default: %.1f).", DefaultRadius))

Scripting.AddEntityParameterHint(
    spawn.LoadHookName,
    ParamCount,
    "Int",
    string.format("Maximum number of spawned NPCs (default: %d).", DefaultCount))
