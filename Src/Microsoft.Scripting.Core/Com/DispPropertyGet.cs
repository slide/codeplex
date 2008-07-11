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

using System.Diagnostics;

namespace System.Scripting.Com {
    public class DispPropertyGet : DispCallable {
        public DispPropertyGet(IDispatchObject dispatch, ComMethodDesc methodDesc)
            : base(dispatch, methodDesc) {
            Debug.Assert(methodDesc.IsPropertyGet);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public object this[params object[] args] {
            get {
                return UnoptimizedInvoke(SymbolId.EmptySymbols, args); 
            }
        }
    }
}

#endif
