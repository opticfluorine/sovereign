-- TimedChange.lua (Sovereign/Generic/TimedChange)
-- Generic behavior script for entities that change template after a period of time.
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
-- The Sovereign/Generic/TimedChange behavior defines a simple behavior in which
-- an entity's template is changed after a fixed amount of in-game time has passed.
-- This behavior correctly accounts for time that has passed while the entity was
-- unloaded (i.e. if the entity is loaded again after its change time has passed, the
-- change will be immediately applied).
--
-- Chains of template changes are supported by using the TimedChange behavior for each
-- template in the chain. The behavior sets an entity property Sovereign.TimedChange.PrevTime
-- at the end of each change, and any following change will use this value as its starting
-- time. Please note that if you interrupt the chain with a different behavior, you should
-- ensure that your custom behavior removes the PrevTime property before changing to a template
-- which uses the TimedChange behavior again, otherwise the change may occur earlier than expected.
--
-- ================
--   HOW TO USE:
-- ================
--
-- 1. Create an NPC template entity.
-- 2. Set the template's load callback to Sovereign/Generic/TimedChange::OnLoad.
-- 3. Set the template's unload callback to Sovereign/Generic/TimedChange::OnUnload.
-- 4. Set parameters as appropriate:
--    * Sovereign.TimedChange.NextId (required) - Absolute template ID of the next template in the chain.
--    * Sovereign.TimedChange.ChangeTime (required) - In-game seconds that must elapse before a change.
-- 5. If the next template in the sequence does not use TimedChange, consider having its behavior delete
--    the Sovereign.TimedChange.PrevTime entity property on load, particularly if the entity may later
--    change again to a template with TimedChange behavior.
--
-- ================

local EntityBehavior = require("Sovereign.EntityBehavior")
local WaitType = EntityBehavior.WaitType

local ParamNextId = "Sovereign.TimedChange.NextId"
local ParamChangeTime = "Sovereign.TimedChange.ChangeTime"
local DataNextTime = "Sovereign.TimedChange.%s.NextTime"
local DataPrevTime = "Sovereign.TimedChange.PrevTime"

local timedChange = EntityBehavior.Create(
---@param behavior EntityBehavior
---@param entity Entity
function (behavior, entity)

    -- Retrieve and validate parameters.
    local nextId = tonumber(entity.Data[ParamNextId])
    if not nextId or not Entities.IsTemplate(nextId) then
        Util.LogError(string.format("Entity %X has invalid or missing parameter %s.",
            entity.EntityId, ParamNextId))
        return
    end

    local changeTime = tonumber(entity.Data[ParamChangeTime])
    if not changeTime or changeTime < 0 then
        Util.LogError(string.format("Entity %X has invalid or missing parameter %s.",
            entity.EntityId, ParamChangeTime))
        return
    end

    local templateId = entity.Properties.TemplateId
    local selfIdStr = templateId and tostring(templateId - Entities.FirstTemplateEntityId) or "Self"
    local key = string.format(DataNextTime, selfIdStr)

    while true do
        -- Check if a timed change is already in process.
        local nextTime = tonumber(entity.Data[key])
        local now = Time.GetAbsoluteTime()
        if nextTime then
            -- Did the time elapse?
            if now >= nextTime then
                -- Elapsed - time to update the entity's template and end processing.
                entity.Data[key] = nil
                entity.Data[DataPrevTime] = nextTime
                entity.Properties.TemplateId = nextId
                return
            else
                -- Wait until ready.
                behavior:Wait(entity.EntityId, WaitType.Time, nextTime - now)
            end
        else
            -- Schedule transformation.
            local prevTime = tonumber(entity.Data[DataPrevTime]) or now
            nextTime = prevTime + changeTime
            entity.Data[key] = nextTime
            if now >= nextTime then
                -- Newest change is already elapsed - apply the transformation.  
                entity.Data[key] = nil
                entity.Data[DataPrevTime] = nextTime
                entity.Properties.TemplateId = nextId
                return
            else
                behavior:Wait(entity.EntityId, WaitType.Time, nextTime - now)
            end
        end
    end

end
)

Scripting.AddEntityParameterHint(
    timedChange.LoadHookName,
    ParamNextId,
    "SameTypeTemplate",
    "Template ID to which the entity will change."
)

Scripting.AddEntityParameterHint(
    timedChange.LoadHookName,
    ParamChangeTime,
    "Float",
    "In-game time after which the entity's template will change."
)
