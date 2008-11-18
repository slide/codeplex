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
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using System.Threading;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    #region Generated Easy Dynamic Sites

    // *** BEGIN GENERATED CODE ***
    // generated by function: gen_easy_sites from: generate_dynsites.py

    /// <summary>
    /// Dynamic site - arity 0
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<TRet> {
        private CallSite<Func<CallSite, CodeContext, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, TRet>>.Create(action);
        }

        public static DynamicSite<TRet> Create(OldDynamicAction action) {
            return new DynamicSite<TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context) {
            return _site.Target(_site, context);
        }
    }

    /// <summary>
    /// Dynamic site - arity 1
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, TRet>>.Create(action);
        }

        public static DynamicSite<T0, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0) {
            return _site.Target(_site, context, arg0);
        }
    }

    /// <summary>
    /// Dynamic site - arity 2
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1) {
            return _site.Target(_site, context, arg0, arg1);
        }
    }

    /// <summary>
    /// Dynamic site - arity 3
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2) {
            return _site.Target(_site, context, arg0, arg1, arg2);
        }
    }

    /// <summary>
    /// Dynamic site - arity 4
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3);
        }
    }

    /// <summary>
    /// Dynamic site - arity 5
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4);
        }
    }

    /// <summary>
    /// Dynamic site - arity 6
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5);
        }
    }

    /// <summary>
    /// Dynamic site - arity 7
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }
    }

    /// <summary>
    /// Dynamic site - arity 8
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
    }

    /// <summary>
    /// Dynamic site - arity 9
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
    }

    /// <summary>
    /// Dynamic site - arity 10
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
    }

    /// <summary>
    /// Dynamic site - arity 11
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }
    }

    /// <summary>
    /// Dynamic site - arity 12
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }
    }

    /// <summary>
    /// Dynamic site - arity 13
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
    }

    /// <summary>
    /// Dynamic site - arity 14
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> {
        private CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>> _site;

        public DynamicSite(OldDynamicAction action) {
            _site = CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> Create(OldDynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(OldDynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<Func<CallSite, CodeContext, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }
    }


    // *** END GENERATED CODE ***

    #endregion
}
