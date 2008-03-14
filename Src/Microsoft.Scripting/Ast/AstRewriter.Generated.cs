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
    partial class AstRewriter {

        private delegate Expression Rewriter(AstRewriter ar, Expression expr, Stack stack);

        private static readonly Rewriter[] _Rewriters = {

            #region Generated Ast Rewriter

            // *** BEGIN GENERATED CODE ***
            // generated by function: gen_ast_rewriter from: generate_tree.py

            RewriteBinaryExpression,                                    //    Add
                                                                        // ** AddChecked
            RewriteBinaryExpression,                                    //    And
            RewriteLogicalBinaryExpression,                             //    AndAlso
                                                                        // ** ArrayLength
            RewriteBinaryExpression,                                    //    ArrayIndex
            RewriteMethodCallExpression,                                //    Call
                                                                        // ** Coalesce
            RewriteConditionalExpression,                               //    Conditional
            RewriteConstantExpression,                                  //    Constant
            RewriteUnaryExpression,                                     //    Convert
                                                                        // ** ConvertChecked
            RewriteBinaryExpression,                                    //    Divide
            RewriteBinaryExpression,                                    //    Equal
            RewriteBinaryExpression,                                    //    ExclusiveOr
            RewriteBinaryExpression,                                    //    GreaterThan
            RewriteBinaryExpression,                                    //    GreaterThanOrEqual
                                                                        // ** Invoke
            RewriteLambdaExpression,                                    //    Lambda
            RewriteBinaryExpression,                                    //    LeftShift
            RewriteBinaryExpression,                                    //    LessThan
            RewriteBinaryExpression,                                    //    LessThanOrEqual
                                                                        // ** ListInit
                                                                        // ** MemberAccess
                                                                        // ** MemberInit
            RewriteBinaryExpression,                                    //    Modulo
            RewriteBinaryExpression,                                    //    Multiply
                                                                        // ** MultiplyChecked
            RewriteUnaryExpression,                                     //    Negate
                                                                        // ** UnaryPlus
                                                                        // ** NegateChecked
            RewriteNewExpression,                                       //    New
                                                                        // ** NewArrayInit
            RewriteNewArrayExpression,                                  //    NewArrayBounds
            RewriteUnaryExpression,                                     //    Not
            RewriteBinaryExpression,                                    //    NotEqual
            RewriteBinaryExpression,                                    //    Or
            RewriteLogicalBinaryExpression,                             //    OrElse
                                                                        // ** Parameter
                                                                        // ** Power
                                                                        // ** Quote
            RewriteBinaryExpression,                                    //    RightShift
            RewriteBinaryExpression,                                    //    Subtract
                                                                        // ** SubtractChecked
                                                                        // ** TypeAs
            RewriteTypeBinaryExpression,                                //    TypeIs
            RewriteActionExpression,                                    //    ActionExpression
            RewriteArrayIndexAssignment,                                //    ArrayIndexAssignment
            RewriteBlock,                                               //    Block
            RewriteBoundAssignment,                                     //    BoundAssignment
            RewriteBreakStatement,                                      //    BreakStatement
            RewriteIntrinsicExpression,                                 //    CodeContextExpression
            RewriteIntrinsicExpression,                                 //    GeneratorIntrinsic
            RewriteGeneratorLambdaExpression,                           //    Generator
            RewriteContinueStatement,                                   //    ContinueStatement
            RewriteDeleteStatement,                                     //    DeleteStatement
            RewriteDeleteUnboundExpression,                             //    DeleteUnboundExpression
            RewriteDoStatement,                                         //    DoStatement
            RewriteEmptyStatement,                                      //    EmptyStatement
            RewriteIntrinsicExpression,                                 //    EnvironmentExpression
            RewriteExpressionStatement,                                 //    ExpressionStatement
            RewriteVariableExpression,                                  //    GlobalVariable
            RewriteLabeledStatement,                                    //    LabeledStatement
            RewriteVariableExpression,                                  //    LocalVariable
            RewriteLoopStatement,                                       //    LoopStatement
            RewriteMemberAssignment,                                    //    MemberAssignment
            RewriteMemberExpression,                                    //    MemberExpression
            RewriteNewArrayExpression,                                  //    NewArrayExpression
            RewriteUnaryExpression,                                     //    OnesComplement
            RewriteVariableExpression,                                  //    Parameter
            RewriteReturnStatement,                                     //    ReturnStatement
            RewriteScopeStatement,                                      //    ScopeStatement
            RewriteSwitchStatement,                                     //    SwitchStatement
            RewriteVariableExpression,                                  //    TemporaryVariable
            RewriteThrowStatement,                                      //    ThrowStatement
            RewriteTryStatement,                                        //    TryStatement
            RewriteUnboundAssignment,                                   //    UnboundAssignment
            RewriteUnboundExpression,                                   //    UnboundExpression
            RewriteYieldStatement,                                      //    YieldStatement

            // *** END GENERATED CODE ***

            #endregion
        };
    }
}

