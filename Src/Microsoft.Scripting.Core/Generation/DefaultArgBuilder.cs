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
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Generation {
    using Ast = Microsoft.Scripting.Ast.Expression;
    using System.Collections.Generic;

    /// <summary>
    /// ArgBuilder which provides a default parameter value for a method call.
    /// </summary>
    class DefaultArgBuilder : ArgBuilder {
        private Type _argumentType;
        private object _defaultValue;

        public DefaultArgBuilder(Type argumentType, object defaultValue) {
            this._argumentType = argumentType;
            this._defaultValue = defaultValue;
        }

        public override int Priority {
            get { return 2; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override object Build(CodeContext context, object[] args) {
            Type argType = _argumentType.IsByRef ? _argumentType.GetElementType() : _argumentType;

            if (_defaultValue is Missing) {
                if (argType.IsEnum) {
                    return Activator.CreateInstance(argType);
                }

                switch (Type.GetTypeCode(argType)) {
                    default:
                    case TypeCode.Object:
                        if (argType.IsSealed && argType.IsValueType)
                            return Activator.CreateInstance(argType);

                        if (argType == typeof(object)) return Missing.Value;

                        return null;
                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                        return null;
                    case TypeCode.Boolean: return false;
                    case TypeCode.Char: return (char)0;
                    case TypeCode.SByte: return (sbyte)0;
                    case TypeCode.Byte: return (byte)0;
                    case TypeCode.Int16: return (short)0;
                    case TypeCode.UInt16: return (ushort)0;
                    case TypeCode.Int32: return (int)0;
                    case TypeCode.UInt32: return (uint)0;
                    case TypeCode.Int64: return (long)0;
                    case TypeCode.UInt64: return (ulong)0;
                    case TypeCode.Single: return (float)0;
                    case TypeCode.Double: return (double)0;
                    case TypeCode.Decimal: return (decimal)0;
                    case TypeCode.DateTime: return new DateTime();
                    case TypeCode.String: return null;
                }
            }
            return _defaultValue;
        }

        internal override Expression ToExpression(MethodBinderContext context, IList<Expression> parameters) {
            object val = _defaultValue;
            if(val is Missing) {
                val = CompilerHelpers.GetMissingValue(_argumentType);
            }

            if (_argumentType.IsByRef) {
                VariableExpression tmp = context.GetTemporary(_argumentType.GetElementType(), "optRef");
                return Ast.Comma(
                    Ast.Assign(
                        tmp,
                        Ast.Convert(Ast.Constant(val), tmp.Type)
                    ),
                    Ast.Read(tmp)
                );
            }

            return context.ConvertExpression(Ast.Constant(val), _argumentType);            
        }
    }
}
