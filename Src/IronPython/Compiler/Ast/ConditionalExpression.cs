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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Internal.Ast;

namespace IronPython.Compiler.Ast {
    public class ConditionalExpression : Expression {
        private readonly Expression _testExpr;
        private readonly Expression _trueExpr;
        private readonly Expression _falseExpr;

        public ConditionalExpression(Expression testExpression, Expression trueExpression, Expression falseExpression) {
            this._testExpr = testExpression;
            this._trueExpr = trueExpression;
            this._falseExpr = falseExpression;
        }

        public Expression FalseExpression {
            get { return _falseExpr; }
        }

        public Expression Test {
            get { return _testExpr; }
        }

        public Expression TrueExpression {
            get { return _trueExpr; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            return new MSAst.ConditionalExpression(
                ag.Transform(_testExpr),
                ag.Transform(_trueExpr),
                ag.Transform(_falseExpr),
                Span
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_testExpr != null) {
                    _testExpr.Walk(walker);
                }
                if (_trueExpr != null) {
                    _trueExpr.Walk(walker);
                }
                if (_falseExpr != null) {
                    _falseExpr.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
