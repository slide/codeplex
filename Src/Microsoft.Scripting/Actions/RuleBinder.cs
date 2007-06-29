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
using System.Diagnostics;

using Microsoft.Scripting.Ast;

namespace Microsoft.Scripting.Actions {
    class RuleBinder : Walker {
        private readonly Expression _test;
        private readonly Statement _target;
        private readonly List<Variable> _temps;

        private Dictionary<Variable, VariableReference> _refs;

        public static VariableReference[] Bind(Expression test, Statement target, List<Variable> temps) {
            RuleBinder rb = new RuleBinder(test, target, temps);
            test.Walk(rb);
            target.Walk(rb);
            return rb.GetReferences();
        }

        private RuleBinder(Expression test, Statement target, List<Variable> temps) {
            _test = test;
            _target = target;
            _temps = temps;
        }

        public override bool Walk(BoundAssignment node) {
            node.Ref = GetOrMakeRef(node.Variable);
            return true;
        }

        public override bool Walk(BoundExpression node) {
            node.Ref = GetOrMakeRef(node.Variable);
            return true;
        }

        public override bool Walk(DeleteStatement node) {
            node.Ref = GetOrMakeRef(node.Variable);
            return true;
        }

        public override bool Walk(DynamicTryStatementHandler node) {
            node.Ref = GetOrMakeRef(node.Variable);
            return true;
        }

        private VariableReference GetOrMakeRef(Variable variable) {
            Debug.Assert(variable != null);
            if (_refs == null) {
                _refs = new Dictionary<Variable, VariableReference>();
            }
            VariableReference reference;
            if (!_refs.TryGetValue(variable, out reference)) {
                _refs[variable] = reference = new VariableReference(variable);
            }
            return reference;
        }

        private VariableReference[] GetReferences() {
            if (_refs != null) {
                VariableReference[] references = new VariableReference[_refs.Values.Count];
                _refs.Values.CopyTo(references, 0);
                return references;
            } else {
                return new VariableReference[0];
            }
        }
    }
}
