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

--- Name of default entity load hook for EntityBehavior.
local DefaultLoadFunction = "OnLoad"

--- Name of default entity unload hook for EntityBehavior.
local DefaultUnloadFunction = "OnUnload"

--- Wraps an asynchronous function into a per-entity behavior with life cycle hooks.
---@class EntityBehavior
---@field followTemplateChanges boolean Whether the behavior should follow an entity's template change.
---@field private _main fun(behavior: table, entityId: integer, ...: any) Behavior function.
---@field private _coroutines table Coroutines indexed by entity ID.
---@field private _startArgs any[] Additional arguments to pass to behavior function at start.
---@field private _templateIds table Table of template IDs. Only used if followTemplateChanges is false.
local EntityBehavior = {}
EntityBehavior.__index = EntityBehavior
setmetatable(EntityBehavior, EntityBehavior)

--- Wraps an asynchronous function into a per-entity behavior.
--- @param f fun(behavior: table, entityId: integer, ...: any) # Behavior function.
--- @return table # Behavior object.
function EntityBehavior.Create(f, ...)
    local obj = {}
    setmetatable(obj, EntityBehavior)

    obj._main = f
    obj._coroutines = {}
    obj._startArgs = {...}

    obj.followTemplateChanges = false
    obj._templateIds = {}

    return obj
end

--- Cleans up resources for an entity after behavior completes.
function EntityBehavior:_CleanupEntity(entityId)
    local thread, isMain = coroutine.running()
    if isMain and self._coroutines[entityId] then
        coroutine.close(self._coroutines[entityId])
        self._coroutines[entityId] = nil
    end
    self._templateIds[entityId] = nil
end

--- Entity load life cycle hook. Starts behavior for newly loaded entity.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnLoad(entityId)
    if self._coroutines[entityId] then
        util.LogWarn(string.format("Behavior for entity %X already exists; replacing.", entityId))
        self:_CleanupEntity(entityId)
    end

    -- Only track the template ID if we're not following template changes.
    -- Otherwise we can reduce the memory footprint.
    if not self.followTemplateChanges then
        local templateId = entities.GetTemplate(entityId)
        if templateId then
            self._templateIds[entityId] = templateId
        end
    end

    if not self.followTemplateChanges then
        local templateId = entities.GetTemplate(entityId)
        if templateId then
            self._templateIds[entityId] = templateId
        else
            self._templateIds[entityId] = nil
        end
    end

    local thread = coroutine.create(self._main)
    self._coroutines[entityId] = thread
    local ok, err = coroutine.resume(thread, self, entityId, table.unpack(self._startArgs))
    if not ok then
        util.LogError(string.format("Failed to add behavior for %X: %s", entityId, err))
    end
    if coroutine.status(thread) == "dead" then
        -- Coroutine terminated.
        self:_CleanupEntity(entityId)
    end
end

--- Entity unload life cycle hook. Stops behavior for unloaded entity and frees resources.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnUnload(entityId)
    local thread = self._coroutines[entityId]
    if thread == nil then
        util.LogError(string.format("Cannot unload behavior for entity %X without loading behavior first.", entityId))
        return
    end

    self:_CleanupEntity(entityId)
end

--- Resumes the behavior for the given entity.
--- @param entityId integer # Entity ID.
--- @param ... any # Additional parameters to pass back to the coroutine.
function EntityBehavior:Resume(entityId, ...)
    local thread = self._coroutines[entityId]
    if thread == nil then
        util.LogError(string.format("Error resuming behavior for entity %X: behavior not loaded.", entityId))
        return
    end

    if not self.followTemplateChanges and self._templateIds[entityId] ~= entities.GetTemplate(entityId) then
        -- Template has changed and we are not following changes, so end the current behavior.
        self:_CleanupEntity(entityId)
        return
    end

    local ok, err = coroutine.resume(thread, ...)
    if not ok then
        util.LogError(string.format("Error resuming behavior for entity %X: %s", entityId, err))
    end
    if coroutine.status(thread) == "dead" then
        -- Coroutine has finished.
        self:_CleanupEntity(entityId)
    end
end

--- Installs entity life cycle hooks to the global scope.
--- @param prefix string? # Optional prefix to prepend hook names with.
function EntityBehavior:InstallGlobalHooks(prefix)
    local loadName = (prefix == nil and "" or prefix .. "_") .. "OnLoad"
    local unloadName = (prefix == nil and "" or prefix .. "_") .. "OnUnload"

    _G[loadName] = function(entityId) self:OnLoad(entityId) end
    _G[unloadName] = function(entityId) self:OnUnload(entityId) end
end

--- Waits for a period of time before continuing.
--- @async
--- @param entityId integer # Entity ID.
--- @param delaySeconds number # Time in seconds to wait.
function EntityBehavior:WaitAsync(entityId, delaySeconds)
    scripting.AddTimedCallback(delaySeconds, function (entityId) self:Resume(entityId) end, entityId)
    coroutine.yield()
end

return {
    EntityBehavior = EntityBehavior,
    Create = EntityBehavior.Create,
    DefaultLoadFunction = DefaultLoadFunction,
    DefaultUnloadFunction = DefaultUnloadFunction
}
