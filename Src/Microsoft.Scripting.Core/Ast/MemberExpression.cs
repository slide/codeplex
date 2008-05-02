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
using System.Reflection;
using System.Diagnostics;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Member expression (statically typed) which represents 
    /// property or field access, both static and instance.
    /// For instance property/field, Expression must be != null.
    /// </summary>
    public sealed class MemberExpression : Expression {
        private readonly MemberInfo _member;
        private readonly Expression _expression;

        public MemberInfo Member {
            get { return _member; }
        }

        public Expression Expression {
            get { return _expression; }
        }

        internal MemberExpression(MemberInfo member, Expression expression, Type type, MemberAction bindingInfo)
            : base(Annotations.Empty, AstNodeType.MemberExpression, type, bindingInfo) {
            if (IsBound) {
                RequiresBound(expression, "expression");
            }
            _member = member;
            _expression = expression;
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        internal static void CheckField(FieldInfo info, Expression instance, Expression rightValue) {
            ContractUtils.RequiresNotNull(info, "field");
            ContractUtils.Requires((instance == null) == info.IsStatic, "expression",
                "Static field requires null expression, non-static field requires non-null expression.");
            ContractUtils.Requires(instance == null || TypeUtils.CanAssign(info.DeclaringType, instance.Type), "expression", "Incorrect instance type for the field");
            ContractUtils.Requires(rightValue == null || TypeUtils.CanAssign(info.FieldType, rightValue.Type), "value", "Incorrect value type for the field");
        }

        internal static void CheckProperty(PropertyInfo info, Expression instance, Expression rightValue) {
            ContractUtils.RequiresNotNull(info, "property");
            MethodInfo mi = (rightValue != null) ? info.GetSetMethod() : info.GetGetMethod();
            ContractUtils.Requires(mi != null, "Property is not readable");
            ContractUtils.Requires((instance == null) == mi.IsStatic, "expression",
                "Static property requires null expression, non-static property requires non-null expression.");
            ContractUtils.Requires(instance == null || TypeUtils.CanAssign(info.DeclaringType, instance.Type), "expression", "Incorrect instance type for the property");
            ContractUtils.Requires(rightValue == null || TypeUtils.CanAssign(info.PropertyType, rightValue.Type), "value", "Incorrect value type for the property");
        }

        internal static FieldInfo GetFieldChecked(Type type, string field, Expression instance, Expression rightValue) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(field, "field");

            FieldInfo fi = type.GetField(field);
            ContractUtils.Requires(fi != null, "field", "Type doesn't have the specified field");
            CheckField(fi, instance, rightValue);
            return fi;
        }

        internal static PropertyInfo GetPropertyChecked(Type type, string property, Expression instance, Expression rightValue) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(property, "property");

            PropertyInfo pi = type.GetProperty(property);
            ContractUtils.Requires(pi != null, "property", "Type doesn't have the specified property");
            CheckProperty(pi, instance, rightValue);
            return pi;
        }

        public static MemberExpression ReadField(Expression expression, Type type, string field) {
            return ReadField(expression, GetFieldChecked(type, field, expression, null));
        }

        /// <summary>
        /// Creates MemberExpression representing field access, instance or static.
        /// For static field, expression must be null and FieldInfo.IsStatic == true
        /// For instance field, expression must be non-null and FieldInfo.IsStatic == false.
        /// </summary>
        /// <param name="expression">Expression that evaluates to the instance for the field access.</param>
        /// <param name="field">Field represented by this Member expression.</param>
        /// <returns>New instance of Member expression</returns>
        public static MemberExpression ReadField(Expression expression, FieldInfo field) {
            CheckField(field, expression, null);
            return new MemberExpression(field, expression, field.FieldType, null);
        }

        public static MemberExpression ReadProperty(Expression expression, Type type, string property) {
            return ReadProperty(expression, GetPropertyChecked(type, property, expression, null));
        }

        /// <summary>
        /// Creates MemberExpression representing property access, instance or static.
        /// For static properties, expression must be null and property.IsStatic == true.
        /// For instance properties, expression must be non-null and property.IsStatic == false.
        /// </summary>
        /// <param name="expression">Expression that evaluates to the instance for instance property access.</param>
        /// <param name="property">PropertyInfo of the property to access</param>
        /// <returns>New instance of the MemberExpression.</returns>
        public static MemberExpression ReadProperty(Expression expression, PropertyInfo property) {
            CheckProperty(property, expression, null);
            return new MemberExpression(property, expression, property.PropertyType, null);
        }

        /// <summary>
        /// A dynamic or unbound get member
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static MemberExpression GetMember(Expression expression, Type result, GetMemberAction bindingInfo) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(bindingInfo, "bindingInfo");
            return new MemberExpression(null, expression, result, bindingInfo);
        }
    }
}
