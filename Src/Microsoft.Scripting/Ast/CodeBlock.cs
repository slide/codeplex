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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Internal;
using Microsoft.Scripting.Internal.Ast;
using Microsoft.Scripting.Internal.Generation;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.Internal.Ast {

    /// <summary>
    /// This captures a block of code that should correspond to a .NET method body.  It takes
    /// input through parameters and is expected to be fully bound.  This code can then be
    /// generated in a variety of ways.  The variables can be kept as .NET locals or in a
    /// 1st class environment object.  This is the primary unit used for passing around
    /// AST's in the DLR.
    /// 
    /// TODO - This should probably not be a Node but that will require some substantial walker changes.
    /// </summary>
    public partial class CodeBlock : Node {
        private static int _Counter = 0;

        private IList<Parameter> _parameters;
        private string _name;

        private CodeBlock _parent;
        private Statement _body;
        private List<Variable> _variables = new List<Variable>();
        private List<VariableReference> _references = new List<VariableReference>();
        private EnvironmentFactory _environmentFactory;

        private int _generatorTemps;

        private bool _isClosure;
        private bool _hasEnvironment;
        private bool _emitLocalDictionary;
        private bool _isGlobal;
        private bool _visibleScope = true;
        private bool _parameterArray;

        public static CodeBlock MakeCodeBlock(string name, Statement body) {
            return MakeCodeBlock(name, body, SourceSpan.None);
        }

        public static CodeBlock MakeCodeBlock(string name, Statement body, SourceSpan span) {
            return new CodeBlock(name, new Parameter[0], body, span);
        }

        public CodeBlock(SymbolId name, IList<Parameter> parameters, Statement body)
            : this(name, parameters, body, SourceSpan.None) {
        }

        public CodeBlock(SymbolId name, IList<Parameter> parameters, Statement body, SourceSpan span)
            : this(SymbolTable.IdToString(name), parameters, body, span) {
        }

        public CodeBlock(string name, IList<Parameter> parameters, Statement body)
            : this(name, parameters, body, SourceSpan.None) {
        }

        public CodeBlock(string name, IList<Parameter> parameters, Statement body, SourceSpan span)
            : base(span) {
            _name = name;
            _body = body;
            _parameters = parameters ?? new Parameter[0];
        }

        public IList<Parameter> Parameters {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The method refers to a variable in one of its parents lexical context and will need an environment
        /// flown into it.  A function which is a closure does not necessarily contain an Environment unless
        /// it contains additional closures or uses language features which require lifting all locals to
        /// an environment.
        /// </summary>
        internal bool IsClosure {
            get { return _isClosure; }
            set { _isClosure = value; }
        }

        /// <summary>
        /// Scopes with environments will have some locals stored within a dictionary (FunctionEnvironment).  If
        /// we are also a closure an environment is flown into the method and our environment will point to the
        /// parent environment.  Ultimately this will enable our children to get at our or our parents envs.
        /// 
        /// Upon entering a function with an environment a new CodeContext will be allocated with a new
        /// FunctionEnviroment as its locals.  In the case of a generator this new CodeContext and environment
        /// is allocated in the function called to create the Generator, not the function that implements the
        /// Generator body.
        /// 
        /// The environment is provided as the Locals of a CodeContext or in the case of a Generator 
        /// as the parentEnvironment field.
        /// </summary>
        internal bool HasEnvironment {
            get { return _hasEnvironment; }
            set { _hasEnvironment = value; }
        }

        /// <summary>
        /// True to force a function to have an environment and have all of its locals lifted
        /// into this environment.  This provides access to local variables via a dictionary but
        /// comes with the performance penality of not using the real stack for locals.
        /// </summary>
        public bool EmitLocalDictionary {
            get {
                // When custom frames are turned on, we emit dictionaries everywhere
                return ScriptDomainManager.Options.Frames || _emitLocalDictionary;
            }
            set {
                _emitLocalDictionary = value;
            }
        }

        public bool IsGlobal {
            get { return _isGlobal; }
            set { _isGlobal = value; }
        }

        public bool ParameterArray {
            get { return _parameterArray; }
            set { _parameterArray = value; }
        }

        public CodeBlock Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        public bool IsVisible {
            get { return _visibleScope; }
            set { _visibleScope = value; }
        }
        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        public List<VariableReference> References {
            get { return _references; }
        }

        public List<Variable> Variables {
            get { return _variables; }
        }

        public Type EnvironmentType {
            get {
                Debug.Assert(_environmentFactory != null);
                return _environmentFactory.EnvironmentType;
            }
        }

        public EnvironmentFactory EnvironmentFactory {
            get { return _environmentFactory; }
        }

        protected internal int GeneratorTemps {
            get { return _generatorTemps; }
        }

        public void AddVariable(Variable variable) {
            Debug.Assert(variable.Block == this);
            _variables.Add(variable);
        }

        public void AddReference(VariableReference reference) {
            _references.Add(reference);
        }

        public VariableReference CreateTemporaryVariable(SymbolId name, Type type) {
            return CreateTemporaryVariable(name, Variable.VariableKind.Temporary, type);
        }

        public VariableReference CreateGeneratorTempVariable(SymbolId name, Type type) {
            return CreateTemporaryVariable(name, Variable.VariableKind.GeneratorTemporary, type);
        }

        private VariableReference CreateTemporaryVariable(SymbolId name, Variable.VariableKind kind, Type type) {
            if (type == null) throw new ArgumentNullException("type");

            Variable variable = Variable.Create(name, kind, this, type);
            VariableReference reference = new VariableReference(name);
            reference.Variable = variable;

            _variables.Add(variable);
            _references.Add(reference);

            return reference;
        }

        private void EmitEnvironmentIDs(CodeGen cg) {
            int size = 0;
            foreach (Parameter prm in _parameters) {
                if (prm.AllocateInEnvironment) size++;
            }
            foreach (Variable var in _variables) {
                if (var.AllocateInEnvironment) size++;
            }

            if (!cg.IsDynamicMethod) {
                Debug.Assert(cg.TypeGen != null);

                CodeGen cctor = cg.TypeGen.TypeInitializer;
                EmitEnvironmentIdArray(cctor, size);
                Slot fields = cg.TypeGen.AddStaticField(typeof(SymbolId[]), "__symbolIds$" + _name + "$" + Interlocked.Increment(ref _Counter));
                fields.EmitSet(cctor);
                fields.EmitGet(cg);
            } else {
                EmitEnvironmentIdArray(cg, size);
            }
        }

        private void EmitEnvironmentIdArray(CodeGen cg, int size) {
            // Create the array for the names
            cg.EmitInt(size);
            cg.Emit(OpCodes.Newarr, typeof(SymbolId));

            int index = 0;
            cg.EmitDebugMarker("--- Environment IDs ---");

            foreach (Parameter prm in _parameters) {
                if (prm.AllocateInEnvironment) {
                    EmitSetVariableName(cg, index++, prm.Name);
                }
            }

            foreach (Variable var in _variables) {
                if (var.AllocateInEnvironment) {
                    EmitSetVariableName(cg, index++, var.Name);
                }
            }
            cg.EmitDebugMarker("--- End Environment IDs ---");
        }

        private static void EmitSetVariableName(CodeGen cg, int index, SymbolId name) {
            cg.Emit(OpCodes.Dup);
            cg.EmitInt(index);
            cg.Emit(OpCodes.Ldelema, typeof(SymbolId));
            cg.EmitSymbolId(name);
            cg.Emit(OpCodes.Call, typeof(SymbolId).GetConstructor(new Type[] { typeof(SymbolId) }));
        }

        internal void CreateEnvironmentFactory(bool generator) {
            if (HasEnvironment) {
                // Get the environment size
                int size = 0;

                if (generator) {
                    size += _generatorTemps;

                    foreach (Variable var in _variables) {
                        if (var.Kind == Variable.VariableKind.GeneratorTemporary) {
                            size++;
                        }
                    }
                }

                foreach (Parameter parm in _parameters) {
                    if (parm.AllocateInEnvironment) size++;
                }
                foreach (Variable var in _variables) {
                    if (var.AllocateInEnvironment) size++;
                }
                // Find the right environment factory for the size of elements to store
                _environmentFactory = CreateEnvironmentFactory(size);
            }
        }

        internal EnvironmentSlot EmitEnvironmentAllocation(CodeGen cg) {
            Debug.Assert(_environmentFactory != null);

            cg.EmitDebugMarker("-- ENV ALLOC START --");

            _environmentFactory.EmitStorage(cg);
            cg.Emit(OpCodes.Dup);
            // Store the environment reference in the local
            EnvironmentSlot environmentSlot = _environmentFactory.CreateEnvironmentSlot(cg);
            environmentSlot.EmitSet(cg);

            // Emit the names array for the environment constructor
            EmitEnvironmentIDs(cg);
            // Emit code to generate the new instance of the environment

            _environmentFactory.EmitNewEnvironment(cg);

            cg.EmitDebugMarker("-- ENV ALLOC END --");

            return environmentSlot;
        }

        /// <summary>
        /// Creates a slot for context of type CodeContext from an environment slot.
        /// </summary>
        internal Slot CreateEnvironmentContext(CodeGen cg) {
            // update CodeContext so it contains the nested scope for the locals
            //  ctxSlot = new CodeContext(currentCodeContext, locals)
            Slot ctxSlot = cg.GetNamedLocal(typeof(CodeContext), "$frame");
            cg.EmitCodeContext();
            cg.EnvironmentSlot.EmitGetDictionary(cg);
            cg.EmitInt(_visibleScope ? 1 : 0);
            cg.EmitCall(typeof(RuntimeHelpers), "CreateNestedCodeContext");
            ctxSlot.EmitSet(cg);
            return ctxSlot;
        }

        public void CreateSlots(CodeGen cg) {
            if (cg == null) throw new ArgumentNullException("cg");

            if (HasEnvironment) {
                // we're an environment slot, we need our own environment slot, and we're
                // going to update our Context slot to point to a CodeContext which has
                // it's Locals pointing at our Environment.
                cg.EnvironmentSlot = EmitEnvironmentAllocation(cg);
                cg.ContextSlot = CreateEnvironmentContext(cg);
            }

            CreateOuterScopeAccessSlots(cg);

            foreach (Parameter prm in _parameters) {
                prm.Allocate(cg);
            }
            foreach (Variable var in _variables) {
                var.Allocate(cg);
            }
            foreach (VariableReference r in _references) {
                r.CreateSlot(cg);
                Debug.Assert(r.Slot != null);
            }

            cg.Allocator.LocalAllocator.PrepareForEmit(cg);
            cg.Allocator.GlobalAllocator.PrepareForEmit(cg);
        }

        public void CreateOuterScopeAccessSlots(CodeGen cg) {
            ScopeAllocator allocator = cg.Allocator;

            // Current context is accessed via environment slot, if any
            if (HasEnvironment) {
                allocator.AddScopeAccessSlot(this, cg.EnvironmentSlot);
            }

            if (IsClosure) {
                Slot scope = cg.GetLocalTmp(typeof(Scope));
                cg.EmitCodeContext();
                cg.EmitPropertyGet(typeof(CodeContext), "Scope");
                if (HasEnvironment) {
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                }
                scope.EmitSet(cg);

                CodeBlock current = this;
                do {
                    CodeBlock parent = current._parent;
                    scope.EmitGet(cg);

                    cg.EmitCall(typeof(RuntimeHelpers).GetMethod("GetTupleDictionaryData").MakeGenericMethod(parent._environmentFactory.StorageType));
    
                    Slot storage = new LocalSlot(cg.DeclareLocal(parent._environmentFactory.StorageType), cg);
                    storage.EmitSet(cg);
                    allocator.AddScopeAccessSlot(parent, storage);

                    scope.EmitGet(cg);
                    cg.EmitPropertyGet(typeof(Scope), "Parent");
                    scope.EmitSet(cg);

                    current = parent;
                } while(current != null && current.IsClosure);

                cg.FreeLocalTmp(scope);
            }

            // TODO: Create access slot for globals
        }

        public void AddGeneratorTemps(int count) {
            _generatorTemps += count;
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                Body.Walk(walker);
            }
            walker.PostWalk(this);
        }


        public object Execute(CodeContext context) {
            //TODO what about parameters?
            return Body.Execute(context);
        }

        private bool NeedsWrapperMethod() {
            return _parameters.Count > CallTargets.MaximumCallArgs;
        }

        protected ConstantPool GetStaticDataForBody(CodeGen cg) {
            if (cg.DynamicMethod) return new ConstantPool();
            else return null;
        }

        public void BindClosures() {
            ClosureBinder.Bind(this);
            FlowChecker.Check(this);
        }

        public void EmitDelegate(CodeGen cg, bool forceWrapperMethod) {
            FlowChecker.Check(this);
            bool createWrapperMethod = _parameterArray ? false : forceWrapperMethod || NeedsWrapperMethod();

            bool hasContextParameter =
                createWrapperMethod ||
                IsClosure ||
                !(cg.ContextSlot is StaticFieldSlot) ||
                _parameterArray;

            cg.EmitSequencePointNone();

            CodeGen impl = CreateMethod(cg, hasContextParameter);
            
            EmitFunctionImplementation(impl);
            impl.Finish();

            // if the method has more than our maximum # of args wrap
            // it in a method that takes an object[] instead.
            if (createWrapperMethod) {
                CodeGen wrapper = MakeWrapperMethodN(cg, impl);
                wrapper.Finish();
                cg.EmitDelegate(wrapper, typeof(CallTargetWithContextN));
            } else if (_parameterArray) {
                cg.EmitDelegate(impl, typeof(CallTargetWithContextN));
            } else {
                cg.EmitDelegate(impl, CallTargets.GetTargetType(hasContextParameter, _parameters.Count));
            }
        }

        /// <summary>
        /// Defines the method with the correct signature and sets up the context slot appropriately.
        /// </summary>
        /// <returns></returns>
        private CodeGen CreateMethod(CodeGen outer, bool hasContextParameter) {
            List<Type> paramTypes = new List<Type>();
            List<SymbolId> paramNames = new List<SymbolId>();
            CodeGen impl;
            int parameterIndex = 0;

            if (hasContextParameter) {
                paramTypes.Add(typeof(CodeContext));
                paramNames.Add(SymbolTable.StringToId("$context"));
                parameterIndex = 1;
            }

            // Parameters
            if (_parameterArray) {
                paramTypes.Add(typeof(object[]));
                paramNames.Add(SymbolTable.StringToId("$params"));
                int index = 0;
                foreach (Parameter p in _parameters) {
                    p.Parameter = index++;
                }
            } else {
                foreach (Parameter p in _parameters) {
                    paramTypes.Add(p.Type);
                    paramNames.Add(p.Name);
                    p.Parameter = parameterIndex++;
                }
            }

            string implName = _name + "$" + Interlocked.Increment(ref _Counter);

            // create the new method & setup its locals
            impl = outer.DefineMethod(implName, typeof(object),
                paramTypes, SymbolTable.IdsToStrings(paramNames), GetStaticDataForBody(outer));

            impl.ContextSlot = hasContextParameter ? impl.GetArgumentSlot(0) : outer.ContextSlot;
            if (_parameterArray) {
                impl.ParamsSlot = impl.GetArgumentSlot(parameterIndex);
            }

            // create the new method & setup its locals
            impl.Allocator = CompilerHelpers.CreateLocalStorageAllocator(outer, impl);

            return impl;
        }

        /// <summary>
        /// Creates a wrapper method for the user-defined function.  This allows us to use the CallTargetN
        /// delegate against the function when we don't have a CallTarget# which is large enough.
        /// </summary>
        private CodeGen MakeWrapperMethodN(CodeGen outer, CodeGen impl) {
            CodeGen wrapper;
            Slot contextSlot = null;
            Slot argSlot;
            ConstantPool staticData = null;

            bool hasContextParameter = impl.ArgumentSlots.Count > 0
                && impl.ArgumentSlots[0].Type == typeof(CodeContext);

            if (impl.ConstantPool != null) {
                staticData = impl.ConstantPool.CopyData();
            }

            string implName = impl.MethodBase.Name;

            if (hasContextParameter) {
                wrapper = outer.DefineMethod(implName,
                    typeof(object),
                    new Type[] { typeof(CodeContext), typeof(object[]) },
                    null, staticData);
                contextSlot = wrapper.GetArgumentSlot(0);
                argSlot = wrapper.GetArgumentSlot(1);
            } else {
                // Context weirdness: DynamicMethods need to flow their context, and if we don't
                // have a TypeGen we'll create a DynamicMethod but we won't flow context w/ it.
                Debug.Assert(outer.TypeGen != null);

                wrapper = outer.DefineMethod(implName, typeof(object), new Type[] { typeof(object[]) },
                    null, staticData);
                argSlot = wrapper.GetArgumentSlot(0);
            }

            if (wrapper.ConstantPool != null) {
                wrapper.ConstantPool.Slot.EmitGet(wrapper);
            }

            if (contextSlot != null) contextSlot.EmitGet(wrapper);

            for (int pi = 0; pi < _parameters.Count; pi++) {
                argSlot.EmitGet(wrapper);
                wrapper.EmitInt(pi);
                wrapper.Emit(OpCodes.Ldelem_Ref);
            }
            wrapper.EmitCall(impl.MethodInfo);
            wrapper.Emit(OpCodes.Ret);
            return wrapper;
        }

        public void EmitFunctionImplementation(CodeGen _impl) {
            // Try block may yield, but we are not interested in the isBlockYielded value
            // hence push a dummySlot to pass the Assertion.
            Slot dummySlot = _impl.GetLocalTmp(typeof(int));

            CompilerHelpers.EmitStackTraceTryBlockStart(_impl, dummySlot);

            // emit the actual body
            EmitBody(_impl);

            // free up dummySlot
            _impl.FreeLocalTmp(dummySlot);
            CompilerHelpers.EmitStackTraceFaultBlock(_impl, _name, _impl.Context.SourceUnit);
        }

        public virtual void EmitBody(CodeGen cg) {
            cg.Allocator.ActiveScope = this;
            CreateEnvironmentFactory(false);
            CreateSlots(cg);

            EmitStartPosition(cg);

            Body.Emit(cg);

            EmitEndPosition(cg);

            cg.EmitReturn(null); //TODO skip if Body is guaranteed to return
        }

        private void EmitStartPosition(CodeGen cg) {
            // ensure a break point exists at the top
            // of the file if there isn't a statement
            // flush with the start of the file.
            if (!Start.IsValid) return;

            if (Body.Start.IsValid) {
                if (Body.Start != Start) {
                    cg.EmitPosition(Start, Start);
                }
            } else {
                BlockStatement block = Body as BlockStatement;
                if (block != null) {
                    for (int i = 0; i < block.Statements.Count; i++) {
                        if (block.Statements[i].Start.IsValid) {
                            if (block.Statements[i].Start != Start) {
                                cg.EmitPosition(Start, Start);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void EmitEndPosition(CodeGen cg) {
            // ensure we emit a sequence point at the end
            // so the user can inspect any info before exiting
            // the function.  Also make sure additional code
            // isn't associated with this function.
            cg.EmitPosition(End, End);
            cg.EmitSequencePointNone();
        }
        

        /// <summary>
        /// TODO Kill this method
        /// Creates global slots only. Used by the "CreateDelegate*" APIs.
        /// The global slots are the only ones that can be referenced by
        /// the lambda function.
        /// </summary>
        /// <param name="cg"></param>
        internal void AllocateGlobals(CodeGen cg) {
            foreach (Variable var in _variables) {
                if (var.Kind == Variable.VariableKind.Global) {
                    var.Allocate(cg);
                }
            }
        }

        public T CreateDelegate<T>(CompilerContext context) 
            where T : class {
            CodeGen cg = CompilerHelpers.CreateDynamicCodeGenerator(context);
            cg.Allocator = CompilerHelpers.CreateFrameAllocator(cg.ContextSlot);
            
            cg.EnvironmentSlot = new EnvironmentSlot(new PropertySlot(cg.ContextSlot, typeof(CodeContext).GetProperty("Locals")));

            EmitFunctionImplementation(cg);
            cg.Finish();

            return (T)(object)cg.CreateDelegate(typeof(T));
        }
 
    }
}
