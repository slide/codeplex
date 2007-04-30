/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Internal;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(BigInteger), typeof(BigIntegerOps), EnableDerivation=true)]
namespace IronPython.Runtime.Operations {

    public static partial class BigIntegerOps {
        private static BigInteger DecimalMax = BigInteger.Create(Decimal.MaxValue);
        private static BigInteger DecimalMin = BigInteger.Create(Decimal.MinValue);        

        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls, string s, int radix) {
            if (cls == TypeCache.BigInteger) {
                return ParseBigIntegerSign(s, radix);
            } else {
                BigInteger res = ParseBigIntegerSign(s, radix);
                return cls.CreateInstance(context, res);
            }
        }

        private static BigInteger ParseBigIntegerSign(string s, int radix) {
            try {
                return LiteralParser.ParseBigIntegerSign(s, radix);
            } catch (ArgumentException e) {
                throw Ops.ValueError(e.Message);
            }
        }

        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls, object x) {
            Extensible<BigInteger> el;

            if (cls == TypeCache.BigInteger) {
                if (x is string) return ParseBigIntegerSign((string)x, 10);
                if (x is BigInteger) return (BigInteger)x;
                else if ((el = x as Extensible<BigInteger>) != null) return el.Value;
                else if (x is int) return BigInteger.Create((int)x);
                else if (x is double) return BigInteger.Create((double)x);
                else if (x is long) return BigInteger.Create((long)x);
                else {
                    BigInteger intVal;
                    if (Converter.TryConvertToBigInteger(x, out intVal)) {
                        if (Object.Equals(intVal, null)) throw Ops.TypeError("can't convert {0} to long", Ops.GetDynamicType(x).Name);
                        return intVal;
                    }
                }
            } else {
                BigInteger intVal = null;

                if (x is string) intVal = ParseBigIntegerSign((string)x, 10);
                else if (x is BigInteger) intVal = (BigInteger)x;
                else if ((el = x as Extensible<BigInteger>) != null) intVal = el.Value;
                else if (x is int) intVal = (long)(int)x;
                else if (x is double) intVal = BigInteger.Create((double)x);
                else if (x is long) intVal = (long)x;
                else {
                    if (Converter.TryConvertToBigInteger(x, out intVal)) {
                        if (Object.Equals(intVal, null)) throw Ops.TypeError("can't convert {0} to long", Ops.GetDynamicType(x).Name);
                        return intVal;
                    }
                }

                if (!Object.ReferenceEquals(intVal, null)) {
                    return cls.CreateInstance(context, intVal);
                }
            }

            if (x is Complex64) throw Ops.TypeError("can't convert complex to long; use long(abs(z))");

            throw Ops.ValueError("long argument must be convertible to long (string, number, or type that defines __long__, got {0})",
                StringOps.Quote(Ops.GetPythonTypeName(x)));
        }

        [StaticOpsMethod("__new__")]
        public static object Make(CodeContext context, DynamicType cls) {
            if (cls == TypeCache.BigInteger) {
                return BigInteger.Zero;
            } else {
                return cls.CreateInstance(context, BigInteger.Zero);
            }
        }

        #region Binary operators

        [OperatorMethod]
        public static object Power(BigInteger x, object y, object z) {
            if (y is int) {
                return Power(x, (int)y, z);
            } else if (y is long) {
                return Power(x, BigInteger.Create((long)y), z);
            } else if (y is BigInteger) {
                return Power(x, (BigInteger)y, z);
            }
            return Ops.NotImplemented;
        }

        [OperatorMethod]
        public static object Power(BigInteger x, int y, object z) {
            if (z is int) {
                return Power(x, y, BigInteger.Create((int)z));
            } else if (z is long) {
                return Power(x, y, BigInteger.Create((long)z));
            } else if (z is BigInteger) {
                return Power(x, y, (BigInteger)z);
            } else if (z == null) {
                return Power(x, y);
            }
            return Ops.NotImplemented;
        }

        [OperatorMethod]
        public static object Power(BigInteger x, BigInteger y, object z) {
            if (z is int) {
                return Power(x, y, BigInteger.Create((int)z));
            } else if (z is long) {
                return Power(x, y, BigInteger.Create((long)z));
            } else if (z is BigInteger) {
                return Power(x, y, (BigInteger)z);
            } else if (z == null) {
                return Power(x, y);
            }
            return Ops.NotImplemented;
        }

        [OperatorMethod]
        public static object Power(BigInteger x, int y, BigInteger z) {
            if (y < 0) {
                throw Ops.TypeError("power", y, "power must be >= 0");
            }
            if (z == BigInteger.Zero) {
                throw Ops.ZeroDivisionError();
            }

            BigInteger result = x.ModPow(y, z);

            if (result >= BigInteger.Zero) {
                if (z < BigInteger.Zero) return result + z;
            } else {
                if (z > BigInteger.Zero) return result + z;
            }
            return result;
        }

        [OperatorMethod]
        public static object Power(BigInteger x, BigInteger y, BigInteger z) {
            if (y < BigInteger.Zero) {
                throw Ops.TypeError("power", y, "power must be >= 0");
            }
            if (z == BigInteger.Zero) {
                throw Ops.ZeroDivisionError();
            }

            BigInteger result = x.ModPow(y, z);

            if (result >= BigInteger.Zero) {
                if (z < BigInteger.Zero) return result + z;
            } else {
                if (z > BigInteger.Zero) return result + z;
            }
            return result;
        }


        [OperatorMethod]
        public static object Power([NotNull]BigInteger x, int y) {
            if (y < 0) {
                return DoubleOps.Power(x.ToFloat64(), y);
            }
            return x.Power(y);
        }

        [OperatorMethod]
        public static object Power([NotNull]BigInteger x, [NotNull]BigInteger y) {
            if (Object.ReferenceEquals(x, null)) throw Ops.TypeError("unsupported operands for __pow__: NoneType and long");
            if (Object.ReferenceEquals(y, null)) throw Ops.TypeError("unsupported operands for __pow__: long and NoneType");

            int yl;
            if (y.AsInt32(out yl)) {
                return Power(x, yl);
            } else {
                if (x == BigInteger.Zero) {
                    if (y.IsNegative())
                        throw Ops.ZeroDivisionError("0.0 cannot be raised to a negative power");
                    return BigInteger.Zero;
                } else if (x == BigInteger.One) {
                    return BigInteger.One;
                } else {
                    throw Ops.ValueError("Number too big");
                }
            }
        }

        private static BigInteger DivMod(BigInteger x, BigInteger y, out BigInteger r) {
            BigInteger rr;
            BigInteger qq;

            if (Object.ReferenceEquals(x, null)) throw Ops.TypeError("unsupported operands for div/mod: NoneType and long");
            if (Object.ReferenceEquals(y, null)) throw Ops.TypeError("unsupported operands for div/mod: long and NoneType");

            qq = BigInteger.DivRem(x, y, out rr);

            if (x >= BigInteger.Zero) {
                if (y > BigInteger.Zero) {
                    r = rr;
                    return qq;
                } else {
                    if (rr == BigInteger.Zero) {
                        r = rr;
                        return qq;
                    } else {
                        r = rr + y;
                        return qq - BigInteger.One;
                    }
                }
            } else {
                if (y > BigInteger.Zero) {
                    if (rr == BigInteger.Zero) {
                        r = rr;
                        return qq;
                    } else {
                        r = rr + y;
                        return qq - BigInteger.One;
                    }
                } else {
                    r = rr;
                    return qq;
                }
            }
        }

        [OperatorMethod]
        public static BigInteger Add([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x + y;
        }
        [OperatorMethod]
        public static BigInteger Subtract([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x - y;
        }
        [OperatorMethod]
        public static BigInteger Multiply([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x * y;
        }

        [OperatorMethod]
        public static BigInteger FloorDivide([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return Divide(x, y);
        }

        [OperatorMethod]
        public static double TrueDivide([NotNull]BigInteger x, [NotNull]BigInteger y) {
            if (y == BigInteger.Zero) {
                throw new DivideByZeroException();
            }
            return x.ToFloat64() / y.ToFloat64();
        }

        [OperatorMethod]
        public static BigInteger Divide([NotNull]BigInteger x, [NotNull]BigInteger y) {
            BigInteger r;
            return DivMod(x, y, out r);
        }

        [OperatorMethod]
        public static BigInteger Mod([NotNull]BigInteger x, [NotNull]BigInteger y) {
            BigInteger r;
            DivMod(x, y, out r);
            return r;
        }


        [OperatorMethod]
        public static BigInteger LeftShift([NotNull]BigInteger x, int y) {
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            return x << y;
        }

        [OperatorMethod]
        public static BigInteger RightShift([NotNull]BigInteger x, int y) {
            BigInteger q;
            if (y < 0) {
                throw Ops.ValueError("negative shift count");
            }
            if (x < BigInteger.Zero) {
                q = x >> y;
                BigInteger r = x - (q << y);
                if (r != BigInteger.Zero) q -= BigInteger.One; ;
            } else {
                q = x >> y;
            }
            return q;
        }

        [OperatorMethod]
        public static BigInteger LeftShift([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return LeftShift(x, y.ToInt32());
        }

        [OperatorMethod]
        public static BigInteger RightShift([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return RightShift(x, y.ToInt32());
        }
        #endregion

        internal static object DivMod(BigInteger x, BigInteger y) {
            BigInteger div, mod;
            div = DivMod(x, y, out mod);
            return Tuple.MakeTuple(div, mod);
        }

        #region Unary operators

        [OperatorMethod, PythonName("__abs__")]
        public static object Abs(BigInteger x) {
            return x.Abs();
        }

        [OperatorMethod, PythonName("__nonzero__")]
        public static bool ConvertToBoolean(BigInteger x) {
            return !x.IsZero();
        }

        [OperatorMethod, PythonName("__neg__")]
        public static object Negate(BigInteger x) {
            return -x;
        }

        [OperatorMethod, PythonName("__pos__")]
        public static object Positive(BigInteger x) {
            return x;
        }

        [OperatorMethod, PythonName("__int__")]
        public static int ToInt(BigInteger x) {
            return x.ToInt32();
        }

        [OperatorMethod, PythonName("__float__")]
        public static object ToFloat(BigInteger self) {
            return self.ToFloat64();
        }

        [OperatorMethod, PythonName("__oct__")]
        public static string Oct(BigInteger x) {
            if (x == BigInteger.Zero) {
                return "0L";
            } else if (x > 0) {
                return "0" + x.ToString(8) + "L";
            } else {
                return "-0" + (-x).ToString(8) + "L";
            }
        }

        [OperatorMethod, PythonName("__hex__")]
        public static string Hex(BigInteger x) {
            if (x < 0) {
                return "-0x" + (-x).ToString(16) + "L";
            } else {
                return "0x" + x.ToString(16) + "L";
            }
        }

        [PythonName("__getnewargs__")]
        public static object GetNewArgs(CodeContext context, BigInteger self) {
            if (!Object.ReferenceEquals(self, null)) {
                return Tuple.MakeTuple(BigIntegerOps.Make(context, TypeCache.BigInteger, self));
            }
            throw Ops.TypeErrorForBadInstance("__getnewargs__ requires a 'long' object but received a '{0}'", self);
        }
        #endregion


        // These functions make the code generation of other types more regular

        internal static BigInteger OnesComplement(BigInteger x) {
            return ~x;
        }

        internal static BigInteger FloorDivideImpl(BigInteger x, BigInteger y) {
            return FloorDivide(x, y);
        }
        [OperatorMethod]
        public static BigInteger BitwiseAnd([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x & y;
        }
        [OperatorMethod]
        public static BigInteger BitwiseOr([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x | y;
        }
        [OperatorMethod]
        public static BigInteger ExclusiveOr([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x ^ y;
        }

        // Binary Operations - Comparisons
        [OperatorMethod]
        public static bool LessThan([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x < y;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x <= y;
        }
        [OperatorMethod]
        public static bool GreaterThan([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x > y;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x >= y;
        }
        [OperatorMethod]
        public static bool Equal([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x == y;
        }
        [OperatorMethod]
        public static bool NotEqual([NotNull]BigInteger x, [NotNull]BigInteger y) {
            return x != y;
        }
        [OperatorMethod]
        public static bool Equal([NotNull]BigInteger x, ulong y) {
            return x == y;
        }
        [OperatorMethod]
        public static bool NotEqual([NotNull]BigInteger x, ulong y) {
            return x != y;
        }
        [OperatorMethod]
        public static bool Equal(ulong y, [NotNull]BigInteger x) {
            return x == y;
        }
        [OperatorMethod]
        public static bool NotEqual(ulong y, [NotNull]BigInteger x) {
            return x != y;
        }

        [OperatorMethod]
        public static bool LessThan(BigInteger x, double y) {
            return DoubleOps.Compare(x, y) < 0;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual(BigInteger x, double y) {
            return DoubleOps.Compare(x, y) <= 0;
        }
        [OperatorMethod]
        public static bool GreaterThan(BigInteger x, double y) {
            return DoubleOps.Compare(x, y) > 0;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual(BigInteger x, double y) {
            return DoubleOps.Compare(x, y) >= 0;
        }
        [OperatorMethod]
        public static bool Equal(BigInteger x, double y) {
            return DoubleOps.Compare(x, y) == 0;
        }
        [OperatorMethod]
        public static bool NotEqual(BigInteger x, double y) {
            return DoubleOps.Compare(x, y) != 0;
        }

        [OperatorMethod]
        public static bool LessThan(BigInteger x, decimal y) {
            return DecimalOps.Compare(x, y) < 0;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual(BigInteger x, decimal y) {
            return DecimalOps.Compare(x, y) <= 0;
        }
        [OperatorMethod]
        public static bool GreaterThan(BigInteger x, decimal y) {
            return DecimalOps.Compare(x, y) > 0;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual(BigInteger x, decimal y) {
            return DecimalOps.Compare(x, y) >= 0;
        }
        [OperatorMethod]
        public static bool Equal(BigInteger x, decimal y) {
            return DecimalOps.Compare(x, y) == 0;
        }
        [OperatorMethod]
        public static bool NotEqual(BigInteger x, decimal y) {
            return DecimalOps.Compare(x, y) != 0;
        }


        [OperatorMethod, PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object Compare(CodeContext context, BigInteger x, object y) {
            if (y == null) return 1;

            int intVal;
            if (y is int) {
                if (x.AsInt32(out intVal)) return Int32Ops.Compare(context, intVal, y);
            } else if (y is double) {
                return -((int)DoubleOps.Compare((double)y, x));
            } else if (y is Extensible<double>) {
                double dbl = x.ToFloat64();
                return DoubleOps.Compare(context, dbl, ((Extensible<double>)y).Value);
            } else if (y is bool) {
                if (x.AsInt32(out intVal)) return Int32Ops.Compare(context, intVal, ((bool)y) ? 1 : 0);
            } else if (y is decimal) {
                double dbl = x.ToFloat64();
                return DoubleOps.Compare(context, dbl, y);
            }

            BigInteger bi;
            if (!Converter.TryConvertToBigInteger(y, out bi)) {
                object res;
                if(Ops.GetDynamicType(y).TryInvokeBinaryOperator(context, Operators.Coerce, y, x, out res)) {
                    if (res != Ops.NotImplemented && !(res is OldInstance)) {
                        return Ops.Compare(context, ((Tuple)res)[1], ((Tuple)res)[0]);
                    }
                }
                return Ops.NotImplemented;
            }

            BigInteger diff = x - bi;
            if (diff == 0) return 0;
            else if (diff < 0) return -1;
            else return 1;
        }
    }
}
