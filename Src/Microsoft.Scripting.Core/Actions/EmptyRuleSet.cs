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

namespace System.Scripting.Actions {
    internal sealed class EmptyRuleSet<T> : RuleSet<T> where T : class {
        internal static readonly RuleSet<T> Instance = new EmptyRuleSet<T>(true);
        internal static readonly RuleSet<T> FixedInstance = new EmptyRuleSet<T>(false);

        private bool _supportAdding;

        private EmptyRuleSet(bool supportAdding) {
            this._supportAdding = supportAdding;
        }

        internal override RuleSet<T> AddRule(Rule<T> newRule) {
            if (_supportAdding) {
                return newRule.RuleSet;
            } else {
                return this;
            }
        }
    }
}