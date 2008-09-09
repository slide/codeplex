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

using System.Diagnostics;
using System.Scripting.Utils;

namespace System.Linq.Expressions.Compiler {
    internal static partial class DelegateHelpers {

        #region Generated Maximum Delegate Arity

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_max_delegate_arity from: generate_dynsites.py

        private const int MaximumArity = 11;

        // *** END GENERATED CODE ***

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static Type MakeDelegate(Type[] types) {
            Debug.Assert(types != null && types.Length > 0);

            // Can only used predefined delegates if we have no byref types and
            // the arity is small enough to fit in Func<...> or Action<...>
            if (types.Length > MaximumArity || types.Any(t => t.IsByRef)) {
                return MakeCustomDelegate(types);
            }

            Type returnType = types[types.Length - 1];
            if (returnType == typeof(void)) {
                types = types.RemoveLast();
                switch (types.Length) {
                    case 0: return typeof(Action);
                    #region Generated Delegate Action Types

                    // *** BEGIN GENERATED CODE ***
                    // generated by function: gen_delegate_action from: generate_dynsites.py

                    case 1: return typeof(Action<>).MakeGenericType(types);
                    case 2: return typeof(Action<,>).MakeGenericType(types);
                    case 3: return typeof(Action<,,>).MakeGenericType(types);
                    case 4: return typeof(Action<,,,>).MakeGenericType(types);
                    case 5: return typeof(Action<,,,,>).MakeGenericType(types);
                    case 6: return typeof(Action<,,,,,>).MakeGenericType(types);
                    case 7: return typeof(Action<,,,,,,>).MakeGenericType(types);
                    case 8: return typeof(Action<,,,,,,,>).MakeGenericType(types);
                    case 9: return typeof(Action<,,,,,,,,>).MakeGenericType(types);
                    case 10: return typeof(Action<,,,,,,,,,>).MakeGenericType(types);

                    // *** END GENERATED CODE ***

                    #endregion
                }
            } else {
                switch (types.Length) {
                    #region Generated Delegate Func Types

                    // *** BEGIN GENERATED CODE ***
                    // generated by function: gen_delegate_func from: generate_dynsites.py

                    case 1: return typeof(Func<>).MakeGenericType(types);
                    case 2: return typeof(Func<,>).MakeGenericType(types);
                    case 3: return typeof(Func<,,>).MakeGenericType(types);
                    case 4: return typeof(Func<,,,>).MakeGenericType(types);
                    case 5: return typeof(Func<,,,,>).MakeGenericType(types);
                    case 6: return typeof(Func<,,,,,>).MakeGenericType(types);
                    case 7: return typeof(Func<,,,,,,>).MakeGenericType(types);
                    case 8: return typeof(Func<,,,,,,,>).MakeGenericType(types);
                    case 9: return typeof(Func<,,,,,,,,>).MakeGenericType(types);
                    case 10: return typeof(Func<,,,,,,,,,>).MakeGenericType(types);
                    case 11: return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);

                    // *** END GENERATED CODE ***

                    #endregion
                }
            }
            throw Assert.Unreachable;
        }
    }
}