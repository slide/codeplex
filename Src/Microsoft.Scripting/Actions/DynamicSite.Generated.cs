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
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting;

namespace Microsoft.Scripting.Actions { 
    #region Generated DynamicSites

    // *** BEGIN GENERATED CODE ***

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity 1
    /// </summary>
    public delegate Tret DynamicSiteTarget<T0, Tret>(DynamicSite<T0, Tret> site, CodeContext context, T0 arg0) ;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity 1
    /// </summary>
    public class DynamicSite<T0, Tret> : DynamicSite  {
        private DynamicSiteTarget<T0, Tret> _target;
        private RuleSet<DynamicSiteTarget<T0, Tret>> _rules;

        public DynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<DynamicSiteTarget<T0, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0) {
            Validate(context);
            return _target(this, context, arg0);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0) {
            StandardRule<DynamicSiteTarget<T0, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<DynamicSiteTarget<T0, Tret>>(Action, new object[] { arg0 });

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<DynamicSiteTarget<T0, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                DynamicSiteTarget<T0, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<DynamicSiteTarget<T0, Tret>>.FixedInstance) return newTarget(this, context, arg0);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity 1
    /// </summary>
    public delegate Tret FastDynamicSiteTarget<T0, Tret>(FastDynamicSite<T0, Tret> site, T0 arg0) ;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity 1
    /// </summary>
    public class FastDynamicSite<T0, Tret> : FastDynamicSite  {
        private FastDynamicSiteTarget<T0, Tret> _target;
        private RuleSet<FastDynamicSiteTarget<T0, Tret>> _rules;

