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
using Microsoft.Scripting.Internal.Generation;

namespace Microsoft.Scripting.Internal.Ast {
    public class ArrayIndexAssignment : Expression{
        private readonly Expression _array;
        private readonly Expression _index;
        private readonly Expression _value;

        public ArrayIndexAssignment(Expression array, Expression index, Expression value, SourceSpan span)
            : base(span) {
            if (array == null) throw new ArgumentNullException("array");
            if (index == null) throw new ArgumentNullException("index");
            if (value == null) throw new ArgumentNullException("value");
            if (!array.ExpressionType.IsArray) {
                throw new NotSupportedException("Expression type of the array must be array (Type.IsArray)!");
            }

            _array = array;
            _index = index;
            _value = value;
        }

        public Expression Array {
            get { return _array; }
        }

        public Expression Index {
            get { return _index; }
        }

        public Expression Value {
            get { return _value; }
        }


        public override void Emit(CodeGen cg) {
            EmitAs(cg, typeof(object));
        }

        public override void EmitAs(CodeGen cg, Type asType) {
            Type arrayType = _array.ExpressionType;
            Type elementType = arrayType.IsArray ? arrayType.GetElementType() : typeof(object);
            _value.EmitAs(cg, elementType);

            // Save the expression value - order of evaluation is different than that of the Stelem* instruction
            Slot temp = cg.GetLocalTmp(elementType);
            temp.EmitSet(cg);

            // Emit the array reference
            _array.Emit(cg);
            // Emit the index (as integer)
            _index.EmitAs(cg, typeof(int));
            // Emit the value
            temp.EmitGet(cg);
            // Store it in the array
            cg.EmitStoreElement(elementType);

            if (asType != typeof(void)) {
                temp.EmitGet(cg);
                cg.EmitConvert(elementType, asType);
            }

            cg.FreeLocalTmp(temp);
        }

        public override void Walk(Walker walker) {
            if (walker.Walk(this)) {
                _array.Walk(walker);
                _index.Walk(walker);
                _value.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
