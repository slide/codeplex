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
using Microsoft.Scripting;
using MSAst = Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast {
    using Ast = Microsoft.Scripting.Ast.Expression;

    public class ImportStatement : Statement {
        private readonly ModuleName[] _names;
        private readonly SymbolId[] _asNames;
        private readonly bool _forceAbsolute;

        private PythonVariable[] _variables;

        public ImportStatement(ModuleName[] names, SymbolId[] asNames, bool forceAbsolute) {
            _names = names;
            _asNames = asNames;
            _forceAbsolute = forceAbsolute;
        }

        internal PythonVariable[] Variables {
            get { return _variables; }
            set { _variables = value; }
        }

        public IList<DottedName> Names {
            get { return _names; }
        }

        public IList<SymbolId> AsNames {
            get { return _asNames; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            List<MSAst.Expression> statements = new List<MSAst.Expression>();

            for (int i = 0; i < _names.Length; i++) {
                statements.Add(
                    // _references[i] = PythonOps.Import(<code context>, _names[i])
                    Ast.Assign(
                        _names[i].Span,
                        _variables[i].Variable,
                        Ast.Call(
                            AstGenerator.GetHelperMethod(                           // helper
                                _asNames[i] == SymbolId.Empty ? "ImportTop" : "ImportBottom"
                            ),
                            Ast.CodeContext(),                                      // 1st arg - code context
                            Ast.Constant(_names[i].MakeString()),                   // 2nd arg - module name
                            Ast.Constant(_forceAbsolute ? 0 : -1)                   // 3rd arg - absolute or relative imports
                        )
                    )
                );
            }

            return Ast.Block(Span, statements.ToArray());
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }
    }
}
