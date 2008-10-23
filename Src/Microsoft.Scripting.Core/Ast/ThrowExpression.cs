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
using System; using Microsoft;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions {
    // TODO: merge with UnaryExpression
    public sealed class ThrowExpression : Expression {
        private readonly Expression _value;

        internal ThrowExpression(Annotations annotations, Expression value)
            : base(annotations) {
            _value = value;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        internal override Expression.NodeFlags GetFlags() {
            return NodeFlags.CanRead;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.ThrowStatement;
        }

        public Expression Value {
            get { return _value; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitThrow(this);
        }
    }

    public partial class Expression {
        public static ThrowExpression Rethrow() {
            return Throw(null);
        }

        public static ThrowExpression Throw(Expression value) {
            return Throw(value, Annotations.Empty);
        }

        public static ThrowExpression Throw(Expression value, Annotations annotations) {
            if (value != null) {
                RequiresCanRead(value, "value");
                ContractUtils.Requires(
                    TypeUtils.AreReferenceAssignable(typeof(Exception), value.Type),
                    "value",
                    Strings.ArgumentMustBeException
                );
            }
            return new ThrowExpression(annotations, value);
        }
    }
}