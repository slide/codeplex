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
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types {
    internal static partial class TypeInfo {
        public static Dictionary<SymbolId, OperatorMapping> InitializeOperatorTable() {
            Dictionary<SymbolId, OperatorMapping> pyOp = new Dictionary<SymbolId, OperatorMapping>();

            #region Generated PythonOperator Mapping

            // *** BEGIN GENERATED CODE ***
            // generated by function: gen_operatorMapping from: generate_ops.py

            pyOp[Symbols.OperatorAdd] = new OperatorMapping(Operators.Add, false, true, false, true);
            pyOp[Symbols.OperatorReverseAdd] = new OperatorMapping(Operators.ReverseAdd, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceAdd] = new OperatorMapping(Operators.InPlaceAdd, false, true, false);
            pyOp[Symbols.OperatorSubtract] = new OperatorMapping(Operators.Subtract, false, true, false, true);
            pyOp[Symbols.OperatorReverseSubtract] = new OperatorMapping(Operators.ReverseSubtract, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceSubtract] = new OperatorMapping(Operators.InPlaceSubtract, false, true, false);
            pyOp[Symbols.OperatorPower] = new OperatorMapping(Operators.Power, false, true, false, true);
            pyOp[Symbols.OperatorReversePower] = new OperatorMapping(Operators.ReversePower, false, true, false, true);
            pyOp[Symbols.OperatorInPlacePower] = new OperatorMapping(Operators.InPlacePower, false, true, false);
            pyOp[Symbols.OperatorMultiply] = new OperatorMapping(Operators.Multiply, false, true, false, true);
            pyOp[Symbols.OperatorReverseMultiply] = new OperatorMapping(Operators.ReverseMultiply, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceMultiply] = new OperatorMapping(Operators.InPlaceMultiply, false, true, false);
            pyOp[Symbols.OperatorFloorDivide] = new OperatorMapping(Operators.FloorDivide, false, true, false, true);
            pyOp[Symbols.OperatorReverseFloorDivide] = new OperatorMapping(Operators.ReverseFloorDivide, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceFloorDivide] = new OperatorMapping(Operators.InPlaceFloorDivide, false, true, false);
            pyOp[Symbols.OperatorDivide] = new OperatorMapping(Operators.Divide, false, true, false, true);
            pyOp[Symbols.OperatorReverseDivide] = new OperatorMapping(Operators.ReverseDivide, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceDivide] = new OperatorMapping(Operators.InPlaceDivide, false, true, false);
            pyOp[Symbols.OperatorTrueDivide] = new OperatorMapping(Operators.TrueDivide, false, true, false, true);
            pyOp[Symbols.OperatorReverseTrueDivide] = new OperatorMapping(Operators.ReverseTrueDivide, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceTrueDivide] = new OperatorMapping(Operators.InPlaceTrueDivide, false, true, false);
            pyOp[Symbols.OperatorMod] = new OperatorMapping(Operators.Mod, false, true, false, true);
            pyOp[Symbols.OperatorReverseMod] = new OperatorMapping(Operators.ReverseMod, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceMod] = new OperatorMapping(Operators.InPlaceMod, false, true, false);
            pyOp[Symbols.OperatorLeftShift] = new OperatorMapping(Operators.LeftShift, false, true, false, true);
            pyOp[Symbols.OperatorReverseLeftShift] = new OperatorMapping(Operators.ReverseLeftShift, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceLeftShift] = new OperatorMapping(Operators.InPlaceLeftShift, false, true, false);
            pyOp[Symbols.OperatorRightShift] = new OperatorMapping(Operators.RightShift, false, true, false, true);
            pyOp[Symbols.OperatorReverseRightShift] = new OperatorMapping(Operators.ReverseRightShift, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceRightShift] = new OperatorMapping(Operators.InPlaceRightShift, false, true, false);
            pyOp[Symbols.OperatorBitwiseAnd] = new OperatorMapping(Operators.BitwiseAnd, false, true, false, true);
            pyOp[Symbols.OperatorReverseBitwiseAnd] = new OperatorMapping(Operators.ReverseBitwiseAnd, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceBitwiseAnd] = new OperatorMapping(Operators.InPlaceBitwiseAnd, false, true, false);
            pyOp[Symbols.OperatorBitwiseOr] = new OperatorMapping(Operators.BitwiseOr, false, true, false, true);
            pyOp[Symbols.OperatorReverseBitwiseOr] = new OperatorMapping(Operators.ReverseBitwiseOr, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceBitwiseOr] = new OperatorMapping(Operators.InPlaceBitwiseOr, false, true, false);
            pyOp[Symbols.OperatorExclusiveOr] = new OperatorMapping(Operators.ExclusiveOr, false, true, false, true);
            pyOp[Symbols.OperatorReverseExclusiveOr] = new OperatorMapping(Operators.ReverseExclusiveOr, false, true, false, true);
            pyOp[Symbols.OperatorInPlaceExclusiveOr] = new OperatorMapping(Operators.InPlaceExclusiveOr, false, true, false);
            pyOp[Symbols.OperatorLessThan] = new OperatorMapping(Operators.LessThan, false, true, false, true);
            pyOp[Symbols.OperatorGreaterThan] = new OperatorMapping(Operators.GreaterThan, false, true, false, true);
            pyOp[Symbols.OperatorLessThanOrEqual] = new OperatorMapping(Operators.LessThanOrEqual, false, true, false, true);
            pyOp[Symbols.OperatorGreaterThanOrEqual] = new OperatorMapping(Operators.GreaterThanOrEqual, false, true, false, true);
            pyOp[Symbols.OperatorEquals] = new OperatorMapping(Operators.Equals, false, true, false, true);
            pyOp[Symbols.OperatorNotEquals] = new OperatorMapping(Operators.NotEquals, false, true, false, true);
            pyOp[Symbols.OperatorLessThanGreaterThan] = new OperatorMapping(Operators.LessThanGreaterThan, false, true, false, true);

            // *** END GENERATED CODE ***

            #endregion

            pyOp[Symbols.GetItem] = new OperatorMapping(Operators.GetItem, false, true, false);
            pyOp[Symbols.SetItem] = new OperatorMapping(Operators.SetItem, false, false, true);
            pyOp[Symbols.DelItem] = new OperatorMapping(Operators.DeleteItem, false, true, false);
            pyOp[Symbols.Cmp] = new OperatorMapping(Operators.Compare, false, true, false);
            pyOp[Symbols.Positive] = new OperatorMapping(Operators.Positive, true, false, false);
            pyOp[Symbols.OperatorNegate] = new OperatorMapping(Operators.Negate, true, false, false);
            pyOp[Symbols.OperatorOnesComplement] = new OperatorMapping(Operators.OnesComplement, true, false, false);
            pyOp[Symbols.Repr] = new OperatorMapping(Operators.CodeRepresentation, true, false, false);
            pyOp[Symbols.Length] = new OperatorMapping(Operators.Length, true, false, false);
            pyOp[Symbols.DivMod] = new OperatorMapping(Operators.DivMod, false, true, false, true);
            pyOp[Symbols.ReverseDivMod] = new OperatorMapping(Operators.ReverseDivMod, false, true, false, true);
            pyOp[Symbols.GetBoundAttr] = new OperatorMapping(Operators.GetBoundMember, false, true, false);
            pyOp[Symbols.OperatorPower] = new OperatorMapping(Operators.Power, false, true, true);
            pyOp[Symbols.Contains] = new OperatorMapping(Operators.Contains, false, true, false);

            pyOp[Symbols.AbsoluteValue] = new OperatorMapping(Operators.AbsoluteValue, true, false, false);

            return pyOp;
        }
    }
}