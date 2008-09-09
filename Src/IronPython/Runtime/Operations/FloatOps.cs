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
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Math;
using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Operations {

    public static partial class DoubleOps {
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls) {
            if (cls == TypeCache.Double) return 0.0;

            return cls.CreateInstance(context);
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, object x) {
            if (cls == TypeCache.Double) {
                if (x is string) {
                    return ParseFloat((string)x);
                } else if (x is Extensible<string>) {
                    return ParseFloat(((Extensible<string>)x).Value);
                } else if (x is char) {
                    return ParseFloat(RuntimeHelpers.CharToString((char)x));
                }

                double doubleVal;
                if (Converter.TryConvertToDouble(x, out doubleVal)) return doubleVal;

                if (x is Complex64) throw PythonOps.TypeError("can't convert complex to float; use abs(z)");

                object d = PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, x, Symbols.ConvertToFloat));
                if (d is double) return d;
                throw PythonOps.TypeError("__float__ returned non-float (type %s)", DynamicHelpers.GetPythonType(d));
            } else {
                return cls.CreateInstance(context, x);
            }
        }

        private static object ParseFloat(string x) {
            try {
                return LiteralParser.ParseFloat(x);
            } catch (FormatException) {
                throw PythonOps.ValueError("invalid literal for float(): {0}", x);
            }
        }


        #region Binary operators

        [SpecialName]
        [return: MaybeNotImplemented]
        public static object DivMod(double x, double y) {
            object div = FloorDivide(x, y);
            if (div == NotImplementedType.Value) return div;
            return PythonTuple.MakeTuple(div, Mod(x, y));
        }

        [SpecialName]
        public static double Mod(double x, double y) {
            if (y == 0) throw PythonOps.ZeroDivisionError();

            double r = x % y;
            if (r > 0 && y < 0) {
                r = r + y;
            } else if (r < 0 && y > 0) {
                r = r + y;
            }
            return r;
        }

        [SpecialName]
        public static double Power(double x, double y) {
            if (x == 0.0 && y < 0.0)
                throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
            if (x < 0 && (Math.Floor(y) != y)) {
                throw PythonOps.ValueError("negative number cannot be raised to fraction");
            }
            double result = Math.Pow(x, y);
            if (double.IsInfinity(result)) {
                throw PythonOps.OverflowError("result too large");
            }
            return result;
        }
        #endregion

        public static PythonTuple __coerce__(CodeContext context, double x, object o) {
            // called via builtin.coerce()
            double d = (double)__new__(context, TypeCache.Double, o);

            if (Double.IsInfinity(d)) {
                throw PythonOps.OverflowError("number too big");
            }

            return PythonTuple.MakeTuple(x, d);
        }
        
        #region Unary operators

        public static object __int__(double d) {
            if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                return (int)d;
            } else if (Int64.MinValue <= d && d <= Int64.MaxValue) {
                return (long)d;
            } else {
                return BigInteger.Create(d);
            }
        }

        public static object __getnewargs__(CodeContext context, double self) {
            return PythonTuple.MakeTuple(DoubleOps.__new__(context, TypeCache.Double, self));
        }

        #endregion

        #region ToString

        public static string __str__(CodeContext/*!*/ context, double x) {
            StringFormatter sf = new StringFormatter(context, "%.12g", x);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        public static string __str__(double x, IFormatProvider provider) {
            return x.ToString(provider);
        }

        public static string __str__(double x, string format) {
            return x.ToString(format);
        }

        public static string __str__(double x, string format, IFormatProvider provider) {
            return x.ToString(format, provider);
        }

        public static int __hash__(double d) {
            // Python allows equality between floats, ints, and big ints.
            if ((d % 1) == 0) {
                // This double represents an integer number, so it must hash like an integer number.
                if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                    return ((int)d).GetHashCode();
                }
                // Big integer
                BigInteger b = BigInteger.Create(d);
                return BigIntegerOps.__hash__(b);
            }
            return d.GetHashCode();
        }

        #endregion

        [SpecialName]
        public static bool LessThan(double x, double y) {
            if (Double.IsInfinity(x) && Double.IsNaN(y)) {
                return false;
            } else if (Double.IsNaN(x) && Double.IsInfinity(y)) {
                return false;
            }

            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(double x, double y) {
            if (x == y) {
                return x != Double.NaN;
            }

            return x < y;
        }

        [SpecialName]
        public static bool GreaterThan(double x, double y) {
            if (Double.IsInfinity(x) && Double.IsNaN(y)) {
                return false;
            } else if (Double.IsNaN(x) && Double.IsInfinity(y)) {
                return false;
            }

            return x > y;
        }

        [SpecialName]
        public static bool GreaterThanOrEqual(double x, double y) {
            if (x == y) {
                return x != Double.NaN;
            }

            return x > y;
        }

        [SpecialName]
        public static bool Equals(double x, double y) {
            if (x == y) {
                return x != Double.NaN;
            }
            return x == y;
        }

        [SpecialName]
        public static bool NotEquals(double x, double y) {
            return !Equals(x, y);
        }

        [SpecialName]
        public static bool LessThan(double x, BigInteger y) {
            return Compare(x, y) < 0;
        }
        [SpecialName]
        public static bool LessThanOrEqual(double x, BigInteger y) {
            return Compare(x, y) <= 0;
        }
        [SpecialName]
        public static bool GreaterThan(double x, BigInteger y) {
            return Compare(x, y) > 0;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(double x, BigInteger y) {
            return Compare(x, y) >= 0;
        }
        [SpecialName]
        public static bool Equals(double x, BigInteger y) {
            return Compare(x, y) == 0;
        }
        [SpecialName]
        public static bool NotEquals(double x, BigInteger y) {
            return Compare(x, y) != 0;
        }

        internal static int Compare(double x, BigInteger y) {
            return -Compare(y, x);
        }

        internal static int Compare(BigInteger x, double y) {
            if (y == Double.PositiveInfinity) {
                return -1;
            } else if (y == Double.NegativeInfinity) {
                return 1;
            }

            // BigInts can hold doubles, but doubles can't hold BigInts, so
            // if we're comparing against a BigInt then we should convert ourself
            // to a long and then compare.
            if (object.ReferenceEquals(x,null)) return -1;
            BigInteger by = BigInteger.Create(y);
            if (by == x) {
                double mod = y % 1;
                if (mod == 0) return 0;
                if (mod > 0) return -1;
                return +1;
            }
            if (by > x) return -1;
            return +1;
        }

        [SpecialName]
        public static bool LessThan(double x, decimal y) {
            return Compare(x, y) < 0;
        }
        [SpecialName]
        public static bool LessThanOrEqual(double x, decimal y) {
            return Compare(x, y) <= 0;
        }
        [SpecialName]
        public static bool GreaterThan(double x, decimal y) {
            return Compare(x, y) > 0;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(double x, decimal y) {
            return Compare(x, y) >= 0;
        }
        [SpecialName]
        public static bool Equals(double x, decimal y) {
            return Compare(x, y) == 0;
        }
        [SpecialName]
        public static bool NotEquals(double x, decimal y) {
            return Compare(x, y) != 0;
        }


        internal static int Compare(double x, decimal y) {
            if (x > (double)decimal.MaxValue) return +1;
            if (x < (double)decimal.MinValue) return -1;
            return ((decimal)x).CompareTo(y);
        }

        [SpecialName]
        public static bool LessThan(Double x, int y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Double x, int y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Double x, int y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Double x, int y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Double x, int y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Double x, int y) {
            return x != y;
        }

        public static string __repr__(CodeContext/*!*/ context, double self) {
            StringFormatter sf = new StringFormatter(context, "%.17g", self);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        public static BigInteger/*!*/ __long__(double self) {
            return BigInteger.Create(self);
        }

        public static double __float__(double self) {
            return self;
        }

        public static string __getformat__(CodeContext/*!*/ context, string typestr) {
            switch (typestr) {
                case "float":
                    return PythonContext.GetContext(context).FloatFormat ?? DefaultFloatFormat();
                case "double":
                    return PythonContext.GetContext(context).DoubleFormat ?? DefaultFloatFormat();
                default: throw PythonOps.ValueError("__getformat__() argument 1 must be 'double' or 'float'");
            }
        }

        private static string DefaultFloatFormat() {
            if (BitConverter.IsLittleEndian) {
                return "IEEE, little-endian";
            }

            return "IEEE, big-endian";
        }

        public static void __setformat__(CodeContext/*!*/ context, string typestr, string fmt) {            
            switch (fmt) {
                case "unknown":
                    break;
                case "IEEE, little-endian":
                    if (!BitConverter.IsLittleEndian) {
                        throw PythonOps.ValueError("can only set double format to 'unknown' or the detected platform value");
                    }
                    break;
                case "IEEE, big-endian":
                    if (BitConverter.IsLittleEndian) {
                        throw PythonOps.ValueError("can only set double format to 'unknown' or the detected platform value");
                    }
                    break;
                default:
                    throw PythonOps.ValueError(" __setformat__() argument 2 must be 'unknown', 'IEEE, little-endian' or 'IEEE, big-endian'");
            }

            switch (typestr) {
                case "float":
                    PythonContext.GetContext(context).FloatFormat = fmt;
                    break;
                case "double":
                    PythonContext.GetContext(context).DoubleFormat = fmt;
                    break;
                default: 
                    throw PythonOps.ValueError("__setformat__() argument 1 must be 'double' or 'float'");
            }            
        }
    }

    public partial class SingleOps {
        [SpecialName]
        public static bool LessThan(float x, float y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(float x, float y) {
            if (x == y) {
                return x != Single.NaN;
            }

            return x < y;
        }

        [SpecialName]
        public static bool GreaterThan(float x, float y) {
            return x > y;
        }

        [SpecialName]
        public static bool GreaterThanOrEqual(float x, float y) {
            if (x == y) {
                return x != Single.NaN;
            }

            return x > y;
        }

        [SpecialName]
        public static bool Equals(float x, float y) {
            if (x == y) {
                return x != Single.NaN;
            }
            return x == y;
        }

        [SpecialName]
        public static bool NotEquals(float x, float y) {
            return !Equals(x, y);
        }

        [SpecialName]
        public static float Mod(float x, float y) {
            return (float)DoubleOps.Mod(x, y);
        }

        [SpecialName]
        public static float Power(float x, float y) {
            return (float)DoubleOps.Power(x, y);
        }

        public static string __str__(CodeContext/*!*/ context, float x) {
            // Python does not natively support System.Single. However, we try to provide
            // formatting consistent with System.Double.
            StringFormatter sf = new StringFormatter(context, "%.6g", x);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        public static string __repr__(CodeContext/*!*/ context, float self) {
            return __str__(context, self);
        }

        public static int __hash__(float x) {
            return DoubleOps.__hash__(((double)x));
        }
    }
}
