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
using System; using Microsoft;
using System.Diagnostics;
using Microsoft.Scripting.Utils;

namespace Microsoft.Linq.Expressions.Compiler {
    partial class LambdaCompiler {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitExpression(Expression node, bool emitStart) {
            Debug.Assert(node != null);

            ExpressionStart startEmitted = emitStart ? EmitExpressionStart(node) : ExpressionStart.None;

            switch (node.NodeType) {
                #region Generated Expression Compiler

                // *** BEGIN GENERATED CODE ***
                // generated by function: gen_compiler from: generate_tree.py

                // Add
                case ExpressionType.Add:
                    EmitBinaryExpression(node);
                    break;
                // AddChecked
                case ExpressionType.AddChecked:
                    EmitBinaryExpression(node);
                    break;
                // And
                case ExpressionType.And:
                    EmitBinaryExpression(node);
                    break;
                // AndAlso
                case ExpressionType.AndAlso:
                    EmitAndAlsoBinaryExpression(node);
                    break;
                // ArrayLength
                case ExpressionType.ArrayLength:
                    EmitUnaryExpression(node);
                    break;
                // ArrayIndex
                case ExpressionType.ArrayIndex:
                    EmitBinaryExpression(node);
                    break;
                // Call
                case ExpressionType.Call:
                    EmitMethodCallExpression(node);
                    break;
                // Coalesce
                case ExpressionType.Coalesce:
                    EmitCoalesceBinaryExpression(node);
                    break;
                // Conditional
                case ExpressionType.Conditional:
                    EmitConditionalExpression(node);
                    break;
                // Constant
                case ExpressionType.Constant:
                    EmitConstantExpression(node);
                    break;
                // Convert
                case ExpressionType.Convert:
                    EmitConvertUnaryExpression(node);
                    break;
                // ConvertChecked
                case ExpressionType.ConvertChecked:
                    EmitConvertUnaryExpression(node);
                    break;
                // Divide
                case ExpressionType.Divide:
                    EmitBinaryExpression(node);
                    break;
                // Equal
                case ExpressionType.Equal:
                    EmitBinaryExpression(node);
                    break;
                // ExclusiveOr
                case ExpressionType.ExclusiveOr:
                    EmitBinaryExpression(node);
                    break;
                // GreaterThan
                case ExpressionType.GreaterThan:
                    EmitBinaryExpression(node);
                    break;
                // GreaterThanOrEqual
                case ExpressionType.GreaterThanOrEqual:
                    EmitBinaryExpression(node);
                    break;
                // Invoke
                case ExpressionType.Invoke:
                    EmitInvocationExpression(node);
                    break;
                // Lambda
                case ExpressionType.Lambda:
                    EmitLambdaExpression(node);
                    break;
                // LeftShift
                case ExpressionType.LeftShift:
                    EmitBinaryExpression(node);
                    break;
                // LessThan
                case ExpressionType.LessThan:
                    EmitBinaryExpression(node);
                    break;
                // LessThanOrEqual
                case ExpressionType.LessThanOrEqual:
                    EmitBinaryExpression(node);
                    break;
                // ListInit
                case ExpressionType.ListInit:
                    EmitListInitExpression(node);
                    break;
                // MemberAccess
                case ExpressionType.MemberAccess:
                    EmitMemberExpression(node);
                    break;
                // MemberInit
                case ExpressionType.MemberInit:
                    EmitMemberInitExpression(node);
                    break;
                // Modulo
                case ExpressionType.Modulo:
                    EmitBinaryExpression(node);
                    break;
                // Multiply
                case ExpressionType.Multiply:
                    EmitBinaryExpression(node);
                    break;
                // MultiplyChecked
                case ExpressionType.MultiplyChecked:
                    EmitBinaryExpression(node);
                    break;
                // Negate
                case ExpressionType.Negate:
                    EmitUnaryExpression(node);
                    break;
                // UnaryPlus
                case ExpressionType.UnaryPlus:
                    EmitUnaryExpression(node);
                    break;
                // NegateChecked
                case ExpressionType.NegateChecked:
                    EmitUnaryExpression(node);
                    break;
                // New
                case ExpressionType.New:
                    EmitNewExpression(node);
                    break;
                // NewArrayInit
                case ExpressionType.NewArrayInit:
                    EmitNewArrayExpression(node);
                    break;
                // NewArrayBounds
                case ExpressionType.NewArrayBounds:
                    EmitNewArrayExpression(node);
                    break;
                // Not
                case ExpressionType.Not:
                    EmitUnaryExpression(node);
                    break;
                // NotEqual
                case ExpressionType.NotEqual:
                    EmitBinaryExpression(node);
                    break;
                // Or
                case ExpressionType.Or:
                    EmitBinaryExpression(node);
                    break;
                // OrElse
                case ExpressionType.OrElse:
                    EmitOrElseBinaryExpression(node);
                    break;
                // Parameter
                case ExpressionType.Parameter:
                    EmitParameterExpression(node);
                    break;
                // Power
                case ExpressionType.Power:
                    EmitBinaryExpression(node);
                    break;
                // Quote
                case ExpressionType.Quote:
                    EmitQuoteUnaryExpression(node);
                    break;
                // RightShift
                case ExpressionType.RightShift:
                    EmitBinaryExpression(node);
                    break;
                // Subtract
                case ExpressionType.Subtract:
                    EmitBinaryExpression(node);
                    break;
                // SubtractChecked
                case ExpressionType.SubtractChecked:
                    EmitBinaryExpression(node);
                    break;
                // TypeAs
                case ExpressionType.TypeAs:
                    EmitUnaryExpression(node);
                    break;
                // TypeIs
                case ExpressionType.TypeIs:
                    EmitTypeBinaryExpression(node);
                    break;
                // Assign
                case ExpressionType.Assign:
                    EmitAssignBinaryExpression(node);
                    break;
                // Block
                case ExpressionType.Block:
                    EmitBlockExpression(node);
                    break;
                // DebugInfo
                case ExpressionType.DebugInfo:
                    EmitDebugInfoExpression(node);
                    break;
                // Dynamic
                case ExpressionType.Dynamic:
                    EmitDynamicExpression(node);
                    break;
                // Default
                case ExpressionType.Default:
                    EmitEmptyExpression(node);
                    break;
                // Extension
                case ExpressionType.Extension:
                    EmitExtensionExpression(node);
                    break;
                // Goto
                case ExpressionType.Goto:
                    EmitGotoExpression(node);
                    break;
                // Index
                case ExpressionType.Index:
                    EmitIndexExpression(node);
                    break;
                // Label
                case ExpressionType.Label:
                    EmitLabelExpression(node);
                    break;
                // RuntimeVariables
                case ExpressionType.RuntimeVariables:
                    EmitRuntimeVariablesExpression(node);
                    break;
                // Loop
                case ExpressionType.Loop:
                    EmitLoopExpression(node);
                    break;
                // ReturnStatement
                case ExpressionType.ReturnStatement:
                    EmitReturnStatement(node);
                    break;
                // Switch
                case ExpressionType.Switch:
                    EmitSwitchExpression(node);
                    break;
                // Throw
                case ExpressionType.Throw:
                    EmitThrowUnaryExpression(node);
                    break;
                // Try
                case ExpressionType.Try:
                    EmitTryExpression(node);
                    break;
                // Unbox
                case ExpressionType.Unbox:
                    EmitUnboxUnaryExpression(node);
                    break;
                // AddAssign
                case ExpressionType.AddAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // AndAssign
                case ExpressionType.AndAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // DivideAssign
                case ExpressionType.DivideAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // ExclusiveOrAssign
                case ExpressionType.ExclusiveOrAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // LeftShiftAssign
                case ExpressionType.LeftShiftAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // ModuloAssign
                case ExpressionType.ModuloAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // MultiplyAssign
                case ExpressionType.MultiplyAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // OrAssign
                case ExpressionType.OrAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // PowerAssign
                case ExpressionType.PowerAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // RightShiftAssign
                case ExpressionType.RightShiftAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // SubtractAssign
                case ExpressionType.SubtractAssign:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // AddAssignChecked
                case ExpressionType.AddAssignChecked:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // MultiplyAssignChecked
                case ExpressionType.MultiplyAssignChecked:
                    EmitOpAssignBinaryExpression(node);
                    break;
                // SubtractAssignChecked
                case ExpressionType.SubtractAssignChecked:
                    EmitOpAssignBinaryExpression(node);
                    break;

                // *** END GENERATED CODE ***

                #endregion

                default:
                    throw Assert.Unreachable;
            }

            if (emitStart) {
                EmitExpressionEnd(startEmitted);
            }
        }
    }
}
