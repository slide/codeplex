/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace TestAst.Runtime {

    public static partial class Converter {
        #region Conversion Sites

        private static readonly CallSite<Func<CallSite, object, int>> _intSite = MakeExplicitConvertSite<int>();
        private static readonly CallSite<Func<CallSite, object, double>> _doubleSite = MakeExplicitConvertSite<double>();
        private static readonly CallSite<Func<CallSite, object, Complex64>> _complexSite = MakeExplicitConvertSite<Complex64>();
        private static readonly CallSite<Func<CallSite, object, BigInteger>> _bigIntSite = MakeExplicitConvertSite<BigInteger>();
        private static readonly CallSite<Func<CallSite, object, string>> _stringSite = MakeExplicitConvertSite<string>();
        private static readonly CallSite<Func<CallSite, object, bool>> _boolSite = MakeExplicitConvertSite<bool>();
        private static readonly CallSite<Func<CallSite, object, char>> _charSite = MakeImplicitConvertSite<char>();
        private static readonly CallSite<Func<CallSite, object, char>> _explicitCharSite = MakeExplicitConvertSite<char>();
        private static readonly CallSite<Func<CallSite, object, IEnumerable>> _ienumerableSite = MakeImplicitConvertSite<IEnumerable>();
        private static readonly CallSite<Func<CallSite, object, IEnumerator>> _ienumeratorSite = MakeImplicitConvertSite<IEnumerator>();
        private static readonly Dictionary<Type, CallSite<Func<CallSite, object, object>>> _siteDict = new Dictionary<Type, CallSite<Func<CallSite, object, object>>>();
        private static readonly CallSite<Func<CallSite, object, byte>> _byteSite = MakeExplicitConvertSite<byte>();
        private static readonly CallSite<Func<CallSite, object, sbyte>> _sbyteSite = MakeExplicitConvertSite<sbyte>();
        private static readonly CallSite<Func<CallSite, object, Int16>> _int16Site = MakeExplicitConvertSite<Int16>();
        private static readonly CallSite<Func<CallSite, object, UInt16>> _uint16Site = MakeExplicitConvertSite<UInt16>();
        private static readonly CallSite<Func<CallSite, object, UInt32>> _uint32Site = MakeExplicitConvertSite<UInt32>();
        private static readonly CallSite<Func<CallSite, object, Int64>> _int64Site = MakeExplicitConvertSite<Int64>();
        private static readonly CallSite<Func<CallSite, object, UInt64>> _uint64Site = MakeExplicitConvertSite<UInt64>();
        private static readonly CallSite<Func<CallSite, object, decimal>> _decimalSite = MakeExplicitConvertSite<decimal>();
        private static readonly CallSite<Func<CallSite, object, float>> _floatSite = MakeExplicitConvertSite<float>();

        private static readonly CallSite<Func<CallSite, object, object>> 
            _tryByteSite       = MakeExplicitTrySite<Byte>(),
            _trySByteSite      = MakeExplicitTrySite<SByte>(),
            _tryInt16Site      = MakeExplicitTrySite<Int16>(),
            _tryInt32Site      = MakeExplicitTrySite<Int32>(),
            _tryInt64Site      = MakeExplicitTrySite<Int64>(),
            _tryUInt16Site     = MakeExplicitTrySite<UInt16>(),
            _tryUInt32Site     = MakeExplicitTrySite<UInt32>(),
            _tryUInt64Site     = MakeExplicitTrySite<UInt64>(),
            _tryDoubleSite     = MakeExplicitTrySite<Double>(),
            _trySingleSite     = MakeExplicitTrySite<Single>(),
            _tryCharSite       = MakeExplicitTrySite<Char>(),
            _tryBigIntegerSite = MakeExplicitTrySite<BigInteger>(),
            _tryComplex64Site  = MakeExplicitTrySite<Complex64>(),
            _tryStringSite     = MakeExplicitTrySite<String>();

        private static CallSite<Func<CallSite, object, T>> MakeImplicitConvertSite<T>() {
            return MakeConvertSite<T>(ConversionResultKind.ImplicitCast);
        }

        private static CallSite<Func<CallSite, object, T>> MakeExplicitConvertSite<T>() {
            return MakeConvertSite<T>(ConversionResultKind.ExplicitCast);
        }

        private static CallSite<Func<CallSite, object, T>> MakeConvertSite<T>(ConversionResultKind kind) {
            return CallSite<Func<CallSite, object, T>>.Create(new ConversionBinder(typeof(T), kind));
        }

        private static CallSite<Func<CallSite, object, object>> MakeImplicitTrySite<T>() {
            return MakeTrySite<T>(ConversionResultKind.ImplicitTry);
        }

        private static CallSite<Func<CallSite, object, object>> MakeExplicitTrySite<T>() {
            return MakeTrySite<T>(ConversionResultKind.ExplicitTry);
        }

        private static CallSite<Func<CallSite, object, object>> MakeTrySite<T>(ConversionResultKind kind) {
            return CallSite<Func<CallSite, object, object>>.Create(new ConversionBinder(typeof(T), kind));
        }

        #endregion

        #region Conversion entry points

        public static Int32 ConvertToInt32(object value) { return _intSite.Target(_intSite, value); }
        public static String ConvertToString(object value) { return _stringSite.Target(_stringSite, value); }
        public static BigInteger ConvertToBigInteger(object value) { return _bigIntSite.Target(_bigIntSite, value); }
        public static Double ConvertToDouble(object value) { return _doubleSite.Target(_doubleSite, value); }
        public static Complex64 ConvertToComplex64(object value) { return _complexSite.Target(_complexSite, value); }
        public static Boolean ConvertToBoolean(object value) { return _boolSite.Target(_boolSite, value); }
        public static Int64 ConvertToInt64(object value) { return _int64Site.Target(_int64Site, value); }

        public static Byte ConvertToByte(object value) { return _byteSite.Target(_byteSite, value); }
        public static SByte ConvertToSByte(object value) { return _sbyteSite.Target(_sbyteSite, value); }
        public static Int16 ConvertToInt16(object value) { return _int16Site.Target(_int16Site, value); }
        public static UInt16 ConvertToUInt16(object value) { return _uint16Site.Target(_uint16Site, value); }
        public static UInt32 ConvertToUInt32(object value) { return _uint32Site.Target(_uint32Site, value); }
        public static UInt64 ConvertToUInt64(object value) { return _uint64Site.Target(_uint64Site, value); }
        public static Single ConvertToSingle(object value) { return _floatSite.Target(_floatSite, value); }
        public static Decimal ConvertToDecimal(object value) { return _decimalSite.Target(_decimalSite, value); }
        public static Char ConvertToChar(object value) { return _charSite.Target(_charSite, value); }

        internal static bool TryConvertToByte(object value, out Byte result) {
            object res = _tryByteSite.Target(_tryByteSite, value);
            if (res != null) {
                result = (Byte)res;
                return true;
            }
            result = default(Byte);
            return false;
        }

        internal static bool TryConvertToSByte(object value, out SByte result) {
            object res = _trySByteSite.Target(_trySByteSite, value);
            if (res != null) {
                result = (SByte)res;
                return true;
            }
            result = default(SByte);
            return false;
        }

        internal static bool TryConvertToInt16(object value, out Int16 result) {
            object res = _tryInt16Site.Target(_tryInt16Site, value);
            if (res != null) {
                result = (Int16)res;
                return true;
            }
            result = default(Int16);
            return false;
        }

        internal static bool TryConvertToInt32(object value, out Int32 result) {
            object res = _tryInt32Site.Target(_tryInt32Site, value);
            if (res != null) {
                result = (Int32)res;
                return true;
            }
            result = default(Int32);
            return false;
        }

        internal static bool TryConvertToInt64(object value, out Int64 result) {
            object res = _tryInt64Site.Target(_tryInt64Site, value);
            if (res != null) {
                result = (Int64)res;
                return true;
            }
            result = default(Int64);
            return false;
        }

        internal static bool TryConvertToUInt16(object value, out UInt16 result) {
            object res = _tryUInt16Site.Target(_tryUInt16Site, value);
            if (res != null) {
                result = (UInt16)res;
                return true;
            }
            result = default(UInt16);
            return false;
        }

        internal static bool TryConvertToUInt32(object value, out UInt32 result) {
            object res = _tryUInt32Site.Target(_tryUInt32Site, value);
            if (res != null) {
                result = (UInt32)res;
                return true;
            }
            result = default(UInt32);
            return false;
        }

        internal static bool TryConvertToUInt64(object value, out UInt64 result) {
            object res = _tryUInt64Site.Target(_tryUInt64Site, value);
            if (res != null) {
                result = (UInt64)res;
                return true;
            }
            result = default(UInt64);
            return false;
        }

        internal static bool TryConvertToDouble(object value, out Double result) {
            object res = _tryDoubleSite.Target(_tryDoubleSite, value);
            if (res != null) {
                result = (Double)res;
                return true;
            }
            result = default(Double);
            return false;
        }

        internal static bool TryConvertToBigInteger(object value, out BigInteger result) {
            object res = _tryBigIntegerSite.Target(_tryBigIntegerSite, value);
            if (res != null) {
                result = (BigInteger)res;
                return true;
            }
            result = default(BigInteger);
            return false;
        }

        internal static bool TryConvertToComplex64(object value, out Complex64 result) {
            object res = _tryComplex64Site.Target(_tryComplex64Site, value);
            if (res != null) {
                result = (Complex64)res;
                return true;
            }
            result = default(Complex64);
            return false;
        }

        internal static bool TryConvertToString(object value, out String result) {
            object res = _tryStringSite.Target(_tryStringSite, value);
            if (res != null) {
                result = (String)res;
                return true;
            }
            result = default(String);
            return false;
        }

        internal static bool TryConvertToChar(object value, out Char result) {
            object res = _tryCharSite.Target(_tryCharSite, value);
            if (res != null) {
                result = (Char)res;
                return true;
            }
            result = default(Char);
            return false;
        }

        #endregion

        internal static Char ExplicitConvertToChar(object value) {
            return _explicitCharSite.Target(_explicitCharSite, value);
        }

        public static T Convert<T>(object value) {
            return (T)Convert(value, typeof(T));
        }
        
        /// <summary>
        /// General conversion routine TryConvert - tries to convert the object to the desired type.
        /// Try to avoid using this method, the goal is to ultimately remove it!
        /// </summary>
        internal static bool TryConvert(object value, Type to, out object result) {
            try {
                result = Convert(value, to);
                return true;
            } catch {
                result = default(object);
                return false;
            }
        }

        public static object Convert(object value, Type to) {
            CallSite<Func<CallSite, object, object>> site;
            lock (_siteDict) {
                if (!_siteDict.TryGetValue(to, out site)) {
                    _siteDict[to] = site = CallSite<Func<CallSite, object, object>>.Create(
                        new ConversionBinder(to, ConversionResultKind.ExplicitCast)
                    );
                }
            }

            object res = site.Target(site, value);
            if (to.IsValueType && res == null && 
                (!to.IsGenericType || to.GetGenericTypeDefinition() != typeof(Nullable<>))) {
                throw MakeTypeError(to, value);
            }
            return res;
        }        

        /// <summary>
        /// This function tries to convert an object to IEnumerator, or wraps it into an adapter
        /// Do not use this function directly. It is only meant to be used by Ops.GetEnumerator.
        /// </summary>
        internal static bool TryConvertToIEnumerator(object o, out IEnumerator e) {
            try {
                e = _ienumeratorSite.Target(_ienumeratorSite, o);
                return e != null;
            } catch {
                e = null;
                return false;
            }            
        }

        /// <summary>
        /// This function tries to convert an object to IEnumerator, or wraps it into an adapter
        /// Do not use this function directly. It is only meant to be used by Ops.GetEnumerator.
        /// </summary>
        internal static IEnumerator ConvertToIEnumerator(object o) {
            return _ienumeratorSite.Target(_ienumeratorSite, o);
        }

        public static IEnumerable ConvertToIEnumerable(object o) {
            return _ienumerableSite.Target(_ienumerableSite, o);
        }
        
        private static Exception MakeTypeError(Type expectedType, object o) {
            return MakeTypeError(expectedType.Name.ToString(), o);
        }

        private static Exception MakeTypeError(string expectedType, object o) {
            return TypeErrorForTypeMismatch(expectedType, o);
        }
        
        private static Exception TypeErrorForTypeMismatch(string expectedTypeName, object instance) {
            return new ArgumentTypeException(string.Format("expected {0}, got {1}", expectedTypeName, (instance ?? new object()).GetType().Name));
        }

        private static Exception InvalidType(object o, RuntimeTypeHandle handle) {
            return TypeErrorForTypeMismatch(Type.GetTypeFromHandle(handle).Name, o);
        }

        #region Cached Type instances

        private static readonly Type Int16Type = typeof(System.Int16);
        private static readonly Type SByteType = typeof(System.SByte);
        private static readonly Type StringType = typeof(System.String);
        private static readonly Type UInt64Type = typeof(System.UInt64);
        private static readonly Type Int32Type = typeof(System.Int32);
        private static readonly Type DoubleType = typeof(System.Double);
        private static readonly Type DecimalType = typeof(System.Decimal);
        private static readonly Type ObjectType = typeof(System.Object);
        private static readonly Type Int64Type = typeof(System.Int64);
        private static readonly Type CharType = typeof(System.Char);
        private static readonly Type SingleType = typeof(System.Single);
        private static readonly Type BooleanType = typeof(System.Boolean);
        private static readonly Type UInt16Type = typeof(System.UInt16);
        private static readonly Type UInt32Type = typeof(System.UInt32);
        private static readonly Type ByteType = typeof(System.Byte);
        private static readonly Type BigIntegerType = typeof(BigInteger);
        private static readonly Type Complex64Type = typeof(Complex64);
        private static readonly Type DelegateType = typeof(Delegate);
        private static readonly Type IEnumerableType = typeof(IEnumerable);
        private static readonly Type ValueTypeType = typeof(ValueType);
        private static readonly Type TypeType = typeof(Type);
#if !SILVERLIGHT
        private static readonly Type ArrayListType = typeof(ArrayList);
        private static readonly Type HashtableType = typeof(Hashtable);
#endif
        private static readonly Type NullableOfTType = typeof(Nullable<>);
        private static readonly Type IListOfTType = typeof(System.Collections.Generic.IList<>);
        private static readonly Type ListOfTType = typeof(System.Collections.Generic.List<>);
        private static readonly Type IDictOfTType = typeof(System.Collections.Generic.IDictionary<,>);
        private static readonly Type IEnumerableOfTType = typeof(System.Collections.Generic.IEnumerable<>);
        private static readonly Type IListOfObjectType = typeof(System.Collections.Generic.IList<object>);
        private static readonly Type IEnumerableOfObjectType = typeof(IEnumerable<object>);
        private static readonly Type IDictionaryOfObjectType = typeof(System.Collections.Generic.IDictionary<object, object>);

        #endregion

        private static object ConvertToNullableT(object value, Type[] typeOf) {
            if (value == null) return null;
            else return Convert(value, typeOf[0]);
        }

        #region Entry points called from the generated code

        public static object ConvertToReferenceType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToNullableType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) return null;
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static object ConvertToValueType(object fromObject, RuntimeTypeHandle typeHandle) {
            if (fromObject == null) throw InvalidType(fromObject, typeHandle);
            return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
        }

        public static Type ConvertToType(object value) {
            if (value == null) return null;

            Type TypeVal = value as Type;
            if (TypeVal != null) return TypeVal;

            TypeGroup typeCollision = value as TypeGroup;
            if (typeCollision != null) {
                Type nonGenericType;
                if (typeCollision.TryGetNonGenericType(out nonGenericType)) {
                    return nonGenericType;
                }
            }

            throw MakeTypeError("Type", value);
        }

        private static DynamicDelegateCreator _delegateCtor = new DynamicDelegateCreator(TestContext._TestContext);
        public static object ConvertToDelegate(object value, Type to) {
            if (value == null) return null;
            return _delegateCtor.GetDelegate(value, to);
        }

        #endregion

        public static bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            ContractUtils.RequiresNotNull(fromType, "fromType");
            ContractUtils.RequiresNotNull(toType, "toType");

            if (toType == fromType) return true;
            if (toType.IsAssignableFrom(fromType)) return true;
            if (fromType.IsCOMObject && toType.IsInterface) return true; // A COM object could be cast to any interface

            if (HasImplicitNumericConversion(fromType, toType)) return true;

            // Handling the hole that Type is the only object that we 'box'
            if (toType == TypeType && typeof(TypeGroup).IsAssignableFrom(fromType)) return true;

            // Support extensible types with simple implicit conversions to their base types
            if (typeof(Extensible<int>).IsAssignableFrom(fromType) && CanConvertFrom(Int32Type, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<BigInteger>).IsAssignableFrom(fromType) && CanConvertFrom(BigIntegerType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<string>).IsAssignableFrom(fromType) && CanConvertFrom(StringType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<double>).IsAssignableFrom(fromType) && CanConvertFrom(DoubleType, toType, allowNarrowing)) {
                return true;
            }
            if (typeof(Extensible<Complex64>).IsAssignableFrom(fromType) && CanConvertFrom(Complex64Type, toType, allowNarrowing)) {
                return true;
            }

#if !SILVERLIGHT
            // try available type conversions...
            object[] tcas = toType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
            foreach (TypeConverterAttribute tca in tcas) {
                TypeConverter tc = GetTypeConverter(tca);

                if (tc == null) continue;

                if (tc.CanConvertFrom(fromType)) {
                    return true;
                }
            }
#endif

            //!!!do user-defined implicit conversions here

            if (allowNarrowing == NarrowingLevel.None) return false;

            return HasNarrowingConversion(fromType, toType, allowNarrowing);
        }

