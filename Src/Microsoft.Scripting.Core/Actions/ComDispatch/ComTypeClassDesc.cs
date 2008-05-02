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

#if !SILVERLIGHT // ComObject

namespace Microsoft.Scripting.Actions.ComDispatch {
    using System;
    using Microsoft.Scripting.Generation;
    using Microsoft.Scripting.Runtime;
    using Ast = Microsoft.Scripting.Ast.Expression;
    using ComTypes = System.Runtime.InteropServices.ComTypes;
    using System.Collections.Generic;

    public class ComTypeClassDesc : ComTypeDesc, IDynamicObject {
        LinkedList<string> _itfs; // implemented interfaces
        LinkedList<string> _sourceItfs; // source interfaces supported by this coclass

        public object CreateInstance() {
            return System.Activator.CreateInstance(System.Type.GetTypeFromCLSID(Guid));
        }

        #region IDynamicObject Members

        // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public LanguageContext LanguageContext {
            get { throw new NotImplementedException(); }
        }

        public RuleBuilder<T> GetRule<T>(DynamicAction action, CodeContext context, object[] args) {
            if (action.Kind == DynamicActionKind.CreateInstance) {
                RuleBuilder<T> rule = new RuleBuilder<T>();
                rule.MakeTest(CompilerHelpers.GetType(this));
                rule.Target = rule.MakeReturn(
                    context.LanguageContext.Binder,
                    Ast.Call(
                        Ast.ConvertHelper(rule.Parameters[0], typeof(ComTypeClassDesc)),
                        this.GetType().GetMethod("CreateInstance")));

                return rule;
            } 

            return null;
        }

        #endregion

        internal ComTypeClassDesc(ComTypes.ITypeInfo typeInfo, ComTypeLibDesc typeLibDesc) : 
            base(typeInfo, ComType.Class, typeLibDesc)
        {
            ComTypes.TYPEATTR typeAttr = ComRuntimeHelpers.GetTypeAttrForTypeInfo(typeInfo);
            Guid = typeAttr.guid;

            for (int i = 0; i < typeAttr.cImplTypes; i++) {
                int hRefType;
                typeInfo.GetRefTypeOfImplType(i, out hRefType);
                ComTypes.ITypeInfo currentTypeInfo;
                typeInfo.GetRefTypeInfo(hRefType, out currentTypeInfo);

                ComTypes.IMPLTYPEFLAGS implTypeFlags;
                typeInfo.GetImplTypeFlags(i, out implTypeFlags);

                bool isSourceItf = (implTypeFlags & ComTypes.IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE) != 0;
                AddInterface(currentTypeInfo, isSourceItf);
            }
        }

        private void AddInterface(ComTypes.ITypeInfo itfTypeInfo, bool isSourceItf)
        {
            string itfName = ComRuntimeHelpers.GetNameOfType(itfTypeInfo);

            if (isSourceItf) {
                if (_sourceItfs == null) {
                    _sourceItfs = new LinkedList<string>();
                }
                _sourceItfs.AddLast(itfName);
            }
            else {
                if (_itfs == null) {
                    _itfs = new LinkedList<string>();
                }
                _itfs.AddLast(itfName );
            }
        }

        internal bool Implements(string itfName, bool isSourceItf) {
            if (isSourceItf)
                return _sourceItfs.Contains(itfName);
            else
                return _itfs.Contains(itfName);
        }
    }
}

#endif
