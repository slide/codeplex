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

using System.Scripting.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types {
    public static class DelegateOps {
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType type, object function) {
            if (type == null) throw PythonOps.TypeError("expected type for 1st param, got {0}", type.Name);

            return BinderOps.GetDelegate(context, function, type.UnderlyingSystemType);
        }
    }    
}
