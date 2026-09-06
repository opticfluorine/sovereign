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

using Xunit;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Unit tests for DataKeyConstraints.
/// </summary>
public class TestDataKeyConstraints
{
    [Theory]
    [InlineData("__Internal", true)]
    [InlineData("__", true)]
    [InlineData("MyKey", false)]
    [InlineData("_MyKey", false)]
    [InlineData("", false)]
    public void IsKeyReadOnly_ChecksPrefix(string key, bool expected)
    {
        Assert.Equal(expected, DataKeyConstraints.IsKeyReadOnly(key));
    }
}
