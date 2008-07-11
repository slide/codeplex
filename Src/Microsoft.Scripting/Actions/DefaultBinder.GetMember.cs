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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Scripting.Actions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    
    public partial class DefaultBinder : ActionBinder {

        /// <summary>
        /// Builds a MetaObject for performing a member get.  Supports all built-in .NET members, the OperatorMethod 
        /// GetBoundMember, and StrongBox instances.
        /// </summary>
        /// <param name="name">
        /// The name of the member to retrieve.  This name is not processed by the DefaultBinder and
        /// is instead handed off to the GetMember API which can do name mangling, case insensitive lookups, etc...
        /// </param>
        /// <param name="target">
        /// The MetaObject from which the member is retrieved.
        /// </param>
        public MetaObject/*!*/ GetMember(string name, MetaObject/*!*/ target) {
            return GetMember(name, target, Ast.Null(typeof(CodeContext)));
        }

        /// <summary>
        /// Builds a MetaObject for performing a member get.  Supports all built-in .NET members, the OperatorMethod 
        /// GetBoundMember, and StrongBox instances.
        /// </summary>
        /// <param name="name">
        /// The name of the member to retrieve.  This name is not processed by the DefaultBinder and
        /// is instead handed off to the GetMember API which can do name mangling, case insensitive lookups, etc...
        /// </param>
        /// <param name="target">
        /// The MetaObject from which the member is retrieved.
        /// </param>
        /// <param name="codeContext">
        /// An expression which provides access to the CodeContext if its required for 
        /// accessing the member (e.g. for an extension property which takes CodeContext).  By default this
        /// a null CodeContext object is passed.
        /// </param>
        public MetaObject/*!*/ GetMember(string name, MetaObject/*!*/ target, Expression/*!*/ codeContext) {
            return GetMember(
                name,
                target,
                codeContext,
                false
            );
        }

        /// <summary>
        /// Builds a MetaObject for performing a member get.  Supports all built-in .NET members, the OperatorMethod 
        /// GetBoundMember, and StrongBox instances.
        /// </summary>
        /// <param name="name">
        /// The name of the member to retrieve.  This name is not processed by the DefaultBinder and
        /// is instead handed off to the GetMember API which can do name mangling, case insensitive lookups, etc...
        /// </param>
        /// <param name="target">
        /// The MetaObject from which the member is retrieved.
        /// </param>
        /// <param name="codeContext">
        /// An expression which provides access to the CodeContext if its required for 
        /// accessing the member (e.g. for an extension property which takes CodeContext).  By default this
        /// a null CodeContext object is passed.
        /// </param>
        /// <param name="isNoThrow">
        /// True if the operation should return Operation.Failed on failure, false if it
        /// should return the exception produced by MakeMissingMemberError.
        /// </param>
        public MetaObject/*!*/ GetMember(string name, MetaObject/*!*/ target, Expression/*!*/ codeContext, bool isNoThrow) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(codeContext, "codeContext");

            return MakeGetMemberTarget(
                new GetMemberInfo(
                    name,
                    codeContext,
                    isNoThrow
                ),
                target
            );
        }

        private MetaObject/*!*/ MakeGetMemberTarget(GetMemberInfo/*!*/ getMemInfo, MetaObject/*!*/ target) {            
            Type type = target.RuntimeType;
            Restrictions restrictions = target.Restrictions;
            Expression self = target.Expression;
            target = target.Restrict(target.RuntimeType);

            // needed for GetMember call until DynamicAction goes away
            OldDynamicAction act = OldGetMemberAction.Make(
                this,
                getMemInfo.Name
            );

            // Specially recognized types: TypeTracker, NamespaceTracker, and StrongBox.  
            // TODO: TypeTracker and NamespaceTracker should technically be IDO's.
            MemberGroup members = MemberGroup.EmptyGroup;
            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                restrictions = restrictions.Merge(
                    Restrictions.InstanceRestriction(target.Expression, target.Value)
                );

                TypeGroup tg = target.Value as TypeGroup;
                Type nonGen;
                if (tg == null || tg.TryGetNonGenericType(out nonGen)) {
                    members = GetMember(act, ((TypeTracker)target.Value).Type, getMemInfo.Name);
                    if (members.Count > 0) {
                        // we have a member that's on the type associated w/ the tracker, return that...
                        type = ((TypeTracker)target.Value).Type;
                        self = null;
                    }
                }
            }

            if (members.Count == 0) {
                // Get the members
                members = GetMember(act, type, getMemInfo.Name);
            }

            Expression propSelf = self;
            // if lookup failed try the strong-box type if available.
            if (members.Count == 0 && typeof(IStrongBox).IsAssignableFrom(type)) {
                // properties/fields need the direct value, methods hold onto the strong box.
                propSelf = Ast.Field(Ast.ConvertHelper(self, type), type.GetField("Value"));

                type = type.GetGenericArguments()[0];
                
                members = GetMember(
                    act, 
                    type, 
                    getMemInfo.Name
                );
            }

            MakeBodyHelper(getMemInfo, self, propSelf, type, members);

            getMemInfo.Body.Restrictions = restrictions;
            return getMemInfo.Body.GetMetaObject(target);
        }

        private void MakeBodyHelper(GetMemberInfo/*!*/ getMemInfo, Expression self, Expression propSelf, Type/*!*/ type, MemberGroup/*!*/ members) {
            if (self != null) {
                MakeOperatorGetMemberBody(getMemInfo, self, type, "GetCustomMember");
            }

            Expression error;
            TrackerTypes memberType = GetMemberType(members, out error);
            
            if (error == null) {
                MakeSuccessfulMemberAccess(getMemInfo, self, propSelf, type, members, memberType);
            } else {
                getMemInfo.Body.FinishCondition(error);
            }
        }

        private void MakeSuccessfulMemberAccess(GetMemberInfo/*!*/ getMemInfo, Expression self, Expression propSelf, Type/*!*/ type, MemberGroup/*!*/ members, TrackerTypes memberType) {
            switch (memberType) {
                case TrackerTypes.TypeGroup:
                case TrackerTypes.Type:
                    MakeTypeBody(getMemInfo, type, members);
                    break;
                case TrackerTypes.Method:
                    // turn into a MethodGroup                    
                    MakeGenericBodyWorker(getMemInfo, type, ReflectionCache.GetMethodGroup(getMemInfo.Name, members), self);
                    break;
                case TrackerTypes.Event:
                case TrackerTypes.Field:
                case TrackerTypes.Property:
                case TrackerTypes.Constructor:
                case TrackerTypes.Custom:
                    MakeGenericBody(getMemInfo, type, members, propSelf);
                    break;
                case TrackerTypes.All:
                    // no members were found
                    if (self != null) {
                        MakeOperatorGetMemberBody(getMemInfo, self, type, "GetBoundMember");
                    }

                    MakeMissingMemberRuleForGet(getMemInfo, type);
                    break;
                default:
                    throw new InvalidOperationException(memberType.ToString());
            }
        }

        private void MakeGenericBody(GetMemberInfo/*!*/ getMemInfo, Type/*!*/ type, MemberGroup/*!*/ members, Expression instance) {
            MakeGenericBodyWorker(getMemInfo, type, members[0], instance);
        }

        private void MakeTypeBody(GetMemberInfo/*!*/ getMemInfo, Type/*!*/ type, MemberGroup/*!*/ members) {
            TypeTracker typeTracker = (TypeTracker)members[0];
            for (int i = 1; i < members.Count; i++) {
                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
            }

            getMemInfo.Body.FinishCondition(typeTracker.GetValue(getMemInfo.CodeContext, this, type));
        }

        private void MakeGenericBodyWorker(GetMemberInfo/*!*/ getMemInfo, Type/*!*/ type, MemberTracker/*!*/ tracker, Expression instance) {
            if (instance != null) {
                tracker = tracker.BindToInstance(instance);
            }

            Expression val = tracker.GetValue(getMemInfo.CodeContext, this, type);

            getMemInfo.Body.FinishCondition(
                val != null ? 
                    val :
                    MakeError(tracker.GetError(this))
            );
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private void MakeOperatorGetMemberBody(GetMemberInfo/*!*/ getMemInfo, Expression instance, Type/*!*/ type, string/*!*/ name) {
            MethodInfo getMem = GetMethod(type, name);
            if (getMem != null && getMem.IsSpecialName) {
                VariableExpression tmp = Ast.Variable(typeof(object), "getVal");
                getMemInfo.Body.AddVariable(tmp);

                getMemInfo.Body.AddCondition(                    
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            MakeCallExpression(
                                getMemInfo.CodeContext,
                                getMem, 
                                instance, 
                                Ast.Constant(getMemInfo.Name)
                            )
                        ),
                        Ast.Field(null, typeof(OperationFailed).GetField("Value"))
                    ),
                    tmp
                );
            }
        }

        private void MakeMissingMemberRuleForGet(GetMemberInfo/*!*/ getMemInfo, Type/*!*/ type) {
            if (getMemInfo.IsNoThrow) {
                getMemInfo.Body.FinishCondition(
                    Ast.Field(null, typeof(OperationFailed).GetField("Value"))
                );
            } else {
                getMemInfo.Body.FinishCondition(
                    MakeError(MakeMissingMemberError(type, getMemInfo.Name))
                );
            }
        }


        /// <summary>
        /// Helper class for flowing information about the GetMember request.
        /// </summary>
        private sealed class GetMemberInfo {
            public readonly string/*!*/ Name;
            public readonly Expression/*!*/ CodeContext;
            public readonly bool IsNoThrow;
            public readonly ConditionalBuilder/*!*/ Body = new ConditionalBuilder();

            public GetMemberInfo(string name, Expression/*!*/ codeContext, bool noThrow) {
                Name = name;
                CodeContext = codeContext;
                IsNoThrow = noThrow;
            }
        }
    }
}
