/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineUtil.Validation
{

    /// <summary>
    /// Validates that elements of an unordered range are consecutively 
    /// numbered from 0 to n-1, where n is the length of the range.
    /// </summary>
    public sealed class ConsecutiveRangeValidation
    {

        /// <summary>
        /// Checks that the elements of an unordered range are consecutively
        /// numbered within an interval.
        /// </summary>
        /// <param name="range">Range to check.</param>
        /// <param name="rangeMin">Lower bound (inclusive).</param>
        /// <param name="rangeMax">Upper bound (exclusive).</param>
        /// <param name="duplicateIds">Set of duplicated IDs.</param>
        /// <param name="outOfRangeIds">Set of out-of-range IDs.</param>
        /// <returns>true if the range is valid, false otherwise.</returns>
        public bool IsRangeConsecutive(IEnumerable<int> range,
            int rangeMin, int rangeMax,
            out ISet<int> duplicateIds, out ISet<int> outOfRangeIds)
        {
            var idSet = new HashSet<int>();
            duplicateIds = new HashSet<int>();
            outOfRangeIds = new HashSet<int>();
            foreach (var id in range)
            {
                if (idSet.Contains(id))
                {
                    duplicateIds.Add(id);
                    continue;
                }

                if (id < rangeMin || id >= rangeMax)
                {
                    outOfRangeIds.Add(id);
                }

                idSet.Add(id);
            }

            return duplicateIds.Count == 0 && outOfRangeIds.Count == 0;
        }

    }

}
