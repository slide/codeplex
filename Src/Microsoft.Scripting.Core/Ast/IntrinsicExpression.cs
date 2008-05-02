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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class IntrinsicExpression : Expression {
        internal IntrinsicExpression(AstNodeType nodeType, Type type)
            : base(nodeType, type) {
        }
    }

    public partial class Expression {
        public static Expression CodeContext() {
            return new IntrinsicExpression(AstNodeType.CodeContextExpression, typeof(CodeContext));
        }

        /// <summary>
        /// Get the generator instance ($gen) passed into the Generator method.
        /// </summary>
        /// <returns>returns generator instance</returns>
        /// <remarks>The generator intrinsic is the state variable passed into a generator code block.
        /// Exposing as an intrinsic allows a language to check additional state in the generator, such 
        /// as if the generator should abort, throw, or return an expression from the yield point.</remarks>
        public static Expression GeneratorInstanceExpression(Type generator) {
            return new IntrinsicExpression(AstNodeType.GeneratorIntrinsic, generator);
        }
    }
}
