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

namespace Sovereign.EngineCore.Events.Details.Validators;

/// <summary>
///     Validation interface for event details.
/// </summary>
public interface IEventDetailsValidator
{
    /// <summary>
    ///     Checks the validity of the event details.
    /// </summary>
    /// <param name="details">Event details, or null if the event lacks details.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(IEventDetails? details);
}