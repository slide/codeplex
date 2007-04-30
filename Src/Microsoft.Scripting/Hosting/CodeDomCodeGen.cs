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
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

#if !SILVERLIGHT // CodeDom objects are not available in Silverlight

namespace Microsoft.Scripting.Hosting {
    public abstract class CodeDomCodeGen {
        // This is the key used in the UserData of the CodeDom objects to track
        // the source location of the CodeObject in the original source file.
        protected static readonly object SourceSpanKey = typeof(SourceSpan);

        // Stores the code as it is generated
        private PositionTrackingWriter _writer;
        protected PositionTrackingWriter Writer { get { return _writer; } }

        abstract protected void WriteExpressionStatement(CodeExpressionStatement s);
        abstract protected void WriteFunctionDefinition(CodeMemberMethod func);
        abstract protected string QuoteString(string val);

        public SourceUnit GenerateCode(IScriptEngine engine, CodeObject codeDom) {
            CodeMemberMethod method = codeDom as CodeMemberMethod;
            if (method == null) {
                throw new ArgumentException("codeDom must be a CodeMemberMethod");
            }

            // Convert the CodeDom to source code
            if (_writer != null) {
                _writer.Close();
            }
            _writer = new PositionTrackingWriter();

            WriteFunctionDefinition(method);

            return CreateSourceUnit(engine);
        }

        private SourceUnit CreateSourceUnit(IScriptEngine engine) {
            KeyValuePair<int, string>[] fileMap = _writer.GetFileMap();
            string sourcePath = null;
            if (fileMap != null && fileMap.Length > 0) {
                // For now we only support one source file in the ScriptEngine.CompileCodeDom API
                sourcePath = fileMap[0].Value;
            }

            string code = _writer.ToString();

            SourceUnit src;
            if (sourcePath != null) {
                src = new SourceFileUnit(engine, sourcePath, sourcePath, code);
            } else {
                src = new SourceCodeUnit(engine, code);
            }
            src.SetLineMapping(_writer.GetLineMap());
            src.SetDocumentMapping(fileMap);
            return src;
        }

        virtual protected void WriteArgumentReferenceExpression(CodeArgumentReferenceExpression e) {
            _writer.Write(e.ParameterName);
        }

        virtual protected void WriteSnippetExpression(CodeSnippetExpression e) {
            _writer.Write(e.Value);
        }

        virtual protected void WriteSnippetStatement(CodeSnippetStatement s) {
            _writer.Write(s.Value);
            _writer.Write('\n');
        }

        protected void WriteStatement(CodeStatement s) {
            // Save statement source location
            if (s.LinePragma != null) {
                _writer.MapLocation(s.LinePragma);
            }

            if (s is CodeExpressionStatement) {
                WriteExpressionStatement((CodeExpressionStatement)s);
            }
            else if (s is CodeSnippetStatement) {
                WriteSnippetStatement((CodeSnippetStatement)s);
            }
        }

        protected void WriteExpression(CodeExpression e) {
            if (e is CodeSnippetExpression) {
                WriteSnippetExpression((CodeSnippetExpression)e);
            }
            else if (e is CodePrimitiveExpression) {
                WritePrimitiveExpression((CodePrimitiveExpression)e);
            }
            else if (e is CodeMethodInvokeExpression) {
                WriteCallExpression((CodeMethodInvokeExpression)e);
            }
            else if (e is CodeArgumentReferenceExpression) {
                WriteArgumentReferenceExpression((CodeArgumentReferenceExpression)e);
            }
        }

        protected void WritePrimitiveExpression(CodePrimitiveExpression e) {
            object val = e.Value;

            if (val is string) {
                _writer.Write(QuoteString((string)val));
            }
            else {
                _writer.Write(val);
            }
        }

        protected void WriteCallExpression(CodeMethodInvokeExpression m) {
            if (m.Method.TargetObject != null) {
                WriteExpression(m.Method.TargetObject);
                _writer.Write(".");
            }

            _writer.Write(m.Method.MethodName);
            _writer.Write("(");
            for (int i = 0; i < m.Parameters.Count; ++i) {
                if (i != 0) {
                    _writer.Write(",");
                }
                WriteExpression(m.Parameters[i]);
            }
            _writer.Write(")");
        }
    }
}

#endif