#if !SILVERLIGHT
        private static TypeConverter GetTypeConverter(TypeConverterAttribute tca) {
            try {
                ConstructorInfo ci = Type.GetType(tca.ConverterTypeName).GetConstructor(Type.EmptyTypes);
                if (ci != null) return ci.Invoke(ArrayUtils.EmptyObjects) as TypeConverter;
            } catch (TargetInvocationException) {
            }
            return null;
        }
#endif

        private static bool HasImplicitNumericConversion(Type fromType, Type toType) {
            if (fromType.IsEnum) return false;

            if (fromType == typeof(BigInteger)) {
                if (toType == typeof(double)) return true;
                if (toType == typeof(float)) return true;
                if (toType == typeof(Complex64)) return true;
                return false;
            }

            if (fromType == typeof(bool)) {
                if (toType == typeof(int)) return true;
                return HasImplicitNumericConversion(typeof(int), toType);
            }

            switch (Type.GetTypeCode(fromType)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Byte:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt16:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt32:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.UInt64:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Char:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                        default:
                            if (toType == BigIntegerType) return true;
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(toType)) {
                        case TypeCode.Double:
                            return true;
                        default:
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                case TypeCode.Double:
                    switch (Type.GetTypeCode(toType)) {
                        default:
                            if (toType == Complex64Type) return true;
                            return false;
                    }
                default:
                    return false;
            }
        }

        public static Candidate PreferConvert(Type t1, Type t2) {
            if (t1 == typeof(bool) && t2 == typeof(int)) return Candidate.Two;
            if (t1 == typeof(Decimal) && t2 == typeof(BigInteger)) return Candidate.Two;
            //if (t1 == typeof(int) && t2 == typeof(BigInteger)) return Candidate.Two;

            switch (Type.GetTypeCode(t1)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
            }
            return Candidate.Equivalent;
        }

        private static bool HasNarrowingConversion(Type fromType, Type toType, NarrowingLevel allowNarrowing) {
            if (allowNarrowing == NarrowingLevel.Three) {
                if (toType == CharType && fromType == StringType) return true;
                if (toType == StringType && fromType == CharType) return true;
                //if (toType == Int32Type && fromType == BigIntegerType) return true;
                //if (IsIntegral(fromType) && IsIntegral(toType)) return true;

                //Check if there is an implicit convertor defined on fromType to toType
                if (HasImplicitConversion(fromType, toType)) {
                    return true;
                }
            }

            if (toType == DoubleType && fromType == DecimalType) return true;
            if (toType == SingleType && fromType == DecimalType) return true;

            if (toType.IsArray) {
                return false;
            }

            if (allowNarrowing == NarrowingLevel.Three) {
                if (IsNumeric(fromType) && IsNumeric(toType)) {
                    if (fromType != typeof(float) && fromType != typeof(double) && fromType != typeof(decimal) && fromType != typeof(Complex64)) {
                        return true;
                    }
                }
                if (fromType == typeof(bool) && IsNumeric(toType)) return true;

                if (toType == CharType && fromType == StringType) return true;
                if (toType == Int32Type && fromType == BooleanType) return true;

                // Everything can convert to Boolean in Python
                if (toType == BooleanType) return true;

                if (toType.IsGenericType) {
                    Type genTo = toType.GetGenericTypeDefinition();
                    if (genTo == IEnumerableOfTType) {
                        return IEnumerableOfObjectType.IsAssignableFrom(fromType) ||
                            IEnumerableType.IsAssignableFrom(fromType);
                    }
                }
            }

            if (allowNarrowing == NarrowingLevel.All) {
                //__int__, __float__, __long__
                if (IsNumeric(fromType) && IsNumeric(toType)) return true;
            }

            if (toType.IsGenericType) {
                Type genTo = toType.GetGenericTypeDefinition();
                if (genTo == IListOfTType) {
                    return IListOfObjectType.IsAssignableFrom(fromType);
                } else if (genTo == NullableOfTType) {
                    if (fromType == typeof(DynamicNull) || CanConvertFrom(fromType, toType.GetGenericArguments()[0], allowNarrowing)) {
                        return true;
                    }                
                } else if (genTo == IDictOfTType) {
                    return IDictionaryOfObjectType.IsAssignableFrom(fromType);
                }
            }

            if (fromType == BigIntegerType && toType == Int64Type) return true;

            return false;
        }

        private static bool HasImplicitConversion(Type fromType, Type toType) {
            return 
                HasImplicitConversionWorker(fromType, fromType, toType) ||
                HasImplicitConversionWorker(toType, fromType, toType);
        }

        private static bool HasImplicitConversionWorker(Type lookupType, Type fromType, Type toType) {
            while (lookupType != null) {
                foreach (MethodInfo method in lookupType.GetMethods()) {
                    if (method.Name == "op_Implicit" &&
                        method.GetParameters()[0].ParameterType.IsAssignableFrom(fromType) &&
                        toType.IsAssignableFrom(method.ReturnType)) {
                        return true;
                    }
                }
                lookupType = lookupType.BaseType;
            }
            return false;
        }

        private static bool IsIntegral(Type t) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.Single:
                case TypeCode.Double:
                    return false;
                case TypeCode.Object:
                    return t == BigIntegerType;
                default:
                    return true;
            }
        }

        internal static bool IsNumeric(Type t) {
            if (t.IsEnum) return false;

            switch (Type.GetTypeCode(t)) {
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Char:
                case TypeCode.Empty:
                case TypeCode.String:
                case TypeCode.Boolean:
                    return false;
                case TypeCode.Object:
                    return t == BigIntegerType || t == Complex64Type;
                default:
                    return true;
            }
        }
    }
}