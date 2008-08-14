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

#if !SILVERLIGHT

using System.Linq.Expressions;
using System.Scripting.Actions;

namespace System.Scripting.Com {
    internal class MetaUnwrappedComObject : MetaObject {
        public MetaUnwrappedComObject(Expression self, Restrictions restrictions)
            : base(self, restrictions) {
        }
        /// <summary>
        /// Our type information is 100% concrete
        /// </summary>
        public override bool NeedsDeferral {
            get {
                return false;
            }
        }

        /// <summary>
        /// When we hand out one of these the exact type is known and
        /// cannot be further restricted.
        /// </summary>
        public override MetaObject Restrict(Type type) {
            return this;
        }
    }
}

#endif