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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Generation;
using System.Scripting.Utils;

namespace System.Scripting.Runtime {
    /// <summary>
    /// These are some generally useful helper methods. Currently the only methods are those to
    /// cached boxed representations of commonly used primitive types so that they can be shared.
    /// This is useful to most dynamic languages that use object as a universal type.
    /// 
    /// The methods in RuntimeHelepers are caleld by the generated code. From here the methods may
    /// dispatch to other parts of the runtime to get bulk of the work done, but the entry points
    /// should be here.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RuntimeHelpers {
        private static readonly string[] chars = MakeSingleCharStrings();

        private static string[] MakeSingleCharStrings() {
            string[] result = new string[255];

            for (char ch = (char)0; ch < result.Length; ch++) {
                result[ch] = new string(ch, 1);
            }

            return result;
        }

        public static string CharToString(char ch) {
            if (ch < 255) return chars[ch];
            return new string(ch, 1);
        }

        public static ArgumentTypeException SimpleTypeError(string message) {
            return new ArgumentTypeException(message);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static Exception CannotConvertError(Type toType, object value) {
            return SimpleTypeError(String.Format("Cannot convert {0}({1}) to {2}", CompilerHelpers.GetType(value).Name, value, toType.Name));
        }

        public static Exception SimpleAttributeError(string message) {
            return new MissingMemberException(message);
        }

        public static void ThrowUnboundLocalError(SymbolId name) {
            throw new UnboundLocalException(string.Format("local variable '{0}' referenced before assignment", SymbolTable.IdToString(name)));
        }

        public static object ReadOnlyAssignError(bool field, string fieldName) {
            throw SimpleAttributeError(String.Format("{0} {1} is read-only", field ? "Field" : "Property", fieldName));
        }

        public static DynamicStackFrame[] GetDynamicStackFrames(Exception e) {
            return GetDynamicStackFrames(e, true);
        }

        public static DynamicStackFrame[] GetDynamicStackFrames(Exception e, bool filter) {
            List<DynamicStackFrame> frames = e.Data[typeof(DynamicStackFrame)] as List<DynamicStackFrame>;

            if (frames == null) {
                // we may have missed a dynamic catch, and our host is looking
                // for the exception...
                frames = ExceptionHelpers.AssociateDynamicStackFrames(e);
                ExceptionHelpers.DynamicStackFrames = null;
            }

            if (frames == null) {
                return new DynamicStackFrame[0];
            }

            if (!filter) return frames.ToArray();
#if !SILVERLIGHT
            frames = new List<DynamicStackFrame>(frames);
            List<DynamicStackFrame> res = new List<DynamicStackFrame>();

            // the list of _stackFrames we build up in RuntimeHelpers can have
            // too many frames if exceptions are thrown from script code and
            // caught outside w/o calling GetDynamicStackFrames.  Therefore we
            // filter down to only script frames which we know are associated
            // w/ the exception here.
            try {
                StackTrace outermostTrace = new StackTrace(e);
                IList<StackTrace> otherTraces = ExceptionHelpers.GetExceptionStackTraces(e) ?? new List<StackTrace>();
                List<StackFrame> clrFrames = new List<StackFrame>();
                foreach (StackTrace trace in otherTraces) {
                    clrFrames.AddRange(trace.GetFrames() ?? new StackFrame[0]); // rare, sometimes GetFrames returns null
                }
                clrFrames.AddRange(outermostTrace.GetFrames() ?? new StackFrame[0]);    // rare, sometimes GetFrames returns null

                int lastFound = 0;
                foreach (StackFrame clrFrame in clrFrames) {
                    MethodBase method = clrFrame.GetMethod();

                    for (int j = lastFound; j < frames.Count; j++) {
                        MethodBase other = frames[j].GetMethod();
                        // method info's don't always compare equal, check based
                        // upon name/module/declaring type which will always be a correct
                        // check for dynamic methods.
                        if (method.Module == other.Module &&
                            method.DeclaringType == other.DeclaringType &&
                            method.Name == other.Name) {
                            res.Add(frames[j]);
                            frames.RemoveAt(j);
                            lastFound = j;
                            break;
                        }
                    }
                }
            } catch (MemberAccessException) {
                // can't access new StackTrace(e) due to security
            }
            return res.ToArray();
#else 
            return frames.ToArray();
#endif
        }

        /// <summary>
        /// Helper method to create an instance.  Work around for Silverlight where Activator.CreateInstance
        /// is SecuritySafeCritical.
        /// 
        /// TODO: Why can't we just emit the right thing for default(T)?
        /// It's always null for reference types and it's well defined for value types
        /// </summary>
        public static T CreateInstance<T>() {
            return default(T);
        }

        // TODO: can't we just emit a new array?
        public static T[] CreateArray<T>(int args) {
            return new T[args];
        }
        
        /// <summary>
        /// EventInfo.EventHandlerType getter is marked SecuritySafeCritical in CoreCLR
        /// This method is to get to the property without using Reflection
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public static Type GetEventHandlerType(EventInfo eventInfo) {
            ContractUtils.RequiresNotNull(eventInfo, "eventInfo");
            return eventInfo.EventHandlerType;
        }

        public static IList<string> GetStringMembers(IList<object> members) {
            List<string> res = new List<string>();
            foreach (object o in members) {
                string str = o as string;
                if (str != null) {
                    res.Add(str);
                }
            }
            return res;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")] // TODO: fix
        public static void SetEvent(EventTracker eventTracker, object value) {
            EventTracker et = value as EventTracker;
            if (et != null) {
                if (et != eventTracker) {
                    throw new ArgumentException(String.Format("expected event from {0}.{1}, got event from {2}.{3}",
                                                eventTracker.DeclaringType.Name,
                                                eventTracker.Name,
                                                et.DeclaringType.Name,
                                                et.Name));
                }
                return;
            }

            BoundMemberTracker bmt = value as BoundMemberTracker;
            if (bmt == null) throw new ArgumentTypeException("expected bound event, got " + CompilerHelpers.GetType(value).Name);
            if (bmt.BoundTo.MemberType != TrackerTypes.Event) throw new ArgumentTypeException("expected bound event, got " + bmt.BoundTo.MemberType.ToString());

            if (bmt.BoundTo != eventTracker) throw new ArgumentException(String.Format("expected event from {0}.{1}, got event from {2}.{3}",
                eventTracker.DeclaringType.Name,
                eventTracker.Name,
                bmt.BoundTo.DeclaringType.Name,
                bmt.BoundTo.Name));
        }

        // TODO: just emit this in the generated code
        public static bool CheckDictionaryMembers(IDictionary dict, string[] names) {
            if (dict.Count != names.Length) return false;

            foreach (string name in names) {
                if (!dict.Contains(name)) {
                    return false;
                }
            }
            return true;
        }

        // TODO: just emit this in the generated code
        public static T IncorrectBoxType<T>(object received) {
            throw new ArgumentTypeException(String.Format("Expected type StrongBox<{0}>, got {1}", typeof(T).Name, CompilerHelpers.GetType(received).Name));
        }
        
        public static void InitializeSymbols(Type t) {
            foreach (FieldInfo fi in t.GetFields()) {
                if (fi.FieldType == typeof(SymbolId)) {
                    Debug.Assert(fi.Name.StartsWith("symbol_"));

                    fi.SetValue(null, SymbolTable.StringToId(fi.Name.Substring(7)));
                }
            }
        }
    }
}
