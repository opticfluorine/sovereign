// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Moq;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Mock factory for IBlockServices.
/// </summary>
public static class MockBlockServicesFactory
{
    /// <summary>
    ///     Gets a builder for a new IBlockServices mock.
    /// </summary>
    /// <returns></returns>
    public static MockBlockServicesBuilder GetBuilder()
    {
        return new MockBlockServicesBuilder();
    }

    public class MockBlockServicesBuilder
    {
        protected internal MockBlockServicesBuilder()
        {
        }

        public Mock<IBlockServices> Mock { get; } = new();

        /// <summary>
        ///     Configures the mock to return the given block presence grid.
        /// </summary>
        /// <param name="segmentIndex">World segment index.</param>
        /// <param name="z">Z plane.</param>
        /// <param name="grid">Grid to be returned.</param>
        /// <returns>Builder.</returns>
        public MockBlockServicesBuilder WithBlockPresenceGrid(GridPosition segmentIndex,
            int z, bool[] grid)
        {
            var outGrid = new BlockPresenceGrid
            {
                Grid = grid,
                Count = grid.Count(x => x)
            };

            Mock.Setup(x => x.TryGetBlockPresenceGrid(segmentIndex, z, out outGrid)).Returns(true);

            return this;
        }
    }
}