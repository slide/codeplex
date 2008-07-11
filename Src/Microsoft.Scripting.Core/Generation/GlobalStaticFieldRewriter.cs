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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Scripting.Runtime;
using System.Scripting.Utils;

namespace System.Scripting.Generation {

    /// <summary>
    /// Rewrites globals to static fields on a type
    /// Also rewrites constants to static fields
    /// 
    /// TODO: move to Microsoft.Scripting !!!
    /// </summary>
    internal sealed class GlobalStaticFieldRewriter : GlobalOptimizedRewriter {
        private readonly Dictionary<GlobalVariableExpression, FieldBuilder> _fields = new Dictionary<GlobalVariableExpression, FieldBuilder>();
        private readonly TypeGen _typeGen;
        private FieldBuilder _contextField;

        // TODO: remove this static data, and switch to instance data.
        // It's only static currently because NewTypeMaker has a dependency on
        // DynamicSiteHelpers.InitializeFields
        private readonly Dictionary<object, int> _constantCache = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Instance);
        private List<FieldBuilder> _staticFields = new List<FieldBuilder>();
        private static readonly List<object> _staticData = new List<object>();
        private static readonly object _nullVal = new object();
        [MultiRuntimeAware]
        private static int _lastCheck, _empties;

        internal GlobalStaticFieldRewriter(TypeGen typeGen) {
            _typeGen = typeGen;
        }

        protected override Expression MakeWrapper(GlobalVariableExpression variable) {
            Debug.Assert(!_fields.ContainsKey(variable));

            FieldBuilder field = _typeGen.TypeBuilder.DefineField(
                variable.Name,
                typeof(ModuleGlobalWrapper),
                FieldAttributes.Assembly | FieldAttributes.Static
            );

            _fields.Add(variable, field);

            return Expression.Field(null, field);
        }

        #region runtime constant support

        protected override Expression Visit(ConstantExpression node) {
            object data = node.Value;
            Type type = node.Type;

            // if the constant can be emitted into IL, nothing to do
            if (CanEmitConstant(data, type)) {
                return node;
            }

            type = TypeUtils.GetConstantType(type);

            int index;
            if (!_constantCache.TryGetValue(data, out index)) {
                int number = AddStaticData(data);
                FieldBuilder field = _typeGen.AddStaticField(type, "#Constant" + number);
                index = _staticFields.Count;
                _staticFields.Add(field);
                _constantCache.Add(data, index);
            }

            return Expression.Field(null, _staticFields[index]);
        }

        // Matches ILGen.TryEmitConstant
        private static bool CanEmitConstant(object value, Type type) {
            if (value == null ||
                CanEmitILConstant(type) ||
                value is SymbolId ||
                value is Missing ||
                value is string[] ||
                value is MethodBase && !(value is DynamicMethod)) {
                return true;
            }

            if (value is RuntimeTypeHandle) {
                RuntimeTypeHandle rth = (RuntimeTypeHandle)value;
                if (!rth.Equals(default(RuntimeTypeHandle))) {
                    return true;
                }
            }

            Type t = value as Type;
            if (t != null && (t is TypeBuilder || t.IsGenericParameter || t.IsVisible)) {
                return true;
            }

            if (value is RuntimeMethodHandle) {
                RuntimeMethodHandle rmh = (RuntimeMethodHandle)value;
                if (rmh != default(RuntimeMethodHandle)) {
                    return true;
                }
            }

            return false;
        }

