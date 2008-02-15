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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Runtime {

    public delegate void CommandDispatcher(Delegate command);

    [Serializable]
    public class InvalidImplementationException : Exception {
        public InvalidImplementationException()
            : base() {
        }

        public InvalidImplementationException(string message)
            : base(message) {
        }

        public InvalidImplementationException(string message, Exception e)
            : base(message, e) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected InvalidImplementationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    [Serializable]
    public class MissingTypeException : Exception {
        public MissingTypeException() {
        }

        public MissingTypeException(string name) : this(name, null) {
        }

        public MissingTypeException(string name, Exception e) : 
            base(String.Format(Resources.MissingType, name), e) {
        }

#if !SILVERLIGHT // SerializationInfo
        protected MissingTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")] // TODO: fix
    public sealed class ScriptDomainManager {

        #region Fields and Initialization

        private readonly Dictionary<Type, ScriptEngine>/*!*/ _engines = new Dictionary<Type, ScriptEngine>(); // TODO: Key Should be LC, not Type
        private readonly PlatformAdaptationLayer/*!*/ _pal;
        private readonly IScriptHost/*!*/ _host;
        private readonly ScriptEnvironment/*!*/ _environment;
        private readonly InvariantContext/*!*/ _invariantContext;
        private readonly SharedIO/*!*/ _sharedIO;

        private CommandDispatcher _commandDispatcher; // can be null

        // TODO: ReaderWriterLock (Silverlight?)
        private readonly object _languageRegistrationLock = new object();
        private readonly Dictionary<string, LanguageRegistration> _languageIds = new Dictionary<string, LanguageRegistration>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, LanguageRegistration> _languageTypes = new Dictionary<string, LanguageRegistration>();
        private readonly List<LanguageContext> _registeredContexts = new List<LanguageContext>();

        // singletons:
        public PlatformAdaptationLayer/*!*/ PAL { get { return _pal; } }
        public ScriptEnvironment/*!*/ Environment { get { return _environment; } }
        public SharedIO/*!*/ SharedIO { get { return _sharedIO; } }
        private ScopeAttributesWrapper _scopeWrapper;
        private Scope/*!*/ _globals;
        
        private static ScriptDomainOptions _options = new ScriptDomainOptions();// TODO: remove or reduce     

        /// <summary>
        /// Initializes environment according to the setup information.
        /// </summary>
        private ScriptDomainManager(ScriptEnvironmentSetup/*!*/ setup) {
            Debug.Assert(setup != null);
            _scopeWrapper = new ScopeAttributesWrapper(this);
            _sharedIO = new SharedIO();

            _invariantContext = new InvariantContext(this);

            // create the initial default scope
            _globals = new Scope(_invariantContext, _scopeWrapper);

            // create local environment for the host:
            _environment = new ScriptEnvironment(this);

            // create PAL (default always available):
            _pal = setup.CreatePAL();

            // let setup register language contexts listed on it:
            setup.RegisterLanguages(this);

            // create a local host unless a remote one has already been created:
            _host = setup.CreateScriptHost(_environment);

            // TODO: Belongs in ScriptEnvironment but can't go there yet
            // because GetEngine is overhere for SourceUnit support
            _environment.Globals = new ScriptScope(_environment.InvariantEngine, new Scope(_invariantContext, _scopeWrapper.Dict));
        }

        public IScriptHost/*!*/ Host {
            get { return _host; }
        }

        /// <summary>
        /// Creates a new local <see cref="ScriptDomainManager"/> unless it already exists. 
        /// Returns either <c>true</c> and the newly created environment initialized according to the provided setup information
        /// or <c>false</c> and the existing one ignoring the specified setup information.
        /// </summary>
        internal static bool TryCreateLocal(ScriptEnvironmentSetup setup, out ScriptDomainManager manager) {
            manager = new ScriptDomainManager(setup ?? GetSetupInformation());

            return true;
        }

        private static ScriptEnvironmentSetup GetSetupInformation() {
#if !SILVERLIGHT
            ScriptEnvironmentSetup result;

            // setup provided by app-domain creator:
            result = ScriptEnvironmentSetup.GetAppDomainAssociated(AppDomain.CurrentDomain);
            if (result != null) {
                return result;
            }

            // setup provided in a configuration file:
            // This will load System.Configuration.dll which costs ~350 KB of memory. However, this does not normally 
            // need to be loaded in simple scenarios (like running the console hosts). Hence, the working set cost
            // is only paid in hosted scenarios.
            ScriptConfiguration config = System.Configuration.ConfigurationManager.GetSection(ScriptConfiguration.Section) as ScriptConfiguration;
            if (config != null) {
                // TODO:
                //return config;
            }
#endif

            // default setup:
            return new ScriptEnvironmentSetup(true);
        }        

        #endregion
       
        #region Language Registration

        /// <summary>
        /// Singleton for each language.
        /// </summary>
        private sealed class LanguageRegistration {
            private readonly ScriptDomainManager/*!*/ _domainManager;
            private readonly string _assemblyName;
            private readonly string _typeName;
            private LanguageContext _context;
            private Type _type;
            
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // TODO: fix
            public string AssemblyName {
                get { return _assemblyName; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // TODO: fix
            public string TypeName {
                get { return _typeName; }
            }

            public LanguageContext LanguageContext {
                get { return _context; }
            }

            public LanguageRegistration(ScriptDomainManager/*!*/ domainManager, Type type) {
                Debug.Assert(type != null);

                _type = type;
                _domainManager = domainManager;
            }

            public LanguageRegistration(ScriptDomainManager/*!*/ domainManager, string typeName, string assemblyName) {
                Debug.Assert(typeName != null && assemblyName != null);

                _assemblyName = assemblyName;
                _typeName = typeName;
                _domainManager = domainManager;
            }

            /// <summary>
            /// Must not be called under a lock as it can potentially call a user code.
            /// </summary>
            /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
            /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
            public LanguageContext/*!*/ LoadLanguageContext(ScriptDomainManager manager) {
                if (_context == null) {
                    
                    if (_type == null) {
                        try {
                            _type = _domainManager.PAL.LoadAssembly(_assemblyName).GetType(_typeName, true);
                        } catch (Exception e) {
                            throw new MissingTypeException(MakeAssemblyQualifiedName(_assemblyName, _typeName), e);
                        }
                    }

                    lock (manager._languageRegistrationLock) {
                        manager._languageTypes[_type.AssemblyQualifiedName] = this;
                    }

                    // needn't to be locked, we can create multiple LPs:
                    LanguageContext context = ReflectionUtils.CreateInstance<LanguageContext>(_type, manager);
                    Utilities.MemoryBarrier();
                    _context = context;
                }
                return _context;
            }
        }

        public void RegisterLanguageContext(string assemblyName, string typeName, params string[] identifiers) {
            RegisterLanguageContext(assemblyName, typeName, false, identifiers);
        }

        public void RegisterLanguageContext(string assemblyName, string typeName, bool overrideExistingIds, params string[] identifiers) {
            Contract.RequiresNotNull(identifiers, "identifiers");

            LanguageRegistration singleton_desc;
            bool add_singleton_desc = false;
            string aq_name = MakeAssemblyQualifiedName(typeName, assemblyName);

            lock (_languageRegistrationLock) {
                if (!_languageTypes.TryGetValue(aq_name, out singleton_desc)) {
                    add_singleton_desc = true;
                    singleton_desc = new LanguageRegistration(this, typeName, assemblyName);
                }

                // check for conflicts:
                if (!overrideExistingIds) {
                    for (int i = 0; i < identifiers.Length; i++) {
                        LanguageRegistration desc;
                        if (_languageIds.TryGetValue(identifiers[i], out desc) && !ReferenceEquals(desc, singleton_desc)) {
                            throw new InvalidOperationException("Conflicting Ids");
                        }
                    }
                }

                // add singleton LP-desc:
                if (add_singleton_desc)
                    _languageTypes.Add(aq_name, singleton_desc);

                // add id mapping to the singleton LP-desc:
                for (int i = 0; i < identifiers.Length; i++) {
                    _languageIds[identifiers[i]] = singleton_desc;
                }
            }
        }

        public bool RemoveLanguageMapping(string/*!*/ identifier) {
            Contract.RequiresNotNull(identifier, "identifier");
            
            lock (_languageRegistrationLock) {
                return _languageIds.Remove(identifier);
            }
        }

        /// <summary>
        /// Throws an exception on failure.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="type"/></exception>
        /// <exception cref="ArgumentException"><paramref name="type"/></exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        internal LanguageContext/*!*/ GetLanguageContext(Type/*!*/ type) {
            Contract.RequiresNotNull(type, "type");
            if (!type.IsSubclassOf(typeof(LanguageContext))) throw new ArgumentException("Invalid type - should be subclass of LanguageContext"); // TODO

            LanguageRegistration desc = null;
            
            lock (_languageRegistrationLock) {
                if (!_languageTypes.TryGetValue(type.AssemblyQualifiedName, out desc)) {
                    desc = new LanguageRegistration(this, type);
                    _languageTypes[type.AssemblyQualifiedName] = desc;
                }
            }

            if (desc != null) {
                return desc.LoadLanguageContext(this);
            }

            // not found, not registered:
            throw new ArgumentException(Resources.UnknownLanguageProviderType);
        }

        /// <summary>
        /// Gets the language context of the specified type.  This can be used by language implementors
        /// to get their LanguageContext for an already existing ScriptDomainManager.
        /// </summary>
        public TContextType/*!*/ GetLanguageContext<TContextType>() where TContextType : LanguageContext {
            return (TContextType)GetLanguageContext(typeof(TContextType));
        }

        internal string[] GetLanguageIdentifiers(Type type, bool extensionsOnly) {
            if (type != null && !type.IsSubclassOf(typeof(LanguageContext))) {
                throw new ArgumentException("Invalid type - should be subclass of LanguageContext"); // TODO
            }

            bool get_all = type == null;
            List<string> result = new List<string>();

            lock (_languageTypes) {
                LanguageRegistration singleton_desc = null;
                if (!get_all && !_languageTypes.TryGetValue(type.AssemblyQualifiedName, out singleton_desc)) {
                    return ArrayUtils.EmptyStrings;
                }

                foreach (KeyValuePair<string, LanguageRegistration> entry in _languageIds) {
                    if (get_all || ReferenceEquals(entry.Value, singleton_desc)) {
                        if (!extensionsOnly || IsExtensionId(entry.Key)) {
                            result.Add(entry.Key);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        /// <exception cref="ArgumentNullException"><paramref name="languageId"/></exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        public bool TryGetLanguageContext(string/*!*/ languageId, out LanguageContext languageContext) {
            Contract.RequiresNotNull(languageId, "languageId");

            bool result;
            LanguageRegistration desc;

            lock (_languageRegistrationLock) {
                result = _languageIds.TryGetValue(languageId, out desc);
            }

            languageContext = result ? desc.LoadLanguageContext(this) : null;

            return result;
        }

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public ScriptEngine GetEngineByFileExtension(string/*!*/ extension) {
            LanguageContext lc;
            if (!TryGetLanguageContextByFileExtension(extension, out lc)) {
                if (extension == null) {
                    throw new ArgumentNullException("extension");
                }
                throw new ArgumentException(Resources.UnknownLanguageId);
            }
            return GetEngine(lc);
        }

        public bool TryGetEngine(string languageId, out ScriptEngine engine) {
            LanguageContext lc;
            if (!TryGetLanguageContext(languageId, out lc)) {
                engine = null;
                return false;
            }

            engine = GetEngine(lc);
            return true;
        }

        public bool TryGetEngineByFileExtension(string extension, out ScriptEngine engine) {
            LanguageContext lc;
            if (!TryGetLanguageContextByFileExtension(extension, out lc)) {
                engine = null;
                return false;
            }

            engine = GetEngine(lc);
            return true;
        }

        public ScriptEngine GetEngine(string/*!*/ languageId) {
            Contract.RequiresNotNull(languageId, "languageId");

            LanguageContext lc;
            if (!TryGetLanguageContext(languageId, out lc)) {
                throw new ArgumentException(Resources.UnknownLanguageId);
            }

            return GetEngine(lc);
        }

        public ScriptEngine GetEngine(Type languageContextType) {
            return GetEngine(GetLanguageContext(languageContextType));
        }

        internal ScriptEngine GetEngine(LanguageContext/*!*/ language) {
            Assert.NotNull(language);
            ScriptEngine engine;
            if (!_engines.TryGetValue(language.GetType(), out engine)) {
                engine = new ScriptEngine(_environment, language);
                _engines[language.GetType()] = engine;

                if (language.GetType() != typeof(InvariantContext)) {
                    _host.EngineCreated(engine);
                }
            }
            return engine;
        }

        public bool TryGetLanguageContextByFileExtension(string extension, out LanguageContext languageContext) {
            if (String.IsNullOrEmpty(extension)) {
                languageContext = null;
                return false;
            }

            // TODO: separate hashtable for extensions (see CodeDOM config)
            if (extension[0] != '.') extension = '.' + extension;
            return TryGetLanguageContext(extension, out languageContext);
        }

        public string[] GetRegisteredFileExtensions() {
            return GetLanguageIdentifiers(null, true);
        }

        public string[] GetRegisteredLanguageIdentifiers() {
            return GetLanguageIdentifiers(null, false);
        }

        // TODO: separate hashtable for extensions (see CodeDOM config)
        private bool IsExtensionId(string id) {
            return id.StartsWith(".");
        }

        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language context's implementation failed to instantiate.</exception>
        public LanguageContext[] GetLanguageContexts(bool usedOnly) {
            List<LanguageContext> results = new List<LanguageContext>(_languageIds.Count);

            List<LanguageRegistration> to_be_loaded = usedOnly ? null : new List<LanguageRegistration>();
            
            lock (_languageRegistrationLock) {
                foreach (LanguageRegistration desc in _languageIds.Values) {
                    if (desc.LanguageContext != null) {
                        results.Add(desc.LanguageContext);
                    } else if (!usedOnly) {
                        to_be_loaded.Add(desc);
                    }
                }
            }

            if (!usedOnly) {
                foreach (LanguageRegistration desc in to_be_loaded) {
                    results.Add(desc.LoadLanguageContext(this));
                }
            }

            return results.ToArray();
        }

        private static string MakeAssemblyQualifiedName(string typeName, string assemblyName) {
            return String.Concat(typeName, ", ", assemblyName);
        }

        #endregion

        #region Variables and Modules

        /// <summary>
        /// A collection of environment variables.
        /// </summary>
        public Scope/*!*/ Globals { 
            get { 
                return _globals; 
            }
        }

        public void SetGlobalsDictionary(IAttributesCollection dictionary) {
            Contract.RequiresNotNull(dictionary, "dictionary");

            _scopeWrapper.Dict = dictionary;
        }

        public event EventHandler<AssemblyLoadedEventArgs> AssemblyLoaded;

        public bool LoadAssembly(Assembly assembly) {
            EventHandler<AssemblyLoadedEventArgs> assmLoaded = AssemblyLoaded;
            if (assmLoaded != null) {
                assmLoaded(this, new AssemblyLoadedEventArgs(assembly));
            }

            return _scopeWrapper.LoadAssembly(assembly);
        }

        public StringComparer/*!*/ PathComparer {
            get {
                return StringComparer.Ordinal;
            }
        }
        
        /// <summary>
        /// Uses the hosts search path and semantics to resolve the provided name to a SourceUnit.
        /// 
        /// If the host provides a SourceUnit which is equal to an already loaded SourceUnit the
        /// previously loaded module is returned.
        /// 
        /// Returns null if a module could not be found.
        /// </summary>
        /// <param name="name">an opaque parameter which has meaning to the host.  Typically a filename without an extension.</param>
        public object UseModule(string name) {
            Contract.RequiresNotNull(name, "name");
            
            SourceUnit su = _host.ResolveSourceFileUnit(name);
            if (su == null) {
                return null;
            }
        
            // TODO: remove (JS test in MerlinWeb relies on scope reuse)
            object result;
            if (Globals.TryGetName(SymbolTable.StringToId(name), out result)) {
                return result;
            }            
            
            result = ExecuteSourceUnit(su);
            Globals.SetName(SymbolTable.StringToId(name), result);

            return result;
        }

        /// <summary>
        /// Requests a SourceUnit from the provided path and compiles it to a ScriptScope.
        /// 
        /// If the host provides a SourceUnit which is equal to an already loaded SourceUnit the
        /// previously loaded module is returned.
        /// 
        /// Returns null if a module could not be found.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="path"/></exception>
        /// <exception cref="ArgumentException">no language registered</exception>
        /// <exception cref="MissingTypeException"><paramref name="languageId"/></exception>
        /// <exception cref="InvalidImplementationException">The language provider's implementation failed to instantiate.</exception>
        public Scope UseModule(string/*!*/ path, string/*!*/ languageId) {
            Contract.RequiresNotNull(path, "path");
            Contract.RequiresNotNull(languageId, "languageId");

            ScriptEngine engine = GetEngine(languageId);

            SourceUnit su = _host.TryGetSourceFileUnit(engine, path, StringUtils.DefaultEncoding, SourceCodeKind.File);
            if (su == null) {
                return null;
            }

            return ExecuteSourceUnit(su);
        }

        // TODO:
        public Scope/*!*/ ExecuteSourceUnit(SourceUnit/*!*/ sourceUnit) {
            ScriptCode compiledCode = sourceUnit.LanguageContext.CompileSourceCode(sourceUnit);
            Scope scope = compiledCode.MakeOptimizedScope();
            compiledCode.Run(scope);
            return scope;
        }

        #endregion

        #region Command Dispatching

        // This can be set to a method like System.Windows.Forms.Control.Invoke for Winforms scenario 
        // to cause code to be executed on a separate thread.
        // It will be called with a null argument to indicate that the console session should be terminated.
        // Can be null.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // TODO: fix
        public CommandDispatcher GetCommandDispatcher() {
            return _commandDispatcher;
        }

        public CommandDispatcher SetCommandDispatcher(CommandDispatcher dispatcher) {
            return Interlocked.Exchange(ref _commandDispatcher, dispatcher);
        }

        public void DispatchCommand(Delegate command) {
            CommandDispatcher dispatcher = _commandDispatcher;
            if (dispatcher != null) {
                dispatcher(command);
            }
        }

        #endregion

        #region TODO

        // TODO: remove or reduce
        public ScriptDomainOptions GlobalOptions {
            get {
                return _options;
            }
            set {
                Contract.RequiresNotNull(value, "value");
                _options = value;
            }
        }

        // TODO: remove or reduce
        public static ScriptDomainOptions Options {
            get { return _options; }
        }

        #endregion

        internal string[] GetRegisteredLanguageIdentifiers(LanguageContext context) {
            List<string> res = new List<string>();
            lock (_languageRegistrationLock) {
                foreach (KeyValuePair<string, LanguageRegistration> kvp in _languageIds) {
                    if (kvp.Key.StartsWith(".")) continue;

                    if (kvp.Value.LanguageContext == context) {
                        res.Add(kvp.Key);
                    }
                }
            }
            return res.ToArray();
        }

        internal string[] GetRegisteredFileExtensions(LanguageContext context) {
            // TODO: separate hashtable for extensions (see CodeDOM config)
            List<string> res = new List<string>();
            lock (_languageRegistrationLock) {
                foreach (KeyValuePair<string, LanguageRegistration> kvp in _languageIds) {
                    if (!kvp.Key.StartsWith(".")) continue;

                    if (kvp.Value.LanguageContext == context) {
                        res.Add(kvp.Key);
                    }
                }
            }            

            return res.ToArray();            
        }

        internal ContextId AssignContextId(LanguageContext lc) {
            lock(_registeredContexts) {
                int index = _registeredContexts.Count;
                _registeredContexts.Add(lc);

                return new ContextId(index + 1);
            }
        }

        private class ScopeAttributesWrapper : IAttributesCollection {
            private IAttributesCollection/*!*/ _dict = new SymbolDictionary();
            private readonly TopNamespaceTracker/*!*/ _tracker;

            public ScopeAttributesWrapper(ScriptDomainManager/*!*/ manager) {
                _tracker = new TopNamespaceTracker(manager);
            }

            public IAttributesCollection/*!*/ Dict {
                get {
                    return _dict;
                }
                set {
                    Assert.NotNull(_dict);

                    _dict = value;
                }
            }

            public bool LoadAssembly(Assembly asm) {
                return _tracker.LoadAssembly(asm);
            }

            #region IAttributesCollection Members

            public void Add(SymbolId name, object value) {
                _dict[name] = value;
            }

            public bool TryGetValue(SymbolId name, out object value) {
                if (!_dict.TryGetValue(name, out value)) {
                    value = _tracker.TryGetPackageAny(name);                    
                }
                return value != null;
            }

            public bool Remove(SymbolId name) {
                return _dict.Remove(name);
            }

            public bool ContainsKey(SymbolId name) {
                return _dict.ContainsKey(name) || _tracker.TryGetPackageAny(name) != null;
            }

            public object this[SymbolId name] {
                get {
                    object value;
                    if (TryGetValue(name, out value)) {
                        return value;
                    }

                    throw new KeyNotFoundException();
                }
                set {
                    Add(name, value);
                }
            }

            public IDictionary<SymbolId, object> SymbolAttributes {
                get { return _dict.SymbolAttributes; }
            }

            public void AddObjectKey(object name, object value) {
                _dict.AddObjectKey(name, value);
            }

            public bool TryGetObjectValue(object name, out object value) {
                return _dict.TryGetObjectValue(name, out value);
            }

            public bool RemoveObjectKey(object name) {
                return _dict.RemoveObjectKey(name);
            }

            public bool ContainsObjectKey(object name) {
                return _dict.ContainsObjectKey(name);
            }

            public IDictionary<object, object> AsObjectKeyedDictionary() {
                return _dict.AsObjectKeyedDictionary();
            }

            public int Count {
                get {
                    int count = _dict.Count + _tracker.Count;
                    foreach (object o in _tracker.Keys) {
                        if (ContainsObjectKey(o)) {
                            count--;
                        }
                    }
                    return count;
                }
            }

            public ICollection<object> Keys {
                get {
                    List<object> keys = new List<object>(_dict.Keys);
                    foreach (object o in _tracker.Keys) {
                        if (!_dict.ContainsObjectKey(o)) {
                            keys.Add(o);
                        }
                    }
                    return keys; 
                }
            }

            #endregion

            #region IEnumerable<KeyValuePair<object,object>> Members

            public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
                foreach(KeyValuePair<object, object> kvp in _dict) {
                    yield return kvp;
                }
                foreach (KeyValuePair<object, object> kvp in _tracker) {
                    if (!_dict.ContainsObjectKey(kvp.Key)) {
                        yield return kvp;
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() {
                foreach (KeyValuePair<object, object> kvp in _dict) {
                    yield return kvp.Key;
                }
                foreach (KeyValuePair<object, object> kvp in _tracker) {
                    if (!_dict.ContainsObjectKey(kvp.Key)) {
                        yield return kvp.Key;
                    }
                }
            }

            #endregion
        }
    }
}
