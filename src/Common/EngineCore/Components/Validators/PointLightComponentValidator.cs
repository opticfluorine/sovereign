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

using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.EngineCore.Components.Validators;

/// <summary>
///     Component validator for PointLight-valued components.
/// </summary>
public class PointLightComponentValidator
{
    /// <summary>
    ///     Tests whether the given PointLight struct is valid.
    /// </summary>
    /// <param name="pointLight">Struct to check.</param>
    /// <returns>true if valid, false otherwise.</returns>
    public bool IsValid(PointLight pointLight)
    {
        return pointLight.Radius >= 0.0f
               && pointLight.Intensity >= 0.0f
               && pointLight.Color.GreaterThanOrEqualAll(Vector3.Zero)
               && pointLight.Color.LessThanOrEqualAll(Vector3.One)
               && pointLight.PositionOffset.IsFinite();
    }
}