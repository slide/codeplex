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
using System.Linq.Expressions;
using System.Text;
using System.Scripting.Actions;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    /// <summary>
    /// Builds up a series of conditionals when the False clause isn't yet known.  We can
    /// keep appending conditions and if true's.  Each subsequent true branch becomes the
    /// false branch of the previous condition and body.  Finally a non-conditional terminating
    /// branch must be added.
    /// </summary>
    class ConditionalBuilder {
        private readonly List<Expression/*!*/>/*!*/ _conditions = new List<Expression>();
        private readonly List<Expression/*!*/>/*!*/ _bodies = new List<Expression>();
        private readonly List<VariableExpression/*!*/>/*!*/ _variables = new List<VariableExpression>();
        private Expression _body;
        private Restrictions/*!*/ _restrictions = Restrictions.Empty;

        /// <summary>
        /// Adds a new conditional and body.  The first call this becomes the top-level
        /// conditional, subsequent calls will have it added as false statement of the
        /// previous conditional.
        /// </summary>
        public void AddCondition(Expression/*!*/ condition, Expression/*!*/ body) {
            Assert.NotNull(condition, body);

            _conditions.Add(condition);
            _bodies.Add(body);
        }

        /// <summary>
        /// Adds the non-conditional terminating node.
        /// </summary>
        public void FinishCondition(Expression/*!*/ body) {
            if (_body != null) throw new InvalidOperationException();

            for (int i = _bodies.Count - 1; i >= 0; i--) {
                Type t = _bodies[i].Type;
                if (t != body.Type) {
                    if (t.IsSubclassOf(body.Type)) {
                        // subclass
                        t = body.Type;
                    } else if (body.Type.IsSubclassOf(t)) {
                        // keep t
                    } else {
                        // incompatible, both go to object
                        t = typeof(object);
                    }
                }

                body = Ast.Condition(
                    _conditions[i],
                    Ast.ConvertHelper(_bodies[i], t),
                    Ast.ConvertHelper(body, t)
                );
            }

            _body = Ast.Scope(
                body,
                _variables
            );
        }

        public Restrictions Restrictions {
            get {
                return _restrictions;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _restrictions = value;
            }
        }

        /// <summary>
        /// Gets the resulting meta object for the full body.  FinishCondition
        /// must have been called.
        /// </summary>
        public MetaObject/*!*/ GetMetaObject(params MetaObject/*!*/[]/*!*/ types) {
            if (_body == null) {
                throw new InvalidOperationException("FinishCondition should have been called");
            }

            return new MetaObject(
                _body,
                Restrictions.Combine(types).Merge(Restrictions)
            );
        }

        /// <summary>
        /// Adds a variable which will be scoped at the level of the final expression.
        /// </summary>
        public void AddVariable(VariableExpression/*!*/ var) {
            _variables.Add(var);
        }
    }

}
