// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Contains global constants related to entities.
/// </summary>
public static class EntityConstants
{
    /// <summary>
    ///     Entity ID of the first template entity.
    /// </summary>
    public const ulong FirstTemplateEntityId = 0x7ffe000000000000;

    /// <summary>
    ///     Entity ID of the last possible template entity.
    /// </summary>
    public const ulong LastTemplateEntityId = 0x7ffeffffffffffff;

    /// <summary>
    ///     Entity ID of the first persisted entity.
    /// </summary>
    public const ulong FirstPersistedEntityId = 0x7fff000000000000;
}