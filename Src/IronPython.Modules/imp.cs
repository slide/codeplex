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
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Runtime.CompilerServices;


using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("imp", typeof(IronPython.Modules.PythonImport))]
namespace IronPython.Modules {
    public static class PythonImport {
        internal const int PythonSource = 1;
        internal const int PythonCompiled = 2;
        internal const int CExtension = 3;
        internal const int PythonResource = 4;
        internal const int PackageDirectory = 5;
        internal const int CBuiltin = 6;
        internal const int PythonFrozen = 7;
        internal const int PythonCodeResource = 8;
        internal const int SearchError = 0;
        internal const int ImporterHook = 9;
        private static readonly object _lockCountKey = new object();

        [SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, IAttributesCollection/*!*/ dict) {
            // set the lock count to zero on the 1st load, don't reset the lock count on reloads
            if (!context.HasModuleState(_lockCountKey)) {
                context.SetModuleState(_lockCountKey, 0L);
            }
        }

        public static string get_magic() {
            return "";
        }

        public static List get_suffixes() {
            return List.FromArrayNoCopy(PythonOps.MakeTuple(".py", "U", PythonSource));
        }

        public static PythonTuple find_module(CodeContext/*!*/ context, string/*!*/ name) {
            if (name == null) throw PythonOps.TypeError("find_module() argument 1 must be string, not None");
            return FindBuiltinOrSysPath(context, name);
        }

        public static PythonTuple find_module(CodeContext/*!*/ context, string/*!*/ name, List path) {
            if (name == null) throw PythonOps.TypeError("find_module() argument 1 must be string, not None");

            if (path == null) {
                return FindBuiltinOrSysPath(context, name);
            } else {
                return FindModulePath(context, name, path);
            }
        }

        public static object load_module(CodeContext/*!*/ context, string name, PythonFile file, string filename, PythonTuple/*!*/ description) {
            if (description == null) {
                throw PythonOps.TypeError("load_module() argument 4 must be 3-item sequence, not None");
            }
            if (description.__len__() != 3) {
                throw PythonOps.TypeError("load_module() argument 4 must be sequence of length 3, not {0}", description.__len__());
            }

            PythonContext pythonContext = PythonContext.GetContext(context);

            // already loaded? do reload()
            PythonModule module = pythonContext.GetModuleByName(name);
            if (module != null) {
                Importer.ReloadModule(context, module.Scope);
                return module.Scope;
            }

            int type = PythonContext.GetContext(context).ConvertToInt32(description[2]);
            switch (type) {
                case PythonSource:
                    return LoadPythonSource(pythonContext, name, file, filename);
                case CBuiltin:
                    return LoadBuiltinModule(context, name);
                case PackageDirectory:
                    return LoadPackageDirectory(pythonContext, name, filename);
                default:
                    throw PythonOps.TypeError("don't know how to import {0}, (type code {1}", name, type);
            }
        }

        [Documentation("new_module(name) -> module\nCreates a new module without adding it to sys.modules.")]
        public static Scope/*!*/ new_module(CodeContext/*!*/ context, string/*!*/ name) {
            if (name == null) throw PythonOps.TypeError("new_module() argument 1 must be string, not None");

            Scope res = PythonContext.GetContext(context).CreateModule().Scope;
            res.SetName(Symbols.Name, name);
            return res;
        }

        public static bool lock_held(CodeContext/*!*/ context) {
            return GetLockCount(context) != 0;
        }

        public static void acquire_lock(CodeContext/*!*/ context) {
            lock (_lockCountKey) {
                SetLockCount(context, GetLockCount(context) + 1);
            }
        }

        public static void release_lock(CodeContext/*!*/ context) {
            lock (_lockCountKey) {
                long lockCount = GetLockCount(context);
                if (lockCount == 0) {
                    throw PythonOps.RuntimeError("not holding the import lock");
                }
                SetLockCount(context, lockCount - 1);
            }            
        }

