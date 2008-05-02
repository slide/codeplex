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
using System.Collections.Generic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;

using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Operations {
    public static partial class PythonOps {
        #region Generated Exception Factories

        // *** BEGIN GENERATED CODE ***
        // generated by function: factory_gen from: generate_exceptions.py


        public static Exception ImportError(string format, params object[] args) {
            return new ImportException(string.Format(format, args));
        }

        public static Exception RuntimeError(string format, params object[] args) {
            return new RuntimeException(string.Format(format, args));
        }

        public static Exception UnicodeTranslateError(string format, params object[] args) {
            return new UnicodeTranslateException(string.Format(format, args));
        }

        public static Exception PendingDeprecationWarning(string format, params object[] args) {
            return new PendingDeprecationWarningException(string.Format(format, args));
        }

        public static Exception EnvironmentError(string format, params object[] args) {
            return new EnvironmentException(string.Format(format, args));
        }

        public static Exception LookupError(string format, params object[] args) {
            return new LookupException(string.Format(format, args));
        }

        public static Exception OSError(string format, params object[] args) {
            return new OSException(string.Format(format, args));
        }

        public static Exception DeprecationWarning(string format, params object[] args) {
            return new DeprecationWarningException(string.Format(format, args));
        }

        public static Exception UnicodeError(string format, params object[] args) {
            return new UnicodeException(string.Format(format, args));
        }

        public static Exception FloatingPointError(string format, params object[] args) {
            return new FloatingPointException(string.Format(format, args));
        }

        public static Exception ReferenceError(string format, params object[] args) {
            return new ReferenceException(string.Format(format, args));
        }

        public static Exception FutureWarning(string format, params object[] args) {
            return new FutureWarningException(string.Format(format, args));
        }

        public static Exception AssertionError(string format, params object[] args) {
            return new AssertionException(string.Format(format, args));
        }

        public static Exception RuntimeWarning(string format, params object[] args) {
            return new RuntimeWarningException(string.Format(format, args));
        }

        public static Exception ImportWarning(string format, params object[] args) {
            return new ImportWarningException(string.Format(format, args));
        }

        public static Exception UserWarning(string format, params object[] args) {
            return new UserWarningException(string.Format(format, args));
        }

        public static Exception SyntaxWarning(string format, params object[] args) {
            return new SyntaxWarningException(string.Format(format, args));
        }

        public static Exception OverflowWarning(string format, params object[] args) {
            return new OverflowWarningException(string.Format(format, args));
        }

        public static Exception UnicodeWarning(string format, params object[] args) {
            return new UnicodeWarningException(string.Format(format, args));
        }

        public static Exception StopIteration(string format, params object[] args) {
            return new StopIterationException(string.Format(format, args));
        }

        public static Exception Warning(string format, params object[] args) {
            return new WarningException(string.Format(format, args));
        }

        // *** END GENERATED CODE ***

        #endregion
    }
}
