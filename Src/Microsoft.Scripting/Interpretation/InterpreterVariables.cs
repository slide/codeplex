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

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Interpretation {

    /// <summary>
    /// An ILocalVariables implementation for the interpreter
    /// 
    /// TODO: This isn't quite correct, because it doesn't implement the
    /// LocalScopeExpression.IsClosure feature that only exposes variables that
    /// would otherwise be lifted. To implement it correctly would require a
    /// full variable binding pass, something the interpreter doesn't need
    /// today. The only thing that this breaks is Python's func_closure
    /// </summary>
    internal sealed class InterpreterVariables : ILocalVariables {
        private readonly InterpreterState _state;
        private readonly ReadOnlyCollection<Expression> _vars;
        private ReadOnlyCollection<string> _names;

        internal InterpreterVariables(InterpreterState state, LocalScopeExpression node) {
            _state = state;
            _vars = node.Variables;
        }

        public int Count {
            get { return _vars.Count; }
        }

        public ReadOnlyCollection<string> Names {
            get {
                if (_names == null) {
                    string[] names = new string[_vars.Count];
                    for (int i = 0; i < _vars.Count; i++) {
                        if (_vars[i].NodeType == ExpressionType.Variable) {
                            names[i] = ((VariableExpression)_vars[i]).Name;
                        } else {
                            names[i] = ((ParameterExpression)_vars[i]).Name;
                        }
                    }
                    _names = new ReadOnlyCollection<string>(names);
                }
                return _names;
            }
        }

        public IStrongBox this[int index] {
            get {
                return new InterpreterBox(_state, _vars[index]);
            }
        }

        // TODO: InterpreterState should store values in strongly typed
        // StrongBox<T>, which gives us the correct cast error if the wrong
        // type is set at runtime.
        private sealed class InterpreterBox : IStrongBox {
            private readonly InterpreterState _state;
            private readonly Expression _variable;

            internal InterpreterBox(InterpreterState state, Expression variable) {
                _state = state;
                _variable = variable;
            }

            public object Value {
                get { return _state.GetValue(_variable); }
                set { _state.SetValue(_variable, value); }
            }
        }
    }
}
