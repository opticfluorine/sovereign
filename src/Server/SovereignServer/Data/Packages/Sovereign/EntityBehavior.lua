-- Sovereign Engine Scripting Library
-- Copyright (c) 2025 opticfluorine
--
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU Lesser General Public License as published by
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

-------------------------------------

local Entity = require('Sovereign.Entity')

-------------------------------------

--- Name of default entity load hook for EntityBehavior.
local DefaultLoadFunction = "OnLoad"

--- Name of default entity unload hook for EntityBehavior.
local DefaultUnloadFunction = "OnUnload"

-------------------------------------

--- @enum WaitType Bitwise flags specifying events to wait on.
local WaitType = {
    --- No wait.
    None = 0,
    --- Wait for a time to elapse.
    Time = 1,
    --- Wait for motion to stop due to a scheduled stop.
    ScheduledStop = 2,
    --- Wait for motion to stop due to a collision.
    Collision = 4,
}

-------------------------------------

--- Wraps an asynchronous function into a per-entity behavior with life cycle hooks.
---@class EntityBehavior
---@field FollowTemplateChanges boolean Whether the behavior should follow an entity's template change.
---@field LoadHookName string Name of the installed load hook function.
---@field UnloadHookName string Name of the installed unload hook function.
---@field private _main fun(behavior: EntityBehavior, entity: Entity, ...: any) Behavior function.
---@field private _coroutines table Coroutines indexed by entity ID.
---@field private _startArgs any[] Additional arguments to pass to behavior function at start.
---@field private _waitTypes table Table of wait types indexed by entity ID.
---@field private _waitKeys table Table of weight keys indexed by entity ID.
---@field private _templateIds table Table of template IDs. Only used if FollowTemplateChanges is false.
---@field private _scheduledStopHandles table Table of scheduled stop callback handles indexed by entity ID.
---@field private _collisionHandles table Table of collision callback handles indexed by entity ID.
local EntityBehavior = {}
EntityBehavior.__index = EntityBehavior
setmetatable(EntityBehavior, EntityBehavior)

-------------------------------------

--- Wraps an asynchronous function into a per-entity behavior with prefixed name.
--- Hook functions are created with prefixed names in the global namespace.
--- @param f fun(behavior: EntityBehavior, entity: Entity, ...: any) # Behavior function.
--- @return EntityBehavior # Behavior object.
function EntityBehavior.CreatePrefixed(prefix, f, ...)
    local obj = {}
    setmetatable(obj, EntityBehavior)

    obj._main = f
    obj._coroutines = {}
    obj._startArgs = {...}
    obj._waitTypes = {}
    obj._waitKeys = {}

    obj.FollowTemplateChanges = false
    obj._templateIds = {}

    obj._scheduledStopHandles = {}
    obj._collisionHandles = {}

    obj:_InstallGlobalHooks(prefix)
    
    return obj
end

-------------------------------------

--- Wraps an asynchronous function into a per-entity behavior.
--- Hook functions are created with default names in the global namespace.
--- @param f fun(behavior: EntityBehavior, entity: Entity, ...: any) # Behavior function.
--- @return EntityBehavior # Behavior object.
function EntityBehavior.Create(f, ...)
    return EntityBehavior.CreatePrefixed(nil, f, ...)
end

-------------------------------------

--- Cleans up resources for an entity after behavior completes.
function EntityBehavior:_CleanupEntity(entityId)
    local thread, isMain = coroutine.running()
    if isMain and self._coroutines[entityId] then
        coroutine.close(self._coroutines[entityId])
        self._coroutines[entityId] = nil
    end
    self._templateIds[entityId] = nil
    self._waitTypes[entityId] = nil
    self._waitKeys[entityId] = nil
    self._scheduledStopHandles[entityId] = nil
    self._collisionHandles[entityId] = nil
end

-------------------------------------

--- Entity load life cycle hook. Starts behavior for newly loaded entity.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnLoad(entityId)
    if self._coroutines[entityId] then
        Util.LogWarn(string.format("Behavior for entity %X already exists; replacing.", entityId))
        self:_CleanupEntity(entityId)
    end

    -- Only track the template ID if we're not following template changes.
    -- Otherwise we can reduce the memory footprint.
    if not self.FollowTemplateChanges then
        local templateId = Entities.GetTemplate(entityId)
        if templateId then
            self._templateIds[entityId] = templateId
        else
            self._templateIds[entityId] = nil
        end
    end

    local thread = coroutine.create(self._main)
    self._coroutines[entityId] = thread
    local ok, err = coroutine.resume(thread, self, Entity.Get(entityId), table.unpack(self._startArgs))
    if not ok then
        Util.LogError(string.format("Failed to add behavior for %X: %s", entityId, err))
    end
    if coroutine.status(thread) == "dead" then
        -- Coroutine terminated.
        self:_CleanupEntity(entityId)
    end
end

-------------------------------------

--- Entity unload life cycle hook. Stops behavior for unloaded entity and frees resources.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnUnload(entityId)
    local thread = self._coroutines[entityId]
    if thread == nil then
        -- Already unloaded or never initially loaded.
        return
    end

    self:_CleanupEntity(entityId)
