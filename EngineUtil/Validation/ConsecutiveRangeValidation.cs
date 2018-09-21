/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
