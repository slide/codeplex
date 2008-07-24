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
using System.Scripting.Utils;
using System.Text;
using System.Diagnostics;

namespace System.Linq.Expressions {
    /// <summary>
    /// Expression is the base type for all nodes in Expression Trees
    /// </summary>
    public abstract partial class Expression {
        // TODO: expose this to derived classes, so ctor doesn't take three booleans?
        [Flags]
        private enum NodeFlags : byte {
            None = 0,
            Reducible = 1,
            CanRead = 2,
            CanWrite = 4,
        }

        // TODO: these two enums could be stored in one int32
        private readonly ExpressionType _nodeType;
        private readonly NodeFlags _flags;

        private readonly Type _type;
        private readonly CallSiteBinder _binder;
        private readonly Annotations _annotations;

        // protected ctors are part of API surface area

        // LinqV1 ctor
        // obsolete this?
        protected Expression(ExpressionType nodeType, Type type)
            : this(nodeType, type, false, null, true, false, null) {
        }

        // LinqV2: ctor for extension nodes
        protected Expression(Type type, bool reducible, Annotations annotations)
            : this(ExpressionType.Extension, type, reducible, annotations, true, false, null) {
        }

        // LinqV2: ctor for extension nodes with read/write flags
        protected Expression(Type type, bool reducible, Annotations annotations, bool canRead, bool canWrite)
            : this(ExpressionType.Extension, type, reducible, annotations, canRead, canWrite, null) {
        }

        // LinqV2: ctor for dynamic extension nodes
        // TODO: remove dynamic node support from Expression?
        protected Expression(Type type, bool reducible, Annotations annotations, CallSiteBinder binder)
            : this(ExpressionType.Extension, type, reducible, annotations, true, false, binder) {
        }

        // internal ctors -- not exposed API

        internal Expression(ExpressionType nodeType, Type type, Annotations annotations, CallSiteBinder binder)
            : this(nodeType, type, false, annotations, true, false, binder) {
        }

        internal Expression(ExpressionType nodeType, Type type, bool reducible, Annotations annotations, bool canRead, bool canWrite, CallSiteBinder binder) {
            ContractUtils.Requires(canRead || canWrite, "canRead", Strings.MustBeReadableOrWriteable);

            // We should also enforce that subtrees of a bound node are also bound.
            // But it's up to the subclasses of Expression to enforce that
            if (type == null && binder == null) {
                throw Error.TypeOrBindingInfoMustBeNonNull();
            }

            // Enforced by protected ctors, also we must do the right thing internally
            Debug.Assert(binder == null || (canRead == true && canWrite == false), "dynamic nodes are only readable");

            _annotations = annotations ?? Annotations.Empty;
            _nodeType = nodeType;
            _type = type;
            _binder = binder;
            _flags = (reducible ? NodeFlags.Reducible : 0) | (canRead ? NodeFlags.CanRead : 0) | (canWrite ? NodeFlags.CanWrite : 0);
        }

        //CONFORMING
        public ExpressionType NodeType {
            get { return _nodeType; }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return _type; }
        }

        /// <summary>
        /// Information that can be used to bind this tree,
        /// either statically or dynamically
        /// </summary>
        public CallSiteBinder BindingInfo {
            get { return _binder; }
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
            get { return _type != null && _binder != null; }
        }

        public Annotations Annotations {
            get { return _annotations; }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this 
        /// returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public bool IsReducible {
            get { return (_flags & NodeFlags.Reducible) != 0; }
        }

        /// <summary>
        /// Indicates that the node can be read
        /// </summary>
        public bool CanRead {
            get { return (_flags & NodeFlags.CanRead) != 0; }
        }

        /// <summary>
        /// Indicates that the node can be written
        /// </summary>
        public bool CanWrite {
            get { return (_flags & NodeFlags.CanWrite) != 0; }
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If IsReducible returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public virtual Expression Reduce() {
            ContractUtils.Requires(!IsReducible, "this", Strings.ReducibleMustOverrideReduce);
            return this;
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If IsReducible returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// 
        /// Unlike Reduce, this method checks that the reduced node satisfies
        /// certain invaraints.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public Expression ReduceAndCheck() {
            ContractUtils.Requires(IsReducible, "this", Strings.MustBeReducible);

            var newNode = Reduce();

            // 1. Reduction must return a new, non-null node
            // 2. Reduction must return a new node whose result type can be assigned to the type of the original node
            // 3. Reduction must return a node that can be read/written to if the original node could
            ContractUtils.Requires(newNode != null && newNode != this, "this", Strings.MustReduceToDifferent);
            ContractUtils.Requires(TypeUtils.AreReferenceAssignable(Type, newNode.Type), "this", Strings.ReducedNotCompatible);
            ContractUtils.Requires(!CanRead || newNode.CanRead, "this", Strings.MustReduceToReadable);
            ContractUtils.Requires(!CanWrite || newNode.CanWrite, "this", Strings.MustReduceToWriteable);
            return newNode;
        }

        /// <summary>
        /// Reduces the expression to a known node type (i.e. not an Extension node)
        /// or simply returns the expression if it is already a known type.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public Expression ReduceToKnown() {
            var node = this;
            while (node.NodeType == ExpressionType.Extension) {
                node = node.ReduceAndCheck();
            }
            return node;
        }

        //CONFORMING
        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            this.BuildString(builder);
            return builder.ToString();
        }

        //CONFORMING
        internal virtual void BuildString(StringBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            builder.Append("[");
            builder.Append(_nodeType.ToString());
            builder.Append("]");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter()) {
                    ExpressionWriter.Dump(this, GetType().Name, writer);
                    return writer.ToString();
                }
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class Expression {
        /// <summary>
        /// Verifies that the expression is fully bound (i.e. it has a non-null type)
        /// </summary>
        internal static void RequiresBound(Expression expression, string paramName) {
            if (expression != null && !expression.IsBound) {
                throw new ArgumentException(Strings.SubtreesMustBeBound, paramName);
            }
        }

        /// <summary>
        /// Verifies that all expressions in the list are fully bound
        /// </summary>
        internal static void RequiresBoundItems(IList<Expression> items, string paramName) {
            if (items != null) {
                for (int i = 0, n = items.Count; i < n; i++) {
                    RequiresBound(items[i], paramName);
                }
            }
        }

        internal static void RequiresCanRead(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }
            if (!expression.CanRead) {
                throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
            }
        }
        internal static void RequiresCanRead(IEnumerable<Expression> items, string paramName) {
            if (items != null) {
                foreach (var i in items) {
                    RequiresCanRead(i, paramName);
                }
            }
        }
        internal static void RequiresCanWrite(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }
            if (!expression.CanWrite) {
                throw new ArgumentException(Strings.ExpressionMustBeWriteable, paramName);
            }
        }

        /// <summary>
        /// Reduces the expression to a known node type (i.e. not an Extension node)
        /// or simply returns the expression if it is already a known type.
        /// </summary>
        [Obsolete("use the instance method ReduceToKnown instead")]
        public static Expression ReduceToKnown(Expression node) {
            ContractUtils.RequiresNotNull(node, "node");
            return node.ReduceToKnown();
        }
    }
}
