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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Provides binding semantics for a language.  This include conversions as well as support
    /// for producing rules for actions.  These optimized rules are used for calling methods, 
    /// performing operators, and getting members using the ActionBinder's conversion semantics.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract class ActionBinder {
        private CodeContext _context;
        private readonly RuleCache _ruleCache = new RuleCache();

        protected ActionBinder(CodeContext context) {
            _context = context;
        }

        /// <summary>
        /// Deprecated - only used by DelegateSignatureInfo.GenerateDelegateStub.  Use CodeContext
        /// passed in at rule creation time instead.
        /// </summary>
        internal CodeContext Context {
            get {
                return _context;
            }
        }

        // TODO: internal and friendly UnitTests
        public void ClearRuleCache() {
            _ruleCache.Clear();
        }

        /// <summary>
        /// Creates argument array to be used to execute the rule on the match making site.
        /// </summary>
        private static object[] MakeCallArgs(Type tt, Matchmaker mm, object site, CodeContext context, object[] args) {
            // Create the matchmaking site, which will become part of the
            // call argument array;
            object mms = mm.CreateMatchMakingSite(site, tt);

            if (DynamicSiteHelpers.IsBigTarget(tt)) {
                args = new object[] { Tuple.MakeTuple(tt.GetGenericArguments()[0], args) };
            }

            object[] callArgs = new object[args.Length + 2];
            callArgs[0] = mms;
            callArgs[1] = context;
            args.CopyTo(callArgs, 2);
            return callArgs;
        }

        /// <summary>
        /// Gets a rule, updates the site that called, and then returns the result of executing the rule.
        /// </summary>
        /// <typeparam name="T">The type of the DynamicSite the rule is being produced for.</typeparam>
        /// <param name="context">The CodeContext that is requesting the rule and that should be used for conversions.</param>
        /// <param name="site">Dynamic site that needs updating.</param>
        /// <param name="args">The arguments to the rule as provided from the call site at runtime.</param>
        /// <returns>The result of executing the rule.</returns>
        internal object UpdateSiteAndExecute<T>(CodeContext context, CallSite<T> site, object[] args) where T : class {
            //
            // Declare the locals here upfront. It actuall saves JIT stack space.
            //
            Rule<T>[] applicable;
            Rule<T> rule;
            T ruleTarget;
            object result;
            int count, index;

            //
            // Create matchmaker, its site and reflected caller. We'll need them regardless.
            //
            Type typeofT = typeof(T);               // Calculate this only once
            ReflectedCaller rc = ReflectedCaller.Create(typeofT.GetMethod("Invoke"));
            Matchmaker mm = new Matchmaker();

            //
            // Create the arguments to use to call the matchmaker site
            //
            object[] callArgs = MakeCallArgs(typeofT, mm, site, context, args);

            //
            // Capture the site's rule set. Since it can change on the site asynchronously,
            // this ensures that we work with the same rule set throughout this function.
            //
            RuleSet<T> siteRules = site.Rules;

            //
            // Look in the site's local cache first, if we have rules, run those.
            // If we find match, the site is polymorphic. Update the delegate in that
            // case and keep running.
            //
            IList<Rule<T>> history = siteRules.GetRules();

            if (history != null) {
                count = history.Count;
                for (index = 0; index < count; index ++) {
                    rule = history[index];

                    if (!rule.IsValid) {
                        continue;
                    }

                    mm.Reset();

                    //
                    // Execute the rule
                    //
                    site._target = ruleTarget = rule.MonomorphicRuleSet.GetOrMakeTarget();

                    try {
                        result = rc.InvokeInstance(ruleTarget, callArgs);
                    } catch (TargetInvocationException e) {
                        throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                    }

                    if (mm.Match) {
                        // Truly polymorphic site (we found the applicable rule in the local cache)
                        if (site._target == ruleTarget) {
                            System.Threading.Interlocked.CompareExchange<T>(ref site._target, siteRules.GetOrMakeTarget(), ruleTarget);
                        }
                        return result;
                    }
                }
            }

            //
            // Look for applicable rules in the caches
            //
            Type[] argTypes = CompilerHelpers.GetTypes(args);
            applicable = _ruleCache.FindApplicableRules<T>(site.Action, argTypes);

            //
            // Do we have rules? If so, run those
            //
            if (applicable != null) {
                count = applicable.Length;
                for (index = 0; index < count; index ++) {
                    rule = applicable[index];

                    mm.Reset();

                    //
                    // Execute the rule
                    //
                    site._target = ruleTarget = rule.MonomorphicRuleSet.GetOrMakeTarget();

                    try {
                        result = rc.InvokeInstance(ruleTarget, callArgs);
                    } catch (TargetInvocationException e) {
                        throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                    }

                    if (mm.Match) {
                        site.AddRule(rule);
                        return result;
                    }

                    // Rule didn't match, let's try the next one
                }
            }

            //
            // No rule matched, we need to create one
            //

            for (; ; ) {
                rule = CreateNewRule<T>(context, site.Action, args);

                //
                // Add the rule to the cache. This is an optimistic add so that cache miss
                // on another site can find this existing rule rather than building a new one.
                //
                _ruleCache.AddRule<T>(site.Action, argTypes, rule);

                //
                // Execute the rule on the matchmaker site
                //

                mm.Reset();

                site._target = ruleTarget = rule.MonomorphicRuleSet.GetOrMakeTarget();

                try {
                    result = rc.InvokeInstance(ruleTarget, callArgs);
                } catch (TargetInvocationException e) {
                    throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                }

                if (mm.Match) {
                    //
                    // The rule worked !!!
                    //
                    site.AddRule(rule);
                    return result;
                }

                //
                // The rule didn't work and since we optimistically added it into the cache,
                // let's remove it now since we already know the rule is no good.
                //
                _ruleCache.RemoveRule<T>(site.Action, argTypes, rule);
            }
        }

        private Rule<T> CreateNewRule<T>(CodeContext context, DynamicAction action, object[] args) {
            NoteRuleCreation(action, args);

            RuleBuilder<T> builder = MakeRule<T>(context, action, args);

            //
            // Check the produced rule
            //
            if (builder == null || builder.Target == null || builder.Test == null) {
                throw new InvalidOperationException("No or Invalid rule produced");
            }

            Rule<T> rule = builder.CreateRule();

#if DEBUG
            AstWriter.Dump(rule);
#endif
            return rule;
        }

        [Conditional("DEBUG")]
        private static void NoteRuleCreation(DynamicAction action, object[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Rules, "MakeRule " + action.ToString() + " " + CompilerHelpers.GetType(args[0]).Name);
        }
        
        public virtual AbstractValue AbstractExecute(DynamicAction action, IList<AbstractValue> args) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Produces a rule for the specified Action for the given arguments.
        /// 
        /// The default implementation can produce rules for standard .NET types.  Languages should
        /// override this and provide any custom behavior they need and fallback to the default
        /// implementation if no custom behavior is required.
        /// </summary>
        /// <typeparam name="T">The type of the DynamicSite the rule is being produced for.</typeparam>
        /// <param name="action">The Action that is being performed.</param>
        /// <param name="args">The arguments to the action as provided from the call site at runtime.</param>
        /// <param name="callerContext">The CodeContext that is requesting the rule and should be use</param>
        /// <returns></returns>
        protected virtual RuleBuilder<T> MakeRule<T>(CodeContext/*!*/ callerContext, DynamicAction/*!*/ action, object[]/*!*/ args) {
            ContractUtils.RequiresNotNull(callerContext, "callerContext");
            ContractUtils.RequiresNotNull(action, "action");
            ContractUtils.RequiresNotNull(args, "args");

            switch (action.Kind) {
                case DynamicActionKind.Call:
                    return new CallBinderHelper<T, CallAction>(callerContext, (CallAction)action, args).MakeRule();
                case DynamicActionKind.GetMember:
                    return new GetMemberBinderHelper<T>(callerContext, (GetMemberAction)action, args).MakeNewRule();
                case DynamicActionKind.SetMember:
                    return new SetMemberBinderHelper<T>(callerContext, (SetMemberAction)action, args).MakeNewRule();
                case DynamicActionKind.CreateInstance:
                    return new CreateInstanceBinderHelper<T>(callerContext, (CreateInstanceAction)action, args).MakeRule();
                case DynamicActionKind.DoOperation:
                    return new DoOperationBinderHelper<T>(callerContext, (DoOperationAction)action, args).MakeRule();
                case DynamicActionKind.DeleteMember:
                    return new DeleteMemberBinderHelper<T>(callerContext, (DeleteMemberAction)action, args).MakeRule();
                case DynamicActionKind.InvokeMember:
                    return new InvokeMemberBinderHelper<T>(callerContext, (InvokeMemberAction)action, args).MakeRule();
                case DynamicActionKind.ConvertTo:
                    return new ConvertToBinderHelper<T>(callerContext, (ConvertToAction)action, args).MakeRule();
                default:
                    throw new NotImplementedException(action.ToString());
            }
        }

        /// <summary>
        /// Converts an object at runtime into the specified type.
        /// </summary>
        public virtual object Convert(object obj, Type toType) {
            if (obj == null) {
                if (!toType.IsValueType) {
                    return null;
                }
            } else {
                if (toType.IsValueType) {
                    if (toType == obj.GetType()) {
                        return obj;
                    }
                } else {
                    if (toType.IsAssignableFrom(obj.GetType())) {
                        return obj;
                    }
                }
            }
            throw new InvalidCastException(String.Format("Cannot convert {0} to {1}", obj != null ? obj.GetType().Name : "(null)", toType.Name));
        }

        /// <summary>
        /// Determines if a conversion exists from fromType to toType at the specified narrowing level.
        /// </summary>
        public abstract bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel level);

        /// <summary>
        /// Selects the best (of two) candidates for conversion from actualType
        /// </summary>
        public virtual Type SelectBestConversionFor(Type actualType, Type candidateOne, Type candidateTwo, NarrowingLevel level) {
            return null;
        }

        /// <summary>
        /// Provides ordering for two parameter types if there is no conversion between the two parameter types.
        /// 
        /// Returns true to select t1, false to select t2.
        /// </summary>
        public abstract bool PreferConvert(Type t1, Type t2);


        /// <summary>
        /// Converts the provided expression to the given type.  The expression is safe to evaluate multiple times.
        /// </summary>
        public virtual Expression/*!*/ ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType) {
            ContractUtils.RequiresNotNull(expr, "expr");
            ContractUtils.RequiresNotNull(toType, "toType");

            Type exprType = expr.Type;

            if (toType == typeof(object)) {
                if (exprType.IsValueType) {
                    return Ast.Expression.Convert(expr, toType);
                } else {
                    return expr;
                }
            }

            if (toType.IsAssignableFrom(exprType)) {
                return expr;
            }

            Type visType = CompilerHelpers.GetVisibleType(toType);
            return Ast.Expression.Action.ConvertTo(
                ConvertToAction.Make(this, visType, ConversionResultKind.ExplicitCast),
                expr);
        }

        /// <summary>
        /// Gets the return value when an object contains out / by-ref parameters.  
        /// </summary>
        /// <param name="args">The values of by-ref and out parameters that the called method produced.  This includes the normal return
        /// value if the method does not return void.</param>
        public virtual object GetByRefArray(object[] args) {
            return args;
        }

        /// <summary>
        /// Gets the members that are visible from the provided type of the specified name.
        /// 
        /// The default implemetnation first searches the type, then the flattened heirachy of the type, and then
        /// registered extension methods.
        /// </summary>
        public virtual MemberGroup GetMember(DynamicAction action, Type type, string name) {
            MemberInfo[] foundMembers = type.GetMember(name);
            if (!Context.LanguageContext.DomainManager.GlobalOptions.PrivateBinding) {
                foundMembers = CompilerHelpers.FilterNonVisibleMembers(type, foundMembers);
            }

            MemberGroup members = new MemberGroup(foundMembers);

            // check for generic types w/ arity...
            Type[] types = type.GetNestedTypes(BindingFlags.Public);
            string genName = name + ReflectionUtils.GenericArityDelimiter;
            List<Type> genTypes = null;
            foreach (Type t in types) {
                if (t.Name.StartsWith(genName)) {
                    if (genTypes == null) genTypes = new List<Type>();
                    genTypes.Add(t);
                }
            }

            if (genTypes != null) {
                List<MemberTracker> mt = new List<MemberTracker>(members);
                foreach (Type t in genTypes) {
                    mt.Add(MemberTracker.FromMemberInfo(t));
                }
                return MemberGroup.CreateInternal(mt.ToArray());
            }
            
            if (members.Count == 0) {
                members = new MemberGroup(type.GetMember(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
                if (members.Count == 0) {
                    members = GetAllExtensionMembers(type, name);
                }
            }
            
            return members;
        }
        
        #region Error Production

        public virtual ErrorInfo MakeContainsGenericParametersError(MemberTracker tracker) {
            return ErrorInfo.FromException(
                Ast.Expression.New(
                    typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Expression.Constant(ResourceUtils.GetString(ResourceUtils.InvalidOperation_ContainsGenericParameters, tracker.DeclaringType.Name, tracker.Name))
                )
            );
        }

        public virtual ErrorInfo MakeMissingMemberErrorInfo(Type type, string name) {
            return ErrorInfo.FromException(
                Ast.Expression.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Expression.Constant(name)
                )
            );
        }

        public virtual ErrorInfo MakeGenericAccessError(MemberTracker info) {
            return ErrorInfo.FromException(
                Ast.Expression.New(
                    typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Expression.Constant(info.Name)
                )
            );
        }

        public ErrorInfo MakeStaticPropertyInstanceAccessError(PropertyTracker/*!*/ tracker, bool isAssignment, params Expression[]/*!*/ parameters) {
            return MakeStaticPropertyInstanceAccessError(tracker, isAssignment, (IList<Expression>)parameters);
        }

        /// <summary>
        /// Called when a set is attempting to assign to a field or property from a derived class through the base class.
        /// 
        /// The default behavior is to allow the assignment.
        /// </summary>
        public virtual ErrorInfo MakeStaticAssignFromDerivedTypeError(Type accessingType, MemberTracker assigning, Expression assignedValue) {
            switch (assigning.MemberType) {
                case TrackerTypes.Property:
                    PropertyTracker pt = (PropertyTracker)assigning;
                    MethodInfo setter = pt.GetSetMethod() ?? pt.GetSetMethod(true);
                    return ErrorInfo.FromValueNoError(
                        Ast.Expression.SimpleCallHelper(
                            setter,
                            ConvertExpression(
                                assignedValue,
                                setter.GetParameters()[0].ParameterType
                            )
                        )
                    );
                case TrackerTypes.Field:
                    FieldTracker ft = (FieldTracker)assigning;
                    return ErrorInfo.FromValueNoError(
                        Ast.Expression.AssignField(
                            null,
                            ft.Field,
                            ConvertExpression(assignedValue, ft.FieldType)
                        )
                    );
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Creates an ErrorInfo object when a static property is accessed from an instance member.  The default behavior is throw
        /// an exception indicating that static members properties be accessed via an instance.  Languages can override this to 
        /// customize the exception, message, or to produce an ErrorInfo object which reads or writes to the property being accessed.
        /// </summary>
        /// <param name="tracker">The static property being accessed through an instance</param>
        /// <param name="isAssignment">True if the user is assigning to the property, false if the user is reading from the property</param>
        /// <param name="parameters">The parameters being used to access the property.  This includes the instance as the first entry, any index parameters, and the
        /// value being assigned as the last entry if isAssignment is true.</param>
        /// <returns></returns>
        public virtual ErrorInfo MakeStaticPropertyInstanceAccessError(PropertyTracker/*!*/ tracker, bool isAssignment, IList<Expression>/*!*/ parameters) {
            ContractUtils.RequiresNotNull(tracker, "tracker");
            ContractUtils.Requires(tracker.IsStatic, "expected only static property");
            ContractUtils.RequiresNotNull(parameters, "parameters");
            ContractUtils.RequiresNotNullItems(parameters, "parameters");

            return ErrorInfo.FromException(
                Ast.Expression.Call(
                    typeof(BinderOps).GetMethod("StaticAssignmentFromInstanceError"),
                    Ast.Expression.RuntimeConstant(tracker),
                    Ast.Expression.Constant(isAssignment)
                )
            );
        }

        public virtual ErrorInfo MakeInvalidParametersError(BindingTarget target) {
            switch (target.Result) {
                case BindingResult.CallFailure:            return MakeCallFailureError(target);
                case BindingResult.AmbigiousMatch:         return MakeAmbigiousCallError(target);
                case BindingResult.IncorrectArgumentCount: return MakeIncorrectArgumentCountError(target);
                default: throw new InvalidOperationException();
            }
        }

        private static ErrorInfo MakeIncorrectArgumentCountError(BindingTarget target) {
            int minArgs = Int32.MaxValue;
            int maxArgs = Int32.MinValue;
            foreach (int argCnt in target.ExpectedArgumentCount) {
                minArgs = System.Math.Min(minArgs, argCnt);
                maxArgs = System.Math.Max(maxArgs, argCnt);
            }

            return ErrorInfo.FromException(
                Ast.Expression.Call(
                    typeof(RuntimeHelpers).GetMethod("TypeErrorForIncorrectArgumentCount", new Type[] {
                                typeof(string), typeof(int), typeof(int) , typeof(int), typeof(int), typeof(bool), typeof(bool)
                            }),
                    Ast.Expression.Constant(target.Name, typeof(string)),  // name
                    Ast.Expression.Constant(minArgs),                      // min formal normal arg cnt
                    Ast.Expression.Constant(maxArgs),                      // max formal normal arg cnt
                    Ast.Expression.Constant(0),                            // default cnt
                    Ast.Expression.Constant(target.ActualArgumentCount),   // args provided
                    Ast.Expression.Constant(false),                        // hasArgList
                    Ast.Expression.Constant(false)                         // kwargs provided
                )
            );
        }

        private ErrorInfo MakeAmbigiousCallError(BindingTarget target) {
            StringBuilder sb = new StringBuilder("Multiple targets could match: ");
            string outerComma = "";
            foreach (MethodTarget mt in target.AmbigiousMatches) {
                Type[] types = mt.GetParameterTypes();
                string innerComma = "";

                sb.Append(outerComma);
                sb.Append(target.Name);
                sb.Append('(');
                foreach (Type t in types) {
                    sb.Append(innerComma);
                    sb.Append(GetTypeName(t));
                    innerComma = ", ";
                }

                sb.Append(')');
                outerComma = ", ";
            }

            return ErrorInfo.FromException(
                Ast.Expression.Call(
                    typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                    Ast.Expression.Constant(sb.ToString(), typeof(string))
                )
            );
        }

        private ErrorInfo MakeCallFailureError(BindingTarget target) {
            foreach (CallFailure cf in target.CallFailures) {
                switch (cf.Reason) {
                    case CallFailureReason.ConversionFailure:
                        foreach (ConversionResult cr in cf.ConversionResults) {
                            if (cr.Failed) {
                                return ErrorInfo.FromException(
                                    Ast.Expression.Call(
                                        typeof(RuntimeHelpers).GetMethod("SimpleTypeError"),
                                        Ast.Expression.Constant(String.Format("expected {0}, got {1}", GetTypeName(cr.To), GetTypeName(cr.From)))
                                    )
                                );
                            }
                        }
                        break;
                    case CallFailureReason.DuplicateKeyword:
                        return ErrorInfo.FromException(
                                Ast.Expression.Call(
                                    typeof(RuntimeHelpers).GetMethod("TypeErrorForDuplicateKeywordArgument"),
                                    Ast.Expression.Constant(target.Name, typeof(string)),
                                    Ast.Expression.Constant(SymbolTable.IdToString(cf.KeywordArguments[0]), typeof(string))    // TODO: Report all bad arguments?
                            )
                        );
                    case CallFailureReason.UnassignableKeyword:
                        return ErrorInfo.FromException(
                                Ast.Expression.Call(
                                    typeof(RuntimeHelpers).GetMethod("TypeErrorForExtraKeywordArgument"),
                                    Ast.Expression.Constant(target.Name, typeof(string)),
                                    Ast.Expression.Constant(SymbolTable.IdToString(cf.KeywordArguments[0]), typeof(string))    // TODO: Report all bad arguments?
                            )
                        );
                    default: throw new InvalidOperationException();
                }
            }
            throw new InvalidOperationException();
        }

        public virtual ErrorInfo MakeConversionError(Type toType, Expression value) {            
            return ErrorInfo.FromException(
                Ast.Expression.Call(
                    typeof(RuntimeHelpers).GetMethod("CannotConvertError"),
                    Ast.Expression.RuntimeConstant(toType),
                    Ast.Expression.Convert(
                        value,
                        typeof(object)
                    )
               )
            );
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// 
        /// Deprecated, use the non-generic version instead
        /// </summary>
        public virtual ErrorInfo MakeMissingMemberError(Type type, string name) {
            return ErrorInfo.FromException(
                Ast.Expression.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Expression.Constant(name)
                )
            );
        }
        
        
        /// <summary>
        /// Checks to see if the language allows keyword arguments to be bound to instance fields or
        /// properties and turned into sets.  By default this is only allowed on contructors.
        /// </summary>
        protected internal virtual bool AllowKeywordArgumentSetting(MethodBase method) {
            return CompilerHelpers.IsConstructor(method);
        }

        #endregion

        #region Deprecated Error production

        
        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public virtual Expression MakeReadOnlyMemberError<T>(RuleBuilder<T> rule, Type type, string name) {
            return rule.MakeError(
                Ast.Expression.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    Ast.Expression.Constant(name)
                )
            );
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public virtual Expression MakeUndeletableMemberError<T>(RuleBuilder<T> rule, Type type, string name) {
            return MakeReadOnlyMemberError<T>(rule, type, name);
        }

        #endregion

        public virtual ErrorInfo MakeEventValidation(RuleBuilder rule, MemberGroup members) {
            EventTracker ev = (EventTracker)members[0];

            // handles in place addition of events - this validates the user did the right thing.
            return ErrorInfo.FromValueNoError(
                Ast.Expression.Call(
                    typeof(BinderOps).GetMethod("SetEvent"),
                    Ast.Expression.RuntimeConstant(ev),
                    rule.Parameters[1]
                )
            );
        }

        protected virtual string GetTypeName(Type t) {
            return t.Name;
        }       
        
        /// <summary>
        /// Gets the extension members of the given name from the provided type.  Base classes are also
        /// searched for their extension members.  Once any of the types in the inheritance hierarchy
        /// provide an extension member the search is stopped.
        /// </summary>
        public MemberGroup GetAllExtensionMembers(Type type, string name) {
            Type curType = type;
            do {
                MemberGroup res = GetExtensionMembers(curType, name);
                if (res.Count != 0) {
                    return res;
                }

                curType = curType.BaseType;
            } while (curType != null);

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Gets the extension members of the given name from the provided type.  Subclasses of the
        /// type and their extension members are not searched.
        /// </summary>
        public MemberGroup GetExtensionMembers(Type declaringType, string name) {
            IList<Type> extTypes = GetExtensionTypes(declaringType);
            List<MemberTracker> members = new List<MemberTracker>();

            foreach (Type ext in extTypes) {
                foreach (MemberInfo mi in ext.GetMember(name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                    if (ext != declaringType) {
                        members.Add(MemberTracker.FromMemberInfo(mi, declaringType));
                    } else {
                        members.Add(MemberTracker.FromMemberInfo(mi));
                    }
                }

                // TODO: Support indexed getters/setters w/ multiple methods
                MethodInfo getter = null, setter = null, deleter = null;
                foreach (MemberInfo mi in ext.GetMember("Get" + name, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) {
                    if (!mi.IsDefined(typeof(PropertyMethodAttribute), false)) continue;

                    Debug.Assert(getter == null);
                    getter = (MethodInfo)mi;
                }

                foreach (MemberInfo mi in ext.GetMember("Set" + name, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) {
                    if (!mi.IsDefined(typeof(PropertyMethodAttribute), false)) continue;
                    Debug.Assert(setter == null);
                    setter = (MethodInfo)mi;
                }

                foreach (MemberInfo mi in ext.GetMember("Delete" + name, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)) {
                    if (!mi.IsDefined(typeof(PropertyMethodAttribute), false)) continue;
                    Debug.Assert(deleter == null);
                    deleter = (MethodInfo)mi;
                }

                if (getter != null || setter != null || deleter != null) {                    
                    members.Add(new ExtensionPropertyTracker(name, getter, setter, deleter, declaringType));
                }
            }

            if (members.Count != 0) {
                return MemberGroup.CreateInternal(members.ToArray());
            }
            return MemberGroup.EmptyGroup;
        }

        protected internal virtual IList<Type> GetExtensionTypes(Type t) {
            // consult globally registered types
            return RuntimeHelpers.GetExtensionTypes(t);
        }

        /// <summary>
        /// Provides an opportunity for languages to replace all MemberInfo's with their own type.
        /// 
        /// Alternatlely a language can expose MemberInfo's directly.
        /// </summary>
        /// <param name="memberTracker">The member which is being returned to the user.</param>
        /// <param name="type">Tthe type which the memberTrack was accessed from</param>
        /// <returns></returns>
        public virtual Expression ReturnMemberTracker(Type type, MemberTracker memberTracker) {
            if (memberTracker.MemberType == TrackerTypes.Bound) {
                BoundMemberTracker bmt = (BoundMemberTracker)memberTracker;
                return Ast.Expression.New(
                    typeof(BoundMemberTracker).GetConstructor(new Type[] { typeof(MemberTracker), typeof(object) }),
                    Ast.Expression.RuntimeConstant(bmt.BoundTo),
                    bmt.Instance);
            }

            return Ast.Expression.RuntimeConstant(memberTracker);
        }

        /// <summary>
        /// Builds an expressoin for a call to the provided method using the given expressions.  If the
        /// method is not static the first parameter is used for the instance.
        /// 
        /// Parameters are converted using the binder's conversion rules.
        /// 
        /// If an incorrect number of parameters is provided MakeCallExpression returns null.
        /// </summary>
        public Expression MakeCallExpression(MethodInfo method, IList<Expression> parameters) {
            ParameterInfo[] infos = method.GetParameters();
            Expression callInst = null;
            int parameter = 0, startArg = 0;
            Expression[] callArgs = new Expression[infos.Length];

            if (!method.IsStatic) {
                callInst = Ast.Expression.ConvertHelper(parameters[0], method.DeclaringType);
                parameter = 1;
            }
            if (infos.Length > 0 && infos[0].ParameterType == typeof(CodeContext)) {
                startArg = 1;
                callArgs[0] = Ast.Expression.CodeContext();
            }

            for (int arg = startArg; arg < infos.Length; arg++) {
                if (parameter < parameters.Count) {
                    callArgs[arg] = ConvertExpression(
                        parameters[parameter++],
                        infos[arg].ParameterType);
                } else {
                    return null;
                }
            }

            // check that we used all parameters
            if (parameter != parameters.Count) {
                return null;
            }

            return Ast.Expression.SimpleCallHelper(callInst, method, callArgs);
        }


        public Expression MakeCallExpression(MethodInfo method, params Expression[] parameters) {
            return MakeCallExpression(method, (IList<Expression>)parameters);
        }
    }
}

