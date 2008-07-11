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

using System.Collections.Generic;
using System.Scripting.Actions;
using System.Scripting.Generation;
using System.Diagnostics;
using System.Scripting.Runtime;

namespace System.Linq.Expressions {

    /// <summary>
    /// Serializes constants and dynamic sites so the code can be saved to disk
    /// TODO: move to Microsoft.Scripting !!!
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal sealed class ToDiskRewriter : GlobalArrayRewriter {
        private List<Expression> _constants;
        private VariableExpression _constantPool;
        private int _depth;

        protected override Expression Visit(LambdaExpression node) {
            _depth++;
            try {

                // Visit the lambda first, so we walk the tree and find any
                // constants we need to rewrite.
                node = (LambdaExpression)base.Visit(node);

                // Only rewrite if we have constants and this is the top lambda
                if (_constants == null || _depth != 1) {
                    return node;
                }

                // Rewrite the constants, they can contain embedded
                // CodeContextExpressions
                for (int i = 0; i < _constants.Count; i++) {
                    _constants[i] = VisitNode(_constants[i]);
                }

                // Add the consant pool variable to the top lambda
                Expression body = AddScopedVariable(
                    node.Body,
                    _constantPool,
                    Expression.NewArrayInit(typeof(object), _constants)
                );

                // Rewrite the lambda
                return Expression.Lambda(
                    node.Annotations,
                    node.NodeType,
                    node.Type,
                    node.Name,
                    body,
                    node.Parameters
                );

            } finally {
                _depth--;
            }
        }

        protected override Expression VisitExtension(Expression node) {
            Expression res = base.VisitExtension(node);

            if (node.IsDynamic) {
                // the node was dynamic, the dynamic nodes were removed,
                // we now need to rewrite any call sites.
                return VisitNode(res);
            }

            return res;
        }

        protected override Expression Visit(ConstantExpression node) {
            CallSite site = node.Value as CallSite;
            if (site != null) {
                return RewriteCallSite(site);
            }

            return base.Visit(node);
        }

        private Expression RewriteCallSite(CallSite site) {
            IExpressionSerializable serializer = site.Binder as IExpressionSerializable;
            if (serializer == null) {
                throw new InvalidOperationException("generating code from non-serializable CallSiteBinder");
            }

            // add the initialization code that we'll generate later into the outermost
            // lambda and then return an index into the array we'll be creating.
            if (_constantPool == null) {
                _constantPool = Expression.Variable(typeof(object[]), "$constantPool");
                _constants = new List<Expression>();
            }

            _constants.Add(Expression.Call(site.GetType().GetMethod("Create"), serializer.CreateExpression()));

            // rewrite the node...
            return VisitNode(
                Expression.ConvertHelper(
                    Expression.ArrayIndex(_constantPool, Expression.Constant(_constants.Count - 1)),
                    site.GetType()
                )
            );
        }
    }
}
