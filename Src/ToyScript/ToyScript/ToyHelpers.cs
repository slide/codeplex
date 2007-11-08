/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using Microsoft.Scripting;

using ToyScript.Runtime;

namespace ToyScript {
    public static class ToyHelpers {
        public static void Print(object o) {
            Console.WriteLine(o ?? "<null>");
        }

        public static Type DateTime {
            get { return typeof(DateTime); }
        }

        public static object Import(string name) {
            return RuntimeHelpers.TopNamespace.TryGetPackage(name);
        }

        public static object GetItem(object target, object index) {
            Type type = target.GetType();
            MethodInfo method = type.GetMethod("get_Item");
            if (method != null) {
                return method.Invoke(target, new object[] { index });
            } else {
                throw new InvalidOperationException("Cannot get item from " + type.Name);
            }
        }

        public static object SetItem(object target, object index, object value) {
            Type type = target.GetType();
            MethodInfo method = type.GetMethod("set_Item");
            if (method != null) {
                method.Invoke(target, new object[] { index, value });
            } else {
                throw new InvalidOperationException("Cannot set item on " + type.Name);
            }
            return value;
        }
    }
}