        public const int PY_SOURCE = PythonSource;
        public const int PY_COMPILED = PythonCompiled;
        public const int C_EXTENSION = CExtension;
        public const int PY_RESOURCE = PythonResource;
        public const int PKG_DIRECTORY = PackageDirectory;
        public const int C_BUILTIN = CBuiltin;
        public const int PY_FROZEN = PythonFrozen;
        public const int PY_CODERESOURCE = PythonCodeResource;
        public const int SEARCH_ERROR = SearchError;
        public const int IMP_HOOK = ImporterHook;

        public static object init_builtin(CodeContext/*!*/ context, string/*!*/ name) {
            if (name == null) throw PythonOps.TypeError("init_builtin() argument 1 must be string, not None");
            return LoadBuiltinModule(context, name);
        }

        public static object init_frozen(string name) {
            return null;
        }

        public static object get_frozen_object(string name) {
            throw PythonOps.ImportError("No such frozen object named {0}", name);
        }

        public static int is_builtin(CodeContext/*!*/ context, string/*!*/ name) {
            if (name == null) throw PythonOps.TypeError("is_builtin() argument 1 must be string, not None");
            Type ty;
            if (PythonContext.GetContext(context).Builtins.TryGetValue(name, out ty)) {
                if (ty.Assembly == typeof(PythonContext).Assembly) {
                    // supposedly these can't be re-initialized and return -1 to
                    // indicate that here, but CPython does allow passing them
                    // to init_builtin.
                    return -1;
                }

                
                return 1;
            }
            return 0;
        }

        public static bool is_frozen(string name) {
            return false;
        }

        public static object load_compiled(string name, string pathname) {
            return null;
        }

        public static object load_compiled(string name, string pathname, PythonFile file) {
            return null;
        }

        public static object load_dynamic(string name, string pathname) {
            return null;
        }

        public static object load_dynamic(string name, string pathname, PythonFile file) {
            return null;
        }
        
        public static object load_package(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ pathname) {
            if (name == null) throw PythonOps.TypeError("load_package() argument 1 must be string, not None");
            if (pathname == null) throw PythonOps.TypeError("load_package() argument 2 must be string, not None");

            return (Importer.LoadPackageFromSource(context, name, pathname) ??
                    CreateEmptyPackage(context, name, pathname)).Scope;
        }

        private static PythonModule/*!*/ CreateEmptyPackage(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ pathname) {
            PythonContext pc = PythonContext.GetContext(context);

            PythonModule mod = pc.CreateModule();
            Scope scope = mod.Scope;
            scope.SetName(Symbols.Name, name);
            scope.SetName(Symbols.Path, pathname);

            pc.SystemStateModules[name] = scope;

            return mod;
        }

        public static object load_source(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ pathname) {
            if (name == null) throw PythonOps.TypeError("load_source() argument 1 must be string, not None");
            if (pathname == null) throw PythonOps.TypeError("load_source() argument 2 must be string, not None");
            
            // TODO: is this supposed to open PythonFile with Python-specific behavior?
            // we may need to insert additional layer to SourceUnit content provider if so
            PythonContext pc = PythonContext.GetContext(context);
            if (!pc.DomainManager.Platform.FileExists(pathname)) {
                throw PythonOps.IOError("Couldn't find file: {0}", pathname);
            }

            SourceUnit sourceUnit = pc.CreateFileUnit(pathname, pc.DefaultEncoding, SourceCodeKind.File);
            return pc.CompileModule(pathname, name, sourceUnit, ModuleOptions.Initialize).Scope;
        }

        public static object load_source(CodeContext/*!*/ context, string/*!*/ name, string/*!*/ pathname, PythonFile/*!*/ file) {
            if (name == null) throw PythonOps.TypeError("load_source() argument 1 must be string, not None");
            if (pathname == null) throw PythonOps.TypeError("load_source() argument 2 must be string, not None");
            if (pathname == null) throw PythonOps.TypeError("load_source() argument 3 must be file, not None");
            
