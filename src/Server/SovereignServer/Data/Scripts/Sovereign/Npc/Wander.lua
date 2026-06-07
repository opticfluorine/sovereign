-- Wander.lua
-- Generic behavior script for wandering mobs.
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
-- The Sovereign/Npc/Wander behavior allows an NPC to randomly wander about the world.
-- The NPC will take a step in a random cardinal direction with a random delay between
-- steps. The initial delay to the first step after load is randomized to minimize the
-- correlation of movement between entities that are loaded at the same time.
--
-- This behavior does not stay within a certain area or avoid obstacles (including drops).
--
-- ================
--   HOW TO USE:
-- ================
--
-- 1. Create an NPC template entity.
-- 2. Set the template's load callback to Sovereign/Npc/Wander::OnLoad.
-- 3. Set the template's unload callback to Sovereign/Npc/Wander::OnUnload.
-- 4. Set parameters as appropriate:
--    * Sovereign.Wander.WanderStep  (required) - Distance per step in world units.
--    * Sovereign.Wander.WanderDelay (required) - Average delay between steps in seconds.
--    * Sovereign.Wander.WanderSpeed (required) - Movement speed in world units per second.
-- 5. Place an NPC and watch it wander around.
--
-- ================

local EntityBehavior = require("Sovereign.EntityBehavior")

local WaitType = EntityBehavior.WaitType

------------------------------------------
-- Constants                            --
------------------------------------------

--- Entity data key for the WanderStep parameter.
local ParamWanderStep = "Sovereign.Wander.WanderStep"

--- Entity data key for the WanderDelay parameter.
local ParamWanderDelay = "Sovereign.Wander.WanderDelay"

--- Entity data key for the WanderSpeed parameter.
local ParamWanderSpeed = "Sovereign.Wander.WanderSpeed"

--- Relative amount to vary the delay in either direction.
local DelayRandomScale = 0.1


------------------------------------------
-- Behavior                             --
------------------------------------------

--- Encapsulates the core behaviors so that they don't appear in the NPC editor.
---@class Wander
local Wander = {}

---Entity load hook that begins the wandering behavior.
---@param entity Entity # Loaded entity. 
function Wander.LoadParams(entity)
    -- Load per-entity parameters.
    local wanderStep = tonumber(entity.Data[ParamWanderStep])
    local wanderDelay = tonumber(entity.Data[ParamWanderDelay])
    local wanderSpeed = tonumber(entity.Data[ParamWanderSpeed])

    -- Validate per-entity parameters.
    local paramsValid = true
    if not wanderStep then
        Util.LogError(string.format("Entity %X requires parameter %s of type number.", entity.EntityId, ParamWanderStep))
        paramsValid = false
    end
    if not wanderDelay then
        Util.LogError(string.format("Entity %X requires parameter %s of type number.", entity.EntityId, ParamWanderDelay))
        paramsValid = false
    end
    if not wanderSpeed then
        Util.LogError(string.format("Entity %X requires parameter %s of type number.", entity.EntityId, ParamWanderSpeed))
        paramsValid = false
    end
    if not paramsValid then
        return false, 0, 0
    end

    if wanderStep <= 0 then
        Util.LogError(string.format("Entity %X has invalid wander step.", entity.EntityId))
        paramsValid = false
    end
    if wanderDelay < 0 then
        Util.LogError(string.format("Entity %X has invalid wander delay.", entity.EntityId))
        paramsValid = false
    end
    if wanderSpeed <= 0 then
        Util.LogError(string.format("Entity %X has invalid wander speed.", entity.EntityId))
        paramsValid = false
    end
    if not paramsValid then
        return false, 0, 0
    end

    -- Create and start a new coroutine for the new entity.
    return true, wanderDelay, wanderSpeed
end

--- Main coroutine for single entity behavior.
--- @async
--- @param behavior EntityBehavior # Behavior object.
--- @param entity Entity # Entity.
function Wander.RunAsync(behavior, entity)
    local ok, wanderDelay, wanderSpeed = Wander.LoadParams(entity)
    if not ok then return end

    -- Start at a random time within the movement period.
    -- This decorrelates the motion between entities that are loaded at the same time.
    behavior:Wait(entity.EntityId, WaitType.Time, math.random() * wanderDelay)

    while true do
        -- Get current position and velocity.
        local posVel = entity.Components.Kinematics
        if not posVel then
            Util.LogError(string.format("No Kinematics data for entity %X.", entity.EntityId))
            return
        end

        -- Pick random movement direction, then set up a velocity vector.
        -- Leave Z component alone to respect gravity.
        local dx, dy = Wander.RandomDirection()
        local nextDelay = wanderDelay + (math.random() - 0.5) * DelayRandomScale * wanderDelay

        -- Set entity in motion, then wait for movement to complete.
        entity:MoveBy({X = dx, Y = dy, Z = 0}, wanderSpeed)
        behavior:Wait(entity.EntityId, WaitType.Collision | WaitType.ScheduledStop)

        -- Pause until ready to move again.
        behavior:Wait(entity.EntityId, WaitType.Time, nextDelay)
    end
end

Wander._xstep = {-1.0, 1.0,  0.0, 0.0}
Wander._ystep = { 0.0, 0.0, -1.0, 1.0}

--- Selects a random direction and returns a (x,y) vector.
--- @return number dx # X step
--- @return number dy # Y step
function Wander.RandomDirection()
    local direction = math.random(4)
    return Wander._xstep[direction], Wander._ystep[direction]
end


------------------------------------------
-- Export Public API to NPC Editor      --
------------------------------------------

local wander = EntityBehavior.Create(Wander.RunAsync)

Scripting.AddEntityParameterHint(
    wander.LoadHookName,
    ParamWanderStep,
    "Float",
    "Distance in world units to travel per step.")

Scripting.AddEntityParameterHint(
    wander.LoadHookName,
    ParamWanderDelay,
    "Float",
    "Average time in seconds between movements.")

Scripting.AddEntityParameterHint(
    wander.LoadHookName,
    ParamWanderSpeed,
    "Float",
    "Movement speed in world units per second.")
