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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.ObjectModel;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    partial class LambdaCompiler {
        /// <summary>
        /// Generates code for this expression in a value position.
        /// This method will leave the value of the expression
        /// on the top of the stack typed as Type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal void EmitExpression(Expression node) {
            Debug.Assert(node != null);

            switch (node.NodeType) {
                case AstNodeType.AndAlso:
                    EmitBooleanOperator((BinaryExpression)node, true);
                    break;

                case AstNodeType.OrElse:
                    EmitBooleanOperator((BinaryExpression)node, false);
                    break;

                case AstNodeType.Add:
                case AstNodeType.And:
                case AstNodeType.ArrayIndex:
                case AstNodeType.Divide:
                case AstNodeType.Equal:
                case AstNodeType.ExclusiveOr:
                case AstNodeType.GreaterThan:
                case AstNodeType.GreaterThanOrEqual:
                case AstNodeType.LeftShift:
                case AstNodeType.LessThan:
                case AstNodeType.LessThanOrEqual:
                case AstNodeType.Modulo:
                case AstNodeType.Multiply:
                case AstNodeType.NotEqual:
                case AstNodeType.Or:
                case AstNodeType.RightShift:
                case AstNodeType.Subtract:
                    Emit((BinaryExpression)node);
                    break;

                case AstNodeType.Call:
                    Emit((MethodCallExpression)node);
                    break;

                case AstNodeType.Conditional:
                    Emit((ConditionalExpression)node);
                    break;

                case AstNodeType.Constant:
                    Emit((ConstantExpression)node);
                    break;

                case AstNodeType.Convert:
                case AstNodeType.Negate:
                case AstNodeType.Not:
                case AstNodeType.OnesComplement:
                    Emit((UnaryExpression)node);
                    break;

                case AstNodeType.New:
                    Emit((NewExpression)node);
                    break;

                case AstNodeType.TypeIs:
                    Emit((TypeBinaryExpression)node);
                    break;

                case AstNodeType.ActionExpression:
                    Emit((ActionExpression)node);
                    break;

                case AstNodeType.ArrayIndexAssignment:
                    Emit((ArrayIndexAssignment)node);
                    break;

                case AstNodeType.BoundAssignment:
                    Emit((BoundAssignment)node);
                    break;

                case AstNodeType.BoundExpression:
                    Emit((BoundExpression)node);
                    break;

                case AstNodeType.CodeBlockExpression:
                    Emit((CodeBlockExpression)node);
                    break;

                case AstNodeType.CodeContextExpression:
                    EmitCodeContext();
                    break;

                case AstNodeType.GeneratorIntrinsic:
                    EmitGeneratorIntrinsic();
                    break;

                case AstNodeType.DeleteUnboundExpression:
                    Emit((DeleteUnboundExpression)node);
                    break;

                case AstNodeType.EnvironmentExpression:
                    EmitEnvironmentExpression();
                    break;

                case AstNodeType.MemberAssignment:
                    Emit((MemberAssignment)node);
                    break;

                case AstNodeType.MemberExpression:
                    Emit((MemberExpression)node);
                    break;

                case AstNodeType.NewArrayExpression:
                case AstNodeType.NewArrayBounds:
                    Emit((NewArrayExpression)node);
                    break;

                case AstNodeType.UnboundAssignment:
                    Emit((UnboundAssignment)node);
                    break;

                case AstNodeType.UnboundExpression:
                    Emit((UnboundExpression)node);
                    break;

                case AstNodeType.Block:
                    Emit((Block)node);
                    break;

                case AstNodeType.BreakStatement:
                    Emit((BreakStatement)node);
                    break;

                case AstNodeType.ContinueStatement:
                    Emit((ContinueStatement)node);
                    break;

                case AstNodeType.DeleteStatement:
                    Emit((DeleteStatement)node);
                    break;

                case AstNodeType.DoStatement:
                    Emit((DoStatement)node);
                    break;

                case AstNodeType.EmptyStatement:
                    Emit((EmptyStatement)node);
                    break;

                case AstNodeType.ExpressionStatement:
                    Emit((ExpressionStatement)node);
                    break;

                case AstNodeType.LabeledStatement:
                    Emit((LabeledStatement)node);
                    break;

                case AstNodeType.LoopStatement:
                    Emit((LoopStatement)node);
                    break;

                case AstNodeType.ReturnStatement:
                    Emit((ReturnStatement)node);
                    break;

                case AstNodeType.ScopeStatement:
                    Emit((ScopeStatement)node);
                    break;

                case AstNodeType.SwitchStatement:
                    Emit((SwitchStatement)node);
                    break;

                case AstNodeType.ThrowStatement:
                    Emit((ThrowStatement)node);
                    break;

                case AstNodeType.TryStatement:
                    Emit((TryStatement)node);
                    break;

                case AstNodeType.YieldStatement:
                    Emit((YieldStatement)node);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Generates the code for the expression, leaving it on
        /// the stack typed as object.
        /// </summary>
        private void EmitExpressionAsObject(Expression node) {
            EmitExpression(node);
            EmitBoxing(node.Type);
        }

        #region BinaryExpression

        private void Emit(BinaryExpression node) {
            Debug.Assert(node.NodeType != AstNodeType.AndAlso && node.NodeType != AstNodeType.OrElse);

            if (NullableVsNull(node.Left, node.Right)) {
                EmitExpressionAddress(node.Left, node.Left.Type);

                GenerateNullableBinaryOperator(node.NodeType, node.Left.Type);
            } else if (NullableVsNull(node.Right, node.Left)) {
                // null vs Nullable<T>
                EmitExpressionAddress(node.Right, node.Right.Type);

                GenerateNullableBinaryOperator(node.NodeType, node.Right.Type);
            } else {
                EmitExpression(node.Left);
                EmitExpression(node.Right);

                if (node.Method != null) {
                    EmitCall(node.Method);
                } else {
                    GenerateBinaryOperator(node.NodeType, node.Type);
                }
            }
        }

        private void GenerateNullableBinaryOperator(AstNodeType astNodeType, Type nullableType) {
            switch(astNodeType) {
                case AstNodeType.NotEqual:
                    EmitPropertyGet(nullableType, "HasValue");
                    break;
                case AstNodeType.Equal:
                    EmitPropertyGet(nullableType, "HasValue");
                    EmitBoolean(false);
                    Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new InvalidOperationException(astNodeType.ToString());
            }
        }

        private static bool NullableVsNull(Expression nullable, Expression nullVal) {
            return TypeUtils.IsNullableType(nullable.Type) && ConstantCheck.IsConstant(nullVal, null);
        }

        private void EmitBooleanOperator(BinaryExpression node, bool isAnd) {
            Label otherwise = DefineLabel();
            Label endif = DefineLabel();

            // if (_left) 
            EmitBranchFalse(node.Left, otherwise);
            // then

            if (isAnd) {
                EmitExpression(node.Right);
            } else {
                EmitInt(1);
            }

            Emit(OpCodes.Br, endif);
            // otherwise
            MarkLabel(otherwise);

            if (isAnd) {
                EmitInt(0);
            } else {
                EmitExpression(node.Right);
            }

            // endif
            MarkLabel(endif);
            return;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void GenerateBinaryOperator(AstNodeType nodeType, Type type) {
            switch (nodeType) {
                case AstNodeType.ArrayIndex:
                    EmitLoadElement(type);
                    break;

                case AstNodeType.Equal:
                    Emit(OpCodes.Ceq);
                    break;

                case AstNodeType.NotEqual:
                    Emit(OpCodes.Ceq);
                    EmitInt(0);
                    Emit(OpCodes.Ceq);
                    break;

                case AstNodeType.GreaterThan:
                    Emit(OpCodes.Cgt);
                    break;

                case AstNodeType.LessThan:
                    Emit(OpCodes.Clt);
                    break;

                case AstNodeType.GreaterThanOrEqual:
                    Emit(OpCodes.Clt);
                    EmitInt(0);
                    Emit(OpCodes.Ceq);
                    break;

                case AstNodeType.LessThanOrEqual:
                    Emit(OpCodes.Cgt);
                    EmitInt(0);
                    Emit(OpCodes.Ceq);
                    break;
                case AstNodeType.Multiply:
                    Emit(OpCodes.Mul);
                    break;
                case AstNodeType.Modulo:
                    Emit(OpCodes.Rem);
                    break;
                case AstNodeType.Add:
                    Emit(OpCodes.Add);
                    break;
                case AstNodeType.Subtract:
                    Emit(OpCodes.Sub);
                    break;
                case AstNodeType.Divide:
                    Emit(OpCodes.Div);
                    break;
                case AstNodeType.LeftShift:
                    Emit(OpCodes.Shl);
                    break;
                case AstNodeType.RightShift:
                    Emit(OpCodes.Shr);
                    break;
                case AstNodeType.And:
                    Emit(OpCodes.And);
                    break;
                case AstNodeType.Or:
                    Emit(OpCodes.Or);
                    break;
                case AstNodeType.ExclusiveOr:
                    Emit(OpCodes.Xor);
                    break;
                default:
                    throw new InvalidOperationException(nodeType.ToString());
            }
        }

        #endregion

        #region MethodCallExpression

        private void Emit(MethodCallExpression node) {
            // Emit instance, if calling an instance method
            if (!node.Method.IsStatic) {
                Type type = node.Method.DeclaringType;

                if (type.IsValueType) {
                    EmitAddress(node.Instance, type);
                } else {
                    EmitExpression(node.Instance);
                }
            }

            // Emit arguments
            Debug.Assert(node.Arguments.Count == node.ParameterInfos.Length);
            for (int arg = 0; arg < node.ParameterInfos.Length; arg++) {
                Expression argument = node.Arguments[arg];
                Type type = node.ParameterInfos[arg].ParameterType;
                EmitArgument(argument, type);
            }

            // Emit the actual call
            EmitCall(node.Method);
        }

        private void EmitArgument(Expression argument, Type type) {
            if (type.IsByRef) {
                EmitAddress(argument, type.GetElementType());
            } else {
                EmitExpression(argument);
            }
        }

        #endregion

        private void Emit(ConditionalExpression node) {
            Label eoi = DefineLabel();
            Label next = DefineLabel();
            EmitBranchFalse(node.Test, next);
            //Emit(OpCodes.Brfalse, next);
            EmitExpression(node.IfTrue);
            EmitSequencePointNone();
            Emit(OpCodes.Br, eoi);
            MarkLabel(next);
            EmitExpression(node.IfFalse);
            MarkLabel(eoi);
        }

        private void Emit(ConstantExpression node) {
            EmitConstant(node.Value);
        }

        private void Emit(UnaryExpression node) {
            EmitExpression(node.Operand);

            switch (node.NodeType) {
                case AstNodeType.Convert:
                    EmitCast(node.Operand.Type, node.Type);
                    break;

                case AstNodeType.Not:
                    if (node.Operand.Type == typeof(bool)) {
                        Emit(OpCodes.Ldc_I4_0);
                        Emit(OpCodes.Ceq);
                    } else {
                        Emit(OpCodes.Not);
                    }
                    break;
                case AstNodeType.Negate:
                    Emit(OpCodes.Neg);
                    break;
                case AstNodeType.OnesComplement:
                    Emit(OpCodes.Not);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Emit(NewExpression node) {
            ReadOnlyCollection<Expression> arguments = node.Arguments;
            for (int i = 0; i < arguments.Count; i++) {
                EmitExpression(arguments[i]);
            }
            if (node.Constructor != null) {
                EmitNew(node.Constructor);
            } else {
                Debug.Assert(arguments.Count == 0, "Node with arguments must have a constructor.");
                Debug.Assert(node.Type.IsValueType, "Only value type may have constructor not set.");

                Slot temp = GetLocalTmp(node.Type);
                temp.EmitGetAddr(this);
                Emit(OpCodes.Initobj, node.Type);
                temp.EmitGet(this);
                FreeLocalTmp(temp);
            }
        }

        private void Emit(TypeBinaryExpression node) {
            if (node.TypeOperand.IsAssignableFrom(node.Expression.Type)) {
                // if its always true just emit the bool
                EmitConstant(true);
                return;
            }

            EmitExpressionAsObject(node.Expression);
            Emit(OpCodes.Isinst, node.TypeOperand);
            Emit(OpCodes.Ldnull);
            Emit(OpCodes.Cgt_Un);
        }

        #region ActionExpression

        private void Emit(ActionExpression node) {
            bool fast;
            Slot site = CreateDynamicSite(node.Action, GetSiteTypes(node), out fast);
            MethodInfo method = site.Type.GetMethod("Invoke");

            Debug.Assert(!method.IsStatic);

            // Emit "this" - the site
            site.EmitGet(this);
            ParameterInfo[] parameters = method.GetParameters();

            int first = 0;

            // Emit code context for unoptimized sites only
            if (!fast) {
                Debug.Assert(parameters[0].ParameterType == typeof(CodeContext));

                EmitCodeContext();

                // skip the CodeContext parameter
                first = 1;
            }

            if (parameters.Length < node.Arguments.Count + first) {
                // tuple parameters
                Debug.Assert(parameters.Length == first + 1);

                EmitTuple(
                    site.Type.GetGenericArguments()[0],
                    node.Arguments.Count,
                    delegate(int index) {
                        EmitExpression(node.Arguments[index]);
                    }
                );
            } else {
                // Emit the arguments
                for (int arg = 0; arg < node.Arguments.Count; arg++) {
                    Debug.Assert(parameters[arg + first].ParameterType == node.Arguments[arg].Type);
                    EmitExpression(node.Arguments[arg]);
                }
            }


            // Emit the site invoke
            EmitCall(site.Type, "Invoke");
        }

        private static Type[] GetSiteTypes(ActionExpression node) {
            Type[] ret = new Type[node.Arguments.Count + 1];
            for (int i = 0; i < node.Arguments.Count; i++) {
                ret[i] = node.Arguments[i].Type;
            }
            ret[node.Arguments.Count] = node.Type;
            return ret;
        }

        #endregion

        private void Emit(ArrayIndexAssignment node) {
            EmitExpression(node.Value);

            // Save the expression value - order of evaluation is different than that of the Stelem* instruction
            Slot temp = GetLocalTmp(node.Type);
            temp.EmitSet(this);

            // Emit the array reference
            EmitExpression(node.Array);
            // Emit the index (integer)
            EmitExpression(node.Index);
            // Emit the value
            temp.EmitGet(this);
            // Store it in the array
            EmitStoreElement(node.Type);
            temp.EmitGet(this);
            FreeLocalTmp(temp);
        }

        private void Emit(BoundAssignment node) {
            if (TypeUtils.IsNullableType(node.Type)) {
                // Nullable<T> being assigned...
                if (ConstantCheck.IsConstant(node.Value, null)) {
                    GetVariableSlot(node.Variable).EmitGetAddr(this);
                    Emit(OpCodes.Initobj, node.Type);
                    GetVariableSlot(node.Variable).EmitGet(this);
                    return;
                } else if (node.Type != node.Value.Type) {
                    throw new InvalidOperationException();
                }
                // fall through & emit the store from Nullable<T> -> Nullable<T>
            } 
            EmitExpression(node.Value);
            Emit(OpCodes.Dup);
            GetVariableSlot(node.Variable).EmitSet(this);
        }

        private void Emit(BoundExpression node) {
            // Do not emit CheckInitialized for variables that are defined, or for temp variables.
            // Only emit CheckInitialized for variables of type object
            bool check = !node.IsDefined && !node.Variable.IsTemporary && node.Variable.Type == typeof(object);
            EmitGet(GetVariableSlot(node.Variable), node.Name, check);
        }

        private void Emit(CodeBlockExpression node) {
            EmitDelegateConstruction(node.Block, node.Type);
        }

        // Emit the generator intrinsic arg used in a GeneratorCodeBlock.
        private void EmitGeneratorIntrinsic() {
            // This is coupled to the codegen in GeneratorCodeBlock, 
            // which always uses the 1st arg.
            GetLambdaArgumentSlot(0).EmitGet(this);
        }

        internal void EmitCodeContext() {
            if (ContextSlot == null) {
                throw new InvalidOperationException("ContextSlot not available.");
            }

            ContextSlot.EmitGet(this);
        }

        private void Emit(DeleteUnboundExpression node) {
            // RuntimeHelpers.RemoveName(CodeContext, name)
            EmitCodeContext();
            EmitSymbolId(node.Name);
            EmitCall(typeof(RuntimeHelpers), "RemoveName");
        }

        private void EmitEnvironmentExpression() {
            EmitEnvironmentOrNull();
        }

        private void Emit(MemberAssignment node) {
            // emit "this", if any
            EmitInstance(node.Expression, node.Member.DeclaringType);

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    EmitExpression(node.Value);
                    EmitFieldSet((FieldInfo)node.Member);
                    break;
                case MemberTypes.Property:
                    EmitExpression(node.Value);
                    EmitPropertySet((PropertyInfo)node.Member);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
        }

        private void Emit(MemberExpression node) {
            // emit "this", if any
            EmitInstance(node.Expression, node.Member.DeclaringType);

            switch (node.Member.MemberType) {
                case MemberTypes.Field:
                    EmitFieldGet((FieldInfo)node.Member);
                    break;
                case MemberTypes.Property:
                    EmitPropertyGet((PropertyInfo)node.Member);
                    break;
                default:
                    Debug.Assert(false, "Invalid member type");
                    break;
            }
        }

        private void EmitInstance(Expression instance, Type type) {
            if (instance != null) {
                if (type.IsValueType) {
                    EmitAddress(instance, type);
                } else {
                    EmitExpression(instance);
                }
            }
        }

        private void Emit(NewArrayExpression node) {
            if (node.NodeType == AstNodeType.NewArrayExpression) {
                EmitArray(
                    node.Type.GetElementType(),
                    node.Expressions.Count,
                    delegate(int index) {
                        EmitExpression(node.Expressions[index]);
                    }
                );
            } else {
                ReadOnlyCollection<Expression> bounds = node.Expressions;
                for (int i = 0; i < bounds.Count; i++) {
                    EmitExpression(bounds[i]);
                }
                EmitArray(node.Type);
            }
        }

        private void Emit(UnboundAssignment node) {
            EmitExpressionAsObject(node.Value);
            EmitCodeContext();
            EmitSymbolId(node.Name);
            EmitCall(typeof(RuntimeHelpers), "SetNameReorder");
        }

        private void Emit(UnboundExpression node) {
            // RuntimeHelpers.LookupName(CodeContext, name)
            EmitCodeContext();
            EmitSymbolId(node.Name);
            EmitCall(typeof(RuntimeHelpers), "LookupName");
        }

        #region Expression helpers

        private void EmitExpressionAsObjectOrNull(Expression node) {
            if (node == null) {
                Emit(OpCodes.Ldnull);
            } else {
                EmitExpressionAsObject(node);
            }
        }


        private void EmitExpressionAndPop(Expression node) {
            EmitExpression(node);
            if (node.Type != typeof(void)) {
                Emit(OpCodes.Pop);
            }
        }

        #endregion
    }
}