            return LoadPythonSource(PythonContext.GetContext(context), name, file, pathname);
        }

        public static PythonType NullImporter {
            get {
                return DynamicHelpers.GetPythonTypeFromType(typeof(NullImporter));
            }
        }

        #region Implementation

        private static PythonTuple FindBuiltinOrSysPath(CodeContext/*!*/ context, string/*!*/ name) {
            List sysPath;
            if (!PythonContext.GetContext(context).TryGetSystemPath(out sysPath)) {
                throw PythonOps.ImportError("sys.path must be a list of directory names");
            }
            return FindModuleBuiltinOrPath(context, name, sysPath);
        }

        private static PythonTuple FindModulePath(CodeContext/*!*/ context, string name, List path) {
            Debug.Assert(path != null);

            if (name == null) {
                throw PythonOps.TypeError("find_module() argument 1 must be string, not None");
            }

            PlatformAdaptationLayer pal = context.LanguageContext.DomainManager.Platform;
            foreach (object d in path) {
                string dir = d as string;
                if (dir == null) continue;  // skip invalid entries

                string pathName = Path.Combine(dir, name);
                if (pal.DirectoryExists(pathName)) {
                    if (pal.FileExists(Path.Combine(pathName, "__init__.py"))) {
                        return PythonTuple.MakeTuple(null, pathName, PythonTuple.MakeTuple("", "", PackageDirectory));
                    }
                }

                string fileName = pathName + ".py";
                if (pal.FileExists(fileName)) {
                    Stream fs = pal.OpenInputFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    PythonFile pf = PythonFile.Create(context, fs, fileName, "U");
                    return PythonTuple.MakeTuple(pf, fileName, PythonTuple.MakeTuple(".py", "U", PythonSource));
                }
            }
            throw PythonOps.ImportError("No module named {0}", name);
        }

        private static PythonTuple FindModuleBuiltinOrPath(CodeContext/*!*/ context, string name, List path) {
            if (name.Equals("sys")) return BuiltinModuleTuple(name);
            if (name.Equals("clr")) {
                PythonContext.EnsureModule(context).ShowCls = true;
                return BuiltinModuleTuple(name);
            }
            Type ty;
            if (PythonContext.GetContext(context).Builtins.TryGetValue(name, out ty)) {
                return BuiltinModuleTuple(name);
            }

            return FindModulePath(context, name, path);
        }

        private static PythonTuple BuiltinModuleTuple(string name) {
            return PythonTuple.MakeTuple(null, name, PythonTuple.MakeTuple("", "", CBuiltin));
        }

        private static Scope/*!*/ LoadPythonSource(PythonContext/*!*/ context, string/*!*/ name, PythonFile/*!*/ file, string/*!*/ fileName) {
            SourceUnit sourceUnit = context.CreateSnippet(file.read(), String.IsNullOrEmpty(fileName) ? null : fileName, SourceCodeKind.File);
            return context.CompileModule(fileName, name, sourceUnit, ModuleOptions.Initialize).Scope;
        }

        private static Scope/*!*/ LoadPackageDirectory(PythonContext/*!*/ context, string moduleName, string path) {
            string initPath = Path.Combine(path, "__init__.py");
            
            SourceUnit sourceUnit =  context.CreateFileUnit(initPath, context.DefaultEncoding);
            return context.CompileModule(initPath, moduleName, sourceUnit, ModuleOptions.Initialize).Scope;
        }

        private static object LoadBuiltinModule(CodeContext/*!*/ context, string/*!*/ name) {
            Assert.NotNull(context, name);
            return Importer.ImportBuiltin(context, name);
        }

        #endregion

        private static long GetLockCount(CodeContext/*!*/ context) {
            return (long)PythonContext.GetContext(context).GetModuleState(_lockCountKey);
        }

        private static void SetLockCount(CodeContext/*!*/ context, long lockCount) {
            PythonContext.GetContext(context).SetModuleState(_lockCountKey, lockCount);
        }
    }
}
