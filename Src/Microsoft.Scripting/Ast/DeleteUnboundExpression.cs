/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Ast {
    public class DeleteUnboundExpression : Expression {
        private SymbolId _name;

        internal DeleteUnboundExpression(SourceSpan span, SymbolId name)
            : base(span) {
            _name = name;
        }

        public override void Emit(CodeGen cg) {
            // RuntimeHelpers.RemoveName(CodeContext, name)
            cg.EmitCodeContext();
            cg.EmitSymbolId(_name);
            cg.EmitCall(typeof(RuntimeHelpers), "RemoveName");
        }

        protected override object DoEvaluate(CodeContext context) {
            return RuntimeHelpers.RemoveName(context, _name);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }

    public static partial class Ast {
        public static DeleteUnboundExpression Delete(SymbolId name) {
            return Delete(SourceSpan.None, name);
        }
        public static DeleteUnboundExpression Delete(SourceSpan span, SymbolId name) {
            return new DeleteUnboundExpression(span, name);
        }
    }

}
