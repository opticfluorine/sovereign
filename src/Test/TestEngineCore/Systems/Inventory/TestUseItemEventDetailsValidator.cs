// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events.Details;
using Sovereign.EngineCore.Events.Details.Validators;
using Xunit;

namespace Sovereign.EngineCore.Systems.Inventory;

/// <summary>
///     Unit tests for <see cref="UseItemEventDetailsValidator" />.
/// </summary>
public class TestUseItemEventDetailsValidator
{
    private readonly UseItemEventDetailsValidator validator = new();

    [Theory]
    [InlineData(ItemUseFace.None)]
    [InlineData(ItemUseFace.BlockFrontFace)]
    [InlineData(ItemUseFace.BlockTopFace)]
    public void IsValid_DefinedFace_ReturnsTrue(ItemUseFace face)
    {
        var details = new UseItemEventDetails
        {
            ToolEntityId = 1,
            TargetEntityId = 2,
            Face = face
        };

        Assert.True(validator.IsValid(details));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public void IsValid_UndefinedFace_ReturnsFalse(int face)
    {
        var details = new UseItemEventDetails
        {
            ToolEntityId = 1,
            TargetEntityId = 2,
            Face = (ItemUseFace)face
        };

        Assert.False(validator.IsValid(details));
    }
}
