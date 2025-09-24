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

local EntityBehavior = require("EntityBehavior")

local ParamNextId = "Sovereign.TimedChange.NextId"
local ParamChangeTime = "Sovereign.TimedChange.ChangeTime"
local DataNextTime = "Sovereign.TimedChange.%s.NextTime"
local DataPrevTime = "Sovereign.TimedChange.PrevTime"

EntityBehavior.Create(
function (behavior, entityId)

    -- Retrieve and validate parameters.
    local entityData = data.GetEntityData(entityId)
    
    local nextId = tonumber(entityData[ParamNextId])
    if not nextId or not entities.IsTemplate(nextId) then
        util.LogError(string.format("Entity %X has invalid or missing parameter %s.",
            entityId, ParamNextId))
        return
    end

    local changeTime = tonumber(entityData[ParamChangeTime])
    if not changeTime or changeTime < 0 then
        util.LogError(string.format("Entity %X has invalid or missing parameter %s.",
            entityId, ParamChangeTime))
        return
    end

    local templateId = entities.GetTemplate(entityId)
    local selfIdStr = templateId and tostring(templateId - entities.FirstTemplateEntityId) or "Self"
    local key = string.format(DataNextTime, selfIdStr)

    while true do
        -- Check if a timed change is already in process.
        local nextTime = tonumber(entityData[key])
        local now = time.GetAbsoluteTime()
        if nextTime then
            -- Did the time elapse?
            if now >= nextTime then
                -- Elapsed - time to update the entity's template and end processing.
                entityData[key] = nil
                entityData[DataPrevTime] = nextTime
                entities.SetTemplate(entityId, nextId)
                return
            else
                -- Wait until ready.
                behavior:WaitAsync(entityId, nextTime - now)
            end
        else
            -- Schedule transformation.
            local prevTime = tonumber(entityData[DataPrevTime]) or now
            nextTime = prevTime + changeTime
            entityData[key] = nextTime
            if now >= nextTime then
                -- Newest change is already elapsed - apply the transformation.  
                entityData[key] = nil
                entityData[DataPrevTime] = nextTime
                entities.SetTemplate(entityId, nextId)
                return
            else
                behavior:WaitAsync(entityId, nextTime - now)
            end
        end
    end

end
):InstallGlobalHooks()

scripting.AddEntityParameterHint(
    EntityBehavior.DefaultLoadFunction,
    ParamNextId,
    "SameTypeTemplate",
    "Template ID to which the entity will change."
)

scripting.AddEntityParameterHint(
    EntityBehavior.DefaultLoadFunction,
    ParamChangeTime,
    "Float",
    "In-game time after which the entity's template will change."
)
