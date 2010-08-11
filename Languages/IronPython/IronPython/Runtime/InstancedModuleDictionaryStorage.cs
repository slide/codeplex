/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using IronPython.Compiler;

namespace IronPython.Runtime {
    /// <summary>
    /// ModuleDictionaryStorage for a built-in module which is bound to a specific instance.
    /// 
    /// These modules don't need to use PythonContext.GetModuleState() for storage and therefore
    /// can provide efficient access to internal variables.  They can also cache PythonGlobal
    /// objects and provide efficient access to module globals.  
    /// 
    /// To the end user these modules appear just like any other module.  These modules are
    /// implemented by subclassing the BuiltinPythonModule class.
    /// </summary>
    class InstancedModuleDictionaryStorage : ModuleDictionaryStorage {
        BuiltinPythonModule _module;

        public InstancedModuleDictionaryStorage(BuiltinPythonModule/*!*/ moduleInstance, Dictionary<string, PythonGlobal> globalsDict)
            : base(moduleInstance.GetType(), globalsDict) {
            _module = moduleInstance;
        }

        public override BuiltinPythonModule Instance {
            get {
                return _module;
            }
        }

    }
}
