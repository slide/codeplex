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

#if DEBUG
namespace Microsoft.Scripting.Ast {
    partial class AstWriter {

        private delegate void Writer(AstWriter ar, Expression expr);

        private static Writer[] _Writers = {
            #region Generated Ast Writer

            // *** BEGIN GENERATED CODE ***

            WriteBinaryExpression,                  //    Add
                                                    // ** AddChecked
            WriteBinaryExpression,                  //    And
            WriteBinaryExpression,                  //    AndAlso
                                                    // ** ArrayLength
            WriteBinaryExpression,                  //    ArrayIndex
            WriteMethodCallExpression,              //    Call
                                                    // ** Coalesce
            WriteConditionalExpression,             //    Conditional
            WriteConstantExpression,                //    Constant
            WriteUnaryExpression,                   //    Convert
                                                    // ** ConvertChecked
            WriteBinaryExpression,                  //    Divide
            WriteBinaryExpression,                  //    Equal
            WriteBinaryExpression,                  //    ExclusiveOr
            WriteBinaryExpression,                  //    GreaterThan
            WriteBinaryExpression,                  //    GreaterThanOrEqual
                                                    // ** Invoke
                                                    // ** Lambda
            WriteBinaryExpression,                  //    LeftShift
            WriteBinaryExpression,                  //    LessThan
            WriteBinaryExpression,                  //    LessThanOrEqual
                                                    // ** ListInit
                                                    // ** MemberAccess
                                                    // ** MemberInit
            WriteBinaryExpression,                  //    Modulo
            WriteBinaryExpression,                  //    Multiply
                                                    // ** MultiplyChecked
            WriteUnaryExpression,                   //    Negate
                                                    // ** UnaryPlus
                                                    // ** NegateChecked
            WriteNewExpression,                     //    New
                                                    // ** NewArrayInit
                                                    // ** NewArrayBounds
            WriteUnaryExpression,                   //    Not
            WriteBinaryExpression,                  //    NotEqual
            WriteBinaryExpression,                  //    Or
            WriteBinaryExpression,                  //    OrElse
                                                    // ** Parameter
                                                    // ** Power
                                                    // ** Quote
            WriteBinaryExpression,                  //    RightShift
            WriteBinaryExpression,                  //    Subtract
                                                    // ** SubtractChecked
                                                    // ** TypeAs
            WriteTypeBinaryExpression,              //    TypeIs
            WriteActionExpression,                  //    ActionExpression
            WriteArrayIndexAssignment,              //    ArrayIndexAssignment
            WriteBlock,                             //    Block
            WriteBoundAssignment,                   //    BoundAssignment
            WriteBoundExpression,                   //    BoundExpression
            WriteBreakStatement,                    //    BreakStatement
            WriteCodeBlockExpression,               //    CodeBlockExpression
            WriteIntrinsicExpression,               //    CodeContextExpression
            WriteIntrinsicExpression,               //    GeneratorIntrinsic
            WriteContinueStatement,                 //    ContinueStatement
            WriteDeleteStatement,                   //    DeleteStatement
            WriteDeleteUnboundExpression,           //    DeleteUnboundExpression
            WriteDoStatement,                       //    DoStatement
            WriteEmptyStatement,                    //    EmptyStatement
            WriteIntrinsicExpression,               //    EnvironmentExpression
            WriteExpressionStatement,               //    ExpressionStatement
            WriteLabeledStatement,                  //    LabeledStatement
            WriteLoopStatement,                     //    LoopStatement
            WriteMemberAssignment,                  //    MemberAssignment
            WriteMemberExpression,                  //    MemberExpression
            WriteNewArrayExpression,                //    NewArrayExpression
            WriteUnaryExpression,                   //    OnesComplement
            WriteIntrinsicExpression,               //    ParamsExpression
            WriteReturnStatement,                   //    ReturnStatement
            WriteScopeStatement,                    //    ScopeStatement
            WriteSwitchStatement,                   //    SwitchStatement
            WriteThrowStatement,                    //    ThrowStatement
            WriteTryStatement,                      //    TryStatement
            WriteUnboundAssignment,                 //    UnboundAssignment
            WriteUnboundExpression,                 //    UnboundExpression
            WriteYieldStatement,                    //    YieldStatement

            // *** END GENERATED CODE ***

            #endregion
        };

    }
}
#endif
