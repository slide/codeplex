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

using System;
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Ast {
    public class BoundAssignment : Expression {
        private readonly Variable _variable;
        private readonly Expression _value;
        private readonly Operators _op;
        private bool _defined;

        // implementation detail.
        private VariableReference _vr;

        public BoundAssignment(Variable variable, Expression value, Operators op)
            : this(variable, value, op, SourceSpan.None) {
        }

        public BoundAssignment(Variable variable, Expression value, Operators op, SourceSpan span)
            : base(span) {
            if (variable == null) throw new ArgumentNullException("variable");
            if (value == null) throw new ArgumentNullException("value");
            _variable = variable;
            _value = value;
            _op = op;
        }

        public Variable Variable {
            get { return _variable; }
        }

        internal VariableReference Ref {
            get { return _vr; }
            set {
                Debug.Assert(value != null);
                Debug.Assert(value.Variable == _variable);
                Debug.Assert(_vr == null || (object)_vr == (object)value);
                _vr = value;
            }
        }

        public Expression Value {
            get { return _value; }
        }

        public Operators Operator {
            get { return _op; }
        }

        public bool IsDefined {
            get { return _defined; }
            set { _defined = value; }
        }

        public override Type ExpressionType {
            get {
                return _variable.Type;
            }
        }

        public override void Emit(CodeGen cg) {
            if (_op == Operators.None) {
                _value.EmitAs(cg, _vr.Slot.Type);
            } else {
                cg.EmitInPlaceOperator(
                    _op,
                    _variable.Type,
                    delegate (CodeGen _cg, Type _as) {
                        _cg.EmitGet(_vr.Slot, _variable.Name, !_defined);
                        _cg.EmitConvert(_variable.Type, _as);
                    },
                    _value.ExpressionType,
                    _value.EmitAs
                );
                cg.EmitConvert(typeof(object), _vr.Slot.Type);
            }

            cg.Emit(OpCodes.Dup);
            _vr.Slot.EmitSet(cg);
        }

        public override object Evaluate(CodeContext context) {
            object result;
            if (_op == Operators.None) { // Just an assignment
                result = _value.Evaluate(context);
            } else {
                //TODO: is constructing an ActionExpression on the fly really necessary?
                ActionExpression action = ActionExpression.Operator(
                    _op, _variable.Type,
                    BoundExpression.Defined(_variable), _value);
                result = action.Evaluate(context);
            }
            
            RuntimeHelpers.SetName(context, _variable.Name, result);
            return result;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }

        #region Factory methods

        public static BoundAssignment Assign(Variable variable, Expression value) {
            return OpAssign(variable, value, Operators.None, SourceSpan.None);
        }

        public static BoundAssignment Assign(Variable variable, Expression value, SourceSpan span) {
            return OpAssign(variable, value, Operators.None, span);
        }

        public static BoundAssignment OpAssign(Variable variable, Expression value, Operators op, SourceSpan span) {
            return new BoundAssignment(variable, value, op, span);
        }

        #endregion
    }
}
