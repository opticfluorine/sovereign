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

--- Utility functions for working with vector types.
local Vectors = {}

local DefaultTol = 0.00001

--- Checks whether two vectors are equal within a given tolerance.
--- @param a Vector2|Vector3 First vector to compare.
--- @param b Vector2|Vector3 Second vector to compare.
--- @param tol number? Optional tolerance for comparing each component.
--- @return boolean true if equal within tolerance, false otherwise.
function Vectors.Equal(a, b, tol)
    if not tol then
        tol = DefaultTol
    end

    -- Require at least the Vector2 interface.
    if not a.X or not b.X or not a.Y or not b.Y then
        return false
    end

    -- If either argument has the Vector3 interface, both must have it.
    if (a.Z and not b.Z) or (not a.Z and b.Z) then
        return false
    end

    -- Vector2 componentwise compare.
    if math.abs(a.X - b.X) > tol or math.abs(a.Y - b.Y) > tol then
        return false
    end

    -- Optional Vector3 componentwise compare.
    if a.Z and math.abs(a.Z - b.Z) > tol then
        return false
    end

    return true
end

--- Utilities for Vector2 objects.
Vectors.Vector2 = {}

--- Zero-valued Vector2.
Vectors.Vector2.Zero = {
    X = 0.0,
    Y = 0.0
}

--- One-valued Vector2.
Vectors.Vector2.One = {
    X = 1.0,
    Y = 1.0
}

--- Utilities for Vector3 objects.
Vectors.Vector3 = {}

--- Zero-valued Vector3.
Vectors.Vector3.Zero = {
    X = 0.0,
    Y = 0.0,
    Z = 0.0
}

--- One-valued Vector3.
Vectors.Vector3.One = {
    X = 0.0,
    Y = 0.0,
    Z = 0.0
}

--------------

return Vectors
