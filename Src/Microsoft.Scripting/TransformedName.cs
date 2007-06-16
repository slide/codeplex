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
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Microsoft.Scripting {
    /// <summary>
    /// Returns the transformed name of the member or null if the member should
    /// not be displayed.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="reason">the reason a transfored name is being asked for.</param>
    /// <returns></returns>
    public delegate IEnumerable<TransformedName> ExtensionNameTransformer(MemberInfo member, TransformReason reason);

    /// <summary>
    /// The reason a transformed name is being requested.
    /// </summary>
    public enum TransformReason {
        None,
        /// <summary>
        /// Asking for a method, both the name and context will be used.
        /// </summary>
        Method,
        /// <summary>
        /// Asking for an operator, only the context will be used.
        /// </summary>
        Operator,
        /// <summary>
        /// Asking for a property
        /// </summary>
        Property,
        
        /// <summary>
        /// Asking for a field
        /// </summary>
        Field,
        Event,
        /// <summary>
        /// Asking for a name for a nested type
        /// </summary>
        NestedType,
    }

    /// <summary>
    /// Represents a name that has been bound to a specific context and had the 
    /// .NET name transformed into a language specific name.  If a language doesn't want to display
    /// this method it should return TransforedName.Empty.
    /// </summary>
    public class TransformedName  {
        private readonly string _name;
        private readonly ContextId _context;
        private readonly OperatorMapping _op;
        private readonly CustomTransformer _custTransform;

        public TransformedName(string name, OperatorMapping op, ContextId context) {
            if (name == null) throw new ArgumentNullException("name");
            if (op == null) throw new ArgumentNullException("op");

            _name = name;
            _context = context;
            _op = op;
        }

        public TransformedName(string name, ContextId context) {
            if (name == null) throw new ArgumentNullException("name");

            _name = name;
            _context = context;
        }

        public TransformedName(string name, CustomTransformer customTransformer, ContextId context) {
            if (name == null) throw new ArgumentNullException("name");
            if (customTransformer == null) throw new ArgumentNullException("customTransformer");

            _name = name;
            _context = context;
            _custTransform = customTransformer;
        }

        public TransformedName(OperatorMapping op, ContextId context) {
            if (op == null) throw new ArgumentNullException("op");

            _op = op;
            _context = context;
        }

        public string Name {
            get { return _name; }
        }

        public OperatorMapping Operator {
            get {
                return _op;
            }
        }

        public ContextId Context {
            get { return _context; }
        }

        public CustomTransformer CustomTransformer {
            get {
                return _custTransform;
            }
        }
    }

    public delegate DynamicTypeSlot CustomTransformer(MemberInfo info, DynamicTypeSlot existing);
}
