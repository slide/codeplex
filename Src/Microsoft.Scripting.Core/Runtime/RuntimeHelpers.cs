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
using System.Threading;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using System.CodeDom.Compiler;
using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// These are some generally useful helper methods. Currently the only methods are those to
    /// cached boxed representations of commonly used primitive types so that they can be shared.
    /// This is useful to most dynamic languages that use object as a universal type.
    /// 
    /// The methods in RuntimeHelepers are caleld by the generated code. From here the methods may
    /// dispatch to other parts of the runtime to get bulk of the work done, but the entry points
    /// should be here.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RuntimeHelpers {
        private const int MIN_CACHE = -100;
        private const int MAX_CACHE = 1000;
        private static readonly object[] cache = MakeCache();
        private static readonly string[] chars = MakeSingleCharStrings();

        /// <summary> Singleton boxed instance of True.  We should never box additional instances. </summary>
        public static readonly object True = true;
        /// <summary> Singleton boxed instance of False  We should never box additional instances. </summary>
        public static readonly object False = false;

        /// <summary> Table of dynamicly generated delegates which are shared based upon method signature. </summary>
        private static readonly SimplePublisher<DelegateSignatureInfo, DelegateInfo> _dynamicDelegateCache = new SimplePublisher<DelegateSignatureInfo, DelegateInfo>();
        private static Dictionary<Type, List<Type>> _extensionTypes = new Dictionary<Type, List<Type>>();

        private static object[] MakeCache() {
            object[] result = new object[MAX_CACHE - MIN_CACHE];

            for (int i = 0; i < result.Length; i++) {
                result[i] = (object)(i + MIN_CACHE);
            }

            return result;
        }

        private static string[] MakeSingleCharStrings() {
            string[] result = new string[255];

            for (char ch = (char)0; ch < result.Length; ch++) {
                result[ch] = new string(ch, 1);
            }

            return result;
        }

        public static string CharToString(char ch) {
            if (ch < 255) return chars[ch];
            return new string(ch, 1);
        }

        public static object BooleanToObject(bool value) {
            return value ? True : False;
        }

        public static object Int32ToObject(Int32 value) {
            // caches improves pystone by ~5-10% on MS .Net 1.1, this is a very integer intense app
            if (value < MAX_CACHE && value >= MIN_CACHE) {
                return cache[value - MIN_CACHE];
            }
            return (object)value;
        }

        // formalNormalArgumentCount - does not include FuncDefFlags.ArgList and FuncDefFlags.KwDict
        // defaultArgumentCount - How many arguments in the method declaration have a default value?
        // providedArgumentCount - How many arguments are passed in at the call site?
        // hasArgList - Is the method declaration of the form "foo(*argList)"?
        // keywordArgumentsProvided - Does the call site specify keyword arguments?
        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(
            string methodName,
            int formalNormalArgumentCount,
            int defaultArgumentCount,
            int providedArgumentCount,
            bool hasArgList,
            bool keywordArgumentsProvided) {
            return TypeErrorForIncorrectArgumentCount(methodName, formalNormalArgumentCount, formalNormalArgumentCount, defaultArgumentCount, providedArgumentCount, hasArgList, keywordArgumentsProvided);
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(
            string methodName,
            int minFormalNormalArgumentCount,
            int maxFormalNormalArgumentCount,
            int defaultArgumentCount,
            int providedArgumentCount,
            bool hasArgList,
            bool keywordArgumentsProvided) {

            int formalCount;
            string formalCountQualifier;
            string nonKeyword = keywordArgumentsProvided ? "non-keyword " : "";

            if (defaultArgumentCount > 0 || hasArgList || minFormalNormalArgumentCount != maxFormalNormalArgumentCount) {
                if (providedArgumentCount < minFormalNormalArgumentCount || maxFormalNormalArgumentCount == Int32.MaxValue) {
                    formalCountQualifier = "at least";
                    formalCount = minFormalNormalArgumentCount - defaultArgumentCount;
                } else {
                    formalCountQualifier = "at most";
                    formalCount = maxFormalNormalArgumentCount;
                }
            } else if (minFormalNormalArgumentCount == 0) {
                return RuntimeHelpers.SimpleTypeError(string.Format("{0}() takes no arguments ({1} given)", methodName, providedArgumentCount));
            } else {            
                formalCountQualifier = "exactly";
                formalCount = minFormalNormalArgumentCount;
            }

            return RuntimeHelpers.SimpleTypeError(string.Format(
                "{0}() takes {1} {2} {3}argument{4} ({5} given)",
                                methodName, // 0
                                formalCountQualifier, // 1
                                formalCount, // 2
                                nonKeyword, // 3
                                formalCount == 1 ? "" : "s", // 4
                                providedArgumentCount)); // 5
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(string name, int formalNormalArgumentCount, int defaultArgumentCount, int providedArgumentCount) {
            return TypeErrorForIncorrectArgumentCount(name, formalNormalArgumentCount, defaultArgumentCount, providedArgumentCount, false, false);
        }

        public static ArgumentTypeException TypeErrorForIncorrectArgumentCount(string name, int expected, int received) {
            return TypeErrorForIncorrectArgumentCount(name, expected, 0, received);
        }

        public static ArgumentTypeException TypeErrorForExtraKeywordArgument(string name, string argumentName) {
            return SimpleTypeError(String.Format("{0}() got an unexpected keyword argument '{1}'", name, argumentName));
        }

        public static ArgumentTypeException TypeErrorForDuplicateKeywordArgument(string name, string argumentName) {
            return SimpleTypeError(String.Format("{0}() got multiple values for keyword argument '{1}'", name, argumentName));
        }

        public static ArgumentTypeException SimpleTypeError(string message) {
            return new ArgumentTypeException(message);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static Exception CannotConvertError(Type toType, object value) {
            return SimpleTypeError(String.Format("Cannot convert {0}({1}) to {2}", CompilerHelpers.GetType(value).Name, value, toType.Name));
        }

        public static Exception SimpleAttributeError(string message) {
            return new MissingMemberException(message);
        }

        public static void ThrowUnboundLocalError(SymbolId name) {
            throw new UnboundLocalException(string.Format("local variable '{0}' referenced before assignment", SymbolTable.IdToString(name)));
        }

        /// <summary>
        /// Called from generated code, helper to do name lookup
        /// </summary>
        public static object LookupName(CodeContext context, SymbolId name) {
            return context.LanguageContext.LookupName(context, name);
        }

        /// <summary>
        /// Called from generated code, helper to do name assignment.
        /// Order of parameters matches the codegen flow.
        /// </summary>
        public static object SetNameReorder(object value, CodeContext context, SymbolId name) {
            context.LanguageContext.SetName(context, name, value);
            return value;
        }

        /// <summary>
        /// Called from generated code, helper to do name assignment
        /// </summary>
        public static void SetName(CodeContext context, SymbolId name, object value) {
            context.LanguageContext.SetName(context, name, value);
        }

        /// <summary>
        /// Called from generated code, helper to remove a name
        /// </summary>
        public static object RemoveName(CodeContext context, SymbolId name) {
            return context.LanguageContext.RemoveName(context, name);
        }
        /// <summary>
        /// Called from generated code, helper to do a global name lookup
        /// </summary>
        public static object LookupGlobalName(CodeContext context, SymbolId name) {
            // TODO: could we get rid of new context creation:
            CodeContext moduleScopedContext = new CodeContext(context.GlobalScope, context.LanguageContext);
            return context.LanguageContext.LookupName(moduleScopedContext, name);
        }

        /// <summary>
        /// Called from generated code, helper to do global name assignment
        /// </summary>
        public static void SetGlobalName(CodeContext context, SymbolId name, object value) {
            // TODO: could we get rid of new context creation:
            CodeContext moduleScopedContext = new CodeContext(context.GlobalScope, context.LanguageContext);
            context.LanguageContext.SetName(moduleScopedContext, name, value);
        }

        /// <summary>
        /// Called from generated code, helper to remove a global name
        /// </summary>
        public static void RemoveGlobalName(CodeContext context, SymbolId name) {
            // TODO: could we get rid of new context creation:
            CodeContext moduleScopedContext = new CodeContext(context.GlobalScope, context.LanguageContext);
            context.LanguageContext.RemoveName(moduleScopedContext, name);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")] // TODO: fix
        public static void InitializeModuleField(CodeContext context, SymbolId name, ref ModuleGlobalWrapper wrapper) {
            ModuleGlobalCache mgc = context.LanguageContext.GetModuleCache(name);

            wrapper = new ModuleGlobalWrapper(context, mgc, name);
        }

        // emitted by TupleModuleGenerator:
        public static TTuple/*!*/ GetGlobalTuple<TTuple>(CodeContext/*!*/ context) where TTuple : Tuple {
            return ((TupleDictionary<TTuple>)context.GlobalScope.Dict).TupleData;
        }

        public static CodeContext/*!*/ GetStorageParent(CodeContext/*!*/ context) {
            CodeContext result = context.GetStorageParent();
            Debug.Assert(result != null);
            return result;
        }

        public static TTuple/*!*/ GetScopeStorage<TTuple>(CodeContext/*!*/ context) where TTuple : Tuple {
            TTuple result = context.GetStorage<TTuple>();
            Debug.Assert(result != null);
            return result;
        }

        public static CodeContext/*!*/ CreateLocalScope<TTuple>(TTuple/*!*/ storage, SymbolId[]/*!*/ names, CodeContext/*!*/ parent, bool isVisible) where TTuple : Tuple {
            return CreateNestedCodeContext(new FunctionEnvironmentDictionary<TTuple>(storage, names), parent, isVisible);
        }


        // The locals dictionary must be first so that we have the benefit of an emtpy stack when we emit the value
        // in the ScopeExpression
        public static CodeContext CreateNestedCodeContext(IAttributesCollection locals, CodeContext context, bool visible) {
            return new CodeContext(new Scope(context.Scope, locals, visible), context.LanguageContext, context);
        }

        public static ArgumentTypeException BadArgumentsForOperation(Operators op, params object[] args) {
            StringBuilder message = new StringBuilder("unsupported operand type(s) for operation ");
            message.Append(op.ToString());
            message.Append(": ");
            string comma = "";

            foreach (object o in args) {
                message.Append(comma);
                message.Append(CompilerHelpers.GetType(o));
                comma = ", ";
            }

            throw new ArgumentTypeException(message.ToString());
        }

        public static object ReadOnlyAssignError(bool field, string fieldName) {
            throw SimpleAttributeError(String.Format("{0} {1} is read-only", field ? "Field" : "Property", fieldName));
        }

        public static DynamicStackFrame[] GetDynamicStackFrames(Exception e) {
            return GetDynamicStackFrames(e, true);
        }

        public static DynamicStackFrame[] GetDynamicStackFrames(Exception e, bool filter) {
            List<DynamicStackFrame> frames = e.Data[typeof(DynamicStackFrame)] as List<DynamicStackFrame>;

            if (frames == null) {
                // we may have missed a dynamic catch, and our host is looking
                // for the exception...
                frames = ExceptionHelpers.AssociateDynamicStackFrames(e);
                ExceptionHelpers.DynamicStackFrames = null;
            }

            if (frames == null) {
                return new DynamicStackFrame[0];
            }

            if (!filter) return frames.ToArray();
#if !SILVERLIGHT
            frames = new List<DynamicStackFrame>(frames);
            List<DynamicStackFrame> res = new List<DynamicStackFrame>();

            // the list of _stackFrames we build up in RuntimeHelpers can have
            // too many frames if exceptions are thrown from script code and
            // caught outside w/o calling GetDynamicStackFrames.  Therefore we
            // filter down to only script frames which we know are associated
            // w/ the exception here.
            try {
                StackTrace outermostTrace = new StackTrace(e);
                IList<StackTrace> otherTraces = ExceptionHelpers.GetExceptionStackTraces(e) ?? new List<StackTrace>();
                List<StackFrame> clrFrames = new List<StackFrame>();
                foreach (StackTrace trace in otherTraces) {
                    clrFrames.AddRange(trace.GetFrames() ?? new StackFrame[0]); // rare, sometimes GetFrames returns null
                }
                clrFrames.AddRange(outermostTrace.GetFrames() ?? new StackFrame[0]);    // rare, sometimes GetFrames returns null

                int lastFound = 0;
                foreach (StackFrame clrFrame in clrFrames) {
                    MethodBase method = clrFrame.GetMethod();

                    for (int j = lastFound; j < frames.Count; j++) {
                        MethodBase other = frames[j].GetMethod();
                        // method info's don't always compare equal, check based
                        // upon name/module/declaring type which will always be a correct
                        // check for dynamic methods.
                        if (method.Module == other.Module &&
                            method.DeclaringType == other.DeclaringType &&
                            method.Name == other.Name) {
                            res.Add(frames[j]);
                            frames.RemoveAt(j);
                            lastFound = j;
                            break;
                        }
                    }
                }
            } catch (MemberAccessException) {
                // can't access new StackTrace(e) due to security
            }
            return res.ToArray();
#else 
            return frames.ToArray();
#endif
        }

        /// <summary>
        /// Creates a delegate with a given signature that could be used to invoke this object from non-dynamic code (w/o code context).
        /// A stub is created that makes appropriate conversions/boxing and calls the object.
        /// The stub should be executed within a context of this object's language.
        /// </summary>
        /// <returns>The delegate or a <c>null</c> reference if the object is not callable.</returns>
        public static Delegate GetDelegate(object callableObject, Type delegateType) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            Delegate result = callableObject as Delegate;
            if (result != null) {
                if (!delegateType.IsAssignableFrom(result.GetType())) {
                    throw RuntimeHelpers.SimpleTypeError(String.Format("Cannot cast {0} to {1}.", result.GetType(), delegateType));
                }

                return result;
            }

            IDynamicObject dynamicObject = callableObject as IDynamicObject;
            if (dynamicObject != null) {

                MethodInfo invoke;

                if (!typeof(Delegate).IsAssignableFrom(delegateType) || (invoke = delegateType.GetMethod("Invoke")) == null) {
                    throw RuntimeHelpers.SimpleTypeError("A specific delegate type is required.");
                }

                // using IDynamicObject.LanguageContext for now, we need todo better
                Debug.Assert(dynamicObject.LanguageContext != null, "Invalid implementation");

                ParameterInfo[] parameters = invoke.GetParameters();

                dynamicObject.LanguageContext.CheckCallable(dynamicObject, parameters.Length);

                // TODO: IDO.LanguageContext should be removed
                Debug.Assert(dynamicObject.LanguageContext != null, "InvariantContext doesn't have a binder");
                DelegateSignatureInfo signatureInfo = new DelegateSignatureInfo(
                    dynamicObject.LanguageContext.Binder,
                    invoke.ReturnType,
                    parameters
                );

                DelegateInfo delegateInfo = _dynamicDelegateCache.GetOrCreateValue(signatureInfo,
                    delegate() {
                        // creation code
                        return signatureInfo.GenerateDelegateStub();
                    });


                result = delegateInfo.CreateDelegate(delegateType, dynamicObject);
                if (result != null) {
                    return result;
                }
            }

            throw RuntimeHelpers.SimpleTypeError("Object is not callable.");
        }

        /// <summary>
        /// Registers a set of extension methods from the provided assemly.
        /// </summary>
        public static void RegisterAssembly(Assembly assembly) {
            object[] attrs = assembly.GetCustomAttributes(typeof(ExtensionTypeAttribute), false);
            foreach (ExtensionTypeAttribute et in attrs) {
                RegisterOneExtension(et.Extends, et.ExtensionType);
            }
        }

        private static void RegisterOneExtension(Type extending, Type extension) {
            lock (_extensionTypes) {
                List<Type> extensions;
                if (!_extensionTypes.TryGetValue(extending, out extensions)) {
                    _extensionTypes[extending] = extensions = new List<Type>();
                }
                extensions.Add(extension);
            }

            ExtensionTypeAttribute.RegisterType(extending, extension);

            FireExtensionEvent(extending, extension);
        }

        private static void FireExtensionEvent(Type extending, Type extension) {
            EventHandler<TypeExtendedEventArgs> ev = _extended;
            if (ev != null) {
                ev(null, new TypeExtendedEventArgs(extending, extension));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public class TypeExtendedEventArgs : EventArgs {
            public TypeExtendedEventArgs(Type extending, Type extension) {
                Extending = extending;
                Extension = extension;
            }

            public Type Extending;
            public Type Extension;
        }

        /// <summary>
        /// Provides a notification when a language agnostic extension event has been registered.
        /// 
        /// Maybe just a work around until Python can pull out the extension types on-demand or 
        /// if we require extension to be registered w/ an engine.
        /// </summary>
        public static event EventHandler<TypeExtendedEventArgs> TypeExtended {
            add {
                List<KeyValuePair<Type, Type>> existing = new List<KeyValuePair<Type, Type>>();
                lock (_extensionTypes) {
                    _extended += value;

                    foreach (KeyValuePair<Type, List<Type>> kvp in _extensionTypes) {
                        foreach (Type t in kvp.Value) {
                            existing.Add(new KeyValuePair<Type, Type>(kvp.Key, t));
                        }
                    }
                }
                foreach (KeyValuePair<Type, Type> extended in existing) {
                    FireExtensionEvent(extended.Key, extended.Value);
                }
            }
            remove {
                _extended -= value;
            }
        }

        private static EventHandler<TypeExtendedEventArgs> _extended;

        internal static Type[] GetExtensionTypes(Type t) {
            lock (_extensionTypes) {
                List<Type> res;
                if (_extensionTypes.TryGetValue(t, out res)) {
                    return res.ToArray();
                }
            }

            return Type.EmptyTypes;
        }

        /// <summary>
        /// EventInfo.EventHandlerType getter is marked SecuritySafeCritical in CoreCLR
        /// This method is to get to the property without using Reflection
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public static Type GetEventHandlerType(EventInfo eventInfo) {
            ContractUtils.RequiresNotNull(eventInfo, "eventInfo");
            return eventInfo.EventHandlerType;
        }

        public static IList<string> GetStringMembers(IList<object> members) {
            List<string> res = new List<string>();
            foreach (object o in members) {
                string str = o as string;
                if (str != null) {
                    res.Add(str);
                }
            }
            return res;
        }

        public static Delegate CreateDynamicClosure(MethodInfo mi, RuntimeTypeHandle @delegate, CodeContext context, object[] constants) {
            return ReflectionUtils.CreateDelegate(mi, Type.GetTypeFromHandle(@delegate), new Closure(context, constants));
        }

        /// <summary>
        /// Used by the code gen of wrapper methods which extract subset of the params array
        /// manually, but then extract the rest in bulk if the underlying method also takes
        /// params array.
        /// 
        /// This calls ArrayUtils.ShiftLeft, but performs additional checks that
        /// ArrayUtils.ShiftLeft assumes.
        /// </summary>
        public static T[] ShiftParamsArray<T>(T[] array, int count) {
            if (array != null && array.Length > count) {
                return ArrayUtils.ShiftLeft(array, count);
            } else {
                return new T[0];
            }
        }
    }
}
