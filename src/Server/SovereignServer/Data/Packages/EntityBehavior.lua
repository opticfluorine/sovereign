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

--- Wraps an asynchronous function into a per-entity behavior with life cycle hooks.
---@class EntityBehavior
---@field private _main fun(behavior: table, entityId: integer, ...: any) Behavior function.
---@field private _coroutines table Coroutines indexed by entity ID.
---@field private _startArgs any[] Additional arguments to pass to behavior function at start.
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

    return obj
end

--- Entity load life cycle hook. Starts behavior for newly loaded entity.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnLoad(entityId)
    if self._coroutines[entityId] ~= nil then
        util.LogWarn(string.format("Behavior for entity %A already exists; replacing.", entityId))
        coroutine.close(self._coroutines[entityId])
        self._coroutines[entityId] = nil
    end

    local thread = coroutine.create(self._main)
    self._coroutines[entityId] = thread
    local ok, err = coroutine.resume(thread, self, entityId, table.unpack(self._startArgs))
    if not ok then
        util.LogError(string.format("Failed to add behavior for %A: %s", entityId, err))
    end
end

--- Entity unload life cycle hook. Stops behavior for unloaded entity and frees resources.
--- @param entityId integer # Entity ID.
function EntityBehavior:OnUnload(entityId)
    local thread = self._coroutines[entityId]
    if thread == nil then
        util.LogError(string.format("Cannot unload behavior for entity %A without loading behavior first.", entityId))
        return
    end

    coroutine.close(thread)
    self._coroutines[entityId] = nil
end

--- Resumes the behavior for the given entity.
--- @param entityId integer # Entity ID.
--- @param ... any # Additional parameters to pass back to the coroutine.
function EntityBehavior:Resume(entityId, ...)
    local thread = self._coroutines[entityId]
    if thread == nil then
        util.LogError(string.format("Error resuming behavior for entity %A: behavior not loaded.", entityId))
        return
    end

    local ok, err = coroutine.resume(thread, ...)
    if not ok then
        util.LogError(string.format("Error resuming behavior for entity %A: %s", entityId, err))
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

return { EntityBehavior = EntityBehavior, Create = EntityBehavior.Create }
