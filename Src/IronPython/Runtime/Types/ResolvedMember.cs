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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Couples a MemberGroup and the name which produces the member group together
    /// </summary>
    class ResolvedMember {
        public readonly string/*!*/ Name;
        public readonly MemberGroup/*!*/ Member;
        public static readonly ResolvedMember[]/*!*/ Empty = new ResolvedMember[0];

        public ResolvedMember(string/*!*/ name, MemberGroup/*!*/ member) {
            Debug.Assert(name != null);
            Debug.Assert(member != null);

            Name = name;
            Member = member;
        }
    }
}
