// source: http://stackoverflow.com/questions/8761238/collection-with-very-fast-iterating-and-good-addition-and-remove-speeds/21665421#21665421

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
    /// <summary>
    /// Specifying null as value has unspecified results.
    /// CopyTo may contain nulls.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SparseList<T> : IList<T>
        where T : class
    {
        int version = 0;
        List<T> list = new List<T>();
        Stack<int> freeIndices = new Stack<int>();

        public int Capacity { get { return list.Capacity; } set { list.Capacity = value; } }

        public void Compact()
        {
            var sortedIndices = freeIndices.ToList();

            foreach (var i in sortedIndices.OrderBy(x => x).Reverse())
            {
                list.RemoveAt(i);
            }
            freeIndices.Clear();
            list.Capacity = list.Count;
            version++; // breaks open enumerators
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// Slow (forces a compact), not recommended
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            // One idea: remove index from freeIndices if it's in there.  Stack doesn't support this though.
            Compact(); // breaks the freeIndices list, so apply it before insert
            list.Insert(index, item);
            version++; // breaks open enumerators
        }

        public void RemoveAt(int index)
        {
            if (index == Count - 1) { list.RemoveAt(index); }
            else { list[index] = null; freeIndices.Push(index); }
            //version++; // Don't increment version for removals
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                list[index] = value;
            }
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException();

            if (freeIndices.Count == 0) { list.Add(item); return; }

            list[freeIndices.Pop()] = item;
            //version++; // Don't increment version for additions?  It could result in missing the new value, but shouldn't break open enumerators
        }

        public void Clear()
        {
            list.Clear();
            freeIndices.Clear();
            version++;
        }

        public bool Contains(T item)
        {
            if (item == null) return false;
            return list.Contains(item);
        }

        /// <summary>
        /// Result may contain nulls
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        //public void CopyNonNullTo(T[] array, int arrayIndex)
        //{
        //}

        /// <summary>
        /// Use this for iterating via for loop.
        /// </summary>
        public int Count { get { return list.Count; } }

        /// <summary>
        /// Don't use this for for loops!  Use Count.
        /// </summary>
        public int NonNullCount
        {
            get { return list.Count - freeIndices.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int i = list.IndexOf(item);
            if (i < 0) return false;

            if (i == list.Count - 1)
            {
                // Could throw .  Could add check in 
                list.RemoveAt(i);
            }
            else
            {
                list[i] = null;
                freeIndices.Push(i);
            }
            //version++;  // Don't increment version for removals
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SparseListEnumerator(this);
        }

        private class SparseListEnumerator : IEnumerator<T>
        {
            SparseList<T> list;
            int version;
            int index = -1;

            public SparseListEnumerator(SparseList<T> list)
            {
                this.list = list;
                this.version = list.version;
            }

            public T Current
            {
                get
                {
                    if (index >= list.Count) return null; // Supports removing last items of collection without throwing on Enumerator access
                    return list[index];
                }
            }

            public void Dispose()
            {
                list = null;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (version != list.version) { throw new InvalidOperationException("Collection modified"); }

                do
                {
                    index++;
                } while (Current == null && index < list.Count);

                return index < list.Count;
            }

            public void Reset()
            {
                index = -1;
                version = list.version;
            }

            /// <summary>
            /// Accessing Current after RemoveCurrent may throw a NullReferenceException or return null.
            /// </summary>
            public void RemoveCurrent()
            {
                list.RemoveAt(index);
            }
        }

        private class SparseListCleaningEnumerator : IEnumerator<T>
        {
            SparseList<T> list;
            int version;
            int index = -1;

            public SparseListCleaningEnumerator(SparseList<T> list)
            {
                this.list = list;
                this.version = list.version;

                //while (Current == null && MoveNext()) ;
            }

            public T Current
            {
                get
                {
                    if (index >= list.Count) return null; // Supports removing last items of collection without throwing on Enumerator access
                    return list[index];
                }
            }

            public void Dispose()
            {
                list = null;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                do
                {
                    if (version != list.version) { throw new InvalidOperationException("Collection modified"); }
                    if (index > 0
                        && Current != null // only works for values that are set, otherwise the index is buried in the free index stack somewhere
                        )
                    {
                        int freeIndex = list.freeIndices.Peek();
                        if (freeIndex < index)
                        {
                            list.freeIndices.Pop();
                            list[freeIndex] = list[index];
                            list.RemoveAt(index);
                        }
                    }
                    index++;
                    return index < list.Count;
                } while (Current == null);
            }

            public void Reset()
            {
                index = -1;
                version = list.version;
            }

            /// <summary>
            /// Accessing Current after RemoveCurrent may throw a NullReferenceException or return null.
            /// </summary>
            public void RemoveCurrent()
            {
                list.RemoveAt(index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}