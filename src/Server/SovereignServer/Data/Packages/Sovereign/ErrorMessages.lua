-- Sovereign Engine Scripting Library
-- Copyright (c) 2026 opticfluorine
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

---@package ErrorMessages
---Provides constants for standard error messages that can be used by scripts.
local ErrorMessages = {}

---Error message indicating that a chat command requires admin privileges.
ErrorMessages.CommandRequiresAdmin = "This command requires admin privileges."

---Error message indicating that no online player matched the requested name.
ErrorMessages.PlayerNotFound = "No matching online player found."

---Error message indicating that no item template matched the requested name.
ErrorMessages.ItemNotFound = "No matching item template found."

---Error message indicating that the target player has no free inventory slot.
ErrorMessages.NoFreeSlot = "The target player has no free inventory slot."

---Error message indicating that the requested quantity was not a positive integer.
ErrorMessages.InvalidQuantity = "Quantity must be a positive integer."

return ErrorMessages
