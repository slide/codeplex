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

using IronPython.Runtime;
using MSAst = Microsoft.Scripting.Internal.Ast;
using Operators = Microsoft.Scripting.Operators;

namespace IronPython.Compiler.Ast {
    public class DictionaryExpression : Expression {
        private readonly SliceExpression[] _items;

        public DictionaryExpression(params SliceExpression[] items) {
            _items = items;
        }

        public SliceExpression[] Items {
            get { return _items; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // 1. Create temp for the result
            MSAst.BoundExpression dictionary = null;
            dictionary = ag.MakeTempExpression("dictionary", typeof(PythonDictionary), Span);

            // 2. Array for the comma expression parts:
            //    - dictionary creation
            //    - _items.Length for the items
            //    - value (the dictionary) == SILVERLIGHT WORKAROUND (dup bug)
            MSAst.Expression[] parts = new MSAst.Expression[_items.Length + 2];

            // 3. Create the dictionary by calling MakeDict(_items.Length)
            parts[0] = new MSAst.BoundAssignment(
                dictionary.Reference,
                new MSAst.MethodCallExpression(
                    AstGenerator.GetHelperMethod("MakeDict"),
                    null,
                    new MSAst.Expression[] {
                            new MSAst.ConstantExpression(_items.Length),
                        },
                    Span
                    ),
                Operators.None,
                Span
                );


            // 4. Get the setter to call on each value inserted into the dictionary
            System.Reflection.MethodInfo setter = typeof(PythonDictionary).GetProperty("Item").GetSetMethod();

            // 5. Transform the slices into method calls and assignments
            int index;
            for (index = 0; index < _items.Length; index++) {
                SliceExpression slice = _items[index];
                parts[index + 1] = new MSAst.MethodCallExpression(
                    setter,
                    dictionary,
                    new MSAst.Expression[] {
                        ag.TransformOrConstantNull(slice.SliceStart),
                        ag.TransformOrConstantNull(slice.SliceStop)
                    },
                    Span);
            }

            // SILVERLIGHT WORKAROUND:
            // The value of the Comma expression could be the first expression, but due
            // to the dup verification bug in Silverlight, we put it again at the end, and it
            // will get reloaded from the local rather than dup-ed
            parts[parts.Length - 1] = dictionary;

            ag.FreeTemp(dictionary);

            // 6. Return the comma expression, return value is the dictionary creation (at index 0)
            return new MSAst.CommaExpression(parts, parts.Length - 1, Span);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_items != null) {
                    foreach (SliceExpression s in _items) {
                        s.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }
    }
}
