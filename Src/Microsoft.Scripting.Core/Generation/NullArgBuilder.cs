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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    using Ast = Microsoft.Scripting.Ast.Expression;
    using System.Collections.Generic;

    /// <summary>
    /// ArgBuilder which always produces null.  
    /// </summary>
    class NullArgBuilder : ArgBuilder {
        public NullArgBuilder() { }

        public override int Priority {
            get { return 0; }
        }

        public override object Build(CodeContext context, object[] args) {
            return null;
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            return Ast.Null();
        }
    }
}
