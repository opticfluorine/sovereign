using Engine8.EngineUtil.Vectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine8.EngineUtil.Collections
{

    /// <summary>
    /// Generic octree implementation for managing mutable collections of
    /// objects in three-dimensional space.
    /// </summary>
    public class Octree<T> : ICollection<T>
    {

        /// <summary>
        /// Getter for the position of a given object.
        /// </summary>
        private readonly Func<T, Vector3> pointGetter;

        public Octree(Func<T, Vector3> pointGetter)
        {
            this.pointGetter = pointGetter;
        }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => false;

        public void Add(T obj)
        {

        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
