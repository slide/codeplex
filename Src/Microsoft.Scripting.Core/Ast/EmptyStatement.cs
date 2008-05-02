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

namespace Microsoft.Scripting.Ast {
    public sealed class EmptyStatement : Expression {

        internal EmptyStatement(Annotations annotations)
            : base(annotations, AstNodeType.EmptyStatement, typeof(void)) {
        }
    }

    public partial class Expression {
        public static EmptyStatement Empty() {
            return Empty(SourceSpan.None);
        }

        public static EmptyStatement Empty(SourceSpan span) {
            return new EmptyStatement(Annotate(span));
        }
    }
}
