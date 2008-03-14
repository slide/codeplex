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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    // TODO: Make internal?
    public sealed class CatchBlock {
        private readonly SourceLocation _start;
        private readonly SourceLocation _header;
        private readonly SourceLocation _end;
        private readonly Type /*!*/ _test;
        private readonly VariableExpression _var;
        private readonly Expression /*!*/ _body;

        internal CatchBlock(SourceSpan span, SourceLocation header, Type /*!*/ test, VariableExpression target, Expression /*!*/ body) {
            _test = test;
            _var = target;
            _body = body;
            _start = span.Start;
            _header = header;
            _end = span.End;
        }

        public SourceLocation Start {
            get { return _start; }
        }

        public SourceLocation Header {
            get { return _header; }
        }

        public SourceLocation End {
            get { return _end; }
        }

        public SourceSpan Span {
            get {
                return new SourceSpan(_start, _end);
            }
        }

        public VariableExpression Variable {
            get { return _var; }
        }

        public Type Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }
    }

    public static partial class Ast {
        public static CatchBlock Catch(Type type, Expression body) {
            return Catch(SourceSpan.None, SourceLocation.None, type, null, body);
        }

        public static CatchBlock Catch(Type type, VariableExpression target, Expression body) {
            return Catch(SourceSpan.None, SourceLocation.None, type, target, body);
        }

        public static CatchBlock Catch(SourceSpan span, SourceLocation header, Type type, VariableExpression target, Expression body) {
            Contract.RequiresNotNull(type, "type");
            Contract.Requires(target == null || TypeUtils.CanAssign(target.Type, type), "target");
            Contract.RequiresNotNull(body, "body");
            return new CatchBlock(span, header, type, target, body);
        }
    }
}