        public FastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<FastDynamicSiteTarget<T0, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0) {
            return _target(this, arg0);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0) {
            StandardRule<FastDynamicSiteTarget<T0, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<T0, Tret>>(Action, new object[] { arg0 });

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<FastDynamicSiteTarget<T0, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                FastDynamicSiteTarget<T0, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0);
        }
    }

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity 2
    /// </summary>
    public delegate Tret DynamicSiteTarget<T0, T1, Tret>(DynamicSite<T0, T1, Tret> site, CodeContext context, T0 arg0, T1 arg1) ;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity 2
    /// </summary>
    public class DynamicSite<T0, T1, Tret> : DynamicSite  {
        private DynamicSiteTarget<T0, T1, Tret> _target;
        private RuleSet<DynamicSiteTarget<T0, T1, Tret>> _rules;

        public DynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<DynamicSiteTarget<T0, T1, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0, T1 arg1) {
            Validate(context);
            return _target(this, context, arg0, arg1);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0, T1 arg1) {
            StandardRule<DynamicSiteTarget<T0, T1, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<DynamicSiteTarget<T0, T1, Tret>>(Action, new object[] { arg0, arg1 });

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0, arg1);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<DynamicSiteTarget<T0, T1, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                DynamicSiteTarget<T0, T1, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<DynamicSiteTarget<T0, T1, Tret>>.FixedInstance) return newTarget(this, context, arg0, arg1);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0, arg1);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity 2
    /// </summary>
    public delegate Tret FastDynamicSiteTarget<T0, T1, Tret>(FastDynamicSite<T0, T1, Tret> site, T0 arg0, T1 arg1) ;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity 2
    /// </summary>
    public class FastDynamicSite<T0, T1, Tret> : FastDynamicSite  {
        private FastDynamicSiteTarget<T0, T1, Tret> _target;
        private RuleSet<FastDynamicSiteTarget<T0, T1, Tret>> _rules;

        public FastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<FastDynamicSiteTarget<T0, T1, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0, T1 arg1) {
            return _target(this, arg0, arg1);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0, T1 arg1) {
            StandardRule<FastDynamicSiteTarget<T0, T1, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<T0, T1, Tret>>(Action, new object[] { arg0, arg1 });

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0, arg1);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<FastDynamicSiteTarget<T0, T1, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                FastDynamicSiteTarget<T0, T1, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0, arg1);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0, arg1);
        }
    }

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity 3
    /// </summary>
    public delegate Tret DynamicSiteTarget<T0, T1, T2, Tret>(DynamicSite<T0, T1, T2, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2) ;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity 3
    /// </summary>
    public class DynamicSite<T0, T1, T2, Tret> : DynamicSite  {
        private DynamicSiteTarget<T0, T1, T2, Tret> _target;
        private RuleSet<DynamicSiteTarget<T0, T1, T2, Tret>> _rules;

        public DynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<DynamicSiteTarget<T0, T1, T2, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
            Validate(context);
            return _target(this, context, arg0, arg1, arg2);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
            StandardRule<DynamicSiteTarget<T0, T1, T2, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<DynamicSiteTarget<T0, T1, T2, Tret>>(Action, new object[] { arg0, arg1, arg2 });

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0, arg1, arg2);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<DynamicSiteTarget<T0, T1, T2, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                DynamicSiteTarget<T0, T1, T2, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<DynamicSiteTarget<T0, T1, T2, Tret>>.FixedInstance) return newTarget(this, context, arg0, arg1, arg2);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0, arg1, arg2);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity 3
    /// </summary>
    public delegate Tret FastDynamicSiteTarget<T0, T1, T2, Tret>(FastDynamicSite<T0, T1, T2, Tret> site, T0 arg0, T1 arg1, T2 arg2) ;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity 3
    /// </summary>
    public class FastDynamicSite<T0, T1, T2, Tret> : FastDynamicSite  {
        private FastDynamicSiteTarget<T0, T1, T2, Tret> _target;
        private RuleSet<FastDynamicSiteTarget<T0, T1, T2, Tret>> _rules;

        public FastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<FastDynamicSiteTarget<T0, T1, T2, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0, T1 arg1, T2 arg2) {
            return _target(this, arg0, arg1, arg2);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0, T1 arg1, T2 arg2) {
            StandardRule<FastDynamicSiteTarget<T0, T1, T2, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<T0, T1, T2, Tret>>(Action, new object[] { arg0, arg1, arg2 });

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0, arg1, arg2);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<FastDynamicSiteTarget<T0, T1, T2, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                FastDynamicSiteTarget<T0, T1, T2, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0, arg1, arg2);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0, arg1, arg2);
        }
    }

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity 4
    /// </summary>
    public delegate Tret DynamicSiteTarget<T0, T1, T2, T3, Tret>(DynamicSite<T0, T1, T2, T3, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) ;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity 4
    /// </summary>
    public class DynamicSite<T0, T1, T2, T3, Tret> : DynamicSite  {
        private DynamicSiteTarget<T0, T1, T2, T3, Tret> _target;
        private RuleSet<DynamicSiteTarget<T0, T1, T2, T3, Tret>> _rules;

        public DynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<DynamicSiteTarget<T0, T1, T2, T3, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            Validate(context);
            return _target(this, context, arg0, arg1, arg2, arg3);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            StandardRule<DynamicSiteTarget<T0, T1, T2, T3, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<DynamicSiteTarget<T0, T1, T2, T3, Tret>>(Action, new object[] { arg0, arg1, arg2, arg3 });

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0, arg1, arg2, arg3);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<DynamicSiteTarget<T0, T1, T2, T3, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                DynamicSiteTarget<T0, T1, T2, T3, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<DynamicSiteTarget<T0, T1, T2, T3, Tret>>.FixedInstance) return newTarget(this, context, arg0, arg1, arg2, arg3);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0, arg1, arg2, arg3);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity 4
    /// </summary>
    public delegate Tret FastDynamicSiteTarget<T0, T1, T2, T3, Tret>(FastDynamicSite<T0, T1, T2, T3, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3) ;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity 4
    /// </summary>
    public class FastDynamicSite<T0, T1, T2, T3, Tret> : FastDynamicSite  {
        private FastDynamicSiteTarget<T0, T1, T2, T3, Tret> _target;
        private RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, Tret>> _rules;

        public FastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            return _target(this, arg0, arg1, arg2, arg3);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            StandardRule<FastDynamicSiteTarget<T0, T1, T2, T3, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<T0, T1, T2, T3, Tret>>(Action, new object[] { arg0, arg1, arg2, arg3 });

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0, arg1, arg2, arg3);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                FastDynamicSiteTarget<T0, T1, T2, T3, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0, arg1, arg2, arg3);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0, arg1, arg2, arg3);
        }
    }

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity 5
    /// </summary>
    public delegate Tret DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>(DynamicSite<T0, T1, T2, T3, T4, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) ;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity 5
    /// </summary>
    public class DynamicSite<T0, T1, T2, T3, T4, Tret> : DynamicSite  {
        private DynamicSiteTarget<T0, T1, T2, T3, T4, Tret> _target;
        private RuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>> _rules;

        public DynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            Validate(context);
            return _target(this, context, arg0, arg1, arg2, arg3, arg4);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            StandardRule<DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>>(Action, new object[] { arg0, arg1, arg2, arg3, arg4 });

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0, arg1, arg2, arg3, arg4);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                DynamicSiteTarget<T0, T1, T2, T3, T4, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, Tret>>.FixedInstance) return newTarget(this, context, arg0, arg1, arg2, arg3, arg4);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0, arg1, arg2, arg3, arg4);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity 5
    /// </summary>
    public delegate Tret FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret>(FastDynamicSite<T0, T1, T2, T3, T4, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) ;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity 5
    /// </summary>
    public class FastDynamicSite<T0, T1, T2, T3, T4, Tret> : FastDynamicSite  {
        private FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret> _target;
        private RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret>> _rules;

        public FastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            return _target(this, arg0, arg1, arg2, arg3, arg4);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            StandardRule<FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret>>(Action, new object[] { arg0, arg1, arg2, arg3, arg4 });

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0, arg1, arg2, arg3, arg4);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                FastDynamicSiteTarget<T0, T1, T2, T3, T4, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0, arg1, arg2, arg3, arg4);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0, arg1, arg2, arg3, arg4);
        }
    }

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity 6
    /// </summary>
    public delegate Tret DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>(DynamicSite<T0, T1, T2, T3, T4, T5, Tret> site, CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) ;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity 6
    /// </summary>
    public class DynamicSite<T0, T1, T2, T3, T4, T5, Tret> : DynamicSite  {
        private DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret> _target;
        private RuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>> _rules;

        public DynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            Validate(context);
            return _target(this, context, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            StandardRule<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>>(Action, new object[] { arg0, arg1, arg2, arg3, arg4, arg5 });

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0, arg1, arg2, arg3, arg4, arg5);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>>.FixedInstance) return newTarget(this, context, arg0, arg1, arg2, arg3, arg4, arg5);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0, arg1, arg2, arg3, arg4, arg5);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity 6
    /// </summary>
    public delegate Tret FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>(FastDynamicSite<T0, T1, T2, T3, T4, T5, Tret> site, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) ;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity 6
    /// </summary>
    public class FastDynamicSite<T0, T1, T2, T3, T4, T5, Tret> : FastDynamicSite  {
        private FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret> _target;
        private RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>> _rules;

        public FastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            return _target(this, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            StandardRule<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>>(Action, new object[] { arg0, arg1, arg2, arg3, arg4, arg5 });

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0, arg1, arg2, arg3, arg4, arg5);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                FastDynamicSiteTarget<T0, T1, T2, T3, T4, T5, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0, arg1, arg2, arg3, arg4, arg5);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0, arg1, arg2, arg3, arg4, arg5);
        }
    }

    /// <summary>
    /// Dynamic site delegate type with CodeContext passed in - arity variable based on Tuple size
    /// </summary>
    public delegate Tret BigDynamicSiteTarget<T0, Tret>(BigDynamicSite<T0, Tret> site, CodeContext context, T0 arg0) where T0 : NewTuple;

    /// <summary>
    /// Dynamic site using CodeContext passed into the Invoke method - arity variable based on Tuple size
    /// </summary>
    public class BigDynamicSite<T0, Tret> : DynamicSite where T0 : NewTuple {
        private BigDynamicSiteTarget<T0, Tret> _target;
        private RuleSet<BigDynamicSiteTarget<T0, Tret>> _rules;

        public BigDynamicSite(Action action)
            : base(action) {
            this._rules = RuleSet<BigDynamicSiteTarget<T0, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(CodeContext context, T0 arg0) {
            Validate(context);
            return _target(this, context, arg0);
        }

        public Tret UpdateBindingAndInvoke(CodeContext context, T0 arg0) {
            StandardRule<BigDynamicSiteTarget<T0, Tret>> rule = 
              context.LanguageContext.Binder.GetRule<BigDynamicSiteTarget<T0, Tret>>(Action, NewTuple.GetTupleValues(arg0));

    #if DEBUG
            if (context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(context.Scope, arg0);
                bool result = (bool)rule.Test.Evaluate(context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(context);
            }
    #endif
            RuleSet<BigDynamicSiteTarget<T0, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                BigDynamicSiteTarget<T0, Tret> newTarget = newRules.GetOrMakeTarget(context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                if (newRules != EmptyRuleSet<BigDynamicSiteTarget<T0, Tret>>.FixedInstance) return newTarget(this, context, arg0);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(context)(this, context, arg0);
        }
    }

    /// <summary>
    /// Dynamic site delegate type using cached CodeContext - arity variable based on Tuple size
    /// </summary>
    public delegate Tret BigFastDynamicSiteTarget<T0, Tret>(BigFastDynamicSite<T0, Tret> site, T0 arg0) where T0 : NewTuple;

    /// <summary>
    /// Dynamic site using cached CodeContext - arity variable based on Tuple size
    /// </summary>
    public class BigFastDynamicSite<T0, Tret> : FastDynamicSite where T0 : NewTuple {
        private BigFastDynamicSiteTarget<T0, Tret> _target;
        private RuleSet<BigFastDynamicSiteTarget<T0, Tret>> _rules;

        public BigFastDynamicSite(CodeContext context, Action action)
            : base(context, action) {
            this._rules = RuleSet<BigFastDynamicSiteTarget<T0, Tret>>.EmptyRules;
            this._target = this._rules.GetOrMakeTarget(null);
        }

        public Tret Invoke(T0 arg0) {
            return _target(this, arg0);
        }

        public Tret UpdateBindingAndInvoke(T0 arg0) {
            StandardRule<BigFastDynamicSiteTarget<T0, Tret>> rule = 
              Context.LanguageContext.Binder.GetRule<BigFastDynamicSiteTarget<T0, Tret>>(Action, NewTuple.GetTupleValues(arg0));

    #if DEBUG
            if (Context.LanguageContext.Engine.Options.FastEvaluation) {
                DynamicSiteHelpers.InsertArguments(Context.Scope, arg0);
                bool result = (bool)rule.Test.Evaluate(Context);
                Debug.Assert(result);
                return (Tret)rule.Target.Execute(Context);
            }
    #endif
            RuleSet<BigFastDynamicSiteTarget<T0, Tret>> newRules = _rules.AddRule(rule);
            if (newRules != _rules) {
                BigFastDynamicSiteTarget<T0, Tret> newTarget = newRules.GetOrMakeTarget(Context);
                lock (this) {
                    _rules = newRules;
                    _target = newTarget;
                }
                return newTarget(this, arg0);
            }

            return rule.MonomorphicRuleSet.GetOrMakeTarget(Context)(this, arg0);
        }
    }


    // *** END GENERATED CODE ***

    #endregion
}
