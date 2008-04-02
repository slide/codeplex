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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Threading;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using IronPython.Compiler;
using IronPython.Runtime.Types;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Math;

namespace IronPython.Runtime {

    [PythonSystemType("list"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class List : IMutableSequence, IList, ICodeFormattable, IValueEquality, IList<object> {
        internal int _size;
        internal volatile object[] _data;

        public void __init__(CodeContext context) {
            _data = new object[8];
            _size = 0;
        }

        public void __init__(CodeContext context, object sequence) {
            ICollection items = sequence as ICollection;
            object len;

            if (items != null) {
                if (this == items) {
                    // list.__init__(l, l) resets l
                    _size = 0;
                    return;
                }
                _data = new object[items.Count];
                int i = 0;
                foreach (object item in items) {
                    _data[i++] = item;
                }
                _size = i; 
                return;
            }

            try {
                if (PythonTypeOps.TryInvokeUnaryOperator(context, sequence, Symbols.Length, out len)) {
                    int ilen = Converter.ConvertToInt32(len);
                    _data = new object[ilen];
                    _size = 0;
                    extend(sequence);
                } else {
                    _data = new object[20];
                    _size = 0;
                    extend(sequence);
                }
            } catch (MissingMemberException) {
                _data = new object[20];
                _size = 0;
                extend(sequence);
            }
        }

        private List(IEnumerator e)
            : this(10) {
            while (e.MoveNext()) AddNoLock(e.Current);
        }

        internal List(int capacity) { 
            _data = new object[capacity]; 
            _size = 0; 
        }

        //!!! do we need to copy
        private List(params object[] items) {
            _data = items; 
            _size = _data.Length; 
        } 

        public List() : this(20) { }  //!!! figure out the right default size

        internal List(object sequence) {
            ICollection items = sequence as ICollection;
            object len;

            if (items != null) {
                _data = new object[items.Count];
                int i = 0;
                foreach (object item in items) {
                    _data[i++] = item;
                }
                _size = i;
            } else if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default,
                sequence,
                Symbols.Length,
                out len)) { 
                int ilen = Converter.ConvertToInt32(len);
                _data = new object[ilen];
                extend(sequence);
            } else {
                _data = new object[20];
                extend(sequence);
            }
        }

        internal List(ICollection items)
            : this(items.Count) {

            int i = 0;
            foreach (object item in items) {
                _data[i++] = item;
            }
            _size = i;
        }

        internal object[] GetObjectArray() {
            lock (this) {
                object[] ret = new object[_size];
                Array.Copy(_data, 0, ret, 0, _size);
                return ret;
            }
        }

        #region binary operators

        public static List operator +([NotNull]List l1, [NotNull]List l2) {
            object[] them = l2.GetObjectArray();
            lock (l1) {
                object[] ret = new object[l1._size + them.Length];

                Array.Copy(l1._data, 0, ret, 0, l1._size);
                Array.Copy(them, 0, ret, l1._size, them.Length);

                return new List(ret);
            }
        }

        public static List operator *([NotNull]List l, int count) {
            return MultiplyWorker(l, count);
        }

        public static List operator *(int count, List l) {
            return MultiplyWorker(l, count);
        }

        public static object operator *([NotNull]List self, object count) {
            return PythonOps.MultiplySequence<List>(MultiplyWorker, self, count, true);
        }

        public static object operator *(object count, [NotNull]List self) {
            return PythonOps.MultiplySequence<List>(MultiplyWorker, self, count, false);
        }

        private static List MultiplyWorker(List self, int count) {
            if (count <= 0) return PythonOps.MakeEmptyList(0);

            int n, newCount;
            object[] ret;
            lock (self) {
                n = self._size;
                //??? is this useful optimization
                //???if (n == 1) return new List(Array.ArrayList.Repeat(this[0], count));

                newCount = n * count;
                ret = new object[newCount];

                Array.Copy(self._data, 0, ret, 0, n);
            }

            // this should be extremely fast for large count as it uses the same algoithim as efficient integer powers
            // ??? need to test to see how large count and n need to be for this to be fastest approach
            int block = n;
            int pos = n;
            while (pos < newCount) {
                Array.Copy(ret, 0, ret, pos, Math.Min(block, newCount - pos));
                pos += block;
                block *= 2;
            }
            return new List(ret);
        }



        #endregion

        public virtual int __len__() {
            return _size;
        }

        public virtual IEnumerator __iter__() {
            // return type is strongly typed to IEnumerator so that
            // we can call it w/o requiring an explicit conversion.  If the
            // user overrides this we'll place a conversion in the wrapper
            // helper
            return new listiterator(this);
        }

        public virtual bool __contains__(object value) {
            lock (this) {
                for (int i = 0; i < _size; i++) {
                    object thisIndex = _data[i];

                    // release the lock while we may call user code...
                    Monitor.Exit(this);
                    try {
                        if (PythonOps.EqualRetBool(thisIndex, value))
                            return true;
                    } finally {
                        Monitor.Enter(this);
                    }
                }
            }
            return false;
        }

        #region ISequence Members

        internal void AddRange<T>(ICollection<T> otherList) {
            foreach (object o in otherList) append(o);
        }

        [SpecialName]
        public virtual object InPlaceAdd(object other) {
            if (!Object.ReferenceEquals(this, other)) {
                IEnumerator e = PythonOps.GetEnumerator(other);
                while (e.MoveNext()) {
                    append(e.Current);
                }
            } else {
                InPlaceMultiply(2);                
            }

            return this;
        }

        [SpecialName]
        public List InPlaceMultiply(int count) {
            lock (this) {
                int n = this._size;
                int newCount = n * count;
                EnsureSize(newCount);

                int block = n;
                int pos = n;
                while (pos < newCount) {
                    Array.Copy(_data, 0, _data, pos, Math.Min(block, newCount - pos));
                    pos += block;
                    block *= 2;
                }
                this._size = newCount;
            }
            return this;
        }

        [SpecialName]
        public object InPlaceMultiply(object count) {
            return PythonOps.MultiplySequence<List>(InPlaceMultiplyWorker, this, count, true);
        }

        private static List InPlaceMultiplyWorker(List self, int count) {
            return self.InPlaceMultiply(count);
        }

        public virtual object __getslice__(int start, int stop) {
            lock (this) {
                Slice.FixSliceArguments(_size, ref start, ref stop);
                
                object[] ret = ArrayOps.GetSlice(_data, start, stop);
                return new List(ret);
            }            
        }

        internal object[] GetSliceAsArray(int start, int stop) {
            if (start < 0) start = 0;
            if (stop > __len__()) stop = __len__();

            lock (this) return ArrayOps.GetSlice(_data, start, stop);
        }

        public virtual void __setslice__(int start, int stop, object value) {
            Slice.FixSliceArguments(_size, ref start, ref stop);
            if (start > stop) return;

            SliceNoStep(start, stop, value);
        }

        public virtual void __delslice__(int start, int stop) {
            lock (this) {
                Slice.FixSliceArguments(_size, ref start, ref stop);
                if (start > stop) return;

                int i = start;
                for (int j = stop; j < _size; j++, i++) {
                    _data[i] = _data[j];
                }
                _size -= stop - start;
            }
        }

        public virtual object this[Slice slice] {
            get {
                if (slice == null) throw PythonOps.TypeError("list indicies must be integer or slice, not None");

                int start, stop, step;
                slice.indices(_size, out start, out stop, out step);

                if ((step > 0 && start >= stop) || (step < 0 && start <= stop)) return new List();

                if (step == 1) {
                    object[] ret;
                    lock (this) ret = ArrayOps.GetSlice(_data, start, stop);
                    return new List(ret);
                } else {
                    // start/stop/step could be near Int32.MaxValue, and simply addition could cause overflow
                    int n = (int)(step > 0 ? (0L + stop - start + step - 1) / step : (0L + stop - start + step + 1) / step);
                    object[] ret = new object[n];
                    lock (this) {
                        int ri = 0;
                        for (int i = 0, index = start; i < n; i++, index += step) {
                            ret[ri++] = _data[index];
                        }
                    }
                    return new List(ret);

                }
            }
            set {
                if (slice == null) throw PythonOps.TypeError("list indicies must be integer or slice, not None");

                if (slice.step != null) {
                    // try to assign back to self: make a copy first
                    if (this == value) value = new List(value);

                    slice.DoSliceAssign(this.SliceAssign, _size, value);
                } else {
                    int start, stop, step;
                    slice.indices(_size, out start, out stop, out step);
                    if (start > stop) return;

                    SliceNoStep(start, stop, value);
                }
            }
        }

        private void SliceNoStep(int start, int stop, object value) {
            IEnumerator enumerator = PythonOps.GetEnumerator(value);

            // save a ref to myData incase other calls cause us 
            // to re-size.

            List newList = new List(_data.Length); // race is tolerable...

            lock (this) {
                for (int i = 0; i < start; i++) {
                    newList.AddNoLock(_data[i]);
                }
            }

            // calling user code, get rid of the lock...
            while (enumerator.MoveNext()) {
                newList.AddNoLock(enumerator.Current);
            }

            lock (this) {
                for (int i = stop; i < _size; i++) {
                    newList.AddNoLock(_data[i]);
                }

                if (newList._data.Length < _data.Length) {
                    // shrinking our array may result in IndexOutOfRange in
                    // this[...] where we read w/o a lock.
                    Array.Copy(newList._data, _data, newList._data.Length);
                } else {
                    this._data = newList._data;
                }

                this._size = newList._size;
            }
        }


        private void SliceAssign(int index, object value) {
            this[index] = value;
        }

        public virtual void __delitem__(int index) {
            lock (this) RawDelete(PythonOps.FixIndex(index, _size));
        }

        public virtual void __delitem__(object index) {
            __delitem__(Converter.ConvertToIndex(index));
        }

        public void __delitem__(Slice slice) {
            if (slice == null) throw PythonOps.TypeError("list indicies must be integers or slices");

            lock (this) {
                int start, stop, step;
                // slice is sealed, indicies can't be user code...
                slice.indices(_size, out start, out stop, out step);

                if (step > 0 && (start >= stop)) return;
                if (step < 0 && (start <= stop)) return;

                if (step == 1) {
                    int i = start;
                    for (int j = stop; j < _size; j++, i++) {
                        _data[i] = _data[j];
                    }
                    _size -= stop - start;
                    return;
                }
                if (step == -1) {
                    int i = stop + 1;
                    for (int j = start + 1; j < _size; j++, i++) {
                        _data[i] = _data[j];
                    }
                    _size -= start - stop;
                    return;
                }

                if (step < 0) {
                    // find "start" we will skip in the 1,2,3,... order
                    int i = start;
                    while (i > stop) {
                        i += step;
                    }
                    i -= step;

                    // swap start/stop, make step positive
                    stop = start + 1;
                    start = i;
                    step = -step;
                }

                int curr, skip, move;
                // skip: the next position we should skip
                // curr: the next position we should fill in data
                // move: the next position we will check
                curr = skip = move = start;

                while (curr < stop && move < stop) {
                    if (move != skip) {
                        _data[curr++] = _data[move];
                    } else
                        skip += step;
                    move++;
                }
                while (stop < _size) {
                    _data[curr++] = _data[stop++];
                }
                _size = curr;
            }
        }

        #endregion

        private void RawDelete(int index) {
            int len = _size - 1;
            _size = len;
            object[] tempData = _data;
            for (int i = index; i < len; i++) {
                tempData[i] = tempData[i + 1];
            }
            tempData[len] = null;
        }


        internal void EnsureSize(int needed) {
            if (_data.Length >= needed) return;

            int newSize = Math.Max(_size * 2, 4);
            while (newSize < needed) newSize *= 2;
            object[] newData = new object[newSize];
            _data.CopyTo(newData, 0);
            _data = newData;
        }

        public void append(object item) {
            lock (this) {
                AddNoLock(item);
            }
        }

        /// <summary>
        /// Non-thread safe adder, should only be used by internal callers that
        /// haven't yet exposed their list.
        /// </summary>
        internal void AddNoLock(object item) {
            EnsureSize(_size + 1);
            _data[_size] = item;
            _size += 1;
        }

        internal void AddNoLockNoDups(object item) {
            for (int i = 0; i < _size; i++) {
                if (PythonOps.EqualRetBool(_data[i], item)) {
                    return;
                }
            }

            AddNoLock(item);
        }

        internal void AppendListNoLockNoDups(List list) {
            if (list != null) {
                foreach (object item in list) {
                    AddNoLockNoDups(item);
                }
            }
        }

        public int count(object item) {
            lock (this) {
                int cnt = 0;
                for (int i = 0, len = _size; i < len; i++) {
                    object val = _data[i];

                    Monitor.Exit(this);
                    try {
                        if (PythonOps.EqualRetBool(val, item)) cnt++;
                    } finally {
                        Monitor.Enter(this);
                    }
                }
                return cnt;
            }
        }

        public void extend(object seq) {
            //!!! optimize case of easy sequence (List or Tuple)

            IEnumerator i = PythonOps.GetEnumerator(seq);
            if (seq == (object)this) {
                List other = new List(i);
                i = ((IEnumerable)other).GetEnumerator();
            }
            while (i.MoveNext()) append(i.Current);
        }

        public int index(object item) {
            return index(item, 0, _size);
        }

        public int index(object item, int start) {
            return index(item, start, _size);
        }

        public int index(object item, int start, int stop) {
            // CPython behavior for index is to only look at the 
            // original items.  If new items are added they
            // are ignored, but if items are removed they
            // aren't iterated.  We therefore get a stable view
            // of our data, and then go with the minimum between
            // our starting size and ending size.

            object[] locData;
            int locSize;
            lock (this) {
                // get a stable view on size / data...
                locData = _data;
                locSize = _size;
            }

            start = PythonOps.FixSliceIndex(start, locSize);
            stop = PythonOps.FixSliceIndex(stop, locSize);

            for (int i = start; i < Math.Min(stop, Math.Min(locSize, _size)); i++) {
                if (PythonOps.EqualRetBool(locData[i], item)) return i;
            }

            throw PythonOps.ValueError("list.index(item): item not in list");
        }

        public int index(object item, object start) {
            return index(item, Converter.ConvertToIndex(start), _size);
        }

        public int index(object item, object start, object stop) {
            return index(item, Converter.ConvertToIndex(start), Converter.ConvertToIndex(stop));
        }

        public void insert(int index, object value) {
            if (index >= _size) {
                append(value);
                return;
            }

            lock (this) {
                index = PythonOps.FixSliceIndex(index, _size);

                EnsureSize(_size + 1);
                _size += 1;
                for (int i = _size - 1; i > index; i--) {
                    _data[i] = _data[i - 1];
                }
                _data[index] = value;
            }
        }

        void IList.Insert(int index, object value) {
            insert(index, value);
        }

        public object pop() {
            if (this._size == 0) throw PythonOps.IndexError("pop off of empty list");

            lock (this) {
                this._size -= 1;
                return _data[this._size];
            }
        }

        public object pop(int index) {
            lock (this) {
                index = PythonOps.FixIndex(index, _size);
                if (_size == 0) throw PythonOps.IndexError("pop off of empty list");

                object ret = _data[index];
                _size -= 1;
                for (int i = index; i < _size; i++) {
                    _data[i] = _data[i + 1];
                }
                return ret;
            }
        }

        public void remove(object value) {
            lock (this) RawDelete(index(value));
        }

        void IList.Remove(object value) {
            remove(value);
        }

        public void reverse() {
            lock (this) Array.Reverse(_data, 0, _size);
        }

        internal void reverse(int index, int count) {
            lock (this) Array.Reverse(_data, index, count);
        }

        private class DefaultPythonComparer : IComparer {
            public static readonly DefaultPythonComparer Instance = new DefaultPythonComparer();
            private FastDynamicSite<object, object, int> site = FastDynamicSite<object, object, int>.Create(DefaultContext.DefaultCLS, DoOperationAction.Make(Operators.Compare));
            public DefaultPythonComparer() { }

            public int Compare(object x, object y) {
                //??? Putting this optimization here is awfully special case, but comes close to halving sort time for int lists
                //				if (x is int && y is int) {
                //					int xi = (int)x;
                //					int yi = (int)y;
                //					return xi == yi ? 0 : (xi < yi ? -1 : +1);
                //				}

                return site.Invoke(x, y);
            }
        }

        private class FunctionComparer : IComparer {
            //??? optimized version when we know we have a Function
            private object cmpfunc;

            private FastDynamicSite<object, object, object, int> FuncSite = 
                RuntimeHelpers.CreateSimpleCallSite<object, object, object, int>(DefaultContext.Default);

            public FunctionComparer(object cmpfunc) { this.cmpfunc = cmpfunc; }

            public int Compare(object o1, object o2) {
                return FuncSite.Invoke(cmpfunc, o1, o2);
            }
        }

        public void sort() {
            sort(null, null, false);
        }

        public void sort(object cmp) {
            sort(cmp, null, false);
        }

        public void sort(object cmp, object key) {
            sort(cmp, key, false);
        }

        public void sort([DefaultParameterValue(null)] object cmp,
                         [DefaultParameterValue(null)] object key,
                         [DefaultParameterValue(false)] bool reverse) {
            IComparer comparer = (cmp == null) ?
                (IComparer)new DefaultPythonComparer() :
                (IComparer)new FunctionComparer(cmp);

            DoSort(comparer, key, reverse, 0, _size);
        }

        internal void DoSort(IComparer cmp, object key, bool reverse, int index, int count) {
            lock (this) {
                object[] sortData = _data;
                int sortSize = _size;

                try {
                    // make the list appear empty for the duration of the sort...
                    _data = ArrayUtils.EmptyObjects;
                    _size = 0;

                    if (key != null) {
                        object[] keys = new object[sortSize];
                        for (int i = 0; i < sortSize; i++) {
                            Debug.Assert(_data.Length == 0);
                            keys[i] = PythonCalls.Call(key, sortData[i]);
                            if (_data.Length != 0) throw PythonOps.ValueError("list mutated while determing keys");
                        }

                        sortData = ListMergeSort(sortData, keys, cmp, index, count, reverse);
                    } else {
                        sortData = ListMergeSort(sortData, cmp, index, count, reverse);
                    }
                } finally {
                    // restore the list to it's old data & size (which is now supported appropriately)
                    _data = sortData;
                    _size = sortSize;
                }

            }
        }

        internal object[] ListMergeSort(object[] sortData, IComparer cmp, int index, int count, bool reverse) {
            return ListMergeSort(sortData, null, cmp, index, count, reverse);
        }

        internal object[] ListMergeSort(object[] sortData, object[] keys, IComparer cmp, int index, int count, bool reverse) {
            if (count - index < 2) return sortData;  // 1 or less items, we're sorted, quit now...

            if (keys == null) keys = sortData;
            // list merge sort - stable sort w/ a minimum # of comparisons.

            int len = count - index;
            // prepare the two lists.
            int[] lists = new int[len + 2];    //0 and count + 1 are auxillary fields

            lists[0] = 1;
            lists[len + 1] = 2;
            for (int i = 1; i <= len - 2; i++) {
                lists[i] = -(i + 2);
            }

            lists[len - 1] = lists[len] = 0;

            // new pass
            for (; ; ) {
                // p & q  traverse the lists during each pass.  
                //  s is usually the most most recently processed record of the current sublist
                //  t points to the end of the previously output sublist
                int s = 0;
                int t = len + 1;
                int p = lists[s];
                int q = lists[t];

                if (q == 0) break;  // we're done
                for (; ; ) {
                    // Indexes into the array here are 1 based.  0 is a 
                    // virtual element and so is (len - 1) - they only exist in
                    // the length array.

                    if ((p < 1) || (q <= len && DoCompare(keys, cmp, p + index - 1, q + index - 1, reverse))) {
                        // advance p
                        if (lists[s] < 0) lists[s] = Math.Abs(p) * -1;
                        else lists[s] = Math.Abs(p);

                        s = p;
                        p = lists[p];

                        if (p > 0) continue;

                        // complete the sublist
                        lists[s] = q;
                        s = t;
                        do {
                            t = q;
                            q = lists[q];
                        } while (q > 0);
                    } else {
                        // advance q
                        if (lists[s] < 0) lists[s] = Math.Abs(q) * -1;
                        else lists[s] = Math.Abs(q);

                        s = q;
                        q = lists[q];

                        if (q > 0) continue;

                        // Complete the sublist
                        lists[s] = p;
                        s = t;

                        do {
                            t = p;
                            p = lists[p];
                        } while (p > 0);
                    }

                    Debug.Assert(p <= 0);
                    Debug.Assert(q <= 0);
                    p *= -1;
                    q *= -1;

                    if (q == 0) {
                        if (lists[s] < 0) lists[s] = Math.Abs(p) * -1;
                        else lists[s] = Math.Abs(p);
                        lists[t] = 0;
                        // go back to new pass
                        break;
                    } // else keep going
                }
            }

            // use the resulting indicies to
            // extract the order.
            object[] newData = new object[len];
            int start = lists[0];
            int outIndex = 0;
            while (start != 0) {
                newData[outIndex++] = sortData[start + index - 1];
                start = lists[start];
            }

            if (sortData.Length != count || index != 0) {
                for (int j = 0; j < count; j++) {
                    sortData[j + index] = newData[j];
                }
            } else {
                sortData = newData;
            }

            return sortData;
        }

        /// <summary>
        /// Compares the two specified keys
        /// </summary>
        private bool DoCompare(object[] keys, IComparer cmp, int p, int q, bool reverse) {
            Debug.Assert(_data.Length == 0);

            int result = cmp.Compare(keys[p], keys[q]);
            bool ret = reverse ? (result >= 0) : (result <= 0);

            if (_data.Length != 0) throw PythonOps.ValueError("list mutated during sort");
            return ret;
        }

        internal int BinarySearch(int index, int count, object value, IComparer comparer) {
            lock (this) return Array.BinarySearch(_data, index, count, value, comparer);
        }

        internal int CompareToWorker(List l) {
            // we need to lock both objects (or copy all of one's data w/ it's lock held, and
            // then compare, which is bad).  Therefore we have a strong order for locking on 
            // the lists
            int result;
            if (this.GetHashCode() < l.GetHashCode()) {
                lock (this) lock (l) result = PythonOps.CompareArrays(_data, _size, l._data, l._size);
            } else if (this.GetHashCode() != l.GetHashCode()) {
                lock (l) lock (this) result = PythonOps.CompareArrays(_data, _size, l._data, l._size);
            } else {
                // rare, but possible.  We need a second opinion
                if (IdDispenser.GetId(this) < IdDispenser.GetId(l))
                    lock (this) lock (l) result = PythonOps.CompareArrays(_data, _size, l._data, l._size);
                else
                    lock (l) lock (this) result = PythonOps.CompareArrays(_data, _size, l._data, l._size);
            }
            return result;
        }

        #region IList Members

        bool IList.IsReadOnly {
            get { return false; }
        }

        public virtual object this[int index] {
            get {
                // no locks works here, we either return an
                // old item (as if we were called first) or return
                // a current item...        

                // force reading the array first, _size can change after
                object[] data = GetData();  

                return data[PythonOps.FixIndex(index, _size)];
            }
            set {
                // but we need a lock here incase we're assigning
                // while re-sizing.
                lock (this) _data[PythonOps.FixIndex(index, _size)] = value;
            }
        }

        public virtual object this[BigInteger index] {
            get {
                return this[index.ToInt32()];
            }
            set {
                this[index.ToInt32()] = value;
            }
        }

        /// <summary>
        /// Supports __index__ on arbitrary types, also prevents __float__
        /// </summary>
        public virtual object this[object index] {
            get {
                return this[Converter.ConvertToIndex(index)];
            }
            set {
                this[Converter.ConvertToIndex(index)] = value;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private object[] GetData() {
            return _data;
        }

        void IList.RemoveAt(int index) {
            lock (this) RawDelete(index);
        }

        bool IList.Contains(object value) {
            return __contains__(value);
        }

        void IList.Clear() {
            lock (this) _size = 0;
        }

        int IList.IndexOf(object value) {
            // we get a stable view of the list, and if user code
            // clears it then we'll stop iterating.
            object[] locData;
            int locSize;
            lock (this) {
                locData = _data;
                locSize = _size;
            }

            for (int i = 0; i < Math.Min(locSize, _size); i++) {
                if (PythonOps.EqualRetBool(locData[i], value)) return i;
            }
            return -1;
        }

        int IList.Add(object value) {
            lock (this) {
                AddNoLock(value);
                return _size - 1;
            }
        }

        bool IList.IsFixedSize {
            get { return false; }
        }

        #endregion

        #region ICollection Members

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        int ICollection.Count {
            get { return __len__(); }
        }

        void ICollection.CopyTo(Array array, int index) {
            Array.Copy(_data, 0, array, index, _size);
        }

        internal void CopyTo(Array array, int index, int arrayIndex, int count) {
            Array.Copy(_data, index, array, arrayIndex, count);
        }

        object ICollection.SyncRoot {
            get {
                return this;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return __iter__();
        }

        #endregion

        #region ICodeFormattable Members

        public virtual string/*!*/ __repr__(CodeContext/*!*/ context) {
            StringBuilder buf = new StringBuilder();
            buf.Append("[");
            for (int i = 0; i < _size; i++) {
                if (i > 0) buf.Append(", ");
                buf.Append(PythonOps.StringRepr(_data[i]));
            }
            buf.Append("]");
            return buf.ToString();
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            throw PythonOps.TypeError("list object is unhashable");
        }

        bool IValueEquality.ValueEquals(object other) {
            List l = other as List;

            if (l == null || l.__len__() != this.__len__()) return false;
            return CompareTo(l) == 0;
        }

        bool IValueEquality.ValueNotEquals(object other) {
            return !((IValueEquality)this).ValueEquals(other);
        }

        #endregion

        #region IList<object> Members

        int IList<object>.IndexOf(object item) {
            return ((IList)this).IndexOf(item);
        }

        void IList<object>.Insert(int index, object item) {
            this.insert(index, item);
        }

        void IList<object>.RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        object IList<object>.this[int index] {
            get {
                return this[index];
            }
            set {
                this[index] = value;
            }
        }

        #endregion

        #region ICollection<object> Members

        void ICollection<object>.Add(object item) {
            append(item);
        }

        void ICollection<object>.Clear() {
            ((IList)this).Clear();
        }

        bool ICollection<object>.Contains(object item) {
            return this.__contains__(item);
        }

        void ICollection<object>.CopyTo(object[] array, int arrayIndex) {
            for (int i = 0; i < __len__(); i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        int ICollection<object>.Count {
            get { return __len__(); }
        }

        bool ICollection<object>.IsReadOnly {
            get { return ((IList)this).IsReadOnly; }
        }

        bool ICollection<object>.Remove(object item) {
            if (this.__contains__(item)) {
                this.remove(item);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return new IEnumeratorOfTWrapper<object>(((IEnumerable)this).GetEnumerator());
        }

        #endregion

        private int CompareTo(List other) {
            CompareUtil.Push(this, other);
            try {
                return CompareToWorker(other);
            } finally {
                CompareUtil.Pop(this, other);
            }
        }

        #region Rich Comparison Members

        [return: MaybeNotImplemented]
        public static object operator > (List self, object other) {
            List l = other as List;
            if (l == null) return PythonOps.NotImplemented;

            return self.CompareTo(l) > 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        [return: MaybeNotImplemented]
        public static object operator <(List self, object other) {
            List l = other as List;
            if (l == null) return PythonOps.NotImplemented;

            return self.CompareTo(l) < 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        [return: MaybeNotImplemented]
        public static object operator >=(List self, object other) {
            List l = other as List;
            if (l == null) return PythonOps.NotImplemented;

            return self.CompareTo(l) >= 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        [return: MaybeNotImplemented]
        public static object operator <=(List self, object other) {
            List l = other as List;
            if (l == null) return PythonOps.NotImplemented;

            return self.CompareTo(l) <= 0 ? RuntimeHelpers.True : RuntimeHelpers.False;
        }

        #endregion
    }

    public class listiterator : IEnumerator, IEnumerable, IEnumerable<object>, IEnumerator<object> {
        private int index = -1;
        private List l;
        private bool sinkState = false;

        public listiterator(List l) { this.l = l; }

        #region IEnumerator Members

        public void Reset() {
            index = -1;
        }

        public object Current {
            get {
                return l._data[index];
            }
        }

        public bool MoveNext() {
            if (sinkState) return false;

            index++;
            bool hit = index > l._size - 1;
            if (hit) sinkState = true;
            return !hit;
        }

        public object __iter__() {
            return this;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return this;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
        }

        #endregion

        #region IEnumerable<object> Members

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this;
        }

        #endregion
    }
}
