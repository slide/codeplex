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
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using MSAst = Microsoft.Scripting.Ast;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

    internal class AstGenerator {
        private readonly MSAst.LambdaBuilder _block;
        private List<MSAst.VariableExpression> _temps;
        private readonly CompilerContext _context;
        private readonly bool _print;
        private readonly Stack<MSAst.LabelTarget> _loopStack = new Stack<MSAst.LabelTarget>();
        private MSAst.VariableExpression _lineNoVar, _lineNoUpdated;

        private bool _generator;

        private AstGenerator(SourceSpan span, string name, bool generator, bool print) {
            _print = print;
            _generator = generator;

            _block = Ast.Lambda(span, name, typeof(object));
        }

        internal AstGenerator(AstGenerator parent, SourceSpan span, string name, bool generator, bool print)
            : this(span, name, generator, print) {
            Assert.NotNull(parent);
            _context = parent.Context;
        }

        internal AstGenerator(CompilerContext context, SourceSpan span, string name, bool generator, bool print)
            : this(span, name, generator, print) {
            Assert.NotNull(context);
            _context = context;
        }

        public bool Optimize {
            get { return ((PythonContext)_context.SourceUnit.LanguageContext).PythonOptions.Optimize; }
        }

        public bool StripDocStrings {
            get { return ((PythonContext)_context.SourceUnit.LanguageContext).PythonOptions.StripDocStrings; }
        }

        public bool DebugMode {
            get { return _context.SourceUnit.LanguageContext.DomainManager.GlobalOptions.DebugMode; }
        }

        public MSAst.LambdaBuilder Block {
            get { return _block; }
        }

        public CompilerContext Context {
            get { return _context; }
        }

        public ActionBinder Binder {
            get { return _context.SourceUnit.LanguageContext.Binder; }
        }

        public bool PrintExpressions {
            get { return _print; }
        }

        internal bool IsGenerator {
            get { return _generator; }
        }

        public MSAst.LabelTarget EnterLoop() {
            MSAst.LabelTarget label = Ast.Label();
            _loopStack.Push(label);
            return label;
        }

        public void ExitLoop() {
            _loopStack.Pop();
        }

        public bool InLoop {
            get { return _loopStack.Count > 0; }
        }

        public MSAst.LabelTarget LoopLabel {
            get { return _loopStack.Peek(); }
        }

        public void AddError(string message, SourceSpan span) {
            // TODO: error code
            _context.Errors.Add(_context.SourceUnit, message, span, -1, Severity.Error);
        }

        public MSAst.VariableExpression MakeTemp(SymbolId name, Type type) {
            if (_temps != null) {
                foreach (MSAst.VariableExpression temp in _temps) {
                    if (temp.Type == type) {
                        _temps.Remove(temp);
                        return temp;
                    }
                }
            }
            return _block.CreateTemporaryVariable(name, type);
        }


        public MSAst.VariableExpression MakeTempExpression(string name) {
            return MakeTempExpression(name, typeof(object));
        }

        public MSAst.VariableExpression MakeTempExpression(string name, Type type) {
            return Ast.Read(MakeTemp(SymbolTable.StringToId(name), type));
        }

        public void FreeTemp(MSAst.VariableExpression temp) {
            if (IsGenerator) {
                return;
            }

            if (_temps == null) {
                _temps = new List<MSAst.VariableExpression>();
            }
            _temps.Add(temp);
        }

        internal static MSAst.Expression MakeAssignment(MSAst.VariableExpression variable, MSAst.Expression right) {
            return MakeAssignment(variable, right, SourceSpan.None);
        }

        internal static MSAst.Expression MakeAssignment(MSAst.VariableExpression variable, MSAst.Expression right, SourceSpan span) {
            return Ast.Assign(span, variable, Ast.Convert(right, variable.Type));
        }

        internal static MSAst.Expression ConvertIfNeeded(MSAst.Expression expression, Type type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                expression = Ast.Convert(expression, type);
            }
            return expression;
        }

        internal MSAst.Expression DynamicConvertIfNeeded(MSAst.Expression expression, Type type) {
            Debug.Assert(expression != null);
            // Do we need conversion?
            if (!CanAssign(type, expression.Type)) {
                // Add conversion step to the AST
                expression = Ast.Action.ConvertTo(Binder, type, ConversionResultKind.ExplicitCast, expression);
            }
            return expression;
        }

        internal static bool CanAssign(Type to, Type from) {
            return to.IsAssignableFrom(from) && (to.IsValueType == from.IsValueType);
        }

        public string GetDocumentation(Statement stmt) {
            if (StripDocStrings) {
                return null;
            }

            return stmt.Documentation;
        }

        #region Dynamic stack trace support

        /// <summary>
        /// A temporary variable to track the current line number
        /// </summary>
        internal MSAst.VariableExpression LineNumberExpression {
            get {
                if (_lineNoVar == null) {
                    _lineNoVar = _block.CreateTemporaryVariable(SymbolTable.StringToId("$lineNo"), typeof(int));
                }

                return _lineNoVar;
            }
        }

        /// <summary>
        /// A temporary variable to track if the current line number has been emitted via the fault update block.
        /// 
        /// For example consider:
        /// 
        /// try:
        ///     raise Exception()
        /// except Exception, e:
        ///     # do something here
        ///     raise
        ///     
        /// At "do something here" we need to have already emitted the line number, when we re-raise we shouldn't add it 
        /// again.  If we handled the exception then we should have set the bool back to false.
        /// </summary>
        internal MSAst.VariableExpression LineNumberUpdated {
            get {
                if (_lineNoUpdated == null) {
                    _lineNoUpdated = _block.CreateTemporaryVariable(SymbolTable.StringToId("$lineUpdated"), typeof(bool));
                }

                return _lineNoUpdated;
            }
        }

        /// <summary>
        /// Wraps the body of a statement which should result in a frame being available during
        /// exception handling.  This ensures the line number is updated as the stack is unwound.
        /// </summary>
        internal MSAst.Expression WrapScopeStatements(MSAst.Expression body) {
            return Ast.Try(
                body
            ).Catch(
                typeof(Exception),
                GetLineNumberUpdateExpression(),
                Ast.Rethrow()
            );
        }

        internal MSAst.Expression GetLineNumberUpdateExpression() {
            return GetLineNumberUpdateExpression(true);
        }

        /// <summary>
        /// Emits the actual updating of the line number for stack traces to be available
        /// </summary>
        internal MSAst.Expression GetLineNumberUpdateExpression(bool preventAdditionalAdds) {
            return Ast.Block(
                SourceSpan.None,
                Ast.If(
                    SourceSpan.None,
                    Ast.Not(
                        LineNumberUpdated
                    ),
                    SourceLocation.None,
                    Ast.Call(
                        SourceSpan.None,
                        typeof(ExceptionHelpers).GetMethod("UpdateStackTrace"),
                        Ast.CodeContext(),
                        Ast.Call(typeof(MethodBase).GetMethod("GetCurrentMethod")),
                        Ast.Constant(_block.Name),
                        Ast.Constant(Context.SourceUnit.Path ?? "<string>"),
                        LineNumberExpression
                    )
                ),
                Ast.Assign(
                    SourceSpan.None,
                    LineNumberUpdated,
                    Ast.Constant(preventAdditionalAdds)
                )
            );
        }

        #endregion

        #region Utility methods

        public MSAst.Expression Transform(Expression from) {
            return Transform(from, typeof(object));
        }

        public MSAst.Expression Transform(Expression from, Type type) {
            if (from != null) {
                return from.Transform(this, type);
            }
            return null;
        }

        public MSAst.Expression TransformAsObject(Expression from) {
            return TransformAndConvert(from, typeof(object));
        }

        public MSAst.Expression TransformAndConvert(Expression from, Type type) {
            if (from != null) {
                MSAst.Expression transformed = from.Transform(this, type);
                transformed = ConvertIfNeeded(transformed, type);
                return transformed;
            }
            return null;
        }

        internal MSAst.Expression TransformOrConstantNull(Expression expression, Type type) {
            if (expression == null) {
                return Ast.Null(type);
            } else {
                return ConvertIfNeeded(expression.Transform(this, type), type);
            }
        }

        public MSAst.Expression TransformAndDynamicConvert(Expression from, Type type) {
            if (from != null) {
                MSAst.Expression transformed = from.Transform(this, type);
                transformed = DynamicConvertIfNeeded(transformed, type);
                return transformed;
            }
            return null;
        }

        public MSAst.Expression Transform(Statement from) {
            if (from == null) {
                return null;
            } else {
                MSAst.Expression expression = from.Transform(this);
                // expression will be null if there's an error, e.g. None += None
                if (expression != null) {
                    if (expression.Type != typeof(void)) {
                        expression = Ast.Convert(expression, typeof(void));
                    }

                    if (from.Start.IsValid) {
                        // add line number info if the new line is valid.  We could
                        // also take the previous statement and see if the line changed
                        // but this Transform method is rarely used when a line change
                        // wouldn't be seen.
                        expression = Ast.Block(
                            SourceSpan.None,
                            Ast.Void(
                                Ast.Assign(
                                    LineNumberExpression,
                                    Ast.Constant(from.Start.Line)
                                )
                            ),
                            expression
                        );
                    }
                }

                return expression;
            }
        }

        internal MSAst.Expression[] Transform(Expression[] expressions) {
            return Transform(expressions, typeof(object));
        }

        internal MSAst.Expression[] Transform(Expression[] expressions, Type type) {
            Debug.Assert(expressions != null);
            MSAst.Expression[] to = new MSAst.Expression[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                Debug.Assert(expressions[i] != null);
                to[i] = Transform(expressions[i], type);
            }
            return to;
        }

        internal MSAst.Expression[] TransformAndConvert(Expression[] expressions, Type type) {
            Debug.Assert(expressions != null);
            MSAst.Expression[] to = new MSAst.Expression[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                Debug.Assert(expressions[i] != null);
                to[i] = TransformAndConvert(expressions[i], type);
            }
            return to;
        }

        internal MSAst.Expression[] Transform(Statement[] from) {
            Debug.Assert(from != null);
            MSAst.Expression[] to = new MSAst.Expression[from.Length];
            Nullable<int> curLine = null;

            for (int i = 0; i < from.Length; i++) {
                Debug.Assert(from[i] != null);
                MSAst.Expression toExpr = from[i].Transform(this);

                if (toExpr == null) {
                    // error node, e.g. break outside of a loop
                    continue;
                }

                // add line number tracking when the line changes...
                if ((curLine.HasValue && from[i].Start.IsValid && curLine.Value != from[i].Start.Line) ||
                    (!curLine.HasValue && from[i].Start.IsValid)) {
                    curLine = from[i].Start.Line;

                    toExpr = Ast.Block(
                        SourceSpan.None,
                        Ast.Void(
                            Ast.Assign(
                                LineNumberExpression,
                                Ast.Constant(curLine.Value)
                            )
                        ),
                        toExpr
                    );

                }

                to[i] = toExpr;
            }
            return to;
        }

        #endregion

        /// <summary>
        /// Returns MethodInfo of the Python helper method given its name.
        /// </summary>
        /// <param name="name">Method name to find.</param>
        /// <returns></returns>
        internal static MethodInfo GetHelperMethod(string name) {
            MethodInfo mi = typeof(PythonOps).GetMethod(name);
            Debug.Assert(mi != null, "Missing Python helper: " + name);
            return mi;
        }

        /// <summary>
        /// Returns MethodInfo of the Python helper method given its name and signature.
        /// </summary>
        /// <param name="name">Name of the method to return</param>
        /// <param name="types">Parameter types</param>
        /// <returns></returns>
        internal static MethodInfo GetHelperMethod(string name, params Type[] types) {
            MethodInfo mi = typeof(PythonOps).GetMethod(name, types);
#if DEBUG
            if (mi == null) {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("(");
                for (int i = 0; i < types.Length; i++) {
                    if (i > 0) sb.Append(", ");
                    sb.Append(types[i].Name);
                }
                sb.Append(")");
                Debug.Assert(mi != null, "Missing Python helper: " + name + sb.ToString());
            }
#endif
            return mi;
        }
    }
}
