/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Permissive License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Permissive License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Permissive License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Actions;

namespace Microsoft.Scripting.Generation {
    using Ast = Microsoft.Scripting.Ast.Ast;

    public class DefaultArgBuilder : ArgBuilder {
        private Type _argumentType;
        private object _defaultValue;

        public DefaultArgBuilder(Type argumentType, object defaultValue) {
            this._argumentType = argumentType;
            this._defaultValue = defaultValue;
        }

        public override int Priority {
            get { return 3; }
        }

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

        public override void Generate(CodeGen cg, IList<Slot> argSlots) {
            if (_argumentType.IsByRef) {
                Type baseType = _argumentType.GetElementType();
                Slot tmp = cg.GetLocalTmp(baseType);
                // Emit the default value as the base type
                EmitDefaultValue(cg, _defaultValue, baseType);
                tmp.EmitSet(cg);
                // And pass the reference to the callee
                tmp.EmitGetAddr(cg);
            } else {
                // Emit the default value directly as the argument type
                EmitDefaultValue(cg, _defaultValue, _argumentType);
            }
        }

        private static void EmitDefaultValue(CodeGen cg, object value, Type type) {
            if (value is Missing) {
                cg.EmitMissingValue(type);
            } else {
                cg.EmitConstant(value);
                //TODO This should turn into cg.EmitConvert(value.GetType(), type)
                if (type.IsValueType) {
                    if (value == null) cg.EmitTypeError("Cannot cast None to {0}", type);
                    else if (value.GetType() != type) cg.EmitTypeError("Cannot cast {0} to {1}", value, type);
                } else {
                    // null is any reference type
                    if (value != null) {
                        Type from = value.GetType();
                        if (!type.IsAssignableFrom(from)) {
                            cg.EmitTypeError("Cannot cast {0} to {1}", value, type);
                        } else {
                            if (from.IsValueType) {
                                cg.Emit(OpCodes.Box, from);
                            }
                        }
                    }
                }
            }
        }

        public override Expression ToExpression(ActionBinder binder, Expression[] parameters) {
            object val = _defaultValue;
            if(val is Missing) {
                val = CompilerHelpers.GetMissingValue(_argumentType);
            } 

            return binder.ConvertExpression(Ast.Constant(val), _argumentType);            
        }
    }
}
