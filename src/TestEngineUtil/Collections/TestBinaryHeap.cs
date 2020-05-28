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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sovereign.EngineUtil.Collections
{

    /// <summary>
    /// Unit tests for the BinaryHeap class.
    /// </summary>
    public class TestBinaryHeap
    {

        /// <summary>
        /// This test checks that elements can be pushed, and the
        /// first element in the heap will be the minimum.
        /// </summary>
        /// <param name="elements">Elements to push onto the heap.</param>
        [Theory]
        [InlineData(new int[] { 1, 6, 7, 9 })]
        [InlineData(new int[] { 6, 7, 1, 9 })]
        [InlineData(new int[] { 7, -9, 182, -46, 782, -596 })]
        public void TestPush(int[] elements)
        {
            /* Create heap with default comparer. */
            IHeap<int> heap = new BinaryHeap<int>();
            Assert.Equal(0, heap.Count);
            Assert.False(heap.IsReadOnly);

            /* Populate heap. */
            for (int i = 0; i < elements.Length; ++i)
            {
                heap.Push(elements[i]);
                Assert.Equal(i + 1, heap.Count);
            }

            /* Check for the correct minimum. */
            Assert.Equal(elements.Min(), heap.Peek());
        }

        /// <summary>
        /// Tests that the minimum element can be popped, the heap
        /// size decreases by one after a pop, and the new first element
        /// is the second smallest element in the set.
        /// </summary>
        /// <param name="elements">Elements in the heap.</param>
        [Theory]
        [InlineData(new int[] { 1, 6, 7, 9 })]
        [InlineData(new int[] { 6, 7, 1, 9 })]
        [InlineData(new int[] { 7, -9, 182, -46, 782, -596 })]
        public void TestPop(int[] elements)
        {
            /* Create heap with default comparer and the given elements. */
            IHeap<int> heap = new BinaryHeap<int>();
            foreach (int elem in elements)
            {
                heap.Push(elem);
            }

            /* Sort the elements into the expected order. */
            int[] sorted = new int[elements.Length];
            Array.Copy(elements, sorted, elements.Length);
            Array.Sort(sorted);
            int firstMin = sorted[0], secondMin = sorted[1];

            /* Iterate until there are no more elements. */
            foreach (int expected in sorted)
            {
                /* Confirm that this element is atop the heap. */
                int initialSize = heap.Count;
                int actual = heap.Pop();
                Assert.Equal(expected, actual);

                /* Confirm that the size of the heap decreased by one. */
                Assert.Equal(initialSize - 1, heap.Count);
            }

            /* Confirm that the heap is empty. */
            Assert.Equal(0, heap.Count);
        }

        /// <summary>
        /// Tests the Contains() method with elements that are present
        /// and not present in the heap.
        /// </summary>
        /// <param name="elements">Test elements.</param>
        [Theory]
        [InlineData(new int[] { 1, 6, 7, 9 })]
        [InlineData(new int[] { 6, 7, 1, 9 })]
        [InlineData(new int[] { 7, -9, 182, -46, 782, -596 })]
        public void TestContains(int[] elements)
        {
            /* Find an element that is in the heap. */
            var heap = MakeHeap(elements);
            int maxElem = elements.Max();
            bool isMaxFound = heap.Contains(maxElem);
            Assert.True(isMaxFound);

            /* Try to find an element that is not in the heap. */
            int missingElem = maxElem + 1;
            bool isMissingFound = heap.Contains(missingElem);
            Assert.False(isMissingFound);
        }

        /// <summary>
        /// Tests removal of an arbitrary element from the heap.
        /// </summary>
        /// <param name="elements">Elements in heap.</param>
        [Theory]
        [InlineData(new int[] { 1, 6, 7, 9 })]
        [InlineData(new int[] { 6, 7, 1, 9 })]
        [InlineData(new int[] { 7, -9, 182, -46, 782, -596 })]
        public void TestRemove(int[] elements)
        {
            /* Create the heap. */
            var heap = MakeHeap(elements);

            /* Try to remove the maximal element. */
            int minElem = elements.Min();
            int maxElem = elements.Max();
            bool removed = heap.Remove(maxElem);

            /* Check that the element is removed without changing the first element. */
            Assert.True(removed);
            Assert.Equal(elements.Length - 1, heap.Count);
            Assert.Equal(minElem, heap.Peek());

            /* Try to remove an element that is not present. */
            int missingElem = maxElem + 1;
            bool missingRemoved = heap.Remove(missingElem);

            /* Verify that the removal failed and the heap is unchanged. */
            Assert.False(missingRemoved);
            Assert.Equal(elements.Length - 1, heap.Count);
            Assert.Equal(minElem, heap.Peek());
        }

        /// <summary>
        /// Utility method that creates a heap with the given elements.
        /// </summary>
        /// <param name="elements">Elements in heap.</param>
        /// <returns>Binary heap.</returns>
        private BinaryHeap<int> MakeHeap(int[] elements)
        {
            var heap = new BinaryHeap<int>();
            foreach (int elem in elements)
            {
                heap.Push(elem);
            }
            return heap;
        }

    }

}