        // Matches ILGen.TryEmitILConstant
        private static bool CanEmitILConstant(Type type) {
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        private static int AddStaticData(object data) {
            lock (_staticData) {
                if (_empties != 0) {
                    while (_lastCheck < _staticData.Count) {
                        if (_staticData[_lastCheck] == null) {
                            _staticData[_lastCheck] = data == null ? _nullVal : data;
                            _empties--;
                            return _lastCheck;
                        }
                        _lastCheck++;
                    }
                }

                _lastCheck = 0;
                _staticData.Add(data == null ? _nullVal : data);
                return _staticData.Count - 1;
            }
        }

        internal static object GetConstantData(int index) {
            lock (_staticData) {
                object res = _staticData[index];
                _staticData[index] = null;
                _empties++;
                Debug.Assert(res != null);
                return res == _nullVal ? null : res;
            }
        }

        internal static object GetConstantDataReusable(int index) {
            lock (_staticData) {
                object res = _staticData[index];
                Debug.Assert(res != null);
                return res == _nullVal ? null : res;
            }
        }

        #endregion

        internal void EmitDictionary() {
            // TODO: Force all dictionaries to share same object data (for multi-module)

            _contextField = _typeGen.TypeBuilder.DefineField(
                CodeContext.ContextFieldName,
                typeof(CodeContext),
                FieldAttributes.Public | FieldAttributes.Static
            );

            MakeGetMethod();
            MakeSetMethod();
            MakeRawKeysMethod();
            MakeInitialization();
        }

        #region EmitDictionary implementation

        private void MakeInitialization() {
            _typeGen.TypeBuilder.AddInterfaceImplementation(typeof(IModuleDictionaryInitialization));
            MethodInfo baseMethod = typeof(IModuleDictionaryInitialization).GetMethod("InitializeModuleDictionary");
            ILGen cg = _typeGen.DefineExplicitInterfaceImplementation(baseMethod);

            Label ok = cg.DefineLabel();
            cg.EmitFieldGet(_contextField);
            cg.Emit(OpCodes.Ldnull);
            cg.Emit(OpCodes.Ceq);
            cg.Emit(OpCodes.Brtrue_S, ok);
            cg.EmitNew(typeof(InvalidOperationException), Type.EmptyTypes);
            cg.Emit(OpCodes.Throw);
            cg.MarkLabel(ok);

            // arg0 -> this
            // arg1 -> MyModuleDictType.ContextSlot
            cg.EmitLoadArg(1);
            cg.EmitFieldSet(_contextField);

            ConstructorInfo wrapperCtor = typeof(ModuleGlobalWrapper).GetConstructor(new Type[] { typeof(CodeContext), typeof(SymbolId) });
            foreach (KeyValuePair<GlobalVariableExpression, FieldBuilder> kv in _fields) {
                // wrapper = new ModuleGlobalWrapper(context, name);
                cg.EmitLoadArg(1);
                cg.EmitSymbolId(SymbolTable.StringToId(kv.Key.Name));
                cg.Emit(OpCodes.Newobj, wrapperCtor);
                cg.Emit(OpCodes.Stsfld, kv.Value);
            }

            cg.Emit(OpCodes.Ret);
        }

        //
        // This generates a method like the following:
        //
        //  TryGetExtraValue(int name, object out value) {
        //      if (name1 == name) {
        //          value = type.name1Slot.RawValue;
        //          return value != Uninitialized.Instance;
        //      }
        //      if (name2 == name) {
        //          value = type.name2Slot.RawValue;
        //          return value != Uninitialized.Instance;
        //      }
        //      ...
        //      return false
        //  }

        private void MakeGetMethod() {
            MethodInfo baseMethod = typeof(CustomSymbolDictionary).GetMethod("TryGetExtraValue", BindingFlags.NonPublic | BindingFlags.Instance);
            ILGen cg = _typeGen.DefineMethodOverride(baseMethod);

            foreach (KeyValuePair<GlobalVariableExpression, FieldBuilder> kv in _fields) {
                SymbolId name = SymbolTable.StringToId(kv.Key.Name);

                cg.EmitSymbolId(name);
                // arg0 -> this
                cg.EmitLoadArg(1);
                cg.EmitCall(typeof(SymbolId), "op_Equality");

                Label next = cg.DefineLabel();
                cg.Emit(OpCodes.Brfalse_S, next);

                cg.EmitLoadArg(2);

                // Expects to push as an object.
                EmitGetRawFromObject(cg, kv.Value);
                cg.Emit(OpCodes.Stind_Ref);

                EmitGetRawFromObject(cg, kv.Value);
                cg.EmitUninitialized();
                cg.Emit(OpCodes.Ceq);
                cg.Emit(OpCodes.Not);
                cg.Emit(OpCodes.Ret);
                cg.MarkLabel(next);
            }
            cg.EmitInt(0);
            cg.Emit(OpCodes.Ret);
        }

        private static void EmitGetRawFromObject(ILGen cg, FieldBuilder wrapper) {
            cg.EmitFieldGet(wrapper);
            cg.EmitPropertyGet(typeof(ModuleGlobalWrapper), "RawValue");
        }

        // This generates a method like the following:
        //
        //  TrySetExtraValue(object name, object value) {
        //      if (name1 == name) {
        //          type.name1Slot = value;
        //          return 1;
        //      }
        //      if (name2 == name) {
        //          type.name2Slot = value;
        //          return 1;
        //      }
        //      ...
        //      return 0
        //  }

        private void MakeSetMethod() {
            MethodInfo baseMethod = typeof(CustomSymbolDictionary).GetMethod("TrySetExtraValue", BindingFlags.NonPublic | BindingFlags.Instance);
            ILGen cg = _typeGen.DefineMethodOverride(baseMethod);

            foreach (KeyValuePair<GlobalVariableExpression, FieldBuilder> kv in _fields) {
                SymbolId name = SymbolTable.StringToId(kv.Key.Name);

                cg.EmitSymbolId(name);
                // arg0 -> this
                cg.EmitLoadArg(1);
                cg.EmitCall(typeof(SymbolId), "op_Equality");

                Label next = cg.DefineLabel();
                cg.Emit(OpCodes.Brfalse_S, next);

                cg.EmitFieldGet(kv.Value);
                cg.EmitLoadArg(2);
                cg.EmitPropertySet(typeof(ModuleGlobalWrapper), "CurrentValue");

                cg.EmitInt(1);
                cg.Emit(OpCodes.Ret);
                cg.MarkLabel(next);
            }
            cg.EmitInt(0);
            cg.Emit(OpCodes.Ret);
        }

        private ILGen/*!*/ MakeRawKeysMethod() {
            FieldBuilder rawKeysCache = _typeGen.AddStaticField(typeof(SymbolId[]), "ExtraKeysCache");
            ILGen init = _typeGen.TypeInitializer;

            init.EmitInt(_fields.Count);
            init.Emit(OpCodes.Newarr, typeof(SymbolId));

            int current = 0;
            foreach (GlobalVariableExpression variable in _fields.Keys) {
                init.Emit(OpCodes.Dup);
                init.EmitInt(current++);
                init.EmitSymbolId(SymbolTable.StringToId(variable.Name));
                init.EmitStoreElement(typeof(SymbolId));
            }

            init.Emit(OpCodes.Stsfld, rawKeysCache);

            MethodInfo baseMethod = typeof(CustomSymbolDictionary).GetMethod("GetExtraKeys", BindingFlags.Public | BindingFlags.Instance);
            ILGen cg = _typeGen.DefineExplicitInterfaceImplementation(baseMethod);

            cg.Emit(OpCodes.Ldsfld, rawKeysCache);
            cg.Emit(OpCodes.Ret);
            return cg;
        }

        #endregion
    }
}
