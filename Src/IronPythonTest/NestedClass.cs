/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace IronPythonTest {
    public class NestedClass {
        public string Field;

        public string CallMe() {
            return " Hello World";
        }

        public string Property {
            get {
                return Field;
            }
            set {
                Field = value;
            }
        }

        public class InnerClass {
            public string InnerField;

            public string CallMeInner() {
                return "Inner Hello World";
            }

            public string InnerProperty {
                get {
                    return InnerField;
                }
                set {
                    InnerField = value;
                }
            }

            public class TripleNested {
                public string TripleField;

                public string CallMeTriple() {
                    return "Triple Hello World";
                }

                public string TripleProperty {
                    get {
                        return TripleField;
                    }
                    set {
                        TripleField = value;
                    }
                }
            }
        }
    }
}
