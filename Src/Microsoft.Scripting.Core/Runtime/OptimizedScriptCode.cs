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

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Utils;
using System.Security;
using System.Security.Permissions;

namespace System.Scripting.Runtime {

    public class OptimizedScriptCode : ScriptCode {
        private Scope _optimizedScope;
        private DlrMainCallTarget _optimizedTarget;

        public OptimizedScriptCode(LambdaExpression code, SourceUnit sourceUnit)
            : base(code, null, sourceUnit) {
            Debug.Assert(code.Parameters.Count == 0, "GlobalRewritter shouldn't have been applied yet");
        }

        public OptimizedScriptCode(Scope optimizedScope, DlrMainCallTarget optimizedTarget, SourceUnit sourceUnit)
            : base(null, optimizedTarget, sourceUnit) {
            ContractUtils.RequiresNotNull(optimizedScope, "optimizedScope");

            _optimizedScope = optimizedScope;
            _optimizedTarget = optimizedTarget;
        }

        public override Scope CreateScope() {
            return MakeOptimizedScope();
        }

        public override void EnsureCompiled() {
            MakeOptimizedScope();
        }

        private Scope MakeOptimizedScope() {
            Debug.Assert((_optimizedTarget == null) == (_optimizedScope == null));

            if (_optimizedScope != null) {
                return _optimizedScope;
            }

            return CompileOptimizedScope();
        }

        protected override object InvokeTarget(LambdaExpression code, Scope scope) {
            if (scope == _optimizedScope) {
                return _optimizedTarget(scope, LanguageContext);
            } else {
                // TODO: fix generated DLR ASTs
                code = new GlobalLookupRewriter().RewriteLambda(code);

                return base.InvokeTarget(code, scope);
            }
        }

        /// <summary>
        /// Creates the methods and optimized Scope's which get associated with each ScriptCode.
        /// </summary>
        private Scope CompileOptimizedScope() {
            DlrMainCallTarget target;
            IAttributesCollection globals;
            if (UseLightweightScopes) {
                CompileWithArrayGlobals(out target, out globals);
            } else {
                CompileWithStaticGlobals(out target, out globals);
            }

            // Force creation of names used in other script codes into all optimized dictionaries
            Scope scope = new Scope(globals);
            ((IModuleDictionaryInitialization)globals).InitializeModuleDictionary(new CodeContext(scope, LanguageContext));

            // everything succeeded, commit the results
            _optimizedTarget = target;
            _optimizedScope = scope;

            return scope;
        }

        private static bool UseLightweightScopes {        
            get {

                if (GlobalDlrOptions.LightweightScopes) {
                    return true;
                }

#if !SILVERLIGHT
                try {
                    // Static field compiler requires ReflectionEmit (in CLR V2) or UnmanagedCode (in CLR V2 SP1) permission.
                    // If we are running in partial-trust, fall through to generated dynamic code.
                    // TODO: Symbol information requires unmanaged code permission.  Move the error
                    // handling lower and just don't dump PDBs.
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                } catch (SecurityException) {
                    return true;
                }
#endif
                return false;
            }
        }

        private void CompileWithArrayGlobals(out DlrMainCallTarget target, out IAttributesCollection globals) {
            GlobalArrayRewriter rewriter = new GlobalArrayRewriter();
            LambdaExpression lambda = rewriter.RewriteLambda(Code);

            // Compile target
            target = LambdaCompiler.CompileLambda<DlrMainCallTarget>(lambda, SourceUnit.EmitDebugSymbols);

            // Create globals
            globals = rewriter.CreateDictionary();
        }

        private void CompileWithStaticGlobals(out DlrMainCallTarget target, out IAttributesCollection globals) {
            // Create typegen
            TypeGen typeGen = Snippets.Shared.DefineType(MakeDebugName(), typeof(CustomSymbolDictionary), false, false, SourceUnit.EmitDebugSymbols);
            typeGen.TypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            // Create rewriter
            GlobalStaticFieldRewriter rewriter = new GlobalStaticFieldRewriter(typeGen);

            // Create method for top level lambda
            MethodBuilder mb = typeGen.TypeBuilder.DefineMethod(
                "Initialize",
                CompilerHelpers.PublicStatic,
                typeof(object),
                new Type[] { typeof(Scope), typeof(LanguageContext) }
            );

            // Compile lambda
            LambdaCompiler.CompileLambda(rewriter.RewriteLambda(Code), typeGen, mb, SourceUnit.EmitDebugSymbols);

            // Create globals dictionary, finish type
            rewriter.EmitDictionary();
            Type type = typeGen.FinishType();
            globals = (IAttributesCollection)Activator.CreateInstance(type);

            // Create target
            target = (DlrMainCallTarget)Delegate.CreateDelegate(typeof(DlrMainCallTarget), type.GetMethod("Initialize"));

            // TODO: clean this up after clarifying dynamic site initialization logic
            DynamicSiteHelpers.InitializeFields(type);
        }

        protected override LambdaExpression PrepareCodeForSave(MethodBuilder builder) {
            ToDiskRewriter diskRewriter = new ToDiskRewriter();
            LambdaExpression lambda = diskRewriter.RewriteLambda(Code);

            builder.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(CachedOptimizedCodeAttribute).GetConstructor(new Type[] { typeof(string[]) }),
                new object[] { ArrayUtils.ToArray(diskRewriter.Names) }
            ));

            return lambda;
        }

        public static OptimizedScriptCode TryLoad(MethodInfo method, LanguageContext language) {
            ContractUtils.RequiresNotNull(method, "method");
            object[] attrs = method.GetCustomAttributes(typeof(CachedOptimizedCodeAttribute), false);
            if (attrs.Length != 1) {
                return null;
            }

            // create the CompilerContext for the ScriptCode
            CachedOptimizedCodeAttribute optimizedCode = (CachedOptimizedCodeAttribute)attrs[0];

            // create the storage for the global scope
            GlobalsDictionary dict = new GlobalsDictionary(SymbolTable.StringsToIds(optimizedCode.Names));

            // create the CodeContext for the code from the storage
            Scope scope = new Scope(dict);
            CodeContext context = new CodeContext(scope, language);

            // initialize the tuple
            IModuleDictionaryInitialization ici = dict as IModuleDictionaryInitialization;
            if (ici != null) {
                ici.InitializeModuleDictionary(context);
            }

            // finally generate the ScriptCode
            SourceUnit su = new SourceUnit(language, NullTextContentProvider.Null, method.Name, SourceCodeKind.File);
            return new OptimizedScriptCode(scope, (DlrMainCallTarget)Delegate.CreateDelegate(typeof(DlrMainCallTarget), method), su);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private string MakeDebugName() {
#if DEBUG
            if (SourceUnit != null && SourceUnit.HasPath) {
                return "OptScope_" + ReflectionUtils.ToValidTypeName(Path.GetFileNameWithoutExtension(IOUtils.ToValidPath(SourceUnit.Path)));
            }
#endif
            return "S";
        }

    }
}
