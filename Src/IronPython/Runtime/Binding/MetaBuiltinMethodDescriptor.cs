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
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using System.Scripting.Generation;
    using Microsoft.Scripting.Generation;

    class MetaBuiltinMethodDescriptor : MetaPythonObject {
        public MetaBuiltinMethodDescriptor(Expression/*!*/ expression, Restrictions/*!*/ restrictions, BuiltinMethodDescriptor/*!*/ value)
            : base(expression, Restrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region MetaObject Overrides

        public override MetaObject/*!*/ Invoke(InvokeAction/*!*/ call, params MetaObject/*!*/[]/*!*/ args) {
            args = ArrayUtils.RemoveFirst(args);

            CallSignature signature = BindingHelpers.GetCallSignature(call);
            Restrictions selfRestrict = Restrictions.InstanceRestriction(Expression, Value).Merge(Restrictions);

            selfRestrict = selfRestrict.Merge(
                Restrictions.ExpressionRestriction(
                    Value.Template.MakeFunctionTest(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("GetBuiltinMethodDescriptorTemplate"),
                            Ast.Convert(Expression, typeof(BuiltinMethodDescriptor))
                        )
                    )
                )
            );

            if (Value.Template.IsReversedOperator) {
                ArrayUtils.SwapLastTwo(args);
            }

            BindingTarget target;

            BinderState state = BinderState.GetBinderState(call);

            MetaObject res = state.Binder.CallMethod(
                Ast.Constant(state.Context),
                Value.Template.Targets, 
                args, 
                signature, 
                selfRestrict,
                NarrowingLevel.None,
                Value.Template.IsBinaryOperator ?
                    PythonNarrowing.BinaryOperator :
                    NarrowingLevel.All,
                Value.Template.Name,
                out target
            );

            if (Value.Template.IsBinaryOperator && args.Length == 2 && res.Expression.NodeType == ExpressionType.ThrowStatement) {
                // Binary Operators return NotImplemented on failure.
                res = new MetaObject(
                    Ast.Property(null, typeof(PythonOps), "NotImplemented"),
                    res.Restrictions
                );
            }

            return res;
        }

        #endregion

        #region Helpers

        public new BuiltinMethodDescriptor/*!*/ Value {
            get {
                return (BuiltinMethodDescriptor)base.Value;
            }
        }

        #endregion
    }
}
