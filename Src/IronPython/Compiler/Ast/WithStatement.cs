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
using System.Scripting;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class WithStatement : Statement {
        private SourceLocation _header;
        private readonly Expression _contextManager;
        private readonly Expression _var;
        private Statement _body;

        public WithStatement(Expression contextManager, Expression var, Statement body) {
            _contextManager = contextManager;
            _var = var;
            _body = body;
        }

        public SourceLocation Header {
            set { _header = value; }
        }

        public Expression Variable {
            get { return _var; }
        }

        public Expression ContextManager {
            get { return _contextManager; }
        }

        public Statement Body {
            get { return _body; }
        }

        /// <summary>
        /// WithStatement is translated to the DLR AST equivalent to
        /// the following Python code snippet (from with statement spec):
        /// 
        /// mgr = (EXPR)
        /// exit = mgr.__exit__  # Not calling it yet
        /// value = mgr.__enter__()
        /// exc = True
        /// try:
        ///     VAR = value  # Only if "as VAR" is present
        ///     BLOCK
        /// except:
        ///     # The exceptional case is handled here
        ///     exc = False
        ///     if not exit(*sys.exc_info()):
        ///         raise
        ///     # The exception is swallowed if exit() returns true
        /// finally:
        ///     # The normal and non-local-goto cases are handled here
        ///     if exc:
        ///         exit(None, None, None)
        /// 
        /// </summary>
        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Five statements in the result...
            MSAst.Expression[] statements = new MSAst.Expression[5];

            //******************************************************************
            // 1. mgr = (EXPR)
            //******************************************************************
            MSAst.VariableExpression manager = ag.MakeTempExpression("with_manager");
            statements[0] = AstGenerator.MakeAssignment(
                manager,
                ag.Transform(_contextManager),
                new SourceSpan(Start, _header)
            );

            //******************************************************************
            // 2. exit = mgr.__exit__  # Not calling it yet
            //******************************************************************
            MSAst.VariableExpression exit = ag.MakeTempExpression("with_exit");
            statements[1] = AstGenerator.MakeAssignment(
                exit,
                AstUtils.GetMember(
                    ag.Binder,
                    "__exit__",
                    typeof(object),
                    Ast.CodeContext(),
                    manager
                )
            );

            //******************************************************************
            // 3. value = mgr.__enter__()
            //******************************************************************
            MSAst.VariableExpression value = ag.MakeTempExpression("with_value");
            statements[2] = AstGenerator.MakeAssignment(
                value,
                AstUtils.Call(
                    ag.Binder,
                    typeof(object),
                    Ast.CodeContext(),
                    AstUtils.GetMember(
                        ag.Binder,
                        "__enter__",
                        typeof(object),
                        Ast.CodeContext(),
                        manager
                    )                
                )
            );

            //******************************************************************
            // 4. exc = True
            //******************************************************************
            MSAst.VariableExpression exc = ag.MakeTempExpression("with_exc", typeof(bool));
            statements[3] = AstGenerator.MakeAssignment(
                exc,
                Ast.True()
            );

            //******************************************************************
            //  5. The final try statement:
            //
            //  try:
            //      VAR = value  # Only if "as VAR" is present
            //      BLOCK
            //  except:
            //      # The exceptional case is handled here
            //      exc = False
            //      if not exit(*sys.exc_info()):
            //          raise
            //      # The exception is swallowed if exit() returns true
            //  finally:
            //      # The normal and non-local-goto cases are handled here
            //      if exc:
            //          exit(None, None, None)
            //******************************************************************

            MSAst.VariableExpression exception = ag.MakeTempExpression("exception", typeof(Exception));

            statements[4] =
                // try:
                AstUtils.Try(// try statement body
                    _var != null ?
                        AstUtils.Block(
                            _body.Span,
                            // VAR = value
                            _var.TransformSet(ag, SourceSpan.None, value, Operators.None),
                            // BLOCK
                            ag.Transform(_body)
                        ) :
                        // BLOCK
                        ag.Transform(_body), // except:, // try statement location
                        Span, _header
                ).Catch(typeof(Exception), exception,
                    Ast.Block(
                        // Python specific exception handling code
                        Ast.Call(
                            AstGenerator.GetHelperMethod("ClearDynamicStackFrames")
                        ),
                        // exc = False
                        AstGenerator.MakeAssignment(
                            exc,
                            Ast.False()
                        ),
                        //  if not exit(*sys.exc_info()):
                        //      raise
                        Ast.IfThen(
                            AstUtils.Operator(ag.Binder, Operators.Not, typeof(bool), Ast.CodeContext(), MakeExitCall(ag, exit, exception)),
                            Ast.Rethrow()
                        )
                    )
                // finally:
                ).Finally(
                    //  if exc:
                    //      exit(None, None, None)
                    Ast.IfThen(
                        exc,
                        Ast.ActionExpression(
                            OldCallAction.Make(ag.Binder, 3),  // signature doesn't include code context / function
                            typeof(object),
                            MSAst.Expression.Annotate(_contextManager.Span),
                            new MSAst.Expression[] {
                                Ast.CodeContext(),
                                exit,
                                Ast.Null(),
                                Ast.Null(),
                                Ast.Null()
                            }
                        )
                    )
                );

            return AstUtils.Block(_body.Span, statements);
        }

        private MSAst.Expression MakeExitCall(AstGenerator ag, MSAst.VariableExpression exit, MSAst.Expression exception) {
            // The 'with' statement's exceptional clause explicitly does not set the thread's current exception information.
            // So while the pseudo code says:
            //    exit(*sys.exc_info())
            // we'll actually do:
            //    exit(*PythonOps.GetExceptionInfoLocal($exception))
            return AstUtils.Call(
                OldCallAction.Make(ag.Binder, new CallSignature(MSAst.ArgumentKind.List)),
                typeof(bool),
                Ast.CodeContext(),
                exit,
                Ast.Call(
                    AstGenerator.GetHelperMethod("GetExceptionInfoLocal"), 
                    Ast.CodeContext(),
                    exception
                )
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_contextManager != null) {
                    _contextManager.Walk(walker);
                }
                if (_var != null) {
                    _var.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
