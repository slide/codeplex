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
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Exceptions {
    #region Generated RuntimeWarningException

    // *** BEGIN GENERATED CODE ***
    // generated by function: gen_one_exception_specialized from: generate_exceptions.py


    [Serializable]
    public class RuntimeWarningException : WarningException, IPythonAwareException {
        private object _pyExceptionObject;
        private List<DynamicStackFrame> _frames;
        private TraceBack _traceback;

        public RuntimeWarningException() : base() { }
        public RuntimeWarningException(string msg) : base(msg) { }
        public RuntimeWarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
#if !SILVERLIGHT // SerializationInfo
        protected RuntimeWarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("frames", _frames);
            info.AddValue("traceback", _traceback);
            base.GetObjectData(info, context);
        }
#endif

        object IPythonAwareException.PythonException {
            get { 
                if (_pyExceptionObject == null) {
                    var newEx = new PythonExceptions.BaseException(PythonExceptions.RuntimeWarning);
                    newEx.InitializeFromClr(this);
                    _pyExceptionObject = newEx;
                }
                return _pyExceptionObject; 
            }
            set { _pyExceptionObject = value; }
        }

        List<DynamicStackFrame> IPythonAwareException.Frames {
            get { return _frames; }
            set { _frames = value; }
        }

        TraceBack IPythonAwareException.TraceBack {
            get { return _traceback; }
            set { _traceback = value; }
        }
    }


    // *** END GENERATED CODE ***

    #endregion

}
