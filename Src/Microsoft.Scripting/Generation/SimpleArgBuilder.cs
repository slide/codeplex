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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    /// <summary>
    /// SimpleArgBuilder produces the value produced by the user as the argument value.  It
    /// also tracks information about the original parameter and is used to create extended
    /// methods for params arrays and param dictionary functions.
    /// </summary>
    class SimpleArgBuilder : ArgBuilder {
        private int _index;
        private Type _parameterType;
        private bool _isParams, _isParamsDict;

        public SimpleArgBuilder(int index, Type parameterType) {
            _index = index;
            _parameterType = parameterType;
        }

        public SimpleArgBuilder(int index, Type parameterType, bool isParams, bool isParamsDict) {
            _index = index;
            _parameterType = parameterType;
            _isParams = isParams;
            _isParamsDict = isParamsDict;
        }

        public SimpleArgBuilder(int index, Type parameterType, ParameterInfo paramInfo) {
            if (index < 0) throw new ArgumentOutOfRangeException("index");
            ContractUtils.RequiresNotNull(parameterType, "parameterType");

            _index = index;
            _parameterType = parameterType;
            _isParams = CompilerHelpers.IsParamArray(paramInfo);
            _isParamsDict = BinderHelpers.IsParamDictionary(paramInfo);
        }

        public override int Priority {
            get { return 0; }
        }

        public bool IsParamsArray {
            get {
                return _isParams;
            }
        }

        public bool IsParamsDict {
            get {
                return _isParamsDict;
            }
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters, bool[] hasBeenUsed) {
            Debug.Assert(_index < parameters.Count);
            Debug.Assert(_index < hasBeenUsed.Length);
            Debug.Assert(parameters[_index] != null);
            hasBeenUsed[_index] = true;
            return context.ConvertExpression(parameters[_index], _parameterType);
        }

        public int Index {
            get {
                return _index;
            }
        }

        public override Type Type {
            get {
                return _parameterType;
            }
        }
    }
}