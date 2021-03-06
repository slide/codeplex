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
    /// <summary>
    /// Base used for implicit operator tests where the operator is defined
    /// on the "derived" class
    /// </summary>
    public class Base {
        public int value;

        public Base(int value) {
            this.value = value;
        }
    }

    public class Derived {
        public Derived(int value) {
            this.value = value;
        }

        public int value;

        public static implicit operator Base(Derived d) {
            return new Base(d.value);
        }
    }

    /// <summary>
    /// Base class used where the implicit operator is defined on the
    /// "base" class
    /// </summary>
    public class Base2 {
        public int value;

        public Base2(int value) {
            this.value = value;
        }

        public static implicit operator Base2(Derived2 d) {
            return new Base2(d.value);
        }
    }

    public class Derived2 {
        public int value;

        public Derived2(int value) {
            this.value = value;
        }
    }

    public class ConversionStorage {
        public Base Base;
        public Derived Derived;
        public Base2 Base2;
        public Derived2 Derived2;
    }

    public class DoubleToFloat {
        public static float ToFloat(double val) { return (float)val; }
    }
}
