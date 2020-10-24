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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sovereign.EngineUtil.Collections
{

    /// <summary>
    /// Concrete implementation of a generic binary heap in an array.
    /// </summary>
    /// <typeparam name="T">Type of object stored in the heap.</typeparam>
    public class BinaryHeap<T> : IHeap<T>
    {

        /// <summary>
        /// Array used to store the heap.
        /// </summary>
        private T[] HeapArray;

        /// <summary>
        /// Comparer used to compare elements.
        /// </summary>
        private readonly IComparer<T> Comparer;

        /// <summary>
        /// Action count used to detect changes during iteration.
        /// </summary>
        private long ActionCount;

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        /// <summary>
        /// Default size of the heap.
        /// </summary>
        public const int DEFAULT_SIZE = 64;

        /// <summary>
        /// Creates a new heap.
        /// </summary>
        /// <param name="size">Initial size of the underlying array.  Should be a power of 2.</param>
        /// <param name="comparer">Optional comparer between elements.</param>
        public BinaryHeap(int size = DEFAULT_SIZE, IComparer<T> comparer = null)
        {
            /* Create the backing array. */
            HeapArray = new T[size];

            /* Set the comparer. */
            if (comparer != null)
            {
                /* Use the user-provided comparer. */
                Comparer = comparer;
            }
            else
            {
                /* Create a default comparer. */
                Comparer = Comparer<T>.Default;
            }
        }

        public void Add(T item)
        {
            Push(item);
        }

        /// <summary>
        /// Clears the heap.  O(1) complexity.
        /// </summary>
        public void Clear()
        {
            ActionCount++;
            Count = 0;
        }

        /// <summary>
        /// Checks if the given item is in the heap.  O(n) complexity.
        /// </summary>
        /// <param name="item">Item to check for.</param>
        /// <returns>true if the item is found, false otherwise.</returns>
        public bool Contains(T item)
        {
            return FindIndexOfElement(item) >= 0;
        }

        /// <summary>
        /// Copies the heap into an array.
        /// The first element will be the minimum element.
        /// The order of the other elements is undefined.
        /// O(n) complexity.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            /* Sanity checks. */
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            int bufferLength = array.Length - arrayIndex;
            if (bufferLength < Count)
            {
                throw new ArgumentException("Not enough space in destination array.");
            }

            /* Copy the array directly. */
            Array.Copy(HeapArray, 0, array, arrayIndex, Count);
        }

        /// <summary>
        /// Enumerator over the heap.
        /// </summary>
        protected class Enumerator : IEnumerator<T>
        {
            private int CurrentIndex = -1;

            private long ReferenceAction;

            private BinaryHeap<T> Heap;

            public T Current => Heap.HeapArray[CurrentIndex];

            object IEnumerator.Current => throw new NotImplementedException();

            internal Enumerator(BinaryHeap<T> heap, long referenceAction)
            {
                Heap = heap;
                ReferenceAction = referenceAction;
            }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                /* Make sure that the heap hasn't been modified */
                if (Heap.ActionCount > ReferenceAction)
                {
                    throw new InvalidOperationException("Heap was modified.");
                }

                /* Advance */
                CurrentIndex++;
                return CurrentIndex < Heap.Count;
            }

            public void Reset()
            {
                /* Make sure that the heap hasn't been modified */
                if (Heap.ActionCount > ReferenceAction)
                {
                    throw new InvalidOperationException("Heap was modified.");
                }

                /* Reset */
                CurrentIndex = -1;
            }
        }

        /// <summary>
        /// Gets an enumerator over the heap.
        /// The first enumerated element will be the minimum element.
        /// The order of enumeration for the other elements is undefined.
        /// </summary>
        /// <returns>Enumerator over the heap.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this, ActionCount);
        }

        /// <summary>
        /// Peeks at the minimum element.  O(1) complexity.
        /// </summary>
        /// <returns>Minimum element.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the heap is empty.
        /// </exception>
        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Heap is empty.");
            }
            return HeapArray[0];
        }

        /// <summary>
        /// Removes and returns the minimum element.  O(log n) complexity.
        /// </summary>
        /// <returns>Minimum element.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the heap is empty.
        /// </exception>
        public T Pop()
        {
            var minElem = Peek();
            RemoveMinimumElement();
            ActionCount++;
            return minElem;
        }

        /// <summary>
        /// Pushes an element onto the heap.  O(log n) complexity.
        /// </summary>
        /// <param name="obj">Object to add to the heap.</param>
        public void Push(T obj)
        {
            /* Is the heap already full? */
            if (Count == HeapArray.Length)
            {
                ExpandArray();
            }

            /* Insert the new element in the last position. */
            HeapArray[Count] = obj;
            Count++;
            ActionCount++;

            /* Sift the new element upward until the heap constraint is satisfied. */
            SiftUp(Count - 1);
        }

        /// <summary>
        /// Removes the given element from the heap.  O(n) complexity.
        /// </summary>
        /// <param name="item">Element to remove from the heap.</param>
        /// <returns>true if removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            /* Attempt to locate the element. */
            int idxThis = FindIndexOfElement(item);
            if (idxThis < 0)
            {
                /* Not found. */
                return false;
            }

            /* If this is the last element, just discard it and return. */
            Count--;
            ActionCount++;
            if (idxThis == Count) return true;

            /* Swap the last element with this element, discarding this element. */
            HeapArray[idxThis] = HeapArray[Count];

            /* Do we need to sift the element up? */
            int idxParent = (idxThis - 1) / 2;
            if (idxThis > 0 && Comparer.Compare(HeapArray[idxParent], HeapArray[idxThis]) > 0)
            {
                /* Yes - sift up until the heap constraint is restored. */
                SiftUp(idxThis);
            }
            else
            {
                /* No - sift down to ensure that the heap constraint is satisfied. */
                SiftDown(idxThis);
            }

            /* Done. */
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, ActionCount);
        }

        /// <summary>
        /// Removes the minimum element from the heap.
        /// This assumes that the heap is not empty.
        /// </summary>
        private void RemoveMinimumElement()
        {
            /* Safety check. */
            if (Count == 1)
            {
                Count = 0;
                return;
            }

            /* Swap the root and the last element, discarding the root. */
            HeapArray[0] = HeapArray[Count - 1];
            Count--;

            /* Sift the new root down until the heap property is restored. */
            SiftDown(0);
        }

        /// <summary>
        /// Sifts down the element at the given index until the heap
        /// property is locally satisfied.
        /// </summary>
        /// <param name="index">Index of the element to sift.</param>
        private void SiftDown(int index)
        {
            /* Sift the new root down until the heap property is restored. */
            int idxThis = index, idxLeftChild = 2 * index + 1, idxRightChild = 2 * index + 2;
            bool satisfied = false;
            while (!satisfied)
            {
                /* Is there a left child? */
                int leftCmp = -1;
                bool hasLeftChild = idxLeftChild < Count;
                if (hasLeftChild)
                {
                    leftCmp = Comparer.Compare(HeapArray[idxThis], HeapArray[idxLeftChild]);
                }

                /* Is there a right child? */
                int rightCmp = -1;
                bool hasRightChild = idxRightChild < Count;
                if (hasRightChild)
                {
                    rightCmp = Comparer.Compare(HeapArray[idxThis], HeapArray[idxRightChild]);
                }

                /* Check heap property */
                satisfied = leftCmp <= 0 && rightCmp <= 0;
                if (satisfied) break;

                /* Determine the direction of the rotation */
                const int LEFT = 0, RIGHT = 1;
                int direction;
                if (hasLeftChild && !hasRightChild)
                {
                    direction = LEFT;
                }
                else
                {
                    /*
                     * Since the binary heap array is left-filled, the presence of a right child
                     * implies the presence of a left child.  Determine which child should be
                     * rotated to satisfy the heap constraint at the current level.
                     */
                    int childCmp = Comparer.Compare(HeapArray[idxLeftChild],
                        HeapArray[idxRightChild]);
                    direction = childCmp <= 0 ? LEFT : RIGHT;
                }

                /* Perform the rotation. */
                var tmp = HeapArray[idxThis];
                if (direction == LEFT)
                {
                    /* Rotate elements */
                    HeapArray[idxThis] = HeapArray[idxLeftChild];
                    HeapArray[idxLeftChild] = tmp;

                    /* Update indices */
                    idxThis = idxLeftChild;
                }
                else
                {
                    /* Rotate elements */
                    HeapArray[idxThis] = HeapArray[idxRightChild];
                    HeapArray[idxRightChild] = tmp;

                    /* Update indices */
                    idxThis = idxRightChild;
                }
                idxLeftChild = 2 * idxThis + 1;
                idxRightChild = 2 * idxThis + 2;

                /*
                 * If there was a left child but not a right child, this implies that the
                 * element being sifted is now a leaf.  It therefore satisfies the heap constraint
                 * locally, and therefore the heap constraint is globally satisfied.
                 */
                if (hasLeftChild && !hasRightChild)
                {
                    satisfied = true;
                }
            }
        }

        /// <summary>
        /// Sifts up the element at the given index until the heap
        /// property is locally satisfied.
        /// </summary>
        /// <param name="index">Index of the element to sift.</param>
        private void SiftUp(int index)
        {
            int idxThis = index;
            int idxParent = (idxThis - 1) / 2;
            bool satisfied = false;
            while (!satisfied)
            {
                /* Are we at the root? */
                if (idxThis == 0)
                {
                    /* Heap property must be satisfied at this point. */
                    satisfied = true;
                    break;
                }

                /* Compare against the parent. */
                int cmp = Comparer.Compare(HeapArray[idxParent], HeapArray[idxThis]);
                satisfied = cmp <= 0;
                if (satisfied) break;

                /* Rotate. */
                var tmp = HeapArray[idxParent];
                HeapArray[idxParent] = HeapArray[idxThis];
                HeapArray[idxThis] = tmp;

                idxThis = idxParent;
                idxParent = (idxThis - 1) / 2;
            }
        }

        /// <summary>
        /// Expands the heap array.  This is O(n).
        /// </summary>
        private void ExpandArray()
        {
            int newSize = HeapArray.Length + DEFAULT_SIZE;
            var newArray = new T[newSize];
            Array.Copy(HeapArray, newArray, HeapArray.Length);
            HeapArray = newArray;
        }

        /// <summary>
        /// Finds the index of the given element.
        /// </summary>
        /// <param name="elem">Element to locate.</param>
        /// <returns>Index of the element, or -1 if not found.</returns>
        private int FindIndexOfElement(T elem)
        {
            /* Start searching from the root. */
            var indexStack = new Stack<int>();
            indexStack.Push(0);

            /* Search until there are no more elements to check. */
            while (indexStack.Count > 0)
            {
                /* Get the next element to check */
                int idxThis = indexStack.Pop();
                int idxLeft = 2 * idxThis + 1;
                int idxRight = 2 * idxThis + 2;

                /* Check for a match */
                if (elem.Equals(HeapArray[idxThis]))
                {
                    /* Found a match */
                    return idxThis;
                }

                /* No match - could the element be located further down? */
                if (Comparer.Compare(elem, HeapArray[idxThis]) < 0)
                {
                    /* Heap property indicates the element is not present in this branch. */
                    continue;
                }

                /* Push the children onto the stack for future evaluation. */
                if (idxLeft < Count) indexStack.Push(idxLeft);
                if (idxRight < Count) indexStack.Push(idxRight);
            }

            /* If we get here, the element wasn't found. */
            return -1;
        }

    }
}
