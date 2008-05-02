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
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions {
    public static partial class DynamicSiteHelpers {

        #region Generated DynamicSiteHelpers

        // *** BEGIN GENERATED CODE ***
        // generated by function: gen_helpers from: generate_dynsites.py

        public static readonly int MaximumArity = 7;

        public static Type MakeDynamicSiteType(params Type[] types) {
            Type genType;
            switch (types.Length) {
                case 2: genType = typeof(DynamicSiteTarget<,>); break;
                case 3: genType = typeof(DynamicSiteTarget<,,>); break;
                case 4: genType = typeof(DynamicSiteTarget<,,,>); break;
                case 5: genType = typeof(DynamicSiteTarget<,,,,>); break;
                case 6: genType = typeof(DynamicSiteTarget<,,,,,>); break;
                case 7: genType = typeof(DynamicSiteTarget<,,,,,,>); break;
                default: return MakeBigDynamicSite(typeof(CallSite<>), typeof(BigDynamicSiteTarget<,>), types);
            }

            genType = genType.MakeGenericType(types);
            return typeof(CallSite<>).MakeGenericType(new Type[] { genType });
        }

        internal static Type MakeDynamicSiteTargetType(Type/*!*/[] types) {
            Type siteType;

            switch (types.Length) {
                case 2: siteType = typeof(DynamicSiteTarget<,>).MakeGenericType(types); break;
                case 3: siteType = typeof(DynamicSiteTarget<,,>).MakeGenericType(types); break;
                case 4: siteType = typeof(DynamicSiteTarget<,,,>).MakeGenericType(types); break;
                case 5: siteType = typeof(DynamicSiteTarget<,,,,>).MakeGenericType(types); break;
                case 6: siteType = typeof(DynamicSiteTarget<,,,,,>).MakeGenericType(types); break;
                case 7: siteType = typeof(DynamicSiteTarget<,,,,,,>).MakeGenericType(types); break;
                default:
                    Type tupleType = Tuple.MakeTupleType(ArrayUtils.RemoveLast(types));
                    siteType = typeof(BigDynamicSiteTarget<,>).MakeGenericType(tupleType, types[types.Length - 1]);
                    break;
            }
            return siteType;
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
