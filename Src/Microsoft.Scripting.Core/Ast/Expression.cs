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
using System.Collections.Generic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Summary description for Expr.
    /// </summary>
    public abstract partial class Expression {
        private readonly AstNodeType _nodeType;
        private readonly Type _type;
        private readonly DynamicAction _bindingInfo;
        private readonly Annotations/*!*/ _annotations;

        protected Expression(AstNodeType nodeType, Type type)
            : this(Annotations.Empty, nodeType, type) {
        }

        protected Expression(Annotations annotations, AstNodeType nodeType, Type type)
            : this(annotations, nodeType, type, null) {
        }

        protected Expression(Annotations annotations, AstNodeType nodeType, Type type, DynamicAction bindingInfo) {
            ContractUtils.RequiresNotNull(annotations, "annotations");

            // We should also enforce that subtrees of a bound node are also bound.
            // But it's up to the subclasses of Expression to enforce that
            ContractUtils.Requires(type != null || bindingInfo != null, "type or bindingInfo must be non-null");

            _annotations = annotations;
            _nodeType = nodeType;
            _type = type;
            _bindingInfo = bindingInfo;
        }

        public AstNodeType NodeType {
            get { return _nodeType; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        /// <summary>
        /// Information that can be used to bind this tree,
        /// either statically or dynamically
        /// </summary>
        public DynamicAction BindingInfo {
            get { return _bindingInfo; }
        }

        /// <summary>
        /// Returns true if the tree is fully bound, i.e. Type is not null
        /// </summary>
        public bool IsBound {
            get { return _type != null; }
        }

        /// <summary>
        /// Returns true if this tree is dynamically bound, i.e.
        /// Type is not null and BindingInfo is not null
        /// </summary>
        public bool IsDynamic {
            get { return _type != null && _bindingInfo != null; }
        }

        public Annotations/*!*/ Annotations {
            get { return _annotations; }
        }

        internal SourceLocation Start {
            get { return _annotations.Get<SourceSpan>().Start; }
        }

        internal SourceLocation End {
            get { return _annotations.Get<SourceSpan>().End; }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this 
        /// returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public virtual bool IsReducible {
            get { return false; }
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If IsReducible returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public virtual Expression Reduce() {
            ContractUtils.Requires(!IsReducible, "this", "reducible nodes must override Expression.Reduce()");
            return this;
        }

#if DEBUG
        public string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter()) {
                    AstWriter.Dump(this, GetType().Name, writer);
                    return writer.ToString();
                }
            }
        }
#endif
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")] // TODO: fix
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class Expression {
        /// <summary>
        /// Verifies that the expression is fully bound (i.e. it has a non-null type)
        /// </summary>
        internal static void RequiresBound(Expression expression, string paramName) {
            if (expression != null && !expression.IsBound) {
                throw new ArgumentException("subtrees of nodes with non-null type must also have non-null type", paramName);
            }
        }

        /// <summary>
        /// Verifies that all expressions in the list are fully bound
        /// </summary>
        internal static void RequiresBoundItems(IList<Expression> items, string paramName) {
            if (items != null) {
                for (int i = 0, count = items.Count; i < count; i++) {
                    RequiresBound(items[i], paramName);
                }
            }
        }

        /// <summary>
        /// Reduces the expression to a known node type (i.e. not an Extension node)
        /// or simply returns the expression if it is already a known type
        /// </summary>
        internal static Expression ReduceToKnown(Expression node) {
            while (node.NodeType == AstNodeType.Extension) {
                ContractUtils.Requires(node.IsReducible, "node", "node must be reducible");

                Expression newNode = node.Reduce();

                // Sanity checks:
                //   1. Reduction must return a new, non-null node
                //   2. Reduction must return a new node whose result type can be assigned to the type of the original node
                ContractUtils.Requires(newNode != null && newNode != node, "node", "node cannot reduce to itself or null");
                ContractUtils.Requires(TypeUtils.CanAssign(node.Type, newNode), "node", "cannot assign from the reduced node type to the original node type");

                node = newNode;
            }
            return node;
        }
    }
}
