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

using System.Scripting;

namespace IronPython.Compiler.Ast {
    public class RelativeModuleName : ModuleName {
        private readonly int _dotCount;

        public RelativeModuleName(SymbolId[] names, int dotCount)
            : base(names) {
            _dotCount = dotCount;
        }

        public int DotCount {
            get {
                return _dotCount;
            }
        }
    }
}
