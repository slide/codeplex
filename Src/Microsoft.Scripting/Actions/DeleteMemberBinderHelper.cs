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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Types;

namespace Microsoft.Scripting.Actions {
    using Ast = Microsoft.Scripting.Ast.Ast;

    class DeleteMemberBinderHelper<T> : MemberBinderHelper<T, DeleteMemberAction> {

        public DeleteMemberBinderHelper(CodeContext context, DeleteMemberAction action, object[] args)
            : base(context, action, args) {
        }

        public StandardRule<T> MakeRule() {
            Rule.MakeTest(StrongBoxType ?? CompilerHelpers.GetType(Target));
            Rule.SetTarget(MakeDeleteMemberTarget());

            return Rule;            
        }

        private Statement MakeDeleteMemberTarget() {
            Type type = CompilerHelpers.GetType(Target);
            // This goes away when ICustomMembers goes away.
            if (typeof(ICustomMembers).IsAssignableFrom(type)) {
                MakeCustomMembersBody(type);
                return Body;
            }

            if (!MakeOperatorGetMemberBody(type, "DeleteMember")) {
                MemberGroup group = Binder.GetMember(Action, type, StringName);
                if (group.Count != 0) {
                    if (group[0].MemberType == TrackerTypes.Property) {
                        MethodInfo del = ((PropertyTracker)group[0]).GetDeleteMethod(ScriptDomainManager.Options.PrivateBinding);
                        if (del != null) {
                            MakePropertyDeleteStatement(del);
                            return Body;
                        }
                    }

                    TrackerTypes memType = TrackerTypes.None;
                    foreach (MemberTracker mt in group) {
                        memType |= mt.MemberType;
                    }

                    // The logic for producing error messages here is too Python specific and doesn't make
                    // a whole lot of sense.  We end up w/ different messages for properties & fields

                    Type declType = GetDeclaringMemberType(group);
                    if (memType == TrackerTypes.Field) {
                        if (declType == type) {
                            MakeUndeletableMemberError(declType);
                        } else {
                            MakeReadOnlyMemberError(type);
                        }
                    } else {
                        MakeReadOnlyMemberError(declType);
                    }
                } else {
                    MakeMissingMemberError(type);
                }
            }

            return Body;
        }

        private Type GetDeclaringMemberType(MemberGroup group) {
            Type t = typeof(object);
            foreach (MemberTracker mt in group) {
                if (t.IsAssignableFrom(mt.DeclaringType)) {
                    t = mt.DeclaringType;
                }
            }
            return t;
        }

        private void MakePropertyDeleteStatement(MethodInfo delete) {
            Body = Ast.Block(Body, MakeCallStatement(delete, Rule.Parameters[0]));
        }

        private void MakeCustomMembersBody(Type type) {
            Body = Ast.Block(Body,
                        Ast.If(
                            Ast.Call(
                                Ast.Cast(Instance, typeof(ICustomMembers)),
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
                Expression call = MakeCallExpression(delMem, Rule.Parameters[0], Ast.Constant(StringName));
                Statement ret;

                if (delMem.ReturnType == typeof(bool)) {
                    ret = Ast.If(call, Rule.MakeReturn(Binder, Ast.Null()));
                } else {
                    ret = Rule.MakeReturn(Binder, call);
                }
                Body = Ast.Block(Body, ret);
                return delMem.ReturnType != typeof(bool);
            }
            return false;
        }
    }
}