end

-------------------------------------

--- Resumes the behavior for the given entity.
--- @param entityId integer # Entity ID.
--- @param ... any # Additional parameters to pass back to the coroutine.
function EntityBehavior:Resume(entityId, ...)
    local thread = self._coroutines[entityId]
    if thread == nil then
        Util.LogError(string.format("Error resuming behavior for entity %X: behavior not loaded.", entityId))
        return
    end

    if not self.FollowTemplateChanges and self._templateIds[entityId] ~= Entities.GetTemplate(entityId) then
        -- Template has changed and we are not following changes, so end the current behavior.
        self:_CleanupEntity(entityId)
        return
    end

    local ok, err = coroutine.resume(thread, ...)
    if not ok then
        Util.LogError(string.format("Error resuming behavior for entity %X: %s", entityId, err))
    end
    if coroutine.status(thread) == "dead" then
        -- Coroutine has finished.
        self:_CleanupEntity(entityId)
    end
end

-------------------------------------

--- Installs entity life cycle hooks to the global scope.
--- @param prefix string? # Optional prefix to prepend hook names with.
function EntityBehavior:_InstallGlobalHooks(prefix)
    self.LoadHookName = (prefix == nil and "" or prefix .. "_") .. "OnLoad"
    self.UnloadHookName = (prefix == nil and "" or prefix .. "_") .. "OnUnload"

    _G[self.LoadHookName] = function(entityId) self:OnLoad(entityId) end
    _G[self.UnloadHookName] = function(entityId) self:OnUnload(entityId) end
end

-------------------------------------

--- Waits until one of a set of conditions is met.
--- @param entityId integer Entity ID.
--- @param waitTypes WaitType Bitwise flags indicating which conditions to wait for.
--- @param delaySeconds? number Seconds to wait if waitTypes includes WaitType.Time.
function EntityBehavior:Wait(entityId, waitTypes, delaySeconds)
    -- Select a new wait key.
    if not self._waitKeys[entityId] then
        self._waitKeys[entityId] = 0
    end
    local waitKey = self._waitKeys[entityId] + 1
    self._waitKeys[entityId] = waitKey

    -- Configure any requested waits.
    local configuredWaits = WaitType.None
    if (waitTypes & WaitType.Time) > 0 and delaySeconds and delaySeconds > 0 then
        Scripting.AddTimedCallback(delaySeconds,
            function (cbEntityId) self:_ResumeFromWait(cbEntityId, WaitType.Time, waitKey) end, entityId)
        configuredWaits = configuredWaits | WaitType.Time
    end

    if (waitTypes & WaitType.ScheduledStop) > 0 then
        self._scheduledStopHandles[entityId] = Scripting.AddScheduledStopCallback(entityId,
            function (cbEntityId) self:_ResumeFromWait(cbEntityId, WaitType.ScheduledStop, waitKey) end)
        configuredWaits = configuredWaits | WaitType.ScheduledStop
    end

    if (waitTypes & WaitType.Collision) > 0 then
        self._collisionHandles[entityId] = Scripting.AddCollisionCallback(entityId, 
            function (cbEntityId) self:_ResumeFromWait(cbEntityId, WaitType.Collision, waitKey) end)
        configuredWaits = configuredWaits | WaitType.Collision
    end

    -- Wait and return the type of wait that resumed first
    local cbWaitType = WaitType.None
    if configuredWaits ~= WaitType.None then
        cbWaitType = coroutine.yield()
    end
    return cbWaitType
end

-------------------------------------

--- Resumes from a Wait call.
--- @param entityId integer Entity ID.
--- @param waitType WaitType Reason that the coroutine is being resumed.
--- @param waitKey integer Wait key for filtering stale callbacks.
function EntityBehavior:_ResumeFromWait(entityId, waitType, waitKey)
    -- Screen out any late callbacks from a previous wait.
    local crWaitTypes = self._waitTypes[entityId]
    local crWaitKey = self._waitKeys[entityId]
    if not crWaitTypes or (crWaitTypes & waitType) == 0 then return end
    if crWaitKey ~= waitKey then return end

    -- Clean up the full set of callbacks that were spawned by the wait call
    local collisionHandle = self._collisionHandles[entityId]
    if collisionHandle then
        self._collisionHandles[entityId] = nil
        Scripting.RemoveCollisionCallback(entityId, collisionHandle)
    end

    local scheduledStopHandle = self._scheduledStopHandles[entityId]
    if scheduledStopHandle then
        self._scheduledStopHandles[entityId] = nil
        Scripting.RemoveScheduledStopCallback(entityId, scheduledStopHandle)
    end

    -- Resume the entity's coroutine, passing the reaosn the wait ended as an argument
    self._waitTypes[entityId] = nil
    self:Resume(entityId, waitType) end

-------------------------------------

return {
    EntityBehavior = EntityBehavior,
    Create = EntityBehavior.Create,
    DefaultLoadFunction = DefaultLoadFunction,
    DefaultUnloadFunction = DefaultUnloadFunction,
    WaitType = WaitType
}
