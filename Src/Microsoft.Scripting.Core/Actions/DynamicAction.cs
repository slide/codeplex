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

using Microsoft.Contracts;

namespace Microsoft.Scripting.Actions {
    public enum DynamicActionKind {
        DoOperation,
        ConvertTo,

        GetMember,
        SetMember,
        DeleteMember,
        InvokeMember,

        Call,
        CreateInstance
    }

    public abstract class DynamicAction {
        private readonly ActionBinder _binder;

        internal DynamicAction(ActionBinder binder) {
            _binder = binder;
        }

        public ActionBinder Binder {
            get {
                return _binder;
            }
        }

        public abstract DynamicActionKind Kind { get; }

        [Confined]
        public override string/*!*/ ToString() {
            return Kind.ToString();
        }
    }
}
