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

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Compiler;

namespace IronPython.Runtime {
    
    /// <summary>
    /// Captures and flows the state of executing code from the generated 
    /// Python code into the IronPython runtime.
    /// </summary>    
    public sealed class CodeContext {
        private readonly ModuleContext/*!*/ _modContext;
        private readonly PythonDictionary/*!*/ _dict;

        /// <summary>
        /// Creates a new CodeContext which is backed by the specified Python dictionary.
        /// </summary>
        public CodeContext(PythonDictionary/*!*/ dict, ModuleContext/*!*/ moduleContext) {
            ContractUtils.RequiresNotNull(dict, "dict");
            ContractUtils.RequiresNotNull(moduleContext, "moduleContext");
            _dict = dict;
            _modContext = moduleContext;
        }

        #region Public APIs

        /// <summary>
        /// Gets the module state for top-level code.
        /// </summary>   
        public ModuleContext ModuleContext {
            get {
                return _modContext;
            }
        }

        /// <summary>
        /// Gets the DLR scope object that corresponds to the global variables of this context.
        /// </summary>
        public Scope GlobalScope {
            get {
                return _modContext.GlobalScope;
            }
        }
        
        /// <summary>
        /// Gets the PythonContext which created the CodeContext.
        /// </summary>
        public PythonContext LanguageContext {
            get {
                return _modContext.Context;
            }
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        /// Gets the dictionary for the global variables from the ModuleContext.
        /// </summary>
        internal PythonDictionary GlobalDict {
            get {
                return _modContext.Globals;
            }
        }
       
        /// <summary>
        /// True if this global context should display CLR members on shared types (for example .ToString on int/bool/etc...)
        /// 
        /// False if these attributes should be hidden.
        /// </summary>
        internal bool ShowCls {
            get {
                return ModuleContext.ShowCls;
            }
            set {
                ModuleContext.ShowCls = value;
            }
        }
     
        /// <summary>
        /// Attempts to lookup the provided name in this scope or any outer scope.
        /// </summary>
        internal bool TryLookupName(SymbolId name, out object value) {
            string strName = SymbolTable.IdToString(name);
            if (_dict.TryGetValue(strName, out value)) {
                return true;
            }

            return _modContext.Globals.TryGetValue(strName, out value);
        }

        /// <summary>
        /// Looks up a global variable.  If the variable is not defined in the
        /// global scope then built-ins is consulted.
        /// </summary>
        internal bool TryLookupGlobal(SymbolId name, out object value) {
            object builtins;
            if (!GlobalDict.TryGetValue("__builtins__", out builtins)) {
                value = null;
                return false;
            }

            PythonModule builtinsScope = builtins as PythonModule;
            if (builtinsScope != null && builtinsScope.__dict__.TryGetValue(SymbolTable.IdToString(name), out value)) {
                return true;
            }

            IAttributesCollection dict = builtins as IAttributesCollection;
            if (dict != null && dict.TryGetValue(name, out value)) {
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets the dictionary used for storage of local variables.
        /// </summary>
        internal PythonDictionary Dict {
            get {
                return _dict;
            }
        }

        /// <summary>
        /// Attempts to lookup the variable in the local scope.
        /// </summary>
        internal bool TryGetVariable(SymbolId name, out object value) {
            return Dict.TryGetValue(SymbolTable.IdToString(name), out value);
        }

        /// <summary>
        /// Removes a variable from the local scope.
        /// </summary>
        internal bool TryRemoveVariable(SymbolId name) {
            return Dict.Remove(SymbolTable.IdToString(name));
        }

        /// <summary>
        /// Sets a variable in the local scope.
        /// </summary>
        internal void SetVariable(SymbolId name, object value) {
            Dict.Add(SymbolTable.IdToString(name), value);
        }

        /// <summary>
        /// Sets a variable in the local scope.
        /// </summary>
        internal void SetVariable(string name, object value) {
            Dict.Add(name, value);
        }

        /// <summary>
        /// Gets a variable from the global scope.
        /// </summary>
        internal bool TryGetGlobalVariable(SymbolId symbolId, out object res) {
            return GlobalDict.TryGetValue(SymbolTable.IdToString(symbolId), out res);
        }

        /// <summary>
        /// Sets a variable in the global scope.
        /// </summary>
        internal void SetGlobalVariable(SymbolId name, object value) {
            GlobalDict.Add(SymbolTable.IdToString(name), value);
        }

        /// <summary>
        /// Removes a variable from the global scope.
        /// </summary>
        internal bool TryRemoveGlobalVariable(SymbolId name) {
            return GlobalDict.Remove(SymbolTable.IdToString(name));
        }

        internal PythonGlobal/*!*/[] GetGlobalArray() {
            return ((GlobalDictionaryStorage)_dict._storage).Data;
        }

        internal bool IsTopLevel {
            get {
                return Dict != ModuleContext.Globals;
            }
        }

        #endregion
    }
}