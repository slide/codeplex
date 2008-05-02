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

using System.CodeDom.Compiler;
using System.Threading;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    #region Generated Easy Dynamic Sites

    // *** BEGIN GENERATED CODE ***
    // generated by function: gen_easy_sites from: generate_dynsites.py

    /// <summary>
    /// Dynamic site - arity 1
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct DynamicSite<T0, TRet> {
        private CallSite<DynamicSiteTarget<T0, TRet>> _site;

        public DynamicSite(DynamicAction action) {
            _site = CallSite<DynamicSiteTarget<T0, TRet>>.Create(action);
        }

        public static DynamicSite<T0, TRet> Create(DynamicAction action) {
            return new DynamicSite<T0, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<T0, TRet>>.Create(action), null);
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
        private CallSite<DynamicSiteTarget<T0, T1, TRet>> _site;

        public DynamicSite(DynamicAction action) {
            _site = CallSite<DynamicSiteTarget<T0, T1, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, TRet> Create(DynamicAction action) {
            return new DynamicSite<T0, T1, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<T0, T1, TRet>>.Create(action), null);
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
        private CallSite<DynamicSiteTarget<T0, T1, T2, TRet>> _site;

        public DynamicSite(DynamicAction action) {
            _site = CallSite<DynamicSiteTarget<T0, T1, T2, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, TRet> Create(DynamicAction action) {
            return new DynamicSite<T0, T1, T2, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<T0, T1, T2, TRet>>.Create(action), null);
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
        private CallSite<DynamicSiteTarget<T0, T1, T2, T3, TRet>> _site;

        public DynamicSite(DynamicAction action) {
            _site = CallSite<DynamicSiteTarget<T0, T1, T2, T3, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, TRet> Create(DynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<T0, T1, T2, T3, TRet>>.Create(action), null);
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
        private CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, TRet>> _site;

        public DynamicSite(DynamicAction action) {
            _site = CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, TRet> Create(DynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, TRet>>.Create(action), null);
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
        private CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>> _site;

        public DynamicSite(DynamicAction action) {
            _site = CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>>.Create(action);
        }

        public static DynamicSite<T0, T1, T2, T3, T4, T5, TRet> Create(DynamicAction action) {
            return new DynamicSite<T0, T1, T2, T3, T4, T5, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<DynamicSiteTarget<T0, T1, T2, T3, T4, T5, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            return _site.Target(_site, context, arg0, arg1, arg2, arg3, arg4, arg5);
        }
    }

    /// <summary>
    /// Dynamic site - arity variable based on Tuple size
    /// </summary>
    [GeneratedCode("DLR", "2.0")]
    public struct BigDynamicSite<T0, TRet> where T0 : Tuple {
        private CallSite<BigDynamicSiteTarget<T0, TRet>> _site;

        public BigDynamicSite(DynamicAction action) {
            _site = CallSite<BigDynamicSiteTarget<T0, TRet>>.Create(action);
        }

        public static BigDynamicSite<T0, TRet> Create(DynamicAction action) {
            return new BigDynamicSite<T0, TRet>(action);
        }

        public bool IsInitialized {
            get {
                return _site != null;
            }
        }

        public void EnsureInitialized(DynamicAction action) {
            if (_site == null) {
                Interlocked.CompareExchange(ref _site, CallSite<BigDynamicSiteTarget<T0, TRet>>.Create(action), null);
            }
        }

        public TRet Invoke(CodeContext context, T0 arg0) {
            return _site.Target(_site, context, arg0);
        }
    }


    // *** END GENERATED CODE ***

    #endregion
}
