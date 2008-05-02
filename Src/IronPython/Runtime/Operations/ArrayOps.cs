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
using System.Text;
using System.Collections;
using System.Diagnostics;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;
using System.Collections.Generic;


[assembly: PythonExtensionType(typeof(Array), typeof(ArrayOps))]
namespace IronPython.Runtime.Operations {
    // Since this uses [SpecialName] and the method binder, it must be publicly visible.
    // This is conceptually nested in ArrayOps, but publicly visible types shouldn't be nested.
    public class ArrayCallSlot : PythonTypeSlot {
        private PythonType _type;

        public ArrayCallSlot() { }

        public ArrayCallSlot(PythonType type) {
            _type = type;
        }

        internal override bool TryGetBoundValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = new ArrayCallSlot((PythonType)owner);
            return true;
        }

        [SpecialName]
        public object Call(CodeContext context, object sequence) {
            return ArrayOps.__new__(context, _type, sequence);
        }
    }

    public static class ArrayOps {
        #region Python APIs

        [SlotField, SpecialName]
        public static readonly PythonTypeSlot Call = new ArrayCallSlot();


        public static bool __contains__(object[] data, int size, object item) {
            for (int i = 0; i < size; i++) {
                if (PythonOps.EqualRetBool(data[i], item)) return true;
            }
            return false;
        }

        [SpecialName]
        public static Array Add(Array data1, Array data2) {
            if (data1 == null) throw PythonOps.TypeError("expected array for 1st argument, got None");
            if (data2 == null) throw PythonOps.TypeError("expected array for 2nd argument, got None");

            if (data1.Rank > 1 || data2.Rank > 1) throw new NotImplementedException("can't add multidimensional arrays");

            Type type1 = data1.GetType();
            Type type2 = data2.GetType();
            Type type = (type1 == type2) ? type1.GetElementType() : typeof(object);

            Array ret = Array.CreateInstance(type, data1.Length + data2.Length);
            Array.Copy(data1, 0, ret, 0, data1.Length);
            Array.Copy(data2, 0, ret, data1.Length, data2.Length);
            return ret;
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType pythonType, ICollection items) {
            Type type = pythonType.UnderlyingSystemType.GetElementType();

            Array res = Array.CreateInstance(type, items.Count);

            int i = 0;
            foreach(object item in items) {
                res.SetValue(Converter.Convert(item, type), i++);
            }

            return res;
        }
        
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType pythonType, object items) {
            Type type = pythonType.UnderlyingSystemType.GetElementType();

            object lenFunc;
            if (!PythonOps.TryGetBoundAttr(items, Symbols.Length, out lenFunc))
                throw PythonOps.TypeErrorForBadInstance("expected object with __len__ function, got {0}", items);

            int len = Converter.ConvertToInt32(PythonOps.CallWithContext(context, lenFunc));

            Array res = Array.CreateInstance(type, len);

            IEnumerator ie = PythonOps.GetEnumerator(items);
            int i = 0;
            while (ie.MoveNext()) {
                res.SetValue(Converter.Convert(ie.Current, type), i++);
            }

            return res;
        }

        /// <summary>
        /// Multiply two object[] arrays - slow version, we need to get the type, etc...
        /// </summary>
        [SpecialName]
        public static Array Multiply(Array data, int count) {
            if (data.Rank > 1) throw new NotImplementedException("can't multiply multidimensional arrays");

            Type elemType = data.GetType().GetElementType();
            if (count <= 0) return Array.CreateInstance(elemType, 0);

            int newCount = data.Length * count;

            Array ret = Array.CreateInstance(elemType, newCount);
            Array.Copy(data, 0, ret, 0, data.Length);

            // this should be extremely fast for large count as it uses the same algoithim as efficient integer powers
            // ??? need to test to see how large count and n need to be for this to be fastest approach
            int block = data.Length;
            int pos = data.Length;
            while (pos < newCount) {
                Array.Copy(ret, 0, ret, pos, Math.Min(block, newCount - pos));
                pos += block;
                block *= 2;
            }
            return ret;
        }

        [SpecialName]
        public static object GetItem(Array data, int index) {
            if (data == null) throw PythonOps.TypeError("expected Array, got None");

            return data.GetValue(PythonOps.FixIndex(index, data.Length) + data.GetLowerBound(0));
        }

        [SpecialName]
        public static object GetItem(Array data, Slice slice) {
            if (data == null) throw PythonOps.TypeError("expected Array, got None");

            return GetSlice(data, data.Length, slice);
        }

        [SpecialName]
        public static object GetItem(Array data, params object[] indices) {
            if (indices == null || indices.Length < 1) throw PythonOps.TypeError("__getitem__ requires at least 1 parameter");

            int iindex;
            if (indices.Length == 1 && Converter.TryConvertToInt32(indices[0], out iindex)) {
                return GetItem(data, iindex);
            }

            Type t = data.GetType();
            Debug.Assert(t.HasElementType);

            Type elm = t.GetElementType();

            int[] iindices = TupleToIndices(data, indices);
            if (data.Rank != indices.Length) throw PythonOps.ValueError("bad dimensions for array, got {0} expected {1}", indices.Length, data.Rank);

            for (int i = 0; i < iindices.Length; i++) iindices[i] += data.GetLowerBound(i);
            return data.GetValue(iindices);
        }

        [SpecialName]
        public static void SetItem(Array data, int index, object value) {
            if (data == null) throw PythonOps.TypeError("expected Array, got None");

            data.SetValue(Converter.Convert(value, data.GetType().GetElementType()), PythonOps.FixIndex(index, data.Length) + data.GetLowerBound(0));
        }

        [SpecialName]
        public static void SetItem(Array a, params object[] indexAndValue) {
            if (indexAndValue == null || indexAndValue.Length < 2) throw PythonOps.TypeError("__setitem__ requires at least 2 parameters");

            int iindex;
            if (indexAndValue.Length == 2 && Converter.TryConvertToInt32(indexAndValue[0], out iindex)) {
                SetItem(a, iindex, indexAndValue[1]);
                return;
            }

            Type t = a.GetType();
            Debug.Assert(t.HasElementType);

            Type elm = t.GetElementType();

            object[] args = ArrayUtils.RemoveLast(indexAndValue);

            int[] indices = TupleToIndices(a, args);

            if (a.Rank != args.Length) throw PythonOps.ValueError("bad dimensions for array, got {0} expected {1}", args.Length, a.Rank);

            for (int i = 0; i < indices.Length; i++) indices[i] += a.GetLowerBound(i);
            a.SetValue(indexAndValue[indexAndValue.Length - 1], indices);
        }

        [SpecialName]
        public static void SetItem(Array a, Slice index, object value) {
            if (a.Rank != 1) throw PythonOps.NotImplementedError("slice on multi-dimensional array");

            Type elm = a.GetType().GetElementType();

            index.DoSliceAssign(
                delegate(int idx, object val) {
                    a.SetValue(Converter.Convert(val, elm), idx + a.GetLowerBound(0));
                },
                a.Length,
                value);
        }

        public static string __repr__(Array a) {
            if (a == null) throw PythonOps.TypeError("expected array, got None");

            StringBuilder ret = new StringBuilder();
            ret.Append(a.GetType().FullName);
            ret.Append("(");
            switch (a.Rank) {
                case 1: {
                        for (int i = 0; i < a.Length; i++) {
                            if (i > 0) ret.Append(", ");
                            ret.Append(PythonOps.StringRepr(a.GetValue(i + a.GetLowerBound(0))));
                        }
                    }
                    break;
                case 2: {
                        int imax = a.GetLength(0);
                        int jmax = a.GetLength(1);
                        for (int i = 0; i < imax; i++) {
                            ret.Append("\n");
                            for (int j = 0; j < jmax; j++) {
                                if (j > 0) ret.Append(", ");
                                ret.Append(PythonOps.StringRepr(a.GetValue(i + a.GetLowerBound(0), j + a.GetLowerBound(1))));
                            }
                        }
                    }
                    break;
                default:
                    ret.Append(" Multi-dimensional array ");
                    break;
            }
            ret.Append(")");
            return ret.ToString();
        }

        #endregion

        #region Internal APIs

       
        /// <summary>
        /// Multiply two object[] arrays - internal version used for objects backed by arrays
        /// </summary>
        internal static object[] Multiply(object[] data, int size, int count) {
            int newCount = size * count;

            object[] ret = new object[newCount];
            if (count > 0) {
                Array.Copy(data, 0, ret, 0, size);

                // this should be extremely fast for large count as it uses the same algoithim as efficient integer powers
                // ??? need to test to see how large count and n need to be for this to be fastest approach
                int block = size;
                int pos = size;
                while (pos < newCount) {
                    Array.Copy(ret, 0, ret, pos, Math.Min(block, newCount - pos));
                    pos += block;
                    block *= 2;
                }
            }
            return ret;
        }

        /// <summary>
        /// Add two arrays - internal versions for objects backed by arrays
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="size1"></param>
        /// <param name="data2"></param>
        /// <param name="size2"></param>
        /// <returns></returns>
        internal static object[] Add(object[] data1, int size1, object[] data2, int size2) {
            object[] ret = new object[size1 + size2];
            Array.Copy(data1, 0, ret, 0, size1);
            Array.Copy(data2, 0, ret, size1, size2);
            return ret;
        }

        internal static object[] GetSlice(object[] data, int start, int stop) {
            if (stop <= start) return ArrayUtils.EmptyObjects;

            object[] ret = new object[stop - start];
            int index = 0;
            for (int i = start; i < stop; i++) {
                ret[index++] = data[i];
            }
            return ret;
        }

        internal static object[] GetSlice(object[] data, Slice slice) {
            int start, stop, step;
            slice.indices(data.Length, out start, out stop, out step);

            if (step == 1) return GetSlice(data, start, stop);
            
            int size = GetSliceSize(start, stop, step);
            if (size <= 0) return ArrayUtils.EmptyObjects;

            object[] res = new object[size];
            for (int i = 0, index = start; i < res.Length; i++, index += step) {
                res[i] = data[index];
            }
            return res;
        }

        internal static Array GetSlice(Array data, int size, Slice slice) {
            if (data.Rank != 1) throw PythonOps.NotImplementedError("slice on multi-dimensional array");

            int start, stop, step;
            slice.indices(size, out start, out stop, out step);

            if ((step > 0 && start >= stop) || (step < 0 && start <= stop)) {
                if (data.GetType().GetElementType() == typeof(object))
                    return ArrayUtils.EmptyObjects;
                
                return Array.CreateInstance(data.GetType().GetElementType(), 0);
            }

            if (step == 1) {
                int n = stop - start;
                Array ret = Array.CreateInstance(data.GetType().GetElementType(), n);
                Array.Copy(data, start + data.GetLowerBound(0), ret, 0, n);
                return ret;
            } else {                
                int n = GetSliceSize(start, stop, step);
                Array ret = Array.CreateInstance(data.GetType().GetElementType(), n);
                int ri = 0;
                for (int i = 0, index = start; i < n; i++, index += step) {
                    ret.SetValue(data.GetValue(index + data.GetLowerBound(0)), ri++);
                }
                return ret;
            }
        }

        private static int GetSliceSize(int start, int stop, int step) {
            // could cause overflow (?)
            return step > 0 ? (stop - start + step - 1) / step : (stop - start + step + 1) / step;
        }

        internal static object GetIndex(Array a, object index) {
            int iindex;

            if (index is int) {
                iindex = (int)index;
                return a.GetValue(PythonOps.FixIndex(iindex, a.Length) + a.GetLowerBound(0));
            }

            IParameterSequence ituple = index as IParameterSequence;
            if (ituple != null && ituple.IsExpandable) {
                int[] indices = TupleToIndices(a, ((IList<object>)ituple));
                for (int i = 0; i < indices.Length; i++) indices[i] += a.GetLowerBound(i);

                return a.GetValue(indices);
            }

            Slice slice = index as Slice;
            if (slice != null) {
                return GetSlice(a, a.Length, slice);
            }

            // last-ditch effort, try it as a a converted int (this can throw & catch an exception)
            if (Converter.TryConvertToInt32(index, out iindex)) {
                return a.GetValue(PythonOps.FixIndex(iindex, a.Length) + a.GetLowerBound(0));
            }

            throw PythonOps.TypeErrorForBadInstance("bad array index: {0}", index);
        }

        

        #endregion

        #region Private helpers


        private static int[] TupleToIndices(Array a, IList<object> tuple) {
            int[] indices = new int[tuple.Count];
            for (int i = 0; i < indices.Length; i++) {
                indices[i] = PythonOps.FixIndex(Converter.ConvertToInt32(tuple[i]), a.GetUpperBound(i) + 1);
            }
            return indices;
        }

        #endregion
    }
}
