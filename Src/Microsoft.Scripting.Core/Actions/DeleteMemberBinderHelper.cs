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

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class DeleteMemberBinderHelper<T> : MemberBinderHelper<T, DeleteMemberAction> {
        private bool _isStatic;

        public DeleteMemberBinderHelper(CodeContext context, DeleteMemberAction action, object[] args)
            : base(context, action, args) {
        }

        public RuleBuilder<T> MakeRule() {
            Rule.MakeTest(StrongBoxType ?? CompilerHelpers.GetType(Target));
            Rule.Target = MakeDeleteMemberTarget();

            return Rule;            
        }

        private Expression MakeDeleteMemberTarget() {
            Type type = CompilerHelpers.GetType(Target);

            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                type = ((TypeTracker)Target).Type;
                _isStatic = true;
                Rule.AddTest(Ast.Equal(Rule.Parameters[0], Ast.RuntimeConstant(Arguments[0])));
            } 


            // This goes away when ICustomMembers goes away.
            if (!_isStatic && typeof(ICustomMembers).IsAssignableFrom(type)) {
                MakeCustomMembersBody(type);
                return Body;
            }

            if (_isStatic || !MakeOperatorGetMemberBody(type, "DeleteMember")) {
                MemberGroup group = Binder.GetMember(Action, type, StringName);
                if (group.Count != 0) {
                    if (group[0].MemberType == TrackerTypes.Property) {
                        MethodInfo del = ((PropertyTracker)group[0]).GetDeleteMethod(PrivateBinding);
                        if (del != null) {
                            MakePropertyDeleteStatement(del);
                            return Body;
                        }
                    }

                    MakeUndeletableMemberError(GetDeclaringMemberType(group));
                } else {
                    MakeMissingMemberError(type);
                }
            }

            return Body;
        }

        private static Type GetDeclaringMemberType(MemberGroup group) {
            Type t = typeof(object);
            foreach (MemberTracker mt in group) {
                if (t.IsAssignableFrom(mt.DeclaringType)) {
                    t = mt.DeclaringType;
                }
            }
            return t;
        }

        private void MakePropertyDeleteStatement(MethodInfo delete) {
            AddToBody(
                Rule.MakeReturn(
                    Binder,
                    Binder.MakeCallExpression(delete, Rule.Parameters[0])
                )
            );
        }

        private void MakeCustomMembersBody(Type type) {
            AddToBody(
                        Ast.If(
                            Ast.Call(
                                Ast.Convert(Instance, typeof(ICustomMembers)),
                                typeof(ICustomMembers).GetMethod("DeleteCustomMember"),
                                Ast.CodeContext(),
                                Ast.Constant(Action.Name)
                            ),
                            Rule.MakeReturn(Binder, Ast.Null())
                        )
                    );
            // if the lookup fails throw an exception
            MakeMissingMemberError(type);
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorGetMemberBody(Type type, string name) {
            MethodInfo delMem = GetMethod(type, name);
            if (delMem != null && delMem.IsSpecialName) {
                Expression call = Binder.MakeCallExpression(delMem, Rule.Parameters[0], Ast.Constant(StringName));
                Expression ret;

                if (delMem.ReturnType == typeof(bool)) {
                    ret = Ast.If(call, Rule.MakeReturn(Binder, Ast.Null()));
                } else {
                    ret = Rule.MakeReturn(Binder, call);
                }
                AddToBody( ret);
                return delMem.ReturnType != typeof(bool);
            }
            return false;
        }
    }
}
