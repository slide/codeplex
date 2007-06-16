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

using System.Collections.Generic;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    public class NullArgBuilder : ArgBuilder {
        public NullArgBuilder() { }

        public override int Priority {
            get { return 0; }
        }

        public override object Build(CodeContext context, object[] args) {
            return null;
        }

        public override void Generate(CodeGen cg, IList<Slot> argSlots) {
            cg.EmitNull();
        }

        public override Expression ToExpression(ActionBinder binder, Expression[] parameters) {
            return ConstantExpression.Constant(null);
        }
    }
}
