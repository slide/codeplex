/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Contracts;

namespace System.Scripting.Utils {
    public static class CollectionUtils {

        public static void AddRange<T>(ICollection<T> collection, IEnumerable<T> items) {
            ContractUtils.RequiresNotNull(collection, "collection");
            ContractUtils.RequiresNotNull(items, "items");

            List<T> list = collection as List<T>;
            if (list != null) {
                list.AddRange(items);
            } else {
                foreach (T item in items) {
                    collection.Add(item);
                }
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(IEnumerable enumerable) {
            foreach (T item in enumerable) {
                yield return item;
            }
        }

        public static IEnumerator<TSuper> ToCovariant<T, TSuper>(IEnumerator<T> enumerator)
            where T : TSuper {

            ContractUtils.RequiresNotNull(enumerator, "enumerator");

            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }

        public static IEnumerable<TSuper> ToCovariant<T, TSuper>(IEnumerable<T> enumerable)
            where T : TSuper {
            return new CovariantConvertor<T, TSuper>(enumerable);
        }

        private class CovariantConvertor<T, TSuper> : IEnumerable<TSuper> where T : TSuper {
            private IEnumerable<T> _enumerable;

            public CovariantConvertor(IEnumerable<T> enumerable) {
                ContractUtils.RequiresNotNull(enumerable, "enumerable");
                _enumerable = enumerable;
            }

            [Pure]
            public IEnumerator<TSuper>/*!*/ GetEnumerator() {
                return CollectionUtils.ToCovariant<T, TSuper>(_enumerable.GetEnumerator());
            }

            [Pure]
            IEnumerator/*!*/ IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        public static List<T> MakeList<T>(T item) {
            List<T> result = new List<T>();
            result.Add(item);
            return result;
        }

        public static int CountOf<T>(IList<T> list, T item) where T : IEquatable<T> {
            if (list == null) return 0;

            int result = 0;
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Equals(item)) {
                    result++;
                }
            }
            return result;
        }

        public static bool TrueForAll<T>(IList<T> collection, Predicate<T> predicate) {
            ContractUtils.RequiresNotNull(collection, "collection");
            ContractUtils.RequiresNotNull(predicate, "predicate");

            foreach (T item in collection) {
                if (!predicate(item)) return false;
            }

            return true;
        }

        public static List<T> GetRange<T>(IList<T> list, int index, int count) {
            ContractUtils.RequiresNotNull(list, "list");
            ContractUtils.RequiresArrayRange(list, index, count, "index", "count");

            List<T> result = new List<T>(count);
            int stop = index + count;
            for (int i = index; i < stop; i++) {
                result.Add(list[i]);
            }
            return result;
        }

        public static void InsertRange<T>(IList<T> collection, int index, IEnumerable<T> items) {
            ContractUtils.RequiresNotNull(collection, "collection");
            ContractUtils.RequiresNotNull(items, "items");
            ContractUtils.RequiresArrayInsertIndex(collection, index, "index");

            List<T> list = collection as List<T>;
            if (list != null) {
                list.InsertRange(index, items);
            } else {
                int i = index;
                foreach (T obj in items) {
                    collection.Insert(i++, obj);
                }
            }
        }

        public static void RemoveRange<T>(IList<T> collection, int index, int count) {
            ContractUtils.RequiresNotNull(collection, "collection");
            ContractUtils.RequiresArrayRange(collection, index, count, "index", "count");

            List<T> list = collection as List<T>;
            if (list != null) {
                list.RemoveRange(index, count);
            } else {
                for (int i = index + count - 1; i >= index; i--) {
                    collection.RemoveAt(i);
                }
            }
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(IEnumerable<T> enumerable) {
            ReadOnlyCollection<T> readOnlyCollection;
            ICollection<T> collection;
            if (enumerable == null) {
                return DefaultReadOnlyCollection<T>.Empty;
            } else if ((readOnlyCollection = enumerable as ReadOnlyCollection<T>) != null) {
                return readOnlyCollection;
            } else if ((collection = enumerable as ICollection<T>) != null) {
                int count = collection.Count;
                if (count == 0) {
                    return DefaultReadOnlyCollection<T>.Empty;
                }
                T[] array = new T[count];
                collection.CopyTo(array, 0);
                return new ReadOnlyCollection<T>(array);
            } else {
                // ToArray trims the excess space and speeds up access
                return new ReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
            }
        }

        internal static int GetHashCode<T>(IEnumerable<T> list) {
            int h = 6551;
            foreach (T t in list) {
                h ^= (h << 5) ^ t.GetHashCode();
            }
            return h;
        }

        internal static bool Equal<T>(ICollection<T> first, ICollection<T> second) {
            if (first.Count != second.Count) {
                return false;
            }
            IEnumerator<T> f = first.GetEnumerator();
            IEnumerator<T> s = second.GetEnumerator();
            while (f.MoveNext()) {
                s.MoveNext();

                if (!object.Equals(f.Current, s.Current)) {
                    return false;
                }
            }

            return true;
        }
    }

    internal static class DefaultReadOnlyCollection<T> {
        internal static ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T>(new T[0]);
    }
}
