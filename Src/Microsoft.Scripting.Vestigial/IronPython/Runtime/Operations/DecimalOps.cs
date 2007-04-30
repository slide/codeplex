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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Internal;

using IronPython.Runtime;
using IronPython.Runtime.Types;

using Microsoft.Scripting.Math;
using IronPython.Runtime.Operations;

[assembly: PythonExtensionType(typeof(Decimal), typeof(DecimalOps))]
namespace IronPython.Runtime.Operations {
    public static class DecimalOps {

        [PythonName("__cmp__")]
        [return: MaybeNotImplemented]
        public static object Compare(CodeContext context, decimal x, object other) {
            return DoubleOps.Compare(context, (double)x, other);
        }

        [OperatorMethod]
        public static bool LessThan(decimal x, decimal y) {
            return x < y;
        }
        [OperatorMethod]
        public static bool LessThanOrEqual(decimal x, decimal y) {
            return x <= y;
        }
        [OperatorMethod]
        public static bool GreaterThan(decimal x, decimal y) {
            return x > y;
        }
        [OperatorMethod]
        public static bool GreaterThanOrEqual(decimal x, decimal y) {
            return x >= y;
        }
        [OperatorMethod]
        public static bool Equal(decimal x, decimal y) {
            return x == y;
        }
        [OperatorMethod]
        public static bool NotEqual(decimal x, decimal y) {
            return x != y;
        }

        internal static int Compare(BigInteger x, decimal y) {
            return -Compare(y, x);
        }

        internal static int Compare(decimal x, BigInteger y) {
            if (object.ReferenceEquals(y, null)) return +1;
            BigInteger bx = BigInteger.Create(x);
            if (bx == y) {
                decimal mod = x % 1;
                if (mod == 0) return 0;
                if (mod > 0) return +1;
                else return -1;
            }
            return bx > y ? +1 : -1;
        }
    }
}
