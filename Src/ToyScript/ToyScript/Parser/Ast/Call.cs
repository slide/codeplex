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

using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace ToyScript.Parser.Ast {
    using Ast = MSAst.Ast;

    class Call : Expression {
        private readonly Expression _target;
        private readonly Expression[] _arguments;

        public Call(SourceSpan span, Expression target, Expression[] arguments)
            : base(span) {
            _target = target;
            _arguments = arguments;
        }

        protected internal override MSAst.Expression Generate(ToyGenerator tg) {
            MSAst.Expression[] arguments = new MSAst.Expression[_arguments.Length + 1];

            arguments[0] = _target.Generate(tg);
            for (int i = 0; i < _arguments.Length; i++) {
                arguments[i+1] = _arguments[i].Generate(tg);
            }

            // TODO: Invoke or call?
            return Ast.Action.Call(
                typeof(object),
                arguments
            );
        }
    }
}
