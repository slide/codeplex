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

#if !SILVERLIGHT // ComObject

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Scripting.Actions.ComDispatch {

    /// <summary>
    /// Variant is the basic COM type for late-binding. It can contain any other COM data type.
    /// 
    /// This type definition precisely matches the unmanaged data layout so that the struct can be passed
    /// to and from COM calls. Its size is the size of 4 pointers (16 bytes on a 32-bit processor, 
    /// and 32 bytes on a 64-bit processor)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Variant {

#if DEBUG
        static Variant() { Debug.Assert(Marshal.SizeOf(typeof(Variant)) == (4 * Marshal.SizeOf(typeof(IntPtr)))); }
#endif

        [FieldOffset(0)]
        private TypeUnion _typeUnion;

        [FieldOffset(0)]
        private Decimal _decimal;

        [StructLayout(LayoutKind.Sequential)]
        internal struct TypeUnion {
            internal ushort _vt;
            internal ushort _wReserved1;
            internal ushort _wReserved2;
            internal ushort _wReserved3;

            internal UnionTypes _unionTypes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TwoIntPtrs {
            internal IntPtr _intPtr1;
            internal IntPtr _intPtr2;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
        [StructLayout(LayoutKind.Explicit)]
        internal struct UnionTypes {
            #region Generated Variant union types

            // *** BEGIN GENERATED CODE ***

            [FieldOffset(0)] internal SByte _i1;
            [FieldOffset(0)] internal Int16 _i2;
            [FieldOffset(0)] internal Int32 _i4;
            [FieldOffset(0)] internal Int64 _i8;
            [FieldOffset(0)] internal Byte _ui1;
            [FieldOffset(0)] internal UInt16 _ui2;
            [FieldOffset(0)] internal UInt32 _ui4;
            [FieldOffset(0)] internal UInt64 _ui8;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            [FieldOffset(0)] internal IntPtr _int;
            [FieldOffset(0)] internal UIntPtr _uint;
            [FieldOffset(0)] internal Int32 _bool;
            [FieldOffset(0)] internal Single _r4;
            [FieldOffset(0)] internal Double _r8;
            [FieldOffset(0)] internal double _date;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            [FieldOffset(0)] internal IntPtr _bstr;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            [FieldOffset(0)] internal IntPtr _unknown;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            [FieldOffset(0)] internal IntPtr _dispatch;

            // *** END GENERATED CODE ***

            #endregion

            [FieldOffset(0)] internal TwoIntPtrs _twoIntPtrs;
        }

        public override string ToString() {
            return String.Format("{0} ({1}", ToObject().ToString(), VariantType);
        }

        # region FxCop-required APIs

        public override bool Equals(object obj) {
            if ((obj == null) || (!(obj is Variant))) {
                return false;
            }

            Variant other = (Variant)obj;
            return _typeUnion._vt == other._typeUnion._vt &&
                _typeUnion._wReserved1 == other._typeUnion._wReserved1 &&
                _typeUnion._wReserved2 == other._typeUnion._wReserved2 &&
                _typeUnion._wReserved3 == other._typeUnion._wReserved3 &&
                _typeUnion._unionTypes._twoIntPtrs._intPtr1 == other._typeUnion._unionTypes._twoIntPtrs._intPtr1 &&
                _typeUnion._unionTypes._twoIntPtrs._intPtr2 == other._typeUnion._unionTypes._twoIntPtrs._intPtr2;
        }

        public override int GetHashCode() {
            return (int)(
                (_typeUnion._vt | (_typeUnion._wReserved1 >> 16)) ^
                (_typeUnion._wReserved2 ^ (_typeUnion._wReserved3 >> 16)) ^ 
                _typeUnion._unionTypes._twoIntPtrs._intPtr1.ToInt32() ^
                _typeUnion._unionTypes._twoIntPtrs._intPtr1.ToInt32());
        }

        public static bool operator ==(Variant a, Variant b) {
            return a.Equals(b);
        }
        public static bool operator !=(Variant a, Variant b) {
            return !a.Equals(b);
        }

        #endregion

        /// <summary>
        /// Primitive types are the basic COM types. It includes valuetypes like ints, but also reference tyeps
        /// like BStrs. It does not include composite types like arrays and user-defined COM types (IUnknown/IDispatch).
        /// </summary>
        internal static bool IsPrimitiveType(VarEnum varEnum) {
            switch(varEnum) {
                #region Generated Variant IsPrimitiveType

                // *** BEGIN GENERATED CODE ***

                case VarEnum.VT_I1:
                case VarEnum.VT_I2:
                case VarEnum.VT_I4:
                case VarEnum.VT_I8:
                case VarEnum.VT_UI1:
                case VarEnum.VT_UI2:
                case VarEnum.VT_UI4:
                case VarEnum.VT_UI8:
                case VarEnum.VT_INT:
                case VarEnum.VT_UINT:
                case VarEnum.VT_BOOL:
                case VarEnum.VT_R4:
                case VarEnum.VT_R8:
                case VarEnum.VT_DECIMAL:
                case VarEnum.VT_DATE:
                case VarEnum.VT_BSTR:

                // *** END GENERATED CODE ***

                #endregion
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the managed object representing the Variant.
        /// </summary>
        /// <returns></returns>
        public object ToObject() {
            // Check the simple case upfront
            if (IsEmpty) {
                return null;
            }

            switch (VariantType) {
                case VarEnum.VT_NULL: return DBNull.Value;

                #region Generated Variant ToObject

                // *** BEGIN GENERATED CODE ***

                case VarEnum.VT_I1: return AsI1;
                case VarEnum.VT_I2: return AsI2;
                case VarEnum.VT_I4: return AsI4;
                case VarEnum.VT_I8: return AsI8;
                case VarEnum.VT_UI1: return AsUi1;
                case VarEnum.VT_UI2: return AsUi2;
                case VarEnum.VT_UI4: return AsUi4;
                case VarEnum.VT_UI8: return AsUi8;
                case VarEnum.VT_INT: return AsInt;
                case VarEnum.VT_UINT: return AsUint;
                case VarEnum.VT_BOOL: return AsBool;
                case VarEnum.VT_R4: return AsR4;
                case VarEnum.VT_R8: return AsR8;
                case VarEnum.VT_DECIMAL: return AsDecimal;
                case VarEnum.VT_DATE: return AsDate;
                case VarEnum.VT_BSTR: return AsBstr;
                case VarEnum.VT_UNKNOWN: return AsUnknown;
                case VarEnum.VT_DISPATCH: return AsDispatch;

                // *** END GENERATED CODE ***

                #endregion

                default:
                    throw new NotImplementedException("Variant.ToObject cannot handle" + VariantType);
            }
        }

        /// <summary>
        /// Release any unmanaged memory associated with the Variant
        /// </summary>
        /// <returns></returns>
        public void Clear() {
            switch(VariantType) {
                case VarEnum.VT_BSTR:
                case VarEnum.VT_UNKNOWN:
                case VarEnum.VT_DISPATCH:
                    IntPtr variantPtr = ComRuntimeHelpers.UnsafeMethods.ConvertByrefToPtr(out this);
                    ComRuntimeHelpers.UnsafeNativeMethods.VariantClear(variantPtr);
                    Debug.Assert(IsEmpty);
                    break;
            }
        }

        public VarEnum VariantType { 
            get { 
                return (VarEnum)_typeUnion._vt; 
            }
            set {
                _typeUnion._vt = (ushort)value;
            }
        }

        internal bool IsEmpty { 
            get { 
                return _typeUnion._vt == ((ushort)VarEnum.VT_EMPTY); 
            }
        }

        public void SetAsNULL() {
            Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
            VariantType = VarEnum.VT_NULL;
        }

        #region Generated Variant accessors

        // *** BEGIN GENERATED CODE ***

        [CLSCompliant(false)]
        public SByte AsI1 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_I1);
                return _typeUnion._unionTypes._i1;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_I1;
                _typeUnion._unionTypes._i1 = value;
            }
        }

        public Int16 AsI2 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_I2);
                return _typeUnion._unionTypes._i2;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_I2;
                _typeUnion._unionTypes._i2 = value;
            }
        }

        public Int32 AsI4 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_I4);
                return _typeUnion._unionTypes._i4;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_I4;
                _typeUnion._unionTypes._i4 = value;
            }
        }

        public Int64 AsI8 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_I8);
                return _typeUnion._unionTypes._i8;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_I8;
                _typeUnion._unionTypes._i8 = value;
            }
        }

        [CLSCompliant(false)]
        public Byte AsUi1 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_UI1);
                return _typeUnion._unionTypes._ui1;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_UI1;
                _typeUnion._unionTypes._ui1 = value;
            }
        }

        [CLSCompliant(false)]
        public UInt16 AsUi2 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_UI2);
                return _typeUnion._unionTypes._ui2;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_UI2;
                _typeUnion._unionTypes._ui2 = value;
            }
        }

        [CLSCompliant(false)]
        public UInt32 AsUi4 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_UI4);
                return _typeUnion._unionTypes._ui4;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_UI4;
                _typeUnion._unionTypes._ui4 = value;
            }
        }

        [CLSCompliant(false)]
        public UInt64 AsUi8 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_UI8);
                return _typeUnion._unionTypes._ui8;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_UI8;
                _typeUnion._unionTypes._ui8 = value;
            }
        }

        public IntPtr AsInt {
            get {
                Debug.Assert(VariantType == VarEnum.VT_INT);
                return _typeUnion._unionTypes._int;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_INT;
                _typeUnion._unionTypes._int = value;
            }
        }

        [CLSCompliant(false)]
        public UIntPtr AsUint {
            get {
                Debug.Assert(VariantType == VarEnum.VT_UINT);
                return _typeUnion._unionTypes._uint;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_UINT;
                _typeUnion._unionTypes._uint = value;
            }
        }

        public bool AsBool {
            get {
                Debug.Assert(VariantType == VarEnum.VT_BOOL);
                return _typeUnion._unionTypes._bool != 0;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_BOOL;
                _typeUnion._unionTypes._bool = value ? -1 : 0;
            }
        }

        public Single AsR4 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_R4);
                return _typeUnion._unionTypes._r4;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_R4;
                _typeUnion._unionTypes._r4 = value;
            }
        }

        public Double AsR8 {
            get {
                Debug.Assert(VariantType == VarEnum.VT_R8);
                return _typeUnion._unionTypes._r8;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_R8;
                _typeUnion._unionTypes._r8 = value;
            }
        }

        public Decimal AsDecimal {
            get {
                Debug.Assert(VariantType == VarEnum.VT_DECIMAL);
                // The first byte of Decimal is unused, but usually set to 0
                Variant v = this;
                v._typeUnion._vt = 0;
                return v._decimal;
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_DECIMAL;
                _decimal = value;
                // _vt overlaps with _decimal, and should be set after setting _decimal
                _typeUnion._vt = (ushort)VarEnum.VT_DECIMAL;
            }
        }

        public DateTime AsDate {
            get {
                Debug.Assert(VariantType == VarEnum.VT_DATE);
                return DateTime.FromOADate(_typeUnion._unionTypes._date);
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_DATE;
                _typeUnion._unionTypes._date = value.ToOADate();
            }
        }

        public String AsBstr {
            get {
                Debug.Assert(VariantType == VarEnum.VT_BSTR);
                return (string)Marshal.GetObjectForNativeVariant(ComRuntimeHelpers.UnsafeMethods.ConvertByrefToPtr(out this));
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_BSTR;
                Marshal.GetNativeVariantForObject(value, ComRuntimeHelpers.UnsafeMethods.ConvertByrefToPtr(out this));
            }
        }

        public Object AsUnknown {
            get {
                Debug.Assert(VariantType == VarEnum.VT_UNKNOWN);
                return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._unknown);
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_UNKNOWN;
                _typeUnion._unionTypes._unknown = Marshal.GetIUnknownForObject(value);
            }
        }

        public Object AsDispatch {
            get {
                Debug.Assert(VariantType == VarEnum.VT_DISPATCH);
                return Marshal.GetObjectForIUnknown(_typeUnion._unionTypes._dispatch);
            }
            set {
                Debug.Assert(IsEmpty); // The setter can only be called once as VariantClear might be needed otherwise
                VariantType = VarEnum.VT_DISPATCH;
                _typeUnion._unionTypes._dispatch = Marshal.GetIDispatchForObject(value);
            }
        }


        // *** END GENERATED CODE ***

        #endregion

        internal static System.Reflection.PropertyInfo GetAccessor(VarEnum varType) {
            switch(varType) {

                #region Generated Variant accessors PropertyInfos

                // *** BEGIN GENERATED CODE ***

                case VarEnum.VT_I1: return typeof(Variant).GetProperty("AsI1");
                case VarEnum.VT_I2: return typeof(Variant).GetProperty("AsI2");
                case VarEnum.VT_I4: return typeof(Variant).GetProperty("AsI4");
                case VarEnum.VT_I8: return typeof(Variant).GetProperty("AsI8");
                case VarEnum.VT_UI1: return typeof(Variant).GetProperty("AsUi1");
                case VarEnum.VT_UI2: return typeof(Variant).GetProperty("AsUi2");
                case VarEnum.VT_UI4: return typeof(Variant).GetProperty("AsUi4");
                case VarEnum.VT_UI8: return typeof(Variant).GetProperty("AsUi8");
                case VarEnum.VT_INT: return typeof(Variant).GetProperty("AsInt");
                case VarEnum.VT_UINT: return typeof(Variant).GetProperty("AsUint");
                case VarEnum.VT_BOOL: return typeof(Variant).GetProperty("AsBool");
                case VarEnum.VT_R4: return typeof(Variant).GetProperty("AsR4");
                case VarEnum.VT_R8: return typeof(Variant).GetProperty("AsR8");
                case VarEnum.VT_DECIMAL: return typeof(Variant).GetProperty("AsDecimal");
                case VarEnum.VT_DATE: return typeof(Variant).GetProperty("AsDate");
                case VarEnum.VT_BSTR: return typeof(Variant).GetProperty("AsBstr");
                case VarEnum.VT_UNKNOWN: return typeof(Variant).GetProperty("AsUnknown");
                case VarEnum.VT_DISPATCH: return typeof(Variant).GetProperty("AsDispatch");

                // *** END GENERATED CODE ***

                #endregion

                default:
                    throw new NotImplementedException("Variant.GetAccessor cannot handle" + varType);
            }
        }
    }

    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantArray {
        public Variant _element0;
        public Variant _element1;
        public Variant _element2;
        public Variant _element3;
        public Variant _element4;
        public Variant _element5;
        public Variant _element6;
        public Variant _element7;

        internal const int NumberOfElements = 8;

        # region FxCop-required APIs

        public override bool Equals(object obj) {
            if ((obj == null) || (!(obj is VariantArray))) {
                return false;
            }

            VariantArray other = (VariantArray)obj;
            return _element0 == other._element0 &&
                _element1 == other._element1 &&
                _element2 == other._element2 &&
                _element3 == other._element3 &&
                _element4 == other._element4 &&
                _element5 == other._element5 &&
                _element6 == other._element6 &&
                _element7 == other._element7;
        }

        public override int GetHashCode() {
            return _element0.GetHashCode() ^ _element1.GetHashCode() ^ _element2.GetHashCode() ^ _element3.GetHashCode() ^
                   _element4.GetHashCode() ^ _element5.GetHashCode() ^ _element6.GetHashCode() ^ _element7.GetHashCode();
        }

        public static bool operator ==(VariantArray a, VariantArray b) {
            return a.Equals(b);
        }
        public static bool operator !=(VariantArray a, VariantArray b) {
            return !a.Equals(b);
        }

        #endregion

        internal static System.Reflection.FieldInfo GetField(int index) {
            Debug.Assert(index < NumberOfElements);
            return typeof(VariantArray).GetField("_element" + index);
        }
    }

}

#endif