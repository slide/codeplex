/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using IronPython.Runtime;

internal sealed class PythonConsoleHost : ConsoleHost {

    protected override ScriptEngine/*!*/ CreateEngine() {
        return Runtime.GetEngine(typeof(PythonContext));
    }

    protected override CommandLine/*!*/ CreateCommandLine() {
        return new PythonCommandLine();
    }

    protected override OptionsParser/*!*/ CreateOptionsParser() {
        return new PythonOptionsParser();
    }

    [STAThread]
    static int Main(string[] args) {
        return new PythonConsoleHost().Run(args);
    }
}