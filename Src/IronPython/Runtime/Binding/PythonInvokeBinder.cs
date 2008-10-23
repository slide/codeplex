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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Actions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    /// <summary>
    /// The Action used for Python call sites.  This supports both splatting of position and keyword arguments.
    /// 
    /// When a foreign object is encountered the arguments are expanded into normal position/keyword arguments.
    /// </summary>
    class PythonInvokeBinder : MetaObjectBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly CallSignature _signature;

        public PythonInvokeBinder(BinderState/*!*/ binder, CallSignature signature) {
            _state = binder;
            _signature = signature;
        }

        #region MetaAction overrides

        /// <summary>
        /// Python's Invoke is a non-standard action.  Here we first try to bind through a Python
        /// internal interface (IPythonInvokable) which supports CallSigantures.  If that fails
        /// and we have an IDO then we translate to the DLR protocol through a nested dynamic site -
        /// this includes unsplatting any keyword / position arguments.  Finally if it's just a plain
        /// old .NET type we use the default binder which supports CallSignatures.
        /// </summary>
        public override MetaObject/*!*/ Bind(MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            Debug.Assert(args.Length > 0);

            MetaObject cc = target;
            MetaObject actualTarget = args[0];
            args = ArrayUtils.RemoveFirst(args);

            Debug.Assert(cc.LimitType == typeof(CodeContext));

            return BindWorker(cc, actualTarget, args);
        }

        private MetaObject BindWorker(MetaObject/*!*/ context, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            // we don't have CodeContext if an IDO falls back to us when we ask them to produce the Call
            IPythonInvokable icc = target as IPythonInvokable;

            if (icc != null) {
                // call it and provide the context
                return icc.Invoke(
                    this,
                    context.Expression,
                    target,
                    args
                );
            } else if (target.IsDynamicObject) {
                return InvokeForeignObject(target, args);
            }

            return Fallback(context.Expression, target, args);
        }

        /// <summary>
        /// Fallback - performs the default binding operation if the object isn't recognized
        /// as being invokable.
        /// </summary>
        internal MetaObject/*!*/ Fallback(Expression codeContext, MetaObject target, MetaObject/*!*/[]/*!*/ args) {
            if (target.NeedsDeferral()) {
                return Defer(args);
            }

            return PythonProtocol.Call(this, target, args) ??
                Binder.Binder.Create(Signature, new ParameterBinderWithCodeContext(Binder.Binder, codeContext), target, args) ??
                Binder.Binder.Call(Signature, new ParameterBinderWithCodeContext(Binder.Binder, codeContext), target, args);
        }

        public override object/*!*/ CacheIdentity {
            get { return this; }
        }

        #endregion

        #region Object Overrides

        public override int GetHashCode() {
            return _signature.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonInvokeBinder ob = obj as PythonInvokeBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                _signature == ob._signature;
        }

        public override string ToString() {
            return "Python Invoke " + Signature.ToString();
        }

        #endregion

        #region Public API Surface

        /// <summary>
        /// Gets the CallSignature for this invocation which describes how the MetaObject array
        /// is to be mapped.
        /// </summary>
        public CallSignature Signature {
            get {
                return _signature;
            }
        }

        #endregion

        #region Implementation Details

        /// <summary>
        /// Creates a nested dynamic site which uses the unpacked arguments.
        /// </summary>
        protected MetaObject InvokeForeignObject(MetaObject target, MetaObject[] args) {
            // need to unpack any dict / list arguments...
            List<ArgumentInfo> newArgs;
            List<Expression> metaArgs;
            Expression test;
            Restrictions restrictions;
            TranslateArguments(target, args, out newArgs, out metaArgs, out test, out restrictions);

            Debug.Assert(metaArgs.Count > 0);

            return BindingHelpers.AddDynamicTestAndDefer(
                this,
                new MetaObject(
                    Expression.Dynamic(
                        new CompatibilityInvokeBinder(_state, newArgs.ToArray()),
                        typeof(object),
                        metaArgs.ToArray()
                    ),
                    restrictions.Merge(Restrictions.GetTypeRestriction(target.Expression, target.LimitType))
                ),
                args,
                new ValidationInfo(test, null)
            );
        }

        /// <summary>
        /// Translates our CallSignature into a DLR Argument list and gives the simple MetaObject's which are extracted
        /// from the tuple or dictionary parameters being splatted.
        /// </summary>
        private void TranslateArguments(MetaObject target, MetaObject/*!*/[]/*!*/ args, out List<ArgumentInfo/*!*/>/*!*/ newArgs, out List<Expression/*!*/>/*!*/ metaArgs, out Expression test, out Restrictions restrictions) {
            Argument[] argInfo = _signature.GetArgumentInfos();

            newArgs = new List<ArgumentInfo>();
            metaArgs = new List<Expression>();
            metaArgs.Add(target.Expression);
            Expression splatArgTest = null;
            Expression splatKwArgTest = null;
            restrictions = Restrictions.Empty;

            for (int i = 0; i < argInfo.Length; i++) {
                Argument ai = argInfo[i];

                switch (ai.Kind) {
                    case ArgumentType.Dictionary:
                        IAttributesCollection iac = (IAttributesCollection)args[i].Value;
                        List<string> argNames = new List<string>();

                        foreach (KeyValuePair<object, object> kvp in iac) {
                            string key = (string)kvp.Key;
                            newArgs.Add(Expression.NamedArg(key));
                            argNames.Add(key);

                            metaArgs.Add(
                                Expression.Call(
                                    Expression.ConvertHelper(args[i].Expression, typeof(IAttributesCollection)),
                                    typeof(IAttributesCollection).GetMethod("get_Item"),
                                    AstUtils.Constant(SymbolTable.StringToId(key))
                                )
                            );
                        }

                        restrictions = restrictions.Merge(Restrictions.GetTypeRestriction(args[i].Expression, args[i].LimitType));
                        splatKwArgTest = Expression.Call(
                            typeof(PythonOps).GetMethod("CheckDictionaryMembers"),
                            Expression.ConvertHelper(args[i].Expression, typeof(IAttributesCollection)),
                            Expression.Constant(argNames.ToArray())
                        );
                        break;
                    case ArgumentType.List:
                        IList<object> splattedArgs = (IList<object>)args[i].Value;
                        splatArgTest = Expression.Equal(
                            Expression.Property(Expression.ConvertHelper(args[i].Expression, args[i].LimitType), typeof(ICollection<object>).GetProperty("Count")),
                            Expression.Constant(splattedArgs.Count)
                        );

                        for (int splattedArg = 0; splattedArg < splattedArgs.Count; splattedArg++) {
                            newArgs.Add(Expression.PositionalArg(splattedArg + i));
                            metaArgs.Add(
                                Expression.Call(
                                    Expression.ConvertHelper(args[i].Expression, typeof(IList<object>)),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    Expression.Constant(splattedArg)
                                )
                            );
                        }

                        restrictions = restrictions.Merge(Restrictions.GetTypeRestriction(args[i].Expression, args[i].LimitType));
                        break;
                    case ArgumentType.Named:
                        newArgs.Add(Expression.NamedArg(SymbolTable.IdToString(ai.Name)));
                        metaArgs.Add(args[i].Expression);
                        break;
                    case ArgumentType.Simple:
                        newArgs.Add(Expression.PositionalArg(i));
                        metaArgs.Add(args[i].Expression);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            test = splatArgTest;
            if (splatKwArgTest != null) {
                if (test != null) {
                    test = Expression.AndAlso(test, splatKwArgTest);
                } else {
                    test = splatKwArgTest;
                }
            }
        }

        #endregion

        #region IPythonSite Members

        public BinderState Binder {
            get { return _state; }
        }

        #endregion

        #region IExpressionSerializable Members

        public virtual Expression CreateExpression() {
            return Expression.Call(
                typeof(PythonOps).GetMethod("MakeInvokeAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Signature.CreateExpression()
            );
        }

        #endregion
    }
}