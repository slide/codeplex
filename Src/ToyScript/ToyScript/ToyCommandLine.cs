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

using Microsoft.Scripting.Hosting.Shell; 

namespace ToyScript {
    public class ToyCommandLine : CommandLine {
        protected override string Logo {
            get {
                return
@"ToyScript Console.

Let's Play ...
";
            }
        }

        protected override string Prompt {
            get {
                return
@"
Toy > ";
            }
        }
    }
}
