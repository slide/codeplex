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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Threading;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

    public interface IScriptHost : IRemotable {
        // virtual file-system ops:
        string NormalizePath(string path);  // throws ArgumentException
        string[] GetSourceFileNames(string mask, string searchPattern);
        
        // source units:
        SourceFileUnit TryGetSourceFileUnit(IScriptEngine engine, string path, string name);
        SourceFileUnit ResolveSourceFileUnit(string name);

        // notifications:
        void EngineCreated(IScriptEngine engine);
        void ModuleCreated(IScriptModule module);

        // environment variables:
        bool TrySetVariable(IScriptEngine engine, SymbolId name, object value);
        bool TryGetVariable(IScriptEngine engine, SymbolId name, out object value);
        
        /// <summary>
        /// Default module is provided by the host.
        /// For some hosting scenarios, the default module is not necessary so the host needn't to implement this method. 
        /// The default module should be created lazily as the environment is not prepared for module creation at the time the 
        /// host tis created (the host is created prior module creation so that it could be notified about the creation).
        /// </summary>
        IScriptModule DefaultModule { get; } // throws InvalidOperationException if no default module is available
    }

    public class ScriptHost : IScriptHost, ILocalObject {

        /// <summary>
        /// The environment the host is attached to.
        /// </summary>
        private IScriptEnvironment _environment;
        private ScriptModule _defaultModule;

        /// <summary>
        /// Default module for convenience. Lazily init'd.
        /// </summary>
        public virtual IScriptModule DefaultModule {
            get {
                if (_defaultModule == null) {
                    if (Utilities.IsRemote(_environment)) 
                        throw new InvalidOperationException("Default module should by created in the remote appdomain.");

                    CreateDefaultModule(ref _defaultModule);
                 }

                return _defaultModule;
            }
        }
        
        #region Construction

        public ScriptHost(IScriptEnvironment environment) {
            if (environment == null) throw new ArgumentNullException("environment");
            _environment = environment;
            _defaultModule = null;
        }

        internal ScriptHost() {
            _environment = null;
        }

        #endregion

#if !SILVERLIGHT
        RemoteWrapper ILocalObject.Wrap() {
            return new RemoteScriptHost(this);
        }
#endif
        static internal void CreateDefaultModule(ref ScriptModule defaultModule) {
           // create a module and throw it away if there is already one:
            ScriptModule module = ScriptDomainManager.CurrentManager.CreateModule("<default>", null, ScriptCode.EmptyArray);
            Utilities.MemoryBarrier();
            Interlocked.CompareExchange<ScriptModule>(ref defaultModule, module, null);
        }

        #region Virtual File System

        /// <summary>
        /// Normalizes a specified path.
        /// </summary>
        /// <param name="path">Path to normalize.</param>
        /// <returns>Normalized path.</returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is not a valid path.</exception>
        /// <remarks>
        /// Normalization should be idempotent, i.e. NormalizePath(NormalizePath(path)) == NormalizePath(path) for any valid path.
        /// </remarks>
        public virtual string NormalizePath(string path) {
            return (path != "") ? ScriptDomainManager.CurrentManager.PAL.GetFullPath(path) : "";
        }

        public virtual string[] GetSourceFileNames(string mask, string searchPattern) {
            return ScriptDomainManager.CurrentManager.PAL.GetFiles(mask, searchPattern);
        }

        #endregion

        #region Source Unit Resolving

        public const string PathEnvironmentVariableName = "DLRPATH";
        
        /// <summary>
        /// Gets the default path used for searching for source units.
        /// </summary>
        internal protected virtual IList<string> SourceUnitResolutionPath {
            get {
#if SILVERLIGHT
                return new string[] { "." };
#else
                return (System.Environment.GetEnvironmentVariable(PathEnvironmentVariableName) ?? ".").Split(Path.PathSeparator);
#endif
            }
        }

        public virtual SourceFileUnit TryGetSourceFileUnit(IScriptEngine engine, string path, string name) {
            if (ScriptDomainManager.CurrentManager.PAL.FileExists(path)) {
                return new SourceFileUnit(engine, path, name, Encoding.Default);
            }
            return null;
        }

        /// <summary>
        /// Loads the module of the given name using the host provided semantics.
        /// 
        /// The default semantics are to search the host path for a file of the specified
        /// name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A valid SourceUnit or null no module could be found.</returns>
        /// <exception cref="System.InvalidOperationException">An ambigious module match has occured</exception>
        public virtual SourceFileUnit ResolveSourceFileUnit(string name) {
            if (name == null) throw new ArgumentNullException("name");

            SourceFileUnit result = null;

            foreach (string directory in SourceUnitResolutionPath) {

                string final_path = null;

                foreach (string extension in _environment.GetRegisteredFileExtensions()) {
                    string full_path = Path.Combine(directory, name + extension);

                    if (ScriptDomainManager.CurrentManager.PAL.FileExists(full_path)) {
                        if (result != null) {
                            throw new InvalidOperationException(String.Format(Resources.AmbigiousModule, full_path, final_path));
                        }

                        LanguageProvider provider;
                        if (!ScriptDomainManager.CurrentManager.TryGetLanguageProviderByFileExtension(extension, out provider)) {
                            // provider may have been unregistered, let's pick another one: 
                            continue;    
                        }

                        result = new SourceFileUnit(provider.GetEngine(), full_path, name, Encoding.Default);
                        final_path = full_path;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Notifications

        public virtual void EngineCreated(IScriptEngine engine) {
            // nop
        }

        public virtual void ModuleCreated(IScriptModule module) {
            // nop
        }

        #endregion

        #region Variables

        public virtual bool TrySetVariable(IScriptEngine engine, SymbolId name, object value) {
            return false;
        }

        public virtual bool TryGetVariable(IScriptEngine engine, SymbolId name, out object value) {
            value = null;
            return false;
        }

        #endregion
    }

}
