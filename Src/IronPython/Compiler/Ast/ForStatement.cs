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

using System.Collections;
using System.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class ForStatement : Statement {
        private SourceLocation _header;
        private readonly Expression _left;
        private Expression _list;
        private Statement _body;
        private readonly Statement _else;

        public ForStatement(Expression left, Expression list, Statement body, Statement else_) {
            _left = left;
            _list = list;
            _body = body;
            _else = else_;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Left {
            get { return _left; }
        }

        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        public Expression List {
            get { return _list; }
            set { _list = value; }
        }

        public Statement Else {
            get { return _else; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Temporary variable for the IEnumerator object
            MSAst.VariableExpression enumerator = ag.GetTemporary("foreach_enumerator", typeof(IEnumerator));

            // Only the body is "in the loop" for the purposes of break/continue
            // The "else" clause is outside
            MSAst.Expression body;
            MSAst.LabelTarget label = ag.EnterLoop();
            try {
                body = ag.Transform(_body);
            } finally {
                ag.ExitLoop();
            }
            return TransformForStatement(ag, enumerator, _list, _left, body, _else, Span, _header, label);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_left != null) {
                    _left.Walk(walker);
                }
                if (_list != null) {
                    _list.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        internal static MSAst.Expression TransformForStatement(AstGenerator ag, MSAst.VariableExpression enumerator,
                                                    Expression list, Expression left, MSAst.Expression body,
                                                    Statement else_, SourceSpan span, SourceLocation header,
                                                    MSAst.LabelTarget loopLabel) {
            // enumerator = PythonOps.GetEnumeratorForIteration(list)
            MSAst.AssignmentExpression init = AstUtils.Assign(
                enumerator, 
                Ast.Call(
                    AstGenerator.GetHelperMethod("GetEnumeratorForIteration"),
                    AstUtils.CodeContext(),
                    ag.TransformAsObject(list)
                ), 
                list.Span
            );

            // while enumerator.MoveNext():
            //    left = enumerator.Current
            //    body
            // else:
            //    else
            MSAst.LoopStatement ls = AstUtils.Loop(
                Ast.Call(
                    enumerator,
                    typeof(IEnumerator).GetMethod("MoveNext")
                ), 
                null, 
                Ast.Block(
                    left.TransformSet(
                        ag,
                        SourceSpan.None,
                        Ast.Call(
                            enumerator,
                            typeof(IEnumerator).GetProperty("Current").GetGetMethod()
                        ),
                        Operators.None
                    ),
                    body,
                    AstUtils.Block(
                        SourceSpan.None,
                        Ast.Assign(ag.LineNumberExpression, Ast.Constant(list.Start.Line))
                    )
                ), 
                ag.Transform(else_), 
                loopLabel, 
                left.End, 
                new SourceSpan(left.Start, span.End)
            );

            return Ast.Block(
                init,
                ls
            );
        }

        internal override bool CanThrow {
            get {
                if (_left.CanThrow) {
                    return true;
                }

                if (_list.CanThrow) {
                    return true;
                }

                // most constants (int, float, long, etc...) will throw here
                ConstantExpression ce = _list as ConstantExpression;
                if (ce != null) {
                    if (ce.Value is string) {
                        return false;
                    }
                    return true;
                }

                return false;
            }
        }
    }
}