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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    internal enum VariableStorageKind {
        Local,
        Hoisted
    }

    /// <summary>
    /// CompilerScope is the data structure which the Compiler keeps information
    /// related to compiling scopes. It stores the following information:
    ///   1. Parent relationship (for resolving variables)
    ///   2. Information about hoisted variables
    ///   3. Information for resolving closures
    /// 
    /// Instances are produced by VariableBinder, which does a tree walk
    /// looking for scope nodes: LambdaExpression and BlockExpression.
    /// </summary>
    internal sealed partial class CompilerScope {
        /// <summary>
        /// parent scope, if any
        /// </summary>
        private CompilerScope _parent;

        /// <summary>
        /// The expression node for this scope
        /// </summary>
        internal readonly Expression Node;

        /// <summary>
        /// Does this scope (or any inner scope) close over variables from any
        /// parent scope?
        /// Populated by VariableBinder
        /// </summary>
        internal bool NeedsClosure;

        /// <summary>
        /// Variables defined in this scope, and whether they're hoisted or not
        /// Populated by VariableBinder
        /// </summary>
        internal readonly Dictionary<ParameterExpression, VariableStorageKind> Definitions = new Dictionary<ParameterExpression, VariableStorageKind>();

        /// <summary>
        /// Each variable referenced within this scope, and how often it was referenced
        /// Populated by VariableBinder
        /// 
        /// Created lazily as we only use in about 1 out of 3 compiles when compiling rules.
        /// </summary>
        internal Dictionary<ParameterExpression, int> ReferenceCount;

        /// <summary>
        /// Scopes whose variables were merged into this one
        /// 
        /// Created lazily as we create hundreds of compiler scopes w/o merging scopes when compiling rules.
        /// </summary>
        internal Set<BlockExpression> MergedScopes;

        /// <summary>
        /// The scope's hoisted locals, if any.
        /// Provides storage for variables that are referenced from nested lambdas
        /// </summary>
        private HoistedLocals _hoistedLocals;

        /// <summary>
        /// The closed over hoisted locals
        /// </summary>
        private HoistedLocals _closureHoistedLocals;

        /// <summary>
        /// Mutable dictionary that maps non-hoisted variables to either local
        /// slots or argument slots
        /// </summary>
        private readonly Dictionary<ParameterExpression, Storage> _locals = new Dictionary<ParameterExpression, Storage>();

        internal CompilerScope(Expression node) {
            Node = node;
            var variables = GetVariables(node);

            Definitions = new Dictionary<ParameterExpression, VariableStorageKind>(variables.Count);
            foreach (var v in variables) {
                Definitions.Add(v, VariableStorageKind.Local);
            }
        }

        /// <summary>
        /// This scope's hoisted locals, or the closed over locals, if any
        /// Equivalent to: _hoistedLocals ?? _closureHoistedLocals
        /// </summary>
        internal HoistedLocals NearestHoistedLocals {
            get { return _hoistedLocals ?? _closureHoistedLocals; }
        }

        /// <summary>
        /// Called when entering a lambda/block. Performs all variable allocation
        /// needed, including creating hoisted locals and IL locals for accessing
        /// parent locals
        /// </summary>
        internal CompilerScope Enter(LambdaCompiler lc, CompilerScope parent) {
            SetParent(lc, parent);

            AllocateLocals(lc);

            if (IsLambda && _closureHoistedLocals != null) {
                EmitClosureAccess(lc, _closureHoistedLocals);
            }

            EmitNewHoistedLocals(lc);

            if (IsLambda) {
                EmitCachedVariables();
            }

            return this;
        }

        /// <summary>
        /// Frees unnamed locals, clears state associated with this compiler
        /// </summary>
        internal CompilerScope Exit() {
            // free scope's variables
            if (!IsLambda) {
                foreach (Storage storage in _locals.Values) {
                    storage.FreeLocal();
                }
            }
            
            // Clear state that is associated with this parent
            // (because the scope can be reused in another context)
            CompilerScope parent = _parent;
            _parent = null;
            _hoistedLocals = null;
            _closureHoistedLocals = null;
            _locals.Clear();

            return parent;
        }

        #region LocalScopeExpression support

        internal void EmitVariableAccess(LambdaCompiler lc, ReadOnlyCollection<ParameterExpression> vars) {
            if (NearestHoistedLocals != null) {
                // Find what array each variable is on & its index
                var indexes = new List<long>(vars.Count);

                foreach (var variable in vars) {
                    // For each variable, find what array it's defined on
                    ulong parents = 0;
                    HoistedLocals locals = NearestHoistedLocals;
                    while (!locals.Indexes.ContainsKey(variable)) {
                        parents++;
                        locals = locals.Parent;
                        Debug.Assert(locals != null);
                    }
                    
                    // combine the number of parents we walked, with the
                    // real index of variable to get the index to emit.
                    ulong index = (parents << 32) | (uint)locals.Indexes[variable];

                    indexes.Add((long)index);
                }

                if (indexes.Count > 0) {
                    EmitGet(NearestHoistedLocals.SelfVariable);
                    lc.EmitConstantArray(indexes.ToArray());
                    lc.IL.Emit(OpCodes.Call, typeof(RuntimeOps).GetMethod("CreateRuntimeVariables", new[] { typeof(object[]), typeof(long[]) }));
                    return;
                }
            }

            // No visible variables
            lc.IL.Emit(OpCodes.Call, typeof(RuntimeOps).GetMethod("CreateRuntimeVariables", Type.EmptyTypes));
            return;
        }

        #endregion

        #region Variable access

        /// <summary>
        /// Adds a new virtual variable corresponding to an IL local
        /// </summary>
        internal void AddLocal(LambdaCompiler gen, ParameterExpression variable) {
            _locals.Add(variable, new LocalStorage(gen, variable));
        }

        internal void EmitGet(ParameterExpression variable) {
            ResolveVariable(variable).EmitLoad();
        }

        internal void EmitSet(ParameterExpression variable) {
            ResolveVariable(variable).EmitStore();
        }

        internal void EmitAddressOf(ParameterExpression variable) {
            ResolveVariable(variable).EmitAddress();
        }

        private Storage ResolveVariable(ParameterExpression variable) {
            return ResolveVariable(variable, NearestHoistedLocals);
        }

        /// <summary>
        /// Resolve a local variable in this scope or a closed over scope
        /// Throws if the variable is defined
        /// </summary>
        private Storage ResolveVariable(ParameterExpression variable, HoistedLocals hoistedLocals) {
            // Search IL locals and arguments, but only in this lambda
            for (CompilerScope s = this; s != null; s = s._parent) {
                Storage storage;
                if (s._locals.TryGetValue(variable, out storage)) {
                    return storage;
                }

                // if this is a lambda, we're done
                if (s.IsLambda) {
                    break;
                }
            }

            // search hoisted locals
            for (HoistedLocals h = hoistedLocals; h != null; h = h.Parent) {
                int index;
                if (h.Indexes.TryGetValue(variable, out index)) {
                    return new ElementBoxStorage(
                        ResolveVariable(h.SelfVariable, hoistedLocals),
                        index,
                        variable
                    );
                }
            }

            // If this is a genuine unbound variable, the error should be
            // thrown in VariableBinder.
            throw Error.UndefinedVariable(variable.Name, variable.Type, CurrentLambdaName);
        }

        #endregion
        
        // private methods:

        private bool IsLambda {
            get { return Node.NodeType == ExpressionType.Lambda; }
        }

        private void SetParent(LambdaCompiler lc, CompilerScope parent) {
            Debug.Assert(_parent == null && parent != this);
            _parent = parent;

            if (NeedsClosure && _parent != null) {
                _closureHoistedLocals = _parent.NearestHoistedLocals;
            }

            var hoistedVars = GetVariables().Where(p => Definitions[p] == VariableStorageKind.Hoisted).ToReadOnly();

            if (hoistedVars.Count > 0) {
                _hoistedLocals = new HoistedLocals(_closureHoistedLocals, hoistedVars);
                AddLocal(lc, _hoistedLocals.SelfVariable);
            }
        }

        // Emits creation of the hoisted local storage
        private void EmitNewHoistedLocals(LambdaCompiler lc) {
            if (_hoistedLocals == null) {
                return;
            }

            // create the array
            lc.IL.EmitInt(_hoistedLocals.Variables.Count);
            lc.IL.Emit(OpCodes.Newarr, typeof(object));

            // initialize all elements
            int i = 0;
            foreach (ParameterExpression v in _hoistedLocals.Variables) {
                // array[i] = new StrongBox<T>(...);
                lc.IL.Emit(OpCodes.Dup);
                lc.IL.EmitInt(i++);
                Type boxType = typeof(StrongBox<>).MakeGenericType(v.Type);

                if (lc.Parameters.Contains(v)) {
                    // array[i] = new StrongBox<T>(argument);
                    int index = lc.Parameters.IndexOf(v);
                    lc.EmitLambdaArgument(index);
                    lc.IL.Emit(OpCodes.Newobj, boxType.GetConstructor(new Type[] { v.Type }));
                } else if (v == _hoistedLocals.ParentVariable) {
                    // array[i] = new StrongBox<T>(closure.Locals);
                    ResolveVariable(v, _closureHoistedLocals).EmitLoad();
                    lc.IL.Emit(OpCodes.Newobj, boxType.GetConstructor(new Type[] { v.Type }));
                } else {
                    // array[i] = new StrongBox<T>();
                    lc.IL.Emit(OpCodes.Newobj, boxType.GetConstructor(Type.EmptyTypes));
                }
                // if we want to cache this into a local, do it now
                if (ShouldCache(v) && !_locals.ContainsKey(v)) {
                    lc.IL.Emit(OpCodes.Dup);
                    CacheBoxToLocal(lc, v);
                }
                lc.IL.Emit(OpCodes.Stelem_Ref);
            }

            // store it
            EmitSet(_hoistedLocals.SelfVariable);
        }


        // If hoisted variables are referenced "enough", we cache the
        // StrongBox<T> in an IL local, which saves an array index and a cast
        // when we go to look it up later
        private void EmitCachedVariables() {
            foreach (var v in GetVariables()) {
                if (ShouldCache(v)) {
                    if (!_locals.ContainsKey(v)) {
                        var storage = ResolveVariable(v) as ElementBoxStorage;
                        if (storage != null) {
                            storage.EmitLoadBox();
                            CacheBoxToLocal(storage.Compiler, v);
                        }
                    }
                }
            }
        }

        private bool ShouldCache(ParameterExpression v) {
            if (ReferenceCount == null) {
                return false;
            }
            // This caching is too aggressive in the face of conditionals and
            // switch. Also, it is too conservative for variables used inside
            // of loops.
            int count;
            return ReferenceCount.TryGetValue(v, out count) && count > 2;
        }

        private void CacheBoxToLocal(LambdaCompiler lc, ParameterExpression v) {
            Debug.Assert(ShouldCache(v) && !_locals.ContainsKey(v));
            var local = new LocalBoxStorage(lc, v);
            local.EmitStoreBox();
            _locals.Add(v, local);
        }

        // Creates IL locals for accessing closures
        private void EmitClosureAccess(LambdaCompiler lc, HoistedLocals locals) {
            if (locals == null) {
                return;
            }

            EmitClosureToVariable(lc, locals);

            while ((locals = locals.Parent) != null) {
                var v =  locals.SelfVariable;
                var local = new LocalStorage(lc, v);
                local.EmitStore(ResolveVariable(v));
                _locals.Add(v, local);
            }
        }

        private void EmitClosureToVariable(LambdaCompiler lc, HoistedLocals locals) {
            lc.EmitClosureArgument();
            lc.IL.Emit(OpCodes.Ldfld, typeof(Closure).GetField("Locals"));
            AddLocal(lc, locals.SelfVariable);
            EmitSet(locals.SelfVariable);
        }

        // Allocates slots for IL locals or IL arguments
        private void AllocateLocals(LambdaCompiler lc) {
            foreach (ParameterExpression v in GetVariables()) {
                if (Definitions[v] == VariableStorageKind.Local) {
                    Storage s;
                    //If v is in lc.Parameters, it is a parameter.
                    //Otherwise, it is a local variable.
                    if (lc.Parameters.Contains(v)) {
                        s = new ArgumentStorage(lc, v);
                    } else {
                        s = new LocalStorage(lc, v);
                    }
                    _locals.Add(v, s);
                }
            }
        }

        private IList<ParameterExpression> GetVariables() {
            var vars = GetVariables(Node);
            if (MergedScopes == null) {
                return vars;
            }
            var list = new List<ParameterExpression>(vars);
            foreach (var block in MergedScopes) {
                list.AddRange(block.Variables);
            }
            return list;
        }

        private static ReadOnlyCollection<ParameterExpression> GetVariables(Expression scope) {
            var lambda = scope as LambdaExpression;
            if (lambda != null) {
                return lambda.Parameters;
            }
            return ((BlockExpression)scope).Variables;
        }

        private string CurrentLambdaName {
            get {
                CompilerScope s = this;
                while (true) {
                    var lambda = s.Node as LambdaExpression;
                    if (lambda != null) {
                        return lambda.Name;
                    }
                }
            }
        }
    }
}
